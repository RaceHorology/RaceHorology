using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RHAlgeTimyUSB
{
  public class AlgeTimyUSB
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
  }
}
