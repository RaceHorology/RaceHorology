using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RHAlgeTimyUSB
{
  public class AlgeTimyUSB : ILiveTimeMeasurementDevice, ILiveDateTimeProvider, IImportTime, IHandTiming
  {
    Alge.TimyUsb _timy;

    public AlgeTimyUSB()
    {
      _timy = new Alge.TimyUsb();

      _timy.DeviceConnected += _timy_DeviceConnected;
      _timy.DeviceDisconnected += _timy_DeviceDisconnected;
      _timy.LineReceived += _timy_LineReceived;
      _timy.Start();
    }

    private void _timy_LineReceived(object sender, Alge.DataReceivedEventArgs e)
    {
      System.Diagnostics.Trace.WriteLine(e.Data);
    }

    private void _timy_DeviceDisconnected(object sender, Alge.DeviceChangedEventArgs e)
    {
      System.Diagnostics.Trace.WriteLine("Device dis-connected");
    }

    private void _timy_DeviceConnected(object sender, Alge.DeviceChangedEventArgs e)
    {
      System.Diagnostics.Trace.WriteLine("Device connected");
    }


    #region ILiveTimeMeasurementDevice

    public bool IsStarted => throw new NotImplementedException();

    public bool IsOnline => throw new NotImplementedException();

    public event StartnumberSelectedEventHandler StartnumberSelectedReceived;
    public event LiveTimingMeasurementDeviceStatusEventHandler StatusChanged;
    public event TimeMeasurementEventHandler TimeMeasurementReceived;
    public event LiveDateTimeChangedHandler LiveDateTimeChanged;
    public event ImportTimeEntryEventHandler ImportTimeEntryReceived;

    public void Start()
    {
      throw new NotImplementedException();
    }

    public void Stop()
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
    #endregion

    public TimeSpan GetCurrentDayTime()
    {
      throw new NotImplementedException();
    }

    #region IHandTiming
    public void Connect()
    {
      throw new NotImplementedException();
    }

    public void Disconnect()
    {
      throw new NotImplementedException();
    }

    public void StartGetTimingData()
    {
      throw new NotImplementedException();
    }

    public IEnumerable<TimingData> TimingData()
    {
      throw new NotImplementedException();
    }

    public void Dispose()
    {
      throw new NotImplementedException();
    }

    public void DoProgressReport(IProgress<StdProgress> progress)
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}
