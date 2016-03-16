using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interpritator
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader file = new StreamReader(args[0]);
            string performingCode = file.ReadToEnd();
        }
    }
}
