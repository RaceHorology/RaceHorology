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
    private Timestamp _timeStamp;
    private TimeMeasurementEventArgs _orgTimeData;
    private uint _startnumber;

    public Timestamp(Timestamp timeStamp, TimeMeasurementEventArgs orgTimeData, uint startnumber = 0, RaceParticipant participant = null)
    {
      _timeStamp = timeStamp;
      _orgTimeData = orgTimeData;
      _startnumber = startnumber;
    }

    public Timestamp Time
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

  public class LiveTimeParticipantAssigning
  {
  }
}
