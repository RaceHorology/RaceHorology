/*
 *  Copyright (C) 2019 - 2022 by Sven Flossmann
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
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public abstract class ALGETdC8001TimeMeasurementBase : ILiveTimeMeasurementDevice, ILiveDateTimeProvider, IImportTime
  {
    public event TimeMeasurementEventHandler TimeMeasurementReceived;
    public event StartnumberSelectedEventHandler StartnumberSelectedReceived;
    public event ImportTimeEntryEventHandler ImportTimeEntryReceived;

    ALGETdC8001LineParser _parser;
    protected string _statusText;

    TimeSpan _currentDayTimeDelta; // Contains the diff between ALGE TdC8001 and the local computer time

    public ALGETdC8001TimeMeasurementBase()
    {
      _statusText = "Not running";
      _parser = new ALGETdC8001LineParser();
    }


    public TimeSpan GetCurrentDayTime()
    {
      return (DateTime.Now - DateTime.Today) - _currentDayTimeDelta;
    }

    public virtual string GetDeviceInfo()
    {
      return "ALGE TdC 8000/8001 (base)";
    }

    public string GetStatusInfo()
    {
      return _statusText;
    }


    public abstract void Start();
    public abstract void Stop();

    public abstract bool IsOnline { get; }
    public abstract bool IsStarted { get; }
    public abstract event LiveTimingMeasurementDeviceStatusEventHandler StatusChanged;


    protected void processLine(string dataLine)
    {
      try
      {
        _parser.Parse(dataLine);
      }
      catch (Exception)
      {}

      if (_parser.TimingData == null)
        return;

      try
      {
        if (_parser.Mode == ALGETdC8001LineParser.EMode.LiveTiming)
        {
          if (_parser.TimingData != null)
          {
            if (_parser.TimingData.Flag == 's' || _parser.TimingData.Flag == 'n')
            {
              StartnumberSelectedEventArgs ssData = TransferToStartnumberSelectedData(_parser.TimingData);
              if (ssData != null)
              {
                // Trigger event
                var handle = StartnumberSelectedReceived;
                handle?.Invoke(this, ssData);

                // Send in addition a start event in case of parallel slalom (ALGE only sends a finish startnumber selected event)
                if (ssData.Color != EParticipantColor.NoColor && ssData.Channel == EMeasurementPoint.Finish)
                {
                  ssData.Channel = EMeasurementPoint.Start;
                  handle?.Invoke(this, ssData);
                }
              }
            }
            else
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
        }
        else if (_parser.Mode == ALGETdC8001LineParser.EMode.Classement)
        {
          ImportTimeEntry entry = new ImportTimeEntry(_parser.TimingData.StartNumber, _parser.TimingData.Time);
          var handle = ImportTimeEntryReceived;
          handle?.Invoke(this, entry);
        }
      }
      catch (FormatException)
      {}
    }

    public static TimeMeasurementEventArgs TransferToTimemeasurementData(in ALGETdC8001LiveTimingData parsedData)
    {
      TimeSpan? parsedDataTime = parsedData.Time;

      // Fill the data
      TimeMeasurementEventArgs data = new TimeMeasurementEventArgs();

      // Sort out invalid data
      if ( parsedData.Flag == 'p'
        || parsedData.Flag == 'b'
        || parsedData.Flag == 'm'
        || parsedData.Flag == 'n'
        || parsedData.Flag == 's')
        return null;

      if (parsedData.Flag == 'd'
        || parsedData.Flag == 'c')
        parsedDataTime = null;

      data.Valid = parsedData.Flag != '?';
      data.StartNumber = parsedData.StartNumber;

      switch (parsedData.Channel)
      {
        case "C0": // Standard 
        case "C3": // Parallel Slalom
          data.StartTime = parsedDataTime;
          data.BStartTime = true;
          break;

        case "C1": // Standard 
        case "C4": // Parallel Slalom
          data.FinishTime = parsedDataTime;
          data.BFinishTime = true;
          break;

        case "RT":
          data.RunTime = parsedDataTime;
          data.BRunTime = true;
          break;

        case "TT":   // TotalTime, calculated automatically
        case "DT":   // DifferenceTime, parallel slalom, not used
          return null;
      }

      return data;
    }

    public static StartnumberSelectedEventArgs TransferToStartnumberSelectedData(in ALGETdC8001LiveTimingData parsedData)
    {
      // Fill the data
      StartnumberSelectedEventArgs data = new StartnumberSelectedEventArgs();

      // Sort out invalid data
      if (parsedData.Flag != 'n' && parsedData.Flag != 's')
        return null;

      data.StartNumber = parsedData.StartNumber;

      if (parsedData.Flag == 'n')
        data.Channel = EMeasurementPoint.Finish;
      else if (parsedData.Flag == 's')
        data.Channel = EMeasurementPoint.Start;

      if (parsedData.StartNumberModifier == 'r')
        data.Color = EParticipantColor.Red;
      else if (parsedData.StartNumberModifier == 'b')
        data.Color = EParticipantColor.Blue;

      return data;
    }

    // Nothing to implement, download is initiated interactively on ALGE device via Classment transfer
    public void DownloadImportTimes(){}


    #region Implementation of ILiveDateTimeProvider
    public event LiveDateTimeChangedHandler LiveDateTimeChanged;

    protected void UpdateLiveDayTime(in ALGETdC8001LiveTimingData justReceivedData)
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
    #endregion
  }


  public class ALGETdC8001TimeMeasurement : ALGETdC8001TimeMeasurementBase, ILiveTimeMeasurementDeviceDebugInfo
  {
    enum EInternalStatus { Stopped, Initializing, NoCOMPort, Running };

    private string _serialPortName;
    private SerialPort _serialPort;
    private EInternalStatus _internalStatus;
    private string _dumpDir;
    System.IO.StreamWriter _dumpFile;
    private string _internalProtocol;

    System.Threading.Thread _instanceCaller;
    bool _stopRequest;

    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


    public ALGETdC8001TimeMeasurement(string comport, string dumpDir) : base()
    {
      _serialPortName = comport;
      _internalStatus = EInternalStatus.Stopped;
      _dumpDir = dumpDir;
      _internalProtocol = string.Empty;
    }

    public override string GetDeviceInfo()
    {
      return "ALGE TdC8000/8001 (" + _serialPortName + ")";
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
      string dumpFilename = String.Format(@"ALGETdC800x-{0}.dump", DateTime.Now.ToString("yyyyMMddHHmm"));
      dumpFilename = System.IO.Path.Combine(_dumpDir, dumpFilename);
      _dumpFile = new System.IO.StreamWriter(dumpFilename, true); // Appending, just in case the filename clashes
    }


    private void setInternalStatus(EInternalStatus value)
    {
      if (_internalStatus != value)
      {
        Logger.Info("new status: {0} (old: {1})", value, _internalStatus);

        _internalStatus = value;

        var handler = StatusChanged;
        handler?.Invoke(this, IsOnline);
      }
    }

    public override bool IsOnline { 
      get { return _serialPort != null && _internalStatus == EInternalStatus.Running; } 
    }

    public override bool IsStarted {
      get {
        return _serialPort != null && !_stopRequest;
      }
    }


    public override event LiveTimingMeasurementDeviceStatusEventHandler StatusChanged;


    private void MainLoop()
    {
      setInternalStatus(EInternalStatus.Initializing);

      if (_dumpDir != null)
        startWritingToDumpFile();

      _serialPort = new SerialPort(_serialPortName, 9600, Parity.None, 8, StopBits.One);
      _serialPort.NewLine = "\r"; // CR, ASCII(13)
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


  public class ALGETdC8001LiveTimingData
  {
    public ALGETdC8001LiveTimingData()
    {
      Flag = ' ';
      StartNumber = 0;
      StartNumberModifier = ' ';
      Channel = "";
      ChannelModifier = ' ';
      Time = new TimeSpan();
      Group = 0;
    }

    public char Flag { get; set; }
    public uint StartNumber { get; set; }
    public char StartNumberModifier { get; set; }
    public string Channel { get; set; }
    public char ChannelModifier { get; set; }
    public TimeSpan Time { get; set; }
    public uint Group { get; set; }
  }


  public class ALGETdC8001LineParser
  {
    public enum EMode
    {
      LiveTiming,
      Classement
    }


    public enum ELineType
    { 
      Unknown,
      TimeLine,
      ClassementStart,
      ClassementEnd
    }


    public ALGETdC8001LiveTimingData TimingData { get; private set;}
    public EMode Mode { get; private set; }


    public ALGETdC8001LineParser()
    {
      // Initial mode
      Mode = EMode.LiveTiming;
    }


    public void Parse(string dataLine)
    {
      // Clear data from previous run
      TimingData = null;

      // Figure out the type of line
      ELineType lineType = parseType(dataLine);

      if (lineType == ELineType.ClassementStart)
        Mode = EMode.Classement;

      if (Mode == EMode.Classement && lineType == ELineType.ClassementEnd)
        Mode = EMode.LiveTiming;

      ALGETdC8001LiveTimingData timingData = parseTimeLine(dataLine);
      if (timingData != null)
        TimingData = timingData;
    }

    private ELineType parseType(string dataLine)
    {
      if (dataLine.StartsWith("CLASSEMENT:"))
        return ELineType.ClassementStart;

      if (Mode == EMode.Classement && dataLine.StartsWith("  ALGE TIMING"))
        return ELineType.ClassementEnd;

      // Just try
      ALGETdC8001LiveTimingData timingData = parseTimeLine(dataLine);
      if (timingData != null)
        return ELineType.TimeLine;

      return ELineType.Unknown;
    }


    private ALGETdC8001LiveTimingData parseTimeLine(string dataLine)
    {
      if (dataLine.Length < 5)
        return null;

      ALGETdC8001LiveTimingData parsedData = new ALGETdC8001LiveTimingData();
      parsedData.Flag = dataLine[0];

      try
      {
        parsedData.StartNumber = UInt32.Parse(dataLine.Substring(1, 4));
      }
      catch (FormatException)
      {
        return null;
      }

      if (dataLine.Length > 5)
      {
        parsedData.StartNumberModifier = dataLine[5];
      }

      if (dataLine.Length > 5+1) // +1 because of parallel slalow => identifier for 'b' (blue course) or 'r' (red course)
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
        }
        parsedData.Group = UInt32.Parse(dataLine.Substring(24, 2));
      }

      return parsedData;
    }
  }

}
