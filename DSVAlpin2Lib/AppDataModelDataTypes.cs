using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{


  public class ParticipantGroup : INotifyPropertyChanged, IComparable<ParticipantGroup>, IComparable
  {
    private string _id;
    private string _name;
    private uint _sortpos;

    public ParticipantGroup(string id, string name, uint sortpos)
    {
      _id = id;
      _name = name;
      _sortpos = sortpos;
    }

    public string Id
    {
      get => _id;
    }

    public string Name
    {
      get => _name;
      //set { _name = value; NotifyPropertyChanged(); }
    }

    public uint SortPos
    {
      get => _sortpos;
    }

    public override string ToString()
    {
      return _name;
    }


    public int CompareTo(ParticipantGroup other)
    {
      if (_sortpos == other._sortpos)
        return _name.CompareTo(other._name);

      return _sortpos.CompareTo(other._sortpos);
    }

    int IComparable.CompareTo(object obj)
    {
      if (obj is ParticipantGroup other)
        return CompareTo(other);

      return -1;
    }


    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

  }

  
  public class ParticipantClass : INotifyPropertyChanged, IComparable<ParticipantClass>, IComparable
  {
    private string _id;
    private ParticipantGroup _group;
    private string _name;
    private string _sex;
    private uint _year; // ältester mit erfaßter Jahrgang
    private uint _sortpos;


    public ParticipantClass(string id, ParticipantGroup parentGroup, string name, string sex, uint year, uint sortpos)
    {
      _id = id;
      _group = parentGroup;
      _name = name;
      _sex = sex;
      _year = year;
      _sortpos = sortpos;
    }

    public string Id
    {
      get => _id;
    }

    public string Name
    {
      get => _name;
      //set { _name = value; NotifyPropertyChanged(); }
    }

    public string Sex
    {
      get => _sex;
      //set { _sex = value; NotifyPropertyChanged(); }
    }

    public uint Year
    {
      get => _year;
      //set { _year = value; NotifyPropertyChanged(); }
    }

    public uint SortPos
    {
      get => _sortpos;
    }

    public ParticipantGroup Group
    {
      get => _group;
    }

    public override string ToString()
    {
      return _name;
    }

    public int CompareTo(ParticipantClass other)
    {
      if (_sortpos == other._sortpos)
        return _name.CompareTo(other._name);

      return _sortpos.CompareTo(other._sortpos);
    }

    int IComparable.CompareTo(object obj)
    {
      if (obj is ParticipantClass other)
        return CompareTo(other);

      return -1;
    }

    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }





  /// <summary>
  /// Represents a participant (or ski alpin racer)
  /// </summary>
  /// <remarks>not yet final</remarks>
  public class Participant : INotifyPropertyChanged
  {
    private string _id;
    private string _name;
    private string _firstname;
    private string _sex;
    private uint _year;
    private string _club;
    private string _nation;
    private string _code;
    private string _svid;
    private ParticipantClass _class;

    public string Id
    {
      get => _id;
      set { _id = value; NotifyPropertyChanged(); }
    }

    public string Name
    {
      get => _name;
      set { _name = value; NotifyPropertyChanged(); }
    }
    public string Firstname
    {
      get => _firstname;
      set { _firstname = value; NotifyPropertyChanged(); }
    }

    public string Fullname
    {
      get { return _name + ", " + _firstname; }
    }


    public string Sex
    {
      get => _sex;
      set { _sex = value; NotifyPropertyChanged(); }
    }

    public uint Year
    {
      get => _year;
      set { _year = value; NotifyPropertyChanged(); }
    }
    public string Club
    {
      get => _club;
      set { _club = value; NotifyPropertyChanged(); }
    }

    public string SvId
    {
      get => _svid;
      set { _svid = value; NotifyPropertyChanged(); }
    }

    public string Code
    {
      get => _code;
      set { _code = value; NotifyPropertyChanged(); }
    }

    public string CodeOrSvId
    {
      get { if (string.IsNullOrEmpty(_code)) return _svid; else return _code; }
    }


    public string Nation
    {
      get => _nation;
      set { _nation = value; NotifyPropertyChanged(); }
    }

    public ParticipantClass Class
    {
      get => _class;
      set { _class = value; NotifyPropertyChanged(); }
    }
    public ParticipantGroup Group
    {
      get => _class.Group;
    }

    public override string ToString()
    {
      return _name + ", " + _firstname + "(" + _year + ")";
    }


    #region INotifyPropertyChanged implementation


    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    #endregion
  }


  /// <summary>
  /// Participant with start number
  /// </summary>
  public class RaceParticipant : INotifyPropertyChanged, IDisposable
  {
    public Participant _participant;
    private uint _startnumber;
    private double _points; // Points prior to the race


    public RaceParticipant(Participant participant, uint startnumber, double points)
    {
      _participant = participant;
      _startnumber = startnumber;
      _points = points;

      _participant.PropertyChanged += OnParticipantPropertyChanged;
    }


    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          _participant.PropertyChanged -= OnParticipantPropertyChanged;
        }

        disposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
    }
    #endregion

    public Participant Participant { get { return _participant; } }

    public string Id { get => _participant.Id; }
    public string Name { get => _participant.Name; }
    public string Firstname { get => _participant.Firstname; }
    public string Fullname { get => _participant.Fullname; }
    public string Sex { get => _participant.Sex; }
    public uint Year { get => _participant.Year; }
    public string Club { get => _participant.Club; }
    public string Nation { get => _participant.Nation; }
    public string SvId { get => _participant.SvId; }
    public string Code { get => _participant.Code; }

    public ParticipantClass Class { get => _participant.Class; }
    public ParticipantGroup Group { get => _participant.Group; }

    public uint StartNumber
    {
      get => _startnumber;
      set { _startnumber = value; NotifyPropertyChanged(); }
    }

    public double Points // Points prior to the race
    {
      get => _points;
      set { _points = value; NotifyPropertyChanged(); }
    }

    public override string ToString()
    {
      return "StNr: " + _startnumber + " " + _participant.ToString();
    }


    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Pass through the property change
    private void OnParticipantPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
      NotifyPropertyChanged(args.PropertyName);
    }

    #endregion
  }


  /// <summary>
  /// Represents a run result (a pass / ein durchgang)
  /// </summary>
  /// <remarks>not yet final</remarks>
  public class StartListEntry : INotifyPropertyChanged
  {
    protected RaceParticipant _participant;
    protected bool _started;

    public StartListEntry(RaceParticipant participant)
    {
      _participant = participant;
      _started = false;
      _participant.PropertyChanged += OnParticipantPropertyChanged;
    }


    public StartListEntry ShallowCopy()
    {
      return (StartListEntry)this.MemberwiseClone();
    }


    // Some public properties to get displayed in the list
    public RaceParticipant Participant { get { return _participant; } }
    public uint StartNumber { get { return _participant.StartNumber; } }
    public double Points { get { return _participant.Points; } }
    public string Id { get { return _participant.Id; } }
    public string Name { get { return _participant.Name; } }
    public string Firstname { get { return _participant.Firstname; } }
    public uint Year { get { return _participant.Year; } }
    public string Club { get { return _participant.Club; } }
    public ParticipantClass Class { get { return _participant.Class; } }
    public ParticipantGroup Group { get => _participant.Group; }
    public string Sex { get { return _participant.Sex; } }
    public string Nation { get { return _participant.Nation; } }
    public string SvId{ get { return _participant.SvId; } }
    public string Code{ get { return _participant.Code; } }

    public bool Started
    {
      get => _started;
      set
      {
        if (_started != value)
        {
          _started = value;
          NotifyPropertyChanged();
        }
      }
    }

    public override string ToString()
    {
      return _participant.ToString();
    }


    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Pass through the property change
    private void OnParticipantPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
      NotifyPropertyChanged(args.PropertyName);
    }

    #endregion
  }


  /// <summary>
  /// Represents a run result (a pass / ein durchgang)
  /// </summary>
  /// <remarks>not yet final</remarks>
  public class StartListEntryAdditionalRun : StartListEntry
  {
    private RunResult _resultPreviousRun;

    public StartListEntryAdditionalRun(RunResult resultPreviousRun) : base(resultPreviousRun.Participant)
    {
      _resultPreviousRun = resultPreviousRun;
    }

    public TimeSpan? Runtime { get { return _resultPreviousRun.Runtime; } }

    public override string ToString()
    {
      return _participant.ToString() + " (" + Runtime?.ToString(@"mm\:s\,ff") + ")";
    }

  }


  /// <summary>
  /// Represents a run result (a pass / ein durchgang)
  /// </summary>
  /// <remarks>not yet final</remarks>
  public class RunResult : INotifyPropertyChanged
  {
    public enum EResultCode { Normal = 0, NaS = 1, NiZ = 2, DIS = 3, NQ = 4 }; // 0;"Normal";1;"Nicht am Start";2;"Nicht im Ziel";3;"Disqualifiziert";4;"Nicht qualifiziert"

    #region internal members

    public RaceParticipant _participant;
    protected TimeSpan? _runTime;
    protected TimeSpan? _startTime;
    protected TimeSpan? _finishTime;
    protected EResultCode _resultCode;
    protected string _disqualText;
    
    #endregion


    // Some public properties to get displayed in the list
    public RaceParticipant Participant { get { return _participant; } }
    public uint StartNumber { get { return _participant.StartNumber; } }
    public string Id { get { return _participant.Id; } }
    public string Name { get { return _participant.Name; } }
    public string Firstname { get { return _participant.Firstname; } }
    public uint Year { get { return _participant.Year; } }
    public string Club { get { return _participant.Club; } }
    public ParticipantClass Class { get { return _participant.Class; } }
    public ParticipantGroup Group { get => _participant.Group; }
    public string Sex { get { return _participant.Sex; } }
    public string Nation { get { return _participant.Nation; } }
    public string SvId { get { return _participant.SvId; } }
    public string Code { get { return _participant.Code; } }


    public TimeSpan? Runtime { get { return GetRunTime(); } }
    public TimeSpan? RuntimeOrig { get { return GetRunTime(true, false); } }
    public EResultCode ResultCode { get { return _resultCode; } set { _resultCode = value; NotifyPropertyChanged(); } }
    public string DisqualText { get { return _disqualText; } set { _disqualText = value; NotifyPropertyChanged(); } }


    public RunResult(RaceParticipant particpant)
    {
      _participant = particpant;

      _runTime = null;
      _startTime = null;
      _finishTime = null;
      _resultCode = EResultCode.Normal;
      _disqualText = null;
    }

    protected RunResult(RunResult original)
    {
      _participant = original._participant;
      _startTime = original._startTime;
      _runTime = original._runTime;
      _finishTime = original._finishTime;
      _resultCode = original._resultCode;
      _disqualText = original._disqualText;
    }

    public void UpdateRunResult(RunResult original)
    {
      System.Diagnostics.Debug.Assert(_participant == original._participant);

      _startTime = original._startTime;
      _runTime = original._runTime;
      _finishTime = original._finishTime;
      _resultCode = original._resultCode;
      _disqualText = original._disqualText;

      NotifyPropertyChanged(propertyName: nameof(Runtime));
      NotifyPropertyChanged(propertyName: nameof(ResultCode));
      NotifyPropertyChanged(propertyName: nameof(DisqualText));
    }


    public void SetRunTime(TimeSpan? t)
    {
      _runTime = t;

      NotifyPropertyChanged(propertyName: nameof(Runtime));
    }

    public TimeSpan? GetRunTime(bool calculateIfNotStored = true, bool considerResultCode = true)
    {
      if (!considerResultCode || _resultCode == EResultCode.Normal)
      {
        if (_runTime != null)
          return _runTime;

        if (calculateIfNotStored && _startTime != null && _finishTime != null)
          return _finishTime - _startTime;
      }

      return null;
    }


    public void SetStartTime(TimeSpan? startTime)
    {
      _startTime = startTime;

      NotifyPropertyChanged(propertyName: nameof(Runtime));
    }

    public void SetFinishTime(TimeSpan? finishTime)
    {
      _finishTime = finishTime;

      NotifyPropertyChanged(propertyName: nameof(Runtime));
    }

    public TimeSpan? GetStartTime() { return _startTime; }
    public TimeSpan? GetFinishTime() { return _finishTime; }


    public override string ToString()
    {
      return "T: " + _runTime?.ToString(@"mm\:s\,ff") + "(" + _startTime?.ToString(@"hh\:mm\:s\,ff") + "," + _finishTime?.ToString(@"hh\:mm\:s\,ff") + ")";
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
  /// Represents a RunResult with position (for a run result list)
  /// </summary>
  public class RunResultWithPosition : RunResult
  {
    private uint _position;
    private bool _justModified;
    private TimeSpan? _diffToFirst;

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

    public TimeSpan? DiffToFirst
    {
      get { return _diffToFirst; }
      set { _diffToFirst = value; NotifyPropertyChanged(); }
    }

    public bool JustModified
    {
      get { return _justModified; }
      set { if (_justModified != value) { _justModified = value; NotifyPropertyChanged(); } }
    }

    public override string ToString()
    {
      return "P:" + _position + " " + base.ToString();
    }

  }


  /// <summary>
  /// Represents a race result. It contains out of the participant including its run results (run, time, status) and its final position within the group.
  /// </summary>
  public class RaceResultItem : INotifyPropertyChanged
  {
    #region private

    RaceParticipant _participant;
    Dictionary<uint, TimeSpan?> _runTimes;
    Dictionary<uint, RunResult.EResultCode> _runResultCodes;
    TimeSpan? _totalTime;
    private uint _position;
    private TimeSpan? _diffToFirst;
    private bool _justModified;


    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="participant">The participant the results belong to.</param>
    public RaceResultItem(RaceParticipant participant)
    {
      _participant = participant;
      _runTimes = new Dictionary<uint, TimeSpan?>();
      _runResultCodes = new Dictionary<uint, RunResult.EResultCode>();
    }

    /// <summary>
    /// Returns the participant
    /// </summary>
    public RaceParticipant Participant { get { return _participant; } }

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

    public TimeSpan? DiffToFirst
    {
      get { return _diffToFirst; }
      set { _diffToFirst = value; NotifyPropertyChanged(); }
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
    /// Returns the separate run results per run
    /// </summary>
    public Dictionary<uint, RunResult.EResultCode> RunResultCodes { get { return _runResultCodes; } }

    /// <summary>
    /// Sets the results for one specific run
    /// </summary>
    /// <param name="run">Run number, typically either 1 or 2</param>
    /// <param name="result">The corresponding results</param>
    public void SetRunResult(uint run, RunResult result)
    {
      if (result != null)
      {
        _runTimes[run] = result.Runtime;
        _runResultCodes[run] = result.ResultCode;
      }
      else
      {
        _runTimes.Remove(run);
        _runResultCodes.Remove(run);
      }

      NotifyPropertyChanged(nameof(RunTimes));
    }


    public override string ToString()
    {
      return "P:" + _position + " (" + string.Join(",", _runTimes) + ")";
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


}
