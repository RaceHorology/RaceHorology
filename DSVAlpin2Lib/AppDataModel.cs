﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DSVAlpin2Lib
{

  /// <summary>
  /// Main Application Data Model - all data shall be get through this instance, also modification shall be done on this instance
  /// </summary>
  /// 
  /// Data is loaded from the data base
  /// Data is written back to the data base in case it is needed
  /// 
  /// <remarks>not yet fully implemented</remarks>
  public class AppDataModel
  {
    private IAppDataModelDataBase _db;

    ItemsChangeObservableCollection<Participant> _participants;
    DatabaseDelegatorParticipant _participantsDelegatorDB;

    List<(RaceRun, DatabaseDelegatorRaceRun)> _runs;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="db">An object that represents the database backend. Typically a object of type DSVAlpin.Database for DSV-Alpin Databases</param>
    public AppDataModel(IAppDataModelDataBase db)
    {
      //// DB Backend ////
      _db = db;

      //// Particpants ////
      _participants = _db.GetParticipants();
      // Get notification if a participant got changed / added / removed and trigger storage in DB
      _participantsDelegatorDB = new DatabaseDelegatorParticipant(_participants, _db);

      //// RaceRuns ////
      _runs = new List<(RaceRun, DatabaseDelegatorRaceRun)>();

      // TODO: Assuming 2 runs for now
      CreateRaceRuns(2);

      // Fill the data from the DB initially (TODO: to be done better)
      _runs[0].Item1.InsertResults(_db.GetRaceRun(1));
      _runs[1].Item1.InsertResults(_db.GetRaceRun(2));
    }


    /// <summary>
    /// Returns the list of participants
    /// </summary>
    /// <returns>The list of participants</returns>
    public ObservableCollection<Participant> GetParticipants()
    {
      return _participants;
    }

    /// <summary>
    /// Creates the RaceRun structures
    /// </summary>
    /// <param name="numRuns">Number of runs</param>
    public void CreateRaceRuns(int numRuns)
    {
      if (_runs.Count() > 0)
        throw new Exception("Runs already existing");

      for(uint i=0; i<numRuns; i++)
      {
        RaceRun rr = new RaceRun(i+1);
        rr.SetStartListProvider(this);

        // Get notification if a result got modified and trigger storage in DB
        DatabaseDelegatorRaceRun ddrr = new DatabaseDelegatorRaceRun(rr, _db);
        _runs.Add((rr, ddrr));
      }
    }

    /// <summary>
    /// Returns the number of race runs.
    /// </summary>
    public uint GetMaxRun()
    {
      return (uint)_runs.Count;
    }

    /// <summary>
    /// Returns the corresponding run.
    /// </summary>
    /// <param name="run">Run number. Counting starts at 0.</param>
    public RaceRun GetRun(uint run)
    {
      return _runs.ElementAt((int)run).Item1;
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

    private StartListProvider _slp;


    /// <summary>
    /// Returns the run number for this run (round, durchgang)
    /// </summary>
    public uint Run { get { return _run; } }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="run">The run number</param>
    /// <remarks>
    /// This object is usually created by the method AppDataModel.CreateRaceRun()
    /// </remarks>
    /// 
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
    public System.Collections.IEnumerable GetStartListV()
    {
      return _slp.GetStartList();
    }

    public void SetStartListProvider(AppDataModel dm)
    {
      _slp = new StartListProvider(dm, _startList);
    }

    public ItemsChangeObservableCollection<RunResult> GetOnTrackList()
    {
      return _onTrack;
    }

    public ItemsChangeObservableCollection<RunResult> GetResultList()
    {
      return _results;
    }


    public void SetTimeMeasurement(Participant participant, TimeSpan? startTime, TimeSpan? finishTime)
    {
      RunResult result = _results.SingleOrDefault(r => r._participant == participant);

      if (result == null)
        result = new RunResult();

      result._participant = participant;
      result.SetStartFinishTime(startTime, finishTime);

      InsertResult(result);
    }

    public void SetTimeMeasurement(Participant participant, TimeSpan? runTime)
    {
      RunResult result = _results.SingleOrDefault(r => r._participant == participant);

      if (result == null)
        result = new RunResult();

      result._participant = participant;

      result.SetRunTime(runTime);

      InsertResult(result);
    }



    public void InsertResult(RunResult r)
    {
      // Check if already inserted
      if (_results.SingleOrDefault(x => x==r)==null)
        _results.Add(r);

      _UpdateInternals();
    }

    public void InsertResults(List<RunResult> r)
    {
      foreach (var v in r)
        _results.Add(v);

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
  /// Observes the run results and triggers a database store in case time / run results changed
  /// </summary>
  /// <remarks>
  /// Delete not implemented (actually not needed)
  /// </remarks>
  internal class DatabaseDelegatorRaceRun
  {
    private RaceRun _rr;
    private IAppDataModelDataBase _db;

    public DatabaseDelegatorRaceRun(RaceRun rr, IAppDataModelDataBase db)
    {
      _db = db;
      _rr = rr;

      rr.GetResultList().ItemChanged += OnItemChanged;
      rr.GetResultList().CollectionChanged += OnCollectionChanged;
    }

    private void OnItemChanged(object sender, PropertyChangedEventArgs e)
    {
      RunResult result = (RunResult)sender;
      _db.CreateOrUpdateRunResult(_rr, result);
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach(RunResult v in e.NewItems)
            _db.CreateOrUpdateRunResult(_rr, v);
          break;

        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Remove:
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Reset:
          throw new Exception("not implemented");
      }
    }
  }

  /// <summary>
  /// Observes the Patients and triggers a database store if needed
  /// </summary>
  /// <remarks>
  /// Delete not yet implemented
  /// </remarks>
  internal class DatabaseDelegatorParticipant
  {
    private ItemsChangeObservableCollection<Participant> _participants;
    private IAppDataModelDataBase _db;

    public DatabaseDelegatorParticipant(ItemsChangeObservableCollection<Participant> participants, IAppDataModelDataBase db)
    {
      _db = db;
      _participants = participants;

      _participants.ItemChanged += OnItemChanged;
      _participants.CollectionChanged += OnCollectionChanged;
    }

    private void OnItemChanged(object sender, PropertyChangedEventArgs e)
    {
      Participant participant = (Participant)sender;
      _db.CreateOrUpdateParticipant(participant);
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (Participant participant in e.NewItems)
            _db.CreateOrUpdateParticipant(participant);
          break;

        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Remove:
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Reset:
          throw new Exception("not implemented");
      }
    }
  }




  public class StartListProvider
  {
    private AppDataModel _dm;

    ObservableCollection<Participant> _participants;
    CollectionViewSource _startListView;

    ItemsChangeObservableCollection<Participant> _startList;

    public StartListProvider(AppDataModel dm, ItemsChangeObservableCollection<Participant> startList)
    {
      _dm = dm;

      _participants = _dm.GetParticipants();
      _startList = startList;

      _startListView = new CollectionViewSource();
      _startListView.Source = _participants;

      _startListView.SortDescriptions.Clear();
      _startListView.SortDescriptions.Add(new SortDescription(nameof(Participant.StartNumber), ListSortDirection.Ascending));

      _startListView.LiveSortingProperties.Add(nameof(Participant.StartNumber));
      _startListView.IsLiveSortingRequested = true;

      //_startListView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Participant.Class)));
      //_startListView.LiveGroupingProperties.Add(nameof(Participant.Class));
      //_startListView.IsLiveGroupingRequested = true;

      //dgTest1.ItemsSource = testParticipantsSrc.View;

      //string output = Newtonsoft.Json.JsonConvert.SerializeObject(testParticipantsSrc.View);
      //System.Diagnostics.Debug.Write(output);

      //_startListView.View.CollectionChanged += StartListChanged;


      //_participants.CollectionChanged +=
    }

    private void StartListChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
      _startList.Clear();
      foreach (object item in _startListView.View)
      {
        Participant participant = (Participant) item ;
        _startList.Add(participant);
      }

    }

    public System.Collections.IEnumerable GetStartList()
    {
      return _startListView.View;
    }


  }


  /// <summary>
  /// Defines the interface to the actual database engine
  /// </summary>
  /// <remarks>Assuming the database format changes we can simply create another implementation.</remarks>
  public interface IAppDataModelDataBase
  {
    ItemsChangeObservableCollection<Participant> GetParticipants();
    List<RunResult> GetRaceRun(uint run);

    void CreateOrUpdateParticipant(Participant participant);
    void CreateOrUpdateRunResult(RaceRun raceRun, RunResult result);

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
