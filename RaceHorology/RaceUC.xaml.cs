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
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for RaceUC.xaml
  /// </summary>
  public partial class RaceUC : UserControl
  {
    // Input Data
    AppDataModel _dataModel;
    readonly Race _thisRace;
    LiveTimingMeasurement _liveTimingMeasurement;
    TextBox _txtLiveTimingStatus;

    // Working Data
    RaceRun _currentRaceRun;

    RemainingStartListViewProvider _rslVP;

    ScrollToMeasuredItemBehavior dgResultsScrollBehavior;


    public RaceUC(AppDataModel dm, Race race, LiveTimingMeasurement liveTimingMeasurement, TextBox txtLiveTimingStatus)
    {
      _dataModel = dm;
      _thisRace = race;
      _liveTimingMeasurement = liveTimingMeasurement;
      _liveTimingMeasurement.LiveTimingMeasurementStatusChanged += OnLiveTimingMeasurementStatusChanged;

      _txtLiveTimingStatus = txtLiveTimingStatus;
      _txtLiveTimingStatus.TextChanged += new DelayedEventHandler(
          TimeSpan.FromMilliseconds(400),
          TxtLiveTimingStatus_TextChanged
      ).Delayed;


      InitializeComponent();

      ucStartNumbers.Init(_dataModel, _thisRace);
      ucDisqualify.Init(_dataModel, _thisRace);
      
      InitializeConfiguration();

      InitializeRaceProperties();

      InitializeLiveTiming(_thisRace);

      InitializeTiming();

      ucRaceLists.Init(_thisRace);
    }

    public Race GetRace() { return _thisRace; }
    public RaceRun GetRaceRun() { return _currentRaceRun; }


    #region Configuration

    RaceConfiguration _raceConfiguration;
    RaceConfigurationPresets _raceConfigurationPresets;

    private void InitializeConfiguration()
    {
      // ApplicationFolder + raceconfigpresets
      _raceConfigurationPresets = new RaceConfigurationPresets(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"raceconfigpresets"));
      foreach (var config in _raceConfigurationPresets.GetConfigurations())
      {
        cmbTemplate.Items.Add(new CBItem { Text = config.Key, Value = config.Value });
      }



      _raceConfiguration = _thisRace.RaceConfiguration.Copy();

      // Configuration Screen
      cmbRuns.Items.Add(new CBItem { Text = "1", Value = 1 });
      cmbRuns.Items.Add(new CBItem { Text = "2", Value = 2 });
      cmbRuns.Items.Add(new CBItem { Text = "3", Value = 3 });
      cmbRuns.Items.Add(new CBItem { Text = "4", Value = 4 });
      cmbRuns.Items.Add(new CBItem { Text = "5", Value = 5 });
      cmbRuns.Items.Add(new CBItem { Text = "6", Value = 6 });

      // Result
      UiUtilities.FillGrouping(cmbConfigErgebnisGrouping);

      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Bester Durchgang", Value = "RaceResult_BestOfTwo" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Summe der besten 2 Durchgänge", Value = "RaceResult_SumBest2" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Summe", Value = "RaceResult_Sum" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Summe + Punkte nach DSV Schülerreglement", Value = "RaceResult_SumDSVPointsSchool" });

      // Run 1
      UiUtilities.FillGrouping(cmbConfigStartlist1Grouping);
      cmbConfigStartlist1.Items.Add(new CBItem { Text = "Startnummer (aufsteigend)", Value = "Startlist_1stRun_StartnumberAscending" });
      cmbConfigStartlist1.Items.Add(new CBItem { Text = "Punkte (nicht gelost)", Value = "Startlist_1stRun_Points_0" });
      cmbConfigStartlist1.Items.Add(new CBItem { Text = "Punkte (ersten 15 gelost)", Value = "Startlist_1stRun_Points_15" });
      cmbConfigStartlist1.Items.Add(new CBItem { Text = "Punkte (ersten 30 gelost)", Value = "Startlist_1stRun_Points_30" });

      // Run 2
      UiUtilities.FillGrouping(cmbConfigStartlist2Grouping);
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Startnummer (aufsteigend)", Value = "Startlist_2nd_StartnumberAscending" });
      //cmbConfigStartlist2.Items.Add(new GroupingCBItem { Text = "Startnummer (aufsteigend, inkl. ohne Ergebnis)", Value = "Startlist_2nd_StartnumberAscending" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Startnummer (absteigend)", Value = "Startlist_2nd_StartnumberDescending" });
      //cmbConfigStartlist2.Items.Add(new GroupingCBItem { Text = "Startnummer (absteigend, inkl. ohne Ergebnis)", Value = "Startlist_2nd_StartnumberDescending" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (nicht gedreht)", Value = "Startlist_2nd_PreviousRun_0_OnlyWithResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (nicht gedreht, inkl. ohne Ergebnis)", Value = "Startlist_2nd_PreviousRun_0_AlsoWithoutResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (ersten 15 gedreht)", Value = "Startlist_2nd_PreviousRun_15_OnlyWithResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (ersten 15 gedreht, inkl. ohne Ergebnis)", Value = "Startlist_2nd_PreviousRun_15_AlsoWithoutResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (ersten 30 gedreht)", Value = "Startlist_2nd_PreviousRun_30_OnlyWithResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (ersten 30 gedreht, inkl. ohne Ergebnis)", Value = "Startlist_2nd_PreviousRun_30_AlsoWithoutResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (alle gedreht)", Value = "Startlist_2nd_PreviousRun_all_OnlyWithResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (alle gedreht, inkl. ohne Ergebnis)", Value = "Startlist_2nd_PreviousRun_all_AlsoWithoutResults" });

      ResetConfigurationSelectionUI(_raceConfiguration);
    }


    private void CmbTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbTemplate.SelectedValue is CBItem selected)
      {
        if (selected.Value is RaceConfiguration config)
          ResetConfigurationSelectionUI(config);
      }
    }



    private void ResetConfigurationSelectionUI(RaceConfiguration cfg)
    {
      cmbRuns.SelectCBItem(cfg.Runs);
      cmbConfigErgebnisGrouping.SelectCBItem(cfg.DefaultGrouping);
      cmbConfigErgebnis.SelectCBItem(cfg.RaceResultView);
      cmbConfigStartlist1.SelectCBItem(cfg.Run1_StartistView);
      cmbConfigStartlist1Grouping.SelectCBItem(cfg.Run1_StartistViewGrouping);
      cmbConfigStartlist2.SelectCBItem(cfg.Run2_StartistView);
      cmbConfigStartlist2Grouping.SelectCBItem(cfg.Run2_StartistViewGrouping);
      txtValueF.Text = cfg.ValueF.ToString();
      txtValueA.Text = cfg.ValueA.ToString();
      txtMinPenalty.Text = cfg.MinimumPenalty.ToString();

      chkConfigFieldsYear.IsChecked = cfg.ActiveFields.Contains("Year");
      chkConfigFieldsClub.IsChecked = cfg.ActiveFields.Contains("Club");
      chkConfigFieldsNation.IsChecked = cfg.ActiveFields.Contains("Nation");
      chkConfigFieldsCode.IsChecked = cfg.ActiveFields.Contains("Code");
      chkConfigFieldsPoints.IsChecked = cfg.ActiveFields.Contains("Points");
      chkConfigFieldsPercentage.IsChecked = cfg.ActiveFields.Contains("Percentage");

    }

    private bool StoreConfigurationSelectionUI(ref RaceConfiguration cfg)
    {

      if (cmbRuns.SelectedIndex < 0
        || cmbConfigErgebnisGrouping.SelectedIndex < 0
        || cmbConfigErgebnis.SelectedIndex < 0
        || cmbConfigStartlist1.SelectedIndex < 0
        || cmbConfigStartlist1Grouping.SelectedIndex < 0
        || cmbConfigStartlist2.SelectedIndex < 0
        || cmbConfigStartlist2Grouping.SelectedIndex < 0
        )
        return false;

      cfg.Runs = (int)((CBItem)cmbRuns.SelectedValue).Value;
      cfg.DefaultGrouping = (string)((CBItem)cmbConfigErgebnisGrouping.SelectedValue).Value;
      cfg.RaceResultView = (string)((CBItem)cmbConfigErgebnis.SelectedValue).Value;
      cfg.Run1_StartistView = (string)((CBItem)cmbConfigStartlist1.SelectedValue).Value;
      cfg.Run1_StartistViewGrouping = (string)((CBItem)cmbConfigStartlist1Grouping.SelectedValue).Value;
      cfg.Run2_StartistView = (string)((CBItem)cmbConfigStartlist2.SelectedValue).Value;
      cfg.Run2_StartistViewGrouping = (string)((CBItem)cmbConfigStartlist2Grouping.SelectedValue).Value;
      try { cfg.ValueF = double.Parse(txtValueF.Text); } catch (Exception) { }
      try { cfg.ValueA = double.Parse(txtValueA.Text); } catch (Exception) { }
      try { cfg.MinimumPenalty = double.Parse(txtMinPenalty.Text); } catch (Exception) { }

      void enableField(List<string> fieldList, string field, bool? enabled)
      {
        if (enabled != null && (bool)enabled)
        {
          if (!fieldList.Contains(field))
            fieldList.Add(field);
        }
        else
        {
          if (fieldList.Contains(field))
            fieldList.Remove(field);
        }
      }

      enableField(cfg.ActiveFields, "Year", chkConfigFieldsYear.IsChecked);
      enableField(cfg.ActiveFields, "Club", chkConfigFieldsClub.IsChecked);
      enableField(cfg.ActiveFields, "Nation", chkConfigFieldsNation.IsChecked);
      enableField(cfg.ActiveFields, "Code", chkConfigFieldsCode.IsChecked);
      enableField(cfg.ActiveFields, "Points", chkConfigFieldsPoints.IsChecked);
      enableField(cfg.ActiveFields, "Percentage", chkConfigFieldsPercentage.IsChecked);

      return true;
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
      ResetConfigurationSelectionUI(_raceConfiguration);
    }

    private void BtnApply_Click(object sender, RoutedEventArgs e)
    {
      RaceConfiguration cfg = new RaceConfiguration();
      if (!StoreConfigurationSelectionUI(ref cfg))
      {
        MessageBox.Show("Alle Optionen müssen korrekt ausgefüllt sein.", "Optionen fehlerhaft", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      _raceConfiguration = cfg.Copy();

      _thisRace.RaceConfiguration = cfg;

      ViewConfigurator viewConfigurator = new ViewConfigurator(_thisRace);
      viewConfigurator.ConfigureRace(_thisRace);

      // Reset UI (TODO should adapt itself based on events)
      ConnectUiToRaceRun(_currentRaceRun);
      ucRaceLists.UpdateAll();
    }


    #endregion


    #region Race Properties

    AdditionalRaceProperties _addRaceProps;
    void InitializeRaceProperties()
    {
      _addRaceProps = _thisRace.AdditionalProperties.Copy();
      RaceProperties.DataContext = _addRaceProps;
    }

    private void BtnAddPropReset_Click(object sender, RoutedEventArgs e)
    {
      InitializeRaceProperties();
    }

    private void BtnAddPropApply_Click(object sender, RoutedEventArgs e)
    {
      _thisRace.AdditionalProperties = _addRaceProps.Copy();
    }


    #endregion


    #region Live Timing

    private void InitializeLiveTiming(Race race) 
    {
      liveTimingRMUC.InitializeLiveTiming(race);
      liveTimingFISUC.InitializeLiveTiming(race);
    }

    protected void TxtLiveTimingStatus_TextChanged(object sender, TextChangedEventArgs e)
    {
      string text = "";
      Application.Current.Dispatcher.Invoke(() =>
      {
        if (sender is TextBox textBox)
        {
          text = textBox.Text;
        }
      });


      if (liveTimingRMUC._liveTimingRM != null)
        liveTimingRMUC._liveTimingRM.UpdateStatus(text);
      if (liveTimingFISUC._liveTimingFIS != null)
        liveTimingFISUC._liveTimingFIS.UpdateStatus(text);
    }

    #endregion


    #region Timing

    LiveTimingAutoNiZ _liveTimingAutoNiZ;
    LiveTimingAutoNaS _liveTimingAutoNaS;
    LiveTimingStartCountDown _liveTimingStartCountDown;

    public class LiveTimingStartCountDown : IDisposable
    {
      RaceRun _raceRun;
      uint _startIntervall;
      Label _lblStart;

      TimerPlus _timer;

      SoundPlayer _soundLow;
      SoundPlayer _soundHigh;
      int _soundStatus;

      public LiveTimingStartCountDown(uint startIntervall, RaceRun raceRun, Label lblStart)
      {
        _raceRun = raceRun;
        _startIntervall = startIntervall;
        _lblStart = lblStart;

        _soundLow = new SoundPlayer(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("RaceHorology.resources.count_down_low.wav"));
        _soundHigh = new SoundPlayer(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("RaceHorology.resources.count_down_high_long.wav"));
        _soundStatus = 0;

        _raceRun.OnTrackChanged += OnSomethingChanged;

        _timer = new TimerPlus(OnTimeout, OnUpdate, (int)_startIntervall, true);

        displayStartFree();
      }


      private void OnSomethingChanged(object sender, RaceParticipant participantEnteredTrack, RaceParticipant participantLeftTrack, RunResult currentRunResult)
      {
        if (participantEnteredTrack != null)
          startCountDown();
      }


      private void startCountDown()
      {
        _timer.Reset();
        _timer.Start();

        _soundStatus = 4;
      }


      private void OnTimeout()
      {
        Application.Current.Dispatcher.Invoke(() =>
        {
          displayStartFree();
        });
      }

      void displayStartFree()
      {
        _lblStart.Content = "Start Frei!";
        _lblStart.Background = Brushes.LightGreen;
      }

      private void OnUpdate()
      {
        Application.Current.Dispatcher.Invoke(() =>
        {
          _lblStart.Background = Brushes.Red;
          _lblStart.Content = string.Format("Start frei in {0}s", _timer.RemainingSeconds);

          if (_timer.RemainingSeconds < _soundStatus)
          {
            if (_soundStatus > 1)
              _soundLow.Play();
            else
              _soundHigh.Play();

            _soundStatus = (int) _timer.RemainingSeconds;
          }
        });

      }


      public void Dispose()
      {
        Application.Current.Dispatcher.Invoke(() =>
        {
          _timer.Dispose();
          _raceRun.OnTrackChanged -= OnSomethingChanged;
        });
      }
    }



    private void InitializeTiming()
    {
      UiUtilities.FillCmbRaceRun(cmbRaceRun, _thisRace);

      cmbManualMode.Items.Add(new CBItem { Text = "Laufzeit", Value = "Absolut" });
      cmbManualMode.Items.Add(new CBItem { Text = "Differenz", Value = "Difference" });
      cmbManualMode.SelectedIndex = 0;

      this.KeyDown += new KeyEventHandler(Timing_KeyDown);

      Properties.Settings.Default.PropertyChanged += SettingChangingHandler;

      _thisRace.RunsChanged += OnRaceRunsChanged;
    }


    private void OnRaceRunsChanged(object sender, EventArgs e)
    {
      UiUtilities.FillCmbRaceRun(cmbRaceRun, _thisRace);
    }

    private void CmbRaceRun_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      CBItem selected = (sender as ComboBox).SelectedValue as CBItem;
      RaceRun selectedRaceRun = selected?.Value as RaceRun;


      if (_currentRaceRun != selectedRaceRun)
      {
        // Stop any helper
        if (_liveTimingAutoNiZ!=null)
          _liveTimingAutoNiZ.Dispose();
        _liveTimingAutoNiZ = null;

        if (_liveTimingAutoNaS != null)
          _liveTimingAutoNaS.Dispose();
        _liveTimingAutoNaS = null;

        // Remember new race run
        _currentRaceRun = selectedRaceRun;

        if (_currentRaceRun != null)
          if (_dataModel.GetCurrentRace() == _currentRaceRun.GetRace())
            _dataModel.SetCurrentRaceRun(_currentRaceRun);

        ConnectUiToRaceRun(_currentRaceRun);

        ConfigureTimingHelper();
      }
    }

    private void SettingChangingHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case "AutomaticNiZTimeout":
        case "AutomaticNaSStarters":
        case "StartTimeIntervall":
          ConfigureTimingHelper();
          break;
        default:
          break;
      }
    }

    private void ConfigureTimingHelper()
    {
      // Start any helper
      if (Properties.Settings.Default.AutomaticNiZTimeout > 0 && _currentRaceRun != null)
        _liveTimingAutoNiZ = new LiveTimingAutoNiZ(Properties.Settings.Default.AutomaticNiZTimeout, _currentRaceRun);
      else if (_liveTimingAutoNiZ != null)
      {
        _liveTimingAutoNiZ.Dispose();
        _liveTimingAutoNiZ = null;
      }

      if (_liveTimingAutoNaS != null)
        _liveTimingAutoNaS.Dispose();
      if (_currentRaceRun != null)
        _liveTimingAutoNaS = new LiveTimingAutoNaS(Properties.Settings.Default.AutomaticNaSStarters, _currentRaceRun);

      if (Properties.Settings.Default.StartTimeIntervall > 0 && _currentRaceRun != null)
      {
        lblStartCountDown.Visibility = Visibility.Visible;
        _liveTimingStartCountDown = new LiveTimingStartCountDown(Properties.Settings.Default.StartTimeIntervall, _currentRaceRun, lblStartCountDown);
      }
      else
      {
        lblStartCountDown.Visibility = Visibility.Hidden;
        _liveTimingStartCountDown.Dispose();
        _liveTimingStartCountDown = null;
      }
    }


    private void ConnectUiToRaceRun(RaceRun raceRun)
    {
      if (raceRun != null)
      {
        _rslVP  = (new ViewConfigurator(_thisRace)).GetRemainingStartersViewProvider(raceRun);
        dgRemainingStarters.ItemsSource = _rslVP.GetView();
        UiUtilities.EnableOrDisableColumns(_thisRace, dgRemainingStarters);

        dgRunning.ItemsSource = raceRun.GetOnTrackList();
        UiUtilities.EnableOrDisableColumns(_thisRace, dgRunning);

        dgFinish.ItemsSource = raceRun.GetInFinishList();
        UiUtilities.EnableOrDisableColumns(_thisRace, dgFinish);
        dgResultsScrollBehavior = new ScrollToMeasuredItemBehavior(dgFinish, _dataModel);
      }
      else
      {
        dgRemainingStarters.ItemsSource = null;
        dgRunning.ItemsSource = null;
        dgFinish.ItemsSource = null;
        dgResultsScrollBehavior = null;
      }
    }

    private void _finishView_Filter(object sender, FilterEventArgs e)
    {
      RunResult rr = (RunResult)e.Item;

      e.Accepted = 
        (rr.ResultCode != RunResult.EResultCode.NotSet && rr.ResultCode != RunResult.EResultCode.Normal) 
        || (rr.ResultCode == RunResult.EResultCode.Normal && ((rr.StartTime != null && rr.FinishTime != null)|| rr.RuntimeIntern != null ));
    }


    /// <summary>
    /// Enables / disable the recerun combobox depending on whether LiveTimingMeasurement is performed or not
    /// </summary>
    private void OnLiveTimingMeasurementStatusChanged(object sender, bool isRunning)
    {
      cmbRaceRun.IsEnabled = !isRunning;

      RaceRun selRRUI = (cmbRaceRun.SelectedValue as CBItem)?.Value as RaceRun;
      System.Diagnostics.Debug.Assert(selRRUI == _currentRaceRun);
    }


    private void CmbManualMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbManualMode.SelectedItem is CBItem item)
      {
        if (object.Equals(item.Value, "Absolut"))
        {
          txtStart.IsEnabled = false;
          txtFinish.IsEnabled = false;
          txtRun.IsEnabled = true;
        }
        else if (object.Equals(item.Value, "Difference"))
        {
          txtStart.IsEnabled = true;
          txtFinish.IsEnabled = true;
          txtRun.IsEnabled = true;
        }
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
        BtnManualTimeStore_Click(null, null);
      else if (e.Key == Key.D1 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        TestingTime("start");
      else if (e.Key == Key.D2 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        TestingTime("stop");
      else if (e.Key == Key.D9 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        TestingTime("clear_start");
      else if (e.Key == Key.D0 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        TestingTime("clear_stop");
    }


    private void TxtManualTime_GotFocus(object sender, RoutedEventArgs e)
    {
      if (sender is TextBox txtbox)
      {
        txtbox.SelectAll();
      }
    }

    private void TxtManualTime_LostFocus(object sender, RoutedEventArgs e)
    {
      CheckTime(txtStart);
      CheckTime(txtFinish);
      UpdateRunTime();
      CheckTime(txtRun);
    }


    private void CheckTime(TextBox txtbox)
    {
      TimeSpan? ts = TimeSpanExtensions.ParseTimeSpan(txtbox.Text);
      if (ts == null && !string.IsNullOrWhiteSpace(txtbox.Text) && txtbox.IsEnabled)
        txtbox.Background = Brushes.Orange;
      else
        txtbox.Background = Brushes.White;
    }

    private void UpdateRunTime()
    {
      try
      {
        TimeSpan? start = TimeSpanExtensions.ParseTimeSpan(txtStart.Text);
        TimeSpan? finish = TimeSpanExtensions.ParseTimeSpan(txtFinish.Text);
        TimeSpan? run = finish - start;
        if (run != null)
          txtRun.Text = run?.ToString(@"mm\:ss\,ff");
      }
      catch (Exception)
      { }
    }


    private void TxtStartNumber_TextChanged(object sender, TextChangedEventArgs e)
    {
      uint startNumber = 0U;
      try { startNumber = uint.Parse(txtStartNumber.Text); } catch (Exception) { }
      RaceParticipant participant = _thisRace.GetParticipant(startNumber);
      if (participant != null)
      {
        txtParticipant.Text = participant.Fullname;
        RunResult rr = _currentRaceRun.GetResultList().FirstOrDefault(r => r.Participant == participant);
        if (rr != null)
        {
          txtStart.Text = rr.GetStartTime()?.ToString(@"hh\:mm\:ss\,ff");
          txtFinish.Text = rr.GetFinishTime()?.ToString(@"hh\:mm\:ss\,ff");
          txtRun.Text = rr.GetRunTime()?.ToString(@"mm\:ss\,ff");
        }
        else
        {
          txtStart.Text = "";
          txtFinish.Text = "";
          txtRun.Text = "";
        }
      }
      else
      {
        txtParticipant.Text = "";
        txtStart.Text = "";
        txtFinish.Text = "";
        txtRun.Text = "";
      }

      CheckTime(txtStart);
      CheckTime(txtFinish);
      CheckTime(txtRun);
    }


    private void TestingTime(string command)
    {
      uint startNumber = 0U;
      try { startNumber = uint.Parse(txtStartNumber.Text); } catch (Exception) { }
      RaceParticipant participant = _thisRace.GetParticipant(startNumber);

      TimeSpan time = DateTime.Now - DateTime.Today;

      if (participant != null)
      {
        if (command == "start")
          _currentRaceRun.SetStartTime(participant, time);
        if (command == "stop")
          _currentRaceRun.SetFinishTime(participant, time);
        if (command == "clear_start")
          _currentRaceRun.SetStartTime(participant, null);
        if (command == "clear_stop")
          _currentRaceRun.SetFinishTime(participant, null);
      }
    }


    private void BtnManualTimeStore_Click(object sender, RoutedEventArgs e)
    {
      TimeSpan? start = null, finish = null, run = null;
      start = TimeSpanExtensions.ParseTimeSpan(txtStart.Text);
      finish = TimeSpanExtensions.ParseTimeSpan(txtFinish.Text);
      run = TimeSpanExtensions.ParseTimeSpan(txtRun.Text);

      uint startNumber = 0U;
      try { startNumber = uint.Parse(txtStartNumber.Text); } catch (Exception) { }
      RaceParticipant participant = _thisRace.GetParticipant(startNumber);

      if (participant != null)
      {
        bool bDifference = object.Equals((cmbManualMode.SelectedItem as CBItem)?.Value, "Difference");
        bool bDeletingTime = (bDifference && (start == null || finish == null)) || run == null;

        if (bDeletingTime)
        {
          MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Manche Zeiten werden gelöscht\nFortfahren?", "Zeiten löschen?", System.Windows.MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
          if (messageBoxResult == MessageBoxResult.No)
            return;
        }

        if (bDifference)
          _currentRaceRun.SetStartFinishTime(participant, start, finish);

        _currentRaceRun.SetRunTime(participant, run);

      }

      selectNextParticipant(participant);
    }


    private void btnHandTiming_Click(object sender, RoutedEventArgs e)
    {
      HandTimingDlg dlg = new HandTimingDlg { Owner = Window.GetWindow(this) };
      dlg.Init(_dataModel, _thisRace);
      dlg.Show();
    }

    private void btnManualTimingNaS_Click(object sender, RoutedEventArgs e)
    {
      storeResultCodeAndSelectNext(RunResult.EResultCode.NaS);
    }

    private void btnManualTimingNiZ_Click(object sender, RoutedEventArgs e)
    {
      storeResultCodeAndSelectNext(RunResult.EResultCode.NiZ);
    }

    private void btnManualTimingDIS_Click(object sender, RoutedEventArgs e)
    {
      storeResultCodeAndSelectNext(RunResult.EResultCode.DIS);
    }

    private void BtnRowNiZ(object sender, RoutedEventArgs e)
    {
      for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
        if (vis is DataGridRow dataRow)
          if (dataRow.Item is RunResult rr)
            _currentRaceRun.SetResultCode(rr.Participant, RunResult.EResultCode.NiZ);
    }

    private void BtnRowDIS(object sender, RoutedEventArgs e)
    {
      for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
        if (vis is DataGridRow dataRow)
          if (dataRow.Item is RunResult rr)
            _currentRaceRun.SetResultCode(rr.Participant, RunResult.EResultCode.DIS);
    }

    private void storeResultCodeAndSelectNext(RunResult.EResultCode code)
    {
      uint startNumber = 0U;
      try { startNumber = uint.Parse(txtStartNumber.Text); } catch (Exception) { }
      RaceParticipant participant = _thisRace.GetParticipant(startNumber);
      if (participant!= null)
        _currentRaceRun.SetResultCode(participant, code);

      selectNextParticipant(participant);
    }

    private void selectNextParticipant(RaceParticipant currentParticipant)
    {
      RaceParticipant nextParticipant = null;
      bool useNext = false;
      foreach(var sle in _rslVP.GetView().SourceCollection.OfType<StartListEntry>())
      {
        if (useNext)
        {
          nextParticipant = sle.Participant;
          break;
        }
        if (sle.Participant == currentParticipant)
          useNext = true;
      }

      if (nextParticipant!= null)
      {
        txtStartNumber.Text = nextParticipant.StartNumber.ToString();
      }

      txtStartNumber.Focus();
    }

    private void dgRemainingStarters_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (dgRemainingStarters.SelectedItem is StartListEntry entry)
      {
        txtStartNumber.Text = entry.StartNumber.ToString();
      }

    }

    #endregion

  }

}
