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

  public class LiveTimeParticipantAssigning : ILiveTimeMeasurementDeviceBase, IDisposable
  {
    private ILiveTimeMeasurementDevice _timeMeasurementDevice;
    private ItemsChangeObservableCollection<Timestamp> _timestamps;

    public LiveTimeParticipantAssigning(ILiveTimeMeasurementDevice timeMeasurementDevice)
    {
      _timeMeasurementDevice = timeMeasurementDevice;
      _timestamps = new ItemsChangeObservableCollection<Timestamp>();

      _timeMeasurementDevice.TimeMeasurementReceived += timeMeasurementDevice_TimeMeasurementReceived;
    }

    public void Dispose()
    {
      _timeMeasurementDevice.TimeMeasurementReceived -= timeMeasurementDevice_TimeMeasurementReceived;
    }


    private void timeMeasurementDevice_TimeMeasurementReceived(object sender, TimeMeasurementEventArgs e)
    {
      var time = e.BStartTime ? e.StartTime : e.BFinishTime ? e.FinishTime : null;
      if (time != null)
      {
        var ts = new Timestamp((TimeSpan)time, e, e.StartNumber);
        _timestamps.Add(ts);
      }
    }

    public ItemsChangeObservableCollection<Timestamp> Timestamps
    {
      get { return _timestamps; }
    }

    public void Assign(Timestamp timestamp, uint startnumber)
    {
      if (_timestamps.FirstOrDefault(item => item == timestamp) == null) // Just check whether the item is in the container
        return;

      // Check if the startnumber is already used by another timestamp
      var inUse = startnumber == 0 ? null : _timestamps.FirstOrDefault(item => item.StartNumber == startnumber);
      if (inUse != null && inUse != timestamp)
        inUse.StartNumber = 0;

      timestamp.StartNumber = startnumber;

      // Trigger TimeMeasurementReceived event with updated startnumber
      var handle = TimeMeasurementReceived;
      handle?.Invoke(this, createTimeMeasurement(timestamp));
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
