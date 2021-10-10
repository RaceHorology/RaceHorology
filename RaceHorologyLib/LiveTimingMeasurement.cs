﻿/*
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  /// <summary>
  /// Main data structure to transfer the measured time data from the Live Timing Measurement Device to the application 
  /// It contains all fields to identify the runner, start / finish time as well as potentailly calculated runtime.
  /// 
  /// Note: Intermediate not yet supported
  /// </summary>
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
  public delegate void LiveTimingMeasurementDeviceStatusEventHandler(object sender, bool isRunning);


  /// <summary>
  /// This Interface must be implemented from a Live Timing Measurement Device which supports online time measurement during the race.
  /// Most favourit device for now are the ALGE TdC8000/TdC8001 or ALGE Timy
  /// </summary>
  public interface ILiveTimeMeasurementDevice
  {
    /// <summary>
    /// If a time measurement happend, this event must be triggered.
    /// </summary>
    event TimeMeasurementEventHandler TimeMeasurementReceived;

    /// <summary>
    /// Starts the timing device to measure.
    /// </summary>
    void Start();
    /// <summary>
    /// Stops the timing device to measure.
    /// </summary>
    void Stop();

    /// <summary>
    /// Status property to get the real status of the measuring device. Might return false even if Start() has been called.
    /// IsOnline shall return true if everything works as expected and the device is connected / online.
    /// </summary>
    bool IsOnline { get; }
    
    /// <summary>
    /// This event must be fired if the status of the device changed.
    /// </summary>
    event LiveTimingMeasurementDeviceStatusEventHandler StatusChanged;
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
  /// Reacts on the Live Timing Device (e.g. ALGE TdC8001) and updates the DataModel accordingly by transferring the received time data into the DataModel
  /// This is the main implementation for performaing the time measurement.
  /// </summary>
  public class LiveTimingMeasurement
  {
    AppDataModel _dm;
    SynchronizationContext _syncContext;

    ILiveTimeMeasurementDevice _liveTimer;
    ILiveDateTimeProvider _liveDateTimeProvider;
    bool _isRunning;
    bool _autoAddParticipants;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dm">The DataMOdel to work on.</param>
    /// <param name="autoAddParticipants">true in case participants with a startnumber not existing shall be created.</param>
    public LiveTimingMeasurement(AppDataModel dm, bool autoAddParticipants = false)
    {
      _dm = dm;
      _syncContext = System.Threading.SynchronizationContext.Current;
      _isRunning = false;
      _autoAddParticipants = autoAddParticipants;
    }

    #region Public Interface

    public bool AutoAddParticipants { get { return _autoAddParticipants; } set { _autoAddParticipants = value; } }

    public delegate void LiveTimingMeasurementStatusEventHandler(object sender, bool isRunning);
    public event LiveTimingMeasurementStatusEventHandler LiveTimingMeasurementStatusChanged;


    /// <summary>
    /// Sets the Timing Device to use
    /// </summary>
    public void SetTimingDevice(ILiveTimeMeasurementDevice liveTimer, ILiveDateTimeProvider liveDateTimeProvider)
    {
      // Cleanup if already used
      if (_liveTimer != null)
      {
        _liveTimer.TimeMeasurementReceived -= OnTimeMeasurementReceived;
        liveTimer.StatusChanged -= OnTimerStatusChanged;
        _liveTimer = null;
      }
      if (_liveDateTimeProvider != null)
      { 
        _liveDateTimeProvider.LiveDateTimeChanged += OnLiveDateTimeChanged;
        _liveDateTimeProvider = null;
      }

      _liveTimer = liveTimer;
      _liveTimer.TimeMeasurementReceived += OnTimeMeasurementReceived;
      _liveTimer.StatusChanged += OnTimerStatusChanged;

      _liveDateTimeProvider = liveDateTimeProvider;
      _liveDateTimeProvider.LiveDateTimeChanged += OnLiveDateTimeChanged;
    }

    /// <summary>
    /// Property to get the used timing device
    /// </summary>
    public ILiveTimeMeasurementDevice LiveTimingDevice { get => _liveTimer; }

    public void Start()
    {
      _isRunning = true;

      var handler = LiveTimingMeasurementStatusChanged;
      handler?.Invoke(this, IsRunning);
    }


    public void Stop()
    {
      _isRunning = false;

      var handler = LiveTimingMeasurementStatusChanged;
      handler?.Invoke(this, IsRunning);
    }

    public bool IsRunning { get => _isRunning && _liveTimer?.IsOnline == true; }

    #endregion

    #region Internal Implementation

    /// <summary>
    /// Callback for the timing device to react on (e.g. disconnected, stopped, ...)
    /// </summary>
    private void OnTimerStatusChanged(object sender, bool isRunning)
    {
      // Just forward changes on status
      var handler = LiveTimingMeasurementStatusChanged;
      handler?.Invoke(this, IsRunning);
    }

    /// <summary>
    /// Callback of the timing device in case of timing data received 
    /// </summary>
    private void OnTimeMeasurementReceived(object sender, TimeMeasurementEventArgs e)
    {
      if (!_isRunning)
        return;

      _syncContext.Send(delegate
      {
        Race currentRace = _dm.GetCurrentRace();
        RaceRun currentRaceRun = _dm.GetCurrentRaceRun();
        RaceParticipant participant = currentRace.GetParticipant(e.StartNumber);

        // Create participant if desired
        if (participant == null)
          participant = createParticipantIfDesired(currentRace, e.StartNumber);

        if (participant != null)
        {

          if (e.BStartTime)
            currentRaceRun.SetStartTime(participant, e.StartTime);

          if (e.BFinishTime)
            currentRaceRun.SetFinishTime(participant, e.FinishTime);

          if (e.BRunTime)
            currentRaceRun.SetRunTime(participant, e.RunTime);
        }
      }, null);
    }


    /// <summary>
    /// Callback to sync the clock with the clock of the timing device
    /// </summary>
    private void OnLiveDateTimeChanged(object sender, LiveDateTimeEventArgs e)
    {
      if (!_isRunning)
        return;

      _dm.SetCurrentDayTime(e.CurrentDayTime);
    }


    /// <summary>
    /// Creates a default participant in case the startnumber was unknown
    /// </summary>
    private RaceParticipant createParticipantIfDesired(Race race, uint startNumber)
    {
      if (!_autoAddParticipants)
        return null;

      Participant p = new Participant
      {
        Name = "Automatisch",
        Firstname = "Erzeugt"
      };

      _dm.GetParticipants().Add(p);

      return race.AddParticipant(p, startNumber);
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

      _raceRun.OnTrackChanged += OnTrackChanged;

      _raceRun.InFinishChanged += OnFinishChanged;
    }


    private void OnFinishChanged(object sender, RaceParticipant participantEnteredTrack, RaceParticipant participantLeftTrack, RunResult currentRunResult)
    {
      processStartListTill(participantEnteredTrack);
    }


    private void OnTrackChanged(object sender, RaceParticipant participantEnteredTrack, RaceParticipant participantLeftTrack, RunResult currentRunResult)
    {
      processStartListTill(participantEnteredTrack);
    }

    private void processStartListTill(RaceParticipant participant)
    {
      // Copy starters (copy to avoid any side effects)
      StartListEntry[] starters = _raceRun.GetStartListProvider().GetViewList().ToArray();

      // Participant enters track
      if (participant != null)
      {
        // Loop over StartList until the starter has been found, remember all not started participants
        List<StartListEntry> toPurge = new List<StartListEntry>();
        foreach (StartListEntry se in starters)
        {
          if (se.Participant == participant)
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
          _raceRun.OnTrackChanged -= OnTrackChanged;
          _raceRun.InFinishChanged -= OnFinishChanged;
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
