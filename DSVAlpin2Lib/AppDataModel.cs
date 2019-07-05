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

    private Dictionary<Participant, DateTime> _interactiveTimeMeasurements; // Contains the time measurements made interactively



    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="db">An object that represents the database backend. Typically a object of type DSVAlpin.Database for DSV-Alpin Databases</param>
    public AppDataModel(IAppDataModelDataBase db)
    {
      //// DB Backend ////
      _db = db;
      _interactiveTimeMeasurements = new Dictionary<Participant, DateTime>();

      //// Particpants ////
      _participants = _db.GetParticipants();
      // Get notification if a participant got changed / added / removed and trigger storage in DB
      _participantsDelegatorDB = new DatabaseDelegatorParticipant(_participants, _db);

      _race = new Race(_db, this);

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

    public void InsertInteractiveTimeMeasurement(Participant participant)
    {
      _interactiveTimeMeasurements[participant] = DateTime.Now;
    }

    public bool TodayMeasured(Participant participant)
    {
      return _interactiveTimeMeasurements.ContainsKey(participant);
    }

    public bool JustMeasured(Participant participant)
    {
      DateTime measuredAt;
      if (_interactiveTimeMeasurements.TryGetValue(participant, out measuredAt))
      {
        return DateTime.Now - measuredAt < delta;
      }
      return false;
    }
    static readonly TimeSpan delta = new TimeSpan(0, 0, 1); // 1 sec
  }


  /// <summary>
  /// Represents a race / contest.
  /// A race typically consists out of 1 or 2 runs.
  /// </summary>
  /// 
  public class Race
  {
    private AppDataModel _appDataModel;
    private IAppDataModelDataBase _db;
    private ItemsChangeObservableCollection<Participant> _participants;
    private List<(RaceRun, DatabaseDelegatorRaceRun)> _runs;
    private RaceResultProvider _raceResultsProvider;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="db">Database for loading and storing</param>
    /// <param name="participants">Participants takeing part in that race</param>
    public Race(IAppDataModelDataBase db, AppDataModel appDataModel)
    {
      // Database Backend
      _db = db;
      _appDataModel = appDataModel;

      _participants = (ItemsChangeObservableCollection < Participant > )_appDataModel.GetParticipants();

      //// RaceRuns ////
      _runs = new List<(RaceRun, DatabaseDelegatorRaceRun)>();

      // TODO: Assuming 2 runs for now
      CreateRaceRuns(2);
    }

    /// <summary>
    /// Creates the RaceRun structures. After this call, the Races can be accessed and worked with via GetRun().
    /// </summary>
    /// <param name="numRuns">Number of runs</param>
    /// <seealso cref="GetRun()"/>
    public void CreateRaceRuns(int numRuns)
    {
      if (_runs.Count() > 0)
        throw new Exception("Runs already existing");

      RaceRun[] raceRunsArr = new RaceRun[numRuns];
      for (uint i = 0; i < numRuns; i++)
      {
        RaceRun rr = new RaceRun(i + 1, _appDataModel);

        // Fill the data from the DB initially (TODO: to be done better)
        rr.InsertResults(_db.GetRaceRun(i + 1));

        rr.SetStartListProvider(new StartListProvider(this, _participants));
        rr.SetResultViewProvider();

        // Get notification if a result got modified and trigger storage in DB
        DatabaseDelegatorRaceRun ddrr = new DatabaseDelegatorRaceRun(rr, _db);
        _runs.Add((rr, ddrr));

        raceRunsArr[i] = rr;
      }

      _raceResultsProvider = new RaceResultProvider(this, raceRunsArr, _appDataModel);
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

    /// <summary>
    /// Returns the participants of the race.
    /// </summary>
    /// <returns></returns>
    public ItemsChangeObservableCollection<Participant> GetParticipants()
    {
      return _participants;
    }

    /// <summary>
    /// Returns the results of the race.
    /// </summary>
    /// <returns>Race results</returns>
    /// <remarks>The race result is grouped by e.g. class and ordered by the position within the group.</remarks>
    public System.ComponentModel.ICollectionView GetTotalResultView()
    {
      return _raceResultsProvider.GetView();
    }

  }


  /// <summary>
  /// Represents a race result. It contains out of the participant including its run results (run, time, status) and its final position within the group.
  /// </summary>
  public class RaceResultItem : INotifyPropertyChanged
  {
    #region private

    Participant _participant;
    Dictionary<uint, TimeSpan?> _runTimes;
    TimeSpan? _totalTime;
    private uint _position;
    private bool _justModified;


    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="participant">The participant the results belong to.</param>
    public RaceResultItem(Participant participant)
    {
      _participant = participant;
      _runTimes = new Dictionary<uint, TimeSpan?>();
    }

    /// <summary>
    /// Returns the participant
    /// </summary>
    public Participant Participant { get { return _participant; } }

    /// <summary>
    /// Returns the final time (sum or minimum time depending on the race type)
    /// </summary>
    public TimeSpan? TotalTime
    {
      get { return _totalTime; }
      set { _totalTime = value; NotifyPropertyChanged(); }
    }


    /// <summary>
    /// The position within the classement
    /// </summary>
    public uint Position
    {
      get { return _position; }
      set { _position = value; NotifyPropertyChanged(); }
    }

    public bool JustModified
    {
      get { return _justModified; }
      set { if (_justModified != value) { _justModified = value; NotifyPropertyChanged(); } }
    }



    /// <summary>
    /// Returns the separate run results per run
    /// </summary>
    public Dictionary<uint, TimeSpan?> RunTimes { get { return _runTimes; } }


    /// <summary>
    /// Sets the results for one specific run
    /// </summary>
    /// <param name="run">Run number, typically either 1 or 2</param>
    /// <param name="result">The corresponding results</param>
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

  /// <summary>
  /// Represents a temporary RunResult used for displayig the live time while the participant is running.
  /// </summary>
  /// <remarks>Run time is updated continuously.</remarks>
  public class LiveResult : RunResult
  {
    System.Timers.Timer _timer;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="original"></param>
    public LiveResult(RunResult original) : base(original)
    {
      _timer = new System.Timers.Timer(1000);
      _timer.Elapsed += OnTimedEvent;
      _timer.AutoReset = true;
      _timer.Enabled = true;

      CalcRunTime();
    }

    /// <summary>
    /// Callback to update the run time continuously
    /// </summary>
    private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
    {
      CalcRunTime();
    }

    /// <summary>
    /// Calculates and updates the run time internally
    /// </summary>
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
    private uint _run;
    AppDataModel _appDataModel;

    private ItemsChangeObservableCollection<RunResult> _results;  // This list represents the actual results. It is the basis for all other lists.

    private ItemsChangeObservableCollection<LiveResult> _onTrack; // This list only contains the particpants that are on the run.

    private StartListProvider _slp;
    private ResultViewProvider _rvp;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="run">The run number</param>
    /// <remarks>
    /// This object is usually created by the method AppDataModel.CreateRaceRun()
    /// </remarks>
    /// 
    public RaceRun(uint run, AppDataModel appDataModel)
    {
      _run = run;
      _appDataModel = appDataModel;

      _onTrack = new ItemsChangeObservableCollection<LiveResult>();
      _results = new ItemsChangeObservableCollection<RunResult>();
    }


    /// <summary>
    /// Returns the run number for this run (round, durchgang)
    /// </summary>
    public uint Run { get { return _run; } }

    /// <summary>
    /// Returns the start list
    /// </summary>
    /// <returns>Start list</returns>
    public ICollectionView GetStartList()
    {
      return _slp.GetStartList();
    }

    public ItemsChangeObservableCollection<LiveResult> GetOnTrackList()
    {
      return _onTrack;
    }

    /// <summary>
    /// Returns the internal results.
    /// </summary>
    public ItemsChangeObservableCollection<RunResult> GetResultList()
    {
      return _results;
    }

    /// <summary>
    /// Returns the results to display including position.
    /// </summary>
    public ICollectionView GetResultView()
    {
      return _rvp.GetResultView(); ;
    }


    public void SetStartListProvider(StartListProvider slp)
    {
      _slp = slp;
    }

    public void SetResultViewProvider()
    {
      _rvp = new ResultViewProvider(_results, _appDataModel);
    }


    /// <summary>
    /// Sets the measured times for a participant based on start and finish time.
    /// </summary>
    /// <param name="participant">The participant</param>
    /// <param name="startTime">Start time</param>
    /// <param name="finishTime">Finish time</param>
    /// <remarks>startTime and finsihTime can be null. In that case it is stored as not available. A potentially set run time is overwritten with the calculated run time (finish - start).</remarks>
    public void SetTimeMeasurement(Participant participant, TimeSpan? startTime, TimeSpan? finishTime)
    {
      RunResult result = _results.SingleOrDefault(r => r.Participant == participant);

      _appDataModel.InsertInteractiveTimeMeasurement(participant);

      if (result == null)
        result = new RunResult(participant);

      result.SetStartFinishTime(startTime, finishTime);

      InsertResult(result);
    }

    /// <summary>
    /// Sets the measured times for a participant based on run time (netto)
    /// </summary>
    /// <param name="participant">The participant</param>
    /// <param name="runTime">Run time</param>
    /// <remarks>Can be null. In that case it is stored as not available. Start and end time are set to null.</remarks>
    public void SetTimeMeasurement(Participant participant, TimeSpan? runTime)
    {
      RunResult result = _results.SingleOrDefault(r => r.Participant == participant);

      _appDataModel.InsertInteractiveTimeMeasurement(participant);

      if (result == null)
        result = new RunResult(participant);

      result.SetRunTime(runTime);

      InsertResult(result);
    }



    protected void InsertResult(RunResult r)
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

    /// <summary>
    /// Updates internal strucutures based on _results
    /// </summary>
    private void _UpdateInternals()
    {
      // Helper definition for a participant is on track
      bool IsOnTrack(RunResult r)
      {
        //FIXME: Consider whether added in programm and not DB
        return r.GetStartTime() != null && r.GetRunTime() == null && _appDataModel.TodayMeasured(r.Participant);
      }

      // Remove from onTrack list if a result is available (= not on track anymore)
      var itemsToRemove = _onTrack.Where(r => !IsOnTrack(r)).ToList();
      foreach (var itemToRemove in itemsToRemove)
        _onTrack.Remove(itemToRemove);

      // Add to onTrack list if run result is not yet available (= is on track)
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


  public class RunResultWithPosition :  RunResult
  {
    private uint _position;
    private bool _justModified;

    public RunResultWithPosition(RunResult result) : base(result)
    {
    }

    /// <summary>
    /// The position within the classement
    /// </summary>
    public uint Position
    {
      get { return _position; }
      set { _position = value; NotifyPropertyChanged(); }
    }

    public bool JustModified
    {
      get { return _justModified; }
      set { if (_justModified != value) { _justModified = value; NotifyPropertyChanged(); } }
    }
  }

  public class ResultViewProvider
  {
    ItemsChangeObservableCollection<RunResult> _originalResults;
    AppDataModel _appDataModel;
    ItemsChangeObservableCollection<RunResultWithPosition> _results;
    System.Collections.Generic.IComparer<RunResultWithPosition> _sorter;

    CollectionViewSource _resultListView;

    public class RuntimeSorter : System.Collections.Generic.IComparer<RunResultWithPosition>
    {
      public int Compare(RunResultWithPosition rrX, RunResultWithPosition rrY)
      {
        TimeSpan? tX = rrX.Runtime;
        TimeSpan? tY = rrY.Runtime;
        
        // Sort by grouping (class or group or ...)
        // TODO: Shall be configurable
        int classCompare = rrX.Class.CompareTo(rrY.Class);
        if (classCompare != 0)
          return classCompare;

        // Sort by time
        if (tX == null && tY == null)
          return 0;

        if (tX != null && tY == null)
          return -1;

        if (tX == null && tY != null)
          return 1;

        return TimeSpan.Compare((TimeSpan)tX, (TimeSpan)tY);
      }
    }


    public ResultViewProvider(ItemsChangeObservableCollection<RunResult> results, AppDataModel appDataModel)
    {
      _sorter = new RuntimeSorter();

      _originalResults = results;
      _appDataModel = appDataModel;

      _originalResults.CollectionChanged += OnOriginalResultsChanged;
      _originalResults.ItemChanged += OnOriginalResultItemChanged;

      _resultListView = new CollectionViewSource();

      _results = new ItemsChangeObservableCollection<RunResultWithPosition>();
      foreach(var result in _originalResults)
        _results.Add(new RunResultWithPosition(result));
      ResortResults();

      _resultListView.Source = _results;


      _resultListView.SortDescriptions.Clear();
      //_resultListView.SortDescriptions.Add(new SortDescription(nameof(RunResult.Runtime), ListSortDirection.Ascending));

      //_resultListView.Filter += new FilterEventHandler(delegate (object s, FilterEventArgs ea) { ea.Accepted = ((RunResult)ea.Item).Runtime != null; });
      //_resultListView.LiveFilteringProperties.Add(nameof(RunResult.Runtime));
      //_resultListView.IsLiveFilteringRequested = true;

      // TODO: Check this out
      ListCollectionView llview = _resultListView.View as ListCollectionView;
      //llview.CustomSort = new RuntimeSorter();

      //_resultListView.LiveSortingProperties.Add(nameof(RunResult.Runtime));
      //_resultListView.IsLiveSortingRequested = true;

      _resultListView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Participant.Class)));
      _resultListView.LiveGroupingProperties.Add(nameof(Participant.Class));
      _resultListView.IsLiveGroupingRequested = true;
    }

    public System.ComponentModel.ICollectionView GetResultView()
    {
      return _resultListView.View;
    }

    void OnOriginalResultsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
        foreach (INotifyPropertyChanged item in e.OldItems)
        {
          // Remove from _results
          RunResult result = (RunResult)item;
          var itemsToRemove = _results.Where(r => r == result).ToList();
          foreach (var itemToRemove in itemsToRemove)
            _results.Remove(itemToRemove);
        }

      if (e.NewItems != null)
        foreach (INotifyPropertyChanged item in e.NewItems)
        {
          // Add to results
          RunResult result = (RunResult)item;
          _results.Add(new RunResultWithPosition(result));
        }

      ResortResults();
    }

    void OnOriginalResultItemChanged(object sender, PropertyChangedEventArgs e)
    {
      RunResult result = (RunResult)sender;
      RunResultWithPosition rrWP = _results.FirstOrDefault(r => r.Participant == result.Participant);

      if (rrWP==null)
        _results.Add(new RunResultWithPosition(result));
      else
        rrWP.UpdateRunResult(result);

      // Anything to update?
      ResortResults();
    }


    void ResortResults()
    {
      // TODO: Could be much more efficient; consumes O(nlogn * n); but underlaying data structure _results needs to be changed to support in-place sorting (e.g. an array)
      // Sort:
      // 1. by Class
      // 2. by Time

      var sortedResults = _results.ToList();
      sortedResults.Sort(_sorter);
      _results.Clear();

      uint curPosition = 1;
      uint samePosition = 1;
      string curClass = "";
      TimeSpan? lastTime = null;
      foreach (var sortedItem in sortedResults)
      {
        if (sortedItem.Class != curClass)
        {
          curClass = sortedItem.Class;
          curPosition = 1;
          lastTime = null;
        }

        if (sortedItem.Runtime != null)
        {
          sortedItem.Position = curPosition;

          // Same position in case same time
          if (sortedItem.Runtime == lastTime)//< TimeSpan.FromMilliseconds(9))
          {
            samePosition++;
          }
          else
          {
            curPosition += samePosition;
            samePosition = 1;
          }
          lastTime = sortedItem.Runtime;
        }
        else
        {
          sortedItem.Position = 0;
        }

        // Set the JustModified flag to highlight new results
        sortedItem.JustModified = _appDataModel.JustMeasured(sortedItem.Participant);

        _results.Add(sortedItem);
      }
    }

    static readonly TimeSpan delta = new TimeSpan(0, 0, 1); // 1 sec
  }



  public class RaceResultProvider
  {
    Race _race;
    RaceRun[] _raceRuns;
    AppDataModel _appDataModel;
    ItemsChangeObservableCollection<RaceResultItem> _raceResults;
    System.Collections.Generic.IComparer<RaceResultItem> _sorter = new TotalTimeSorter();
    CollectionViewSource _raceResultsView;


    public class TotalTimeSorter : System.Collections.Generic.IComparer<RaceResultItem>
    {
      public int Compare(RaceResultItem rrX, RaceResultItem rrY)
      {
        TimeSpan? tX = rrX.TotalTime;
        TimeSpan? tY = rrY.TotalTime;

        // Sort by grouping (class or group or ...)
        // TODO: Shall be configurable
        int classCompare = rrX.Participant.Class.CompareTo(rrY.Participant.Class);
        if (classCompare != 0)
          return classCompare;

        // Sort by time
        if (tX == null && tY == null)
          return 0;

        if (tX != null && tY == null)
          return -1;

        if (tX == null && tY != null)
          return 1;

        return TimeSpan.Compare((TimeSpan)tX, (TimeSpan)tY);
      }
    }



    public RaceResultProvider(Race race, RaceRun[] rr, AppDataModel appDataModel)
    {
      _race = race;
      _raceRuns = rr;
      _appDataModel = appDataModel;
      _raceResults = new ItemsChangeObservableCollection<RaceResultItem>();

      foreach (RaceRun r in _raceRuns)
      {
        r.GetResultList().CollectionChanged += OnResultListCollectionChanged;
        OnResultListCollectionChanged(r.GetResultList(), new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, r.GetResultList().ToList()));
      }


      _raceResultsView = new CollectionViewSource();

      _raceResultsView.Source = _raceResults;

      _raceResultsView.SortDescriptions.Clear();
      //_raceResultsView.SortDescriptions.Add(new SortDescription(nameof(RaceResultItem.TotalTime), ListSortDirection.Ascending));

      //_raceResultsView.Filter += new FilterEventHandler(delegate (object s, FilterEventArgs ea) { ea.Accepted = ((RaceResultItem)ea.Item).TotalTime != null; });
      //_raceResultsView.LiveFilteringProperties.Add(nameof(RaceResultItem.TotalTime));
      //_raceResultsView.IsLiveFilteringRequested = true;

      // TODO: Seems like sorting null at the end does not work ... visible if the Filter above is turned off ...
      ListCollectionView llview = _raceResultsView.View as ListCollectionView;
      //llview.CustomSort = new TotalTimeSorter();

      //_raceResultsView.LiveSortingProperties.Add(nameof(RaceResultItem.TotalTime));
      //_raceResultsView.IsLiveSortingRequested = true;

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
      {
        UpdateResultsFor(rr.Participant);
        ResortResults();
      }
    }

    private void OnResultListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
      {
        foreach (INotifyPropertyChanged item in e.OldItems)
        {
          item.PropertyChanged -= OnRunResultItemChanged;
          RunResult rr = item as RunResult;
          UpdateResultsFor(rr?.Participant);
        }
        ResortResults();
      }

      if (e.NewItems != null)
      {
        foreach (INotifyPropertyChanged item in e.NewItems)
        {
          item.PropertyChanged += OnRunResultItemChanged;
          RunResult rr = item as RunResult;
          UpdateResultsFor(rr?.Participant);
        }
        ResortResults();
      }
    }


    private void UpdateAll()
    {
      foreach (Participant p in _race.GetParticipants())
        UpdateResultsFor(p);

      ResortResults();
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

    void ResortResults()
    {
      // TODO: Could be much more efficient; consumes O(nlogn * n); but underlaying data structure _results needs to be changed to support in-place sorting (e.g. an array)
      // Sort:
      // 1. by Class
      // 2. by Time

      var sortedResults = _raceResults.ToList();
      sortedResults.Sort(_sorter);
      _raceResults.Clear();

      uint curPosition = 1;
      uint samePosition = 1;
      string curClass = "";
      TimeSpan? lastTime = null;
      foreach (var sortedItem in sortedResults)
      {
        if (sortedItem.Participant.Class != curClass)
        {
          curClass = sortedItem.Participant.Class;
          curPosition = 1;
          lastTime = null;
        }

        if (sortedItem.TotalTime != null)
        {
          sortedItem.Position = curPosition;

          // Same position in case same time
          if (sortedItem.TotalTime == lastTime)//< TimeSpan.FromMilliseconds(9))
          {
            samePosition++;
          }
          else
          {
            curPosition += samePosition;
            samePosition = 1;
          }
          lastTime = sortedItem.TotalTime;
        }
        else
        {
          sortedItem.Position = 0;
        }

        // Set the JustModified flag to highlight new results
        sortedItem.JustModified = _appDataModel.JustMeasured(sortedItem.Participant);

        _raceResults.Add(sortedItem);
      }
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
