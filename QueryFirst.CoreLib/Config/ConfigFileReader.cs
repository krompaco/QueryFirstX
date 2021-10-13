using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QueryFirst
{
    public class ConfigFileReader : IConfigFileReader
    {

        public QfConfigModel ReadAndResolveProjectAndInstallConfigs(StartupOptions startupOptions, ConfigFileReader configFileReader)
        {
            var projectConfig = GetProjectConfig(startupOptions.SourcePath);
            var installConfig = GetInstallConfig();

            var projectType = new ProjectType().DetectProjectType();

            // build config project-install
            var configBuilder = new ConfigBuilder();
            var outerConfig = configBuilder.Resolve2Configs(projectConfig, configBuilder.GetInstallConfigForProjectType(installConfig, projectType));
            return outerConfig;
        }
        /// <summary>
        /// Returns the string contents of the first qfconfig.json file found,
        /// starting in the directory of the path supplied and going up.
        /// </summary>
        /// <param name="folder">Full path name of the query file</param>
        /// <returns></returns>
        public string GetProjectConfigFile(string folder)
        {
            while (folder != null)
            {
                if (File.Exists(folder + "\\qfconfig.json"))
                {
                    return File.ReadAllText(folder + "\\qfconfig.json");
                }
                folder = Directory.GetParent(folder)?.FullName;
            }
            return null;
        }
        public QfConfigModel GetProjectConfig(string fileOrFolder)
        {
            string searchStart;
            if (File.Exists(fileOrFolder))
            {
                searchStart = Path.GetDirectoryName(fileOrFolder);
            }
            else if (Directory.Exists(fileOrFolder))
            {
                searchStart = fileOrFolder;
            }
            else
            {
                throw new ArgumentException("No such file or folder: " + fileOrFolder);
            }
            var projectConfigFileContents = GetProjectConfigFile(searchStart);
            if(projectConfigFileContents == null)
            {
                return null;
            }
            try
            {
                var project = JsonConvert.DeserializeObject<QfConfigModel>(projectConfigFileContents);
                SetDefaultProvider(project);
                return project;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deserializing project qfconfig.json. Is there anything funny in there?", ex);
            }
        }
        public QfConfigModel GetQueryConfig(string queryFilename)
        {
            QfConfigModel query;
            try
            {
                string queryConfigFileContents;
                if (File.Exists(queryFilename.Replace(".sql", ".qfconfig.json")))
                {
                    queryConfigFileContents = File.ReadAllText(queryFilename.Replace(".sql", ".qfconfig.json"));
                    query = JsonConvert.DeserializeObject<QfConfigModel>(queryConfigFileContents);
                }
                else query = new QfConfigModel();
                SetDefaultProvider(query);
                return query;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deserializing {queryFilename.Replace(".sql", ".qfconfig.json")}. Is there anything funny in there?", ex);
            }
        }
        public List<ProjectSection> GetInstallConfig()
        {
            List<ProjectSection> installConfig;
            try
            {
                string installConfigFileContents;
                var installFolder = Path.GetDirectoryName(typeof(ConfigFileReader).Assembly.Location);
                if (File.Exists(installFolder + @"\qfconfig.json"))
                {
                    installConfigFileContents = File.ReadAllText(installFolder + @"\qfconfig.json");
                    installConfig = JsonConvert.DeserializeObject<List<ProjectSection>>(installConfigFileContents);
                }
                else installConfig = new List<ProjectSection>();
                foreach (var section in installConfig)
                {
                    SetDefaultProvider(section.QfConfig);
                }
                return installConfig;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deserializing install config. Is there anything funny in there?", ex);
            }
        }
        private QfConfigModel SetDefaultProvider(QfConfigModel config)
        {
            // Sql server is the default IF connection string is provided.
            if (!string.IsNullOrEmpty(config.DefaultConnection) && string.IsNullOrEmpty(config.Provider))
            {
                config.Provider = "System.Data.SqlClient";
            }
            return config;
        }
    }

    public class Generator
    {
        public string Name { get; set; }
    }
    public class ProjectSection
    {
        public string ProjectType { get; set; }
        public QfConfigModel QfConfig { get; set; }
    }
}
