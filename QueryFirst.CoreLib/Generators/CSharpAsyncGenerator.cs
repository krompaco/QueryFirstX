using System;
using System.Collections.Generic;
using System.Text;

namespace QueryFirst
{
    [RegistrationName("CSharpAsync")]
    class CSharpAsyncGenerator : IGenerator
    {
        public QfTextFile Generate(State state)
        {
            StringBuilder Code = new StringBuilder();
            var _wrapper = new AsyncWrapperClassMaker();
            var _results = new AsyncResultClassMaker();
            Code.Append(_wrapper.StartNamespace(state));
            Code.Append(_wrapper.Usings(state));
            if (state._3Config.MakeSelfTest.GetValueOrDefault())
                Code.Append(_wrapper.SelfTestUsings(state));
            if (state._7ResultFields != null && state._7ResultFields.Count > 0)
                Code.Append(_results.Usings());
            Code.Append(_wrapper.MakeInterface(state));
            Code.Append(_wrapper.StartClass(state));
            Code.AppendLine("");
            Code.AppendLine("#region Sync");
            Code.Append(_wrapper.MakeExecuteNonQueryWithoutConn(state));
            Code.Append(_wrapper.MakeExecuteNonQueryWithConn(state));
            Code.AppendLine("");
            Code.AppendLine("#endregion");
            Code.AppendLine("");
            Code.AppendLine("");
            Code.AppendLine("#region ASync");
            Code.AppendLine("");
            Code.Append(_wrapper.MakeExecuteNonQueryWithoutConn_A(state));
            Code.Append(_wrapper.MakeExecuteNonQueryWithConn_A(state));
            Code.AppendLine("#endregion");

            Code.Append(_wrapper.MakeGetCommandTextMethod(state));
            //Code.Append(_provider.MakeAddAParameter(state));
            Code.Append(_wrapper.MakeTvpPocos(state));

            if (state._3Config.MakeSelfTest.GetValueOrDefault())
                Code.Append(_wrapper.MakeSelfTestMethod(state));
            if (state._7ResultFields != null && state._7ResultFields.Count > 0)
            {
                Code.AppendLine("");
                Code.AppendLine("#region Sync");
                Code.AppendLine("");
                Code.Append(_wrapper.MakeExecuteWithoutConn(state));
                Code.Append(_wrapper.MakeExecuteWithConn(state));
                Code.Append(_wrapper.MakeGetOneWithoutConn(state));
                Code.Append(_wrapper.MakeGetOneWithConn(state));
                Code.Append(_wrapper.MakeExecuteScalarWithoutConn(state));
                Code.Append(_wrapper.MakeExecuteScalarWithConn(state));
                Code.AppendLine("");
                Code.AppendLine("#endregion");
                Code.AppendLine("");
                Code.AppendLine("#region ASync");
                Code.AppendLine("");
                Code.Append(_wrapper.MakeExecuteWithoutConn_A(state));
                Code.Append(_wrapper.MakeExecuteWithConn_A(state));
                Code.Append(_wrapper.MakeGetOneWithoutConn_A(state));
                Code.Append(_wrapper.MakeGetOneWithConn_A(state));
                Code.Append(_wrapper.MakeExecuteScalarWithoutConn_A(state));
                Code.Append(_wrapper.MakeExecuteScalarWithConn_A(state));
                Code.AppendLine("");
                Code.AppendLine("#endregion");
                Code.AppendLine("");

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
