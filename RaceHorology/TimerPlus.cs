/*
 *  Copyright (C) 2019 - 2023 by Sven Flossmann
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
using System.Timers;

namespace RaceHorology
{
  public class TimerPlus : IDisposable
  {
    public delegate void TimerPlusCallback();

    TimerPlusCallback _onTimeOut, _onUpdate;
    TimeSpan _timerTime;
    bool _oneShot;

    private Timer _timer;
    DateTime _lastTime;
    TimeSpan _remainingTime;
    private bool _isRunning;


    public TimerPlus(TimerPlusCallback onTimeOut, TimerPlusCallback onUpdate, int timeOut, bool oneShot)
    {
      _onTimeOut = onTimeOut;
      _onUpdate = onUpdate;

      _timerTime = new TimeSpan(0, 0, timeOut);
      _oneShot = oneShot;
      _isRunning = false;

      _timer = new Timer(200); // Update every 200ms
      _timer.Elapsed += OnTimedEvent;
      _timer.AutoReset = true;

      Reset();
    }


    public TimeSpan RemainingTime
    {
      get { return _remainingTime; }
    }
    public long RemainingSeconds
    {
      get { return (long)(_remainingTime.TotalSeconds); }
    }

    public bool IsRunning
    {
      get { return _isRunning; }
      private set { _isRunning = value; }
    }


    public void Start()
    {
      _lastTime = DateTime.Now;
      _timer.Start();
      _isRunning = true;
    }

    public void Stop()
    {
      _isRunning = false;
      _timer.Stop();
    }

    public void Reset()
    {
      _remainingTime = _timerTime;
      _isRunning = false;
    }


    private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
    {
      DateTime curTime = DateTime.Now;

      _remainingTime -= (curTime - _lastTime);
      _lastTime = curTime;

      _onUpdate?.Invoke();

      if (_remainingTime < new TimeSpan(0))
      {
        _remainingTime = new TimeSpan(0);
        _onTimeOut?.Invoke();

        if (_oneShot)
          _timer.Stop();
        else
          Reset();
      }
    }


    public void Dispose()
    {
      _timer.Dispose();
    }
  }
}
