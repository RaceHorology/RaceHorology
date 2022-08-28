using System;
using System.Collections.Generic;
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
    private TimeMeasurementEventArgs _orgTimeData;
    private uint _startnumber;

    public Timestamp(TimeSpan timeStamp, TimeMeasurementEventArgs orgTimeData, uint startnumber = 0)
    {
      _timeStamp = timeStamp;
      _orgTimeData = orgTimeData;
      _startnumber = startnumber;
    }

    public TimeSpan Time
    {
      get => _timeStamp;
    }

    public TimeMeasurementEventArgs OrgTimeData
    {
      get => _orgTimeData;
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


  public class LiveTimeParticipantAssigning : ILiveTimeMeasurementDeviceBase, IDisposable
  {
    public enum EMeasurementPoint { Undefined, Start, Finish };
    private ILiveTimeMeasurementDevice _timeMeasurementDevice;
    private EMeasurementPoint _measurementPoint;
    private ItemsChangeObservableCollection<Timestamp> _timestamps;
    private System.Threading.SynchronizationContext _syncContext;

    private IComparer<Timestamp> _sorter = new TimestampComparerDesc();

    public LiveTimeParticipantAssigning(ILiveTimeMeasurementDevice timeMeasurementDevice, EMeasurementPoint measurementPoint)
    {
      _syncContext = System.Threading.SynchronizationContext.Current;

      _timeMeasurementDevice = timeMeasurementDevice;
      _measurementPoint = measurementPoint;
      _timestamps = new ItemsChangeObservableCollection<Timestamp>();

      _timeMeasurementDevice.TimeMeasurementReceived += timeMeasurementDevice_TimeMeasurementReceived;
    }

    public void Dispose()
    {
      _timeMeasurementDevice.TimeMeasurementReceived -= timeMeasurementDevice_TimeMeasurementReceived;
    }


    private void timeMeasurementDevice_TimeMeasurementReceived(object sender, TimeMeasurementEventArgs e)
    {
      _syncContext.Send(delegate
      {
        var measurementPoint = e.BStartTime ? EMeasurementPoint.Start: e.BFinishTime ? EMeasurementPoint.Finish : EMeasurementPoint.Undefined;
        var time = e.BStartTime ? e.StartTime : e.BFinishTime ? e.FinishTime : null;
        if ( (_measurementPoint == EMeasurementPoint.Undefined || measurementPoint == _measurementPoint) 
             && time != null)
        {
          var ts = new Timestamp((TimeSpan)time, e, e.StartNumber);

          ts.PropertyChanged += timeStamp_PropertyChanged;

          _timestamps.Insert(0, ts);
          _timestamps.Sort<Timestamp>(_sorter);
        }
      }, null);
    }

    private void timeStamp_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender is Timestamp ts)
      {
        NotifyChange(ts, ts.StartNumber);
      }
    }

    public ItemsChangeObservableCollection<Timestamp> Timestamps
    {
      get { return _timestamps; }
    }

    public void NotifyChange(Timestamp timestamp, uint startnumber)
    {
      if (_timestamps.FirstOrDefault(item => item == timestamp) == null) // Just check whether the item is in the container
        return;

      // Trigger TimeMeasurementReceived event with updated startnumber
      var handle = TimeMeasurementReceived;
      var newEvent = createTimeMeasurement(timestamp);
      newEvent.Valid = true; // Make this event a valid one, because it's intended to be (manuelly triggered)
      handle?.Invoke(this, newEvent);
    }

    private TimeMeasurementEventArgs createTimeMeasurement(Timestamp timestamp)
    {
      var data = new TimeMeasurementEventArgs(timestamp.OrgTimeData);
      data.StartNumber = timestamp.StartNumber;
      return data;
    }


    #region Implementation of ILiveTimeMeasurementDeviceBase

    public event TimeMeasurementEventHandler TimeMeasurementReceived;

    #endregion


  }
}
