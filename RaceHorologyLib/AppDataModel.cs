/*
 *  Copyright (C) 2019 - 2021 by Sven Flossmann
 *  
 *  This file is part of Race Horology.
 *
 *  Race Horology is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  any later version.
 * 
 *  Race Horology is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with Race Horology.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  Diese Datei ist Teil von Race Horology.
 *
 *  Race Horology ist Freie Software: Sie können es unter den Bedingungen
 *  der GNU Affero General Public License, wie von der Free Software Foundation,
 *  Version 3 der Lizenz oder (nach Ihrer Wahl) jeder neueren
 *  veröffentlichten Version, weiter verteilen und/oder modifizieren.
 *
 *  Race Horology wird in der Hoffnung, dass es nützlich sein wird, aber
 *  OHNE JEDE GEWÄHRLEISTUNG, bereitgestellt; sogar ohne die implizite
 *  Gewährleistung der MARKTFÄHIGKEIT oder EIGNUNG FÜR EINEN BESTIMMTEN ZWECK.
 *  Siehe die GNU Affero General Public License für weitere Details.
 *
 *  Sie sollten eine Kopie der GNU Affero General Public License zusammen mit diesem
 *  Programm erhalten haben. Wenn nicht, siehe <https://www.gnu.org/licenses/>.
 * 
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace RaceHorologyLib
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

    ObservableCollection<ParticipantGroup> _particpantGroups;
    DatabaseDelegatorGroups _particpantGroupsDelegatorDB;
    
    ObservableCollection<ParticipantClass> _particpantClasses;
    DatabaseDelegatorClasses _particpantClassesDelegatorDB;

    ObservableCollection<ParticipantCategory> _particpantCategories;
    DatabaseDelegatorCategories _particpantCategoriesDelegatorDB;

    ItemsChangeObservableCollection<Participant> _participants;
    DatabaseDelegatorParticipant _participantsDelegatorDB;

    DatabaseDelegatorCompetition _competitionDelegatorDB;

    ObservableCollection<Race> _races;
    Race _currentRace;
    RaceRun _currentRaceRun;

    private Dictionary<Participant, DateTime> _interactiveTimeMeasurements; // Contains the time measurements made interactively

    public class CurrentRaceEventArgs :  EventArgs
    {
      public Race CurrentRace { get; set; }
      public RaceRun CurrentRaceRun { get; set; }
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

      _particpantGroups = new ObservableCollection<ParticipantGroup>(_db.GetParticipantGroups());
      _particpantGroups.CollectionChanged += OnGroupCollectionChanged;
      _particpantClasses = new ObservableCollection<ParticipantClass>(_db.GetParticipantClasses());
      _particpantClasses.CollectionChanged += OnClassCollectionChanged;
      _particpantCategories = new ObservableCollection<ParticipantCategory>(_db.GetParticipantCategories());
      _particpantCategories.CollectionChanged += OnCategoryCollectionChanged;

      _particpantGroupsDelegatorDB = new DatabaseDelegatorGroups(this, _db);
      _particpantClassesDelegatorDB = new DatabaseDelegatorClasses(this, _db);
      _particpantCategoriesDelegatorDB = new DatabaseDelegatorCategories(this, _db);


      //// Particpants ////
      _participants = _db.GetParticipants();
      // Get notification if a participant got changed / added / removed and trigger storage in DB
      _participantsDelegatorDB = new DatabaseDelegatorParticipant(_participants, _db);

      _races = new ObservableCollection<Race>();

      var races = _db.GetRaces();
      foreach (Race.RaceProperties raceProperties in races)
        _races.Add(new Race(_db, this, raceProperties));
      // Get notification if a race got changed / added / removed and trigger storage in DB
      _competitionDelegatorDB = new DatabaseDelegatorCompetition(this, _db);

      if (_races.Count > 0)
        _currentRace = _races.First();
    }

    public IAppDataModelDataBase GetDB()
    {
      return _db;
    }

    /// <summary>
    /// Returns the list of participants
    /// </summary>
    /// <returns>The list of participants</returns>
    public ObservableCollection<Participant> GetParticipants()
    {
      return _participants;
    }


    public ObservableCollection<ParticipantCategory> GetParticipantCategories()
    {
      return _particpantCategories;
    }

    public ObservableCollection<ParticipantGroup> GetParticipantGroups()
    {
      return _particpantGroups;
    }

    public ObservableCollection<ParticipantClass> GetParticipantClasses()
    {
      return _particpantClasses;
    }


    public ObservableCollection<Race> GetRaces()
    {
      return _races;
    }
    public Race GetRace(int idx)
    {
      if (0 <= idx && idx < _races.Count)
        return _races[idx];

      return null;
    }

    public Race AddRace(Race.RaceProperties raceProperties)
    {
      // Ensure this type of race is not yet existing
      Race raceExisting = _races.FirstOrDefault(r => r.RaceType == raceProperties.RaceType);
      if (raceExisting != null)
      {
        return null;
      }

      Race race = new Race(_db, this, raceProperties);
      _races.Add(race);

      return race;
    }


    public bool RemoveRace(Race race)
    {
      return _races.Remove(race);
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
        if (_currentRaceRun != null && _currentRaceRun.GetRace() != _currentRace)
          throw (new Exception("The RaceRun that shall be set as current race run does not match to the current Race."));

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


    #region Internal - Fix Consistencies

    private void OnGroupCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      void removeGroupFromClasses(ParticipantGroup g)
      {
        foreach(var c in _particpantClasses)
          if (c.Group == g)
            c.Group = null;
      }

      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Remove:
          foreach (ParticipantGroup v in e.OldItems)
            removeGroupFromClasses(v);
          break;
      }
    }

    private void OnClassCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      void removeClassFromParticipants(ParticipantClass c)
      {
        foreach (var p in _participants)
          if (p.Class == c)
            p.Class = null;
      }

      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Remove:
          foreach (ParticipantClass v in e.OldItems)
            removeClassFromParticipants(v);
          break;
      }
    }

    private void OnCategoryCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      void removeCategoryFromClasses(ParticipantCategory g)
      {
        foreach (var c in _particpantClasses)
          if (c.Sex == g)
            c.Sex = null;
      }

      void removeCategoryFromParticipants(ParticipantCategory c)
      {
        foreach (var p in _participants)
          if (p.Sex == c)
            p.Sex = null;
      }

      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Remove:
          foreach (ParticipantCategory c in e.OldItems)
          {
            removeCategoryFromClasses(c);
            removeCategoryFromParticipants(c);
          }
          break;
      }
    }

    #endregion


  }



  public class AdditionalRaceProperties
  {
    public class Person
    {
      public string Name { get; set; }
      public string Club { get; set; }

      public bool IsEmpty() { return string.IsNullOrEmpty(Name); }
    }

    public class RaceRunProperties
    {
      public Person CoarseSetter { get; set; } = new Person();
      public Person Forerunner1 { get; set; } = new Person();
      public Person Forerunner2 { get; set; } = new Person();
      public Person Forerunner3 { get; set; } = new Person();

      public int Gates { get; set; }
      public int Turns { get; set; }
      public string StartTime { get; set; }
    }


    public string Location { get; set; }
    public string RaceNumber { get; set; }
    public string Description { get; set; }

    public DateTime? DateStartList { get; set; }
    public DateTime? DateResultList { get; set; }

    public string Analyzer { get; set; }
    public string Organizer { get; set; }
    public Person RaceReferee { get; set; } = new Person(); // Schiedsrichter
    public Person RaceManager { get; set; } = new Person(); // Rennleiter
    public Person TrainerRepresentative { get; set; } = new Person(); // Trainer Vertreter

    public string CoarseName { get; set; }
    public int CoarseLength { get; set; } // m
    public string CoarseHomologNo { get; set; }

    public int StartHeight { get; set; } // m
    public int FinishHeight { get; set; } // m

    public RaceRunProperties RaceRun1 { get; set; } = new RaceRunProperties();
    public RaceRunProperties RaceRun2 { get; set; } = new RaceRunProperties();

    public string Weather { get; set; }
    public string Snow { get; set; }
    public string TempStart { get; set; }
    public string TempFinish { get; set; }
  }


  /// <summary>
  /// Represents a race / contest.
  /// A race typically consists out of 1 or 2 runs.
  /// </summary>
  /// 
  public class Race
  {
    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public enum ERaceType { DownHill = 0, SuperG = 1, GiantSlalom = 2, Slalom = 3, KOSlalom = 4, ParallelSlalom = 5 };
    public class RaceProperties
    {
      public Race.ERaceType RaceType;
      public uint Runs;
    }

    // Mainly race decription parameters
    RaceProperties _properties;
    AdditionalRaceProperties _addProperties;

    // Mainly ViewConfiguration (sorting, grouing, ...)
    RaceConfiguration _raceConfiguration;
    
    private AppDataModel _appDataModel;
    private IAppDataModelDataBase _db;
    private DatabaseDelegatorRaceParticipant _raceParticipantDBDelegator;
    private ItemsChangeObservableCollection<RaceParticipant> _participants;
    private List<(RaceRun, DatabaseDelegatorRaceRun)> _runs;
    private RaceResultViewProvider _raceResultsProvider;


    public ERaceType RaceType { get { return _properties.RaceType;  } }
    public string RaceNumber {  get { return _addProperties?.RaceNumber; } }
    public string Description { get { return _addProperties?.Description; } }
    public DateTime? DateStartList { get { return _addProperties?.DateStartList; } }
    public DateTime? DateResultList { get { return _addProperties?.DateResultList; } }

    public RaceConfiguration RaceConfiguration
    {
      get { return _raceConfiguration; }
      set { _raceConfiguration = value.Copy(); StoreRaceConfig(); }
    }


    public AdditionalRaceProperties AdditionalProperties
    {
      get { return _addProperties; }
      set { _addProperties = value; _db.StoreRaceProperties(this, _addProperties); }
    }

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

      _addProperties = _db.GetRaceProperties(this);

      LoadRaceConfig();
      // Ensure no inconsistencies
      _raceConfiguration.Runs = (int)_properties.Runs;

      // Get initially from DB
      _participants = new ItemsChangeObservableCollection<RaceParticipant>();
      var particpants = _db.GetRaceParticipants(this);
      foreach (var p in particpants)
        _participants.Add(p);

      //// RaceRuns ////
      _runs = new List<(RaceRun, DatabaseDelegatorRaceRun)>();

      _raceParticipantDBDelegator = new DatabaseDelegatorRaceParticipant(this, _db);

      CreateRaceRuns((int)properties.Runs);

      ViewConfigurator viewConfigurator = new ViewConfigurator(this);
      viewConfigurator.ConfigureRace(this);
    }


    #region Configuration

    protected void StoreRaceConfig()
    {
      string configFile = GetRaceConfigFilepath();
      try
      {
        string configJSON = Newtonsoft.Json.JsonConvert.SerializeObject(_raceConfiguration, Newtonsoft.Json.Formatting.Indented);

        System.IO.File.WriteAllText(configFile, configJSON);
      }
      catch (Exception e)
      {
        logger.Info(e, "could not write race config {name}", configFile);
      }
    }

    protected string GetRaceConfigFilepath()
    {
      string configFile = System.IO.Path.Combine(
        _appDataModel.GetDB().GetDBPathDirectory(), 
        _appDataModel.GetDB().GetDBFileName() + "_" + _properties.RaceType.ToString() + ".config");
      return configFile;
    }

    protected void LoadRaceConfig()
    {
      _raceConfiguration = new RaceConfiguration();
      string configFile = GetRaceConfigFilepath();
      try
      {
        string configJSON = System.IO.File.ReadAllText(configFile);

        Newtonsoft.Json.JsonConvert.PopulateObject(configJSON, _raceConfiguration);
      }
      catch(Exception e)
      {
        logger.Info(e, "could not load race config {name}", configFile);
      }
    }

    public bool IsFieldActive(string field)
    {
      return _raceConfiguration.ActiveFields.Contains(field);
    }

    #endregion


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

        // Get notification if a result got modified and trigger storage in DB
        DatabaseDelegatorRaceRun ddrr = new DatabaseDelegatorRaceRun(this, rr, _db);
        _runs.Add((rr, ddrr));

        raceRunsArr[i] = rr;
      }
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
      if (0 <= run && run < GetMaxRun())
        return _runs.ElementAt(run).Item1;
      
      return null;
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
    /// Adds a particpant to the race
    /// </summary>
    /// <param name="participant">The particpant to add</param>
    /// <returns>The the corresponding RaceParticipant object</returns>
    public RaceParticipant AddParticipant(Participant participant, uint startnumber= 0, double points = -1)
    {
      RaceParticipant raceParticipant = GetParticipant(participant);

      if (raceParticipant == null)
      {
        raceParticipant = new RaceParticipant(this, participant, startnumber, points);
        _participants.Add(raceParticipant);
      }

      return raceParticipant;
    }

    /// <summary>
    /// Removes a particpant from the race
    /// </summary>
    /// <param name="participant">The particpant to add</param>
    /// <returns>The the corresponding RaceParticipant object</returns>
    public void RemoveParticipant(Participant participant)
    {
      RaceParticipant raceParticipant = GetParticipant(participant);
      _participants.Remove(raceParticipant);
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


    public void SetResultViewProvider(RaceResultViewProvider raceVP)
    {
      _raceResultsProvider = raceVP;
    }


    public AppDataModel GetDataModel()
    {
      return _appDataModel;
    }


    public override string ToString()
    {
      return RaceType.ToString();
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

    protected TimeSpan? _liveRunTime;

    RunResult _original;

    public RunResult OriginalResult { get { return _original; } }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="original"></param>
    public LiveResult(RunResult original, ILiveDateTimeProvider timeProvider) : base(original)
    {
      _original = original;

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
        _liveRunTime = _timeProvider.GetCurrentDayTime() - _startTime;
        NotifyPropertyChanged(propertyName: nameof(Runtime));
      }
    }


    public override TimeSpan? GetRunTime(bool calculateIfNotStored = true, bool considerResultCode = true)
    {
      if (calculateIfNotStored)
        return _liveRunTime;

      return base.GetRunTime(calculateIfNotStored, considerResultCode);
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
    private bool _hasRealResults;

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
      _hasRealResults = false;

      // Ensure the results always are in sync with participants
      _race.GetParticipants().CollectionChanged += onParticipantsChanged;
      findOrCreateRunResults(_race.GetParticipants());
    }


    /// <summary>
    /// Returns the run number for this run (round, durchgang)
    /// </summary>
    public uint Run { get { return _run; } }

    /// <summary>
    /// Returns the Race this RaceRun belongs to.
    /// </summary>
    /// <returns>The Race this run belongs to.</returns>
    public Race GetRace() { return _race; }

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

    public ResultViewProvider GetResultViewProvider()
    {
      return _rvp;
    }


    /// <summary>
    /// Sets the measured times for a participant based on start and finish time.
    /// </summary>
    /// <param name="participant">The participant</param>
    /// <param name="startTime">Start time</param>
    /// <remarks>startTime and finsihTime can be null. In that case it is stored as not available. A potentially set run time is overwritten with the calculated run time (finish - start).</remarks>
    public void SetStartTime(RaceParticipant participant, TimeSpan? startTime)
    {
      RunResult result = findOrCreateRunResult(participant);

      _appDataModel.InsertInteractiveTimeMeasurement(participant.Participant);

      result.SetStartTime(startTime);

      _UpdateInternals();
    }

    /// <summary>
    /// Sets the measured times for a participant based on start and finish time.
    /// </summary>
    /// <param name="participant">The participant</param>
    /// <param name="startTime">Start time</param>
    /// <remarks>startTime and finsihTime can be null. In that case it is stored as not available. A potentially set run time is overwritten with the calculated run time (finish - start).</remarks>
    public void SetFinishTime(RaceParticipant participant, TimeSpan? finishTime)
    {
      RunResult result = findOrCreateRunResult(participant);

      _appDataModel.InsertInteractiveTimeMeasurement(participant.Participant);

      result.SetFinishTime(finishTime);

      _UpdateInternals();
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
      RunResult result = findOrCreateRunResult(participant);

      _appDataModel.InsertInteractiveTimeMeasurement(participant.Participant);

      result.SetStartTime(startTime);
      result.SetFinishTime(finishTime);

      _UpdateInternals();
    }


    /// <summary>
    /// Sets the measured times for a participant based on run time (netto)
    /// </summary>
    /// <param name="participant">The participant</param>
    /// <param name="runTime">Run time</param>
    /// <remarks>Can be null. In that case it is stored as not available. Start and end time are set to null.</remarks>
    public void SetRunTime(RaceParticipant participant, TimeSpan? runTime)
    {
      RunResult result = findOrCreateRunResult(participant);

      _appDataModel.InsertInteractiveTimeMeasurement(participant.Participant);

      result.SetRunTime(runTime);

      _UpdateInternals();
    }


    public void SetResultCode(RaceParticipant participant, RunResult.EResultCode rc)
    {
      RunResult result = findOrCreateRunResult(participant);

      result.ResultCode = rc;

      _UpdateInternals();
    }


    public void SetResultCode(RaceParticipant participant, RunResult.EResultCode rc, string disqualText)
    {
      RunResult result = findOrCreateRunResult(participant);

      result.ResultCode = rc;
      result.DisqualText = disqualText;

      _UpdateInternals();
    }


    private RunResult findOrCreateRunResult(RaceParticipant participant)
    {
      RunResult result = _results.SingleOrDefault(r => r.Participant == participant);
      if (result == null)
      {
        result = new RunResult(participant);
        _results.Add(result);
      }

      return result;
    }


    private void findOrCreateRunResults(IEnumerable<RaceParticipant> participants)
    {
      foreach (RaceParticipant rp in participants)
        findOrCreateRunResult(rp);
    }


    public RunResult DeleteRunResult(RaceParticipant participant)
    {
      RunResult result = _results.SingleOrDefault(r => r.Participant == participant);
      if (result != null)
      {
        _results.Remove(result);
      }

      return result;
    }


    public void DeleteRunResults()
    {
      while (_results.Count > 0)
        _results.RemoveAt(0);
    }


    protected void onParticipantsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.NewItems != null)
        foreach (RaceParticipant rp in e.NewItems)
          findOrCreateRunResult(rp);

      if (e.OldItems != null)
        foreach (RaceParticipant rp in e.OldItems)
          DeleteRunResult(rp);
    }



    public void InsertResults(List<RunResult> r)
    {
      foreach (var source in r)
      {
        var target = findOrCreateRunResult(source.Participant);
        target.UpdateRunResult(source);

      }

      _UpdateInternals();
    }


    // Helper definition for a participant is on track
    public bool IsOnTrack(RunResult r)
    {
      return r.GetStartTime() != null && r.GetFinishTime() == null && r.ResultCode == RunResult.EResultCode.Normal && _appDataModel.TodayMeasured(r.Participant.Participant);
    }

    // Helper definition for a participant is on track
    public bool IsOrWasOnTrack(RunResult r)
    {
      return r.GetStartTime() != null || r.GetRunTime() != null || (r.ResultCode != RunResult.EResultCode.NotSet && r.ResultCode != RunResult.EResultCode.Normal);
    }

    public bool IsOrWasOnTrack(RaceParticipant rp)
    {
      RunResult result = _results.SingleOrDefault(r => r.Participant == rp);
      if (result != null)
        return IsOrWasOnTrack(result);

      return false;
    }

    public bool HasResults()
    {
      return _hasRealResults;
    }



    public delegate void OnTrackChangedHandler(object o, RaceParticipant participantEnteredTrack, RaceParticipant participantLeftTrack, RunResult currentRunResult);
    public event OnTrackChangedHandler OnTrackChanged;


    /// <summary>
    /// Updates internal strucutures based on _results
    /// </summary>
    private void _UpdateInternals()
    {
      var results = _results.ToArray();

      var firstResult = results.FirstOrDefault(r => r.ResultCode != RunResult.EResultCode.NotSet);
      _hasRealResults = results.Length > 0 && firstResult != null;

      // Remove from onTrack list if a result is available (= not on track anymore)
      var itemsToRemove = _onTrack.Where(r => !IsOnTrack(r.OriginalResult)).ToList();
      foreach (var itemToRemove in itemsToRemove)
      {
        _onTrack.Remove(itemToRemove);

        OnTrackChangedHandler handler = OnTrackChanged;
        handler?.Invoke(this, null, itemToRemove.Participant, itemToRemove);
      }

      // Add to onTrack list if run result is not yet available (= is on track)
      var shallBeOnTrack = results.Where(r => IsOnTrack(r)).ToList();

      foreach (var r in shallBeOnTrack)
      {
        if (_onTrack.SingleOrDefault(o => o.Participant == r.Participant) == null)
        {
          _onTrack.Add(new LiveResult(r, _appDataModel));

          OnTrackChangedHandler handler = OnTrackChanged;
          handler?.Invoke(this, r.Participant, null, r);
        }
      }
    }

  }




  /// <summary>
  /// Defines the interface to the actual database engine
  /// </summary>
  /// <remarks>Assuming the database format changes we can simply create another implementation.</remarks>
  public interface IAppDataModelDataBase
  {
    string GetDBPath();
    string GetDBFileName();
    string GetDBPathDirectory();


    ItemsChangeObservableCollection<Participant> GetParticipants();

    List<ParticipantGroup> GetParticipantGroups();
    List<ParticipantClass> GetParticipantClasses();
    List<ParticipantCategory> GetParticipantCategories();

    List<Race.RaceProperties> GetRaces();
    List<RaceParticipant> GetRaceParticipants(Race race);

    List<RunResult> GetRaceRun(Race race, uint run);

    AdditionalRaceProperties GetRaceProperties(Race race);
    void StoreRaceProperties(Race race, AdditionalRaceProperties props);

    void CreateOrUpdateParticipant(Participant participant);
    void RemoveParticipant(Participant participant);

    void CreateOrUpdateClass(ParticipantClass c);
    void RemoveClass(ParticipantClass c);
    void CreateOrUpdateGroup(ParticipantGroup g);
    void RemoveGroup(ParticipantGroup g);
    void CreateOrUpdateCategory(ParticipantCategory c);
    void RemoveCategory(ParticipantCategory c);


    void CreateOrUpdateRaceParticipant(RaceParticipant participant);
    void RemoveRaceParticipant(RaceParticipant raceParticipant);

    void CreateOrUpdateRunResult(Race race, RaceRun raceRun, RunResult result);
    void DeleteRunResult(Race race, RaceRun raceRun, RunResult result);

    void UpdateRace(Race race, bool active);


    void StoreKeyValue(string key, string value);
    string GetKeyValue(string key);
  };

}
