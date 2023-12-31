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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  /// <summary>
  /// Observes the run results and triggers a database store in case time / run results changed
  /// </summary>
  /// <remarks>
  /// Delete not implemented (actually not needed)
  /// </remarks>
  internal class DatabaseDelegatorRaceRun
  {
    private Race _race;
    private RaceRun _rr;
    private IAppDataModelDataBase _db;

    public DatabaseDelegatorRaceRun(Race race, RaceRun rr, IAppDataModelDataBase db)
    {
      _db = db;
      _race = race;
      _rr = rr;

      rr.GetResultList().ItemChanged += OnItemChanged;
      rr.GetResultList().CollectionChanged += OnCollectionChanged;

      rr.GetTimestamps().ItemChanged += OnTimestampItemChanged;
      rr.GetTimestamps().CollectionChanged += OnTimestampCollectionChanged;
    }

    private void OnItemChanged(object sender, PropertyChangedEventArgs e)
    {
      RunResult result = (RunResult)sender;

      if (result.IsEmpty())
        _db.DeleteRunResult(_race, _rr, result);
      else
        _db.CreateOrUpdateRunResult(_race, _rr, result);

    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (RunResult v in e.NewItems)
          {
            if (v.IsEmpty())
              _db.DeleteRunResult(_race, _rr, v);
            else
              _db.CreateOrUpdateRunResult(_race, _rr, v);
          }
          break;

        case NotifyCollectionChangedAction.Remove:
          // do not delete from DB in this case, data shall be preserved
          // Data is only deleted if the RunResult is set to empty (see above)
          break;

        case NotifyCollectionChangedAction.Reset:
        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Replace:
          throw new Exception("not implemented");
      }
    }


    private void OnTimestampItemChanged(object sender, PropertyChangedEventArgs e)
    {
      var ts = (Timestamp)sender;
      _db.CreateOrUpdateTimestamp(_rr, ts);
    }

    private void OnTimestampCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (Timestamp v in e.NewItems)
          {
            _db.CreateOrUpdateTimestamp(_rr, v);
          }
          break;

        case NotifyCollectionChangedAction.Remove:
        case NotifyCollectionChangedAction.Reset:
        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Replace:
          throw new Exception("not implemented");
      }
    }
  }

  /// <summary>
  /// Observes the run results and triggers a database store in case time / run results changed
  /// </summary>
  /// <remarks>
  /// Delete not implemented (actually not needed)
  /// </remarks>
  internal class DatabaseDelegatorRaceParticipant
  {
    private Race _race;
    private IAppDataModelDataBase _db;

    public DatabaseDelegatorRaceParticipant(Race race, IAppDataModelDataBase db)
    {
      _db = db;
      _race = race;

      _race.GetParticipants().ItemChanged += OnItemChanged;
      _race.GetParticipants().CollectionChanged += OnCollectionChanged;
    }

    private void OnItemChanged(object sender, PropertyChangedEventArgs e)
    {
      RaceParticipant raceParticipant = (RaceParticipant)sender;
      _db.CreateOrUpdateRaceParticipant(raceParticipant);
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (RaceParticipant v in e.NewItems)
            _db.CreateOrUpdateRaceParticipant(v);
          break;
        case NotifyCollectionChangedAction.Remove:
          foreach (RaceParticipant v in e.OldItems)
            _db.RemoveRaceParticipant(v);
          break;

        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Reset:
          throw new Exception("not implemented");
      }
    }
  }

  /// <summary>
  /// Observes the Patients and triggers a database store if needed
  /// </summary>
  /// <remarks>
  /// Delete not yet implemented
  /// </remarks>
  internal class DatabaseDelegatorParticipant
  {
    private ItemsChangeObservableCollection<Participant> _participants;
    private IAppDataModelDataBase _db;

    public DatabaseDelegatorParticipant(ItemsChangeObservableCollection<Participant> participants, IAppDataModelDataBase db)
    {
      _db = db;
      _participants = participants;

      _participants.ItemChanged += OnItemChanged;
      _participants.CollectionChanged += OnCollectionChanged;
    }

    private void OnItemChanged(object sender, PropertyChangedEventArgs e)
    {
      Participant participant = (Participant)sender;
      _db.CreateOrUpdateParticipant(participant);
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (Participant participant in e.NewItems)
            _db.CreateOrUpdateParticipant(participant);
          break;

        case NotifyCollectionChangedAction.Remove:
          foreach (Participant participant in e.OldItems)
            _db.RemoveParticipant(participant);
          break;

        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Reset:
          throw new Exception("not implemented");
      }
    }
  }


  internal class DatabaseDelegatorCompetition
  {
    private IAppDataModelDataBase _db;
    AppDataModel _dm;

    public DatabaseDelegatorCompetition(AppDataModel dm, IAppDataModelDataBase db)
    {
      _dm = dm;
      _db = db;

      _dm.GetRaces().CollectionChanged += OnRacesChanged;
    }

    private void OnRacesChanged(object source, NotifyCollectionChangedEventArgs args)
    {
      if (args.NewItems != null)
        foreach (var item in args.NewItems)
          if (item is Race race)
            _db.UpdateRace(race, true);

      if (args.OldItems != null)
        foreach (var item in args.OldItems)
          if (item is Race race)
            _db.UpdateRace(race, false);
    }
  }


  internal class DatabaseDelegatorRace
  {
    private IAppDataModelDataBase _db;
    Race _race;

    public DatabaseDelegatorRace(Race race, IAppDataModelDataBase db)
    {
      _race = race;
      _db = db;

      _race.RunsChanged += OnRaceRunsChanged;
    }

    private void OnRaceRunsChanged(object source, EventArgs args)
    {
      _db.UpdateRace(_race, true); // Update the race run number; assume to be active (true)
    }
  }



  internal class DatabaseDelegatorClasses
  {
    private IAppDataModelDataBase _db;
    AppDataModel _dm;

    ItemsChangedNotifier _notifierClasses;

    public DatabaseDelegatorClasses(AppDataModel dm, IAppDataModelDataBase db)
    {
      _dm = dm;
      _db = db;

      _notifierClasses = new ItemsChangedNotifier(_dm.GetParticipantClasses());

      _notifierClasses.ItemChanged += OnItemChanged;
      _notifierClasses.CollectionChanged += OnCollectionChanged;
    }

    void OnItemChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender is ParticipantClass c)
        _db.CreateOrUpdateClass(c);
    }

    void OnCollectionChanged(object source, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (ParticipantClass c in e.NewItems)
            _db.CreateOrUpdateClass(c);
          break;

        case NotifyCollectionChangedAction.Remove:
          foreach (ParticipantClass v in e.OldItems)
            _db.RemoveClass(v);
          break;

        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Reset:
          throw new Exception("not implemented");
      }
    }
  }




  internal class DatabaseDelegatorGroups
  {
    private IAppDataModelDataBase _db;
    AppDataModel _dm;

    ItemsChangedNotifier _notifierClasses;

    public DatabaseDelegatorGroups(AppDataModel dm, IAppDataModelDataBase db)
    {
      _dm = dm;
      _db = db;

      _notifierClasses = new ItemsChangedNotifier(_dm.GetParticipantGroups());

      _notifierClasses.ItemChanged += OnItemChanged;
      _notifierClasses.CollectionChanged += OnCollectionChanged;
    }

    void OnItemChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender is ParticipantGroup g)
        _db.CreateOrUpdateGroup(g);
    }

    void OnCollectionChanged(object source, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (ParticipantGroup c in e.NewItems)
            _db.CreateOrUpdateGroup(c);
          break;

        case NotifyCollectionChangedAction.Remove:
          foreach (ParticipantGroup v in e.OldItems)
            _db.RemoveGroup(v);
          break;

        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Reset:
          throw new Exception("not implemented");
      }
    }
  }


  internal class DatabaseDelegatorCategories
  {
    private IAppDataModelDataBase _db;
    AppDataModel _dm;

    ItemsChangedNotifier _notifierClasses;

    public DatabaseDelegatorCategories(AppDataModel dm, IAppDataModelDataBase db)
    {
      _dm = dm;
      _db = db;

      _notifierClasses = new ItemsChangedNotifier(_dm.GetParticipantCategories());

      _notifierClasses.ItemChanged += OnItemChanged;
      _notifierClasses.CollectionChanged += OnCollectionChanged;
    }

    void OnItemChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender is ParticipantCategory c)
        _db.CreateOrUpdateCategory(c);
    }

    void OnCollectionChanged(object source, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (ParticipantCategory c in e.NewItems)
            _db.CreateOrUpdateCategory(c);
          break;

        case NotifyCollectionChangedAction.Remove:
          foreach (ParticipantCategory c in e.OldItems)
            _db.RemoveCategory(c);
          break;

        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Reset:
          throw new Exception("not implemented");
      }
    }
  }


  internal class DatabaseDelegatorTeams
  {
    private IAppDataModelDataBase _db;
    AppDataModel _dm;

    ItemsChangedNotifier _notifier;

    public DatabaseDelegatorTeams(AppDataModel dm, IAppDataModelDataBase db)
    {
      _dm = dm;
      _db = db;

      _notifier = new ItemsChangedNotifier(_dm.GetTeams());

      _notifier.ItemChanged += OnItemChanged;
      _notifier.CollectionChanged += OnCollectionChanged;
    }

    void OnItemChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender is Team t)
        _db.CreateOrUpdateTeam(t);
    }

    void OnCollectionChanged(object source, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (Team t in e.NewItems)
            _db.CreateOrUpdateTeam(t);
          break;

        case NotifyCollectionChangedAction.Remove:
          foreach (Team t in e.OldItems)
            _db.RemoveTeam(t);
          break;

        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Reset:
          throw new Exception("not implemented");
      }
    }
  }

  internal class DatabaseDelegatorTeamGroups
  {
    private IAppDataModelDataBase _db;
    AppDataModel _dm;

    ItemsChangedNotifier _notifier;

    public DatabaseDelegatorTeamGroups(AppDataModel dm, IAppDataModelDataBase db)
    {
      _dm = dm;
      _db = db;

      _notifier = new ItemsChangedNotifier(_dm.GetTeamGroups());

      _notifier.ItemChanged += OnItemChanged;
      _notifier.CollectionChanged += OnCollectionChanged;
    }

    void OnItemChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender is Team t)
        _db.CreateOrUpdateTeam(t);
    }

    void OnCollectionChanged(object source, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (TeamGroup g in e.NewItems)
            _db.CreateOrUpdateTeamGroup(g);
          break;

        case NotifyCollectionChangedAction.Remove:
          foreach (TeamGroup g in e.OldItems)
            _db.RemoveTeamGroup(g);
          break;

        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Replace:
        case NotifyCollectionChangedAction.Reset:
          throw new Exception("not implemented");
      }
    }
  }

}
