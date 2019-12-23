using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLibTest
{
  static class TestUtilities
  {
    public static string CreateWorkingFileFrom(string srcDirectory, string srcFilename)
    {
      string srcPath = Path.Combine(srcDirectory, srcFilename);

      string dstDirectory = Path.Combine(srcDirectory, Path.GetRandomFileName());
      Directory.CreateDirectory(dstDirectory);

      string dstPath = Path.Combine(dstDirectory, srcFilename);
      File.Copy(srcPath, dstPath);

      var additionalFiles = Directory.GetFiles(srcDirectory, Path.GetFileNameWithoutExtension(srcFilename) + "*");
      foreach(var f in additionalFiles)
      {
        if (f == srcPath)
          continue;

        string dstF = Path.Combine(dstDirectory, Path.GetFileName(f));
        File.Copy(f, dstF);
      }

      return dstPath;
    }
  }
}
