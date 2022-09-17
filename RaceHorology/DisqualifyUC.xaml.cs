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

using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static RaceHorologyLib.RunResult;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for DisqualifyUC.xaml
  /// </summary>
  public partial class DisqualifyUC : UserControl
  {
    private AppDataModel _dm;
    private Race _race;
    private RaceRun _currentRaceRun;

    DiqualifyVM _disqualificationsVM;
    CollectionViewSource _viewDisqualifications;
    FilterEventHandler _viewDisqualificationsFilterHandler;

    public List<EResultCode> ListOfResultCodesToSet { get; } = new List<EResultCode> { EResultCode.Normal, EResultCode.DIS, EResultCode.NaS, EResultCode.NiZ, EResultCode.NQ };

    public DisqualifyUC()
    {
      InitializeComponent();

      IsVisibleChanged += DisqualifyUC_IsVisibleChanged;
    }

    private void DisqualifyUC_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(bool)e.OldValue && (bool)e.NewValue)
      {
        UiUtilities.FillCmbRaceRun(cmbRaceRun, _race);
        setRaceRun(_dm.GetCurrentRaceRun());
        _viewDisqualifications.View.Refresh();
      }
    }

    public void Init(AppDataModel dm, Race race)
    {
      _dm = dm;
      _race = race;

      _race.RunsChanged += OnRaceRunsChanged;

      UiUtilities.FillCmbRaceRun(cmbRaceRun, _race);
      UiUtilities.FillGrouping(cmbResultGrouping, _currentRaceRun.GetResultViewProvider().ActiveGrouping);

      cmbFilter.Items.Clear();
      cmbFilter.Items.Add(new CBItem { Text = "alle Teilnehmer", Value = "all" });
      cmbFilter.Items.Add(new CBItem { Text = "Teilnehmer ohne Zeit", Value = "no_time"});
      cmbFilter.Items.Add(new CBItem { Text = "ausgeschiedene Teilnehmer", Value = "out" });
      cmbFilter.Items.Add(new CBItem { Text = "offene Teilnehmer (keine Zeit oder Ausscheidung)", Value = "no_data" });
      cmbFilter.SelectedIndex = 1;

      cmbDisqualify.ItemsSource = ListOfResultCodesToSet;

      cmbDisqualifyReason.Items.Add("Vorbei am Tor");
      cmbDisqualifyReason.Items.Add("Eingefädelt am Tor");
      cmbDisqualifyReason.Items.Add("Nicht weit genug zurückgestiegen am Tor");
      cmbDisqualifyReason.Items.Add("Hilfe durch fremde Person am Tor");
      cmbDisqualifyReason.Items.Add("Unerlaubtes Weiterfahren nach Sturz");

      this.KeyDown += new KeyEventHandler(Timing_KeyDown);
    }


    private void OnRaceRunsChanged(object sender, EventArgs e)
    {
      UiUtilities.FillCmbRaceRun(cmbRaceRun, _race);
    }


    private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      bool isInStartList(RunResult rr)
      {
        return this._currentRaceRun.GetStartListProvider().GetViewList().FirstOrDefault(p => p.Participant == rr.Participant) != null;
      }


      if (_viewDisqualificationsFilterHandler != null)
        _viewDisqualifications.Filter -= _viewDisqualificationsFilterHandler;

      if (cmbFilter.SelectedItem is CBItem selected)
      {
        _viewDisqualificationsFilterHandler = null;
        if (string.Equals(selected.Value, "all"))
          _viewDisqualificationsFilterHandler = new FilterEventHandler(
            delegate (object s, FilterEventArgs ea) 
            { 
              RunResult rr = (RunResult)ea.Item; ea.Accepted = isInStartList(rr); 
            });
        else if (string.Equals(selected.Value, "no_time"))
          _viewDisqualificationsFilterHandler = new FilterEventHandler(
            delegate (object s, FilterEventArgs ea) 
            { 
              RunResult rr = (RunResult)ea.Item; ea.Accepted = isInStartList(rr) && rr.RuntimeWOResultCode == null; 
            });
        else if (string.Equals(selected.Value, "out"))
          _viewDisqualificationsFilterHandler = new FilterEventHandler(
            delegate (object s, FilterEventArgs ea) 
            { 
              RunResult rr = (RunResult)ea.Item; ea.Accepted = isInStartList(rr) && (rr.ResultCode != RunResult.EResultCode.Normal && rr.ResultCode != RunResult.EResultCode.NotSet); 
            });
        else if (string.Equals(selected.Value, "no_data"))
          _viewDisqualificationsFilterHandler = new FilterEventHandler(
            delegate (object s, FilterEventArgs ea) 
            { 
              RunResult rr = (RunResult)ea.Item; ea.Accepted = isInStartList(rr) && rr.ResultCode == RunResult.EResultCode.NotSet; 
            });
      }

      if (_viewDisqualificationsFilterHandler != null)
        _viewDisqualifications.Filter += _viewDisqualificationsFilterHandler;

      _viewDisqualifications.View.Refresh();
    }


    private void CmbRaceRun_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      CBItem selected = (sender as ComboBox).SelectedValue as CBItem;
      RaceRun selectedRaceRun = selected?.Value as RaceRun;

      setRaceRun(selectedRaceRun);
    }


    private void setRaceRun(RaceRun rr)
    {
      if (rr == null)
        return;

      _currentRaceRun = rr;
      connectUiToRaceRun(_currentRaceRun);

      cmbRaceRun.SelectCBItem(rr);
    }


    private void connectUiToRaceRun(RaceRun raceRun)
    {
      if (raceRun != null)
      {
        if (_viewDisqualifications == null)
        {
          _viewDisqualifications = new CollectionViewSource();
          _viewDisqualifications.LiveFilteringProperties.Add(nameof(RunResult.Runtime));
          _viewDisqualifications.LiveFilteringProperties.Add(nameof(RunResult.ResultCode));
          _viewDisqualifications.IsLiveFilteringRequested = true;
        }

        _disqualificationsVM = new DiqualifyVM(raceRun);
        _viewDisqualifications.Source = _disqualificationsVM.GetGridView();

        dgDisqualifications.ItemsSource = _viewDisqualifications.View;
        dgResults.ItemsSource = raceRun.GetResultViewProvider().GetView();

        cmbResultGrouping.SelectCBItem(raceRun.GetResultViewProvider().ActiveGrouping);
      }
      else
      {
        _disqualificationsVM = null;
        dgDisqualifications.ItemsSource = null;
        dgResults.ItemsSource = null;
      }
    }

    private void Timing_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.M && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
      {
        txtStartNumber.Focus();
        txtStartNumber.SelectAll();
      }
      else if (e.Key == Key.F2)
        BtnStore_Click(null, null);
    }


    private void TxtStartNumber_TextChanged(object sender, TextChangedEventArgs e)
    {
      uint startNumber = 0U;
      try { startNumber = uint.Parse(txtStartNumber.Text); } catch (Exception) { }
      RaceParticipant participant = _race.GetParticipant(startNumber);
      if (participant != null)
      {
        txtParticipant.Text = participant.Fullname;
        RunResult rr = _currentRaceRun.GetResultList().FirstOrDefault(r => r.Participant == participant);
        if (rr != null)
        {
          cmbDisqualifyReason.Text = rr.GetDisqualifyText();
          txtDisqualify.Text = rr.GetDisqualifyGoal();
          cmbDisqualify.SelectedValue = rr.ResultCode;
        }
        else
        {
          txtDisqualify.Text = "";
          cmbDisqualify.SelectedValue = null;
        }
      }
      else
        txtParticipant.Text = "";
    }


    private void Txt_GotFocus_SelectAll(object sender, RoutedEventArgs e)
    {
      if (sender is TextBox txtbox)
        txtbox.SelectAll();
    }


    private void BtnStore_Click(object sender, RoutedEventArgs e)
    {
      uint startNumber = 0U;
      try { startNumber = uint.Parse(txtStartNumber.Text); } catch (Exception) { }

      RaceParticipant participant = _race.GetParticipant(startNumber);


      string disqualifyText = RunResultExtension.JoinDisqualifyText(cmbDisqualifyReason.Text, txtDisqualify.Text);

      if (participant != null)
        _currentRaceRun.SetResultCode(participant, (EResultCode)cmbDisqualify.SelectedValue, disqualifyText);
      else
      {
        foreach( object item in dgDisqualifications.SelectedItems)
        {
          if (item is RunResult rr)
            _currentRaceRun.SetResultCode(rr.Participant, (EResultCode)cmbDisqualify.SelectedValue, disqualifyText);
        }
      }

      txtStartNumber.Focus();
    }

    private void CmbResultGrouping_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbResultGrouping.SelectedValue is CBItem grouping)
        _currentRaceRun.GetResultViewProvider().ChangeGrouping((string)grouping.Value);
    }

    private void DgDisqualifications_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (dgDisqualifications.SelectedItems.Count == 1 && dgDisqualifications.SelectedItems[0] is RunResult rr)
      {
        txtStartNumber.Text = rr.StartNumber.ToString();
      }
      else
        txtStartNumber.Text = "Multiple";
    }
  }
}
