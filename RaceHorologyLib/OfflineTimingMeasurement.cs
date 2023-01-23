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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public class TimingData
  {
    public TimeSpan? Time { get; set; }
  }

  public interface IHandTiming : IDisposable, IHasProgress<StdProgress>
  {

    void Connect();
    void Disconnect();

    void StartGetTimingData();
    IEnumerable<TimingData> TimingData();
  }


  public class FromFileHandTiming : IHandTiming
  {
    string _filePath;
    StreamReader _file;
    FromFileParser _parser;

    public FromFileHandTiming(string path)
    {
      _filePath = path;

      _parser = new FromFileParser();
    }

    public void Connect()
    {
      _file = new System.IO.StreamReader(_filePath);
    }

    public void Disconnect()
    {
      if (_file != null)
      {
        _file.Close();
        _file.Dispose();
        _file = null;
      }
    }


    public void StartGetTimingData()
    {
      return;
    }


    public IEnumerable<TimingData> TimingData()
    {
      string line;
      while ((line = _file.ReadLine()) != null)
      {
        var time = _parser.ParseTime(line);

        reportProgress(line);

        yield return new TimingData { Time = time };
      }

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
          // Dispose managed state (managed objects)
          reportFinal();
          Disconnect();
        }

        disposedValue = true;
      }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~FromFileHandTiming()
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



  public class FromFileParser
  {

    // Format: 09:37:16,40
    public TimeSpan ParseTime(string line)
    {
      TimeSpan parsedTime;

      try
      {
        string[] formats = 
        {
          @"hh\:mm\:ss\.fffff", @"hh\:mm\:ss\.ffff", @"hh\:mm\:ss\.fff", @"hh\:mm\:ss\.ff", @"hh\:mm\:ss\.f",
          @"hh\:mm\:ss\,fffff", @"hh\:mm\:ss\,ffff", @"hh\:mm\:ss\,fff", @"hh\:mm\:ss\,ff", @"hh\:mm\:ss\,f"
        };

        parsedTime = TimeSpan.ParseExact(line, formats, System.Globalization.CultureInfo.InvariantCulture);
      }
      catch (FormatException)
      {
        throw;
      }

      return parsedTime;
    }
  }


}
