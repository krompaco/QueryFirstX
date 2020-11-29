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
            var commandLineConfig = DefineAndParseOptions(args, out string sourcePath);

            // check we have a file
            if (!File.Exists(sourcePath))
            {
                QfConsole.WriteLine($@"The file {sourcePath} does not exist. Exiting...");
                return;
            }
            // fetch config query/project/install
            var configFileReader = new ConfigFileReader();
            var projectConfig = configFileReader.GetProjectConfig(Path.GetDirectoryName(sourcePath));
            var installConfig = configFileReader.GetInstallConfig();

            var projectType = new ProjectType().DetectProjectType();

            // build config project-install
            var configBuilder = new ConfigBuilder();            
            var outerConfig = configBuilder.Resolve2Configs(projectConfig, configBuilder.GetInstallConfigForProjectType(installConfig, projectType));

            // register types
            RegisterTypes.Register(outerConfig.HelperAssemblies);

            //process
            var conductor = new Conductor().BuildUp(); 
            conductor.ProcessOneQuery(sourcePath, outerConfig);
            if (keepOpen)
            {
                Console.ReadKey();
            }

            

        }
        static QfConfigModel DefineAndParseOptions(string[] args, out string sourcePath)
        {
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
                {"k|keepOpen", "Wait for a keystroke before exiting the console.", k => keepOpen = k != null },
                { "h|help", "show this message and exit", h => shouldShowHelp = h != null },
            };

            // now parse the options...
            List<string> extra;
            try
            {
                // parse the command line
                extra = options.Parse(args);
                if(extra != null && extra.Count > 0 && !string.IsNullOrEmpty(extra[0]))
                    sourcePath = extra[0];
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
            }
            return commandLineConfig;
        }

    }
}
