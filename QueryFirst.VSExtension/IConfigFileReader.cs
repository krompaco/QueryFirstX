namespace QueryFirst.VSExtension
{
    public interface IConfigFileReader
    {
        string GetConfigFile(string filePath);
        QfConfigModel GetConfigObj(string filePath);
    }
}