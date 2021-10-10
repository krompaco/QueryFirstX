namespace Net5CmdLineTestTarget{
using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;
using static GetCustomersAsync;
using System.Data.SqlClient;
public interface IGetCustomersAsync{

#region Sync
List< GetCustomersAsyncResults > Execute();
IEnumerable<GetCustomersAsyncResults> Execute(IDbConnection conn, IDbTransaction tx = null);
System.String ExecuteScalar();
System.String ExecuteScalar(IDbConnection conn, IDbTransaction tx = null);
GetCustomersAsyncResults Create(IDataRecord record);

GetCustomersAsyncResults GetOne();
GetCustomersAsyncResults GetOne(IDbConnection conn, IDbTransaction tx = null);
int ExecuteNonQuery();            
int ExecuteNonQuery(IDbConnection conn, IDbTransaction tx = null);

#endregion
#region Async
Task<List< GetCustomersAsyncResults >> ExecuteAsync();
Task<IEnumerable<GetCustomersAsyncResults>> ExecuteAsync(IDbConnection conn, IDbTransaction tx = null);
Task<System.String> ExecuteScalarAsync();
Task<System.String> ExecuteScalarAsync(IDbConnection conn, IDbTransaction tx = null);
Task<GetCustomersAsyncResults> GetOneAsync();
Task<GetCustomersAsyncResults> GetOneAsync(IDbConnection conn, IDbTransaction tx = null);
Task<int> ExecuteNonQueryAsync();            
Task<int> ExecuteNonQueryAsync(IDbConnection conn, IDbTransaction tx = null);

#endregion
string ExecutionMessages { get; }
}

public partial class GetCustomersAsync : IGetCustomersAsync
{// props for params

void AppendExececutionMessage(string msg) { ExecutionMessages += msg + Environment.NewLine; }       
public string ExecutionMessages { get; protected set; }

#region Sync

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


#endregion


#region ASync


public virtual async Task<int> ExecuteNonQueryAsync()
{
using (DbConnection conn = (DbConnection)QfRuntimeConnection.GetConnection())
{
await conn.OpenAsync();
return await ExecuteNonQueryAsync(conn);
}
}
public virtual async Task<int> ExecuteNonQueryAsync(IDbConnection conn, IDbTransaction tx = null){
// this line will not compile in .net core unless you install the System.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });
using(DbCommand cmd = ((SqlConnection)conn).CreateCommand())
{
if(tx != null)
{ cmd.Transaction = (DbTransaction)tx; }

cmd.CommandText = getCommandText();

var result = await cmd.ExecuteNonQueryAsync();

// Assign output parameters to instance properties. 

// only convert dbnull if nullable
return result;
}
}

#endregion
public string getCommandText(){
return @"
-- QueryFirst will save you
/*designTime

endDesignTime*/
select * from customers

";
}
#region Sync


public virtual List<GetCustomersAsyncResults> Execute()
{
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
var returnVal = Execute(conn).ToList();
return returnVal;
}
}

public virtual IEnumerable<GetCustomersAsyncResults> Execute(IDbConnection conn, IDbTransaction tx = null){
// this line will not compile in .net core unless you install the System.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });
using(IDbCommand cmd = conn.CreateCommand())
{
if(tx != null)
cmd.Transaction = tx;
cmd.CommandText = getCommandText();

using (var reader = cmd.ExecuteReader())
{
while (reader.Read())
{
yield return Create(reader);
}
}

// Assign output parameters to instance properties. These will be available after this method returns.

}
}
public virtual GetCustomersAsyncResults  GetOne()
{
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
return GetOne(conn);
}
}
public virtual GetCustomersAsyncResults GetOne(IDbConnection conn, IDbTransaction tx = null)
{
// this line will not compile in .net core unless you install the System.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });
{
var all = Execute(conn,tx);
GetCustomersAsyncResults returnVal;
using (IEnumerator<GetCustomersAsyncResults> iter = all.GetEnumerator())
{
iter.MoveNext();
returnVal = iter.Current;
}
return returnVal;
}
}
public virtual System.String ExecuteScalar(){
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
return ExecuteScalar(conn);
}
}

public virtual System.String ExecuteScalar(IDbConnection conn, IDbTransaction tx = null){
// this line will not compile in .net core unless you install the System.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });
using(IDbCommand cmd = conn.CreateCommand()){
if(tx != null)
cmd.Transaction = tx;
cmd.CommandText = getCommandText();

var result = cmd.ExecuteScalar();

// only convert dbnull if nullable
// Assign output parameters to instance properties. 

if( result == null || result == DBNull.Value)
return null;
else
return (System.String)result;
}
}


#endregion

#region ASync


public virtual async Task<List<GetCustomersAsyncResults>> ExecuteAsync()
{
using (DbConnection conn = (DbConnection)QfRuntimeConnection.GetConnection())
{
await conn.OpenAsync();
var returnVal = await ExecuteAsync(conn);
return returnVal.ToList();
}
}

