/*
 *  Copyright (C) 2019 - 2026 by Sven Flossmann & Co-Authors (CREDITS.TXT)
 *
 *  This file is part of Race Horology.
 *
 *  Race Horology is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  any later version.
 *
 *  Race Horology is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with Race Horology.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  Diese Datei ist Teil von Race Horology.
 *
 *  Race Horology ist Freie Software: Sie können es unter den Bedingungen
 *  der GNU Affero General Public License, wie von der Free Software Foundation,
 *  Version 3 der Lizenz oder (nach Ihrer Wahl) jeder neueren
 *  veröffentlichten Version, weiter verteilen und/oder modifizieren.
 *
 *  Race Horology wird in der Hoffnung, dass es nützlich sein wird, aber
 *  OHNE JEDE GEWÄHRLEISTUNG, bereitgestellt; sogar ohne die implizite
 *  Gewährleistung der MARKTFÄHIGKEIT oder EIGNUNG FÜR EINEN BESTIMMTEN ZWECK.
 *  Siehe die GNU Affero General Public License für weitere Details.
 *
 *  Sie sollten eine Kopie der GNU Affero General Public License zusammen mit diesem
 *  Programm erhalten haben. Wenn nicht, siehe <https://www.gnu.org/licenses/>.
 *
 */

using System;
using System.IO;
using System.IO.Ports;
using WebSocketSharp;

namespace RaceHorologyLib
{
  public abstract class MicrogateV1TimeMeasurementBase : ILiveTimeMeasurementDevice, ILiveDateTimeProvider, IImportTime
  {
    public event TimeMeasurementEventHandler TimeMeasurementReceived;
    public event StartnumberSelectedEventHandler StartnumberSelectedReceived;
    public event ImportTimeEntryEventHandler ImportTimeEntryReceived;

    MicrogateV1LineParser _parser;
    protected string _statusText;
    protected DeviceInfo _deviceInfo = new DeviceInfo
    {
      Manufacturer = "Microgate",
      Model = "Racetime 2",
      PrettyName = "Microgate Racetime 2",
      SerialNumber = string.Empty
    };

    TimeSpan _currentDayTimeDelta; // Contains the diff between ALGE TdC8001 and the local computer time

    public MicrogateV1TimeMeasurementBase()
    {
      _statusText = "Not running";
      _parser = new MicrogateV1LineParser();
    }


    public TimeSpan GetCurrentDayTime()
    {
      return (DateTime.Now - DateTime.Today) - _currentDayTimeDelta;
    }

    public virtual DeviceInfo GetDeviceInfo()
    {
      return _deviceInfo;
    }

    public virtual string GetStatusInfo()
    {
      return _statusText;
    }


    public abstract void Start();
    public abstract void Stop();

    protected StatusType _onlineStatus;
    public StatusType OnlineStatus
    {
      protected set
      {
        if (_onlineStatus != value)
        {
          _onlineStatus = value;
          var handler = StatusChanged;
          handler?.Invoke(this, OnlineStatus);
        }
      }
      get
      {
        return _onlineStatus;
      }
    }
    public event LiveTimingMeasurementDeviceStatusEventHandler StatusChanged;

    public abstract bool IsStarted { get; }

    protected void processLine(string dataLine)
    {
      try
      {
        _parser.Parse(dataLine);
      }
      catch (Exception)
      { }

      if (_parser.TimingData == null)
        return;

      try
      {
        if (_parser.Mode == MicrogateV1LineParser.EMode.LiveTiming)
        {
          if (_parser.TimingData != null)
          {
            UpdateLiveDayTime(_parser.TimingData);

            TimeMeasurementEventArgs timeMeasurmentData = TransferToTimemeasurementData(_parser.TimingData);
            if (timeMeasurmentData != null)
            {
              // Trigger event
              var handle = TimeMeasurementReceived;
              handle?.Invoke(this, timeMeasurmentData);
            }
          }
        }
        else if (_parser.Mode == MicrogateV1LineParser.EMode.Classement)
        {
          ImportTimeEntry entry = new ImportTimeEntry(_parser.TimingData.StartNumber, _parser.TimingData.Time);
          var handle = ImportTimeEntryReceived;
          handle?.Invoke(this, entry);
        }
      }
      catch (FormatException)
      { }
    }

