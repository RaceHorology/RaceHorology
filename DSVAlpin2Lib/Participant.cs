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
    // Some public properties to get displayed in the list
    // TODO: This should not be part of this calss, instead another entity should do the conversion
    public string StartNumber { get { return _participant.StartNumber.ToString(); } }
    public string Name { get { return _participant.Name; } }
    public string Firstname { get { return _participant.Firstname; } }
    public string Club { get { return _participant.Club; } }
    public string Class { get { return _participant.Class; } }

    public string Runtime { get { return _runTime == null ? "niz" : _runTime.ToString(); } }


    public Participant _participant;

    public TimeSpan? _runTime;

    public TimeSpan? _startTime;
    public TimeSpan? _finishTime;
  }

}
