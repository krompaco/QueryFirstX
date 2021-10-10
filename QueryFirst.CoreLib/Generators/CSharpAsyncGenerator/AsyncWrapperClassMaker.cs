using System;
using System.Data;
using System.Linq;
using System.Text;

namespace QueryFirst
{
    public class AsyncWrapperClassMaker : IWrapperClassMaker
    {
        private static readonly string n = Environment.NewLine;
        public virtual string Usings(State state)
        {
            return @"using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;" +
$"{n}using static {state._1BaseName};" +
(state._8HasTableValuedParams ? $@"
using FastMember; // Table valued params require the FastMember Nuget package
" : n) + state._6ProviderSpecificUsings;


        }
        public virtual string StartNamespace(State state)
        {
            if (!string.IsNullOrEmpty(state._4Namespace))
                return "namespace " + state._4Namespace + "{" + Environment.NewLine;
            else
                return "";
        }
        public virtual string StartClass(State state)
        {
            return
$"public partial class {state._1BaseName} : I{state._1BaseName}{Environment.NewLine}{{" +
@"// props for params
" + string.Join("", state._8QueryParams.Select(qp => $"public {qp.CSType} {qp.CSNamePascal}{{get;set;}}")) + @"
void AppendExececutionMessage(string msg) { ExecutionMessages += msg + Environment.NewLine; }       
public string ExecutionMessages { get; protected set; }
";

        }

        #region Sync

        public virtual string MakeExecuteWithoutConn(State state)
        {
            StringBuilder code = new StringBuilder();
            char[] spaceComma = new char[] { ',', ' ' };
            // Execute method, without connection
            if (state._8QueryParams.Count > 0)
            {
                code.AppendLine(
    $"public virtual List<{state._4ResultInterfaceName}> Execute({state._8MethodSignature.Trim(spaceComma)})\r\n{{"
    + @"
" + string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{ qp.CSNamePascal} = { qp.CSNameCamel};\r\n")) + @"
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
var returnVal = Execute(conn).ToList();
" + string.Join(";\r\n", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{qp.CSNameCamel} = {qp.CSNamePascal}")) + @";
return returnVal;
}
}");
            }
            code.AppendLine(
@"
public virtual List<" + state._4ResultInterfaceName + @"> Execute()
{
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
var returnVal = Execute(conn).ToList();
return returnVal;
}
}
");
            return code.ToString();
        }
        public virtual string MakeExecuteWithConn(State state)
        {
            StringBuilder code = new StringBuilder();
            // Execute method with connection. Use properties. First variant assigns props, then calls inner exec with no args.
            if (state._8QueryParams.Count > 0)
            {
                code.AppendLine(
$"public virtual IEnumerable<{state._4ResultInterfaceName}> Execute({state._8MethodSignature}IDbConnection conn, IDbTransaction tx = null){{" +
            string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{qp.CSNamePascal} = {qp.CSNameCamel};\r\n")) + @"
var returnVal = Execute(conn);
" + string.Join("", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{ qp.CSNameCamel} = { qp.CSNamePascal};\r\n")) + // assign output params from prome
@"return returnVal;
}"
                );
            }
            // Inner exec with no args. Parameters have already been stocked in class props.
            code.AppendLine(
$"public virtual IEnumerable<{state._4ResultInterfaceName}> Execute(IDbConnection conn, IDbTransaction tx = null){{\r\n" +
state._8HookupExecutionMessagesMethodText + @"
using(IDbCommand cmd = conn.CreateCommand())
{
if(tx != null)
cmd.Transaction = tx;
cmd.CommandText = getCommandText();"
            );
            foreach (var qp in state._8QueryParams)
            {
                // Direction
                string direction;
                if (qp.IsInput && qp.IsOutput)
                    direction = "ParameterDirection.InputOutput";
                else if (qp.IsOutput)
                    direction = "ParameterDirection.Output";
                else
                    direction = "ParameterDirection.Input";


                //code.AppendLine("AddAParameter(cmd, \"" + qp.DbType + "\", \"" + qp.DbName + "\", " + qp.CSName + ", " + qp.Length + ", " + qp.Scale + ", " + qp.Precision + ");");
                code.Append(@"
{
var myParam = cmd.CreateParameter();
myParam.Direction = " + direction + @";
myParam.ParameterName = """ + qp.DbName + @""";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), """ + qp.DbType + "\");\r\n" +
(qp.IsInput ? $"myParam.Value = (object){qp.CSNamePascal} ?? DBNull.Value;" : "") + Environment.NewLine +
@"cmd.Parameters.Add(myParam);
}"
                );

            }
            code.Append(@"
using (var reader = cmd.ExecuteReader())
{
while (reader.Read())
{
yield return Create(reader);
}
}

// Assign output parameters to instance properties. These will be available after this method returns.
" + string.Join("", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{qp.CSNamePascal} = ((SqlParameter)cmd.Parameters[\"{qp.DbName}\"]).Value == DBNull.Value?null:({qp.CSType})((SqlParameter)cmd.Parameters[\"{qp.DbName}\"]).Value;\r\n")) + @"
}
}
");


            return code.ToString();
        }
        public virtual string MakeGetOneWithoutConn(State state)
        {
            // don't make GetOne if there are output params.
            if (state._8QueryParams.Where(qp => qp.IsOutput).Count() == 0)
            {
                char[] spaceComma = new char[] { ',', ' ' };
                string code = "";
                if (state._8QueryParams.Count > 0)
                {
                    code +=
$"public virtual {state._4ResultInterfaceName} GetOne({state._8MethodSignature.Trim(spaceComma)})" + @"
{
" + string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{qp.CSNamePascal} = {qp.CSNameCamel};\r\n")) + @"
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
var returnVal = GetOne(conn);
return returnVal;
}
}";
                }
                code +=
$"public virtual {state._4ResultInterfaceName}  GetOne()" + @"
{
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
return GetOne(conn);
}
}
";
                return code;
            }
            else return "";

        }
        public virtual string MakeGetOneWithConn(State state)
        {
            // don't make GetOne if there are output params.
            if (state._8QueryParams.Where(qp => qp.IsOutput).Count() == 0)
            {
                // GetOne() with connection
                string code = "";
                if (state._8QueryParams.Count > 0)
                {

                    code +=
$"public virtual {state._4ResultInterfaceName} GetOne({state._8MethodSignature}IDbConnection conn, IDbTransaction tx = null)" + @"
{
" + string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{qp.CSNamePascal} = {qp.CSNameCamel};\r\n")) + @"
{
var returnVal = GetOne(conn);
return returnVal;
}
}";
                }
                code +=
$"public virtual {state._4ResultInterfaceName} GetOne(IDbConnection conn, IDbTransaction tx = null)" + @"
{
" + state._8HookupExecutionMessagesMethodText + @"
{
var all = Execute(conn,tx);
" + state._4ResultInterfaceName + @" returnVal;
using (IEnumerator<" + state._4ResultInterfaceName + @"> iter = all.GetEnumerator())
{
iter.MoveNext();
returnVal = iter.Current;
}
return returnVal;
}
}
";
                return code;
            }
            else return "";
        }
        public virtual string MakeExecuteScalarWithoutConn(State state)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();
            //ExecuteScalar without connection
            if (state._8QueryParams.Count > 0)
            {
                code.AppendLine(
"public virtual " + state._7ExecuteScalarReturnType + " ExecuteScalar(" + state._8MethodSignature.Trim(spaceComma) + @"){
" + string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{qp.CSNamePascal} = {qp.CSNameCamel};\r\n")) + @"
var returnVal = ExecuteScalar();
" + string.Join(";\r\n", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{qp.CSNameCamel} = {qp.CSNamePascal}")) + @";
return returnVal;
}
");
            }
            code.AppendLine(
@"public virtual " + state._7ExecuteScalarReturnType + @" ExecuteScalar(){
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
return ExecuteScalar(conn);
}
}
"
            );
            return code.ToString();
        }
        public virtual string MakeExecuteScalarWithConn(State state)
        {
            StringBuilder code = new StringBuilder();
            // ExecuteScalar() with connection
            if (state._8QueryParams.Count > 0)
            {
                code.AppendLine(
"public virtual " + state._7ExecuteScalarReturnType + " ExecuteScalar(" + state._8MethodSignature + @"IDbConnection conn, IDbTransaction tx = null){
" + string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{qp.CSNamePascal} = {qp.CSNameCamel};\r\n")) + @"
var returnVal = ExecuteScalar(conn, tx);
" + string.Join(";\r\n", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{qp.CSNameCamel} = {qp.CSNamePascal}")) + @";
return returnVal;
}");
            }
            code.AppendLine(
@"public virtual " + state._7ExecuteScalarReturnType + @" ExecuteScalar(IDbConnection conn, IDbTransaction tx = null){
" + state._8HookupExecutionMessagesMethodText + @"
using(IDbCommand cmd = conn.CreateCommand()){
if(tx != null)
cmd.Transaction = tx;
cmd.CommandText = getCommandText();
"
            );
            foreach (var qp in state._8QueryParams)
            {
                // Direction
                string direction;
                if (qp.IsInput && qp.IsOutput)
                    direction = "ParameterDirection.InputOutput";
                else if (qp.IsOutput)
                    direction = "ParameterDirection.Output";
                else
                    direction = "ParameterDirection.Input";

                code.Append(@"
{
var myParam = cmd.CreateParameter();
myParam.Direction = " + direction + @";
myParam.ParameterName = """ + qp.DbName + @""";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), """ + qp.DbType + "\");\r\n" +
(qp.IsInput ? $"myParam.Value = (object){qp.CSNamePascal} ?? DBNull.Value;" : "") + Environment.NewLine +
@"cmd.Parameters.Add(myParam);
}"
                );

            }
            code.AppendLine(
@"var result = cmd.ExecuteScalar();

// only convert dbnull if nullable
// Assign output parameters to instance properties. 
" + string.Join("", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{qp.CSNamePascal} = ((SqlParameter)cmd.Parameters[\"{qp.DbName}\"]).Value == DBNull.Value?null:({qp.CSType})((SqlParameter)cmd.Parameters[\"{qp.DbName}\"]).Value;\r\n")) + @"
if( result == null || result == DBNull.Value)
return null;
else
return (" + state._7ExecuteScalarReturnType + @")result;
}
}
"
            );// close ExecuteScalar()
            return code.ToString();
        }
        public virtual string MakeExecuteNonQueryWithoutConn(State state)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            //ExecuteScalar without connection
            string code = "";
            if (state._8QueryParams.Count > 0)
            {
                code +=
                "public virtual int ExecuteNonQuery(" + state._8MethodSignature.Trim(spaceComma) + @")
{
" + string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{qp.CSNamePascal} = {qp.CSNameCamel};\r\n")) + @"
var returnVal = ExecuteNonQuery();
" + string.Join(";\r\n", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{qp.CSNameCamel} = {qp.CSNamePascal}")) + @";
return returnVal;
}
";
            }
            code +=
