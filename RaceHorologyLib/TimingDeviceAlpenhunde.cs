using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace RaceHorologyLib
{
  enum EStatus { NotConnected, Connecting, Connected };

  public class TimeMeasurementEventArgsAlpenhunde: TimeMeasurementEventArgs
  {
    public TimeMeasurementEventArgsAlpenhunde() : base()
    {
      Index = 0;
    }
    public long Index;

  }


  public class TimingDeviceAlpenhunde : ILiveTimeMeasurementDevice, ILiveDateTimeProvider, ILiveTimeMeasurementDeviceDebugInfo, IImportTime
  {
    private System.Threading.SynchronizationContext _syncContext;
    private string _hostname;
    private string _baseUrl;
    private string _baseUrlWs;
    private EStatus _status;

    private HttpClient _webClient;
    private WebSocket _webSocket;
    private AlpenhundeParser _parser;

    public event TimeMeasurementEventHandler TimeMeasurementReceived;
    public event StartnumberSelectedEventHandler StartnumberSelectedReceived;
    public event LiveTimingMeasurementDeviceStatusEventHandler StatusChanged;

    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public TimingDeviceAlpenhunde(string hostname)
    {
      _syncContext = System.Threading.SynchronizationContext.Current;

      _hostname = hostname;
      _baseUrl = String.Format("http://{0}/", hostname);
      _baseUrlWs = String.Format("ws://{0}/ws/events", hostname);
      _parser = new AlpenhundeParser();

      _internalProtocol = String.Empty;
    }


    #region Implementation of ILiveTimeMeasurementDevice

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
      return String.Format("Alpenhunde ({0})", _hostname);
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

      _webSocket = new WebSocket(_baseUrlWs);
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
          debugMessage(e.Data);

          var parsedData = _parser.ParseMessage(e.Data);
          if (parsedData != null && parsedData.type == "timestamp")
          {
            var timeMeasurmentData = AlpenhundeParser.ConvertToTimemeasurementData(parsedData.data);
            if (timeMeasurmentData != null)
            {
              // Update internal clock for livetiming
              UpdateLiveDayTime(timeMeasurmentData);
              // Trigger time measurment event
              _syncContext.Send(delegate
              {
                var handle = TimeMeasurementReceived;
                handle?.Invoke(this, timeMeasurmentData);
              }, null);
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
        cleanup();
      };
      _webSocket.OnError += (sender, e) => {
        Logger.Info("onerror called");
        setInternalStatus(EStatus.NotConnected);
        cleanup();
      };

      // Actually connect
      Logger.Info("start connecting");
      _webSocket.ConnectAsync();
    }

    public bool IsStarted
    {
      get { return _webSocket != null && _status != EStatus.NotConnected; }
    }

    public bool IsBroken
    {
      get { return false; }
    }


    public void Stop()
    {
      Logger.Info("Stop()");
      cleanup();
    }

    private void cleanup()
    {
      if (_webSocket != null)
        _webSocket.Close();

      _webSocket = null;
    }

    #endregion


    #region Implementation of IImportTime

    public event ImportTimeEntryEventHandler ImportTimeEntryReceived;

    public EImportTimeFlags SupportedImportTimeFlags() { return EImportTimeFlags.RemoteDownload | EImportTimeFlags.StartFinishTime; }

    public void DownloadImportTimes()
    {
      if (_webClient == null)
      {
        _webClient = new HttpClient();
        _webClient.BaseAddress = new Uri(_baseUrl);
      }

      _webClient.GetAsync("timing/results/?action=all_events")
        .ContinueWith((response) =>
        {
          response.Result.Content.ReadAsStringAsync().ContinueWith((data) =>
          {
            var events = _parser.ParseEvents(data.Result);
            Logger.Debug(data.Result);

            foreach(var i in events)
            {
              var te = AlpenhundeParser.ConvertToImportTimeEntry(i);
              // Trigger time measurment event
              _syncContext.Send(delegate
              {
                var handle = ImportTimeEntryReceived;
                handle?.Invoke(this, te);
              }, null);
            }
          });
        });
    }


    #endregion


    #region Implementation of ILiveDateTimeProvider
    public event LiveDateTimeChangedHandler LiveDateTimeChanged;

    TimeSpan _currentDayTimeDelta; // Contains the diff between ALGE TdC8001 and the local computer time
    long _lastReceivedIndex = 0;

    protected void UpdateLiveDayTime(in TimeMeasurementEventArgsAlpenhunde justReceivedData)
    {
      var currentIndex = justReceivedData.Index;
      TimeSpan? receivedTime = justReceivedData.StartTime != null ? justReceivedData.StartTime : (justReceivedData.FinishTime != null ? justReceivedData.FinishTime : null);

      // If an index is returned for the first time, use this as time synchronization
      if ((    (currentIndex > _lastReceivedIndex)        // Standard case: next run
            || (currentIndex < _lastReceivedIndex - 20 )) // Special case: reset of Alpenhunde system; assumption: a larger gap to the last received index is a reset
           && receivedTime != null) 
      {
        TimeSpan tDiff = (DateTime.Now - DateTime.Today) - (TimeSpan)receivedTime;
        _currentDayTimeDelta = tDiff;

        var handler = LiveDateTimeChanged;
        handler?.Invoke(this, new LiveDateTimeEventArgs((TimeSpan)receivedTime));

        _lastReceivedIndex = currentIndex;
      }
    }
    #endregion


    #region Implementation of ILiveTimeMeasurementDeviceDebugInfo

    public event RawMessageReceivedEventHandler RawMessageReceived;

    private string _internalProtocol;

    public string GetProtocol()
    {
      return _internalProtocol;
    }

    void debugMessage(string message)
    {
      if (!string.IsNullOrEmpty(_internalProtocol))
        _internalProtocol += "\n";
      _internalProtocol += message;

      RawMessageReceivedEventHandler handler = RawMessageReceived;
      handler?.Invoke(this, message);
    }

    #endregion

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
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


    public AlpenhundeEvent ParseMessage(string data)
    {
      //JsonConversion
      AlpenhundeEvent o = Newtonsoft.Json.JsonConvert.DeserializeObject<AlpenhundeEvent>(data);
      return o;
    }

    public List<AlpenhundeTimingData> ParseEvents(string data)
    {
      //JsonConversion
      var parsedData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<AlpenhundeTimingData>>>(data);
      return parsedData["events"];
    }



    public static TimeMeasurementEventArgsAlpenhunde ConvertToTimemeasurementData(in AlpenhundeTimingData parsedData)
    {
      var data = new TimeMeasurementEventArgsAlpenhunde();

      TimeSpan? parsedTime = AlpenhundeParser.ParseTime(parsedData);
      if (parsedTime == null) return null;

      uint startNumber = AlpenhundeParser.ParseStartNumber(parsedData);
      data.Index = parsedData.i;

      data.StartNumber = startNumber;
      data.Valid = startNumber > 0;
      switch (AlpenhundeParser.GetMeasurementPoint(parsedData))
      {
        case EMeasurementPoint.Start:
          data.BStartTime = true;
          data.StartTime = parsedTime;
          break;
        case EMeasurementPoint.Finish:
          data.BFinishTime = true;
          data.FinishTime = parsedTime;
          break;
        default:
          return null;
      }

      return data;
    }


    public static ImportTimeEntry ConvertToImportTimeEntry(in AlpenhundeTimingData parsedData)
    {
      var parsedTime = AlpenhundeParser.ParseTime(parsedData);
      uint startNumber = AlpenhundeParser.ParseStartNumber(parsedData);

      ImportTimeEntry data;
      switch (AlpenhundeParser.GetMeasurementPoint(parsedData))
      {
        case EMeasurementPoint.Start:
          data = new ImportTimeEntry(startNumber, parsedTime, null);
          break;
        case EMeasurementPoint.Finish:
          data = new ImportTimeEntry(startNumber, null, parsedTime);
          break;
        default:
          return null;
      }

      return data;
    }


    public static TimeSpan? ParseTime(in AlpenhundeTimingData parsedData)
    {
      var timeStr = parsedData.t;
      try
      {
        string[] formats = { @"hh\:mm\:ss\.ffff", @"hh\:mm\:ss\.fff", @"hh\:mm\:ss\.ff", @"hh\:mm\:ss\.f" };
        timeStr = timeStr.Trim(' ');
        return TimeSpan.ParseExact(timeStr, formats, System.Globalization.CultureInfo.InvariantCulture);
      }
      catch (FormatException e)
      {
        Logger.Error(e, "Error while parsing Alpenhunde 't'");
        return null;
      }
    }

    public static EMeasurementPoint GetMeasurementPoint(in AlpenhundeTimingData parsedData)
    {
      switch (parsedData.c)
      {
        case 1: // Start
          return EMeasurementPoint.Start;
        case 128: // Finish
          return EMeasurementPoint.Finish;
      }
      return EMeasurementPoint.Undefined;
    }

    public static uint ParseStartNumber(in AlpenhundeTimingData parsedData)
    {
      uint startNumber = 0;
      try
      {
        startNumber = uint.Parse(parsedData.n);
      }
      catch (FormatException e)
      {
        Logger.Error(e, "Error while parsing Alpenhunde 'n'; assuming startnumber 0");
      }
      return startNumber;
    }

  }

}
