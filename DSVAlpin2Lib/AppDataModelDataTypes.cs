using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{
  /// <summary>
  /// Represents a participant (or ski alpin racer)
  /// </summary>
  /// <remarks>not yet final</remarks>
  public class Participant : INotifyPropertyChanged
  {
    private string _name;
    private string _firstname;
    private string _sex;
    private int _year;
    private string _club;
    private string _nation;
    private string _class;
    private uint _startnumber;

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

    public string Sex
    {
      get => _sex;
      set { _sex = value; NotifyPropertyChanged(); }
    }

    public int Year
    {
      get => _year;
      set { _year = value; NotifyPropertyChanged(); }
    }
    public string Club
    {
      get => _club;
      set { _club = value; NotifyPropertyChanged(); }
    }

    public string Nation
    {
      get => _nation;
      set { _nation = value; NotifyPropertyChanged(); }
    }

    public string Class
    {
      get => _class;
      set { _class = value; NotifyPropertyChanged(); }
    }
    public uint StartNumber
    {
      get => _startnumber;
      set { _startnumber = value; NotifyPropertyChanged(); }
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
  /// Represents a run result (a pass / ein durchgang)
  /// </summary>
  /// <remarks>not yet final</remarks>
  public class RunResult : INotifyPropertyChanged
  {
    public enum EResultCode { Normal = 0, NaS = 1, NiZ = 2, DIS = 3, NQ = 4 }; // 0;"Normal";1;"Nicht am Start";2;"Nicht im Ziel";3;"Disqualifiziert";4;"Nicht qualifiziert"


    #region internal members

    public Participant _participant;
    protected TimeSpan? _runTime;
    protected TimeSpan? _startTime;
    protected TimeSpan? _finishTime;
    protected EResultCode _resultCode;
    protected string _disqualText;
    
    #endregion


    // Some public properties to get displayed in the list
    // TODO: This should not be part of this calss, instead another entity should do the conversion
    public Participant Participant { get { return _participant; } }
    public string StartNumber { get { return _participant.StartNumber.ToString(); } }
    public string Name { get { return _participant.Name; } }
    public string Firstname { get { return _participant.Firstname; } }
    public int Year { get { return _participant.Year; } }
    public string Club { get { return _participant.Club; } }
    public string Class { get { return _participant.Class; } }
    public TimeSpan? Runtime { get { return _runTime; } }
    public EResultCode ResultCode { get { return _resultCode; } set { _resultCode = value; NotifyPropertyChanged(); } }
    public string DisqualText { get { return _disqualText; } set { _disqualText = value; NotifyPropertyChanged(); } }


    public RunResult(Participant particpant)
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
      _startTime = null;
      _finishTime = null;
      _runTime = t;

      // Clear Start & Finish Time (might be inconsistent to the start & finish time)
      MakeConsistencyCheck();

      NotifyPropertyChanged(propertyName: nameof(Runtime));
    }

    public TimeSpan? GetRunTime() { return _runTime;  }


    public void SetStartTime(TimeSpan? startTime)
    {
      _runTime = null;
      _startTime = startTime;

      if (_startTime != null && _finishTime != null)
        _runTime = _finishTime - _startTime;
      else
        MakeConsistencyCheck();

      NotifyPropertyChanged(propertyName: nameof(Runtime));
    }

    public void SetFinishTime(TimeSpan? finishTime)
    {
      _runTime = null;
      _finishTime = finishTime;

      if (_startTime != null && _finishTime != null)
        _runTime = _finishTime - _startTime;
      else
        MakeConsistencyCheck();

      NotifyPropertyChanged(propertyName: nameof(Runtime));
    }

    public TimeSpan? GetStartTime() { return _startTime; }
    public TimeSpan? GetFinishTime() { return _finishTime; }


    private void MakeConsistencyCheck()
    {
      // Consistency check
      if (_runTime != null && _startTime != null && _finishTime != null)
      {
        TimeSpan calcRunTime = (TimeSpan )_runTime;
        TimeSpan diff = calcRunTime - (TimeSpan)_runTime;

        System.Diagnostics.Debug.Assert(Math.Abs(diff.TotalMilliseconds) < 1.0);
      }
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



  public class RunResultWithPosition : RunResult
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


}
