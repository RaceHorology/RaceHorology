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
  public class TagHeuer : IHandTiming
  {

    TagHeuerParser _parser;
    private SerialPort _serialPort;
    private string _serialPortName;

    private DateTime _synchroTime;

    public TagHeuer(string serialPortName)
    {
      _parser = new TagHeuerParser();
      _serialPortName = serialPortName;

    }


    public void Connect()
    {
      _serialPort = new SerialPort(_serialPortName, 38400, Parity.None, 8, StopBits.One);
      _serialPort.NewLine = "\r"; // CR, ASCII(13)
      _serialPort.Handshake = Handshake.RequestToSend;
      _serialPort.ReadTimeout = 1000;
      _serialPort.Open();

      performHandshake();
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


    protected void performHandshake()
    {
      _serialPort.WriteLine("#SN"); // Try to get serial number, 
      string dataLine = _parser.TrimNewEndLine(_serialPort.ReadLine()); 
    }


    public void StartGetTimingData()
    {
      _serialPort.WriteLine("#!T"); // Get Sync Time (needs to be added to timevalues)
      string dataLine = _serialPort.ReadLine(); // "!T 08:14:00 01/03/20"
      DateTime syncTime = _parser.ParseSynchroTime(dataLine);
      _synchroTime = syncTime;

      _serialPort.WriteLine("#WC 012");
      dataLine = _parser.TrimNewEndLine(_serialPort.ReadLine());
      if (!dataLine.StartsWith("DS "))
        throw new Exception("wrong answer");
    }

    public IEnumerable<TimingData> TimingData()
    {
      do
      {
        TagHeuerData parsedData = null;
        try
        {
          string dataLine = _parser.TrimNewEndLine(_serialPort.ReadLine());

          if (dataLine.StartsWith("DE ")) // until "DE NN"
            break;

          parsedData = _parser.ParseRR(dataLine);
        }
        catch (TimeoutException)
        {
          break; // no new data
        }
        catch (Exception)
        { }

        TimingData td = new TimingData
        {
          Time = (_synchroTime + parsedData.Time).TimeOfDay
        };

        reportProgress(td.Time.ToString());

        yield return td;

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


  public class TagHeuerData
  {
    public int Rank { get; set; }
    public int Number { get; set; }
    public TimeSpan Time { get; set; }
  }


  public class TagHeuerParser
  {
    public string TrimNewEndLine(string line)
    {
      return line.TrimStart('\n').TrimEnd('\t');
    }

    // "!T 08:14:00 01/03/20"
    public DateTime ParseSynchroTime(string line)
    {
      line = TrimNewEndLine(line);

      DateTime parsedDate;

      try
      {
        string[] formats = { @"HH\:mm\:ss dd/MM/yy"};
        string timeStr = line.Substring(3);
        parsedDate = DateTime.ParseExact(timeStr, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
      }
      catch (FormatException)
      {
        throw;
      }

      return parsedDate;
    }


    /* Data Looks like this
     * DS 01 245 TIME
     * ...
     * RR 0000 0232   05:27:51.01040
     * ...
     * RR 0002 9999   06:06:01.35403
     * DE 01
     */
    public TagHeuerData ParseRR(string line)
    {
      line = TrimNewEndLine(line);

      TagHeuerData parsedData = null;

      string prefix = line.Substring(0, 2);
      if (prefix == "RR")
      {
        string rank = line.Substring(3, 4);
        string number = line.Substring(8, 4);
        string timeStr = line.Substring(15);
        try
        {
          int nRank = int.Parse(rank);
          int nNumber = int.Parse(number);
          string[] formats = { @"hh\:mm\:ss\.fffff", @"hh\:mm\:ss\.ffff", @"hh\:mm\:ss\.fff", @"hh\:mm\:ss\.ff", @"hh\:mm\:ss\.f" };
          timeStr = timeStr.TrimEnd(' ');
          TimeSpan time = TimeSpan.ParseExact(timeStr, formats, System.Globalization.CultureInfo.InvariantCulture);

          parsedData = new TagHeuerData { Number = nNumber, Rank = nRank, Time = time };
        }
        catch (FormatException)
        {
          throw;
        }
      }
      return parsedData;
    }

  }
}
