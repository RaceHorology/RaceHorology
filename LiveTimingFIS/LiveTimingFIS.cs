/*
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

using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LiveTimingFIS
{

  internal class IsCompleteObserver : IDisposable
  {
    RaceRun _previousRaceRun;
    RaceRun _raceRun;
    Action<RaceRun, RaceRun> _updateStartListFunc;
    bool disposedValue;

    public IsCompleteObserver(RaceRun previousRaceRun, RaceRun raceRun, Action<RaceRun, RaceRun> updateStartListFunc)
    {
      _previousRaceRun = previousRaceRun;
      _raceRun = raceRun;
      _updateStartListFunc = updateStartListFunc;

      _previousRaceRun.PropertyChanged += OnChanged;
    }

    private void OnChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "IsComplete")
        _updateStartListFunc(_previousRaceRun, _raceRun);
    }

    #region IDisposable implementation
    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          _previousRaceRun.PropertyChanged -= OnChanged;
        }

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }



  internal class LiveTimingDelegator : IDisposable
  {
    Race _race;
    LiveTimingFIS _liveTiming;

    List<IDisposable> _notifier;

    public LiveTimingDelegator(Race race, LiveTimingFIS liveTiming)
    {
      _race = race;
      _liveTiming = liveTiming;

      _notifier = new List<IDisposable>();

      observeRace();
    }


    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          foreach (var n in _notifier)
            n.Dispose();
        }

        disposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // TODO: uncomment the following line if the finalizer is overridden above.
      // GC.SuppressFinalize(this);
    }
    #endregion


    private void observeRace()
    {
      RaceRun rLast = null;
      foreach (var r in _race.GetRuns())
      {
        System.Diagnostics.Debug.Assert(rLast == null || rLast.Run < r.Run, "previous run number must be smaller than current run number");
        observeRaceRun(rLast, r);
        rLast = r;
      }
    }


    private void observeRaceRun(RaceRun previousRaceRun, RaceRun raceRun)
    {
      ItemsChangedNotifier startListNotifier = new ItemsChangedNotifier(raceRun.GetStartListProvider().GetViewList());
      startListNotifier.ItemChanged += (o, e) =>
      {
        updateStartList(previousRaceRun, raceRun);
      };
      updateStartList(previousRaceRun, raceRun); // Initial update
      _notifier.Add(startListNotifier);

      if (previousRaceRun != null)
      {
        IsCompleteObserver completeObserver = new IsCompleteObserver(previousRaceRun, raceRun, updateStartList);
        _notifier.Add(completeObserver);
      }

      // Results
      _liveTiming.UpdateResults(raceRun); // Initial update


      ItemsChangedNotifier resultsNotifier = new ItemsChangedNotifier(raceRun.GetResultList());
      resultsNotifier.ItemChanged += (sender, e) =>
      {
        if (sender is RunResult rr)
        {
          Task.Delay(new TimeSpan(0, 0, 0, 0, 200)).ContinueWith(o =>
          {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
              _liveTiming.UpdateInFinish(raceRun, rr.Participant);
            });
          });
        }
      };
      _notifier.Add(resultsNotifier);
      //raceRun.InFinishChanged += raceRun_InFinishChanged;

      raceRun.OnTrackChanged += raceRun_OnTrackChanged;
    }


    private void raceRun_OnTrackChanged(RaceRun raceRun, RaceParticipant participantEnteredTrack, RaceParticipant participantLeftTrack, RunResult currentRunResult)
    {
      if (participantEnteredTrack != null)
      {
        _liveTiming.UpdateOnTrack(raceRun, participantEnteredTrack);
        updateNextStarter(raceRun);
      }
    }


    private void raceRun_InFinishChanged(RaceRun raceRun, RaceParticipant participantEnteredTrack, RaceParticipant participantLeftTrack, RunResult currentRunResult)
    {
      if (participantEnteredTrack != null)
      {
        // Uses following workaround to get consistent results:
        // - wait some time
        // - use race results to get data
        Task.Delay(new TimeSpan(0, 0, 0, 0, 200)).ContinueWith(o =>
       {
         System.Windows.Application.Current.Dispatcher.Invoke(() =>
         {
           _liveTiming.UpdateInFinish(raceRun, participantEnteredTrack);
         });
       });
      }

      if (participantLeftTrack != null)
      {

      }
    }


    private void updateStartList(RaceRun previousRaceRun, RaceRun raceRun)
    {
      if (previousRaceRun == null || previousRaceRun.IsComplete)
      {
        _liveTiming.UpdateStartList(raceRun);
        updateNextStarter(raceRun);
      }
    }

    private void updateNextStarter(RaceRun raceRun)
    {
      // Find current starter
      foreach (var sle in raceRun.GetStartListProvider().GetViewList())
      {
        if (!raceRun.IsOrWasOnTrack(sle.Participant))
        {
          _liveTiming.UpdateOnStart(raceRun, sle.Participant);
          break;
        }
      }
    }

  }






  public class LiveTimingFIS : ILiveTiming, IDisposable
  {
    protected class Utf8StringWriter : StringWriter
    {
      // Use UTF8 encoding 
      public override Encoding Encoding
      {
        get { return new UTF8Encoding(false); }
      }
    }


    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    string _fisRaceCode;
    string _fisCategory;
    string _fisPassword;

    string _fisHostName;
    int _fisPort;
    int _sequence;
    System.Net.Sockets.TcpClient _tcpClient;
    System.Threading.Thread _receiveThread;
    System.Timers.Timer _keepAliveTimer;


    bool _isLoggedOn;
    bool _started;
    string _statusText;

    LiveTimingDelegator _delegator;


    public LiveTimingFIS()
    {
      _isLoggedOn = false;
      _started = false;

      setUpXmlFormat();
    }


    #region IDisposable Implementation
    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          Disconnect();
        }

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
    #endregion

    private Race _race;
    public Race Race
    {
      set
      { 
        if (Connected)
          throw new Exception("Race cannot be set if already connected");

        _race = value; 
      }
      
      get 
      { 
        return _race; 
      }
    }


    public void Connect(int fisPort)
    {
      if (_tcpClient != null)
        return;

      _fisHostName = "live.fisski.com";
      _fisPort = fisPort;

      _sequence = 0;
      try
      {
        _tcpClient = new System.Net.Sockets.TcpClient();
        _tcpClient.Connect(_fisHostName, _fisPort);

        // Spawn receive thread
        _receiveThread = new System.Threading.Thread(receiveMethod);
        _receiveThread.Start();

        _keepAliveTimer = new System.Timers.Timer();
        _keepAliveTimer.Elapsed += keepAliveTimer_Elapsed;
        //_keepAliveTimer.Interval = 1000; // for testing
        _keepAliveTimer.Interval = 5*60*1000; // 5 minutes * 60s * 1000ms
        _keepAliveTimer.AutoReset = true;
        _keepAliveTimer.Enabled = true;
      }
      catch (Exception e)
      {
        Logger.Warn(e, "Connect to {0} on port {1} failed", _fisHostName, _fisPort);
        _tcpClient.Dispose();
        _tcpClient = null;
        throw; // re-throw
      }
    }

    private void keepAliveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      scheduleTransfer(new LTTransfer(getXmlKeepAlive(), _tcpClient));
    }

    public void Disconnect()
    {
      if (_tcpClient == null)
        return;

      Stop(); // Stop first

      _tcpClient.Dispose();
      _tcpClient = null;

      _receiveThread.Join();
      _receiveThread = null;
    }

    public bool Connected { get { return _tcpClient != null && _tcpClient.Connected; } }


    public void Login(string fisRaceCode, string fisCategory, string fisPassword)
    {
      if (_isLoggedOn)
        return;

      _fisRaceCode = fisRaceCode;
      _fisCategory = fisCategory;
      _fisPassword = fisPassword;

      scheduleTransfer(new LTTransfer(getXmlClearRace(), _tcpClient));
      scheduleTransfer(new LTTransfer(getXmlStatusUpdateInfo(""), _tcpClient));
      scheduleTransfer(new LTTransfer(getXmlRaceInfo(_race), _tcpClient));
      _isLoggedOn = true;
    }

    public void Start()
    {
      if (_started)
        return;

      _started = true;

      // Observes for changes and triggers UpdateMethods, also sends data initially
      _delegator = new LiveTimingDelegator(_race, this);

      // Figure out the right run and set it
      foreach (var r in _race.GetRuns())
      {
        if (!r.IsComplete)
        {
          SetActiveRaceRun(r);
          break;
        }
      }
    }

    public void Stop()
    {
      if (!_started)
        return;

      _started = false;

      _delegator.Dispose();
    }

    public bool Started
    {
      get { return _started; }
    }


    #region Top Level Calls

    public void UpdateStatus(string statusText)
    {
      if (!Started || _statusText == statusText)
        return;

      _statusText = statusText;

      scheduleTransfer(new LTTransfer(getXmlStatusUpdateInfo(_statusText), _tcpClient));
    }


    RaceRun _activeRaceRun;
    public void SetActiveRaceRun(RaceRun raceRun)
    {
      if (_activeRaceRun == raceRun)
        return;

      scheduleTransfer(new LTTransfer(getXmlActiveRun(raceRun), _tcpClient));
      _activeRaceRun = raceRun;
    }


    public void UpdateStartList(RaceRun raceRun)
    {
      scheduleTransfer(new LTTransfer(getXmlStartList(raceRun), _tcpClient));
    }


    public void UpdateOnStart(RaceRun raceRun, RaceParticipant rp)
    {
      SetActiveRaceRun(raceRun);
      scheduleTransfer(new LTTransfer(getXmlEventOnStart(rp), _tcpClient));
    }


    public void UpdateOnTrack(RaceRun raceRun, RaceParticipant rp)
    {
      SetActiveRaceRun(raceRun);
      scheduleTransfer(new LTTransfer(getXmlEventStarted(rp), _tcpClient));
    }


    public void UpdateInFinish(RaceRun raceRun, RaceParticipant rp)
    {
      SetActiveRaceRun(raceRun);

      var results = ViewUtilities.ViewToList<RaceResultItem>(raceRun.GetRace().GetTotalResultView());
      var rri4Participant = results.FirstOrDefault(rri => rri.Participant == rp);

      scheduleTransfer(new LTTransfer(getXmlEventResult(raceRun, rri4Participant), _tcpClient));
    }


    public void UpdateResults(RaceRun raceRun)
    {
      SetActiveRaceRun(raceRun);

      var results = ViewUtilities.ViewToList<RunResultWithPosition>(raceRun.GetResultView());

      RunResultWithPosition lastRR = null;
      foreach (var rr in results)
      {
        if (rr.ResultCode == RunResult.EResultCode.NotSet)
          continue;

        scheduleTransfer(new LTTransfer(getXmlEventResult(raceRun, rr), _tcpClient));

        if ( lastRR == null
          || (lastRR.StartTime  != null && rr.StartTime  != null && lastRR.StartTime  < rr.StartTime)
          )
          lastRR = rr;
      }

      // Update livetiming with last known time
      if (lastRR != null)
        scheduleTransfer(new LTTransfer(getXmlEventResult(raceRun, lastRR), _tcpClient));
    }


    TimeSpan? getAccumulatedTime(RaceRun rr, RunResultWithPosition resWp)
    {
      Race race = rr.GetRace();

      TimeSpan? ts = new TimeSpan();
      for(int i=0; i<race.GetMaxRun(); i++)
      {
        RaceRun r = race.GetRun(i);
        if (r.Run <= rr.Run)
        {
          TimeSpan? t = r.GetRunResult(resWp.Participant)?.Runtime;
          if (t != null && ts != null)
            ts = ts + (TimeSpan)t;
          else
            ts = null;
        }
      }

      return ts;
    }


    #endregion


    #region FIS specific XML serializer

    protected XmlWriterSettings _xmlSettings;
    protected XmlWriter _writer;

    private void setUpXmlFormat()
    {
      _xmlSettings = new XmlWriterSettings();
      _xmlSettings.Indent = true;
      _xmlSettings.IndentChars = "  ";
      _xmlSettings.Encoding = Encoding.UTF8;
    }

    internal string getXmlKeepAlive()
    {

      using (var sw = new Utf8StringWriter())
      {
        using (var xw = XmlWriter.Create(sw, _xmlSettings))
        {
          xw.WriteStartDocument();
          xmlWriteStartElementLivetiming(xw);
          xw.WriteStartElement("command");

          xw.WriteStartElement("keepalive");
          xw.WriteEndElement(); // clear

          xw.WriteEndElement(); // command
          xw.WriteEndElement(); // Livetiming
          xw.WriteEndDocument();
        }
        return sw.ToString();
      }
    }

    internal string getXmlClearRace()
    {

      using (var sw = new Utf8StringWriter())
      {
        using (var xw = XmlWriter.Create(sw, _xmlSettings))
        {
          xw.WriteStartDocument();
          xmlWriteStartElementLivetiming(xw);
          xw.WriteStartElement("command");

          xw.WriteStartElement("clear");
          xw.WriteEndElement(); // clear

          xw.WriteEndElement(); // command
          xw.WriteEndElement(); // Livetiming
          xw.WriteEndDocument();
        }
        return sw.ToString();
      }
    }

    internal string getXmlStatusUpdateInfo(string info)
    {

      using (var sw = new Utf8StringWriter())
      {
        using (var xw = XmlWriter.Create(sw, _xmlSettings))
        {
          xw.WriteStartDocument();
          xmlWriteStartElementLivetiming(xw);
          xw.WriteStartElement("message");

          xw.WriteElementString("text", info);

          xw.WriteEndElement(); // message
          xw.WriteEndElement(); // Livetiming
          xw.WriteEndDocument();
        }
        return sw.ToString();
      }
    }

    internal string getXmlRaceInfo(Race race)
    {
      RaceRun raceRun = race.GetRun(0);
      using (var sw = new Utf8StringWriter())
      {
        using (var xw = XmlWriter.Create(sw, _xmlSettings))
        {
          xw.WriteStartDocument();
          xmlWriteStartElementLivetiming(xw);
          xw.WriteStartElement("raceinfo");

          xw.WriteElementString("event", raceRun.GetRace().Description);
          xw.WriteElementString("slope", raceRun.GetRace().AdditionalProperties.CoarseName);
          xw.WriteElementString("discipline", getDisciplin(raceRun.GetRace()));
          xw.WriteElementString("gender", getGender(raceRun.GetRace()));
          xw.WriteElementString("category", _fisCategory);
          xw.WriteElementString("place", raceRun.GetRace().AdditionalProperties.Location);
          xw.WriteElementString("tempunit", "c");
          xw.WriteElementString("longunit", "m");
          xw.WriteElementString("speedunit", "Kmh");

          foreach (RaceRun rr in race.GetRuns())
          {

            xw.WriteStartElement("run");
            xw.WriteAttributeString("no", rr.Run.ToString());

            xw.WriteElementString("discipline", getDisciplin(rr.GetRace()));

            if (rr.GetRace().AdditionalProperties?.StartHeight > 0)
              xw.WriteElementString("start", rr.GetRace().AdditionalProperties?.StartHeight.ToString());
            if (rr.GetRace().AdditionalProperties?.FinishHeight > 0)
              xw.WriteElementString("finish", rr.GetRace().AdditionalProperties?.FinishHeight.ToString());
            if (rr.GetRace().AdditionalProperties?.StartHeight > 0 && rr.GetRace().AdditionalProperties?.FinishHeight > 0)
              xw.WriteElementString("height", (rr.GetRace().AdditionalProperties?.StartHeight - rr.GetRace().AdditionalProperties?.FinishHeight).ToString());

            AdditionalRaceProperties.RaceRunProperties raceRunProperties = null;
            if (rr.Run == 1)
              raceRunProperties = rr.GetRace().AdditionalProperties?.RaceRun1;
            else if (rr.Run >= 2)
              raceRunProperties = rr.GetRace().AdditionalProperties?.RaceRun2;

            if (raceRunProperties != null)
            {
              if (raceRunProperties.Gates > 0)
                xw.WriteElementString("gates", raceRunProperties.Gates.ToString());

              if (raceRunProperties.Turns > 0)
                xw.WriteElementString("turninggates", raceRunProperties.Turns.ToString());

              if (raceRunProperties.StartTime != null && raceRunProperties.StartTime.Contains(":") && raceRunProperties.StartTime.Length == 5)
              {
                xw.WriteElementString("hour", raceRunProperties.StartTime.Substring(0, 2));
                xw.WriteElementString("minute", raceRunProperties.StartTime.Substring(3, 2));
              }
            }

            if (rr.GetRace().AdditionalProperties?.DateResultList != null)
            {
              xw.WriteElementString("day", ((DateTime)rr.GetRace().AdditionalProperties?.DateResultList).Day.ToString());
              xw.WriteElementString("month", ((DateTime)rr.GetRace().AdditionalProperties?.DateResultList).Month.ToString());
              xw.WriteElementString("year", ((DateTime)rr.GetRace().AdditionalProperties?.DateResultList).Year.ToString());
            }

            xw.WriteStartElement("racedef");
            xw.WriteEndElement();

            xw.WriteEndElement(); // run
          }

          xw.WriteEndElement(); // raceinfo
          xw.WriteEndElement(); // Livetiming
          xw.WriteEndDocument();
        }
        return sw.ToString();
      }
    }

    internal string getXmlActiveRun(RaceRun raceRun)
    {
      using (var sw = new Utf8StringWriter())
      {
        using (var xw = XmlWriter.Create(sw, _xmlSettings))
        {
          xw.WriteStartDocument();
          xmlWriteStartElementLivetiming(xw);

          xw.WriteStartElement("command");
          xw.WriteStartElement("activerun");
          xw.WriteAttributeString("no", raceRun.Run.ToString());
          xw.WriteEndElement(); // activerun
          xw.WriteEndElement(); // command

          xw.WriteEndElement(); // Livetiming
          xw.WriteEndDocument();
        }
        return sw.ToString();
      }
    }

    internal string getXmlStartList(RaceRun raceRun)
    {
      using (var sw = new Utf8StringWriter())
      {
        using (var xw = XmlWriter.Create(sw, _xmlSettings))
        {
          xw.WriteStartDocument();
          xmlWriteStartElementLivetiming(xw);

          xw.WriteStartElement("startlist");
          xw.WriteAttributeString("runno", raceRun.Run.ToString());

          StartListViewProvider slp = raceRun.GetStartListProvider();
          var startList = slp.GetViewList();

          int i = 1;
          foreach (var sle in startList)
          {
            xw.WriteStartElement("racer");
            xw.WriteAttributeString("order", i.ToString());

            xw.WriteElementString("bib", sle.StartNumber.ToString());
            xw.WriteElementString("lastname", sle.Name);
            xw.WriteElementString("firstname", sle.Firstname);
            xw.WriteElementString("nat", sle.Nation);
            xw.WriteElementString("fiscode", sle.Code);

            xw.WriteEndElement(); // racer
            i++;
          }

          xw.WriteEndElement(); // startlist

          xw.WriteEndElement(); // Livetiming
          xw.WriteEndDocument();
        }
        return sw.ToString();
      }
    }

    internal string getXmlEventOnStart(RaceParticipant rp)
    {
      using (var sw = new Utf8StringWriter())
      {
        using (var xw = XmlWriter.Create(sw, _xmlSettings))
        {
          xw.WriteStartDocument();
          xmlWriteStartElementLivetiming(xw);

          xw.WriteStartElement("raceevent");

          xw.WriteStartElement("nextstart");
          xw.WriteAttributeString("bib", rp.StartNumber.ToString());
          xw.WriteEndElement();

          xw.WriteEndElement(); // raceevent

          xw.WriteEndElement(); // livetiming
          xw.WriteEndDocument();
        }
        return sw.ToString();
      }
    }

    internal string getXmlEventStarted(RaceParticipant rp)
    {
      using (var sw = new Utf8StringWriter())
      {
        using (var xw = XmlWriter.Create(sw, _xmlSettings))
        {
          xw.WriteStartDocument();
          xmlWriteStartElementLivetiming(xw);

          xw.WriteStartElement("raceevent");

          xw.WriteStartElement("start");
          xw.WriteAttributeString("bib", rp.StartNumber.ToString());
          xw.WriteEndElement();

          xw.WriteEndElement(); // raceevent

          xw.WriteEndElement(); // livetiming
          xw.WriteEndDocument();
        }
        return sw.ToString();
      }
    }

    internal string getXmlEventResult(RaceRun raceRun, IResultWithPosition result)
    {
      using (var sw = new Utf8StringWriter())
      {
        using (var xw = XmlWriter.Create(sw, _xmlSettings))
        {
          xw.WriteStartDocument();
          xmlWriteStartElementLivetiming(xw);

          xw.WriteStartElement("raceevent");

          if (result.ResultCode == RunResult.EResultCode.Normal && result.Runtime != null)
          {
            TimeSpan? runTime = null;
            if (result is RunResultWithPosition rrwp)
              runTime = getAccumulatedTime(raceRun, rrwp);
            else
              runTime = result.Runtime;

            xw.WriteStartElement("finish");
            xw.WriteAttributeString("bib", result.Participant.StartNumber.ToString());

            xw.WriteElementString("time", toFisTimeString(runTime));

            if (result.DiffToFirst != null)
              xw.WriteElementString("diff", ((TimeSpan)result.DiffToFirst).ToString(@"s\.ff"));
            else
              xw.WriteElementString("diff", "0.00");
            xw.WriteElementString("rank", result.Position.ToString());
            xw.WriteEndElement();
          }

          if (result.ResultCode == RunResult.EResultCode.NotSet)
          {
            xw.WriteStartElement("finish");
            xw.WriteAttributeString("bib", result.Participant.StartNumber.ToString());
            xw.WriteAttributeString("correction", "y");
            xw.WriteElementString("time", "0.00");
            xw.WriteEndElement();
          }

          if (result.ResultCode == RunResult.EResultCode.NaS || result.ResultCode == RunResult.EResultCode.NQ)
          { 
            xw.WriteStartElement("dns");
            xw.WriteAttributeString("bib", result.Participant.StartNumber.ToString());
            xw.WriteEndElement();
          }

          if (result.ResultCode == RunResult.EResultCode.NiZ)
          { 
            xw.WriteStartElement("dnf");
            xw.WriteAttributeString("bib", result.Participant.StartNumber.ToString());
            xw.WriteEndElement();
          }

          if (result.ResultCode == RunResult.EResultCode.DIS)
          { 
            xw.WriteStartElement("dq");
            xw.WriteAttributeString("bib", result.Participant.StartNumber.ToString());
            xw.WriteEndElement();
          }

          xw.WriteEndElement(); // raceevent

          xw.WriteEndElement(); // livetiming
          xw.WriteEndDocument();
        }
        return sw.ToString();
      }
    }

    private void xmlWriteStartElementLivetiming(XmlWriter xw)
    {
      xw.WriteStartElement("livetiming");
      xw.WriteAttributeString("codex", _fisRaceCode);
      xw.WriteAttributeString("passwd", _fisPassword);
      xw.WriteAttributeString("sequence", _sequence.ToString("D5"));
      xw.WriteAttributeString("timestamp", System.DateTime.Now.ToString("hh:mm:ss"));

      _sequence++;
    }


    private static string toFisTimeString(TimeSpan? time)
    {
      if (time == null)
        return "0.00";

      TimeSpan time2 = (TimeSpan)time;
      string str;
      if (time2 < new TimeSpan(0, 1, 0))
        str = time2.ToString(@"s\.ff");
      else if (time2 < new TimeSpan(1, 0, 0))
        str = time2.ToString(@"m\:ss\.ff");
      else
        str = time2.ToString(@"hh\:mm\:ss\.ff");

      return str;
    }

  #endregion


  #region FIS specific getter

  private string getDisciplin(Race race)
    {
      switch(race.RaceType)
      {
        case Race.ERaceType.DownHill: return "DH";
        case Race.ERaceType.GiantSlalom: return "GS";
        case Race.ERaceType.Slalom: return "SL";
        case Race.ERaceType.SuperG: return "SG";

        case Race.ERaceType.ParallelSlalom:
        case Race.ERaceType.KOSlalom:
        default:
          throw new Exception(string.Format("{0} not supported for FIS livetiming", race.RaceType));
      }
    }

    /// <summary>
    /// Determines the FIS gender based on the participants of a race
    /// </summary>
    /// <returns>M: men, L: ladies, A: mixed</returns>
    private string getGender(Race race)
    {
      string raceGender = string.Empty;
      foreach(var rp in race.GetParticipants())
      {
        char sex = char.ToUpper(rp.Sex.Name);
        string gender = string.Empty;

        if (sex == 'M')
          gender = "M";
        if (sex == 'W' || sex == 'L')
          gender = "L";

        if (raceGender == string.Empty)
          raceGender = gender;
        else if (raceGender != gender)
          raceGender = "A";

      }

      if (raceGender == string.Empty) // Fallback, if no participants
        raceGender = "A";

      return raceGender;
    }

    private string getDate(Race race)
    {
      DateTime? date = race.AdditionalProperties?.DateResultList;
      if (date == null)
        date = DateTime.Today;

      return ((DateTime)date).ToString("dd.MM.yy", System.Globalization.DateTimeFormatInfo.InvariantInfo);
    }


    #endregion


    #region Transfer Implementation

    List<LTTransfer> _transfers = new List<LTTransfer>();
    object _transferLock = new object();
    bool _transferInProgress = false;
    private bool disposedValue;

    private void scheduleTransfer(LTTransfer transfer)
    {
      lock (_transferLock)
      {
        // Remove all outdated transfers
        //_transfers.RemoveAll(x => x.IsEqual(transfer));
        _transfers.Add(transfer);
      }

      if (!_transferInProgress)
      {
        _transferInProgress = true;
        processNextTransfer();
      }
    }


    private void processNextTransfer()
    {
      LTTransfer nextItem = null;
      lock (_transferLock)
      {
        if (_transfers.Count() > 0)
        {
          nextItem = _transfers[0];
          _transfers.RemoveAt(0);
        }
      }

      if (nextItem != null)
      {
        // Trigger execution of transfers
        Task.Run(() =>
        {
          Logger.Debug("process transfer: " + nextItem.ToString());
          nextItem.performTransfer();
        })
          .ContinueWith(delegate { processNextTransfer(); });
      }
      else
        _transferInProgress = false;
    }


    /// <summary>
    /// Receives all the data from FIS server
    /// Note: Right now all data is ignored.
    /// </summary>
    private void receiveMethod()
    {
      using (var sr = new BufferedStream(_tcpClient.GetStream()))
      {
        byte[] buf = new byte[1024];

        int bytesRead;
        try
        {
          while ((bytesRead = sr.Read(buf, 0, buf.Length)) > 0)
          {
            Logger.Debug(Encoding.UTF8.GetString(buf, 0, bytesRead));
          }
        }
        catch (Exception)
        { }
      }
    }

    #endregion
  }


  public class LTTransfer
  {
    protected string _type;

    protected string _xmlMessage;
    protected System.Net.Sockets.TcpClient _tcpClient;

    public LTTransfer(string xmlMessage, System.Net.Sockets.TcpClient tcpClient)
    {
      _xmlMessage = xmlMessage;
      _tcpClient = tcpClient;
    }

    public override string ToString()
    {
      return "LTTransfer(" + _xmlMessage + ")";
    }


    public void performTransfer()
    {
      try
      {
        byte[] utf8Message = System.Text.Encoding.UTF8.GetBytes(_xmlMessage);
        var stream = _tcpClient.GetStream();
        stream.Write(utf8Message, 0, utf8Message.Length);
      }
      catch(Exception )
      {
      }
    }

  }


}
