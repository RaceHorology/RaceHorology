/*
 *  Copyright (C) 2019 - 2022 by Sven Flossmann
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
    }

    public override string GetDeviceInfo()
    {
      return "ALGE Timy (USB)";
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

    #region IHandTiming
    public void Connect()
    {
      throw new NotImplementedException();
    }

    public void Disconnect()
    {
      throw new NotImplementedException();
    }

    public void StartGetTimingData()
    {
      throw new NotImplementedException();
    }

    public IEnumerable<TimingData> TimingData()
    {
      throw new NotImplementedException();
    }

    public void Dispose()
    {
      throw new NotImplementedException();
    }

    public void DoProgressReport(IProgress<StdProgress> progress)
    {
      throw new NotImplementedException();
    }
    #endregion

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
}
