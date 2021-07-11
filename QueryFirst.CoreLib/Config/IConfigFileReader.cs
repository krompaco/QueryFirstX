using System.Collections.Generic;

namespace QueryFirst
{
    public interface IConfigFileReader
    {
        List<ProjectSection> GetInstallConfig();
        QfConfigModel GetProjectConfig(string fileOrFolder);
        string GetProjectConfigFile(string folder);
        QfConfigModel GetQueryConfig(string queryFilename);
    }
}