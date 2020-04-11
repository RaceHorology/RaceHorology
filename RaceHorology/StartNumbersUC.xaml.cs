/*
 *  Copyright (C) 2019 - 2020 by Sven Flossmann
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for StartNumbersUC.xaml
  /// </summary>
  public partial class StartNumbersUC : UserControl
  {
    private AppDataModel _dm;
    private Race _race;

    private StartNumberAssignment _snaWorkspace;
    private ParticpantSelector _rpSelector;

    CollectionViewSource _participantFilter;

    public StartNumbersUC()
    {
      InitializeComponent();
    }

    public void Init(AppDataModel dm, Race race)
    {
      _dm = dm;
      _race = race;

      _snaWorkspace = new StartNumberAssignment();
      _snaWorkspace.ParticipantList.CollectionChanged += OnWorkspaceChanged;
      _snaWorkspace.NextStartnumberChanged += OnNextStartnumberChanged;

      _rpSelector = new ParticpantSelector(_race, _snaWorkspace);
      _rpSelector.CurrentGroupChanged += OnCurrentGroupChangedHandler;
      _rpSelector.GroupingChanged += OnGroupingChangedHandler;

      dgStartList.ItemsSource = _snaWorkspace.ParticipantList;

      _participantFilter = new CollectionViewSource() { Source = _race.GetParticipants() };
      _participantFilter.Filter += new FilterEventHandler(delegate (object s, FilterEventArgs ea) 
      { 
        RaceParticipant rr = (RaceParticipant)ea.Item; ea.Accepted = !_snaWorkspace.IsAssigned(rr); 
      });

      dgParticipants.ItemsSource = _participantFilter.View;
      //RaceUC.EnableOrDisableColumns(_race, dgStartList);

      IsVisibleChanged += StartNumbersUC_IsVisibleChanged;

      UiUtilities.FillGrouping(cmbGrouping);
    }

    private void StartNumbersUC_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(bool)e.OldValue && (bool)e.NewValue)
      {
        _snaWorkspace.LoadFromRace(_race);

        OnWorkspaceChanged(this, null);
        OnCurrentGroupChangedHandler(this, null);
      }
    }


    private void OnWorkspaceChanged(object source, EventArgs e)
    {
      _participantFilter.View.Refresh();
    }

    private void OnNextStartnumberChanged(object source, EventArgs e)
    { 
      txtNextStartNumber.Text = _snaWorkspace.NextFreeStartNumber.ToString();
      txtNextStartNumberManual.Text = _snaWorkspace.NextFreeStartNumber.ToString();
    }


    private void OnCurrentGroupChangedHandler(object source, EventArgs e)
    {
      // Selected current group
      foreach (var v in cmbNextGroup.Items)
        if (v is CBItem cbItem)
          if (cbItem.Value == _rpSelector.CurrentGroup)
            cmbNextGroup.SelectedItem = v;
    }


    private void OnGroupingChangedHandler(object source, EventArgs e)
    {
      cmbNextGroup.Items.Clear();
      foreach (var g in _rpSelector.Group2Participant.Keys)
      {
        cmbNextGroup.Items.Add(new CBItem { Text = g.ToString(), Value = g });
      }
    }


    private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
    {
      _snaWorkspace.DeleteAll();
      _rpSelector.SwitchToFirstGroup();
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
      _snaWorkspace.LoadFromRace(_race);
    }

    private void BtnApply_Click(object sender, RoutedEventArgs e)
    {

    }


    private void btnInsert_Click(object sender, RoutedEventArgs e)
    {
      if (dgStartList.SelectedItem is AssignedStartNumber selItem)
      {
        _snaWorkspace.InsertAndShift(selItem.StartNumber);
      }
    }


    private void btnRemove_Click(object sender, RoutedEventArgs e)
    {
      if (dgStartList.SelectedItem is AssignedStartNumber selItem)
      {
        _snaWorkspace.Delete(selItem.StartNumber);
      }
    }


    private void btnAssign_Click(object sender, RoutedEventArgs e)
    {

      try
      {
        uint sn = uint.Parse(txtNextStartNumberManual.Text);

        if (dgParticipants.SelectedItem is RaceParticipant selItem)
        {
          _snaWorkspace.Assign(sn, selItem);
        }
      }
      catch (Exception)
      { }
    }

    private void btnAssignCurrentGroup_Click(object sender, RoutedEventArgs e)
    {
      _rpSelector.AssignParticipants();
      _rpSelector.SwitchToNextGroup();
    }

    private void btnAssignAll_Click(object sender, RoutedEventArgs e)
    {
      do
      {
        _rpSelector.AssignParticipants();
      }
      while (_rpSelector.SwitchToNextGroup());
    }

    private void cmbGrouping_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbGrouping.SelectedValue is CBItem grouping)
        _rpSelector.GroupProperty = (string)grouping.Value;
    }

  }
}
