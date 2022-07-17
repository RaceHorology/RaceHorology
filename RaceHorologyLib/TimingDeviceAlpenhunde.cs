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

    public event TimeMeasurementEventHandler TimeMeasurementReceived;
    public event StartnumberSelectedEventHandler StartnumberSelectedReceived;
    public event LiveTimingMeasurementDeviceStatusEventHandler StatusChanged;
    public event LiveDateTimeChangedHandler LiveDateTimeChanged;

    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public TimingDeviceAlpenhunde(string baseUrl)
    {
      _baseUrl = baseUrl;
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
          Logger.Info("data received: ", e.Data);
          // e.Data
        }
        else
        {
          Logger.Warn("unknown data received");
          // Problem
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
  }


  public class AlpenhundeTimingData
  {
    public AlpenhundeTimingData()
    {
      i = 0;
      c = 0;
      n = "";
      t = "";
    }

    public long i { get; set; }
    public byte c { get; set; }
    public string n { get; set; }
    public string t { get; set; }
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

    public void ParseMessage(string data)
    {
      //JsonConversion
      object o = Newtonsoft.Json.JsonConvert.DeserializeObject<AlpenhundeEvent>(data);
    }

  }

}