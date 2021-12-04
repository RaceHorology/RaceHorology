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

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  public class ALGETimy : IHandTiming
  {
    ALGETdC8001LineParser _parser;
    private SerialPort _serialPort;
    private string _serialPortName;

    public ALGETimy(string serialPortName)
    {
      _parser = new ALGETdC8001LineParser();
      _serialPortName = serialPortName;
    }


    public void Connect()
    {
      _serialPort = new SerialPort(_serialPortName, 9600, Parity.None, 8, StopBits.One);
      _serialPort.NewLine = "\r"; // CR, ASCII(13)
      _serialPort.Handshake = Handshake.RequestToSend;
      _serialPort.ReadTimeout = 1000;
      _serialPort.Open();
    }

    public void Disconnect()
    {
      if (_serialPort != null)
      {
        _serialPort.Close();
        _serialPort.Dispose();
        _serialPort = null;
      }
    }


    public void StartGetTimingData()
    {
      _serialPort.WriteLine("RSM");
      string response = _serialPort.ReadLine();
    }


    public IEnumerable<TimingData> TimingData()
    {
      do
      {
        try
        {
          string dataLine = _serialPort.ReadLine();
          if (dataLine.StartsWith("  ALGE-TIMING"))
          {
            // End of data => read two more lines
            _serialPort.ReadLine(); // "  TIMY V 1982"
            _serialPort.ReadLine(); // "20-10-04  16:54"
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

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ALGETimy()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion

  }

}
