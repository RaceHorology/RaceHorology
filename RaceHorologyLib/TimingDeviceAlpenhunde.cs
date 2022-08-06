using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace RaceHorologyLib
{
  enum EStatus { NotConnected, Connecting, Connected };
  public class TimingDeviceAlpenhunde : ILiveTimeMeasurementDevice, ILiveDateTimeProvider
  {
    private string _baseUrl;
    private EStatus _status;

    private WebSocket _webSocket;
    private AlpenhundeParser _parser;

    public event TimeMeasurementEventHandler TimeMeasurementReceived;
    public event StartnumberSelectedEventHandler StartnumberSelectedReceived;
    public event LiveTimingMeasurementDeviceStatusEventHandler StatusChanged;

    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public TimingDeviceAlpenhunde(string baseUrl)
    {
      _baseUrl = baseUrl;
      _parser = new AlpenhundeParser();
    }


    private void setInternalStatus(EStatus status)
    {
      if (_status != status)
      {
        this._status = status;
        var handler = StatusChanged;
        handler?.Invoke(this, IsOnline);
      }
    }

    public bool IsOnline
    {
      get { return _status == EStatus.Connected; }
    }

    public TimeSpan GetCurrentDayTime()
    {
      return DateTime.Now - DateTime.Today;
    }

    public string GetDeviceInfo()
    {
      return "Alpenhunde";
    }

    public string GetStatusInfo()
    {
      switch (_status)
      {
        case EStatus.NotConnected: return "nicht verbunden";
        case EStatus.Connecting: return "verbinde ...";
        case EStatus.Connected: return "verbunden";
      }
      return "unbekannt";
    }

    public void Start()
    {
      Logger.Info("Start()");
      if (_webSocket != null)
        return;

      setInternalStatus(EStatus.Connecting);

      _webSocket = new WebSocket(_baseUrl);
      _webSocket.EmitOnPing = true;

      _webSocket.OnOpen += (sender, e) => {
        Logger.Info("connected");
        setInternalStatus(EStatus.Connected);
      };
      _webSocket.OnMessage += (sender, e) => {
        if (e.IsPing)
        {
          Logger.Debug("ping received");

        }
        else if (e.IsText)
        {
          Logger.Info("data received: {0}", e.Data);

          var parsedData = _parser.ParseMessage(e.Data);
          if (parsedData != null && parsedData.type == "timestamp")
          {
            var timeMeasurmentData = ConvertToTimemeasurementData(parsedData.data);
            if (timeMeasurmentData != null)
            {
              // Trigger event
              var handle = TimeMeasurementReceived;
              handle?.Invoke(this, timeMeasurmentData);
            }
          }
          else
          {
            Logger.Warn("could not parse received data: {0}", e.Data);
          }
        }
        else
        {
          Logger.Warn("unknown data received");
        }
      };

      _webSocket.OnClose += (sender, e) => {
        Logger.Info("onclose called");
        setInternalStatus(EStatus.NotConnected);
      };
      _webSocket.OnError += (sender, e) => {
        Logger.Info("onerror called");
        setInternalStatus(EStatus.NotConnected);
      };

      // Actually connect
      Logger.Info("start connecting");
      _webSocket.ConnectAsync();
    }

    public void Stop()
    {
      Logger.Info("Stop()");
      if (_webSocket != null)
        _webSocket.Close();
    }


    #region Implementation of ILiveDateTimeProvider
    public event LiveDateTimeChangedHandler LiveDateTimeChanged;

    TimeSpan _currentDayTimeDelta; // Contains the diff between ALGE TdC8001 and the local computer time
    protected void UpdateLiveDayTime(in TimeMeasurementEventArgs justReceivedData)
    {
      TimeSpan? receivedTime = justReceivedData.StartTime != null ? justReceivedData.StartTime : (justReceivedData.FinishTime != null ? justReceivedData.FinishTime : null);

      if (receivedTime != null)
      {
        TimeSpan tDiff = (DateTime.Now - DateTime.Today) - (TimeSpan)receivedTime;
        _currentDayTimeDelta = tDiff;

        var handler = LiveDateTimeChanged;
        handler?.Invoke(this, new LiveDateTimeEventArgs((TimeSpan)receivedTime));
      }
    }
    #endregion


    public static TimeMeasurementEventArgs ConvertToTimemeasurementData(in AlpenhundeTimingData parsedData)
    {
      TimeMeasurementEventArgs data = new TimeMeasurementEventArgs();

      TimeSpan parsedTime;
      try
      {
        var timeStr = parsedData.t;
        string[] formats = { @"hh\:mm\:ss\.ffff", @"hh\:mm\:ss\.fff", @"hh\:mm\:ss\.ff", @"hh\:mm\:ss\.f" };
        timeStr = timeStr.Trim(' ');
        parsedTime = TimeSpan.ParseExact(timeStr, formats, System.Globalization.CultureInfo.InvariantCulture);
      }
      catch (FormatException e)
      {
        Logger.Error(e, "Error while parsing Alpenhunde 't'");
        return null;
      }

      uint startNumber = 0;
      try
      {
        startNumber = byte.Parse(parsedData.n);
      }
      catch (FormatException e)
      {
        Logger.Error(e, "Error while parsing Alpenhunde 'n'; assuming startnumber 0");
      }

      data.StartNumber = startNumber;
      switch (parsedData.c)
      {
        case 1: // Start
          data.BStartTime = true;
          data.StartTime = parsedTime;
          break;
        case 128: // Finish
          data.BFinishTime= true;
          data.FinishTime = parsedTime;
          break;
        default:
          return null;
      }

      return data;
    }
  }


  public class AlpenhundeTimingData
  {
    public AlpenhundeTimingData()
    {
      i = 0;  // Index, unique identifier
      c = 0;  // Module generating the timestamp (1: Start, 128: Finish)
      n = ""; // (Short)Name of the race participant, usually the startnumber (BIB)
      m = 0;  // Time in milliseconds
      t = ""; // Time HH:MM:SS.ZHMT
    }

    public long i { get; set; }
    public byte c { get; set; }
    public string n { get; set; }
    public long m { get; set; }
    public string t { get; set; }
    public string d { get; set; }
  }

  public class AlpenhundeEvent
  {
    public AlpenhundeEvent()
    {
      type = "";
      data = null;
    }

    public string type { get; set; }
    public AlpenhundeTimingData data { get; set; }
  }


  public class AlpenhundeParser
  {

    public AlpenhundeEvent ParseMessage(string data)
    {
      //JsonConversion
      AlpenhundeEvent o = Newtonsoft.Json.JsonConvert.DeserializeObject<AlpenhundeEvent>(data);
      return o;
    }

  }

}
