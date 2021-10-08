namespace Net5CmdLineTestTarget{
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using static TestDynamicOrderBy;

public interface ITestDynamicOrderBy{

List<TestDynamicOrderByResults> Execute((Cols col, bool descending)[] orderBy);
IEnumerable< TestDynamicOrderByResults> Execute((Cols col, bool descending)[] orderBy,IDbConnection conn, IDbTransaction tx = null);
System.String ExecuteScalar((Cols col, bool descending)[] orderBy);
System.String ExecuteScalar((Cols col, bool descending)[] orderBy,IDbConnection conn, IDbTransaction tx = null);

List< TestDynamicOrderByResults > Execute();
IEnumerable<TestDynamicOrderByResults> Execute(IDbConnection conn, IDbTransaction tx = null);
System.String ExecuteScalar();
System.String ExecuteScalar(IDbConnection conn, IDbTransaction tx = null);
TestDynamicOrderByResults Create(IDataRecord record);

TestDynamicOrderByResults GetOne((Cols col, bool descending)[] orderBy);
TestDynamicOrderByResults GetOne((Cols col, bool descending)[] orderBy,IDbConnection conn, IDbTransaction tx = null);
TestDynamicOrderByResults GetOne();
TestDynamicOrderByResults GetOne(IDbConnection conn, IDbTransaction tx = null);
int ExecuteNonQuery((Cols col, bool descending)[] orderBy);
int ExecuteNonQuery((Cols col, bool descending)[] orderBy,IDbConnection conn, IDbTransaction tx = null);
int ExecuteNonQuery();            
int ExecuteNonQuery(IDbConnection conn, IDbTransaction tx = null);

string ExecutionMessages { get; }
}

public partial class TestDynamicOrderBy : ITestDynamicOrderBy
{

void AppendExececutionMessage(string msg) { ExecutionMessages += msg + Environment.NewLine; }

public (Cols col, bool descending)[] OrderBy{get;set;}

public string ExecutionMessages { get; protected set; }
public virtual int ExecuteNonQuery((Cols col, bool descending)[] orderBy)
{

OrderBy = orderBy;
var returnVal = ExecuteNonQuery();
;
return returnVal;
}

public virtual int ExecuteNonQuery()
{
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
return ExecuteNonQuery(conn);
}
}
public virtual int ExecuteNonQuery((Cols col, bool descending)[] orderBy,IDbConnection conn, IDbTransaction tx = null)
{

OrderBy = orderBy;
var returnVal = ExecuteNonQuery(conn, tx);
;
return returnVal;
}

public virtual int ExecuteNonQuery(IDbConnection conn, IDbTransaction tx = null){
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(delegate (object sender, SqlInfoMessageEventArgs e) { AppendExececutionMessage(e.Message); });
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
var queryText = $@"-- use queryfirst
/*designTime

endDesignTime*/

select * from customers
--qforderby

";

// Dynamic order by
if(OrderBy != null && OrderBy.Length > 0)
{
    var dynamicOrderBy = $" order by {string.Join(", ", OrderBy.Select((t)=> $"{t.col} {(t.descending?"desc":"asc")}" ))} ";
    var pattern = @"--\s*qforderby";
    queryText = Regex.Replace(queryText, pattern, dynamicOrderBy, RegexOptions.IgnoreCase | RegexOptions.Multiline);
}
return queryText;
}

public enum Cols
{
CustomerID = 1,
CompanyName = 2,
ContactName = 3,
ContactTitle = 4,
Address = 5,
City = 6,
Region = 7,
PostalCode = 8,
Country = 9,
Phone = 10,
Fax = 11
}
public virtual List<TestDynamicOrderByResults> Execute((Cols col, bool descending)[] orderBy)
{


OrderBy = orderBy;
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
var returnVal = Execute(conn).ToList();
;
return returnVal;
}
}

public virtual List<TestDynamicOrderByResults> Execute()
{
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
var returnVal = Execute(conn).ToList();
return returnVal;
}
}

public virtual IEnumerable<TestDynamicOrderByResults> Execute((Cols col, bool descending)[] orderBy,IDbConnection conn, IDbTransaction tx = null){


OrderBy = orderBy;
var returnVal = Execute(conn);

return returnVal;
}
public virtual IEnumerable<TestDynamicOrderByResults> Execute(IDbConnection conn, IDbTransaction tx = null){

((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(delegate (object sender, SqlInfoMessageEventArgs e) { AppendExececutionMessage(e.Message); });
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
public virtual TestDynamicOrderByResults GetOne((Cols col, bool descending)[] orderBy)
{


OrderBy = orderBy;
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
var returnVal = GetOne(conn);
return returnVal;
}
}public virtual TestDynamicOrderByResults  GetOne()
{
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
return GetOne(conn);
}
}
public virtual TestDynamicOrderByResults GetOne((Cols col, bool descending)[] orderBy,IDbConnection conn, IDbTransaction tx = null)
{


OrderBy = orderBy;
{
var returnVal = GetOne(conn);
return returnVal;
}
}public virtual TestDynamicOrderByResults GetOne(IDbConnection conn, IDbTransaction tx = null)
{
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(delegate (object sender, SqlInfoMessageEventArgs e) { AppendExececutionMessage(e.Message); });
{
var all = Execute(conn,tx);
TestDynamicOrderByResults returnVal;
using (IEnumerator<TestDynamicOrderByResults> iter = all.GetEnumerator())
{
iter.MoveNext();
returnVal = iter.Current;
}
return returnVal;
}
}
public virtual System.String ExecuteScalar((Cols col, bool descending)[] orderBy)
{


OrderBy = orderBy;
var returnVal = ExecuteScalar();
;
return returnVal;
}

public virtual System.String ExecuteScalar()
{
using (IDbConnection conn = QfRuntimeConnection.GetConnection())
{
conn.Open();
return ExecuteScalar(conn);
}
}

public virtual System.String ExecuteScalar((Cols col, bool descending)[] orderBy,IDbConnection conn, IDbTransaction tx = null)
{


OrderBy = orderBy;
var returnVal = ExecuteScalar(conn, tx);
;
return returnVal;
}

public virtual System.String ExecuteScalar(IDbConnection conn, IDbTransaction tx = null)
{
((SqlConnection)conn).InfoMessage += new SqlInfoMessageEventHandler(delegate (object sender, SqlInfoMessageEventArgs e) { AppendExececutionMessage(e.Message); });
using(IDbCommand cmd = conn.CreateCommand())
{
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

public virtual TestDynamicOrderByResults Create(IDataRecord record)
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

// vestige from user partial
//returnVal.OnLoad();
return returnVal;
}
TestDynamicOrderByResults CreatePoco(System.Data.IDataRecord record)
{
    return new TestDynamicOrderByResults();
}}
public partial class TestDynamicOrderByResults {
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
}
}
