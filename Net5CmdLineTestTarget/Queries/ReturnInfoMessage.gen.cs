namespace Net5CmdLineTestTarget{
using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using static ReturnInfoMessage;

using System.Data.SqlClient;
public interface IReturnInfoMessage{

int ExecuteNonQuery();            
int ExecuteNonQuery(IDbConnection conn, IDbTransaction tx = null);

string ExecutionMessages { get; }
}

public partial class ReturnInfoMessage : IReturnInfoMessage
{

void AppendExececutionMessage(string msg) { ExecutionMessages += msg + Environment.NewLine; }

public string ExecutionMessages { get; protected set; }

public virtual int ExecuteNonQuery()
{
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
return ExecuteNonQuery(conn);
}
}
public virtual int ExecuteNonQuery(IDbConnection conn, IDbTransaction tx = null){
// this line will not compile in .net core unless you install the System.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });
using(IDbCommand cmd = conn.CreateCommand())
{
if(tx != null)
cmd.Transaction = tx;
cmd.CommandText = getCommandText();

var result = cmd.ExecuteNonQuery();

// Assign output parameters to instance properties. 

// only convert dbnull if nullable
return result;
}
}

public string getCommandText(){
var queryText = $@"-- use querfirst
/*designTime

endDesignTime*/

PRINT 'hello cobber'
";
return queryText;
}protected internal virtual void OnLoad(){}
}
}