@"
public virtual int ExecuteNonQuery()
{
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
return ExecuteNonQuery(conn);
}
}
";
            return code;
        }
        public virtual string MakeExecuteNonQueryWithConn(State state)
        {
            StringBuilder code = new StringBuilder();
            // ExecuteNonQuery() with connection
            if (state._8QueryParams.Count > 0)
            {
                code.AppendLine(
"public virtual int ExecuteNonQuery(" + state._8MethodSignature + @"IDbConnection conn, IDbTransaction tx = null)
{
" + string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{qp.CSNamePascal} = {qp.CSNameCamel};\r\n")) + @"
var returnVal = ExecuteNonQuery(conn, tx);
" + string.Join(";\r\n", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{qp.CSNameCamel} = {qp.CSNamePascal}")) + @";
return returnVal;
}
");
            }
            code.AppendLine(
@"public virtual int ExecuteNonQuery(IDbConnection conn, IDbTransaction tx = null){
" + state._8HookupExecutionMessagesMethodText + @"
using(IDbCommand cmd = conn.CreateCommand())
{
if(tx != null)
cmd.Transaction = tx;
cmd.CommandText = getCommandText();
"
            );
            foreach (var qp in state._8QueryParams)
            {
                // Direction
                string direction;
                if (qp.IsInput && qp.IsOutput)
                    direction = "ParameterDirection.InputOutput";
                else if (qp.IsOutput)
                    direction = "ParameterDirection.Output";
                else
                    direction = "ParameterDirection.Input";


                //code.AppendLine("AddAParameter(cmd, \"" + qp.DbType + "\", \"" + qp.DbName + "\", " + qp.CSName + ", " + qp.Length + ", " + qp.Scale + ", " + qp.Precision + ");");
                code.Append(@"
{
var myParam = cmd.CreateParameter();
myParam.Direction = " + direction + @";
myParam.ParameterName = """ + qp.DbName + @""";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), """ + qp.DbType + "\");\r\n" +
(qp.IsInput ? $"myParam.Value = (object){qp.CSNamePascal} ?? DBNull.Value;" : "") + Environment.NewLine +
@"cmd.Parameters.Add(myParam);
}"
                );

            }
            code.AppendLine(
@"var result = cmd.ExecuteNonQuery();

// Assign output parameters to instance properties. 
" + string.Join("", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{qp.CSNamePascal} = ((SqlParameter)cmd.Parameters[\"{qp.DbName}\"]).Value == DBNull.Value?null:({qp.CSType})((SqlParameter)cmd.Parameters[\"{qp.DbName}\"]).Value;\r\n")) + @"
// only convert dbnull if nullable
return result;
}
}
"
            );// close ExecuteNonQuery()
            return code.ToString();
        }
        #endregion

        #region Async

        public virtual string MakeExecuteWithoutConn_A(State state)
        {
            StringBuilder code = new StringBuilder();
            char[] spaceComma = new char[] { ',', ' ' };
            // Execute method, without connection
            if (state._8QueryParams.Count > 0)
            {
                code.AppendLine(
    $"public virtual async Task<List<{state._4ResultInterfaceName}>> ExecuteAsync({state._8MethodSignature.Trim(spaceComma)})\r\n{{" + @"
" + string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{ qp.CSNamePascal} = { qp.CSNameCamel};\r\n"))

  + @"
using (DbConnection conn = (DbConnection)QfRuntimeConnection.GetConnection())
{
await conn.OpenAsync();
var returnVal = await ExecuteAsync(conn);"
+ string.Join(";\r\n", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{ qp.CSNameCamel} = { qp.CSNamePascal}")) + @"
return returnVal.ToList();
}
}");
            }

            code.AppendLine(
            @"
public virtual async Task<List<" + state._4ResultInterfaceName + @">> ExecuteAsync()
{
using (DbConnection conn = (DbConnection)QfRuntimeConnection.GetConnection())
{
await conn.OpenAsync();
var returnVal = await ExecuteAsync(conn);
return returnVal.ToList();
}
}
");
            return code.ToString();
        }
        public virtual string MakeExecuteWithConn_A(State state)
        {
            StringBuilder code = new StringBuilder();
            // Execute method with connection. Use properties. First variant assigns props, then calls inner exec with no args.
            if (state._8QueryParams.Count > 0)
            {
                code.AppendLine(
$"public virtual async Task<IEnumerable<{state._4ResultInterfaceName}>> ExecuteAsync({state._8MethodSignature}IDbConnection conn, IDbTransaction tx = null){{" +
            string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{qp.CSNamePascal} = {qp.CSNameCamel};\r\n")) + @"
var returnVal = await ExecuteAsync(conn);
" + string.Join("", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{ qp.CSNameCamel} = { qp.CSNamePascal};\r\n")) + // assign output params from prome
@"return returnVal;
}"
                );
            }
            // Inner exec with no args. Parameters have already been stocked in class props.
            code.AppendLine(
$"public async virtual Task<IEnumerable<{state._4ResultInterfaceName}>> ExecuteAsync(IDbConnection conn, IDbTransaction tx = null){{\r\n" +
state._8HookupExecutionMessagesMethodText + @"
using (DbCommand cmd = ((SqlConnection)conn).CreateCommand())
{
if(tx != null)
{ cmd.Transaction = (DbTransaction)tx; }

cmd.CommandText = getCommandText();"
            );
            foreach (var qp in state._8QueryParams)
            {
                // Direction
                string direction;
                if (qp.IsInput && qp.IsOutput)
                    direction = "ParameterDirection.InputOutput";
                else if (qp.IsOutput)
                    direction = "ParameterDirection.Output";
                else
                    direction = "ParameterDirection.Input";


                //code.AppendLine("AddAParameter(cmd, \"" + qp.DbType + "\", \"" + qp.DbName + "\", " + qp.CSName + ", " + qp.Length + ", " + qp.Scale + ", " + qp.Precision + ");");
                code.Append(@"

var myParam = cmd.CreateParameter();
myParam.Direction = " + direction + @";
myParam.ParameterName = """ + qp.DbName + @""";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), """ + qp.DbType + "\");\r\n" +
(qp.IsInput ? $"myParam.Value = (object){qp.CSNamePascal} ?? DBNull.Value;" : "") + Environment.NewLine +
@"cmd.Parameters.Add(myParam);"
                );

            }
            code.Append(@"
SqlDataReader reader = (SqlDataReader)await cmd.ExecuteReaderAsync();
                

// Assign output parameters to instance properties. These will be available after this method returns.
" + string.Join("", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{qp.CSNamePascal} = ((SqlParameter)cmd.Parameters[\"{qp.DbName}\"]).Value == DBNull.Value?null:({qp.CSType})((SqlParameter)cmd.Parameters[\"{qp.DbName}\"]).Value;\r\n")) + @"

