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
    private ParticipantSelector _rpSelector;

    CollectionViewSource _participantFilter;
    CollectionViewSource _startNUmberAssignmentFilter;

    public StartNumbersUC()
    {
      InitializeComponent();
    }

    public void Init(AppDataModel dm, Race race, TabControl parent, TabItem thisTabItem)
    {
      _dm = dm;
      _race = race;

      ucSaveOrReset.Init("Startnummerzuweisungen", parent, thisTabItem, existingChanges, saveChanges, resetChanges);

      _snaWorkspace = new StartNumberAssignment();
      _snaWorkspace.ParticipantList.CollectionChanged += OnWorkspaceChanged;
      _snaWorkspace.NextStartnumberChanged += OnNextStartnumberChanged;

      _rpSelector = new ParticipantSelector(_race, _snaWorkspace);
      _rpSelector.CurrentGroupChanged += OnCurrentGroupChangedHandler;
      _rpSelector.GroupingChanged += OnGroupingChangedHandler;

      cmbDirection.Items.Clear();
      cmbDirection.Items.Add(new CBItem { Text = "Aufsteigend", Value = new ParticipantSelector.PointsComparerAsc() });
      cmbDirection.Items.Add(new CBItem { Text = "Absteigend", Value = new ParticipantSelector.PointsComparerDesc() });
      cmbDirection.SelectedIndex = 0;

      _startNUmberAssignmentFilter = new CollectionViewSource() { Source = _snaWorkspace.ParticipantList };
      _startNUmberAssignmentFilter.IsLiveFilteringRequested = true;
      _startNUmberAssignmentFilter.LiveFilteringProperties.Add("StartNumber");
      chkShowEmptyStartNumbers_Click(null, null); // Initially setup the filter
      dgStartList.ItemsSource = _startNUmberAssignmentFilter.View;

      _participantFilter = new CollectionViewSource() { Source = _race.GetParticipants() };
      _participantFilter.Filter += new FilterEventHandler(delegate (object s, FilterEventArgs ea) 
      { 
        RaceParticipant rr = (RaceParticipant)ea.Item; ea.Accepted = !_snaWorkspace.IsAssigned(rr); 
      });
      dgParticipants.ItemsSource = _participantFilter.View;
      //RaceUC.EnableOrDisableColumns(_race, dgStartList);

      IsVisibleChanged += StartNumbersUC_IsVisibleChanged;
    }

    private void SetupDefaults()
    {
      int nVerlosung = int.MaxValue;
      string grouping = _race.RaceConfiguration.Run1_StartistViewGrouping;

      switch (_race.RaceConfiguration.Run1_StartistView)
      {
        case "Startlist_1stRun_StartnumberAscending":
          nVerlosung = int.MaxValue;
          break;
        case "Startlist_1stRun_Points_0":
          nVerlosung = 0;
          break;
        case "Startlist_1stRun_Points_15":
          nVerlosung = 15;
          break;
        case "Startlist_1stRun_Points_30":
          nVerlosung = 30;
          break;
      }

      if (nVerlosung != int.MaxValue)
        txtVerlosung.Text = nVerlosung.ToString();
      else
        txtVerlosung.Text = "";

      cmbGrouping.SelectCBItem(grouping);
    }

    private void StartNumbersUC_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(bool)e.OldValue && (bool)e.NewValue) // Became visible
      {
        _snaWorkspace.LoadFromRace(_race, true);

        UiUtilities.FillGrouping(cmbGrouping, _race.RaceConfiguration.Run1_StartistViewGrouping);
        txtNotToBeAssigned.Text = Properties.Settings.Default.StartNumbersNotToBeAssigned;

        SetupDefaults();

        OnWorkspaceChanged(this, null);
        OnCurrentGroupChangedHandler(this, null);
      }
    }


    private void OnWorkspaceChanged(object source, EventArgs e)
    {
      _participantFilter.View.Refresh();

      enableOrDisableControls();
    }

    private void OnNextStartnumberChanged(object source, EventArgs e)
    { 
      txtNextStartNumber.Text = _snaWorkspace.NextFreeStartNumber.ToString();
      txtNextStartNumberManual.Text = _snaWorkspace.NextFreeStartNumber.ToString();
    }


    private void OnCurrentGroupChangedHandler(object source, EventArgs e)
    {
      // Selected current group
      if (_rpSelector.CurrentGroup != null)
      {
        foreach (var v in cmbNextGroup.Items)
          if (v is CBItem cbItem)
            if (cbItem.Value == _rpSelector.CurrentGroup)
              cmbNextGroup.SelectedItem = v;
      }
      else
        cmbNextGroup.SelectedItem = null;

      enableOrDisableControls();
    }


    private void enableOrDisableControls()
    {
      // Enable / Disable Buttons
      bool enableGroup = (cmbNextGroup.SelectedItem as CBItem)?.Value != null;
      bool enableOthers = !_participantFilter.View.IsEmpty;

      btnAssignCurrentGroup.IsEnabled = enableGroup && enableOthers;
      cmbNextGroup.IsEnabled = enableGroup && enableOthers;

      btnAssignAll.IsEnabled = enableOthers;
      btnAssign.IsEnabled = enableOthers;
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


    private bool existingChanges()
    {
      return _snaWorkspace.DifferentToRace(_race);
    }

    private void resetChanges()
    {
      _snaWorkspace.LoadFromRace(_race);
    }

    private void saveChanges()
    {
      _snaWorkspace.SaveToRace(_race);
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
      var items = dgStartList.SelectedItems.OfType<AssignedStartNumber>().ToList();

      foreach (var i in items)
        _snaWorkspace.Delete(i.StartNumber);
    }


    private void btnAssign_Click(object sender, RoutedEventArgs e)
    {
      setStartNumbersNotToAssign();

      try
      {
        uint sn = uint.Parse(txtNextStartNumberManual.Text);

        var selParticipants = dgParticipants.SelectedItems.OfType<RaceParticipant>().ToList();
        
        _snaWorkspace.SetNextStartNumber(sn);
        foreach (var selParticipant in selParticipants)
        {
          _snaWorkspace.Assign(sn, selParticipant);
          sn++;
        }

        dgParticipants.SelectedIndex = 0;
      }
      catch (Exception)
      { }
    }

    private void btnAssignCurrentGroup_Click(object sender, RoutedEventArgs e)
    {
      setStartNumbersNotToAssign();
      setAnzVerlosung();

      try
      {
        _snaWorkspace.SetNextStartNumber(uint.Parse(txtNextStartNumber.Text));

        if (cmbNextGroup.SelectedValue is CBItem nextGroup)
          _rpSelector.SwitchToGroup(nextGroup.Value);

        _rpSelector.AssignParticipants();
        _rpSelector.SwitchToNextGroup();
      }
      catch (Exception)
      { }
    }

    private void btnAssignAll_Click(object sender, RoutedEventArgs e)
    {
      setStartNumbersNotToAssign();
      setAnzVerlosung();

      try
      {
        _snaWorkspace.SetNextStartNumber(uint.Parse(txtNextStartNumber.Text));
      }
      catch (Exception)
      { }

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

    private void cmbDirection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbDirection.SelectedValue is CBItem direction)
        _rpSelector.Sorting = (ParticipantSelector.ISorting)direction.Value;
    }

    private void setAnzVerlosung()
    {
      try
      {
        int anzVerlosung = int.Parse(txtVerlosung.Text);
        _rpSelector.AnzahlVerlosung = anzVerlosung;
      }
      catch (Exception)
      {
        _rpSelector.AnzahlVerlosung = int.MaxValue;
      }
    }

    private void setStartNumbersNotToAssign()
    {
      var parts = txtNotToBeAssigned.Text.Split(new char[] { ',', ' ', ';' });
      List<uint> snNotToAsign = new List<uint>();
      foreach (var p in parts)
      {

        try
        {
          uint v = uint.Parse(p);
          snNotToAsign.Add(v);
        }
        catch (Exception)
        { }
      }
      _snaWorkspace.SetStartNumbersNotToAssign(snNotToAsign);
    }


    private void startNUmberAssignmentFilter(object sender, FilterEventArgs ea)
    {
      AssignedStartNumber rr = (AssignedStartNumber)ea.Item;
      ea.Accepted = rr.Participant != null;
    }


    private void chkShowEmptyStartNumbers_Click(object sender, RoutedEventArgs e)
    {
      if (chkShowEmptyStartNumbers.IsChecked == false)
        _startNUmberAssignmentFilter.Filter += startNUmberAssignmentFilter;
      else
        _startNUmberAssignmentFilter.Filter -= startNUmberAssignmentFilter;

      _startNUmberAssignmentFilter.View.Refresh();
    }

  }
}
