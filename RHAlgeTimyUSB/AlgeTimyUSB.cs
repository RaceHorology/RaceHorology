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
  public class AlgeTimyUSB : ILiveTimeMeasurementDevice, ILiveDateTimeProvider, IImportTime, IHandTiming
  {
    Alge.TimyUsb _timy;

    public AlgeTimyUSB()
    {
      _timy = new Alge.TimyUsb();

      _timy.DeviceConnected += _timy_DeviceConnected;
      _timy.DeviceDisconnected += _timy_DeviceDisconnected;
      _timy.LineReceived += _timy_LineReceived;
      _timy.Start();
    }

    private void _timy_LineReceived(object sender, Alge.DataReceivedEventArgs e)
    {
      System.Diagnostics.Trace.WriteLine(e.Data);
    }

    private void _timy_DeviceDisconnected(object sender, Alge.DeviceChangedEventArgs e)
    {
      System.Diagnostics.Trace.WriteLine("Device dis-connected");
    }

    private void _timy_DeviceConnected(object sender, Alge.DeviceChangedEventArgs e)
    {
      System.Diagnostics.Trace.WriteLine("Device connected");
    }


    #region ILiveTimeMeasurementDevice

    public bool IsStarted => throw new NotImplementedException();

    public bool IsOnline => throw new NotImplementedException();

    public event StartnumberSelectedEventHandler StartnumberSelectedReceived;
    public event LiveTimingMeasurementDeviceStatusEventHandler StatusChanged;
    public event TimeMeasurementEventHandler TimeMeasurementReceived;
    public event LiveDateTimeChangedHandler LiveDateTimeChanged;
    public event ImportTimeEntryEventHandler ImportTimeEntryReceived;

    public void Start()
    {
      throw new NotImplementedException();
    }

    public void Stop()
    {
      throw new NotImplementedException();
    }

    public string GetDeviceInfo()
    {
      throw new NotImplementedException();
    }

    public string GetStatusInfo()
    {
      throw new NotImplementedException();
    }
    #endregion

    public TimeSpan GetCurrentDayTime()
    {
      throw new NotImplementedException();
    }

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
  }
}
