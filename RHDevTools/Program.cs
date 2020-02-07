using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RHDevTools
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args[0] == "-a" && args.Length == 2)
        DSVAlpinDBTools.AnonymizeDB(args[1]);
      else
        printUsage();
    }

    static void printUsage()
    {
      Console.WriteLine("-a <DBPath>     Anonymize Database");
    }
  }
}