return ReadItems(reader).ToArray();
}
}

"

+ $"IEnumerable<{state._4ResultInterfaceName}> ReadItems(SqlDataReader reader)"
+ @"        {
            while (reader.Read())
            {
                yield return Create(reader);
            }
        }

");


            return code.ToString();
        }
        public virtual string MakeGetOneWithoutConn_A(State state)
        {
            // don't make GetOne if there are output params.
            if (state._8QueryParams.Where(qp => qp.IsOutput).Count() == 0)
            {
                char[] spaceComma = new char[] { ',', ' ' };
                string code = "";
                if (state._8QueryParams.Count > 0)
                {
                    code +=
$"public virtual async Task<{state._4ResultInterfaceName}> GetOneAsync({state._8MethodSignature.Trim(spaceComma)})" + @"
{
" + string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{qp.CSNamePascal} = {qp.CSNameCamel};\r\n")) + @"
using (DbConnection conn = (DbConnection)QfRuntimeConnection.GetConnection())
{
    await conn.OpenAsync();
    return await GetOneAsync(conn);
}
}";
                }
                code +=
$"public virtual async Task<{state._4ResultInterfaceName}>  GetOneAsync()" + @"
{
using (DbConnection conn = (DbConnection)QfRuntimeConnection.GetConnection())
{
    await conn.OpenAsync();
    return await GetOneAsync(conn);
}
}
";
                return code;
            }
            else return "";

        }
        public virtual string MakeGetOneWithConn_A(State state)
        {
            // don't make GetOne if there are output params.
            if (state._8QueryParams.Where(qp => qp.IsOutput).Count() == 0)
            {
                // GetOne() with connection
                string code = "";
                if (state._8QueryParams.Count > 0)
                {

                    code +=
$"public virtual async Task<{state._4ResultInterfaceName}> GetOneAsync({state._8MethodSignature}IDbConnection conn, IDbTransaction tx = null)" + @"
{
" + string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{qp.CSNamePascal} = {qp.CSNameCamel};\r\n")) + @"

return await GetOneAsync(conn); 
}";
                }
                code +=
