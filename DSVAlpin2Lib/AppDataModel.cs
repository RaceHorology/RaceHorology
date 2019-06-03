using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{

  /// <summary>
  /// Main Application Data Model - all data shall be get through this instance, also modification shall be done on this instance
  /// </summary>
  /// 
  /// Data is loaded from the data base
  /// Data is written back to the data base in case it is needed (not yet implemented)
  /// 
  /// <remarks>not yet fully implemented</remarks>
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

      // TODO: Assuming 2 runs for now
      var rr1 = _db.GetRaceRun(1);
      rr1.SetStartList(_participants);
      _runs.Add(rr1);

      var rr2 = _db.GetRaceRun(2);
      rr2.SetStartList(_participants);
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

  
  /// <summary>
  /// Represents a race run. Typically a race consists out of two race runs.
  /// </summary>
  public class RaceRun
  {
    uint _run;
    ItemsChangeObservableCollection<Participant> _startList;
    ItemsChangeObservableCollection<RunResult> _results; // This list represents the actual results

    ItemsChangeObservableCollection<RunResult> _onTrack; // TODO: This list only contains the particpants that are on the run (might get removed later)


    public RaceRun(uint run)
    {
      _run = run;

      _startList = new ItemsChangeObservableCollection<Participant>();
      _onTrack   = new ItemsChangeObservableCollection<RunResult>(); 
      _results   = new ItemsChangeObservableCollection<RunResult>();
    }


    public ItemsChangeObservableCollection<Participant> GetStartList()
    {
      return _startList;
    }

    public void SetStartList(ICollection<Participant> participants)
    {
      foreach (var p in participants)
        _startList.Add(p);
    }

    public ItemsChangeObservableCollection<RunResult> GetOnTrackList()
    {
      return _onTrack;
    }

    public ItemsChangeObservableCollection<RunResult> GetResultList()
    {
      return _results;
    }



    public void UpdateTimeMeasurement(Participant participant, TimeSpan? startTime = null, TimeSpan? finishTime = null, TimeSpan? runTime = null)
    {
      RunResult result = _results.SingleOrDefault(r => r._participant == participant);

      if (result == null)
        result = new RunResult();

        result._participant = participant;

      if (startTime != null)
        result.SetStartTime((TimeSpan)startTime);

      if (finishTime != null)
        result.SetFinishTime((TimeSpan)finishTime);

      if (runTime != null && startTime == null && finishTime == null)
        result.SetRunTime((TimeSpan)runTime);

      InsertResult(result);
    }

    public void InsertResult(RunResult r)
    {
      // Check if already inserted
      if (_results.SingleOrDefault(x => x==r)==null)
        _results.Add(r);

      _UpdateInternals();
    }

    private void _UpdateInternals()
    {
      // Remove from onTrack list if a result is available
      var itemsToRemove = _onTrack.Where(r => r.GetRunTime() != null).ToList();
      foreach (var itemToRemove in itemsToRemove)
        _onTrack.Remove(itemToRemove);

      // Add to onTrack list if run result is not yet available
      foreach (var res in _results)
        if (res.GetRunTime() == null)
          if (!_onTrack.Contains(res))
            _onTrack.Add(res);
    }

  }


  /// <summary>
  /// Defines the interface to the actual database engine
  /// </summary>
  /// <remarks>Assuming the database format changes we can simply create another implementation.</remarks>
  public interface IAppDataModelDataBase
  {
    ObservableCollection<Participant> GetParticipants();
    RaceRun GetRaceRun(uint run);

  };


  #region Time Measurement

  public class TimeMeasurementEventArgs : EventArgs
  {
    public uint StartNumber;
    public TimeSpan? RunTime;
    public TimeSpan? StartTime;
    public TimeSpan? FinishTime;
  }

  public delegate void TimeMeasurementEventHandler(object sender, TimeMeasurementEventArgs e);

  public interface ILiveTimeMeasurement
  {
    /// <summary>
    /// If a time measurement happend, this event is triggered
    /// </summary>
    event TimeMeasurementEventHandler TimeMeasurementReceived;

  }

  #endregion

}