    public static TimeMeasurementEventArgs TransferToTimemeasurementData(in MicrogateV1LiveTimingData parsedData)
    {
      TimeSpan? parsedDataTime = parsedData.Time;

      // Fill the data
      TimeMeasurementEventArgs data = new TimeMeasurementEventArgs();


      // Sort out invalid data
      if (parsedData.Flag != '0' && parsedData.Flag != 'a' && parsedData.Flag != 'A' && parsedData.Flag != 'Q' && parsedData.Flag != 'P')
        return null;

      data.Valid = parsedData.Flag == '0' || parsedData.Flag == 'A' || parsedData.Flag == 'Q' || parsedData.Flag == 'P' || parsedData.Flag == 'a';
      data.StartNumber = parsedData.StartNumber;

      switch (parsedData.LogicalChannel)
      {
        case "000": // Standard
          if (parsedData.Flag == '0')
          {
            data.StartTime = parsedDataTime;
          }
          else
          {
            data.StartTime = null;
          }
          data.BStartTime = true;
          break;

        case "255": // Standard
          if (parsedData.Flag == '0')
          {
            data.FinishTime = parsedDataTime;
          }
          else
          {
            data.FinishTime = null;
          }
          data.BFinishTime = true;
          break;

        default:
          return null;
      }

      switch (parsedData.Flag)
      {
        case 'P': data.DisqualificationCode = 1; break;
        case 'A': data.DisqualificationCode = 2; break;
        case 'Q': data.DisqualificationCode = 3; break;
        case '0': // intentional fallthrough
        default:
          data.DisqualificationCode = 0;
          break;
      }


      return data;
    }

    // Nothing to implement, download is initiated interactively on ALGE device via Classment transfer
    public EImportTimeFlags SupportedImportTimeFlags() { return EImportTimeFlags.RunTime; }
    public void DownloadImportTimes() { }


    #region Implementation of ILiveDateTimeProvider
    public event LiveDateTimeChangedHandler LiveDateTimeChanged;

    protected void UpdateLiveDayTime(in MicrogateV1LiveTimingData justReceivedData)
    {
      // Sort out invalid data
      if (justReceivedData.Flag != '0')
        return;

      TimeSpan tDiff = (DateTime.Now - DateTime.Today) - justReceivedData.Time;
      _currentDayTimeDelta = tDiff;

      var handler = LiveDateTimeChanged;
      handler?.Invoke(this, new LiveDateTimeEventArgs(justReceivedData.Time));

    }
    #endregion
  }


  public class MicrogateV1TimeMeasurement : MicrogateV1TimeMeasurementBase, ILiveTimeMeasurementDeviceDebugInfo
  {
    enum EInternalStatus { Stopped, Initializing, NoCOMPort, Running };

    private string _serialPortName;
    private int _comBitRate;
    private SerialPort _serialPort;
    private EInternalStatus _internalStatus;
    private string _dumpDir;
    System.IO.StreamWriter _dumpFile;
    private string _internalProtocol;

    System.Threading.Thread _instanceCaller;
    bool _stopRequest;

    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


    public MicrogateV1TimeMeasurement(string comport, int comBitRate, string dumpDir) : base()
    {
      _serialPortName = comport;
      _comBitRate = comBitRate;
      _stopRequest = true;
      _internalStatus = EInternalStatus.Stopped;
      _dumpDir = dumpDir;
      _internalProtocol = string.Empty;
    }

    public override string GetStatusInfo()
    {
      if (_serialPortName.IsNullOrEmpty())
        return "kein COM Port";

      return _serialPortName + ", " + _statusText;
    }

