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

using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RHAlgeTimyUSB
{
  public class AlgeTimyUSB : ALGETdC8001TimeMeasurementBase, ILiveTimeMeasurementDeviceDebugInfo
  {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    enum EInternalStatus { Stopped, Initializing, NoDevice, Running };
    private EInternalStatus _internalStatus;
    private string _internalProtocol;

    Alge.TimyUsb _timy;

    public AlgeTimyUSB()
    {
      _internalProtocol = string.Empty;
      _deviceInfo = new DeviceInfo
      {
        Manufacturer = "ALGE",
        Model = "Timy (USB)",
        PrettyName = "ALGE Timy",
        SerialNumber = string.Empty
      };
    }


    private void _timy_LineReceived(object sender, Alge.DataReceivedEventArgs e)
    {
      try
      {
        string dataLine = e.Data;
        Logger.Info("data received: {0}", dataLine);

        debugLine(dataLine);
        processLine(dataLine);
      }
      catch (Exception err)
      {
        Logger.Info("Exception catched: {0}", err.ToString());
      }
    }

    private void _timy_DeviceDisconnected(object sender, Alge.DeviceChangedEventArgs e)
    {
      Logger.Info("timy dis-connected: {0}", e.Device.ToString());
      setInternalStatus(EInternalStatus.NoDevice);
    }

    private void _timy_DeviceConnected(object sender, Alge.DeviceChangedEventArgs e)
    {
      Logger.Info("timy connected: {0}", e.Device.ToString());
      setInternalStatus(EInternalStatus.Running);
    }


    public override void Start()
    {
      Logger.Info("Start()");
      setInternalStatus(EInternalStatus.Initializing);

      _timy = new Alge.TimyUsb();

      _timy.DeviceConnected += _timy_DeviceConnected;
      _timy.DeviceDisconnected += _timy_DeviceDisconnected;
      _timy.LineReceived += _timy_LineReceived;
      _timy.Start();
      _internalProtocol = string.Empty;
    }

    public override void Stop()
    {
      Logger.Info("Stop()");

      if (_timy == null)
        return;

      setInternalStatus(EInternalStatus.Stopped);

      _timy.DeviceConnected -= _timy_DeviceConnected;
      _timy.DeviceDisconnected -= _timy_DeviceDisconnected;
      _timy.LineReceived -= _timy_LineReceived;
      _timy.Stop();
      _timy.Dispose();
      _timy = null;
    }

    private void setInternalStatus(EInternalStatus value)
    {
      if (_internalStatus != value)
      {
        Logger.Info("new status: {0} (old: {1})", value, _internalStatus);

        _internalStatus = value;
        switch (_internalStatus)
        {
          case EInternalStatus.Running:
            _statusText = "Verbunden"; break;
          case EInternalStatus.Stopped:
            _statusText = "nicht verbunden"; break;
          case EInternalStatus.Initializing:
            _statusText = "Verbinde ..."; break;
          case EInternalStatus.NoDevice:
            _statusText = "kein Gerät gefunden"; break;
        }

        var handler = StatusChanged;
        handler?.Invoke(this, IsOnline);
      }
    }

    public override bool IsOnline
    {
      get { return _internalStatus == EInternalStatus.Running; }
    }

    public override bool IsStarted
    {
      get
      {
        return _timy != null;
      }
    }
    public override event LiveTimingMeasurementDeviceStatusEventHandler StatusChanged;

    #region Implementation of ILiveTimeMeasurementDeviceDebugInfo
    public event RawMessageReceivedEventHandler RawMessageReceived;

    public string GetProtocol()
    {
      return _internalProtocol;
    }

    void debugLine(string dataLine)
    {
      if (!string.IsNullOrEmpty(_internalProtocol))
        _internalProtocol += "\n";
      _internalProtocol += dataLine;

      RawMessageReceivedEventHandler handler = RawMessageReceived;
      handler?.Invoke(this, dataLine);
    }
    #endregion
  }



  public class AlgeTimyHTUSB : IHandTiming
  {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    ALGETdC8001LineParser _parser;
    Alge.TimyUsb _timy;
    BufferBlock<string> _buffer;

    TaskCompletionSource<string> _connectSignal = new TaskCompletionSource<string>();

    public AlgeTimyHTUSB()
    {
      _parser = new ALGETdC8001LineParser();
    }

    public void Connect()
    {
      Logger.Info("Connect()");

      _buffer = new BufferBlock<string>();
      _timy = new Alge.TimyUsb();

      _timy.DeviceConnected += _timy_DeviceConnected;
      _timy.DeviceDisconnected += _timy_DeviceDisconnected;
      _timy.LineReceived += _timy_LineReceived;
      _timy.Start();

      if (!_connectSignal.Task.Wait(2000))
        throw new Exception("Verbindung zu ALGE Timy kann nicht aufgebaut werden");
    }

    public void Disconnect()
    {
      Logger.Info("Disconnect()");

      if (_timy == null)
        return;

      _timy.DeviceConnected -= _timy_DeviceConnected;
      _timy.DeviceDisconnected -= _timy_DeviceDisconnected;
      _timy.LineReceived -= _timy_LineReceived;
      _timy.Stop();
      _timy.Dispose();
      _timy = null;
      _buffer = null;

    }

    private void _timy_LineReceived(object sender, Alge.DataReceivedEventArgs e)
    {
      string dataLine = e.Data;
      Logger.Info("data received: {0}", dataLine);

      _buffer.Post(dataLine);
    }

    private void _timy_DeviceDisconnected(object sender, Alge.DeviceChangedEventArgs e)
    {
      Logger.Info("timy dis-connected: {0}", e.Device.ToString());
    }

    private void _timy_DeviceConnected(object sender, Alge.DeviceChangedEventArgs e)
    {
      Logger.Info("timy connected: {0}", e.Device.ToString());

      _connectSignal.SetResult(e.Device.ToString());
    }

    public void StartGetTimingData()
    {
      _timy.Send("");    // Sending an empty command, let the Timy answer for the second time ... no clue why this is the case ...
      _timy.Send("RSM"); // RSM transfers the stored times
    }

    public IEnumerable<TimingData> TimingData()
    {
      do
      {
        try
        {
          string dataLine = _buffer.Receive(new TimeSpan(0, 0, 10));
          if (dataLine.StartsWith("  ALGE-TIMING")) // END marker
          {
            // End of data => read two more lines
            _buffer.Receive(); // "  TIMY V 1982"
            _buffer.Receive(); // "20-10-04  16:54"
            break;
          }
          _parser.Parse(dataLine);
        }
        catch (TimeoutException)
        {
          break; // no new data
        }
        catch (Exception)
        { }

        if (_parser.TimingData != null)
        {
          TimingData td = new TimingData
          {
            Time = _parser.TimingData.Time
          };

          reportProgress(td.Time.ToString());

          yield return td;
        }
      } while (true);

      reportFinal();
    }

    #region IDispose

    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          reportFinal();
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

    #region IHasProgress implementation

    IProgress<StdProgress> _progress;

    public void DoProgressReport(IProgress<StdProgress> progress)
    {
      _progress = progress;
    }

    private void reportProgress(string current)
    {
      _progress?.Report(new StdProgress { CurrentStatus = current });
    }
    private void reportFinal()
    {
      _progress?.Report(new StdProgress { Finished = true });
    }
    #endregion
  }


}
