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

    public TimingDeviceAlpenhunde(string baseUrl)
    {
      _baseUrl = baseUrl;
    }


    public bool IsOnline
    {
      get { return _status == EStatus.Connected; }
    } 

    public event TimeMeasurementEventHandler TimeMeasurementReceived;
    public event StartnumberSelectedEventHandler StartnumberSelectedReceived;
    public event LiveTimingMeasurementDeviceStatusEventHandler StatusChanged;
    public event LiveDateTimeChangedHandler LiveDateTimeChanged;

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
      if (_webSocket != null)
        return;

      _status = EStatus.Connecting;

      _webSocket = new WebSocket(_baseUrl);
      _webSocket.EmitOnPing = true;

      _webSocket.OnOpen += (sender, e) => {
        _status = EStatus.Connected;
      };
      _webSocket.OnMessage += (sender, e) => { 
        if (e.IsPing)
        {

        }
        else if (e.IsText)
        {
          // e.Data
        }
        else
        {
          // Problem
        }
      };

      _webSocket.OnClose += (sender, e) => {
        _status = EStatus.NotConnected;
      };
      _webSocket.OnError += (sender, e) => {
        _status = EStatus.NotConnected;
      };

      // Actually connect
      _webSocket.Connect();
    }

    public void Stop()
    {
      if (_webSocket != null)
        _webSocket.Close();
    }
  }
}
