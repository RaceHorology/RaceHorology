using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{
  public class ALGETdC8001TimeMeasurement : ILiveTimeMeasurement
  {
    public event TimeMeasurementEventHandler TimeMeasurementReceived;

    public delegate void RawMessageReceivedEventHandler(object sender, string message);
    public event RawMessageReceivedEventHandler RawMessageReceived;


    private SerialPort _serialPort;
    System.IO.StreamWriter _dumpFile;

    public ALGETdC8001TimeMeasurement(string comport)
    {
      string dumpFilename = String.Format(@"ALGETdC8001-{0}.dump", DateTime.Now.ToString("yyyyMMddHHmm"));
      _dumpFile = new System.IO.StreamWriter(dumpFilename);

      _serialPort = new SerialPort(comport, 9600, Parity.None, 8, StopBits.One);
      _serialPort.NewLine = "\r"; // CR, ASCII(13)
      _serialPort.Handshake = Handshake.RequestToSend;
      _serialPort.Open();

      // Start processing in a separate Thread
      System.Threading.Thread InstanceCaller = new System.Threading.Thread(
          new System.Threading.ThreadStart(this.MainLoop));
      InstanceCaller.Start();
    }

    private void MainLoop()
    {
      ALGETdC8001LineParser parser = new ALGETdC8001LineParser();

      while (true)
      {
        string dataLine = _serialPort.ReadLine();
        DebugLine(dataLine);

        ALGETdC8001LiveTimingData parsedData = null;
        try
        {
          parsedData = parser.Parse(dataLine);
        }
        catch(FormatException)
        { continue; }

        try
        {
          TimeMeasurementEventArgs timeMeasurmentData = TransferToTimemeasurementData(parsedData);

          if (timeMeasurmentData!=null)
          {
            // Trigger event
            var handle = TimeMeasurementReceived;
            handle?.Invoke(this, timeMeasurmentData);
          }
        }
        catch (FormatException)
        { continue; }
      }
    }


    void DebugLine(string dataLine)
    {
      _dumpFile?.WriteLine(dataLine);
      _dumpFile?.Flush();

      RawMessageReceivedEventHandler handler = RawMessageReceived;
      handler?.Invoke(this, dataLine);
    }


    public static TimeMeasurementEventArgs TransferToTimemeasurementData(in ALGETdC8001LiveTimingData parsedData)
    {
      TimeSpan? parsedDataTime = parsedData.Time;

      // Fill the data
      TimeMeasurementEventArgs data = new TimeMeasurementEventArgs();

      // Sort out invalid data
      if ( parsedData.Flag == 'p' 
        || parsedData.Flag == '?' 
        || parsedData.Flag == 'b' 
        || parsedData.Flag == 'm'
        || parsedData.Flag == 'n')
        return null;

      if (parsedData.Flag == 'd'
        || parsedData.Flag == 'c')
        parsedDataTime = null;

      data.StartNumber = parsedData.StartNumber;

      switch (parsedData.Channel)
      {
        case "C0":
          data.StartTime = parsedDataTime;
          data.BStartTime = true;
          break;

        case "C1":
          data.FinishTime = parsedDataTime;
          data.BFinishTime = true;
          break;

        case "RT":   // RunTime, calculated automatically
        case "TT":   // TotalTime, calculated automatically
          return null;
      }

      return data;
    }
  }



  public class ALGETdC8001LiveTimingData
  {
    public ALGETdC8001LiveTimingData()
    {
      Flag = ' ';
      StartNumber = 0;
      Channel = "";
      ChannelModifier = ' ';
      Time = new TimeSpan();
      Group = 0;
    }

    public char Flag { get; set; }
    public uint StartNumber { get; set; }
    public string Channel { get; set; }
    public char ChannelModifier { get; set; }
    public TimeSpan Time { get; set; }
    public uint Group { get; set; }
  }


  public class ALGETdC8001LineParser
  {
    public ALGETdC8001LineParser()
    { }

    public ALGETdC8001LiveTimingData Parse(string dataLine)
    {
      ALGETdC8001LiveTimingData parsedData = new ALGETdC8001LiveTimingData();

      parsedData.Flag = dataLine[0];
      parsedData.StartNumber = UInt32.Parse(dataLine.Substring(1, 4));

      if (dataLine.Length > 5)
      {
        parsedData.Channel = dataLine.Substring(6, 2);
        parsedData.ChannelModifier = dataLine[8];
        string timeStr = dataLine.Substring(10, 13);
        try
        {
          string[] formats = { @"hh\:mm\:ss\.ffff", @"hh\:mm\:ss\.fff", @"hh\:mm\:ss\.ff", @"hh\:mm\:ss\.f" };
          timeStr = timeStr.TrimEnd(' ');
          parsedData.Time = TimeSpan.ParseExact(timeStr, formats, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
          throw;
        }
        parsedData.Group = UInt32.Parse(dataLine.Substring(24, 2));
      }

      return parsedData;
    }
  }

}