public async virtual Task<IEnumerable<GetCustomersAsyncResults>> ExecuteAsync(IDbConnection conn, IDbTransaction tx = null){
// this line will not compile in .net core unless you install the System.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });
using (DbCommand cmd = ((SqlConnection)conn).CreateCommand())
{
if(tx != null)
{ cmd.Transaction = (DbTransaction)tx; }

cmd.CommandText = getCommandText();

SqlDataReader reader = (SqlDataReader)await cmd.ExecuteReaderAsync();
                

// Assign output parameters to instance properties. These will be available after this method returns.


return ReadItems(reader).ToArray();
}
}

IEnumerable<GetCustomersAsyncResults> ReadItems(SqlDataReader reader)        {
            while (reader.Read())
            {
                yield return Create(reader);
            }
        }

public virtual async Task<GetCustomersAsyncResults>  GetOneAsync()
{
using (DbConnection conn = (DbConnection)QfRuntimeConnection.GetConnection())
{
    await conn.OpenAsync();
    return await GetOneAsync(conn);
}
}
public virtual async Task<GetCustomersAsyncResults> GetOneAsync(IDbConnection conn, IDbTransaction tx = null)
{
// this line will not compile in .net core unless you install the System.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });
{
var all = await ExecuteAsync(conn,tx);
GetCustomersAsyncResults returnVal;
using (IEnumerator<GetCustomersAsyncResults> iter = all.GetEnumerator())
{
iter.MoveNext();
returnVal = iter.Current;
}
return returnVal;
}
}
public virtual async Task<System.String> ExecuteScalarAsync(){
using (DbConnection conn = (DbConnection)QfRuntimeConnection.GetConnection())
{
await conn.OpenAsync();
return await ExecuteScalarAsync(conn);
}
}

public virtual async Task<System.String> ExecuteScalarAsync(IDbConnection conn, IDbTransaction tx = null){
// this line will not compile in .net core unless you install the System.Data.SqlClient nuget package.
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(
    delegate (object sender, SqlInfoMessageEventArgs e)  { AppendExececutionMessage(e.Message); });
using(DbCommand cmd = ((SqlConnection)conn).CreateCommand()){
if(tx != null)
{ cmd.Transaction = (DbTransaction)tx; }

cmd.CommandText = getCommandText();

var result = await cmd.ExecuteScalarAsync();

// only convert dbnull if nullable
// Assign output parameters to instance properties. 

if( result == null || result == DBNull.Value)
return null;
else
return (System.String)result;
}
}


#endregion

public virtual GetCustomersAsyncResults Create(IDataRecord record)
{
var returnVal = CreatePoco(record);

    if(record[0] != null && record[0] != DBNull.Value)
    returnVal.CustomerID =  (string)record[0];

    if(record[1] != null && record[1] != DBNull.Value)
    returnVal.CompanyName =  (string)record[1];

    if(record[2] != null && record[2] != DBNull.Value)
    returnVal.ContactName =  (string)record[2];

    if(record[3] != null && record[3] != DBNull.Value)
    returnVal.ContactTitle =  (string)record[3];

    if(record[4] != null && record[4] != DBNull.Value)
    returnVal.Address =  (string)record[4];

    if(record[5] != null && record[5] != DBNull.Value)
    returnVal.City =  (string)record[5];

    if(record[6] != null && record[6] != DBNull.Value)
    returnVal.Region =  (string)record[6];

    if(record[7] != null && record[7] != DBNull.Value)
    returnVal.PostalCode =  (string)record[7];

    if(record[8] != null && record[8] != DBNull.Value)
    returnVal.Country =  (string)record[8];

    if(record[9] != null && record[9] != DBNull.Value)
    returnVal.Phone =  (string)record[9];

    if(record[10] != null && record[10] != DBNull.Value)
    returnVal.Fax =  (string)record[10];

returnVal.OnLoad();
return returnVal;
}
protected virtual GetCustomersAsyncResults CreatePoco(System.Data.IDataRecord record)
{
    return new GetCustomersAsyncResults();
}}
public partial class GetCustomersAsyncResults {
protected string _CustomerID; //(nchar not null)
public string CustomerID{
get{return _CustomerID;}
set{_CustomerID = value;}
}
protected string _CompanyName; //(nvarchar not null)
public string CompanyName{
get{return _CompanyName;}
set{_CompanyName = value;}
}
protected string _ContactName; //(nvarchar null)
public string ContactName{
get{return _ContactName;}
set{_ContactName = value;}
}
protected string _ContactTitle; //(nvarchar null)
public string ContactTitle{
get{return _ContactTitle;}
set{_ContactTitle = value;}
}
protected string _Address; //(nvarchar null)
public string Address{
get{return _Address;}
set{_Address = value;}
}
protected string _City; //(nvarchar null)
public string City{
get{return _City;}
set{_City = value;}
}
protected string _Region; //(nvarchar null)
public string Region{
get{return _Region;}
set{_Region = value;}
}
protected string _PostalCode; //(nvarchar null)
public string PostalCode{
get{return _PostalCode;}
set{_PostalCode = value;}
}
protected string _Country; //(nvarchar null)
public string Country{
get{return _Country;}
set{_Country = value;}
}
protected string _Phone; //(nvarchar null)
public string Phone{
get{return _Phone;}
set{_Phone = value;}
}
protected string _Fax; //(nvarchar null)
public string Fax{
get{return _Fax;}
set{_Fax = value;}
}
protected internal virtual void OnLoad(){}
}
}
