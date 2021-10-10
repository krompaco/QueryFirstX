using System.Data.SqlClient;
using System.Data;

namespace Net5CmdLineTestTarget
{    class QfRuntimeConnection
    {
        public static IDbConnection GetConnection()
        {
            return new SqlConnection("Server=localhost\\SQLEXPRESS;Database=NORTHWND;Trusted_Connection=True;");
        }
    }
}

