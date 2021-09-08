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
        }
    }

}