    public override void Start()
    {
      if (string.IsNullOrEmpty(_serialPortName))
        return;

      Logger.Info("Start()");

      _statusText = "Starting";

      _stopRequest = false;

      // Start processing in a separate Thread
      _instanceCaller = new System.Threading.Thread(
          new System.Threading.ThreadStart(this.MainLoop));
      _instanceCaller.Start();
    }

    public override void Stop()
    {
      Logger.Info("Stop()");

      if (_instanceCaller != null)
      {
        _statusText = "Stopping";

        _stopRequest = true;
        _instanceCaller = null;
      }
    }


    private void startWritingToDumpFile()
    {
      string dumpFilename = String.Format(@"MicrogateV1-{0}.dump", DateTime.Now.ToString("yyyyMMddHHmm"));
      dumpFilename = System.IO.Path.Combine(_dumpDir, dumpFilename);
      _dumpFile = new System.IO.StreamWriter(dumpFilename, true); // Appending, just in case the filename clashes
    }

    private void setInternalStatus(EInternalStatus value)
    {
      if (_internalStatus != value)
      {
        Logger.Info("new status: {0} (old: {1})", value, _internalStatus);

        _internalStatus = value;

        var onlineStatus = mapInternalToOnlineStatus(_onlineStatus, value);
        if (onlineStatus != null)
          OnlineStatus = (StatusType)onlineStatus;
      }
    }

    private StatusType? mapInternalToOnlineStatus(StatusType oldStatus, EInternalStatus newInternalStatus)
    {
      var wasConnected = !_stopRequest && oldStatus == StatusType.Online || oldStatus == StatusType.Error_GotOffline;
      switch (newInternalStatus)
      {
        case EInternalStatus.Initializing: return wasConnected ? StatusType.Error_GotOffline : StatusType.Offline;
        case EInternalStatus.Running: return StatusType.Online;
        case EInternalStatus.Stopped: return wasConnected ? StatusType.Error_GotOffline : StatusType.Offline;
        case EInternalStatus.NoCOMPort: return wasConnected ? StatusType.Error_GotOffline : StatusType.NoDevice;
        default: return null;
      }
    }


    public override bool IsStarted
    {
      get
      {
        return _serialPort != null && !_stopRequest;
      }
    }


    private void MainLoop()
    {
      setInternalStatus(EInternalStatus.Initializing);

      if (_dumpDir != null)
        startWritingToDumpFile();

      _serialPort = new SerialPort(_serialPortName, _comBitRate, Parity.None, 8, StopBits.One);
      _serialPort.NewLine = "\r"; // LF, ASCII(10)
      _serialPort.Handshake = Handshake.RequestToSend;
      _serialPort.ReadTimeout = 1000;

      while (!_stopRequest)
      {
        if (!EnsureOpenPort())
        {
          _statusText = "Serial port not available";
          setInternalStatus(EInternalStatus.NoCOMPort);

          System.Threading.Thread.Sleep(2000);
          continue;
        }

        try
        {
          _statusText = "Running";
          setInternalStatus(EInternalStatus.Running);

          string dataLine = _serialPort.ReadLine();
          Logger.Info("data received: {0}", dataLine);

          debugLine(dataLine);
          processLine(dataLine);
        }
        catch (TimeoutException)
        { continue; }
        catch (System.IO.IOException)
        { }
      }

      Logger.Info("closing serial port");
      _serialPort.Close();
      _serialPort.Dispose();
      _serialPort = null;

      if (_dumpFile != null)
      {
        _dumpFile.Close();
        _dumpFile.Dispose();
        _dumpFile = null;
      }

      _statusText = "Stopped";
      setInternalStatus(EInternalStatus.Stopped);
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
        catch (Exception)
        {
          return false;
        }
      }
      return true;
    }


    #region Implementation of ILiveTimeMeasurementDeviceDebugInfo
    public event RawMessageReceivedEventHandler RawMessageReceived;

    public string GetProtocol()
    {
      return _internalProtocol;
    }

