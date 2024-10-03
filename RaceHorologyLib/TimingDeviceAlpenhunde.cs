using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Timers;
using WebSocketSharp;
using WebSocket = WebSocketSharp.WebSocket;

namespace RaceHorologyLib
{
  enum EStatus { NotConnected, Connecting, Connected };

  public class TimeMeasurementEventArgsAlpenhunde : TimeMeasurementEventArgs
  {
    public TimeMeasurementEventArgsAlpenhunde() : base()
    {
      Index = 0;
    }
    public long Index;

  }


  public class TimingDeviceAlpenhunde : ILiveTimeMeasurementDevice, ILiveDateTimeProvider, ILiveTimeMeasurementDeviceDebugInfo, IImportTime
  {
    private static int ConfigPingInterval = 5000; // ms
    private static int ConfigPingTimeout = 2000; // ms
    private static int ConfigMissingPings = 2;
    private static TimeSpan KeepAliveDelta = TimeSpan.FromMilliseconds(ConfigMissingPings * ConfigPingInterval + ConfigPingTimeout);


    private System.Threading.SynchronizationContext _syncContext;
    private string _hostname;
    private string _baseUrl;
    private string _baseUrlWs;
    private EStatus _status;

    private object _lock = new object();
    private HttpClient _webClient;
    private WebSocket _webSocket;
    private System.Timers.Timer _keepAliveTimer;
    private System.Timers.Timer _keepAliveCheckTimer;
    private DateTime _lastPingReceivedTime = DateTime.Now;
    private DateTime _lastPingSentTime = DateTime.Now;
    private int _connectRetryCount = 0;
    private bool _isStarted = false; // Flag indicating whether Start() has been called and shall stay online

    private AlpenhundeParser _parser;
    private AlpenhundeSystemInfo _systemInfo;
    protected DeviceInfo _deviceInfo = new DeviceInfo
    {
      Manufacturer = "Alpenhunde",
      Model = string.Empty,
      PrettyName = "Alpenhunde",
      SerialNumber = string.Empty
    };


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

      _systemInfo = new AlpenhundeSystemInfo();
      _webClient = new HttpClient()
      {
        BaseAddress = new Uri(_baseUrl)
      };
    }


    #region Implementation of ILiveTimeMeasurementDevice

