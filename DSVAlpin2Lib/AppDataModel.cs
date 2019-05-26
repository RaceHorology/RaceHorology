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


  public interface IAppDataModelDataBase
  {
    ObservableCollection<Participant> GetParticipants();
    RaceRun GetRaceRun(uint run);

  };

  public class AppDataModel
  {
    private IAppDataModelDataBase _db;

    ObservableCollection<Participant> _participants;
    List<RaceRun> _runs;

    public ObservableCollection<Participant> GetParticipants()
    {
      return _participants;
    }

    public AppDataModel(IAppDataModelDataBase db)
    {
      _db = db;
      _participants = _db.GetParticipants();

      _runs = new List<RaceRun>();

      // TODO: Assuming 1 run for now
      var rr1 = _db.GetRaceRun(1);
      _runs.Add(rr1);

      var rr2 = _db.GetRaceRun(2);
      _runs.Add(rr2);
    }


    public uint GetMaxRun()
    {
      return (uint)_runs.Count;
    }
    public RaceRun GetRun(uint run)
    {
      return _runs.ElementAt((int)run);
    }

  }
}
