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
    protected ObservableCollection<RaceResultItem> _teamResults;

    TeamRaceResultViewProvider()
    {
      _teamResults = new ObservableCollection<RaceResultItem>();
    }

    public override ViewProvider Clone()
    {
      return new TeamRaceResultViewProvider();
    }

    protected override object GetViewSource()
    {
      return _teamResults;
    }
  }


  /// <summary>
  /// Interface of common properties for entries in the team reuslt data grid
  /// </summary>
  interface ITeamResultViewListeItems : INotifyPropertyChanged
  {
    string Name { get; }
    TimeSpan? Runtime { get; }
    RunResult.EResultCode ResultCode { get; }
  }


  /// <summary>
  /// Represents a team participant result, possibility to enable/disable whether the item shall be included in the team results
  /// </summary>
  class TeamParticipantItem : ITeamResultViewListeItems
  {
    RaceResultItem _rri;
    bool _consider;

    public TeamParticipantItem(RaceResultItem rri)
    {
      _rri = rri;
    }

    public string Name { get { return _rri.Participant.Name; } }
    public TimeSpan? Runtime { get { return _rri.Runtime; } }
    public RunResult.EResultCode ResultCode { get { return _rri.ResultCode; } }

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
  class TeamResultViewItem : ITeamResultViewListeItems
  {
    #region Members

    protected Team _team;
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

    public override string ToString()
    {
      return string.Format("T: {0} - {1}", Name, TotalTime);
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


}
