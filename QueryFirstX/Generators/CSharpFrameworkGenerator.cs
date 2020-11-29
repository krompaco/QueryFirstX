using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst
{
    [RegistrationName("CSharpFramework")]
    class CSharpFrameworkGenerator : IGenerator
    {
        public QfTextFile Generate(State state)
        {
            StringBuilder Code = new StringBuilder();
            var _wrapper = new WrapperClassMaker();
            var _results = new ResultClassMaker();
            Code.Append(_wrapper.StartNamespace(state));
            Code.Append(_wrapper.Usings(state));
            if (state._4Config.MakeSelfTest.GetValueOrDefault())
                Code.Append(_wrapper.SelfTestUsings(state));
            if (state._7ResultFields != null && state._7ResultFields.Count > 0)
                Code.Append(_results.Usings());
            Code.Append(_wrapper.MakeInterface(state));
            Code.Append(_wrapper.StartClass(state));
            Code.Append(_wrapper.MakeExecuteNonQueryWithoutConn(state));
            Code.Append(_wrapper.MakeExecuteNonQueryWithConn(state));
            Code.Append(_wrapper.MakeGetCommandTextMethod(state));
            //Code.Append(_provider.MakeAddAParameter(state));
            Code.Append(_wrapper.MakeTvpPocos(state));

            if (state._4Config.MakeSelfTest.GetValueOrDefault())
                Code.Append(_wrapper.MakeSelfTestMethod(state));
            if (state._7ResultFields != null && state._7ResultFields.Count > 0)
            {
                Code.Append(_wrapper.MakeExecuteWithoutConn(state));
                Code.Append(_wrapper.MakeExecuteWithConn(state));
                Code.Append(_wrapper.MakeGetOneWithoutConn(state));
                Code.Append(_wrapper.MakeGetOneWithConn(state));
                Code.Append(_wrapper.MakeExecuteScalarWithoutConn(state));
                Code.Append(_wrapper.MakeExecuteScalarWithConn(state));

                Code.Append(_wrapper.MakeCreateMethod(state));
                Code.Append(_wrapper.MakeOtherMethods(state));
                Code.Append(_wrapper.CloseClass(state));
                Code.Append(_results.StartClass(state));
                foreach (var fld in state._7ResultFields)
                {
                    Code.Append(_results.MakeProperty(fld));
                }
            }
            Code.Append(_results.CloseClass()); // closes wrapper class if no results !
            Code.Append(_wrapper.CloseNamespace(state));

            return new QfTextFile()
            {
                Filename = state._1GeneratedClassFullFilename,
                FileContents = Code.ToString()
            };
        }
    }
}
