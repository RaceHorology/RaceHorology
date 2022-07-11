using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  class TimingDeviceAlpenhunde : ILiveTimeMeasurementDevice, ILiveDateTimeProvider
  {
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
      throw new NotImplementedException();
    }

    public void Stop()
    {
      throw new NotImplementedException();
    }
  }
}
