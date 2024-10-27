/*
 *  Copyright (C) 2019 - 2024 by Sven Flossmann
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

using Microsoft.Win32;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WebSocketSharp;

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
    ComboBox _cmbLiveTimingStatus;

    // Working Data
    RaceRun _currentRaceRun;

    RemainingStartListViewProvider _rslVP;

    public RaceUC(AppDataModel dm, Race race, LiveTimingMeasurement liveTimingMeasurement, ComboBox cmbLiveTimingStatus)
    {
      _dataModel = dm;
      _thisRace = race;
      _liveTimingMeasurement = liveTimingMeasurement;
      _liveTimingMeasurement.LiveTimingMeasurementStatusChanged += OnLiveTimingMeasurementStatusChanged;

      _cmbLiveTimingStatus = cmbLiveTimingStatus;
      _cmbLiveTimingStatus.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                      new System.Windows.Controls.TextChangedEventHandler(
                        new DelayedEventHandler(
                          TimeSpan.FromMilliseconds(400),
                          CmbLiveTimingStatus_TextChanged
                        ).Delayed));
      _cmbLiveTimingStatus.SelectionChanged += new ComboBoxDelayedEventHandler(
          TimeSpan.FromMilliseconds(400),
          CmbLiveTimingStatus_SelectionChanged
      ).Delayed;

      InitializeComponent();

      ucStartNumbers.Init(_dataModel, _thisRace, tabControlRace1, tabItemStartNumberAssignment);
      ucDisqualify.Init(_dataModel, _thisRace);

      InitializeConfiguration();

      InitializeRaceProperties();

      InitializeLiveTiming(_thisRace);

      InitializeTiming();

      ucRaceLists.Init(_thisRace);
      ucReports.Init(_thisRace);
    }

    public Race GetRace() { return _thisRace; }
    public RaceRun GetRaceRun() { return _currentRaceRun; }


    #region Configuration

    private void InitializeConfiguration()
    {
      _thisRace.PropertyChanged += thisRace_PropertyChanged;

      ucRaceConfig.Init(_thisRace.RaceConfiguration, _thisRace.RaceType);

      ucRaceConfigSaveOrReset.Init(
        "Konfigurationsänderungen",
        tabControlRace1, tabItemConfiguration,
        configuration_ExistingChanges, configuration_SaveChanges, configuration_ResetChanges);
    }

    private void thisRace_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "RaceConfiguration")
        configuration_reconfigureViews();
    }

    private bool configuration_ExistingChanges()
    {
      return ucRaceConfig.ExistingChanges();
    }

    private void configuration_SaveChanges()
    {
      RaceConfiguration cfg = new RaceConfiguration();

      _thisRace.RaceConfiguration = ucRaceConfig.GetConfig();

    }

    private void configuration_ResetChanges()
    {
      ucRaceConfig.ResetChanges();
    }


    private void btnClearConfiguration_Click(object sender, RoutedEventArgs e)
    {
      _thisRace.RaceConfiguration = null;
    }


    private void configuration_reconfigureViews()
    {
      ViewConfigurator viewConfigurator = new ViewConfigurator(_thisRace);
      viewConfigurator.ConfigureRace(_thisRace);

      ucRaceConfig.Init(_thisRace.RaceConfiguration, _thisRace.RaceType);

      imgTabHeaderConfiguration.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(
        _thisRace.IsRaceConfigurationLocal ? "/Icons/icons8-umkreist-l-50.png" : "/Icons/icons8-umkreist-g-50.png",
        UriKind.Relative));

      // Reset UI
      ConnectUiToRaceRun(_currentRaceRun);
      ucRaceLists.UpdateAll();
    }

    #endregion


    #region Race Properties

    AdditionalRaceProperties _addRaceProps;
    void InitializeRaceProperties()
    {
      ucPropSaveOrReset.Init("Renndatenänderungen",
                              tabControlRace1, tabItemRaceProperties,
                              prop_ExistingChanges, prop_SaveChanges, prop_ResetChanges);
      prop_ResetChanges();
    }

    private bool prop_ExistingChanges()
    {
      return !AdditionalRaceProperties.Equals(_thisRace.AdditionalProperties, _addRaceProps);
    }

    private void prop_ResetChanges()
    {
      _addRaceProps = _thisRace.AdditionalProperties.Copy();
      tabItemRaceProperties.DataContext = _addRaceProps;
    }

    private void prop_SaveChanges()
    {
      _thisRace.AdditionalProperties = _addRaceProps.Copy();
    }

    private void btnLoadProp_Click(object sender, RoutedEventArgs ea)
    {
      try
      {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter =
          "Race Horology Daten|*.mdb|DSValpin Daten|*.mdb";
        if (openFileDialog.ShowDialog() == true)
        {
          Database importDB = new Database();
          importDB.Connect(openFileDialog.FileName);
          AppDataModel importModel = new AppDataModel(importDB);

          if (importModel.GetRaces().Count() > 0)
          {
            var race = importModel.GetRace(0);
            _addRaceProps = race.AdditionalProperties.Copy();
            tabItemRaceProperties.DataContext = _addRaceProps;
          }
          else
            throw new Exception(string.Format("Die Bewerbsdatei {0} enthält keine Rennen.", openFileDialog.FileName));
        }
      }
      catch (Exception e)
      {
        MessageBox.Show("Die Daten konnten nicht importiert werden.\n\nFehlermeldung: " + e.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }


    private void txtHeight_TextChanged(object sender, TextChangedEventArgs e)
    {
      var text = "";
      int startHeight, finishHeight, diffHeight;
      if (int.TryParse(txtStartHeight.Text, out startHeight) &&
          int.TryParse(txtFinishHeight.Text, out finishHeight))
      {
        if (startHeight > finishHeight)
        {
          // Both textboxes contain valid integer values
          diffHeight = startHeight - finishHeight;
          text = $"Höhendifferenz (m): {diffHeight}";
        }
      }
      LabelDiffHeight.Content = text;
    }


    private void TextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (sender is TextBox textbox)
      {
        int value;
        if (int.TryParse(textbox.Text, out value))
        {
          int delta = Keyboard.Modifiers == ModifierKeys.Control ? 10 : 1;
          if (e.Delta > 0)
            value += delta;
          else
            if (value > 0)
            value -= delta;

          textbox.Text = string.Format("{0}", value);
        }
        e.Handled = true;
      }
    }

    private void txtStartHeight_LostFocus(object sender, RoutedEventArgs e)
    {
      int value = 0;
      int.TryParse(txtFinishHeight.Text, out value);
      if (!txtStartHeight.Text.IsNullOrEmpty() && (txtFinishHeight.Text.IsNullOrEmpty() || value == 0))
        txtFinishHeight.Text = txtStartHeight.Text;
    }

    private void txtFinishHeight_LostFocus(object sender, RoutedEventArgs e)
    {
      int value = 0;
      int.TryParse(txtStartHeight.Text, out value);
      if (!txtFinishHeight.Text.IsNullOrEmpty() && (txtStartHeight.Text.IsNullOrEmpty() || value == 0))
        txtStartHeight.Text = txtFinishHeight.Text;
    }


    #endregion


    #region Live Timing

    private void InitializeLiveTiming(Race race)
    {
      liveTimingRMUC.InitializeLiveTiming(race);
      liveTimingFISUC.InitializeLiveTiming(race);

      updateLiveTimingStatus();
      // TODO: Should actually be refactored to listen to the signals ILiveTiming.StatusChanged instead of polling
      var timer = new System.Windows.Threading.DispatcherTimer();
      timer.Tick += new EventHandler(liveTiming_OnTimeBeforeRefreshElapsed);
      timer.Interval = new TimeSpan(0, 0, 1);
      timer.Start();
    }

    private void liveTiming_OnTimeBeforeRefreshElapsed(object sender, EventArgs e)
    {
      updateLiveTimingStatus();
    }


    protected void CmbLiveTimingStatus_TextChanged(object sender, TextChangedEventArgs e)
    {
      string text = "";
      if (sender is ComboBox comboBox)
      {
        text = comboBox.Text;
      }
      liveTimingRMUC._liveTimingRM?.UpdateStatus(text);
      liveTimingFISUC._liveTimingFIS?.UpdateStatus(text);
    }

    protected void CmbLiveTimingStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      string text = "";
      if (sender is ComboBox comboBox)
      {
        if (comboBox.SelectedItem != null)
        {
          text = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
        }
        else
        {
          text = comboBox.Text;
        }
      }
      liveTimingRMUC._liveTimingRM?.UpdateStatus(text);
      liveTimingFISUC._liveTimingFIS?.UpdateStatus(text);
    }

    private void updateLiveTimingStatus()
    {
      bool isRunning = false;

      if (liveTimingRMUC._liveTimingRM != null)
        isRunning = liveTimingRMUC._liveTimingRM.Started;
      if (liveTimingFISUC._liveTimingFIS != null)
        isRunning = liveTimingFISUC._liveTimingFIS.Started;

      imgTabHeaderLiveTiming.Visibility = isRunning ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion


    #region Timing

    LiveTimingAutoNiZ _liveTimingAutoNiZ;
    LiveTimingAutoNaS _liveTimingAutoNaS;
    LiveTimingStartCountDown _liveTimingStartCountDown;
    DataGridColumnVisibilityContextMenu _dgColVisRemainingStarters;
    DataGridColumnVisibilityContextMenu _dgColVisRunning;
    DataGridColumnVisibilityContextMenu _dgColVisFinish;

    public class LiveTimingStartCountDown : IDisposable
    {
      LiveTimingMeasurement _ltm;
      RaceRun _raceRun;
      uint _startIntervall;
      Label _lblStart;

      bool _liveTimingOn;
      enum EStatus { StartFree, Blocked };
      EStatus _status;

      TimerPlus _timer;

      SoundPlayer _soundLow;
      SoundPlayer _soundHigh;
      int _soundStatus;

      public LiveTimingStartCountDown(uint startIntervall, RaceRun raceRun, Label lblStart, LiveTimingMeasurement ltm)
      {
        _ltm = ltm;
        _raceRun = raceRun;
        _startIntervall = startIntervall;
        _lblStart = lblStart;

        _soundLow = new SoundPlayer(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("RaceHorology.resources.count_down_low.wav"));
        _soundHigh = new SoundPlayer(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("RaceHorology.resources.count_down_high_long.wav"));
        _soundStatus = 0;

        _ltm.LiveTimingMeasurementStatusChanged += ltm_LiveTimingMeasurementStatusChanged;
        _raceRun.OnTrackChanged += OnSomethingChanged;

        _timer = new TimerPlus(OnTimeout, OnUpdate, (int)_startIntervall, true);

        _liveTimingOn = _ltm.IsRunning;
        _status = EStatus.StartFree;
        updateStatus();
      }

      private void ltm_LiveTimingMeasurementStatusChanged(object sender, bool isRunning)
      {
        _liveTimingOn = isRunning;
        if (!_liveTimingOn)
        {
          _timer.Stop();
          _timer.Reset();
        }
        else
          _status = EStatus.StartFree;

        Application.Current.Dispatcher.Invoke(() =>
        {
          updateStatus();
        });
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

        _status = EStatus.Blocked;
        _soundStatus = 4;
        updateStatus();
      }


      private void OnTimeout()
      {
        Application.Current.Dispatcher.Invoke(() =>
        {
          _status = EStatus.StartFree;
          updateStatus();
        });
      }


      void updateStatus()
      {
        if (!_liveTimingOn)
        {
          _lblStart.Content = "Zeitmessung gestoppt!";
          _lblStart.Background = Brushes.Orange;
        }
        else if (_status == EStatus.StartFree)
        {
          _lblStart.Content = "Start Frei!";
          _lblStart.Background = Brushes.LightGreen;
        }
        else if (_status == EStatus.Blocked)
        {
          _lblStart.Background = Brushes.Red;
          _lblStart.Content = string.Format("Start frei in {0}s", _timer.RemainingSeconds);
        }
      }

      private void OnUpdate()
      {
        Application.Current.Dispatcher.Invoke(() =>
        {
          updateStatus();

          if (_timer.RemainingSeconds < _soundStatus)
          {
            if (_soundStatus > 1)
              _soundLow.Play();
            else
              _soundHigh.Play();

            _soundStatus = (int)_timer.RemainingSeconds;
          }
        });
      }


      public void Dispose()
      {
        Application.Current.Dispatcher.Invoke(() =>
        {
          _timer.Dispose();
          _raceRun.OnTrackChanged -= OnSomethingChanged;
          _ltm.LiveTimingMeasurementStatusChanged -= ltm_LiveTimingMeasurementStatusChanged;
        });
      }
    }



    private void InitializeTiming()
    {
      UiUtilities.FillCmbRaceRun(cmbRaceRun, _thisRace);

      cmbManualMode.Items.Add(new CBItem { Text = "Laufzeit", Value = "Absolut" });
      cmbManualMode.Items.Add(new CBItem { Text = "Differenz", Value = "Difference" });
      cmbManualMode.SelectedIndex = 1;

      gridManualMode.Visibility = Visibility.Collapsed;

      this.KeyDown += new KeyEventHandler(Timing_KeyDown);

      Properties.Settings.Default.PropertyChanged += SettingChangingHandler;

      _thisRace.RunsChanged += OnRaceRunsChanged;
    }


    List<LiveTimeParticipantAssigning> _ltpa = new List<LiveTimeParticipantAssigning>();
    private void ReInitLtpa()
    {
      if (_currentRaceRun == null)
        return;

      bool showParticipantAssignment = Properties.Settings.Default.Timing_DisplayPartcipantAssignment;

      while (_ltpa.Count > 0)
      {
        _ltpa[_ltpa.Count - 1].Dispose();
        _ltpa.RemoveAt(_ltpa.Count - 1);
      }
      _ltpa.Add(new LiveTimeParticipantAssigning(_currentRaceRun, EMeasurementPoint.Start));
      _ltpa.Add(new LiveTimeParticipantAssigning(_currentRaceRun, EMeasurementPoint.Finish));
      mlapaStart.Init(_ltpa[0], _thisRace);
      mlapaFinish.Init(_ltpa[1], _thisRace);

      UpdateLiveTimingStartStopButtons(false); // Initial status

      if (showParticipantAssignment)
      {
        grdTimingMain.RowDefinitions[5].Height = new GridLength(5);
        grdTimingMain.RowDefinitions[6].Height = new GridLength(3, GridUnitType.Star);
      }
      else
      {
        grdTimingMain.RowDefinitions[5].Height = new GridLength(0);
        grdTimingMain.RowDefinitions[6].Height = new GridLength(0);
      }
    }


    private void OnRaceRunsChanged(object sender, EventArgs e)
    {
      UiUtilities.FillCmbRaceRun(cmbRaceRun, _thisRace);
    }


    private void cmbRaceRun_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (_liveTimingMeasurement != null && _liveTimingMeasurement.IsRunning)
      {
        MessageBox.Show("Durchgangswechsel während einer Zeitnahme nicht möglich.\n\nBeenden Sie erst die aktuelle Zeitnahme und wähle Sie anschließend den Durchgang aus.", "Durchgangswechsel nicht möglich", MessageBoxButton.OK, MessageBoxImage.Information);
        e.Handled = true;
      }
    }

    private void CmbRaceRun_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      CBItem selected = (sender as ComboBox).SelectedValue as CBItem;
      RaceRun selectedRaceRun = selected?.Value as RaceRun;


      if (_currentRaceRun != selectedRaceRun)
      {
        // Stop any helper
        if (_liveTimingAutoNiZ != null)
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
        case "Timing_DisplayPartcipantAssignment":
          ReInitLtpa();
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
        if (_liveTimingStartCountDown != null)
          _liveTimingStartCountDown.Dispose();
        _liveTimingStartCountDown = new LiveTimingStartCountDown(Properties.Settings.Default.StartTimeIntervall, _currentRaceRun, lblStartCountDown, _liveTimingMeasurement);
      }
      else
      {
        lblStartCountDown.Visibility = Visibility.Hidden;
        if (_liveTimingStartCountDown != null)
          _liveTimingStartCountDown.Dispose();
        _liveTimingStartCountDown = null;
      }
    }


    private void ConnectUiToRaceRun(RaceRun raceRun)
    {
      if (raceRun != null)
      {
        _rslVP = (new ViewConfigurator(_thisRace)).GetRemainingStartersViewProvider(raceRun);
        dgRemainingStarters.ItemsSource = _rslVP.GetView();
        UiUtilities.EnableOrDisableColumns(_thisRace, dgRemainingStarters);
        _dgColVisRemainingStarters = new DataGridColumnVisibilityContextMenu(dgRemainingStarters, "timing_remaining_starter");
        dgRemainingStarters.SelectedItem = null;

        dgRunning.ItemsSource = raceRun.GetOnTrackList();
        UiUtilities.EnableOrDisableColumns(_thisRace, dgRunning);
        _dgColVisRunning = new DataGridColumnVisibilityContextMenu(dgRunning, "timing_running");

        dgFinish.ItemsSource = raceRun.GetInFinishList();
        UiUtilities.EnableOrDisableColumns(_thisRace, dgFinish);
        _dgColVisFinish = new DataGridColumnVisibilityContextMenu(dgFinish, "timing_finish");

        lblStartList.DataContext = _rslVP.GetView();

        ReInitLtpa();
      }
      else
      {
        dgRemainingStarters.ItemsSource = null;
        dgRunning.ItemsSource = null;
        dgFinish.ItemsSource = null;
      }
    }

    private void _finishView_Filter(object sender, FilterEventArgs e)
    {
      RunResult rr = (RunResult)e.Item;

      e.Accepted =
        (rr.ResultCode != RunResult.EResultCode.NotSet && rr.ResultCode != RunResult.EResultCode.Normal)
        || (rr.ResultCode == RunResult.EResultCode.Normal && ((rr.StartTime != null && rr.FinishTime != null) || rr.RuntimeIntern != null));
    }


    /// <summary>
    /// Enables / disable UI elements based on whether LiveTimingMeasurement is performed or not
    /// </summary>
    private void OnLiveTimingMeasurementStatusChanged(object sender, bool isRunning)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        RaceRun selRRUI = (cmbRaceRun.SelectedValue as CBItem)?.Value as RaceRun;
        System.Diagnostics.Debug.Assert(selRRUI == _currentRaceRun);

        UpdateLiveTimingStartStopButtons(isRunning);
      });
    }


    private void LiveTimingStart_Click(object sender, RoutedEventArgs e)
    {
      if (_liveTimingMeasurement == null)
      {
        MessageBox.Show("Zeitnahmegerät ist nicht verfügbar.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      if (!_thisRace.IsConsistent)
      {
        MessageBox.Show("Die Startnummern sind nicht eindeutig zugewiesen.\n(Die Zeitnahme wird dennoch gestartet.)", "Warnung", MessageBoxButton.OK, MessageBoxImage.Warning);
      }

      if (_thisRace.GetPreviousRun(_currentRaceRun) != null && !_thisRace.GetPreviousRun(_currentRaceRun).IsComplete)
      {
        MessageBox.Show("Der vorhergehende Durchlauf ist noch nicht komplett abgeschlossen.\n(Die Zeitnahme wird dennoch gestartet.)", "Warnung", MessageBoxButton.OK, MessageBoxImage.Warning);
      }

      _liveTimingMeasurement.AutoAddParticipants = Properties.Settings.Default.AutoAddParticipants;
      _liveTimingMeasurement.Start();

      _thisRace.SetTimingDeviceInfo(_liveTimingMeasurement.LiveTimingDevice.GetDeviceInfo());
    }


    private void LiveTimingStop_Click(object sender, RoutedEventArgs e)
    {
      if (_liveTimingMeasurement == null)
        return;

      _liveTimingMeasurement.Stop();
    }


    private void UpdateLiveTimingStartStopButtons(bool isRunning)
    {
      // Enable buttons if Timing Device is generally online
      bool enableButtons = _liveTimingMeasurement?.LiveTimingDevice?.OnlineStatus == StatusType.Online;

      btnLiveTimingStart.IsEnabled = enableButtons;
      btnLiveTimingStop.IsEnabled = enableButtons;

      if (!enableButtons)
      {
        btnLiveTimingStart.IsChecked = false;
        btnLiveTimingStop.IsChecked = false;
        imgTabHeaderTiming.Visibility = Visibility.Collapsed;
      }
      else
      {
        bool running = _liveTimingMeasurement.IsRunning;
        // Set corresponding color whether running or not
        btnLiveTimingStart.IsChecked = running;
        btnLiveTimingStop.IsChecked = !running;
        imgTabHeaderTiming.Visibility = running ? Visibility.Visible : Visibility.Collapsed;
      }
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
          txtRun.IsEnabled = false;
        }
      }
    }



    private void Timing_KeyDown(object sender, KeyEventArgs e)
    {
      if (tabControlRace1.SelectedItem != tabItemTiming)
        return;

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
      bool startTimeAvailable = CheckTime(txtStart);
      bool finishTimeAvailable = CheckTime(txtFinish);

      if (!txtRun.IsEnabled) // Only calculate in case it is read only
        UpdateRunTime();

      CheckTime(txtRun);
    }


    private bool CheckTime(TextBox txtbox)
    {

      TimeSpan? ts = TimeSpanExtensions.ParseTimeSpan(txtbox.Text);
      bool validTime = ts != null;

      if (!validTime && !string.IsNullOrWhiteSpace(txtbox.Text) && txtbox.IsEnabled)
        txtbox.Background = Brushes.Orange;
      else
        txtbox.Background = Brushes.White;

      return validTime;
    }

    private void UpdateRunTime()
    {
      try
      {
        TimeSpan? run = null;
        TimeSpan? start = TimeSpanExtensions.ParseTimeSpan(txtStart.Text);
        TimeSpan? finish = TimeSpanExtensions.ParseTimeSpan(txtFinish.Text);

        if (start != null && finish != null)
          run = (new RoundedTimeSpan((TimeSpan)(finish - start), 2, RoundedTimeSpan.ERoundType.Floor)).TimeSpan;

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
          txtStart.Text = rr.GetStartTime()?.ToString(@"hh\:mm\:ss\,ffff");
          txtFinish.Text = rr.GetFinishTime()?.ToString(@"hh\:mm\:ss\,ffff");
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


    private void btnManualModeShow_Click(object sender, RoutedEventArgs e)
    {
      if (gridManualMode.Visibility == Visibility.Visible)
      {
        gridManualMode.Visibility = Visibility.Collapsed;
        btnManualModeShow.Content = "Anzeigen";
      }
      else
      {
        gridManualMode.Visibility = Visibility.Visible;
        btnManualModeShow.Content = "Verstecken";
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

    private void BtnRowNaS(object sender, RoutedEventArgs e)
    {
      for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
        if (vis is DataGridRow dataRow)
          if (dataRow.Item is StartListEntry sle)
            _currentRaceRun.SetResultCode(sle.Participant, RunResult.EResultCode.NaS);
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
      if (participant != null)
        _currentRaceRun.SetResultCode(participant, code);

      selectNextParticipant(participant);
    }

    private void selectNextParticipant(RaceParticipant currentParticipant)
    {
      RaceParticipant nextParticipant = null;
      bool useNext = false;
      foreach (var sle in _rslVP.GetView().SourceCollection.OfType<StartListEntry>())
      {
        if (useNext)
        {
          nextParticipant = sle.Participant;
          break;
        }
        if (sle.Participant == currentParticipant)
          useNext = true;
      }

      if (nextParticipant != null)
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

    private void dgRunning_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (dgRunning.SelectedItem is RunResult entry)
      {
        txtStartNumber.Text = entry.StartNumber.ToString();
      }
    }

    #endregion

  }

}
