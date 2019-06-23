using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

    Race _race;


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

      _race = new Race(_db, _participants);
    }


    /// <summary>
    /// Returns the list of participants
    /// </summary>
    /// <returns>The list of participants</returns>
    public ObservableCollection<Participant> GetParticipants()
    {
      return _participants;
    }

    public Race GetRace()
    {
      return _race;
    }


  }



  public class Race
  {
    private IAppDataModelDataBase _db;

    ItemsChangeObservableCollection<Participant> _participants;

    List<(RaceRun, DatabaseDelegatorRaceRun)> _runs;
    RaceResultProvider _raceResultsProvider;

    public Race(IAppDataModelDataBase db, ItemsChangeObservableCollection<Participant> participants)
    {
      // Database Backend
      _db = db;

      _participants = participants;

      //// RaceRuns ////
      _runs = new List<(RaceRun, DatabaseDelegatorRaceRun)>();

      // TODO: Assuming 2 runs for now
      CreateRaceRuns(2);
    }

    /// <summary>
    /// Creates the RaceRun structures
    /// </summary>
    /// <param name="numRuns">Number of runs</param>
    public void CreateRaceRuns(int numRuns)
    {
      if (_runs.Count() > 0)
        throw new Exception("Runs already existing");

      RaceRun[] raceRunsArr = new RaceRun[numRuns];
      for (uint i = 0; i < numRuns; i++)
      {
        RaceRun rr = new RaceRun(i + 1);

        // Fill the data from the DB initially (TODO: to be done better)
        rr.InsertResults(_db.GetRaceRun(i + 1));

        rr.SetStartListProvider(new StartListProvider(this, _participants));
        rr.SetResultViewProvider();

        // Get notification if a result got modified and trigger storage in DB
        DatabaseDelegatorRaceRun ddrr = new DatabaseDelegatorRaceRun(rr, _db);
        _runs.Add((rr, ddrr));

        raceRunsArr[i] = rr;
      }

      _raceResultsProvider = new RaceResultProvider(this, raceRunsArr);
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


    public ItemsChangeObservableCollection<Participant> GetParticipants()
    {
      return _participants;
    }

    public System.ComponentModel.ICollectionView GetTotalResultView()
    {
      return _raceResultsProvider.GetView();
    }

  }


  public class RaceResultItem : INotifyPropertyChanged
  {
    public Participant Participant { get { return _participant; } }

    public Dictionary<uint, TimeSpan?> RunTimes { get { return _runTimes; } }

    public TimeSpan? TotalTime
    {
      get { return _totalTime; }
      set { _totalTime = value; NotifyPropertyChanged(); }
    }


    Participant _participant;
    Dictionary<uint, TimeSpan?> _runTimes;
    TimeSpan? _totalTime;


    public RaceResultItem(Participant participant)
    {
      _participant = participant;
      _runTimes = new Dictionary<uint, TimeSpan?>();
    }


    public void SetRunResult(uint run, RunResult result)
    {
      _runTimes[run] = result?.Runtime;

      NotifyPropertyChanged(nameof(RunTimes));
    }



    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion


  }

  public class LiveResult : RunResult
  {
    System.Timers.Timer _timer;

    public LiveResult(RunResult original) : base(original)
    {
      _timer = new System.Timers.Timer(1000);
      _timer.Elapsed += OnTimedEvent;
      _timer.AutoReset = true;
      _timer.Enabled = true;

      CalcRunTime();
    }

    private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
    {
      CalcRunTime();
    }

    private void CalcRunTime()
    {
      if (_startTime != null)
      {
        _runTime = (DateTime.Now - DateTime.Today) - _startTime;
        NotifyPropertyChanged(propertyName: nameof(Runtime));
      }
    }

  }

  /// <summary>
  /// Represents a race run. Typically a race consists out of two race runs.
  /// </summary>
  public class RaceRun
  {
    uint _run;
    ItemsChangeObservableCollection<RunResult> _results; // This list represents the actual results

    ItemsChangeObservableCollection<LiveResult> _onTrack; // TODO: This list only contains the particpants that are on the run (might get removed later)

    private StartListProvider _slp;
    private ResultViewProvider _rvp;


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

      _onTrack = new ItemsChangeObservableCollection<LiveResult>();
      _results = new ItemsChangeObservableCollection<RunResult>();
    }


    public ICollectionView GetStartList()
    {
      return _slp.GetStartList();
    }

    public void SetStartListProvider(StartListProvider slp)
    {
      _slp = slp;
    }

    public ItemsChangeObservableCollection<LiveResult> GetOnTrackList()
    {
      return _onTrack;
    }

    public ItemsChangeObservableCollection<RunResult> GetResultList()
    {
      return _results;
    }

    public void SetResultViewProvider()
    {
      _rvp = new ResultViewProvider(_results);
    }

    public ICollectionView GetResultView()
    {
      return _rvp.GetResultView(); ;
    }



    public void SetTimeMeasurement(Participant participant, TimeSpan? startTime, TimeSpan? finishTime)
    {
      RunResult result = _results.SingleOrDefault(r => r._participant == participant);

      if (result == null)
        result = new RunResult(participant);

      result.SetStartFinishTime(startTime, finishTime);

      InsertResult(result);
    }

    public void SetTimeMeasurement(Participant participant, TimeSpan? runTime)
    {
      RunResult result = _results.SingleOrDefault(r => r._participant == participant);

      if (result == null)
        result = new RunResult(participant);

      result.SetRunTime(runTime);

      InsertResult(result);
    }



    public void InsertResult(RunResult r)
    {
      // Check if already inserted
      if (_results.SingleOrDefault(x => x == r) == null)
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
      bool IsOnTrack(RunResult r)
      {
        return r.GetStartTime() != null && r.GetRunTime() == null;
      }

      // Remove from onTrack list if a result is available
      var itemsToRemove = _onTrack.Where(r => !IsOnTrack(r)).ToList();
      foreach (var itemToRemove in itemsToRemove)
        _onTrack.Remove(itemToRemove);

      // Add to onTrack list if run result is not yet available
      foreach (var r in _results)
        if (IsOnTrack(r))
          if (!_onTrack.Contains(r))
            _onTrack.Add(new LiveResult(r));
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
          foreach (RunResult v in e.NewItems)
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
    private Race _race;

    ObservableCollection<Participant> _participants;
    CollectionViewSource _startListView;

    public StartListProvider(Race race, ObservableCollection<Participant> participants)
    {
      _race = race;

      _participants = participants;

      _startListView = new CollectionViewSource();

      _startListView.Source = _participants;

      _startListView.SortDescriptions.Clear();
      _startListView.SortDescriptions.Add(new SortDescription(nameof(Participant.StartNumber), ListSortDirection.Ascending));

      _startListView.LiveSortingProperties.Add(nameof(Participant.StartNumber));
      _startListView.IsLiveSortingRequested = true;

      //_startListView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Participant.Class)));
      //_startListView.LiveGroupingProperties.Add(nameof(Participant.Class));
      //_startListView.IsLiveGroupingRequested = true;
    }

    public System.ComponentModel.ICollectionView GetStartList()
    {
      return _startListView.View;
    }
  }


  public class ResultViewProvider
  {
    ItemsChangeObservableCollection<RunResult> _results;
    CollectionViewSource _resultListView;

    public class RuntimeSorter : System.Collections.IComparer
    {
      public int Compare(object x, object y)
      {
        RunResult rrX = x as RunResult;
        RunResult rrY = y as RunResult;

        if (rrX.Runtime == null && rrY.Runtime == null)
          return 0;

        if (rrX.Runtime != null && rrY.Runtime == null)
          return -1;

        if (rrX.Runtime == null && rrY.Runtime != null)
          return 1;

        return TimeSpan.Compare((TimeSpan)rrX.Runtime, (TimeSpan)rrY.Runtime);
      }
    }


    public ResultViewProvider(ItemsChangeObservableCollection<RunResult> results)
    {
      _results = results;

      _resultListView = new CollectionViewSource();

      _resultListView.Source = _results;

      _resultListView.SortDescriptions.Clear();
      //_resultListView.SortDescriptions.Add(new SortDescription(nameof(RunResult.Runtime), ListSortDirection.Ascending));

      _resultListView.Filter += new FilterEventHandler(delegate (object s, FilterEventArgs ea) { ea.Accepted = ((RunResult)ea.Item).Runtime != null; });
      _resultListView.LiveFilteringProperties.Add(nameof(RunResult.Runtime));
      _resultListView.IsLiveFilteringRequested = true;

      // TODO: Check this out
      ListCollectionView llview = _resultListView.View as ListCollectionView;
      llview.CustomSort = new RuntimeSorter();

      _resultListView.LiveSortingProperties.Add(nameof(RunResult.Runtime));
      _resultListView.IsLiveSortingRequested = true;

      _resultListView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Participant.Class)));
      _resultListView.LiveGroupingProperties.Add(nameof(Participant.Class));
      _resultListView.IsLiveGroupingRequested = true;
    }

    public System.ComponentModel.ICollectionView GetResultView()
    {
      return _resultListView.View;
    }
  }



  public class RaceResultProvider
  {
    Race _race;
    RaceRun[] _raceRuns;
    ItemsChangeObservableCollection<RaceResultItem> _raceResults;
    CollectionViewSource _raceResultsView;


    public class TotalTimeSorter : System.Collections.IComparer
    {
      public int Compare(object x, object y)
      {
        RaceResultItem rrX = x as RaceResultItem;
        RaceResultItem rrY = y as RaceResultItem;

        if (rrX.TotalTime == null && rrY.TotalTime == null)
          return 0;

        if (rrX.TotalTime != null && rrY.TotalTime == null)
          return -1;

        if (rrX.TotalTime == null && rrY.TotalTime != null)
          return 1;

        return TimeSpan.Compare((TimeSpan)rrX.TotalTime, (TimeSpan)rrY.TotalTime);
      }
    }



    public RaceResultProvider(Race race, RaceRun[] rr)
    {
      _race = race;
      _raceRuns = rr;
      _raceResults = new ItemsChangeObservableCollection<RaceResultItem>();

      foreach (RaceRun r in _raceRuns)
      {
        r.GetResultList().CollectionChanged += OnResultListCollectionChanged;
      }


      _raceResultsView = new CollectionViewSource();

      _raceResultsView.Source = _raceResults;

      _raceResultsView.SortDescriptions.Clear();
      //_raceResultsView.SortDescriptions.Add(new SortDescription(nameof(RaceResultItem.TotalTime), ListSortDirection.Ascending));

      _raceResultsView.Filter += new FilterEventHandler(delegate (object s, FilterEventArgs ea) { ea.Accepted = ((RaceResultItem)ea.Item).TotalTime != null; });
      _raceResultsView.LiveFilteringProperties.Add(nameof(RaceResultItem.TotalTime));
      _raceResultsView.IsLiveFilteringRequested = true;

      // TODO: Seems like sorting null at the end does not work ... visible if the Filter above is turned off ...
      ListCollectionView llview = _raceResultsView.View as ListCollectionView;
      llview.CustomSort = new TotalTimeSorter();

      _raceResultsView.LiveSortingProperties.Add(nameof(RaceResultItem.TotalTime));
      _raceResultsView.IsLiveSortingRequested = true;

      _raceResultsView.GroupDescriptions.Add(new PropertyGroupDescription("Participant.Class"));
      _raceResultsView.LiveGroupingProperties.Add("Participant.Class");
      _raceResultsView.IsLiveGroupingRequested = true;

      UpdateAll();
    }


    public System.ComponentModel.ICollectionView GetView()
    {
      return _raceResultsView.View;
    }


    private void OnRunResultItemChanged(object sender, PropertyChangedEventArgs e)
    {
      RunResult rr = sender as RunResult;

      if (rr != null)
        UpdateResultsFor(rr.Participant);
    }

    private void OnResultListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
        foreach (INotifyPropertyChanged item in e.OldItems)
          item.PropertyChanged -= OnRunResultItemChanged;

      if (e.NewItems != null)
        foreach (INotifyPropertyChanged item in e.NewItems)
        {
          item.PropertyChanged += OnRunResultItemChanged;
          RunResult rr = item as RunResult;
          UpdateResultsFor(rr?.Participant);
        }
    }


    private void UpdateAll()
    {
      foreach (Participant p in _race.GetParticipants())
        UpdateResultsFor(p);
    }

    private void UpdateResultsFor(Participant participant)
    {
      RaceResultItem rri = _raceResults.SingleOrDefault(x => x.Participant == participant);
      if (rri == null)
      {
        rri = new RaceResultItem(participant);
        _raceResults.Add(rri);
      }

      // Look for the sub-result
      Dictionary<uint, RunResult> results = new Dictionary<uint, RunResult>();
      foreach (RaceRun run in _raceRuns)
      {
        RunResult result = run.GetResultList().SingleOrDefault(x => x.Participant == participant);
        results.Add(run.Run, result);
      }

      // Combine and update the race result
      foreach (var res in results)
        rri.SetRunResult(res.Key, res.Value);
      rri.TotalTime = MinimumTime(results);
    }

    TimeSpan? MinimumTime(Dictionary<uint, RunResult> results)
    {
      TimeSpan? minTime = null;

      foreach (var res in results)
      {
        if (res.Value != null && res.Value.Runtime != null)
        {
          if (minTime == null || TimeSpan.Compare((TimeSpan)res.Value.Runtime, (TimeSpan)minTime) < 0)
            minTime = res.Value.Runtime;
        }
      }

      return minTime;
    }

    TimeSpan? SumTime(Dictionary<uint, RunResult> results)
    {
      TimeSpan sumTime = new TimeSpan(0);

      foreach (var res in results)
      {
        if (res.Value != null && res.Value.Runtime != null)
        {
          sumTime += (TimeSpan)res.Value.Runtime;
        }
      }

      return sumTime;
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
