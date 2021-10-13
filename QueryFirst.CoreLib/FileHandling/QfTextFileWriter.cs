using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst
{
    public class QfTextFileWriter : IQfTextFileWriter
    {
        public void WriteFile(QfTextFile fileToWrite)
        {
            File.WriteAllText(fileToWrite.Filename, fileToWrite.FileContents);
        }
    }
}
