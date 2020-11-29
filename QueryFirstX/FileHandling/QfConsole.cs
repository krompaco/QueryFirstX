using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst
{
    public class QfConsole
    {
        public static IQfConsole Fake { get; set; }
        public static void WriteLine(string line)
        {
            if (Fake is not null)
            {
                Fake.WriteLine(line);
            }
            else Console.WriteLine(line);
        }
    }
    public interface IQfConsole
    {
        void WriteLine(string line);
    }
}
