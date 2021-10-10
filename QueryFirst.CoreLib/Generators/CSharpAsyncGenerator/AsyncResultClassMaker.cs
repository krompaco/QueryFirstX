using System;
using System.Text;

namespace QueryFirst
{
    public class AsyncResultClassMaker : IResultClassMaker
    {
        public virtual string Usings() { return ""; }

        private string nl = Environment.NewLine;
        public virtual string StartClass(State state)
        {
            return string.Format("public partial class {0} {{" + nl, state._4ResultClassName);
        }
        public virtual string MakeProperty(ResultFieldDetails fld)
        {
            StringBuilder code = new StringBuilder();
            code.AppendLine($"protected {fld.TypeCsShort} _{fld.CSColumnName}; //({fld.TypeDb} {(fld.AllowDBNull ? "null" : "not null")})");
            code.AppendLine($"public {fld.TypeCsShort} {fld.CSColumnName}{{{nl}get{{return _{fld.CSColumnName};}}{nl}set{{_{fld.CSColumnName} = value;}}{nl}}}");
            return code.ToString();
        }

        public virtual string CloseClass()
        {
            return $@"protected internal virtual void OnLoad(){{}}
}}
";
        }
    }
}
