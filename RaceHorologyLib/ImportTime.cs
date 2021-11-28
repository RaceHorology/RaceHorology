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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  /// <summary>
  /// Represent an imported time e.g. received via ALGE Classement
  /// </summary>
  public class ImportTimeEntry
  {
    uint _startNumber;
    TimeSpan? _runTime;

    public ImportTimeEntry()
    {
      _startNumber = 0;
      _runTime = null;
    }

    public uint StartNumber
    {
      get { return _startNumber; }
      set { if (_startNumber != value) { _startNumber = value; } }
    }

    public TimeSpan? RunTime
    {
      get { return _runTime; }
      set { if (_runTime != value) { _runTime = value; } }
    }

  }



  public class ImportTimeEntryWithParticipant : ImportTimeEntry
  {
    RaceParticipant _rp;

    public ImportTimeEntryWithParticipant(ImportTimeEntry ie, RaceParticipant rp)
    {
      StartNumber = ie.StartNumber;
      RunTime = ie.RunTime;
      _rp = rp;
    }

    public RaceParticipant Participant
    {
      get { return _rp; }
      set { if (_rp != value) { _rp = value; } }
    }
  }



  public class ImportTimeEntryVM
  {
    ObservableCollection<ImportTimeEntryWithParticipant> _importEntries;

    Race _race;

    public ImportTimeEntryVM(Race race)
    {
      _race = race;
    }

    public ObservableCollection<ImportTimeEntryWithParticipant> ImportEntries
    {
      get { return _importEntries; }
    }

    public void AddEntry(ImportTimeEntry entry)
    {
      var participant = _race.GetParticipant(entry.StartNumber);

      var e = new ImportTimeEntryWithParticipant(entry, participant);
      _importEntries.Add(e);
    }

  }




}