$"public virtual async Task<{state._4ResultInterfaceName}> GetOneAsync(IDbConnection conn, IDbTransaction tx = null)" + @"
{
" + state._8HookupExecutionMessagesMethodText + @"
{
var all = await ExecuteAsync(conn,tx);
" + state._4ResultInterfaceName + @" returnVal;
using (IEnumerator<" + state._4ResultInterfaceName + @"> iter = all.GetEnumerator())
{
iter.MoveNext();
returnVal = iter.Current;
}
return returnVal;
}
}
";
                return code;
            }
            else return "";
        }
        public virtual string MakeExecuteScalarWithoutConn_A(State state)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();
            //ExecuteScalar without connection
            if (state._8QueryParams.Count > 0)
            {
                code.AppendLine(
"public virtual async Task<" + state._7ExecuteScalarReturnType + "> ExecuteScalarAsync(" + state._8MethodSignature.Trim(spaceComma) + @"){
" + string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{qp.CSNamePascal} = {qp.CSNameCamel};\r\n")) + @"
var returnVal = await ExecuteScalarAsync();
" + string.Join(";\r\n", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{qp.CSNameCamel} = {qp.CSNamePascal}")) + @";
return returnVal;
}
");
            }
            code.AppendLine(
@"public virtual async Task<" + state._7ExecuteScalarReturnType + @"> ExecuteScalarAsync(){
using (DbConnection conn = (DbConnection)QfRuntimeConnection.GetConnection())
{
await conn.OpenAsync();
return await ExecuteScalarAsync(conn);
}
}
"
            );
            return code.ToString();
        }
        public virtual string MakeExecuteScalarWithConn_A(State state)
        {
            StringBuilder code = new StringBuilder();
            // ExecuteScalar() with connection
            if (state._8QueryParams.Count > 0)
            {
                code.AppendLine(
"public virtual async Task<" + state._7ExecuteScalarReturnType + "> ExecuteScalarAsync(" + state._8MethodSignature + @"IDbConnection conn, IDbTransaction tx = null){
" + string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{qp.CSNamePascal} = {qp.CSNameCamel};\r\n")) + @"
var returnVal = await ExecuteScalarAsync(conn, tx);
" + string.Join(";\r\n", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{qp.CSNameCamel} = {qp.CSNamePascal}")) + @";
return returnVal;
}");
            }
            code.AppendLine(
@"public virtual async Task<" + state._7ExecuteScalarReturnType + @"> ExecuteScalarAsync(IDbConnection conn, IDbTransaction tx = null){
" + state._8HookupExecutionMessagesMethodText + @"
using(DbCommand cmd = ((SqlConnection)conn).CreateCommand()){
if(tx != null)
{ cmd.Transaction = (DbTransaction)tx; }

cmd.CommandText = getCommandText();
"
            );
            foreach (var qp in state._8QueryParams)
            {
                // Direction
                string direction;
                if (qp.IsInput && qp.IsOutput)
                    direction = "ParameterDirection.InputOutput";
                else if (qp.IsOutput)
                    direction = "ParameterDirection.Output";
                else
                    direction = "ParameterDirection.Input";

                code.Append(@"
{
var myParam = cmd.CreateParameter();
myParam.Direction = " + direction + @";
myParam.ParameterName = """ + qp.DbName + @""";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), """ + qp.DbType + "\");\r\n" +
(qp.IsInput ? $"myParam.Value = (object){qp.CSNamePascal} ?? DBNull.Value;" : "") + Environment.NewLine +
@"cmd.Parameters.Add(myParam);
}"
                );

            }
            code.AppendLine(
@"var result = await cmd.ExecuteScalarAsync();

// only convert dbnull if nullable
// Assign output parameters to instance properties. 
" + string.Join("", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{qp.CSNamePascal} = ((SqlParameter)cmd.Parameters[\"{qp.DbName}\"]).Value == DBNull.Value?null:({qp.CSType})((SqlParameter)cmd.Parameters[\"{qp.DbName}\"]).Value;\r\n")) + @"
if( result == null || result == DBNull.Value)
return null;
else
return (" + state._7ExecuteScalarReturnType + @")result;
}
}
"
            );// close ExecuteScalar()
            return code.ToString();
        }
        public virtual string MakeExecuteNonQueryWithoutConn_A(State state)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            //ExecuteScalar without connection
            string code = "";
            if (state._8QueryParams.Count > 0)
            {
                code +=
                "public virtual async Task<int> ExecuteNonQueryAsync(" + state._8MethodSignature.Trim(spaceComma) + @")
{
" + string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{qp.CSNamePascal} = {qp.CSNameCamel};\r\n")) + @"
var returnVal = await ExecuteNonQueryAsync();
" + string.Join(";\r\n", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{qp.CSNameCamel} = {qp.CSNamePascal}")) + @";
return returnVal;
}
";
            }
            code +=
@"
public virtual async Task<int> ExecuteNonQueryAsync()
{
using (DbConnection conn = (DbConnection)QfRuntimeConnection.GetConnection())
{
await conn.OpenAsync();
return await ExecuteNonQueryAsync(conn);
}
}
";
            return code;
        }
        public virtual string MakeExecuteNonQueryWithConn_A(State state)
        {
            StringBuilder code = new StringBuilder();
            // ExecuteNonQuery() with connection
            if (state._8QueryParams.Count > 0)
            {
                code.AppendLine(
"public virtual async Task<int> ExecuteNonQueryAsync(" + state._8MethodSignature + @"IDbConnection conn, IDbTransaction tx = null)
{
" + string.Join("", state._8QueryParams.Where(qp => qp.IsInput).Select(qp => $"{qp.CSNamePascal} = {qp.CSNameCamel};\r\n")) + @"
var returnVal = await ExecuteNonQueryAsync(conn, tx);
" + string.Join(";\r\n", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{qp.CSNameCamel} = {qp.CSNamePascal}")) + @";
return returnVal;
}
");
            }
            code.AppendLine(
@"public virtual async Task<int> ExecuteNonQueryAsync(IDbConnection conn, IDbTransaction tx = null){
" + state._8HookupExecutionMessagesMethodText + @"
using(DbCommand cmd = ((SqlConnection)conn).CreateCommand())
{
if(tx != null)
{ cmd.Transaction = (DbTransaction)tx; }

cmd.CommandText = getCommandText();
"
            );
            foreach (var qp in state._8QueryParams)
            {
                // Direction
                string direction;
                if (qp.IsInput && qp.IsOutput)
                    direction = "ParameterDirection.InputOutput";
                else if (qp.IsOutput)
                    direction = "ParameterDirection.Output";
                else
                    direction = "ParameterDirection.Input";


                //code.AppendLine("AddAParameter(cmd, \"" + qp.DbType + "\", \"" + qp.DbName + "\", " + qp.CSName + ", " + qp.Length + ", " + qp.Scale + ", " + qp.Precision + ");");
                code.Append(@"
{
var myParam = cmd.CreateParameter();
myParam.Direction = " + direction + @";
myParam.ParameterName = """ + qp.DbName + @""";
myParam.DbType = (DbType)Enum.Parse(typeof(DbType), """ + qp.DbType + "\");\r\n" +
(qp.IsInput ? $"myParam.Value = (object){qp.CSNamePascal} ?? DBNull.Value;" : "") + Environment.NewLine +
@"cmd.Parameters.Add(myParam);
}"
                );

            }
            code.AppendLine(
@"var result = await cmd.ExecuteNonQueryAsync();

// Assign output parameters to instance properties. 
" + string.Join("", state._8QueryParams.Where(qp => qp.IsOutput).Select(qp => $"{qp.CSNamePascal} = ((SqlParameter)cmd.Parameters[\"{qp.DbName}\"]).Value == DBNull.Value?null:({qp.CSType})((SqlParameter)cmd.Parameters[\"{qp.DbName}\"]).Value;\r\n")) + @"
// only convert dbnull if nullable
return result;
}
}
"
            );// close ExecuteNonQuery()
            return code.ToString();
        }
        #endregion

        public virtual string MakeCreateMethod(State state)
        {
            return
@"public virtual " + state._4ResultInterfaceName + @" Create(IDataRecord record)
{
var returnVal = CreatePoco(record);
" + string.Join("", state._7ResultFields.Select((f, index) => @"
    if(record[" + index + "] != null && record[" + index + @"] != DBNull.Value)
    returnVal." + f.CSColumnName + " =  (" + f.TypeCsShort + ")record[" + index + @"];
")) + $@"
returnVal.OnLoad();
return returnVal;
}}
protected virtual { state._4ResultInterfaceName } CreatePoco(System.Data.IDataRecord record)
{{
    return new { state._4ResultClassName }();
}}";

        }
        public virtual string MakeGetCommandTextMethod(State state)
        {

            return
@"public string getCommandText(){
return @""
" + state._6FinalQueryTextForCode + @"
"";
}";
        }
        public virtual string MakeTvpPocos(State state)
        {
            var pocos = new StringBuilder();
            foreach (var param in state._8QueryParams)
            {
                if (param.IsTableType)
                {
                    pocos.Append(makeAPoco(param));
                }
            }
            return pocos.ToString();
        }
        string makeAPoco(QueryParamInfo param)
        {
            return
$"public class {param.InnerCSType}{{\n" +
string.Join(",", param.ParamSchema.Select(col => $"public {col.CSType} {col.CSNameCamel}{{get; set;}}\n")) +
"}\n";

        }
        public virtual string MakeOtherMethods(State state)
        {
            return "";
        }
        public virtual string CloseClass(State state)
        {
            return "}" + Environment.NewLine;
        }
        public virtual string CloseNamespace(State state)
        {
            if (!string.IsNullOrEmpty(state._4Namespace))
                return "}" + Environment.NewLine;
            else
                return "";
        }

        public string MakeInterface(State state)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();
            code.AppendLine("public interface I" + state._1BaseName + "{" + Environment.NewLine);
            code.AppendLine("#region Sync");
            if (state._7ResultFields != null && state._7ResultFields.Count > 0)
            {
                if (state._8QueryParams.Count > 0)
                {
                    code.AppendLine(
"List<" + state._4ResultInterfaceName + "> Execute(" + state._8MethodSignature.Trim(spaceComma) + @");
IEnumerable< " + state._4ResultInterfaceName + "> Execute(" + state._8MethodSignature + @"IDbConnection conn, IDbTransaction tx = null);
" + state._7ExecuteScalarReturnType + " ExecuteScalar(" + state._8MethodSignature.Trim(spaceComma) + @");
" + state._7ExecuteScalarReturnType + " ExecuteScalar(" + state._8MethodSignature + @"IDbConnection conn, IDbTransaction tx = null);
"
                    );
                }
                code.AppendLine(
@"List< " + state._4ResultInterfaceName + @" > Execute();
IEnumerable<" + state._4ResultInterfaceName + @"> Execute(IDbConnection conn, IDbTransaction tx = null);
" + state._7ExecuteScalarReturnType + @" ExecuteScalar();
" + state._7ExecuteScalarReturnType + @" ExecuteScalar(IDbConnection conn, IDbTransaction tx = null);
" + state._4ResultInterfaceName + @" Create(IDataRecord record);
"
                );

                if (state._8QueryParams.Where(qp => qp.IsOutput).Count() == 0)
                {
                    if (state._8QueryParams.Count > 0)
                    {
                        code.AppendLine(
state._4ResultInterfaceName + " GetOne(" + state._8MethodSignature.Trim(spaceComma) + @");
" + state._4ResultInterfaceName + " GetOne(" + state._8MethodSignature + @"IDbConnection conn, IDbTransaction tx = null);"
                        );
                    }
                    code.AppendLine(
    state._4ResultInterfaceName + @" GetOne();
" + state._4ResultInterfaceName + @" GetOne(IDbConnection conn, IDbTransaction tx = null);"
                    );
                }
                else code.AppendLine("// GetOne methods are not available because they do not play well with output params.");
            }
            if (state._8QueryParams.Count > 0)
            {
                code.AppendLine(
"int ExecuteNonQuery(" + state._8MethodSignature.Trim(spaceComma) + @");
int ExecuteNonQuery(" + state._8MethodSignature + @"IDbConnection conn, IDbTransaction tx = null);"
                );
            }
            code.AppendLine(
@"int ExecuteNonQuery();            
int ExecuteNonQuery(IDbConnection conn, IDbTransaction tx = null);
" + string.Join("", state._8QueryParams.Select(qp => $"{qp.CSType} {qp.CSNamePascal}{{get;set;}}\r\n")) + @"
#endregion
#region Async");

            if (state._7ResultFields != null && state._7ResultFields.Count > 0)
            {
                if (state._8QueryParams.Count > 0)
                {
                    code.AppendLine(
"Task<List<" + state._4ResultInterfaceName + ">> ExecuteAsync(" + state._8MethodSignature.Trim(spaceComma) + @");
Task<IEnumerable< " + state._4ResultInterfaceName + ">> ExecuteAsync(" + state._8MethodSignature + @"IDbConnection conn, IDbTransaction tx = null);
" + "Task<" + state._7ExecuteScalarReturnType + "> ExecuteScalarAsync(" + state._8MethodSignature.Trim(spaceComma) + @");
" + "Task<" + state._7ExecuteScalarReturnType + "> ExecuteScalarAsync(" + state._8MethodSignature + @"IDbConnection conn, IDbTransaction tx = null);
"
                    );
                }
                code.AppendLine(
@"Task<List< " + state._4ResultInterfaceName + @" >> ExecuteAsync();
Task<IEnumerable<" + state._4ResultInterfaceName + @">> ExecuteAsync(IDbConnection conn, IDbTransaction tx = null);
Task<" + state._7ExecuteScalarReturnType + @"> ExecuteScalarAsync();
Task<" + state._7ExecuteScalarReturnType + @"> ExecuteScalarAsync(IDbConnection conn, IDbTransaction tx = null);");

                if (state._8QueryParams.Where(qp => qp.IsOutput).Count() == 0)
                {
                    if (state._8QueryParams.Count > 0)
                    {
                        code.AppendLine(
"Task<" + state._4ResultInterfaceName + "> GetOneAsync(" + state._8MethodSignature.Trim(spaceComma) + @");
Task<" + state._4ResultInterfaceName + "> GetOneAsync(" + state._8MethodSignature + @"IDbConnection conn, IDbTransaction tx = null);"
                        );
                    }
                    code.AppendLine(
"Task<" + state._4ResultInterfaceName + @"> GetOneAsync();
Task<" + state._4ResultInterfaceName + @"> GetOneAsync(IDbConnection conn, IDbTransaction tx = null);"
                    );
                }
                else code.AppendLine("// GetOne methods are not available because they do not play well with output params.");
            }
            if (state._8QueryParams.Count > 0)
            {
                code.AppendLine(
"Task<int> ExecuteNonQueryAsync(" + state._8MethodSignature.Trim(spaceComma) + @");
Task<int> ExecuteNonQueryAsync(" + state._8MethodSignature + @"IDbConnection conn, IDbTransaction tx = null);");
            }
            code.AppendLine(
@"Task<int> ExecuteNonQueryAsync();            
Task<int> ExecuteNonQueryAsync(IDbConnection conn, IDbTransaction tx = null);
");


code.AppendLine(@"#endregion
string ExecutionMessages { get; }
}
");
            return code.ToString();
        }

        public string SelfTestUsings(State state)
        {
            return
    @"using QueryFirst;
using Xunit;
";
        }

        public string MakeSelfTestMethod(State state)
        {
            char[] spaceComma = new char[] { ',', ' ' };
            StringBuilder code = new StringBuilder();

            code.AppendLine("[Fact]");
            code.AppendLine("public void " + state._1BaseName + "SelfTest()");
            code.AppendLine("{");
            code.AppendLine("var queryText = getCommandText();");
            code.AppendLine("// we'll be getting a runtime version with the comments section closed. To run without parameters, open it.");
            code.AppendLine("queryText = queryText.Replace(\"/*designTime\", \"-- designTime\");");
            code.AppendLine("queryText = queryText.Replace(\"endDesignTime*/\", \"-- endDesignTime\");");
            // QfruntimeConnection will be used, but we still need to reference a provider, for the prepare parameters method.
            code.AppendLine($"var schema = new AdoSchemaFetcher().GetFields(QfRuntimeConnection.GetConnection(), \"{state._3Config.Provider}\", queryText);");
            code.Append("Assert.True(" + state._7ResultFields.Count + " <=  schema.Count,");
            code.AppendLine("\"Query only returns \" + schema.Count.ToString() + \" columns. Expected at least " + state._7ResultFields.Count + ". \");");
            for (int i = 0; i < state._7ResultFields.Count; i++)
            {
                var col = state._7ResultFields[i];
                code.Append("Assert.True(schema[" + i.ToString() + "].TypeDb == \"" + col.TypeDb + "\",");
                code.AppendLine("\"Result Column " + i.ToString() + " Type wrong. Expected " + col.TypeDb + ". Found \" + schema[" + i.ToString() + "].TypeDb + \".\");");
                code.Append("Assert.True(schema[" + i.ToString() + "].ColumnName == \"" + col.ColumnName + "\",");
                code.AppendLine("\"Result Column " + i.ToString() + " Name wrong. Expected " + col.ColumnName + ". Found \" + schema[" + i.ToString() + "].ColumnName + \".\");");
            }
            code.AppendLine("}");
            return code.ToString();
        }
    }
}
