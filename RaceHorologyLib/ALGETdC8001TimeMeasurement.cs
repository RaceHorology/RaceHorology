using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public class ALGETdC8001TimeMeasurement : ILiveTimeMeasurement, ILiveDateTimeProvider
  {
    public event TimeMeasurementEventHandler TimeMeasurementReceived;

    public delegate void RawMessageReceivedEventHandler(object sender, string message);
    public event RawMessageReceivedEventHandler RawMessageReceived;

    private string _serialPortName;
    private SerialPort _serialPort;
    System.IO.StreamWriter _dumpFile;

    System.Threading.Thread _instanceCaller;
    bool _stopRequest;

    string _statusText;

    TimeSpan _currentDayTimeDelta; // Contains the diff between ALGE TdC8001 and the local computer time

    public ALGETdC8001TimeMeasurement(string comport)
    {
      _serialPortName = comport;
      _statusText = "Not running";
    }


    public TimeSpan GetCurrentDayTime()
    {
      return (DateTime.Now - DateTime.Today) - _currentDayTimeDelta;
    }

    public string GetInfo()
    {
      return "ALGE TdC 8001 (" + _serialPortName + ")";
    }


    public string GetStatusInfo()
    {
      return _statusText;
    }


    public void Start()
    {
      if (string.IsNullOrEmpty(_serialPortName))
        return;

      _statusText = "Starting";

      _stopRequest = false;

      string dumpFilename = String.Format(@"ALGETdC8001-{0}.dump", DateTime.Now.ToString("yyyyMMddHHmm"));
      _dumpFile = new System.IO.StreamWriter(dumpFilename, true); // Appending, just in case the filename clashes

      _serialPort = new SerialPort(_serialPortName, 9600, Parity.None, 8, StopBits.One);
      _serialPort.NewLine = "\r"; // CR, ASCII(13)
      _serialPort.Handshake = Handshake.RequestToSend;
      _serialPort.ReadTimeout = 1000;

      // Start processing in a separate Thread
      _instanceCaller = new System.Threading.Thread(
          new System.Threading.ThreadStart(this.MainLoop));
      _instanceCaller.Start();
    }

    public void Stop()
    {
      if (_instanceCaller != null)
      {
        _statusText = "Stopping";

        _stopRequest = true;
        _instanceCaller.Join(); // Wait until thread has been terminated

        _dumpFile.Close();
      }
    }

    private void MainLoop()
    {
      ALGETdC8001LineParser parser = new ALGETdC8001LineParser();

      while (!_stopRequest)
      {
        if (!EnsureOpenPort())
        {
          _statusText = "Serial port not available";

          System.Threading.Thread.Sleep(2000);
          continue;
        }

        try
        {
          _statusText = "Running";

          string dataLine = _serialPort.ReadLine();
          DebugLine(dataLine);

          ALGETdC8001LiveTimingData parsedData = null;
          try
          {
            parsedData = parser.Parse(dataLine);
          }
          catch (FormatException)
          { continue; }

          try
          {
            UpdateLiveDayTime(parsedData);

            TimeMeasurementEventArgs timeMeasurmentData = TransferToTimemeasurementData(parsedData);
            if (timeMeasurmentData != null)
            {
              // Trigger event
              var handle = TimeMeasurementReceived;
              handle?.Invoke(this, timeMeasurmentData);
            }
          }
          catch (FormatException)
          { continue; }
        }
        catch (TimeoutException)
        { continue; }
      }

      _serialPort.Close();

      _statusText = "Stopped";
    }


    bool EnsureOpenPort()
    {
      if (!_serialPort.IsOpen)
      {
        try
        {
          _serialPort.Open();
        }
        catch (ArgumentException)
        {
          return false;
        }
        catch (IOException)
        {
          return false;
        }
        catch (InvalidOperationException)
        {
          return false;
        }
      }
      return true;
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

        case "RT":   
          data.RunTime = parsedDataTime;
          data.BRunTime = true;
          break;

        case "TT":   // TotalTime, calculated automatically
          return null;
      }

      return data;
    }



    #region Implementation of ILiveDateTimeProvider
    public event LiveDateTimeChangedHandler LiveDateTimeChanged;

    void UpdateLiveDayTime(in ALGETdC8001LiveTimingData justReceivedData)
    {
      // Sort out invalid data
      if (justReceivedData.Flag == 'p'
        || justReceivedData.Flag == 'm'
        || justReceivedData.Flag == 'n'
        || justReceivedData.Flag == 'd'
        || justReceivedData.Flag == 'c')
        return;

      if (!(justReceivedData.Flag == ' ' || justReceivedData.Flag == '?'))
        return;

      if (justReceivedData.Channel[0] != 'C')
        return;

      TimeSpan tDiff = (DateTime.Now - DateTime.Today) - justReceivedData.Time;
      _currentDayTimeDelta = tDiff;

      var handler = LiveDateTimeChanged;
      handler?.Invoke(this, new LiveDateTimeEventArgs(justReceivedData.Time));

    }
  }

  #endregion


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
