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
            // create state object

            // parse args
            var commandLineConfig = DefineAndParseOptions(args, out string sourcePath, out bool watch);

            // check we have a file
            if (!string.IsNullOrEmpty(sourcePath) && !File.Exists(sourcePath) && !Directory.Exists(sourcePath))
            {
                QfConsole.WriteLine($@"The file or directory {sourcePath} does not exist. Exiting...");
                return;
            }
            // fetch config query/project/install
            var configFileReader = new ConfigFileReader();
            var projectConfig = configFileReader.GetProjectConfig(sourcePath);
            var installConfig = configFileReader.GetInstallConfig();

            var projectType = new ProjectType().DetectProjectType();

            // build config project-install
            var configBuilder = new ConfigBuilder();
            var outerConfig = configBuilder.Resolve2Configs(projectConfig, configBuilder.GetInstallConfigForProjectType(installConfig, projectType));

            // register types
            RegisterTypes.Register(outerConfig.HelperAssemblies);

            // Process one file
            if (File.Exists(sourcePath))
            {
                if (watch)
                {
                    // watch for changes or renaming of the specified file. (VS renames a temp file after deleting the original file)
                    using (FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(sourcePath), Path.GetFileName(sourcePath)))
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
                                conductor.ProcessOneQuery(sourcePath, outerConfig);
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
                                conductor.ProcessOneQuery(sourcePath, outerConfig);
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
                    conductor.ProcessOneQuery(sourcePath, outerConfig);
                }

            }

            // Process a folder
            if (Directory.Exists(sourcePath))
            {
                if (watch)
                {
                    // watch for changes or renaming of .sql files in the specified folder.
                    using (FileSystemWatcher watcher = new FileSystemWatcher(sourcePath, "*.sql"))
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
                    ProcessDirectory(sourcePath, outerConfig);
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
        /// <param name="sourcePath">Out param. The file or folder to process</param>
        /// <returns></returns>
        static QfConfigModel DefineAndParseOptions(string[] args, out string sourcePath, out bool watch)
        {
            bool _watch = false;
            sourcePath = "";
            // these variables will be set when the command line is parsed

            // these are the available options, note that they set the variables
            var commandLineConfig = new QfConfigModel();
            commandLineConfig.Generators = new List<Generator>();
            int verbosity = 0;
            var shouldShowHelp = false;
            var options = new OptionSet {
                { "c|qfDefaultConnection", "Connection string for query generation, overrides config files.", c => commandLineConfig.DefaultConnection = c },
                { "m|makeSelfTest", "Make integration test for query. Requires xunit. Overrides config files.", m => commandLineConfig.MakeSelfTest = m != null},
                {"g|generator" , "Generator(s) to use. Will replace generators specified in config files", g => commandLineConfig.Generators.Add(new Generator{Name = g })},
                { "v", "increase debug message verbosity", v => {
                    if (v != null)
                        ++verbosity;
                } },
                {"w|watch", "Watch for changes in the file or directory", w => _watch = w != null },
                {"k|keepOpen", "Wait for a keystroke before exiting the console.", k => keepOpen = k != null },
                { "h|help", "show this message and exit", h => shouldShowHelp = h != null },
            };

            // now parse the options...
            List<string> extra;
            try
            {
                // parse the command line
                extra = options.Parse(args);
                if (extra != null && extra.Count > 0 && !string.IsNullOrEmpty(extra[0]))
                    sourcePath = extra[0];
                else
                    shouldShowHelp = true;
            }
            catch (OptionException e)
            {
                // output some error message
                Console.Write("greet: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `greet --help' for more information.");
                return null;
            }
            finally
            {
                // clean up if none...
                if (commandLineConfig.Generators.Count == 0)
                    commandLineConfig.Generators = null;
                watch = _watch;

            }
            if (shouldShowHelp)
                options.WriteOptionDescriptions(Console.Out);
            return commandLineConfig;
        }

    }
}
