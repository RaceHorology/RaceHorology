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
    public string Sex { get; set; }
    public int Year { get; set; }
    public string Club { get; set; }
    public string Nation { get; set; }
    public string Class { get; set; } 
    public uint StartNumber { get; set; }
  }


  public class RunResult
  {
    public Participant _participant;

    public TimeSpan? _runTime;

    public TimeSpan? _startTime;
    public TimeSpan? _finishTime;
  }

}
