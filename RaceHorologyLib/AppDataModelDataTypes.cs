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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{


  public class ParticipantGroup : INotifyPropertyChanged, IComparable<ParticipantGroup>, IComparable
  {
    private string _id;
    private string _name;
    private uint _sortpos;

    public ParticipantGroup()
    {
      _id = null;
      _name = "";
      _sortpos = uint.MaxValue;
    }

    public ParticipantGroup(string id, string name, uint sortpos)
    {
      _id = id;
      _name = name;
      _sortpos = sortpos;
    }

    public string Id
    {
      get => _id;
      set
      {
        if (_id != value)
        {
          _id = value;
          NotifyPropertyChanged();
        }
      }
    }

    public string Name
    {
      get => _name;
      set
      {
        if (_name != value)
        {
          _name = value; 
          NotifyPropertyChanged();
        }
      }
    }

    public uint SortPos
    {
      get => _sortpos;
      set
      { 
        if (_sortpos != value)
        {
          _sortpos = value;
          NotifyPropertyChanged();
        }
      }
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

  /// <summary>
  /// Represents the participant's class
  /// Defines a relation via IComparable based on its sortkey
  /// </summary>
  public class ParticipantClass : INotifyPropertyChanged, IComparable<ParticipantClass>, IComparable
  {
    private string _id;
    private ParticipantGroup _group;
    private string _name;
    private ParticipantCategory _sex;
    private uint _year; // ältester mit erfaßter Jahrgang
    private uint _sortpos;


    public ParticipantClass()
    {
      _id = null;
      _group = null;
      _name = "";
      _sex = null;
      _year = 0;
      _sortpos = uint.MaxValue;
    }

    public ParticipantClass(string id, ParticipantGroup parentGroup, string name, ParticipantCategory sex, uint year, uint sortpos)
    {
      _id = id;
      Group = parentGroup;
      _name = name;
      _sex = sex;
      _year = year;
      _sortpos = sortpos;
    }

    public string Id
    {
      get => _id;
      set
      {
        if (_id != value)
        {
          _id = value;
          NotifyPropertyChanged();
        }
      }
    }

    public string Name
    {
      get => _name;
      set
      {
        if (_name != value)
        {
          _name = value;
          NotifyPropertyChanged();
        }
      }
    }

    public ParticipantCategory Sex
    {
      get => _sex;
      set { _sex = value; NotifyPropertyChanged(); }
    }

    public uint Year
    {
      get => _year;
      set { _year = value; NotifyPropertyChanged(); }
    }

    public uint SortPos
    {
      get => _sortpos;
      set
      {
        if (_sortpos != value)
        {
          _sortpos = value;
          NotifyPropertyChanged();
        }
      }
    }

    public ParticipantGroup Group
    {
      get => _group;
      set
      {
        if (_group != value)
        {
          if (_group != null)
            _group.PropertyChanged -= OnGroupChanged;

          _group = value; NotifyPropertyChanged();

          if (_group != null)
            _group.PropertyChanged += OnGroupChanged;
        }
      }
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

    private void OnGroupChanged(object source, PropertyChangedEventArgs eargs)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Group"));
    }
    #endregion
  }


  /// <summary>
  /// Represents the category of a participant, typically its sex
  /// </summary>
  public class ParticipantCategory : INotifyPropertyChanged, IComparable<ParticipantCategory>, IComparable, IEquatable<ParticipantCategory>
  {
    private char _name;
    private string _synonyms;
    private string _prettyName;
    private uint _sortpos;

    public ParticipantCategory()
    {
      _name = char.MinValue;
      _synonyms = null;
      _prettyName = "";
      _sortpos = uint.MaxValue;
    }

    public ParticipantCategory(char name)
    {
      _name = name;
      _synonyms = null;
      _prettyName = new string(name, 1);
      _sortpos = uint.MaxValue;
    }

    public ParticipantCategory(char name, string prettyName, uint sortpos, string synonyms = null)
    {
      _name = name;
      _synonyms = synonyms;
      _prettyName = prettyName;
      _sortpos = sortpos;
    }

    public string PrettyName
    {
      get => _prettyName;
      set
      {
        if (_prettyName != value)
        {
          _prettyName = value;
          NotifyPropertyChanged();
        }
      }
    }

    public string Synonyms
    {
      get => _synonyms;
      set
      {
        if (_synonyms != value)
        {
          _synonyms = value;
          NotifyPropertyChanged();
        }
      }
    }

    public char Name
    {
      get => _name;
      set
      {
        if (_name != value)
        {
          _name = value;
          NotifyPropertyChanged();
        }
      }
    }

    public uint SortPos
    {
      get => _sortpos;
      set
      {
        if (_sortpos != value)
        {
          _sortpos = value;
          NotifyPropertyChanged();
        }
      }
    }

    public override string ToString()
    {
      return _prettyName;
    }


    public int CompareTo(ParticipantCategory other)
    {
      if (_sortpos == other._sortpos)
        return _name.CompareTo(other._name);

      return _sortpos.CompareTo(other._sortpos);
    }

    int IComparable.CompareTo(object obj)
    {
      if (obj is ParticipantCategory other)
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

    #region Equality
    public bool Equals(ParticipantCategory other)
    {
      return _name == other._name;
    }

    public override bool Equals(object obj)
    {
      if (obj is ParticipantCategory other)
        return Equals(other);

      return false;
    }

    public static bool operator ==(ParticipantCategory obj1, ParticipantCategory obj2)
    {
      return object.Equals(obj1, obj2);
    }

    public static bool operator !=(ParticipantCategory obj1, ParticipantCategory obj2)
    {
      return !object.Equals(obj1, obj2);
    }

    public override int GetHashCode()
    {
      return _name.GetHashCode();
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
    private ParticipantCategory _sex;
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
      set { if (_name != value) { _name = value; NotifyPropertyChanged(); } }
    }
    public string Firstname
    {
      get => _firstname;
      set { if (_firstname != value) { _firstname = value; NotifyPropertyChanged(); } }
    }

    public string Fullname
    {
      get { return _name + ", " + _firstname; }
    }


    public ParticipantCategory Sex
    {
      get => _sex;
      set
      {
        if (_sex != value)
        {
          if (_sex != null)
            _sex.PropertyChanged -= OnSexChanged;

          _sex = value;
          NotifyPropertyChanged();

          if (_sex != null)
            _sex.PropertyChanged += OnSexChanged;
        }
      }
    }

    public uint Year
    {
      get => _year;
      set { if (_year != value) { _year = value; NotifyPropertyChanged(); } }
    }
    public string Club
    {
      get => _club;
      set { if (_club != value) { _club = value; NotifyPropertyChanged(); } }
    }

    public string SvId
    {
      get => _svid;
      set { if (_svid != value) { _svid = value; NotifyPropertyChanged(); } }
    }

    public string Code
    {
      get => _code;
      set { if (_code != value) { _code = value; NotifyPropertyChanged(); } }
    }

    public string CodeOrSvId
    {
      get { if (string.IsNullOrEmpty(_code)) return _svid; else return _code; }
    }


    public string Nation
    {
      get => _nation;
      set { if (_nation != value) { _nation = value; NotifyPropertyChanged(); } }
    }

    public ParticipantClass Class
    {
      get => _class;
      set 
      { 
        if (_class != value) 
        {
          if (_class != null)
            _class.PropertyChanged -= OnClassChanged;

          if (_class?.Group != null)
            (_class?.Group).PropertyChanged -= OnGroupChanged;

          _class = value;
          NotifyPropertyChanged();
          NotifyPropertyChanged("Group");

          if (_class != null)
            _class.PropertyChanged += OnClassChanged;

          if (_class?.Group != null)
            (_class?.Group).PropertyChanged += OnGroupChanged;
        }
      }
    }
    public ParticipantGroup Group
    {
      get => _class?.Group;
    }

    public override string ToString()
    {
      return _name + ", " + _firstname + "(" + _year + ")";
    }


    public void Assign(Participant other)
    {
      Name = other.Name;
      Firstname = other.Firstname;
      Sex = other.Sex;
      Year = other.Year;
      Club = other.Club;
      SvId = other.SvId;
      Code = other.Code;
      Nation = other.Nation;
      Class = other.Class;
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

    private void OnClassChanged(object source, PropertyChangedEventArgs eargs)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Class"));
    }

    private void OnGroupChanged(object source, PropertyChangedEventArgs eargs)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Group"));
    }

    private void OnSexChanged(object source, PropertyChangedEventArgs eargs)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Sex"));
    }


    #endregion
  }


  /// <summary>
  /// Participant with start number
  /// </summary>
  public class RaceParticipant : INotifyPropertyChanged, IDisposable
  {
    private Race _race;
    public Participant _participant;
    private uint _startnumber;
    private double _points; // Points prior to the race


    public RaceParticipant(Race race, Participant participant, uint startnumber, double points)
    {
      _race = race;
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

    public Race Race { get { return _race; } }
    public Participant Participant { get { return _participant; } }

    public string Id { get => _participant.Id; }
    public string Name { get => _participant.Name; }
    public string Firstname { get => _participant.Firstname; }
    public string Fullname { get => _participant.Fullname; }
    public ParticipantCategory Sex { get => _participant.Sex; }
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
      StartListEntry copy = new StartListEntry(_participant);
      copy._started = _started;
      return copy;
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
    public ParticipantCategory Sex { get { return _participant.Sex; } }
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
    public RunResult.EResultCode ResultCode { get { return _resultPreviousRun.ResultCode; } }

    public override string ToString()
    {
      return _participant.ToString() + " (" + Runtime?.ToString(@"mm\:s\,ff") + ")";
    }

  }


  /// <summary>
  /// Represents a run result (a pass / ein durchgang)
  /// </summary>
  public class RunResult : INotifyPropertyChanged
  {
    public enum EResultCode { Normal = 0, NaS = 1, NiZ = 2, DIS = 3, NQ = 4, NotSet = -1 }; // 0;"Normal";1;"Nicht am Start";2;"Nicht im Ziel";3;"Disqualifiziert";4;"Nicht qualifiziert"

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
    public ParticipantCategory Sex { get { return _participant.Sex; } }
    public string Nation { get { return _participant.Nation; } }
    public string SvId { get { return _participant.SvId; } }
    public string Code { get { return _participant.Code; } }


    public TimeSpan? Runtime { get { return GetRunTime(); } }
    public TimeSpan? RuntimeWOResultCode { get { return GetRunTime(true, false); } }
    public TimeSpan? RuntimeIntern { get { return GetRunTime(false, false); } }
    public EResultCode ResultCode { get { return _resultCode; } set { if (_resultCode != value) { _resultCode = value; NotifyPropertyChanged(); } } }
    public string DisqualText { get { return _disqualText; } set { if (_disqualText != value) { _disqualText = value; NotifyPropertyChanged(); } } }

    public TimeSpan? FinishTime { get { return _finishTime; } }
    public TimeSpan? StartTime { get { return _startTime; } }


    public RunResult(RaceParticipant particpant)
    {
      _participant = particpant;

      _runTime = null;
      _startTime = null;
      _finishTime = null;
      _resultCode = EResultCode.NotSet;
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
      if (original != null)
      {
        System.Diagnostics.Debug.Assert(_participant == original._participant);

        _startTime = original._startTime;
        _runTime = original._runTime;
        _finishTime = original._finishTime;
        _resultCode = original._resultCode;
        _disqualText = original._disqualText;
      }
      else
      {
        _startTime = null;
        _runTime = null;
        _finishTime = null;
        _resultCode = EResultCode.NotSet;
        _disqualText = null;
      }

      NotifyPropertyChanged(propertyName: nameof(Runtime));
      NotifyPropertyChanged(propertyName: nameof(StartTime));
      NotifyPropertyChanged(propertyName: nameof(FinishTime));
      NotifyPropertyChanged(propertyName: nameof(ResultCode));
      NotifyPropertyChanged(propertyName: nameof(DisqualText));
    }

    public bool IsEmpty()
    {
      return _startTime == null && _finishTime == null && _runTime == null && string.IsNullOrEmpty(_disqualText) && _resultCode == EResultCode.NotSet;
    }


    public void SetRunTime(TimeSpan? t, bool resetResultCode = true)
    {
      _runTime = t;

      if (resetResultCode)
        _resultCode = EResultCode.Normal;

      if (_resultCode == EResultCode.NotSet)
        _resultCode = EResultCode.Normal;

      NotifyPropertyChanged(propertyName: nameof(Runtime));
    }

    public virtual TimeSpan? GetRunTime(bool calculateIfNotStored = true, bool considerResultCode = true)
    {
      if (!considerResultCode || _resultCode == EResultCode.Normal)
      {
        if (_runTime != null)
          return (new RoundedTimeSpan((TimeSpan)_runTime, 2, RoundedTimeSpan.ERoundType.Floor)).TimeSpan;

        if (calculateIfNotStored && _startTime != null && _finishTime != null)
          return (new RoundedTimeSpan((TimeSpan)(_finishTime - _startTime), 2, RoundedTimeSpan.ERoundType.Floor)).TimeSpan;
      }

      return null;
    }


    public void SetStartTime(TimeSpan? startTime, bool resetResultCode = true)
    {
      _startTime = startTime;

      if (resetResultCode)
      {
        ResultCode = EResultCode.Normal;
        // Reset FinishTime as well if start time is newer than finishtime
        if (_startTime > _finishTime)
        {
          _finishTime = null;
          NotifyPropertyChanged(propertyName: nameof(FinishTime));
        }
      }

      if (_resultCode == EResultCode.NotSet)
        _resultCode = EResultCode.Normal;

      NotifyPropertyChanged(propertyName: nameof(StartTime));
      NotifyPropertyChanged(propertyName: nameof(Runtime));
    }

    public void SetFinishTime(TimeSpan? finishTime, bool resetResultCode = true)
    {
      _finishTime = finishTime;

      if (resetResultCode)
        ResultCode = EResultCode.Normal;

      if (_resultCode == EResultCode.NotSet)
        _resultCode = EResultCode.Normal;

      NotifyPropertyChanged(propertyName: nameof(FinishTime));
      NotifyPropertyChanged(propertyName: nameof(Runtime));
    }

    public TimeSpan? GetStartTime() { return _startTime; }
    public TimeSpan? GetFinishTime() { return _finishTime; }


    public override string ToString()
    {
      return 
        _participant.ToString() + 
        ", T: " + Runtime?.ToString(@"mm\:s\,ff") + "(" + _startTime?.ToString(@"hh\:mm\:s\,ff") + "," + _finishTime?.ToString(@"hh\:mm\:s\,ff") + ")";
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


  public static class RunResultExtension
  {
    public static string JoinDisqualifyText(string reason, string goalNumber)
    {
      string result = reason;

      if (!string.IsNullOrWhiteSpace(goalNumber))
        result += " " + goalNumber;

      return result;
    }

    public static void SplitDisqualifyText(string disqualifyText, out string reason, out string goalNumber)
    {
      goalNumber = reason = string.Empty;

      if (string.IsNullOrEmpty(disqualifyText))
        return;

      // If the string ends with a number, this returns that number
      string number = new string(disqualifyText
                          .Reverse()
                          .TakeWhile(c => char.IsDigit(c))
                          .Reverse()
                          .ToArray());

      if (!string.IsNullOrWhiteSpace(number))
      {
        goalNumber = number.Trim();
        reason = disqualifyText.Substring(0, disqualifyText.LastIndexOf(number)).Trim();
      }
      else
        reason = disqualifyText;
    }


    public static string GetDisqualifyText(this RunResult rr)
    {
      string r, g;
      SplitDisqualifyText(rr.DisqualText, out r, out g);
      return r;
    }


    public static string GetDisqualifyGoal(this RunResult rr)
    {
      string r, g;
      SplitDisqualifyText(rr.DisqualText, out r, out g);
      return g;
    }
  }

  /// <summary>
  /// Represents a RunResult with position (for a run result list)
  /// </summary>
  public class RunResultWithPosition : RunResult
  {
    private uint _position;
    private bool _justModified;
    private TimeSpan? _diffToFirst;
    private double _diffToFirstPercentage;

    public RunResultWithPosition(RunResult result) : base(result)
    {
    }

    public RunResultWithPosition(RaceParticipant rp) : base(rp)
    {
    }

    /// <summary>
    /// The position within the classement
    /// </summary>
    public uint Position
    {
      get { return _position; }
      set { if (_position != value) { _position = value; NotifyPropertyChanged(); } }
    }

    public TimeSpan? DiffToFirst
    {
      get { return _diffToFirst; }
      set { if (_diffToFirst != value) { _diffToFirst = value; NotifyPropertyChanged(); } }
    }

    public double DiffToFirstPercentage
    {
      get { return _diffToFirstPercentage; }
      set { if (_diffToFirstPercentage != value) { _diffToFirstPercentage = value; NotifyPropertyChanged(); } }
    }

    public bool JustModified
    {
      get { return _justModified; }
      set { if (_justModified != value) { _justModified = value; NotifyPropertyChanged(); } }
    }

    public override string ToString()
    {
      return
        _participant.ToString() +
        ", P:" + _position + ", T: " + Runtime?.ToString(@"mm\:s\,ff") + "(" + _startTime?.ToString(@"hh\:mm\:s\,ff") + "," + _finishTime?.ToString(@"hh\:mm\:s\,ff") + ")";
    }

  }


  /// <summary>
  /// Represents a race result. It contains out of the participant including its run results (run, time, status) and its final position within the group.
  /// </summary>
  public class RaceResultItem : INotifyPropertyChanged
  {
    public class SubResult
    {
      public SubResult(RunResultWithPosition rr)
      {
        UpdateSubResult(rr);
      }

      public bool UpdateSubResult(RunResultWithPosition rr)
      {
        bool significantChange = false;

        if (Runtime != rr.Runtime)
        {
          Runtime = rr.Runtime;
          significantChange = true;
        }

        if (RunResultCode != rr.ResultCode)
        {
          RunResultCode = rr.ResultCode;
          significantChange = true;
        }

        Position = rr.Position;
        DiffToFirst = rr.DiffToFirst;
        DiffToFirstPercentage = rr.DiffToFirstPercentage;

        return significantChange;
      }

      public TimeSpan? Runtime { get; set; }
      public RunResult.EResultCode RunResultCode { get; set; }
      public uint Position { get; set; }
      public TimeSpan? DiffToFirst { get; set; }
      public double DiffToFirstPercentage { get; set; }

      public override string ToString()
      {
        return string.Format("{0} - {1}", Runtime.ToRaceTimeString(), RunResultCode);
      }
    }

    #region private

    protected RaceParticipant _participant;
    protected Dictionary<uint, SubResult> _subResults;
    protected TimeSpan? _totalTime;
    protected RunResult.EResultCode _resultCode;
    protected string _disqualText;
    protected uint _position;
    protected TimeSpan? _diffToFirst;
    protected double _diffToFirstPercentage;
    protected double _points;
    protected bool _justModified;


    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="participant">The participant the results belong to.</param>
    public RaceResultItem(RaceParticipant participant)
    {
      _participant = participant;
      _subResults = new Dictionary<uint, SubResult>();

      _totalTime = null;
      _resultCode = RunResult.EResultCode.Normal;
      _disqualText = null;
      _position = 0;
      _diffToFirst = null;
      _diffToFirstPercentage = 0.0;
      _points = -1.0;
      _justModified = false;
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
      set { if (_totalTime != value) { _totalTime = value; NotifyPropertyChanged(); } }
    }

    public RunResult.EResultCode ResultCode 
    { 
      get { return _resultCode; } 
      set { if (_resultCode != value) { _resultCode = value; NotifyPropertyChanged(); } } 
    }

    public string DisqualText 
    { 
      get { return _disqualText; } 
      set { if (_disqualText != value) { _disqualText = value; NotifyPropertyChanged(); } } 
    }


    /// <summary>
    /// The position within the classement
    /// </summary>
    public uint Position
    {
      get { return _position; }
      set { if (_position != value) { _position = value; NotifyPropertyChanged(); } }
    }

    public TimeSpan? DiffToFirst
    {
      get { return _diffToFirst; }
      set { if (_diffToFirst != value) { _diffToFirst = value; NotifyPropertyChanged(); } }
    }

    public double DiffToFirstPercentage
    {
      get { return _diffToFirstPercentage; }
      set { if (_diffToFirstPercentage != value) { _diffToFirstPercentage = value; NotifyPropertyChanged(); } }
    }


    /// <summary>
    /// The position within the classement
    /// </summary>
    public double Points
    {
      get { return _points; }
      set { if (_points != value) { _points = value; NotifyPropertyChanged(); } }
    }

    public bool JustModified
    {
      get { return _justModified; }
      set { if (_justModified != value) { _justModified = value; NotifyPropertyChanged(); } }
    }



    public Dictionary<uint, SubResult> SubResults { get { return _subResults; } }

    /// <summary>
    /// Sets the results for one specific run
    /// </summary>
    /// <param name="run">Run number, typically either 1 or 2</param>
    /// <param name="result">The corresponding results</param>
    public bool SetRunResult(uint run, RunResultWithPosition result)
    {
      bool significantChange = false;

      if (result != null)
      {
        if (_subResults.ContainsKey(run))
        {
          if (_subResults[run].UpdateSubResult(result))
            significantChange = true;
        }
        else
        {
          _subResults[run] = new SubResult(result);
          significantChange = true;
        }
      }
      else
      {
        if (_subResults.ContainsKey(run))
        {
          _subResults.Remove(run);
          significantChange = true;
        }
      }

      NotifyPropertyChanged(nameof(SubResults));

      return significantChange;
    }


    public override string ToString()
    {
      return "P:" + _position + " (" + string.Join(",", _subResults) + ")";
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