    private void setInternalStatus(EStatus status)
    {
      if (_status != status)
      {
        if (this._status == EStatus.Connected && status != EStatus.Connected)
          _systemInfo.Reset();
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


    public virtual DeviceInfo GetDeviceInfo()
    {
      _deviceInfo.SerialNumber = _systemInfo.SerialNumber;
      return _deviceInfo;
    }

    public AlpenhundeSystemInfo SystemInfo { get { return _systemInfo; } }

    public string GetStatusInfo()
    {
      var status = "unbekannt";
      switch (_status)
      {
        case EStatus.NotConnected: status = "nicht verbunden"; break;
        case EStatus.Connecting: status = "verbinde ..."; break;
        case EStatus.Connected: status = "verbunden"; break;
      }

      return String.Format("{0} - {1}", _hostname, status);
    }

    public void Start()
    {
      Logger.Info("Start()");
      _isStarted = true;
      _connectRetryCount = 0;
      startInternal();
    }

    public void startInternal()
    {
      Logger.Info("startInternal()");
      lock (_lock)
      {
        if (_webSocket != null)
          return;

        setInternalStatus(EStatus.Connecting);
        _connectRetryCount++;

        _webSocket = new WebSocket(_baseUrlWs)
        {
          EmitOnPing = true
        };

        _webSocket.OnOpen += (sender, e) =>
        {
          Logger.Info("connected {0}", sender);
          setInternalStatus(EStatus.Connected);

          // Reset Ping time stamps
          _lastPingReceivedTime = DateTime.Now;
          _lastPingSentTime = DateTime.Now;
          // Start Keep Alive Timer
          _keepAliveTimer = new System.Timers.Timer();
          _keepAliveTimer.Elapsed += keepAliveTimer_Elapsed;
          _keepAliveTimer.Interval = ConfigPingInterval; // ms
          _keepAliveTimer.AutoReset = true;
          _keepAliveTimer.Start();
          // Start Check Timer
          _keepAliveCheckTimer = new System.Timers.Timer();
          _keepAliveCheckTimer.Elapsed += keepAliveCheckTimer_Elapsed;
          _keepAliveCheckTimer.Interval = 1000; // ms
          _keepAliveCheckTimer.AutoReset = true;
          _keepAliveCheckTimer.Start();
        };
        _webSocket.OnMessage += (sender, e) =>
        {
          if (e.IsPing || e.Data == "PONG")
          {
            Logger.Debug("pong received");
            _lastPingReceivedTime = DateTime.Now;
          }
          else if (e.IsText && !e.Data.IsNullOrEmpty())
          {
            Logger.Info("data received: {0}", e.Data);
            debugMessage(e.Data);

            var parsedData = _parser.ParseMessage(e.Data);
            if (parsedData != null)
            {
              if (parsedData.type == "timestamp")
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
                Logger.Warn("Unknown data type: {0}", parsedData.type);
              }
            }
            else
            {
              Logger.Warn("could not parse received data: {0}", e.Data);
            }
          }
        };

        _webSocket.OnClose += (sender, e) =>
        {
          Logger.Info("onclose called {0}", sender);
          setInternalStatus(EStatus.NotConnected);
          cleanup(false);
        };
        _webSocket.OnError += (sender, e) =>
        {
          Logger.Info("onerror called {0}", sender);
          setInternalStatus(EStatus.NotConnected);
          cleanup(false);
        };
      }

      // Actually connect
      Logger.Info("start connecting");
      _webSocket.ConnectAsync();

      // Pull some infos at startup
      DownloadSystemStatus();
    }

    private void keepAliveCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
    {

      var pingDiff = _lastPingSentTime - _lastPingReceivedTime; // 
      var nowDiff = DateTime.Now - _lastPingReceivedTime;

      Logger.Info(String.Format(
        "Ping check, last sent: {0} last received: {1}, pingDiff: {2}, nowDiff: {3}",
        _lastPingSentTime, _lastPingReceivedTime, pingDiff, nowDiff));

      if (pingDiff.Ticks > 0 /*if positiv: outstanding ping*/
        && nowDiff > KeepAliveDelta  /* timeout */)
      {
        Logger.Warn("Ping outstanding, closing connection");
        setInternalStatus(EStatus.NotConnected);
        cleanup(false);
      }
    }

    private void keepAliveTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      try
      {
        if (_webSocket != null)
        {
          _lastPingSentTime = DateTime.Now;
          _webSocket.Send("PING");

          DownloadSystemStatus();
        }

      }
      catch (Exception ex)
      {
        Logger.Error(ex);
      }
    }

    public bool IsStarted
    {
      get => _isStarted;
    }


    public void Stop()
    {
      Logger.Info("Stop()");
      _isStarted = false;
      cleanup(false);
    }

    private void cleanup(bool reconnectIfPossible)
    {
      Logger.Info("cleanup(), reconnectIfPossible: {0}", reconnectIfPossible);
      lock (_lock)
      {
        if (_keepAliveTimer != null)
        {
          _keepAliveTimer.Stop();
          _keepAliveTimer.Dispose();
        }
        _keepAliveTimer = null;

        if (_keepAliveCheckTimer != null)
        {
          _keepAliveCheckTimer.Stop();
          _keepAliveCheckTimer.Dispose();
        }
        _keepAliveCheckTimer = null;
        _webSocket?.Close();
        _webSocket = null;
      }

      if (reconnectIfPossible && _isStarted)
      {
        if (_connectRetryCount < 10)
        {
          Logger.Info("reconnecting, trial {0} ... ", _connectRetryCount);
          startInternal();  // Re-connect
        }
        else
        {
          Logger.Info("giving up after trial {0} ... ", _connectRetryCount);
          _isStarted = false;
          setInternalStatus(EStatus.NotConnected);
        }
      }
      else if (!reconnectIfPossible && _isStarted)
      {
        Logger.Info("Stopping connection... ");
        _isStarted = false;
        setInternalStatus(EStatus.NotConnected);
      }

    }

    #endregion


    #region Implementation of IImportTime

    public event ImportTimeEntryEventHandler ImportTimeEntryReceived;

    public EImportTimeFlags SupportedImportTimeFlags() { return EImportTimeFlags.RemoteDownload | EImportTimeFlags.StartFinishTime; }

