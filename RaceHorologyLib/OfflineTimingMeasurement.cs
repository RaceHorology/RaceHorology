using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public class TimingData
  {
    public TimeSpan? Time { get; set; }
  }


  interface IHandTiming
  {
    IEnumerable<TimingData> TimingData();
  }


  public class FromFileHandTiming : IHandTiming
  {
    string _filePath;
    StreamReader _file;
    FromFileParser _parser;

    public FromFileHandTiming(string path)
    {
      _filePath = path;

      _parser = new FromFileParser();
    }

    public void Connect()
    {
      _file = new System.IO.StreamReader(_filePath);
    }

    public void Disconnect()
    {
      _file.Close();
    }


    public IEnumerable<TimingData> TimingData()
    {
      string line;
      while ((line = _file.ReadLine()) != null)
      {
        var time = _parser.ParseTime(line);

        yield return new TimingData { Time = time };
      }
    }


  }



  public class FromFileParser
  {

    // Format: 09:37:16,40
    public TimeSpan ParseTime(string line)
    {
      TimeSpan parsedTime;

      try
      {
        string[] formats = 
        {
          @"hh\:mm\:ss\.fffff", @"hh\:mm\:ss\.ffff", @"hh\:mm\:ss\.fff", @"hh\:mm\:ss\.ff", @"hh\:mm\:ss\.f",
          @"hh\:mm\:ss\,fffff", @"hh\:mm\:ss\,ffff", @"hh\:mm\:ss\,fff", @"hh\:mm\:ss\,ff", @"hh\:mm\:ss\,f"
        };

        parsedTime = TimeSpan.ParseExact(line, formats, System.Globalization.CultureInfo.InvariantCulture);
      }
      catch (FormatException)
      {
        throw;
      }

      return parsedTime;
    }
  }


}
