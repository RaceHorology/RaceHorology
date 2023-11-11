/*
 *  Copyright (C) 2019 - 2023 by Sven Flossmann
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
  public class AppDataModel : ILiveDateTimeProvider, INotifyPropertyChanged
  {
    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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

    // Main configuration which is used by the different races, contains mainly ViewConfiguration (sorting, grouing, ...)
    RaceConfiguration _globalRaceConfig;

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

      loadRaceConfig();

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


    /// <summary>
    /// Closes the data model
    /// 
    /// The object cannot be used anymore after that call.
    /// </summary>
    public void Close()
    {
      if (_db == null) // If database isn't set anymore, there is nothing to close
        return;

      _interactiveTimeMeasurements = null;

      _currentRace = null;
      _currentRaceRun = null;

      _competitionDelegatorDB = null;
      _participantsDelegatorDB = null;
      _particpantCategoriesDelegatorDB = null;
      _particpantClassesDelegatorDB = null;
      _particpantGroupsDelegatorDB = null;

      _races = null;
      _particpantGroups = null;
      _particpantClasses = null;
      _particpantCategories = null;
      _participants = null;

      // Close data base and set to null
      _db.Close();
      _db = null;
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

    #region Race Management
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
    #endregion


    #region Time Measurement specifics
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
    static readonly TimeSpan delta = new TimeSpan(0, 0, 5); // 5 sec

    public delegate void ParticipantMeasuredHandler(object sender, Participant participant);
    public event ParticipantMeasuredHandler ParticipantMeasuredEvent;

    #endregion


    #region Global Race Configuration

    public RaceConfiguration GlobalRaceConfig
    {
      get 
      { 
        return _globalRaceConfig; 
      }
      
      set 
      { 
        _globalRaceConfig = value.Copy(); 
        storeRaceConfig(); 
        NotifyPropertyChanged(); 
      }
    }

    protected void storeRaceConfig()
    {
      try
      {
        string configJSON = Newtonsoft.Json.JsonConvert.SerializeObject(_globalRaceConfig, Newtonsoft.Json.Formatting.Indented);
        _db.StoreKeyValue("GlobalRaceConfig", configJSON);

        storeRaceConfig_FixDSVAlpinType();
      }
      catch (Exception e)
      {
        logger.Info(e, "could store global race config");
      }
    }

    protected void loadRaceConfig()
    {
      _globalRaceConfig = new RaceConfiguration();
      try
      {
        string configJSONDB = _db.GetKeyValue("GlobalRaceConfig");
        if (!string.IsNullOrEmpty(configJSONDB))
          Newtonsoft.Json.JsonConvert.PopulateObject(configJSONDB, _globalRaceConfig);
        else
        {
          loadRaceConfig_BasedOnDSVAlpinType();
        }
      }
      catch (Exception e)
      {
        logger.Info(e, "could not load global race config");
      }

      NotifyPropertyChanged("GlobalRaceConfig");
    }

    protected void storeRaceConfig_FixDSVAlpinType()
    {
      if (_db is Database dsvAlpinDB)
      {
        if (_globalRaceConfig.InternalDSVAlpinCompetitionTypeWrite != null)
        {
          CompetitionProperties compProps = dsvAlpinDB.GetCompetitionProperties();
          compProps.Type = (CompetitionProperties.ECompetitionType)_globalRaceConfig.InternalDSVAlpinCompetitionTypeWrite;
          compProps.WithPoints = _globalRaceConfig.ActiveFields.Contains("Points");
          compProps.FieldActiveClub = _globalRaceConfig.ActiveFields.Contains("Club");
          compProps.FieldActiveCode = _globalRaceConfig.ActiveFields.Contains("Code");
          compProps.FieldActiveYear = _globalRaceConfig.ActiveFields.Contains("Year");
          compProps.FieldActiveNation = _globalRaceConfig.ActiveFields.Contains("Nation");
          dsvAlpinDB.UpdateCompetitionProperties(compProps);

          // Enusre that the bewerbsnummer is set correctly
          if ( compProps.Type == CompetitionProperties.ECompetitionType.DSV_Points 
            || compProps.Type == CompetitionProperties.ECompetitionType.DSV_NoPoints
            || compProps.Type == CompetitionProperties.ECompetitionType.DSV_SchoolPoints
            || compProps.Type == CompetitionProperties.ECompetitionType.DSV_SchoolNoPoints )
          dsvAlpinDB.EnsureDSVAlpinBewerbsnummer( _races );
        }
      }
    }

    protected void loadRaceConfig_BasedOnDSVAlpinType()
    {
      Dictionary<CompetitionProperties.ECompetitionType, string> mapDSVAlpinType2RaceConfig
        = new Dictionary<CompetitionProperties.ECompetitionType, string>
              {
              {CompetitionProperties.ECompetitionType.FIS_Women, "FIS Rennen Women" },
              {CompetitionProperties.ECompetitionType.FIS_Men, "FIS Rennen Men" },
              {CompetitionProperties.ECompetitionType.DSV_Points, "DSV Erwachsene" },
              {CompetitionProperties.ECompetitionType.DSV_NoPoints, "DSV Erwachsene" },
              {CompetitionProperties.ECompetitionType.DSV_SchoolPoints, "DSV Schüler U14-U16" },
              {CompetitionProperties.ECompetitionType.DSV_SchoolNoPoints, "DSV Schüler U14-U16" },
              {CompetitionProperties.ECompetitionType.VersatilityPoints, "Vielseitigkeit (Punkte)" },  // BestOfTwo-Points
              {CompetitionProperties.ECompetitionType.VersatilityNoPoints, "Vielseitigkeit (Nicht-Punkte)" },
              {CompetitionProperties.ECompetitionType.ClubInternal_Sum, "Vereinsrennen - Summe" },
              {CompetitionProperties.ECompetitionType.ClubInternal_BestRun, "Vereinsrennen - BestOfTwo" },
              //{CompetitionProperties.ECompetitionType.Parallel, "???" },
              //{CompetitionProperties.ECompetitionType.Sledding_Points, "???" },
              //{CompetitionProperties.ECompetitionType.Sledding_NoPoints, "???" },
        };

      var raceConfigurationPresets = new RaceConfigurationPresets(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"raceconfigpresets"));

      if (_db is Database dsvAlpinDB)
      {
        CompetitionProperties p = dsvAlpinDB.GetCompetitionProperties();

        string defaultConfigName = null;
        if (mapDSVAlpinType2RaceConfig.TryGetValue(p.Type, out defaultConfigName))
        {
          RaceConfiguration defaultConfig = null;
          if (raceConfigurationPresets.GetConfigurations().TryGetValue(defaultConfigName, out defaultConfig))
          {
            if (defaultConfig != null)
              _globalRaceConfig = defaultConfig.Copy();
          }
        }
      }
    }

    #endregion

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


    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null)
        handler(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }


  /// <summary>
  /// Represents some properties for the competition i.e., top level properties directly on the AppDataModel.
  /// Note: This mainly originates from the DSVAlpin data base and might change if database backend changes.
  /// </summary>
  public class CompetitionProperties
  {
    public enum ECompetitionType
    {
      FIS_Women = 0,
      FIS_Men = 1,
      DSV_Points = 2,
      DSV_NoPoints = 3,
      DSV_SchoolPoints = 4,
      DSV_SchoolNoPoints = 5,
      VersatilityPoints = 6,
      VersatilityNoPoints = 7,
      ClubInternal_Sum = 8,
      Parallel = 9,
      Sledding_Points = 10,
      Sledding_NoPoints = 11,
      ClubInternal_BestRun = 12
    };


    public string Name { get; set; } = "";
    public ECompetitionType Type { get; set; } = ECompetitionType.ClubInternal_Sum;
    public bool WithPoints { get; set; }
    // Note: Location is already part of AdditionalRaceProperties
    public string Nation { get; set; }
    public uint Saeson { get; set; }

    public bool KlassenWertung { get; set; }
    public bool MannschaftsWertung { get; set; }
    public bool ZwischenZeit { get; set; }
    public bool FreierListenKopf { get; set; }
    public bool FISSuperCombi { get; set; }

    public bool FieldActiveYear { get; set; }
    public bool FieldActiveClub { get; set; }
    public bool FieldActiveNation { get; set; }
    public bool FieldActiveCode { get; set; }

    public double Nenngeld { get; set; }
  }


  public class AdditionalRaceProperties
  {
    public class Person
    {
      public string Name { get; set; }
      public string Club { get; set; }

      public bool IsEmpty() { return string.IsNullOrEmpty(Name); }

      static public bool Equals(Person p1, Person p2)
      {
        return string.Equals(p1?.Name, p2?.Name) && string.Equals(p1?.Club, p2?.Club);
      }
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


      static public bool Equals(RaceRunProperties rrp1, RaceRunProperties rrp2)
      {
        return Person.Equals(rrp1?.CoarseSetter, rrp2?.CoarseSetter)
          && Person.Equals(rrp1?.Forerunner1, rrp2?.Forerunner1)
          && Person.Equals(rrp1?.Forerunner2, rrp2?.Forerunner2)
          && Person.Equals(rrp1?.Forerunner3, rrp2?.Forerunner3)
          && rrp1?.Gates == rrp2?.Gates
          && rrp1?.Turns == rrp2?.Turns
          && rrp1?.StartTime == rrp2?.StartTime;
      }
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


    static public bool Equals(AdditionalRaceProperties p1, AdditionalRaceProperties p2)
    {
      return string.Equals(p1?.Location, p2?.Location)
        && string.Equals(p1?.RaceNumber, p2?.RaceNumber)
        && string.Equals(p1?.Description, p2?.Description)
        && p1?.DateStartList == p2?.DateStartList
        && p1?.DateResultList == p2?.DateResultList
        && string.Equals(p1?.Analyzer, p2?.Analyzer)
        && string.Equals(p1?.Organizer, p2?.Organizer)
        && Person.Equals(p1?.RaceReferee, p2?.RaceReferee)
        && Person.Equals(p1?.RaceManager, p2?.RaceManager)
        && Person.Equals(p1?.TrainerRepresentative, p2?.TrainerRepresentative)
        && string.Equals(p1?.CoarseName, p2?.CoarseName)
        && p1?.CoarseLength == p2?.CoarseLength
        && string.Equals(p1?.CoarseHomologNo, p2?.CoarseHomologNo)
        && p1?.StartHeight == p2?.StartHeight
        && p1?.FinishHeight == p2?.FinishHeight
        && RaceRunProperties.Equals(p1?.RaceRun1, p2?.RaceRun1)
        && RaceRunProperties.Equals(p1?.RaceRun2, p2?.RaceRun2)
        && string.Equals(p1?.Weather, p2?.Weather)
        && string.Equals(p1?.Snow, p2?.Snow)
        && string.Equals(p1?.TempStart, p2?.TempStart)
        && string.Equals(p1?.TempFinish, p2?.TempFinish);
    }
  }


  /// <summary>
  /// Represents a race / contest.
  /// A race typically consists out of 1 or 2 runs.
  /// </summary>
  /// 
  public class Race : INotifyPropertyChanged
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
    bool _raceConfigurationIsLocal;


    private AppDataModel _appDataModel;
    private IAppDataModelDataBase _db;
    private DatabaseDelegatorRace _raceDBDelegator;
    private DatabaseDelegatorRaceParticipant _raceParticipantDBDelegator;
    private ItemsChangeObservableCollection<RaceParticipant> _participants;
    private List<(RaceRun, DatabaseDelegatorRaceRun)> _runs;
    private RaceResultViewProvider _raceResultsProvider;


    public ERaceType RaceType { get { return _properties.RaceType; } }
    public string RaceNumber { get { return _addProperties?.RaceNumber; } }
    public string Description { get { return _addProperties?.Description; } }
    public DateTime? DateStartList { get { return _addProperties?.DateStartList; } }
    public DateTime? DateResultList { get { return _addProperties?.DateResultList; } }

    private bool _isConsistent; // Member storing value for property IsConsistent
    /// <summary>
    /// True in case there aren't any inconsistencies in the data, false otherwise.
    /// Inconsistencies in data can be:
    /// - Start numbers are not correctly assigned (either one startnumber is 0 or a start nnumber is used twice)
    /// </summary>
    public bool IsConsistent
    {
      get { return _isConsistent; }
      private set { if (value != _isConsistent) { _isConsistent = value; NotifyPropertyChanged(); } }
    }


    private bool _isComplete; // Member storing value for property IsComplete
    /// <summary>
    /// True in case all runs have been completed.
    /// False if one or more runs haven't been completed.
    /// </summary>
    public bool IsComplete
    {
      get { return _isComplete; }
      private set { if (_isComplete != value) { _isComplete = value; NotifyPropertyChanged(); } }
    }

    public void SetTimingDeviceInfo(DeviceInfo deviceInfo)
    {
      TimingDevice = deviceInfo.PrettyName;
    }

    string _timingDevice;
    public string TimingDevice {
      get { return _timingDevice; } 
      protected set
      {
        if (_timingDevice != value)
        {
          _timingDevice = value;
          _db.StoreTimingDevice(this, _timingDevice);
        }
      }
    }

    public RaceConfiguration RaceConfiguration
    {
      get 
      { 
        return _raceConfiguration;
      }
      
      set 
      {
        if (value != null)
        {
          _raceConfiguration = value.Copy();
          _raceConfigurationIsLocal = true;
          storeRaceConfig();
        }
        else
        {
          _raceConfiguration = null;
          storeRaceConfig();
          loadGlobalRaceConfig();
        }

        UpdateNumberOfRuns((uint)_raceConfiguration.Runs);

        NotifyPropertyChanged();
      }
    }

    public bool IsRaceConfigurationLocal
    {
      get { return _raceConfigurationIsLocal; }
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
      _timingDevice = _db.GetTimingDevice(this);

      loadRaceConfig();
      // Ensure no inconsistencies
      _raceConfiguration.Runs = (int)_properties.Runs;

      // Get initially from DB
      _participants = new ItemsChangeObservableCollection<RaceParticipant>();
      var particpants = _db.GetRaceParticipants(this);
      foreach (var p in particpants)
        _participants.Add(p);

      // Watch for changes on the RaceConfiguration
      _appDataModel.PropertyChanged += appDataModel_PropertyChanged;

      // Watch for changes on actual participants and its properties => check internal state
      _participants.ItemChanged += onRaceParticipants_ItemChanged;
      _participants.CollectionChanged += onRaceParticipants_CollectionChanged;
      checkConsistency();

      // Watch for changes on main particpants => react accordingly
      _appDataModel.GetParticipants().CollectionChanged += onParticipants_CollectionChanged;

      //// RaceRuns ////
      _runs = new List<(RaceRun, DatabaseDelegatorRaceRun)>();

      createRaceRuns((int)properties.Runs);

      // Store participant specific things
      _raceParticipantDBDelegator = new DatabaseDelegatorRaceParticipant(this, _db);
      // Store and Race related things
      _raceDBDelegator = new DatabaseDelegatorRace(this, db);
      
      ViewConfigurator viewConfigurator = new ViewConfigurator(this);
      viewConfigurator.ConfigureRace(this);
    }

    private void appDataModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "GlobalRaceConfig")
      {
        loadRaceConfig();
        UpdateNumberOfRuns((uint)_raceConfiguration.Runs);
      }
    }

    private void onParticipants_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
        foreach (Participant p in e.OldItems)
          RemoveParticipant(p);
    }


    #region Configuration

    protected void storeRaceConfig()
    {
      try
      {
        if (_raceConfiguration != null)
        {
          string configJSON = Newtonsoft.Json.JsonConvert.SerializeObject(_raceConfiguration, Newtonsoft.Json.Formatting.Indented);
          _db.StoreKeyValue(getRaceConfigKey(), configJSON);
        }
        else
        {
          _db.StoreKeyValue(getRaceConfigKey(), "");
        }
      }
      catch (Exception e)
      {
        logger.Info(e, "could store race config");
      }
    }

    private string getRaceConfigFilepath()
    {
      string configFile = System.IO.Path.Combine(
        _appDataModel.GetDB().GetDBPathDirectory(),
        _appDataModel.GetDB().GetDBFileName() + "_" + _properties.RaceType.ToString() + ".config");
      return configFile;
    }

    protected string getRaceConfigKey()
    {
      return string.Format("RaceConfig_{0}", _properties.RaceType.ToString());
    }

    protected void loadRaceConfig()
    {
      if (!loadLocalRaceConfig())
        loadGlobalRaceConfig();

      NotifyPropertyChanged("RaceConfiguration");
    }

    protected bool loadLocalRaceConfig()
    {
      try
      {
        string configJSON;

        string configJSONDB = _db.GetKeyValue(getRaceConfigKey());
        if (!string.IsNullOrEmpty(configJSONDB))
          configJSON = configJSONDB;
        else
        {
          string configFile = getRaceConfigFilepath();
          configJSON = System.IO.File.ReadAllText(configFile);
        }

        if (!string.IsNullOrEmpty(configJSON))
        {
          RaceConfiguration loadedConfig = new RaceConfiguration();
          Newtonsoft.Json.JsonConvert.PopulateObject(configJSON, loadedConfig);
          _raceConfiguration = loadedConfig;
          _raceConfigurationIsLocal = true;
          return true;
        }
      }
      catch (Exception e)
      {
        logger.Info(e, "could not load race config");
      }

      return false;
    }

    protected bool loadGlobalRaceConfig()
    {
      _raceConfiguration = _appDataModel.GlobalRaceConfig.Copy();
      _raceConfigurationIsLocal = false;
      return true;
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
    private void createRaceRuns(int numRuns)
    {
      if (_runs.Count() > 0)
        throw new Exception("Runs already existing");

      for (uint i = 0; i < numRuns; i++)
        AddRaceRun();
    }


    /// <summary>
    /// Creates a new RaceRun structure. 
    /// </summary>
    /// <seealso cref="GetRun()"/>
    public void AddRaceRun()
    {
      uint run = (uint)GetMaxRun() + 1;
      RaceRun rr = new RaceRun(run, this, _appDataModel);

      // Fill the data from the DB initially (TODO: to be done better)
      rr.InsertResults(_db.GetRaceRun(this, run));
      rr.InsertTimestamps(_db.GetTimestamps(this, run));

      // Get notification if a result got modified and trigger storage in DB
      DatabaseDelegatorRaceRun ddrr = new DatabaseDelegatorRaceRun(this, rr, _db);
      _runs.Add((rr, ddrr));

      // Observe properties
      rr.PropertyChanged += raceRun_PropertyChanged;

      RunsChanged?.Invoke(this, null);
    }

    private void raceRun_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e?.PropertyName == "IsComplete")
        updateComplete();
    }

    private void updateComplete()
    {
      bool complete = true;
      foreach (var rr in _runs)
        complete = complete && rr.Item1.IsComplete;

      IsComplete = complete;
    }

    /// <summary>
    /// Deletes the RaceRun with highest run number. 
    /// </summary>
    /// <seealso cref="GetRun()"/>
    public void DeleteRaceRun()
    {
      if (_runs.Count == 0)
        return;

      (RaceRun, DatabaseDelegatorRaceRun) runItem = _runs[_runs.Count - 1];

      RaceRun rr = runItem.Item1;
      DatabaseDelegatorRaceRun ddrr = runItem.Item2;

      // Un-Observe properties
      rr.PropertyChanged -= raceRun_PropertyChanged;

      // TODO
      //rr.Dispose();
      //ddrr.Dispose();

      _runs.RemoveAt(_runs.Count - 1);

      RunsChanged?.Invoke(this, null);
    }


    public void UpdateNumberOfRuns(uint numberOfRuns)
    {
      while (GetMaxRun() < numberOfRuns)
        AddRaceRun();

      while (GetMaxRun() > numberOfRuns)
        DeleteRaceRun();
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
      
      throw new Exception("invalid race run in GetRun()");
    }

    public RaceRun[] GetRuns()
    {
      RaceRun[] runs = new RaceRun[_runs.Count];
      for (int i = 0; i < _runs.Count; i++)
        runs[i] = GetRun(i);

      return runs;
    }

    public RaceRun GetPreviousRun(RaceRun race)
    {
      int i = 0;
      for (; i < _runs.Count; i++)
        if (_runs[i].Item1 == race)
          break;

      // Race not found or null passed
      if (i == _runs.Count)
        return null;

      // First run does not have a previous run
      if (i == 0)
        return null;
      
      --i;
      return _runs[i].Item1;
    }

    public event EventHandler RunsChanged;


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
      if (points == -1)
      {
        // Update points from DB if existing
        var raceParticipantDB = _db.GetRaceParticipants(this, true).FirstOrDefault(r => r.Participant == participant);
        if (raceParticipantDB != null)
          points = raceParticipantDB.Points;
      }
      if (startnumber == 0)
      {
        // Update startnumber from DB if existing
        var raceParticipantDB = _db.GetRaceParticipants(this, true).FirstOrDefault(r => r.Participant == participant);
        if (raceParticipantDB != null)
          startnumber = raceParticipantDB.StartNumber;
      }

      RaceParticipant raceParticipant = GetParticipant(participant);

      if (raceParticipant == null)
      {
        raceParticipant = new RaceParticipant(this, participant, startnumber, points);
        _participants.Add(raceParticipant);

        // Add existing timings (again)
        foreach (var run in GetRuns())
        {
          var rr = _db.GetRaceRun(this, run.Run).FindAll(r => r.Participant.Participant == participant);
          run.InsertResults(rr);
        }
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
      return RaceUtil.ToString(RaceType);
    }

    /// <summary>
    /// Handler to watches out for changes on collection _participants and performs internal checks, e.g. checkConsistency()
    /// </summary>
    private void onRaceParticipants_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      checkConsistency();
    }

    /// <summary>
    /// Handler to watches out for changes on collection _participants and performs internal checks, e.g. checkConsistency()
    /// </summary>
    private void onRaceParticipants_ItemChanged(object sender, PropertyChangedEventArgs e)
    {
      checkConsistency();
    }

    /// <summary>
    /// Internal function to check on consistency
    /// </summary>
    private void checkConsistency()
    {
      IsConsistent = RaceUtil.IsConsistent(this);
    }


    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null)
        handler(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }


  public static class RaceUtil
  {
    public static string ToString(Race.ERaceType raceType)
    {
      switch (raceType)
      {
        case Race.ERaceType.DownHill:
          return "Abfahrt";

        case Race.ERaceType.SuperG:
          return "Super G";

        case Race.ERaceType.GiantSlalom:
          return "Riesenslalom";

        case Race.ERaceType.Slalom:
          return "Slalom";

        case Race.ERaceType.KOSlalom:
          return "KO Slalom";

        case Race.ERaceType.ParallelSlalom:
          return "Parallel-Slalom";

        default:
          return raceType.ToString();
      }
    }

    /// <summary>
    /// Returns true if:
    /// - all participants have a startnumber assigned
    /// - there wasn't a startnumber assigned twice
    /// </summary>
    /// <param name="race"></param>
    /// <returns></returns>
    public static bool IsConsistent(Race race)
    {
      HashSet<uint> startnumbers = new HashSet<uint>();
      foreach(var rp in race.GetParticipants())
      {
        var stnr = rp.StartNumber;
        if (stnr == 0 || !startnumbers.Add(stnr))
          return false;
      }

      return true;
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
    EParticipantColor? _markedForMeasurement;

    public RunResult OriginalResult { get { return _original; } }

    public EParticipantColor? MarkedForMeasurement 
    { 
      get => _markedForMeasurement;
      
      set 
      { 
        if (value != _markedForMeasurement)
        {
          _markedForMeasurement = value;
          NotifyPropertyChanged();
        }
      } 
    }

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
  public class RaceRun : INotifyPropertyChanged
  {
    private uint _run;
    private Race _race;
    private AppDataModel _appDataModel;

    private ItemsChangeObservableCollection<RunResult> _results;  // This list represents the actual results. It is the basis for all other lists.
    private ItemsChangeObservableCollection<Timestamp> _timestamps;

    private ItemsChangeObservableCollection<LiveResult> _onTrack; // This list only contains the particpants that are on the run.
    private ItemsChangeObservableCollection<RunResult> _inFinish;  // This list represents the particpants in finish.

    private Dictionary<EParticipantColor, RaceParticipant> _markedParticipantForStartMeasurement;
    private Dictionary<EParticipantColor, RaceParticipant> _markedParticipantForFinishMeasurement;

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
      _inFinish = new ItemsChangeObservableCollection<RunResult>();
      _results = new ItemsChangeObservableCollection<RunResult>();
      _timestamps = new ItemsChangeObservableCollection<Timestamp>();

      _markedParticipantForStartMeasurement = new Dictionary<EParticipantColor, RaceParticipant>();
      _markedParticipantForFinishMeasurement = new Dictionary<EParticipantColor, RaceParticipant>();

      // Ensure the results always are in sync with participants
      _race.GetParticipants().CollectionChanged += onParticipantsChanged;

      _UpdateInternals(); // Do this initially
    }


    /// <summary>
    /// Returns the run number for this run (round, durchgang)
    /// </summary>
    public uint Run { get { return _run; } }


    private bool _isComplete;

    /// <summary>
    /// True in case all participants have a valid time or a status other than NotSet or Normal
    /// </summary>
    public bool IsComplete 
    {
      get { return _isComplete; }
      private set { if (_isComplete != value) { _isComplete = value; NotifyPropertyChanged(); } }
    }

    public Dictionary<EParticipantColor, RaceParticipant> MarkedParticipantForStartMeasurement
    {
      get { return _markedParticipantForStartMeasurement; }
    }

    public Dictionary<EParticipantColor, RaceParticipant> MarkedParticipantForFinishMeasurement
    {
      get { return _markedParticipantForFinishMeasurement; }
    }



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

    public ItemsChangeObservableCollection<RunResult> GetInFinishList()
    {
      return _inFinish;
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
      if (_slVP != null)
        _slVP.GetViewList().CollectionChanged -= startListVP_CollectionChanged;

      _slVP = slp;

      _slVP.GetViewList().CollectionChanged += startListVP_CollectionChanged;

      _UpdateInternals();
      sortInFinish();
    }

    private void startListVP_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      _UpdateInternals();
    }

    public StartListViewProvider GetStartListProvider()
    {
      return _slVP;
    }

    public void SetResultViewProvider(ResultViewProvider rvp)
    {
      _rvp = rvp;
      _UpdateInternals();
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

      //_appDataModel.InsertInteractiveTimeMeasurement(participant.Participant);

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
    /// <remarks>startTime and finsihTime can be null. In that case it is stored as not available. A potentially set run time is overwritten with the calculated run time (finish - start).</remarks>
    public void SetTime(EMeasurementPoint measurementPoint, RaceParticipant participant, TimeSpan? time)
    {
      if (measurementPoint == EMeasurementPoint.Start)
        SetStartTime(participant, time);
      else if (measurementPoint == EMeasurementPoint.Finish)
        SetFinishTime(participant, time);
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

      _appDataModel.InsertInteractiveTimeMeasurement(participant.Participant);

      result.ResultCode = rc;

      _UpdateInternals();
    }


    public void SetResultCode(RaceParticipant participant, RunResult.EResultCode rc, string disqualText)
    {
      RunResult result = findOrCreateRunResult(participant);

      _appDataModel.InsertInteractiveTimeMeasurement(participant.Participant);

      result.ResultCode = rc;
      result.DisqualText = disqualText;

      _UpdateInternals();
    }


    public void MarkStartMeasurement(RaceParticipant participant, EParticipantColor color)
    {
      if (!_markedParticipantForStartMeasurement.ContainsKey(color) || _markedParticipantForStartMeasurement[color] != participant)
      {
        if (participant != null)
          _markedParticipantForStartMeasurement[color] = participant;
        else
          _markedParticipantForStartMeasurement.Remove(color);

        NotifyPropertyChanged("MarkedParticipantForStartMeasurement");
      }
    }
    public EParticipantColor? IsMarkedForStartMeasurement(RaceParticipant participant)
    {
      foreach(var entry in _markedParticipantForStartMeasurement)
      {
        if (entry.Value == participant)
          return entry.Key;
      }
      return null;
    }


    public void MarkFinishMeasurement(RaceParticipant participant, EParticipantColor color)
    {
      if (!_markedParticipantForFinishMeasurement.ContainsKey(color) || _markedParticipantForFinishMeasurement[color] != participant)
      {
        if (participant != null)
          _markedParticipantForFinishMeasurement[color] = participant;
        else
          _markedParticipantForFinishMeasurement.Remove(color);

        NotifyPropertyChanged("MarkedParticipantForFinishMeasurement");
        _UpdateOnTrackMarkedForMeasurement();
      }
    }
    public EParticipantColor? IsMarkedForFinishMeasurement(RaceParticipant participant)
    {
      foreach (var entry in _markedParticipantForFinishMeasurement)
      {
        if (entry.Value == participant)
          return entry.Key;
      }
      return null;
    }


    private RunResult findOrCreateRunResult(RaceParticipant participant)
    {
      if (participant == null)
        return null;

      RunResult result = GetRunResult(participant);
      if (result == null)
      {
        result = new RunResult(participant);
        _results.Add(result);
      }

      return result;
    }


    public RunResult GetRunResult(RaceParticipant participant)
    {
      RunResult result = _results.SingleOrDefault(r => r.Participant == participant);
      return result;
    }



    public RunResult DeleteRunResult(RaceParticipant participant)
    {
      RunResult result = _results.SingleOrDefault(r => r.Participant == participant);
      if (result != null)
      {
        _results.Remove(result);
      }

      _UpdateInternals();

      return result;
    }


    public void DeleteRunResults()
    {
      while (_results.Count > 0)
        _results.RemoveAt(0);

      _UpdateInternals();
    }


    protected void onParticipantsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
      {
        foreach (RaceParticipant rp in e.OldItems)
          DeleteRunResult(rp); // _UpdateInternals is called internally
      }
      else
        _UpdateInternals();
    }



    public void InsertResults(List<RunResult> r)
    {
      foreach (var source in r)
      {
        var target = findOrCreateRunResult(source.Participant);
        if (target != null)
          target.UpdateRunResult(source);
      }

      _UpdateInternals();
    }

    public void InsertTimestamps(List<Timestamp> timestamps)
    {
      foreach(var item in timestamps)
        _timestamps.Add(item);
    }

    public ItemsChangeObservableCollection<Timestamp> GetTimestamps()
    {
      return _timestamps;
    }

    public Timestamp GetTimestamp(TimeSpan time, EMeasurementPoint mp)
    {
      return _timestamps.FirstOrDefault(t => t.Time == time && t.MeasurementPoint == mp);
    }


    public Timestamp AddOrUpdateTimestamp(Timestamp ts)
    {
      var tsFound = GetTimestamp(ts.Time, ts.MeasurementPoint);
      if (tsFound != null)
      {
        tsFound.StartNumber = ts.StartNumber;
        tsFound.Valid = ts.Valid;
        return tsFound;
      }
      else
      {
        _timestamps.Add(ts);
        return ts;
      }
    }


    // Helper definition for a participant is on track
    private bool IsOnTrack(RunResult r)
    {
      return r.GetStartTime() != null && r.GetFinishTime() == null && r.ResultCode == RunResult.EResultCode.Normal; // && _appDataModel.TodayMeasured(r.Participant.Participant);
    }

    // Helper definition for a participant is on track
    public bool IsOrWasOnTrack(RunResult r)
    {
      return r.GetStartTime() != null || r.GetRunTime() != null || (r.ResultCode != RunResult.EResultCode.NotSet && r.ResultCode != RunResult.EResultCode.Normal);
    }

    public bool WasOnTrack(RunResult r)
    {
      return r.GetRunTime() != null || (r.ResultCode != RunResult.EResultCode.NotSet && r.ResultCode != RunResult.EResultCode.Normal);
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
      if (_results.Count == 0)
        return false;

      if (_results.FirstOrDefault(
        r => (r.ResultCode == RunResult.EResultCode.Normal && r.Runtime != null) 
          || (r.ResultCode != RunResult.EResultCode.NotSet && r.ResultCode != RunResult.EResultCode.Normal)) != null)
        return true;

      return false;
    }



    public delegate void OnTrackChangedHandler(RaceRun rr, RaceParticipant participantEnteredTrack, RaceParticipant participantLeftTrack, RunResult currentRunResult);
    public event OnTrackChangedHandler OnTrackChanged;
    public event OnTrackChangedHandler InFinishChanged;

    /// <summary>
    /// Updates internal strucutures based on _results
    /// </summary>
    private void _UpdateInternals()
    {
      _UpdateOnTrack();
      _UpdateInFinish();
      IsComplete = RaceRunUtil.IsComplete(this);
    }

    private void _UpdateOnTrack()
    {
      var results = _results.ToArray();

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
          _onTrack.Insert(0, new LiveResult(r, _appDataModel));

          OnTrackChangedHandler handler = OnTrackChanged;
          handler?.Invoke(this, r.Participant, null, r);
        }
      }

      _UpdateOnTrackMarkedForMeasurement();
    }

    private void _UpdateOnTrackMarkedForMeasurement()
    {
      foreach (var r in _onTrack)
        r.MarkedForMeasurement = IsMarkedForFinishMeasurement(r.Participant);
    }


    private void _UpdateInFinish()
    {
      var results = _results.ToArray();

      // Remove from inFinish list if a result is not available anymore (= not on track anymore)
      var itemsToRemove = _inFinish.Where(r => !WasOnTrack(r)).ToList();
      foreach (var itemToRemove in itemsToRemove)
      {
        _inFinish.Remove(itemToRemove);

        OnTrackChangedHandler handler = InFinishChanged;
        handler?.Invoke(this, null, itemToRemove.Participant, itemToRemove);
      }

      // Add to inFinish list if run result is available (= WasOnTrack())
      var shallBeInFinish = results.Where(r => WasOnTrack(r)).ToList();
      foreach (var r in shallBeInFinish)
      {
        if (_inFinish.SingleOrDefault(o => o.Participant == r.Participant) == null)
        {
          _inFinish.Add(r);

          OnTrackChangedHandler handler = InFinishChanged;
          handler?.Invoke(this, r.Participant, null, r);
        }
      }

      sortInFinish();
    }


    /// <summary>
    /// Adapts order of finishlist according to the startlist
    /// </summary>
    private void sortInFinish()
    {
      var vpStart = GetStartListProvider();
      if (vpStart == null)
        return;

      var startList = vpStart.GetViewList();
      if (startList == null)
        return;

      int idxFinishDst = 0;
      for(int idxStart = startList.Count-1; idxStart>0; idxStart--)
      {
        var entryStartList = startList[idxStart];

        int idxFinishSrc = idxFinishDst; 
        while (idxFinishSrc < _inFinish.Count)
        {
          if (_inFinish[idxFinishSrc].Participant == entryStartList.Participant)
            break;
          idxFinishSrc++;
        }

        if (idxFinishSrc < _inFinish.Count && idxFinishDst < _inFinish.Count)
        {
          _inFinish.Move(idxFinishSrc, idxFinishDst);
          idxFinishDst++;
        }

      }
    }


    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null)
        handler(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }


  public static class RaceRunUtil
  {
    /// <summary>
    /// True in case all participants have a valid time or a status other than NotSet or Normal
    /// </summary>
    static public bool IsComplete(RaceRun rr)
    {
      // Check whether all Participants have a result
      // Check whether the result is either:
      // a) Normal with valid runtime
      //  or
      // b) a resultcode different than NotSet or Normal
      if (rr.GetStartListProvider() == null)
        return false;

      var slpList = rr.GetStartListProvider().GetViewList();
      if (slpList.Count == 0)
        return false;

      foreach (var sle in slpList)
      {
        var runResult = rr.GetRunResult(sle.Participant);
        if (runResult == null)
          return false;

        if (!(
             (runResult.ResultCode == RunResult.EResultCode.Normal && runResult.RuntimeWOResultCode != null)
          || (runResult.ResultCode != RunResult.EResultCode.Normal && runResult.ResultCode != RunResult.EResultCode.NotSet)
          ))
          return false;
      }

      return true;
    }
  }




  /// <summary>
  /// Defines the interface to the actual database engine
  /// </summary>
  /// <remarks>Assuming the database format changes we can simply create another implementation.</remarks>
  public interface IAppDataModelDataBase
  {

    void Close();

    string GetDBPath();
    string GetDBFileName();
    string GetDBPathDirectory();


    ItemsChangeObservableCollection<Participant> GetParticipants();

    List<ParticipantGroup> GetParticipantGroups();
    List<ParticipantClass> GetParticipantClasses();
    List<ParticipantCategory> GetParticipantCategories();

    List<Race.RaceProperties> GetRaces();
    List<RaceParticipant> GetRaceParticipants(Race race, bool ignoreAktiveFlag = false);

    List<RunResult> GetRaceRun(Race race, uint run);

    AdditionalRaceProperties GetRaceProperties(Race race);
    void StoreRaceProperties(Race race, AdditionalRaceProperties props);

    string GetTimingDevice(Race race);
    void StoreTimingDevice(Race race, string timingDevice);

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



    void CreateOrUpdateTimestamp(RaceRun raceRun, Timestamp timestamp);
    List<Timestamp> GetTimestamps(Race raceRun, uint run);
    void RemoveTimestamp(RaceRun raceRun, Timestamp timestamp);


    PrintCertificateModel GetCertificateModel(Race race);


    void StoreKeyValue(string key, string value);
    string GetKeyValue(string key);
  };

}
