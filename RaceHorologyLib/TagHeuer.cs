using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public class TagHeuer : IHandTiming
  {

    TagHeuerParser _parser;
    private SerialPort _serialPort;
    private string _serialPortName;

    private DateTime _synchroTime;

    public TagHeuer(string serialPortName)
    {
      _parser = new TagHeuerParser();
      _serialPortName = serialPortName;

    }


    public void Connect()
    {
      _serialPort = new SerialPort(_serialPortName, 38400, Parity.None, 8, StopBits.One);
      _serialPort.NewLine = "\r"; // CR, ASCII(13)
      _serialPort.Handshake = Handshake.RequestToSend;
      _serialPort.ReadTimeout = 1000;
      _serialPort.Open();

      performHandshake();
    }

    protected void performHandshake()
    {
      _serialPort.WriteLine("#SN"); // Try to get serial number, 
      string dataLine = _parser.TrimNewEndLine(_serialPort.ReadLine()); 

    }


    public void StartGetTimingData()
    {
      _serialPort.WriteLine("#!T"); // Get Sync Time (needs to be added to timevalues)
      string dataLine = _serialPort.ReadLine(); // "!T 08:14:00 01/03/20"
      DateTime syncTime = _parser.ParseSynchroTime(dataLine);
      _synchroTime = syncTime;

      _serialPort.WriteLine("#WC 012");
      dataLine = _parser.TrimNewEndLine(_serialPort.ReadLine());
      if (!dataLine.StartsWith("DS "))
        throw new Exception("wrong answer");
    }

    public IEnumerable<TimingData> TimingData()
    {
      do
      {
        TagHeuerData parsedData = null;
        try
        {
          string dataLine = _parser.TrimNewEndLine(_serialPort.ReadLine());

          if (dataLine.StartsWith("DE ")) // until "DE NN"
            break;

          parsedData = _parser.ParseRR(dataLine);
        }
        catch (TimeoutException)
        {
          break; // no new data
        }
        catch (Exception)
        { }

        TimingData td = new TimingData
        {
          Time = (_synchroTime + parsedData.Time).TimeOfDay
        };

        yield return td;

      } while (true);
    }
  }


  public class TagHeuerData
  {
    public int Rank { get; set; }
    public int Number { get; set; }
    public TimeSpan Time { get; set; }
  }


  public class TagHeuerParser
  {

    public string TrimNewEndLine(string line)
    {
      return line.TrimStart('\n').TrimEnd('\t');
    }


    // "!T 08:14:00 01/03/20"
    public DateTime ParseSynchroTime(string line)
    {
      line = TrimNewEndLine(line);

      DateTime parsedDate;

      try
      {
        string[] formats = { @"HH\:mm\:ss dd/MM/yy"};
        string timeStr = line.Substring(3);
        parsedDate = DateTime.ParseExact(timeStr, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
      }
      catch (FormatException)
      {
        throw;
      }

      return parsedDate;
    }


    /* Data Looks like this
     * DS 01 245 TIME
     * ...
     * RR 0000 0232   05:27:51.01040
     * ...
     * RR 0002 9999   06:06:01.35403
     * DE 01
     */
    public TagHeuerData ParseRR(string line)
    {
      line = TrimNewEndLine(line);

      TagHeuerData parsedData = null;

      string prefix = line.Substring(0, 2);
      if (prefix == "RR")
      {
        string rank = line.Substring(3, 4);
        string number = line.Substring(8, 4);
        string timeStr = line.Substring(15);
        try
        {
          int nRank = int.Parse(rank);
          int nNumber = int.Parse(number);
          string[] formats = { @"hh\:mm\:ss\.fffff", @"hh\:mm\:ss\.ffff", @"hh\:mm\:ss\.fff", @"hh\:mm\:ss\.ff", @"hh\:mm\:ss\.f" };
          timeStr = timeStr.TrimEnd(' ');
          TimeSpan time = TimeSpan.ParseExact(timeStr, formats, System.Globalization.CultureInfo.InvariantCulture);

          parsedData = new TagHeuerData { Number = nNumber, Rank = nRank, Time = time };
        }
        catch (FormatException)
        {
          throw;
        }
      }
      return parsedData;
    }

  }
}
