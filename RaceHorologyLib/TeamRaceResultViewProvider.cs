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

using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static RaceHorologyLib.RaceResultItem;
using static RaceHorologyLib.RunResult;

namespace RaceHorologyLib
{
  public class TeamRaceResultViewProvider : ViewProvider
  {
    protected TeamTimeSorter _comparer;
    protected Race _race;
    protected AppDataModel _appDataModel;

    protected ObservableCollection<TeamResultViewItem> _teamResults;
    protected ObservableCollection<ITeamResultViewListItems> _teamViewResults;
    protected ItemsChangeObservableCollection<RaceResultItem> _raceResults;


    TeamRaceResultViewProvider()
    {
      _comparer = new TeamTimeSorter();
    }

    public override ViewProvider Clone()
    {
      return new TeamRaceResultViewProvider();
    }

    public virtual void Init(Race race, AppDataModel appDataModel)
    {
      _race = race;
      _appDataModel = appDataModel;

      _teamResults = new ObservableCollection<TeamResultViewItem>();
      _teamViewResults = new ObservableCollection<ITeamResultViewListItems>();

      // Watch for changes of race results
      _raceResults = _race.GetResultViewProvider().GetViewList();
      _raceResults.CollectionChanged += OnRaceResults_CollectionChanged;
      // Initial update
      OnRaceResults_CollectionChanged(_raceResults, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _raceResults.ToList()));

      Calculate();

      FinalizeInit();
    }

    protected override void OnChangeGrouping(string propertyName)
    {
      _comparer.SetGrouping(propertyName);

      Calculate();
    }


    private void OnRaceResults_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      bool bSomethingChanged = false;

      if (e.OldItems != null)
      {
        foreach (INotifyPropertyChanged item in e.OldItems)
        {
          item.PropertyChanged -= OnRaceResultItem_Changed;
          RunResultWithPosition rr = item as RunResultWithPosition;
          bSomethingChanged = true;
        }
      }

      if (e.NewItems != null)
      {
        foreach (INotifyPropertyChanged item in e.NewItems)
        {
          item.PropertyChanged += OnRaceResultItem_Changed; ;
          RunResultWithPosition rr = item as RunResultWithPosition;
          bSomethingChanged = true;
        }
      }

