using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{
  public class Participant
  {
    public string Name { get; set; }
    public string Firstname { get; set; }
    public int Year { get; set; }
    public string Club{ get; set; }
  }

  public class TimeMeasurement
  {
    private double _value; // time in decimal normalized to a day

    public TimeMeasurement(double value)
    {
      _value = value;
    }

    public TimeSpan GetTimeSpan()
    {
      const Int64 nanosecondsPerDay = 24L * 60 * 60 * 1000 * 1000 * 10 ;

      Int64 ticks = (Int64)( nanosecondsPerDay * _value + .5); 
      TimeSpan ts = new TimeSpan(ticks); // unit: 1 tick = 100 nanoseconds
      return ts;
    }

  };

}
