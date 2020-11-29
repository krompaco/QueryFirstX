using System.Collections.Generic;

namespace QueryFirst
{
    public class QfConfigModel
    {
        public string DefaultConnection { get; set; }
        public string Provider { get; set; }
        public List<string> HelperAssemblies { get; set; }
        public bool? MakeSelfTest { get; set; }
        public List<Generator> Generators { get; set; }
        public string Namespace { get; set; }
        public string ResultClassName { get; set; }
        public string ResultInterfaceName { get; set; }

    }
}
