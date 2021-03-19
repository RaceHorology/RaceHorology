using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  public class RunResultProxy : RunResult
  {
    RaceRun _raceRun;
    RunResult _rrMaster;

    public RunResultProxy(RaceParticipant rp, RaceRun raceRun)
      : base(rp)
    {
      _raceRun = raceRun;
      _raceRun.GetResultList().CollectionChanged += runResults_CollectionChanged;

      runResults_CollectionChanged(null, null);
    }

    private void runResults_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      // Look for RunResult for current participant and update this
      var rr = _raceRun.GetResultList().FirstOrDefault( r => r.Participant == this.Participant );
      if (rr != _rrMaster)
      {
        if (_rrMaster != null)
          _rrMaster.PropertyChanged -= rr_PropertyChanged;
        
        _rrMaster = rr;

        if (_rrMaster != null)
          _rrMaster.PropertyChanged += rr_PropertyChanged;
        
        UpdateRunResult(rr);
      }
    }

    private void rr_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      var rr = _raceRun.GetResultList().FirstOrDefault( r => r.Participant == this.Participant );
      UpdateRunResult(rr);
    }
  }


  /// <summary>
  /// View Model for the DisqualifyUC
  /// It contains a list (see GetGridView()) that has a RunResult for each partcipant of the race, so that the grid can filter based on RunResults.
  /// Reason: Internally the RaceRun only has RunResults stored in case something has been entered (time of disqualification). 
  ///         The DisqualifyUI however needs data to be present from all participants.
  /// </summary>
  public class DiqualifyVM
  {
    RaceRun _raceRun;

    CopyObservableCollection<RunResultProxy, RaceParticipant> _disqualifyList;

    public DiqualifyVM(RaceRun raceRun)
    {
      _raceRun = raceRun;

      _disqualifyList = new CopyObservableCollection<RunResultProxy, RaceParticipant>(_raceRun.GetRace().GetParticipants(), (p) => { return new RunResultProxy(p, _raceRun); }, false);
    }

    public ObservableCollection<RunResultProxy> GetGridView()
    {
      return _disqualifyList;
    }

  }

}