    public void DownloadImportTimes()
    {
      _webClient.GetAsync("timing/results/?action=all_events")
        .ContinueWith((response) =>
        {
          if (response.Result.IsSuccessStatusCode)
          {
            response.Result.Content.ReadAsStringAsync().ContinueWith((data) =>
            {
              try
              {
                var events = _parser.ParseEvents(data.Result);
                Logger.Debug(data.Result);

                foreach (var i in events)
                {
                  var te = AlpenhundeParser.ConvertToImportTimeEntry(i);
                  // Trigger time measurment event
                  _syncContext.Send(delegate
                  {
                    var handle = ImportTimeEntryReceived;
                    handle?.Invoke(this, te);
                  }, null);
                }
              }
              catch (Exception ex)
              {
                Logger.Error(ex);
              }
            });
          }
        });
    }

    public void DownloadSystemStatus()
    {
      _webClient.GetAsync("system/")
        .ContinueWith((response) =>
        {
          try
          {
            if (response.Result.IsSuccessStatusCode)
            {
              response.Result.Content.ReadAsStringAsync().ContinueWith((data) =>
              {
                Logger.Debug(data.Result);
                var systemData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(data.Result);
                Logger.Debug(systemData);
                _systemInfo.SetRawData(systemData);
                checkAndSetSystemTime();
              });
            }
          }
          catch (Exception ex)
          {
            Logger.Error(ex);
          }
        });
    }


    public void DownloadFIS(Func<byte[], bool> saveCallback)
    {
      _webClient.GetAsync("FIS")
        .ContinueWith((response) =>
        {
          try
          {
            if (response.Result.IsSuccessStatusCode)
            {
              response.Result.Content.ReadAsByteArrayAsync().ContinueWith((data) =>
              {
                Logger.Debug("FIS export {0} Bytes", data.Result.Length);
                saveCallback(data.Result);
              });
            }
          }
          catch (Exception ex)
          {
            Logger.Error(ex);
          }
        });
    }

    public void Synchronize()
    {
      performPostAction("system/?action=sync_clock");
    }
    public void SetChannel(int channel)
    {
      performPostAction(string.Format("system/?action=switch_channel&channel={0}", channel));
    }

    protected void performPostAction(string subUrl)
    {
      Logger.Info("POST \"{0}\"", subUrl);
      _webClient.PostAsync(subUrl, null)
        .ContinueWith((response) =>
        {
          Logger.Info("POST Completed \"{0}\", Status-Code: {1}", subUrl, response.Result.StatusCode);
        });
    }


