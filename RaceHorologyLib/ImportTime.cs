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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  public delegate void ImportTimeEntryEventHandler(object sender, ImportTimeEntry e);

  public enum EImportTimeFlags { None = 0x0, RemoteDownload = 0x1, RunTime = 0x2, StartFinishTime = 0x4 }
  public interface IImportTime
  {
    event ImportTimeEntryEventHandler ImportTimeEntryReceived;

    void DownloadImportTimes();
    EImportTimeFlags SupportedImportTimeFlags();

  }


  /// <summary>
  /// Represent an imported time e.g. received via ALGE Classement
  /// </summary>
  public class ImportTimeEntry
  {
    protected uint _startNumber;
    protected TimeSpan? _runTime;
    protected TimeSpan? _startTime;
    protected TimeSpan? _finishTime;

    protected ImportTimeEntry(ImportTimeEntry that)
    {
      _startNumber = that._startNumber;
      _runTime = that._runTime;
      _startTime = that._startTime;
      _finishTime = that._finishTime;
    }

    public ImportTimeEntry(uint startNumber, TimeSpan? runTime)
    {
      _startNumber = startNumber;
      _runTime = runTime;
    }
    public ImportTimeEntry(uint startNumber, TimeSpan? startTime, TimeSpan? finishTime)
    {
      _startNumber = startNumber;
      _startTime = startTime;
      _finishTime = finishTime;
    }

    virtual public uint StartNumber
    {
      get { return _startNumber; }
      set { if (_startNumber != value) { _startNumber = value; } }
    }

    // Methods for reading properties and setting
    // Note: Setting is intentionally done via setFunction and not property accessors in order to avoid chainging the time accidentally (e.g. via a DataGridView)
    public TimeSpan? RunTime { get { return _runTime; } }
    public virtual void setRunTime(TimeSpan value) { if (_runTime != value) { _runTime = value; } }

    public TimeSpan? StartTime { get { return _startTime; } }
    public virtual void setStartTime(TimeSpan value) { if (_startTime != value) { _startTime = value; } }

    public TimeSpan? FinishTime { get { return _finishTime; } }
    public virtual void setFinishTime(TimeSpan value) { if (_finishTime != value) { _finishTime = value; } }
  }



  public class ImportTimeEntryWithParticipant : ImportTimeEntry, INotifyPropertyChanged
  {
    Race _race;
    RaceParticipant _rp;

    public ImportTimeEntryWithParticipant(ImportTimeEntry ie, Race race)
      : base(ie)
    {
      _race = race;
      _rp = _race.GetParticipant(_startNumber);
    }

    public RaceParticipant Participant
    {
      get { return _rp; }
    }

    override public uint StartNumber
    {
      get { return base.StartNumber; }
      set { 
        if (_startNumber != value) { 
          _startNumber = value;
          _rp = _race.GetParticipant(_startNumber);
          NotifyAllPropertiesChanged();
        } 
      }
    }

    public override void setRunTime(TimeSpan value) { if (_runTime != value) { _runTime = value; NotifyPropertyChanged("RunTime"); } }

    public override void setStartTime(TimeSpan value) { if (_startTime != value) { _startTime = value; NotifyPropertyChanged("StartTime"); } }

    public override void setFinishTime(TimeSpan value) { if (_finishTime != value) { _finishTime = value; NotifyPropertyChanged("FinishTime"); } }



    public string Name { get => _rp?.Name; }
    public string Firstname { get => _rp?.Firstname; }
    public string Fullname { get => _rp?.Fullname; }
    public ParticipantCategory Sex { get => _rp?.Sex; }
    public uint Year { get => _rp == null ? 0 : _rp.Year; }
    public string Club { get => _rp?.Club; }
    public string Nation { get => _rp?.Nation; }
    public string SvId { get => _rp?.SvId; }
    public string Code { get => _rp?.Code; }
    public ParticipantClass Class { get => _rp?.Class; }
    public ParticipantGroup Group { get => _rp?.Group; }



    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    protected void NotifyAllPropertiesChanged()
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    #endregion
  }



  public class ImportTimeEntryVM : IDisposable
  {
    ObservableCollection<ImportTimeEntryWithParticipant> _importEntries;

    Race _race;
    IImportTime _importTimeDevice;

    public ImportTimeEntryVM(Race race, IImportTime importTimeDevice)
    {
      _race = race;
      _importTimeDevice = importTimeDevice;

      _importEntries = new ObservableCollection<ImportTimeEntryWithParticipant>();

      _importTimeDevice.ImportTimeEntryReceived += importTimeDevice_ImportTimeEntryReceived;
    }

    private void importTimeDevice_ImportTimeEntryReceived(object sender, ImportTimeEntry entry)
    {
      System.Windows.Application.Current.Dispatcher.Invoke(() =>
      {
        AddEntry(entry);
      });
    }

    public ObservableCollection<ImportTimeEntryWithParticipant> ImportEntries
    {
      get { return _importEntries; }
    }

    public void Clear()
    {
      _importEntries.Clear();
    }

    public void AddEntry(ImportTimeEntry entry)
    {
      var existingEntry = _importEntries.LastOrDefault(x => x.StartNumber == entry.StartNumber);
      ImportTimeEntryWithParticipant e = null;
      if (entry.StartNumber != 0 && existingEntry != null)
      {
        var idx = _importEntries.IndexOf(existingEntry);
        if (entry.RunTime != null && entry.StartTime == null && entry.FinishTime == null)
          existingEntry.setRunTime((TimeSpan)entry.RunTime);
        else
        {
          if (_importEntries.Count - idx > 10)
            e = new ImportTimeEntryWithParticipant(entry, _race);
          else if (entry.StartTime != null && existingEntry.StartTime == null)
            existingEntry.setStartTime((TimeSpan)entry.StartTime);
          else if (entry.FinishTime != null && existingEntry.FinishTime == null)
            existingEntry.setFinishTime((TimeSpan)entry.FinishTime);
          else
            e = new ImportTimeEntryWithParticipant(entry, _race);
        }
      }
      else
        e = new ImportTimeEntryWithParticipant(entry, _race);

      if (e != null)
        _importEntries.Add(e);
    }


    /// <summary>
    /// Saves all runtimes to the race run specified
    /// </summary>
    public int Save(RaceRun raceRun, IEnumerable<ImportTimeEntryWithParticipant> items, bool overwriteAlreadyImportedParticipantAssignment)
    {
      int count = 0;
      foreach( var entry in items)
      {
        bool didSomething = false;
        if (entry.StartTime != null)
        {
          bool valid = false;

          if (overwriteAlreadyImportedParticipantAssignment || raceRun.GetTimestamp((TimeSpan)entry.StartTime, EMeasurementPoint.Start)==null)
          {
            if (entry.Participant != null)
            {
              raceRun.SetStartTime(entry.Participant, entry.StartTime);
              valid = true;
            }
            var ts = new Timestamp((TimeSpan)entry.StartTime, EMeasurementPoint.Start, entry.StartNumber, valid);
            raceRun.AddOrUpdateTimestamp(ts);
            didSomething = true;
          }
        }

        if (entry.FinishTime != null)
        {
          bool valid = false;
          if (overwriteAlreadyImportedParticipantAssignment || raceRun.GetTimestamp((TimeSpan)entry.FinishTime, EMeasurementPoint.Finish) == null)
          {
            if (entry.Participant != null)
            {
              raceRun.SetFinishTime(entry.Participant, entry.FinishTime);
              valid = true;
            }
            var ts = new Timestamp((TimeSpan)entry.FinishTime, EMeasurementPoint.Finish, entry.StartNumber, valid);
            raceRun.AddOrUpdateTimestamp(ts);
            didSomething = true;
          }
        }

        if (entry.RunTime != null)
          if (entry.Participant != null)
          {
            raceRun.SetRunTime(entry.Participant, entry.RunTime);
            didSomething = true;
          }

        if (didSomething)
          count++;
      }
      return count;
    }


    #region IDisposable implementation

    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          _importTimeDevice.ImportTimeEntryReceived -= importTimeDevice_ImportTimeEntryReceived;
          _importTimeDevice = null;
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




}
