using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public class TimingData
  {
    public TimeSpan? Time { get; set; }
  }


  interface IHandTiming
  {
    IEnumerable<TimingData> TimingData();
  }

}
