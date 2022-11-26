using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  public class Timestamp : INotifyPropertyChanged
  {
    private TimeSpan _timeStamp;
    private EMeasurementPoint _measurementPoint;
    private uint _startnumber;
    private bool _valid;

    public Timestamp(TimeSpan timeStamp, EMeasurementPoint measurementPoint, uint startnumber = 0, bool valid = true)
    {
      _timeStamp = timeStamp;
      _startnumber = startnumber;
      _measurementPoint = measurementPoint;
      _valid = valid;
    }

    public Timestamp(TimeMeasurementEventArgs orgTimeData)
    {
      _startnumber = orgTimeData.StartNumber;

      if (orgTimeData.StartTime != null && orgTimeData.FinishTime == null)
      {
        _measurementPoint = EMeasurementPoint.Start;
        _timeStamp = (TimeSpan) orgTimeData.StartTime;
      }
      else if (orgTimeData.StartTime == null && orgTimeData.FinishTime != null)
      {
        _measurementPoint = EMeasurementPoint.Finish;
        _timeStamp = (TimeSpan)orgTimeData.FinishTime;
      }
      else
      {
        _measurementPoint = EMeasurementPoint.Undefined;
      }

      _valid = orgTimeData.Valid;
    }

    public TimeSpan Time
    {
      get => _timeStamp;
    }

    public EMeasurementPoint MeasurementPoint
    {
      get => _measurementPoint;
    }


    public bool Valid
    {
      get => _valid;
      set { if (_valid != value) { _valid = value; NotifyPropertyChanged(); } }
    }

    public uint StartNumber
    {
      get => _startnumber;
      set { if (_startnumber != value) { _startnumber = value; NotifyPropertyChanged(); } }
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


  internal class TimestampComparerDesc : IComparer<Timestamp>
  {
    public int Compare(Timestamp a, Timestamp b)
    {
      return b.Time.CompareTo(a.Time);
    }
  }


  public class LiveTimeParticipantAssigning : IDisposable
  {
    private RaceRun _raceRun;
    private EMeasurementPoint _measurementPoint;
    private ObservableCollection<Timestamp> _timestamps;

    private IComparer<Timestamp> _sorter = new TimestampComparerDesc();

    public LiveTimeParticipantAssigning(RaceRun rr, EMeasurementPoint measurementPoint)
    {
      _raceRun = rr;
      _measurementPoint = measurementPoint;
      _timestamps = new FilterObservableCollection<Timestamp>(rr.GetTimestamps(), (v) => { return v.MeasurementPoint == measurementPoint; });
    }

    public void Dispose()
    {
    }


    public ObservableCollection<Timestamp> Timestamps
    {
      get { return _timestamps; }
    }


    public void Assign(Timestamp timestamp, uint startnumber)
    {
      if (_timestamps.FirstOrDefault(item => item == timestamp) == null) // Just check whether the item is in the container
        return;

      // Check if the startnumber is already used by another timestamp
      invalidateOtherWithSameStartnumber(timestamp, startnumber);

      timestamp.StartNumber = startnumber;
      timestamp.Valid = true;

      var rp = _raceRun.GetRace().GetParticipant(startnumber);
      if (rp != null)
        _raceRun.SetTime(_measurementPoint, rp, timestamp.Time);
    }

    private void invalidateOtherWithSameStartnumber(Timestamp timestamp, uint startnumber)
    {
      foreach (var inUse in _timestamps.Where(item => item.StartNumber == startnumber))
      {
        if (inUse != null && inUse != timestamp)
          inUse.Valid = false;
      }
    }
  }
}
