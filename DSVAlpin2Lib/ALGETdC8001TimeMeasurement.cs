using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{
  class ALGETdC8001TimeMeasurement : ILiveTimeMeasurement
  {
    public event TimeMeasurementEventHandler TimeMeasurementReceived;

    public ALGETdC8001TimeMeasurement(uint comport)
    { }

    private void MainLoop()
    {
      // ... do some magic


      // Fill the data
      TimeMeasurementEventArgs data = new TimeMeasurementEventArgs();
      data.StartNumber = 100;
      data.RunTime = new TimeSpan();
      //...

      // Trigger event
      var handle = TimeMeasurementReceived;
      handle?.Invoke(this, data);
    }
  }
}
