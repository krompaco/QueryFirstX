using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace QueryFirst
{
    public class _4ResolveConfig
    {
        public IConfigFileReader ConfigFileReader { get; set; }
        /// <summary>
        /// Returns the QueryFirst config for a given query file. Values specified directly in 
        /// the query file will trump values specified in the qfconfig.json file.
        /// We look for a qfconfig.json file beside the query file. If none found, we look in the parent directory,
        /// and so on up to the root directory. 
        /// 
        /// If the query specifies a QfDefaultConnection but no QfDefaultConnectionProviderName, "System.Data.SqlClient"
        /// will be used.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="queryText"></param>
        /// <returns></returns>
        public State Go(State state)
        {
            // read the config file if there is one.
            var queryConfig = ConfigFileReader.GetQueryConfig(state._1SourceQueryFullPath) ?? new QfConfigModel();


            
            // to do perhaps. Do we want to make this work again?
            //// if the query defines a QfDefaultConnection, use it.
            //var match = Regex.Match(state._3InitialQueryText, "^--QfDefaultConnection(=|:)(?<cstr>[^\r\n]*)", RegexOptions.Multiline);
            //if (match.Success)
            //{
            //    queryConfig.DefaultConnection = match.Groups["cstr"].Value;
            //    var matchProviderName = Regex.Match(state._3InitialQueryText, "^--QfDefaultConnectionProviderName(=|:)(?<pn>[^\r\n]*)", RegexOptions.Multiline);
            //    if (matchProviderName.Success)
            //    {
            //        queryConfig.Provider = matchProviderName.Groups["pn"].Value;
            //    }
            //    else
            //    {
            //        queryConfig.Provider = "System.Data.SqlClient";
            //    }

            //}
            state._4Config = queryConfig;
            return state;
        }
    }
}