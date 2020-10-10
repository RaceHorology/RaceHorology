/*
 *  Copyright (C) 2019 - 2020 by Sven Flossmann
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public class HandTiming
  {
    public static IHandTiming CreateHandTiming(string device, string devicePort)
    {
      IHandTiming handTiming = null;

      switch (device)
      {
        case "ALGETimy":
          handTiming = new ALGETimy(devicePort);
          break;
        case "TagHeuerPPro":
          handTiming = new TagHeuer(devicePort);
          break;
        case "File":
          handTiming = new FromFileHandTiming(devicePort);
          break;
      }

      return handTiming;
    }

  }



  public class HandTimingVMEntry
  {
    public enum ETimeModus { EStartTime, EFinishTime };


    public uint? StartNumber { get { return _runResult?.StartNumber; } }
    public TimeSpan? StartTime { get { return _runResult?.GetStartTime(); } }
    public TimeSpan? FinishTime { get { return _runResult?.GetFinishTime(); } }
    public TimeSpan? RunTime { get { return _runResult?.GetRunTime(true, false); } }

    /// Returns either StartTime or FinishTime depending on timeModus
    public TimeSpan? ATime { get { return _timeModus == ETimeModus.EStartTime ? StartTime : FinishTime; } }

    public TimeSpan? HandTime { get { return _handTime; } }
    public TimeSpan? HandTimeDiff { get { return _handTimeDiff; } }

    public ETimeModus TimeModus
    {
      get { return _timeModus; }
      set { if (_timeModus != value) _timeModus = value; }
    }

    RunResult _runResult;
    TimeSpan? _handTime;
    TimeSpan? _handTimeDiff;
    ETimeModus _timeModus;

    public HandTimingVMEntry(ETimeModus timeModus, RunResult runResult, TimeSpan? handTime)
    {
      _timeModus = timeModus;
      _handTime = handTime;
      _runResult = runResult;
    }

    public void SetRunResult(RunResult runResult)
    {
      _runResult = runResult;
      updateInternal();
    }

    public void SetHandTime(TimeSpan? handTime)
    {
      _handTime = handTime;
      updateInternal();
    }

    private void updateInternal()
    {
      if (_handTime != null && ATime != null)
      {
        var t1 = new RoundedTimeSpan((TimeSpan)_handTime, 2, RoundedTimeSpan.ERoundType.Floor);
        var t2 = new RoundedTimeSpan((TimeSpan)ATime, 2, RoundedTimeSpan.ERoundType.Floor);

        _handTimeDiff = t1.TimeSpan.Subtract(t2.TimeSpan);
      }
      else
        _handTimeDiff = null;
    }
  }


  internal class HandTimingVMEntrySorter : IComparer<HandTimingVMEntry>
  {
    NullEnabledComparer _nullComparer = new NullEnabledComparer();

    public int Compare(HandTimingVMEntry x, HandTimingVMEntry y)
    {
      TimeSpan? tX = x.ATime != null ? x.ATime : x.HandTime;
      TimeSpan? tY = y.ATime != null ? y.ATime : y.HandTime;

      int s = _nullComparer.Compare(tX, tY);
      if (s != 0)
        return s;

      return _nullComparer.Compare(x.StartNumber, y.StartNumber);
    }
  }

  public class HandTimingVM
  {
    HandTimingVMEntry.ETimeModus _timeModus;
    ObservableCollection<HandTimingVMEntry> _handTimings;
    HandTimingVMEntrySorter _handTimingsSorter;

    public ObservableCollection<HandTimingVMEntry> Items { get { return _handTimings; } }

    public HandTimingVM(HandTimingVMEntry.ETimeModus timeModus)
    {
      _timeModus = timeModus;
      _handTimings = new ObservableCollection<HandTimingVMEntry>();
      _handTimingsSorter = new HandTimingVMEntrySorter();
    }


    public HandTimingVMEntry.ETimeModus TimeModus
    {
      get { return _timeModus; }
      set 
      { 
        if (_timeModus != value)
        {
          _timeModus = value;
          foreach (var v in _handTimings)
            v.TimeModus = _timeModus;

          _handTimings.Sort(_handTimingsSorter);
        }
      }

    }


    public void AddRunResults(IEnumerable<RunResult> runResults)
    { 
      foreach(var rr in runResults)
      {
        HandTimingVMEntry e = findEntry(rr);
        if (e != null && e.ATime == null)
        {
          e.SetRunResult(rr);
        }
        else
        {
          _handTimings.InsertSorted(new HandTimingVMEntry(_timeModus, rr, null), _handTimingsSorter);
        }
      }
      _handTimings.Sort(_handTimingsSorter);
    }

    public void AddHandTimings(IEnumerable<TimingData> handTimings)
    {
      foreach (var ht in handTimings)
      {
        HandTimingVMEntry e = findEntry(ht);
        if (e != null && e.HandTime == null)
        {
          e.SetHandTime(ht.Time);
        }
        else
        {
          _handTimings.InsertSorted(new HandTimingVMEntry(_timeModus, null, ht.Time), _handTimingsSorter);
        }
      }
      _handTimings.Sort(_handTimingsSorter);
    }


    private TimeSpan? getATime(RunResult rr)
    {
      return _timeModus == HandTimingVMEntry.ETimeModus.EStartTime ? rr.GetStartTime() : rr.GetFinishTime();
    }

    public HandTimingVMEntry findEntry(RunResult rr)
    {
      foreach (var e in _handTimings)
      {
        if (e.HandTime == null)
          continue;

        TimeSpan? aTime = getATime(rr);
        if (aTime == null)
          continue;

        if ( Math.Abs(((TimeSpan)aTime).Subtract((TimeSpan)e.HandTime).TotalMilliseconds) < 1000)
        {
          return e;
        }
      }
      return null;
    }


    public HandTimingVMEntry findEntry(TimingData td)
    {
      foreach (var e in _handTimings)
      {
        if (e.ATime == null)
          continue;

        TimeSpan? hTime = td.Time;
        if (hTime == null)
          continue;

        if (Math.Abs(((TimeSpan)hTime).Subtract((TimeSpan)e.ATime).TotalMilliseconds) < 1000)
        {
          return e;
        }
      }
      return null;
    }

  }


}
