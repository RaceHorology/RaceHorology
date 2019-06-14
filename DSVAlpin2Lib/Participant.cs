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


    public void SetStartFinishTime(TimeSpan? startTime, TimeSpan? finishTime)
    {
      _runTime = null;
      _startTime = startTime;
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

    public Participant _participant;

    private TimeSpan? _runTime;
    private TimeSpan? _startTime;
    private TimeSpan? _finishTime;
    private EResultCode _resultCode;
    private string _disqualText;


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

}
