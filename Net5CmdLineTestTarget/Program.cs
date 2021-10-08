using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Net5CmdLineTestTarget
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var result = new GetCustomers().Execute();
            result.ForEach(l => Console.WriteLine($"{l.ContactName} {l.City}"));

            // I'm not reproducing the elcia issue here. Need to try this at work.
            var TestMessagesWithMicrosoftDataSqlClient = new MicrosoftDataSqlClient_ReturnInfoMessage();
            using( var conn = new Microsoft.Data.SqlClient.SqlConnection("Server = localhost\\SQLEXPRESS; Database = NORTHWND; Trusted_Connection = True; "))
            {
                conn.Open();
                var msgResult = TestMessagesWithMicrosoftDataSqlClient.ExecuteNonQuery(conn);
            }

            // Test Dynamic OrderBy
            var query = new TestDynamicOrderBy();
            var sorted = query.Execute(new[] { (TestDynamicOrderBy.Cols.ContactName, false) });
            sorted.ForEach(l => Console.WriteLine($"{l.ContactName} {l.City}"));

        }
    }

}
