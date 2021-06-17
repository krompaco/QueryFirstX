using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst.VSExtension.TypeMappings
{
    class Postgres2CS : IDB2CS
    {
        public string Map(string DBType, out string DBTypeNormalized, bool nullable = true)
        {
            throw new NotImplementedException();
        }
    }
}
