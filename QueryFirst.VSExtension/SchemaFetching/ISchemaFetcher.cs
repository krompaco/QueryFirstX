using System.Collections.Generic;
using System.Configuration;

namespace QueryFirst.VSExtension
{
    public interface ISchemaFetcher
    {
        List<ResultFieldDetails> GetFields(string connectionString, string provider, string Query);
    }
}