using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace RaceHorologyLib
{
  class TimingDeviceAlpenhunde : ILiveTimeMeasurementDevice, ILiveDateTimeProvider
  {
    private string _baseUrl;

    private WebSocket _webSocket;

    public TimingDeviceAlpenhunde(string baseUrl)
    {
      _baseUrl = baseUrl;
    }


    public bool IsOnline => throw new NotImplementedException();

    public event TimeMeasurementEventHandler TimeMeasurementReceived;
    public event StartnumberSelectedEventHandler StartnumberSelectedReceived;
    public event LiveTimingMeasurementDeviceStatusEventHandler StatusChanged;
    public event LiveDateTimeChangedHandler LiveDateTimeChanged;

    public TimeSpan GetCurrentDayTime()
    {
      throw new NotImplementedException();
    }

    public string GetDeviceInfo()
    {
      throw new NotImplementedException();
    }

    public string GetStatusInfo()
    {
      throw new NotImplementedException();
    }

    public void Start()
    {
      if (_webSocket != null)
        return;

      _webSocket = new WebSocket(_baseUrl);
      _webSocket.EmitOnPing = true;

      _webSocket.OnOpen += (sender, e) => { };
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

      _webSocket.OnClose += (sender, e) => { };
      _webSocket.OnError += (sender, e) => { };

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
