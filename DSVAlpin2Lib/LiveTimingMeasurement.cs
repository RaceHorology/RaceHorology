using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{

  public class TimeMeasurementEventArgs : EventArgs
  {
    public TimeMeasurementEventArgs()
    {
      StartNumber = 0;
      RunTime = null;
      BRunTime = false;
      StartTime = null;
      BStartTime = false;
      FinishTime = null;
      BFinishTime = false;
    }

    public uint StartNumber;
    public TimeSpan? RunTime;    // if null and corresponding time property is set true => time shall be deleted
    public bool BRunTime;        // true if RunTime is set
    public TimeSpan? StartTime;  // if null and corresponding time property is set true => time shall be deleted
    public bool BStartTime;      // true if StartTime is set
    public TimeSpan? FinishTime; // if null and corresponding time property is set true => time shall be deleted
    public bool BFinishTime;     // true if FinishTime is set
  }

  public delegate void TimeMeasurementEventHandler(object sender, TimeMeasurementEventArgs e);

  public interface ILiveTimeMeasurement
  {
    /// <summary>
    /// If a time measurement happend, this event is triggered
    /// </summary>
    event TimeMeasurementEventHandler TimeMeasurementReceived;

    void Start();
    void Stop();

  }



  public class LiveTimingMeasurement
  {
    AppDataModel _dm;
    ILiveTimeMeasurement _liveTimer;

    public LiveTimingMeasurement(AppDataModel dm, ILiveTimeMeasurement liveTimer)
    {
      _dm = dm;
      _liveTimer = liveTimer;

      _liveTimer.TimeMeasurementReceived += OnTimeMeasurementReceived;
    }


    private void OnTimeMeasurementReceived(object sender, TimeMeasurementEventArgs e)
    {
      Race currentRace = _dm.GetCurrentRace();
      RaceRun currentRaceRun = _dm.GetCurrentRaceRun();
      RaceParticipant participant = currentRace.GetParticipant(e.StartNumber);

      System.Windows.Application.Current.Dispatcher.Invoke(() =>
      {
        if (participant != null)
        {

          if (e.BStartTime)
            currentRaceRun.SetStartTime(participant, e.StartTime);

          if (e.BFinishTime)
            currentRaceRun.SetFinishTime(participant, e.FinishTime);

          if (e.BRunTime)
            currentRaceRun.SetRunTime(participant, e.RunTime);
        }
      });
    }



  }
}
