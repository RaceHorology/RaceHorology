using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
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


  public class LiveDateTimeEventArgs : EventArgs
  {
    public LiveDateTimeEventArgs(TimeSpan currentDayTime)
    {
      CurrentDayTime = currentDayTime;
    }

    public TimeSpan CurrentDayTime;
  }


  public delegate void LiveDateTimeChangedHandler(object sender, LiveDateTimeEventArgs e);

  /// <summary>
  /// Interface providing the current day live time synchron with the actual live time measurement
  /// </summary>
  public interface ILiveDateTimeProvider
  {
    event LiveDateTimeChangedHandler LiveDateTimeChanged;

    TimeSpan GetCurrentDayTime();
  }


  /// <summary>
  /// Reacts on the Live Timing (e.g. ALGE TdC8001) and updates the DataModel accordingly by transferring the received time data into the DataModel
  /// </summary>
  public class LiveTimingMeasurement
  {
    AppDataModel _dm;
    ILiveTimeMeasurement _liveTimer;
    ILiveDateTimeProvider _liveDateTimeProvider;
    bool _isRunning;

    public LiveTimingMeasurement(AppDataModel dm)
    {
      _dm = dm;
      _isRunning = false;
    }


    #region Public Interface

    public delegate void LiveTimingMeasurementStatusEventHandler(object sender, bool isRunning);
    public event LiveTimingMeasurementStatusEventHandler LiveTimingMeasurementStatusChanged;


    public void SetTimingDevice(ILiveTimeMeasurement liveTimer, ILiveDateTimeProvider liveDateTimeProvider)
    {
      // Cleanup if already used
      if (_liveTimer != null)
      {
        _liveTimer.TimeMeasurementReceived -= OnTimeMeasurementReceived;
        _liveTimer = null;
      }
      if (_liveDateTimeProvider != null)
      { 
        _liveDateTimeProvider.LiveDateTimeChanged += OnLiveDateTimeChanged;
        _liveDateTimeProvider = null;
      }

      _liveTimer = liveTimer;
      _liveTimer.TimeMeasurementReceived += OnTimeMeasurementReceived;

      _liveDateTimeProvider = liveDateTimeProvider;
      _liveDateTimeProvider.LiveDateTimeChanged += OnLiveDateTimeChanged;
    }


    public void Start()
    {
      _isRunning = true;

      var handler = LiveTimingMeasurementStatusChanged;
      handler?.Invoke(this, _isRunning);
    }


    public void Stop()
    {
      _isRunning = false;

      var handler = LiveTimingMeasurementStatusChanged;
      handler?.Invoke(this, _isRunning);
    }

    #endregion

    #region Internal Implementation
    private void OnTimeMeasurementReceived(object sender, TimeMeasurementEventArgs e)
    {
      if (!_isRunning)
        return;

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

    private void OnLiveDateTimeChanged(object sender, LiveDateTimeEventArgs e)
    {
      if (!_isRunning)
        return;

      _dm.SetCurrentDayTime(e.CurrentDayTime);
    }
    #endregion

  }

  /// <summary>
  /// Regularly, checks the particpants on track and sets them to NiZ if runtime is greater than secondsTillAutoNiZ
  /// </summary>
  public class LiveTimingAutoNiZ : IDisposable
  {
    RaceRun _raceRun;
    uint _secondsTillAutoNiZ;

    System.Timers.Timer _timer;

    public LiveTimingAutoNiZ(uint secondsTillAutoNiZ, RaceRun raceRun)
    {
      _secondsTillAutoNiZ = secondsTillAutoNiZ;
      _raceRun = raceRun;

      startObservation();
    }

    private void startObservation()
    {
      _timer = new System.Timers.Timer(1000);
      _timer.Elapsed += OnTimedEvent;
      _timer.AutoReset = true;
      _timer.Enabled = true;
    }

    private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
    {
      var onTrack = _raceRun.GetOnTrackList().ToArray();

      foreach ( var lr in onTrack)
      {
        if (lr.GetStartTime() != null)
        {
          TimeSpan startTime = (TimeSpan)lr.GetStartTime();
          TimeSpan curTime = _raceRun.GetRace().GetDataModel().GetCurrentDayTime();
          TimeSpan timeSinceStart = curTime - startTime;

          if (timeSinceStart.TotalSeconds > _secondsTillAutoNiZ)
            setToNiZ(lr.Participant);
        }
      }
    }

    private void setToNiZ(RaceParticipant participant)
    {
      System.Windows.Application.Current.Dispatcher.Invoke(() =>
      {
        _raceRun.SetResultCode(participant, RunResult.EResultCode.NiZ);
      });
    }



    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          _timer.Dispose();
        }

        disposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }

  /// <summary>
  /// Observes started particpants and sets not started participants to NaS if participant is not started detected by successor started participants.
  /// </summary>
  public class LiveTimingAutoNaS : IDisposable
  {
    RaceRun _raceRun;
    uint _startersTillAutoNaS;

    public LiveTimingAutoNaS(uint startersTillAutoNaS, RaceRun raceRun)
    {
      _raceRun = raceRun;
      _startersTillAutoNaS = startersTillAutoNaS;

      _raceRun.OnTrackChanged += OnSomethingChanged;
    }


    private void OnSomethingChanged(object sender, RaceParticipant participantEnteredTrack, RaceParticipant participantLeftTrack)
    {
      // Copy starters (copy to avoid any side effects)
      StartListEntry[] starters = _raceRun.GetStartListProvider().GetViewList().ToArray();

      // Participant enters track
      if (participantEnteredTrack != null)
      {
        // Loop over StartList until the starter has been found, remember all not started participants
        List<StartListEntry> toPurge = new List<StartListEntry>();
        foreach (StartListEntry se in starters)
        {
          if (se.Participant == participantEnteredTrack)
            break;

          toPurge.Add(se);
        }

        // Loop 
        for (int i = 0; i < toPurge.Count() - Math.Abs(_startersTillAutoNaS); i++)
        {
          RaceParticipant rp = toPurge[i].Participant;
          if (!_raceRun.IsOrWasOnTrack(rp))
            _raceRun.SetResultCode(rp, RunResult.EResultCode.NaS);
        }
      }
    }


    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // TODO: dispose managed state (managed objects).
          _raceRun.OnTrackChanged -= OnSomethingChanged;
        }

        disposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }

}
