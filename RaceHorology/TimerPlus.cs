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


    public TimerPlus(TimerPlusCallback onTimeOut, TimerPlusCallback onUpdate, int timeOut, bool oneShot)
    {
      _onTimeOut = onTimeOut;
      _onUpdate = onUpdate;

      _timerTime = new TimeSpan(0, 0, timeOut);
      _oneShot = oneShot;

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


    public void Start()
    {
      _lastTime = DateTime.Now;
      _timer.Start();
    }

    public void Stop()
    {
      _timer.Stop();
    }

    public void Reset()
    {
      _remainingTime = _timerTime;
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