    void debugLine(string dataLine)
    {
      _dumpFile?.WriteLine(dataLine);
      _dumpFile?.Flush();

      if (!string.IsNullOrEmpty(_internalProtocol))
        _internalProtocol += "\n";
      _internalProtocol += dataLine;

      RawMessageReceivedEventHandler handler = RawMessageReceived;
      handler?.Invoke(this, dataLine);
    }
    #endregion

  }


  public class MicrogateV1LiveTimingData
  {
    public MicrogateV1LiveTimingData()
    {
      Flag = ' ';
      StartNumber = 0;
      StartNumberModifier = ' ';
      LogicalChannel = "";
      LogicalChannelModifier = ' ';
      Time = new TimeSpan();
    }

    public char Flag { get; set; }
    public uint StartNumber { get; set; }
    public char StartNumberModifier { get; set; }
    public string LogicalChannel { get; set; }
    public char LogicalChannelModifier { get; set; }
    public TimeSpan Time { get; set; }
  }


  public class MicrogateV1LineParser
  {
    public enum EMode
    {
      LiveTiming,
      Classement
    }


    public enum ELineType
    {
      Unknown,
      OnlineTimeLine,
      ClassementStart,
      ClassementTimeLine,
      ClassementEnd
    }


    public MicrogateV1LiveTimingData TimingData { get; private set; }
    public EMode Mode { get; private set; }


    public MicrogateV1LineParser()
    {
      // Initial mode
      Mode = EMode.LiveTiming;
    }


    public void Parse(string dataLine)
    {
      // Clear data from previous run
      TimingData = null;
      MicrogateV1LiveTimingData timingData;

      // Figure out the type of line
      ELineType lineType = parseType(dataLine);

      switch (lineType)
      {
        case ELineType.OnlineTimeLine: timingData = parseOnlineTimeLine(dataLine); break;
        case ELineType.ClassementTimeLine: timingData = parseClassementTimeLine(dataLine); break;
        default: timingData = null; break;
      }

      if (timingData != null)
        TimingData = timingData;
    }

    private ELineType parseType(string dataLine)
    {
      if (dataLine.Length == 23 - 1) // respect missing carriage return
        return ELineType.OnlineTimeLine;

      if (dataLine.Length == 30 - 1) // respect missing carriage return
        return ELineType.ClassementTimeLine;

      switch ((int)dataLine[0])
      {
        case 2: return ELineType.ClassementStart;
        case 3: return ELineType.ClassementEnd;
        default: return ELineType.Unknown;
      }
    }


    private MicrogateV1LiveTimingData parseOnlineTimeLine(string dataLine)
    {
      MicrogateV1LiveTimingData parsedData = new MicrogateV1LiveTimingData();

      try
      {
        parsedData.Flag = dataLine[3];
        parsedData.StartNumber = UInt32.Parse(dataLine.Substring(0, 3));
        parsedData.LogicalChannel = dataLine.Substring(5, 3);
        string timeStr = dataLine.Substring(8, 12);
        string[] formats = { @"hh\:mm\:ss\.fff" };
        parsedData.Time = TimeSpan.ParseExact(timeStr, formats, System.Globalization.CultureInfo.InvariantCulture);
      }
      catch (FormatException)
      {
        return null;
      }

      return parsedData;
    }


    private MicrogateV1LiveTimingData parseClassementTimeLine(string dataLine)
    {
      MicrogateV1LiveTimingData parsedData = new MicrogateV1LiveTimingData();

      try
      {
        parsedData.Flag = dataLine[17];
        parsedData.StartNumber = UInt32.Parse(dataLine.Substring(4, 4));
        parsedData.LogicalChannel = dataLine.Substring(14, 3);
        string timeStr = dataLine.Substring(20, 9);
        string[] formats = { @"hhmmssfff" };
        parsedData.Time = TimeSpan.ParseExact(timeStr, formats, System.Globalization.CultureInfo.InvariantCulture);
      }
      catch (FormatException)
      {
        return null;
      }

      return parsedData;
    }
  }

}
