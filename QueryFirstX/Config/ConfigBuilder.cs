using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst
{
    public class ConfigBuilder
    {
        public QfConfigModel GetInstallConfigForProjectType(List<ProjectSection> installFull, string projectType)
        {
            return installFull.Where(c => c.ProjectType == projectType).FirstOrDefault().QfConfig;
        }
        public QfConfigModel Resolve2Configs(QfConfigModel overridden, QfConfigModel overides)
        {
            QfConfigModel returnVal = new QfConfigModel
            {
                DefaultConnection = overides.DefaultConnection ?? overridden.DefaultConnection,
                Provider = overides.Provider ?? overridden.Provider,
                HelperAssemblies = new List<string>(),
                Generators = overides.Generators ?? overridden.Generators,
                MakeSelfTest = overides.MakeSelfTest ?? overridden.MakeSelfTest,
                Namespace = overides.Namespace ?? overridden.Namespace
            };

            // helper assemblies. Unlike other config, these cumulate.
            if (overides.HelperAssemblies is not null)
            {
                returnVal.HelperAssemblies.AddRange(overides.HelperAssemblies);
            }
            if (overridden.HelperAssemblies is not null)
            {
                returnVal.HelperAssemblies.AddRange(overridden.HelperAssemblies);
            }
            return returnVal;
        }
    }
}
