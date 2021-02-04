using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;

namespace QueryFirst
{
    public class Program
    {
        static bool keepOpen;
        public static void Main(string[] args)
        {
            // parse args
            var startupOptions = DefineAndParseOptions(args);

            // check we have a file (should never be empty because if nothing provided we take the current directory.)
            if (!string.IsNullOrEmpty(startupOptions.SourcePath) && !File.Exists(startupOptions.SourcePath) && !Directory.Exists(startupOptions.SourcePath))
            {
                QfConsole.WriteLine($@"The file or directory {startupOptions.SourcePath} does not exist. Exiting...");
                return;
            }
            // fetch config query/project/install
            var configFileReader = new ConfigFileReader();
            var projectConfig = configFileReader.GetProjectConfig(startupOptions.SourcePath);
            var installConfig = configFileReader.GetInstallConfig();

            var projectType = new ProjectType().DetectProjectType();

            // build config project-install
            var configBuilder = new ConfigBuilder();
            var outerConfig = configBuilder.Resolve2Configs(projectConfig, configBuilder.GetInstallConfigForProjectType(installConfig, projectType));

            // register types
            RegisterTypes.Register(outerConfig.HelperAssemblies);

            // New Config
            if (startupOptions.CreateConfig.GetValueOrDefault())
            {
                var fileToCreate = Path.Join(Environment.CurrentDirectory, "qfconfig.json");
                if (File.Exists(fileToCreate))
                {
                    Console.WriteLine($"QueryFirst: {fileToCreate} exists already. Skipping.");
                }
                File.WriteAllText(fileToCreate,
$@"{{
  ""defaultConnection"": ""Server = localhost\\SQLEXPRESS; Database = NORTHWND; Trusted_Connection = True; "",
  ""provider"": ""System.Data.SqlClient"",
  ""namespace"": ""MyNamespaceForCodeGeneration""
}}
            "
                );
            }
            // New Runtime Connection
            if (startupOptions.CreateRuntimeConnection.GetValueOrDefault())
            {
                var fileToCreate = Path.Join(Environment.CurrentDirectory, "QfRuntimeConnection.cs");
                if (File.Exists(fileToCreate))
                {
                    Console.WriteLine($"QueryFirst: {fileToCreate} exists already. Skipping.");
                }
                File.WriteAllText(fileToCreate,
$@"using Microsoft.Data.SqlClient;
using System.Data;

    class QfRuntimeConnection
    {{
        public static IDbConnection GetConnection()
        {{
            return new SqlConnection(""Server=localhost\\SQLEXPRESS;Database=NORTHWND;Trusted_Connection=True;"");
        }}
    }}
"
                );
            }
            // New query
            if (!string.IsNullOrEmpty(startupOptions.NewQueryName))
            {
                if (!startupOptions.NewQueryName.EndsWith(".sql"))
                    startupOptions.NewQueryName = startupOptions.NewQueryName + ".sql";
                if (File.Exists(startupOptions.NewQueryName))
                {
                    Console.WriteLine($"QueryFirst: {startupOptions.NewQueryName} exists already. Exiting");
                    return;
                }
                File.WriteAllText(startupOptions.NewQueryName,
@"/* .sql query managed by QueryFirst */
-- designTime - put parameter declarations and design time initialization here
-- endDesignTime"
                );
                return;
            }
            // Process one file
            if (File.Exists(startupOptions.SourcePath))
            {
                if (startupOptions.Watch)
                {
                    // watch for changes or renaming of the specified file. (VS renames a temp file after deleting the original file)
                    using (FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(startupOptions.SourcePath), Path.GetFileName(startupOptions.SourcePath)))
                    {
                        watcher.NotifyFilter = NotifyFilters.LastAccess
                      | NotifyFilters.LastWrite
                      | NotifyFilters.FileName
                      | NotifyFilters.DirectoryName;


                        watcher.Changed += (source, e) =>
                        {
                            try
                            {
                                var conductor = new Conductor().BuildUp();
                                conductor.ProcessOneQuery(startupOptions.SourcePath, outerConfig);
                            }
                            catch (Exception ex)
                            {
                                Console.Write(ex.TellMeEverything());
                            }

                        };
                        //watcher.Deleted += OnChanged;
                        watcher.Renamed += (source, e) =>
                        {
                            try
                            {
                                var conductor = new Conductor().BuildUp();
                                conductor.ProcessOneQuery(startupOptions.SourcePath, outerConfig);
                            }
                            catch (Exception ex)
                            {
                                Console.Write(ex.TellMeEverything());
                            }

                        };
                        watcher.EnableRaisingEvents = true;
                        Console.WriteLine("Press 'q' to stop watching.");
                        while (Console.Read() != 'q') ;
                    }
                }
                else
                {
                    var conductor = new Conductor().BuildUp();
                    conductor.ProcessOneQuery(startupOptions.SourcePath, outerConfig);
                }

            }

            // Process a folder
            if (Directory.Exists(startupOptions.SourcePath))
            {
                if (startupOptions.Watch)
                {
                    // watch for changes or renaming of .sql files in the specified folder.
                    using (FileSystemWatcher watcher = new FileSystemWatcher(startupOptions.SourcePath, "*.sql"))
                    {
                        watcher.NotifyFilter = NotifyFilters.LastAccess
                      | NotifyFilters.LastWrite
                      | NotifyFilters.FileName
                      | NotifyFilters.DirectoryName;


                        watcher.Changed += (source, e) =>
                        {
                            try
                            {
                                if (e.FullPath.ToLower().EndsWith(".sql"))
                                {
                                    var conductor = new Conductor().BuildUp();
                                    conductor.ProcessOneQuery(e.FullPath, outerConfig);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.Write(ex.TellMeEverything());
                            }

                        };
                        //watcher.Deleted += OnChanged;
                        watcher.Renamed += (source, e) =>
                        {
                            try
                            {
                                if (e.FullPath.ToLower().EndsWith(".sql"))
                                {
                                    var conductor = new Conductor().BuildUp();
                                    conductor.ProcessOneQuery(e.FullPath, outerConfig);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.Write(ex.TellMeEverything());
                            }

                        };
                        watcher.EnableRaisingEvents = true;
                        Console.WriteLine("Press 'q' to stop watching.");
                        while (Console.Read() != 'q') ;
                    }
                }
                else
                {
                    ProcessDirectory(startupOptions.SourcePath, outerConfig);
                }

                if (keepOpen)
                {
                    Console.ReadKey();
                }



            }
        }
        // Process all files in the directory passed in, recurse on any directories
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string targetDirectory, QfConfigModel outerConfig)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory,"*.sql");
            foreach (string fileName in fileEntries)
                ProcessFile(fileName, outerConfig);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory, outerConfig);
        }

        public static void ProcessFile(string path, QfConfigModel outerConfig)
        {
            var conductor = new Conductor().BuildUp();
            conductor.ProcessOneQuery(path, outerConfig);
            Console.WriteLine("Processed file '{0}'.", path);
        }
        /// <summary>
        /// Defines the command line switches. See
        /// https://github.com/xamarin/XamarinComponents/tree/master/XPlat/Mono.Options
        /// </summary>
        /// <param name="args">The command line args to parse</param>
        /// <returns></returns>
        static StartupOptions DefineAndParseOptions(string[] args)
        {
            var returnVal = new StartupOptions();
            var commandLineConfig = new QfConfigModel();

            // these variables will be set when the command line is parsed

            // these are the available options, note that they set the variables
            commandLineConfig.Generators = new List<Generator>();
            int verbosity = 0;
            var shouldShowHelp = false;
            var options = new OptionSet {
                {"c|qfDefaultConnection=", "Connection string for query generation, overrides config files.", c => commandLineConfig.DefaultConnection = c },
                {"m|makeSelfTest", "Make integration test for query. Requires xunit. Overrides config files.", m => commandLineConfig.MakeSelfTest = m != null},
                {"g|generator=" , "Generator(s) to use. Will replace generators specified in config files", g => commandLineConfig.Generators.Add(new Generator{Name = g })},
                {"w|watch", "Watch for changes in the file or directory", w => returnVal.Watch = w != null },
                {"k|keepOpen", "Wait for a keystroke before exiting the console.", k => keepOpen = k != null },
                {"h|help", "show this message and exit", h => shouldShowHelp = h != null },
                {"n|new=", "create a new QueryFirst query with the specified name.", n => returnVal.NewQueryName = n },
                {"j|newConfig", "create qfconfig.json in the current directory", nc => returnVal.CreateConfig = nc != null },
                {"r|newRuntimeConnection", "create QfRuntimeConnection.cs in the current directory", nc => returnVal.CreateRuntimeConnection = nc != null },
            };

            // now parse the options...
            List<string> extra;
            try
            {
                // parse the command line
                extra = options.Parse(args);
                if (extra != null && extra.Count > 0 && !string.IsNullOrEmpty(extra[0]))
                    returnVal.SourcePath = extra[0];
                else
                    // by default, process the current directory
                    returnVal.SourcePath = Environment.CurrentDirectory;
            }
            catch (OptionException e)
            {
                // output some error message
                Console.Write("greet: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `QueryFirst --help' for more information.");
                return null;
            }
            finally
            {
                // clean up if none...
                if (commandLineConfig.Generators.Count == 0)
                    commandLineConfig.Generators = null;
            }
            if (shouldShowHelp)
                options.WriteOptionDescriptions(Console.Out);
            returnVal.StartupConfig = commandLineConfig;
            return returnVal;
        }

    }
    public class StartupOptions
    {
        public string SourcePath { get; set; }
        public bool Watch { get; set; }
        public string NewQueryName { get; set; }
        public bool? CreateConfig { get; set; }
        public bool? CreateRuntimeConnection { get; set; }
        public QfConfigModel StartupConfig { get; set; }
    }
}
