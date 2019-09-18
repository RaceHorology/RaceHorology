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
  public class AppDataModel : ILiveDateTimeProvider
  {
    private IAppDataModelDataBase _db;

    ItemsChangeObservableCollection<Participant> _participants;
    DatabaseDelegatorParticipant _participantsDelegatorDB;

    List<Race> _races;
    Race _currentRace;
    RaceRun _currentRaceRun;

    private Dictionary<Participant, DateTime> _interactiveTimeMeasurements; // Contains the time measurements made interactively

    public class CurrentRaceEventArgs :  EventArgs
    {
      Race CurrentRace { get; set; }
      RaceRun CurrentRaceRun { get; set; }
      public CurrentRaceEventArgs(Race currentRace, RaceRun currentRaceRun)
      {
        CurrentRace = currentRace;
        CurrentRaceRun = currentRaceRun;
      }
    }
    public delegate void CurrentRaceChangedHandler(object sender, CurrentRaceEventArgs e);

    public event CurrentRaceChangedHandler CurrentRaceChanged;


    #region Implementation of ILiveDateTimeProvider
    public event LiveDateTimeChangedHandler LiveDateTimeChanged;

    TimeSpan _currentDayTimeDelta;

    public void SetCurrentDayTime(TimeSpan currentDayTime)
    {
      _currentDayTimeDelta = (DateTime.Now - DateTime.Today) - currentDayTime;
      var handler = LiveDateTimeChanged;
      handler?.Invoke(this, new LiveDateTimeEventArgs(_currentDayTimeDelta));
    }

    public TimeSpan GetCurrentDayTime()
    {
      return (DateTime.Now - DateTime.Today) - _currentDayTimeDelta;
    }

    #endregion


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


      _races = new List<Race>();

      var races = _db.GetRaces();
      foreach (Race.RaceProperties raceProperties in races)
        _races.Add(new Race(_db, this, raceProperties));

      _currentRace = _races.First();
    }


    /// <summary>
    /// Returns the list of participants
    /// </summary>
    /// <returns>The list of participants</returns>
    public ObservableCollection<Participant> GetParticipants()
    {
      return _participants;
    }

    public List<Race> GetRaces()
    {
      return _races;
    }

    public void SetCurrentRace(Race race)
    {
      if (_currentRace != race)
      {
        _currentRace = race;
        _currentRaceRun = null;

        CurrentRaceChangedHandler handler = CurrentRaceChanged;
        handler?.Invoke(this, new CurrentRaceEventArgs(_currentRace, _currentRaceRun));
      }
    }


    public Race GetCurrentRace()
    {
      return _currentRace;
    }


    public void SetCurrentRaceRun(RaceRun raceRun)
    {
      if (_currentRaceRun != raceRun)
      {
        _currentRaceRun = raceRun;

        CurrentRaceChangedHandler handler = CurrentRaceChanged;
        handler?.Invoke(this, new CurrentRaceEventArgs(_currentRace, _currentRaceRun));
      }
    }


    public RaceRun GetCurrentRaceRun()
    {
      return _currentRaceRun;
    }

    public void InsertInteractiveTimeMeasurement(Participant participant)
    {
      _interactiveTimeMeasurements[participant] = DateTime.Now;
      ParticipantMeasuredHandler handler = ParticipantMeasuredEvent;
      handler?.Invoke(this, participant);
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
    static readonly TimeSpan delta = new TimeSpan(0, 0, 5); // 1 sec

    public delegate void ParticipantMeasuredHandler(object sender, Participant participant);
    public event ParticipantMeasuredHandler ParticipantMeasuredEvent;
  }


  /// <summary>
  /// Represents a race / contest.
  /// A race typically consists out of 1 or 2 runs.
  /// </summary>
  /// 
  public class Race
  {
    public enum ERaceType { DownHill = 0, SuperG = 1, GiantSlalom = 2, Slalom = 3, KOSlalom = 4, ParallelSlalom = 5 };
    public class RaceProperties
    {
      public Race.ERaceType RaceType;
      public uint Runs;
      public string RaceNumber;
      public string Description;
      public DateTime DateStart;
      public DateTime DateResult;
    }


    RaceProperties _properties;

    private AppDataModel _appDataModel;
    private IAppDataModelDataBase _db;
    private ItemsChangeObservableCollection<RaceParticipant> _participants;
    private List<(RaceRun, DatabaseDelegatorRaceRun)> _runs;
    private RaceResultViewProvider _raceResultsProvider;


    public ERaceType RaceType { get { return _properties.RaceType;  } }


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="db">Database for loading and storing</param>
    /// <param name="participants">Participants takeing part in that race</param>
    public Race(IAppDataModelDataBase db, AppDataModel appDataModel, RaceProperties properties)
    {
      // Database Backend
      _db = db;
      _appDataModel = appDataModel;
      _properties = properties;

      // Get initially from DB
      _participants = new ItemsChangeObservableCollection<RaceParticipant>();
      var particpants = _db.GetRaceParticipants(this);
      foreach (var p in particpants)
        _participants.Add(p);

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
        RaceRun rr = new RaceRun(i + 1, this, _appDataModel);

        // Fill the data from the DB initially (TODO: to be done better)
        rr.InsertResults(_db.GetRaceRun(this, i + 1));

        //rr.SetStartListProvider(new FirstRunStartListViewProvider());
        rr.SetStartListProvider(new DSVFirstRunStartListViewProvider(15));

        RaceRunResultViewProvider rVP = new RaceRunResultViewProvider();
        rVP.Init(rr, _appDataModel);
        rr.SetResultViewProvider(rVP);

        // Get notification if a result got modified and trigger storage in DB
        DatabaseDelegatorRaceRun ddrr = new DatabaseDelegatorRaceRun(this, rr, _db);
        _runs.Add((rr, ddrr));

        raceRunsArr[i] = rr;
      }

      _raceResultsProvider = new RaceResultViewProvider();
      _raceResultsProvider.Init(this, _appDataModel);
    }

    /// <summary>
    /// Returns the number of race runs.
    /// </summary>
    public int GetMaxRun()
    {
      return _runs.Count;
    }

    /// <summary>
    /// Returns the corresponding run.
    /// </summary>
    /// <param name="run">Run number. Counting starts at 0.</param>
    public RaceRun GetRun(int run)
    {
      return _runs.ElementAt(run).Item1;
    }

    public RaceRun[] GetRuns()
    {
      RaceRun[] runs = new RaceRun[_runs.Count];
      for (int i = 0; i < _runs.Count; i++)
        runs[i] = GetRun(i);

      return runs;
    }


    /// <summary>
    /// Returns the participants of the race.
    /// </summary>
    /// <returns></returns>
    public ItemsChangeObservableCollection<RaceParticipant> GetParticipants()
    {
      return _participants;
    }

    /// <summary>
    /// Get the particpant by startnumber
    /// </summary>
    /// <returns>The RaceParticipant for the specified startnumber</returns>
    public RaceParticipant GetParticipant(uint startNumber)
    {
      return _participants.FirstOrDefault(p => p.StartNumber == startNumber);
    }

    /// <summary>
    /// Get the race particpant by its original participant
    /// </summary>
    /// <returns>The RaceParticipant for the specified particpant</returns>
    public RaceParticipant GetParticipant(Participant participant)
    {
      return _participants.FirstOrDefault(p => p.Participant == participant);
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
    public RaceResultViewProvider GetResultViewProvider()
    {
      return _raceResultsProvider;
    }

  }


  /// <summary>
  /// Represents a temporary RunResult used for displayig the live time while the participant is running.
  /// </summary>
  /// <remarks>Run time is updated continuously.</remarks>
  public class LiveResult : RunResult
  {
    System.Timers.Timer _timer;
    ILiveDateTimeProvider _timeProvider;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="original"></param>
    public LiveResult(RunResult original, ILiveDateTimeProvider timeProvider) : base(original)
    {
      _timeProvider = timeProvider;

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
        _runTime = _timeProvider.GetCurrentDayTime() - _startTime;
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
    private Race _race;
    private AppDataModel _appDataModel;

    private ItemsChangeObservableCollection<RunResult> _results;  // This list represents the actual results. It is the basis for all other lists.

    private ItemsChangeObservableCollection<LiveResult> _onTrack; // This list only contains the particpants that are on the run.

    private StartListViewProvider _slVP;
    private ResultViewProvider _rvp;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="run">The run number</param>
    /// <remarks>
    /// This object is usually created by the method AppDataModel.CreateRaceRun()
    /// </remarks>
    /// 
    public RaceRun(uint run, Race race, AppDataModel appDataModel)
    {
      _run = run;
      _race = race;
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
      return _slVP.GetView();
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
      return _rvp.GetView(); ;
    }


    public void SetStartListProvider(StartListViewProvider slp)
    {
      slp.Init(_race.GetParticipants());
      _slVP = slp;
    }
    public StartListViewProvider GetStartListProvider()
    {
      return _slVP;
    }

    public void SetResultViewProvider(ResultViewProvider rvp)
    {
      _rvp = rvp;
    }


    /// <summary>
    /// Sets the measured times for a participant based on start and finish time.
    /// </summary>
    /// <param name="participant">The participant</param>
    /// <param name="startTime">Start time</param>
    /// <remarks>startTime and finsihTime can be null. In that case it is stored as not available. A potentially set run time is overwritten with the calculated run time (finish - start).</remarks>
    public void SetStartTime(RaceParticipant participant, TimeSpan? startTime)
    {
      RunResult result = _results.SingleOrDefault(r => r.Participant == participant);

      _appDataModel.InsertInteractiveTimeMeasurement(participant.Participant);

      if (result == null)
        result = new RunResult(participant);

      result.SetStartTime(startTime);

      InsertResult(result);
    }

    /// <summary>
    /// Sets the measured times for a participant based on start and finish time.
    /// </summary>
    /// <param name="participant">The participant</param>
    /// <param name="startTime">Start time</param>
    /// <remarks>startTime and finsihTime can be null. In that case it is stored as not available. A potentially set run time is overwritten with the calculated run time (finish - start).</remarks>
    public void SetFinishTime(RaceParticipant participant, TimeSpan? finishTime)
    {
      RunResult result = _results.SingleOrDefault(r => r.Participant == participant);

      _appDataModel.InsertInteractiveTimeMeasurement(participant.Participant);

      if (result == null)
        result = new RunResult(participant);

      result.SetFinishTime(finishTime);

      InsertResult(result);
    }

    /// <summary>
    /// Sets the measured times for a participant based on start and finish time.
    /// </summary>
    /// <param name="participant">The participant</param>
    /// <param name="startTime">Start time</param>
    /// <param name="finishTime">Finish time</param>
    /// <remarks>startTime and finsihTime can be null. In that case it is stored as not available. A potentially set run time is overwritten with the calculated run time (finish - start).</remarks>
    public void SetStartFinishTime(RaceParticipant participant, TimeSpan? startTime, TimeSpan? finishTime)
    {
      RunResult result = _results.SingleOrDefault(r => r.Participant == participant);

      _appDataModel.InsertInteractiveTimeMeasurement(participant.Participant);

      if (result == null)
        result = new RunResult(participant);

      result.SetStartTime(startTime);
      result.SetFinishTime(finishTime);

      InsertResult(result);
    }


    /// <summary>
    /// Sets the measured times for a participant based on run time (netto)
    /// </summary>
    /// <param name="participant">The participant</param>
    /// <param name="runTime">Run time</param>
    /// <remarks>Can be null. In that case it is stored as not available. Start and end time are set to null.</remarks>
    public void SetRunTime(RaceParticipant participant, TimeSpan? runTime)
    {
      RunResult result = _results.SingleOrDefault(r => r.Participant == participant);

      _appDataModel.InsertInteractiveTimeMeasurement(participant.Participant);

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

    // Helper definition for a participant is on track
    public bool IsOnTrack(RunResult r)
    {
      return r.GetStartTime() != null && r.GetRunTime() == null && _appDataModel.TodayMeasured(r.Participant.Participant);
    }

    // Helper definition for a participant is on track
    public bool IsOrWasOnTrack(RunResult r)
    {
      return r.GetStartTime() != null || r.GetRunTime() != null;
    }

    /// <summary>
    /// Updates internal strucutures based on _results
    /// </summary>
    private void _UpdateInternals()
    {
      // Remove from onTrack list if a result is available (= not on track anymore)
      var itemsToRemove = _onTrack.Where(r => !IsOnTrack(r)).ToList();
      foreach (var itemToRemove in itemsToRemove)
        _onTrack.Remove(itemToRemove);

      // Add to onTrack list if run result is not yet available (= is on track)
      foreach (var r in _results)
        if (IsOnTrack(r))
          if (!_onTrack.Contains(r))
            _onTrack.Add(new LiveResult(r, _appDataModel));
    }

  }




  /// <summary>
  /// Defines the interface to the actual database engine
  /// </summary>
  /// <remarks>Assuming the database format changes we can simply create another implementation.</remarks>
  public interface IAppDataModelDataBase
  {
    ItemsChangeObservableCollection<Participant> GetParticipants();
    List<Race.RaceProperties> GetRaces();
    List<RaceParticipant> GetRaceParticipants(Race race);

    List<RunResult> GetRaceRun(Race race, uint run);

    void CreateOrUpdateParticipant(Participant participant);
    void CreateOrUpdateRunResult(Race race, RaceRun raceRun, RunResult result);

  };

}
