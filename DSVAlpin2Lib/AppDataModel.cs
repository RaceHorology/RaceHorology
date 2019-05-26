using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{

  public class RaceRun
  {
    uint _run;
    ObservableCollection<Participant> _startList;
    ObservableCollection<Tuple<Participant, TimeSpan>> _onTrack; // Participant and StartTime
    ObservableCollection<RunResult> _results;

    public RaceRun(uint run)
    {
      _run = run;

      _startList = new ObservableCollection<Participant>();
      _onTrack   = new ObservableCollection<Tuple<Participant, TimeSpan>>(); 
      _results   = new ObservableCollection<RunResult>();
    }


    ObservableCollection<Participant> GetStartList(uint run)
    {

      return _startList;
    }

    ObservableCollection<Tuple<Participant, TimeSpan>> GetOnTrackList()
    {
      return _onTrack;
    }

    ObservableCollection<RunResult> GetResultList()
    {
      return _results;
    }

        
    public void InsertResult(RunResult r)
    {
      _results.Add(r);
    }

  }


  /*
    Race
      Run(n)
        StartList => Participants
        OnTrack => Participants, time
        Result => Participants, result-time

    SplitBy
      Class
      Group
  */

  public class AppDataModel
  {
    RaceRun[] _runs;


    public uint GetMaxRun()
    {
      return (uint)_runs.Length;
    }
    public RaceRun GetRun(uint run)
    {
      return _runs[run];
    }

  }
}