      if (bSomethingChanged)
        Calculate();
    }


    void Calculate()
    {
      // Group By Team
      var itemsPerTeam = _raceResults.GroupBy(i => i.Participant.Team);
      foreach (var team in itemsPerTeam)
      {
        var trri = _teamResults.FirstOrDefault(t => t.Team == team);
        // Update RaceResults and Calculate time and points
        trri.SetRaceResults(team.ToList());
        var consideredTeamMembers = trri.RaceResults.Where(r => r.Consider);
        trri.TotalTime = consideredTeamMembers.Select(r => r).Sum(r => r.Runtime);
        trri.Points = consideredTeamMembers.Sum(r => r.Points);
      }

      // Group, Sort and Rank
      _teamResults.Sort(_comparer);

      // Build viewable List (Flatten)
      _teamViewResults.Clear();
      foreach (var team in _teamResults)
      {
        _teamViewResults.Add(team);
        foreach (var participants in team.RaceResults)
        {
          _teamViewResults.Add(participants);
        }
      }
    }

    private void OnRaceResultItem_Changed(object sender, PropertyChangedEventArgs e)
    {
      Calculate();
    }

    protected override object GetViewSource()
    {
      return _teamViewResults;
    }
  }


  public class TeamTimeSorter : ResultSorter<TeamResultViewItem>
  {
    public override int Compare(TeamResultViewItem teamX, TeamResultViewItem teamY)
    {
      int groupCompare = CompareGroup(teamX, teamY);
      if (groupCompare != 0)
        return groupCompare;

      TimeSpan? tX = teamX.TotalTime;
      TimeSpan? tY = teamY.TotalTime;

      // Sort by time
      if (tX != null && tY == null)
        return -1;

      if (tX == null && tY != null)
        return 1;

      // If equal, consider startnumber as well
      int timeComp = TimeSpan.Compare((TimeSpan)tX, (TimeSpan)tY);
      if (timeComp == 0)
        return teamX.Name.CompareTo(teamY.Name);

      return timeComp;
    }
  }



  /// <summary>
  /// Interface of common properties for entries in the team reuslt data grid
  /// </summary>
  public interface ITeamResultViewListItems : INotifyPropertyChanged
  {
    string Name { get; }
    TimeSpan? Runtime { get; }
    RunResult.EResultCode ResultCode { get; }
  }


  /// <summary>
  /// Represents a team participant result, possibility to enable/disable whether the item shall be included in the team results
  /// </summary>
  public class TeamParticipantItem : ITeamResultViewListItems
  {
    RaceResultItem _rri;
    bool _consider;

    public TeamParticipantItem(RaceResultItem rri)
    {
      _rri = rri;
    }

    public string Name { get { return _rri.Participant.Name; } }
    public TimeSpan? Runtime { get { return _rri.Runtime; } }
    public double Points{ get { return _rri.Points; } }
    public RunResult.EResultCode ResultCode { get { return _rri.ResultCode; } }

    public RaceResultItem Original { get { return _rri; } }

    public Boolean Consider
    {
      get { return _consider; }
      set { if (_consider != value) { _consider = value; NotifyPropertyChanged(); } }
    }

    #region INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
  }


  /// <summary>
  /// Represents a team result item for diaply in a data grid
  /// </summary>
  public class TeamResultViewItem : ITeamResultViewListItems
  {
    #region Members

    protected Team _team;
    protected List<TeamParticipantItem> _raceResults;
    //protected SubResultMap _subResults;
    protected TimeSpan? _totalTime;
    protected RunResult.EResultCode _resultCode;
    protected string _disqualText;
    protected uint _position;
    protected TimeSpan? _diffToFirst;
    protected double _points;

    #endregion

    TeamResultViewItem(Team team)
    {
      _team = team;
      _totalTime = null;
      _resultCode = RunResult.EResultCode.Normal;
      _disqualText = null;
      _position = 0;
      _diffToFirst = null;
      _points = -1.0;
    }

    /// <summary>
    /// Returns the final time (sum or minimum time depending on the race type)
    /// </summary>
    public TimeSpan? TotalTime
    {
      get { return _totalTime; }
      set { if (_totalTime != value) { _totalTime = value; NotifyPropertyChanged(); } }
    }

    /// <summary>
    /// Returns the final time (sum or minimum time depending on the race type)
    /// </summary>
    public TimeSpan? Runtime
    {
      get { return _totalTime; }
    }

    public RunResult.EResultCode ResultCode
    {
      get { return _resultCode; }
      set { if (_resultCode != value) { _resultCode = value; NotifyPropertyChanged(); } }
    }

    public string DisqualText
    {
      get { return _disqualText; }
      set { if (_disqualText != value) { _disqualText = value; NotifyPropertyChanged(); } }
    }


    /// <summary>
    /// The position within the classement
    /// </summary>
    public uint Position
    {
      get { return _position; }
      set { if (_position != value) { _position = value; NotifyPropertyChanged(); } }
    }

    public TimeSpan? DiffToFirst
    {
      get { return _diffToFirst; }
      set { if (_diffToFirst != value) { _diffToFirst = value; NotifyPropertyChanged(); } }
    }


    /// <summary>
    /// The position within the classement
    /// </summary>
    public double Points
    {
      get { return _points; }
      set { if (_points != value) { _points = value; NotifyPropertyChanged(); } }
    }

    public string Name
    {
      get { return _team.Name; }
    }
    public Team Team
    {
      get { return _team; }
    }

    public override string ToString()
    {
      return string.Format("T: {0} - {1}", Name, TotalTime);
    }

    public void SetRaceResults(List<RaceResultItem> results)
    {
      foreach (var r in _raceResults)
      {
        if (!results.Contains(r.Original))
          _raceResults.Remove(r);
      }
      foreach (var rri in results)
      {
        var f = _raceResults.FirstOrDefault(r => r.Original == rri);
        if (f == null)
          _raceResults.Add(new TeamParticipantItem(rri));
      }
    }
    public List<TeamParticipantItem> RaceResults
    {
      get { return _raceResults; }
    }



    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }


  public static class StatisticExtensions
  {
    public static TimeSpan? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan?> selector)
    {
      return source.Select(selector).Aggregate(TimeSpan.Zero, (t1, t2) =>
      {
        if (t1 != null && t2 != null)
          return (TimeSpan)t1 + (TimeSpan)t2;
        if (t1 != null)
          return (TimeSpan)t1;
        if (t2 != null)
          return (TimeSpan)t2;
        return TimeSpan.Zero;
      });
    }
  }
}