    private void checkAndSetSystemTime()
    {
      if (SystemInfo.SystemTime == null)
      {
        // Set System Time
        Logger.Info("Systemzeit nicht gesetzt => muss gesetzt werden");
        performPostAction(String.Format("system/?action=date_time&sec={0}&usec=0", DateTime.Now.UnixEpoch(true)));
      }
      else
      {
        Logger.Debug("Systemzeit Alpenhunde: {0}, PC: {1}, Diff: {2}", SystemInfo.SystemTime, DateTime.Now, (SystemInfo.SystemTime - DateTime.Now));
      }
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
      if (((currentIndex > _lastReceivedIndex)        // Standard case: next run
            || (currentIndex < _lastReceivedIndex - 20)) // Special case: reset of Alpenhunde system; assumption: a larger gap to the last received index is a reset
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
      keepAliveData = null;
    }

    public string type { get; set; }
    public AlpenhundeTimingData data { get; set; }
    public int? keepAliveData { get; set; }
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


  public class AlpenhundeSystemInfo : INotifyPropertyChanged
  {
    protected Dictionary<string, string> _rawData = new Dictionary<string, string>();
    public void SetRawData(Dictionary<string, string> rawData)
    {
      _rawData = rawData;
      string v;
      int i;
      if (_rawData.TryGetValue("systemSerialNumber", out v))
        SerialNumber = v;
      else
        SerialNumber = "";
      if (_rawData.TryGetValue("firmwareVersion", out v))
        FirmwareVersion = v;
      else
        FirmwareVersion = "";
      if (_rawData.TryGetValue("dateAndTime", out v))
        setSystemTime(v);
      else
        _systemTime = null;

      if (_rawData.TryGetValue("serial", out v) && int.TryParse(v, out i))
        CurrentDevice = i;
      else
        CurrentDevice = 0;

      if (_rawData.TryGetValue("battery_level", out v) && int.TryParse(v, out i))
        BatteryLevel = i;
      else
        BatteryLevel = 0;

      if (_rawData.TryGetValue("NextFreeIndex", out v))
        NextFreeIndex = v;
      else
        NextFreeIndex = "";
      if (_rawData.TryGetValue("channel", out v))
        Channel = v;
      else
        Channel = "";

      if (_rawData.TryGetValue("starter_status", out v))
        StarterStatus = v;
      else
        StarterStatus = "";
      if (_rawData.TryGetValue("stopper_status", out v))
        StopperStatus = v;
      else
        StopperStatus = "";
      if (_rawData.TryGetValue("RSSI_master", out v) && int.TryParse(v, out i))
        RSSIMaster = i;
      else
        RSSIMaster = -1000;
      if (_rawData.TryGetValue("RSSI_start", out v) && int.TryParse(v, out i))
        RSSIStarter = i;
      else
        RSSIStarter = -1000;
      if (_rawData.TryGetValue("RSSI_stop", out v) && int.TryParse(v, out i))
        RSSIStopper = i;
      else
        RSSIStopper = -1000;

      if (_rawData.TryGetValue("openLightBarrier_id_0", out v) && int.TryParse(v, out i))
        OpenLightBarrier = i;
      else
        OpenLightBarrier = 0;
    }

    public void Reset()
    {
      SetRawData(new Dictionary<string, string>());
    }

    private string _serialNumber;
    public string SerialNumber
    {
      get => _serialNumber;
      set { if (value != _serialNumber) { _serialNumber = value; NotifyPropertyChanged(); } }
    }

    private string _firmwareVersion;
    public string FirmwareVersion
    {
      get => _firmwareVersion;
      set { if (value != _firmwareVersion) { _firmwareVersion = value; NotifyPropertyChanged(); } }
    }

    private DateTime? _systemTime;
    public DateTime? SystemTime { get => _systemTime; }
    private void setSystemTime(string timeStr)
    {
      try
      {
        // Format: 2024-06-16 08:22:20.11
        _systemTime = DateTime.ParseExact(timeStr, "yyyy-MM-dd HH:mm:ss.ff", System.Globalization.CultureInfo.InvariantCulture);
      }
      catch (Exception)
      {
        _systemTime = null;
      }
      NotifyPropertyChanged("SystemTime");
    }

    private int _batteryLevel;
    public int BatteryLevel
    {
      get => _batteryLevel;
      set { if (value != _batteryLevel) { _batteryLevel = value; NotifyPropertyChanged(); } }
    }

    private string _nextFreeIndex;
    public string NextFreeIndex
    {
      get => _nextFreeIndex;
      set { if (value != _nextFreeIndex) { _nextFreeIndex = value; NotifyPropertyChanged(); } }
    }

    private string _channel;
    public string Channel
    {
      get => _channel;
      set { if (value != _channel) { _channel = value; NotifyPropertyChanged(); } }
    }

    private string _starterStatus;
    public string StarterStatus
    {
      get => _starterStatus;
      set { if (value != _starterStatus) { _starterStatus = value; NotifyPropertyChanged(); } }
    }
    private string _stopperStatus;
    public string StopperStatus
    {
      get => _stopperStatus;
      set { if (value != _stopperStatus) { _stopperStatus = value; NotifyPropertyChanged(); } }
    }

    private int _rssiMaster = -1000;
    public int RSSIMaster
    {
      get => _rssiMaster;
      set { if (value != _rssiMaster) { _rssiMaster = value; NotifyPropertyChanged(); } }
    }
    private int _rssiStarter = -1000;
    public int RSSIStarter
    {
      get => _rssiStarter;
      set { if (value != _rssiStarter) { _rssiStarter = value; NotifyPropertyChanged(); } }
    }
    private int _rssiStopper = -1000;
    public int RSSIStopper
    {
      get => _rssiStopper;
      set { if (value != _rssiStopper) { _rssiStopper = value; NotifyPropertyChanged(); } }
    }

    private int _openLightBarrier;
    public int OpenLightBarrier
    {
      get => _openLightBarrier;
      set { if (value != _openLightBarrier) { _openLightBarrier = value; NotifyPropertyChanged(); NotifyPropertyChanged("OpenLightBarrierName"); } }
    }
    public string OpenLightBarrierName { get => _openLightBarrier > 0 ? GetDeviceName(_openLightBarrier) : ""; }

    private int _currentDevice;
    public int CurrentDevice
    {
      get => _currentDevice;
      set { if (value != _currentDevice) { _currentDevice = value; NotifyPropertyChanged(); NotifyPropertyChanged("CurrentDeviceName"); } }
    }
    public string CurrentDeviceName { get => GetDeviceName(_currentDevice); }

    static string GetDeviceName(int device)
    {
      switch (device)
      {
        case 0: return "Master";
        case 1: return "Starter";
        case 128: return "Stopper";
        default: return "";
      }
    }


    #region INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
  }
}
