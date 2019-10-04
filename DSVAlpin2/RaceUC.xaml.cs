using DSVAlpin2Lib;
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

namespace DSVAlpin2
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

    // Working Data
    RaceRun _currentRaceRun;

    RemainingStartListViewProvider _rslVP;

    ScrollToMeasuredItemBehavior dgResultsScrollBehavior;
    ScrollToMeasuredItemBehavior dgTotalResultsScrollBehavior;


    public RaceUC(AppDataModel dm, Race race, LiveTimingMeasurement liveTimingMeasurement)
    {
      _dataModel = dm;
      _thisRace = race;
      _liveTimingMeasurement = liveTimingMeasurement;
      _liveTimingMeasurement.LiveTimingMeasurementStatusChanged += OnLiveTimingMeasurementStatusChanged;

      InitializeComponent();

      InitializeConfiguration();

      InitializeTiming();

      InitializeTotalResults();
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
      foreach(var config in _raceConfigurationPresets.GetConfigurations())
      {
        cmbTemplate.Items.Add(new CBItem { Text = config.Key, Value = config.Value });
      }



      _raceConfiguration = _thisRace.RaceConfiguration.Copy();
      
      // Configuration Screen
      cmbRuns.Items.Add(new CBItem { Text = "1", Value = 1 });
      cmbRuns.Items.Add(new CBItem { Text = "2", Value = 2 });

      // Result
      FillGrouping(cmbConfigErgebnisGrouping);
      
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Bester Durchgang", Value = "RaceResult_BestOfTwo" });
      cmbConfigErgebnis.Items.Add(new CBItem { Text = "Summe", Value = "RaceResult_Sum" });

      // Run 1
      FillGrouping(cmbConfigStartlist1Grouping);
      cmbConfigStartlist1.Items.Add(new CBItem { Text = "Startnummer (aufsteigend)", Value = "Startlist_1stRun_StartnumberAscending" });
      cmbConfigStartlist1.Items.Add(new CBItem { Text = "Punkte (ersten 15 gelost)", Value = "Startlist_1stRun_Points_15" });
      cmbConfigStartlist1.Items.Add(new CBItem { Text = "Punkte (ersten 30 gelost)", Value = "Startlist_1stRun_Points_30" });

      // Run 2
      FillGrouping(cmbConfigStartlist2Grouping);
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Startnummer (aufsteigend)", Value = "Startlist_2nd_StartnumberAscending" });
      //cmbConfigStartlist2.Items.Add(new GroupingCBItem { Text = "Startnummer (aufsteigend, inkl. ohne Ergebnis)", Value = "Startlist_2nd_StartnumberAscending" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Startnummer (absteigend)", Value = "Startlist_2nd_StartnumberDescending" });
      //cmbConfigStartlist2.Items.Add(new GroupingCBItem { Text = "Startnummer (absteigend, inkl. ohne Ergebnis)", Value = "Startlist_2nd_StartnumberDescending" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit", Value = "Startlist_2nd_PreviousRunOnlyWithResults" });
      cmbConfigStartlist2.Items.Add(new CBItem { Text = "Vorheriger Lauf nach Zeit (inkl. ohne Ergebnis)", Value = "Startlist_2nd_PreviousRunAlsoWithoutResults" });

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
    }

    private void StoreConfigurationSelectionUI(ref RaceConfiguration cfg)
    {
      cfg.Runs = (int)((CBItem)cmbRuns.SelectedValue).Value;
      cfg.DefaultGrouping = (string)((CBItem)cmbConfigErgebnisGrouping.SelectedValue).Value;
      cfg.RaceResultView = (string)((CBItem)cmbConfigErgebnis.SelectedValue).Value;
      cfg.Run1_StartistView = (string)((CBItem)cmbConfigStartlist1.SelectedValue).Value;
      cfg.Run1_StartistViewGrouping = (string)((CBItem)cmbConfigStartlist1Grouping.SelectedValue).Value;
      cfg.Run2_StartistView = (string)((CBItem)cmbConfigStartlist2.SelectedValue).Value;
      cfg.Run2_StartistViewGrouping = (string)((CBItem)cmbConfigStartlist2Grouping.SelectedValue).Value;
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
      ResetConfigurationSelectionUI(_raceConfiguration);
    }

    private void BtnApply_Click(object sender, RoutedEventArgs e)
    {
      RaceConfiguration cfg = new RaceConfiguration();
      StoreConfigurationSelectionUI(ref cfg);

      _raceConfiguration = cfg.Copy();

      _thisRace.RaceConfiguration = cfg;
      ViewConfigurator viewConfigurator = new ViewConfigurator(_dataModel);
      //viewConfigurator.ApplyNewConfig(cfg);
      viewConfigurator.ConfigureRace(_thisRace);

      // Reset UI (TODO should adapt itself based on events)
      ConnectUiToRaceRun(_currentRaceRun);
      InitializeTotalResults();
    }


    #endregion


    #region Timing


    private void InitializeTiming()
    {
      FillCmbRaceRun(cmbRaceRun);

      FillGrouping(cmbStartListGrouping, _currentRaceRun.GetStartListProvider().ActiveGrouping);
      FillGrouping(cmbResultGrouping, _currentRaceRun.GetResultViewProvider().ActiveGrouping);

      cmbManualMode.Items.Add(new CBItem { Text = "Laufzeit", Value = "Absolut" });
      cmbManualMode.Items.Add(new CBItem { Text = "Differenz", Value = "Difference" });
      cmbManualMode.SelectedIndex = 0;

      this.KeyDown += new KeyEventHandler(Timing_KeyDown);
    }


    private void FillCmbRaceRun(ComboBox cmb)
    {
      cmb.Items.Clear();

      // Fill Runs
      for (int i = 0; i < _thisRace.GetMaxRun(); i++)
      {
        string sz1 = String.Format("{0}. Durchgang", i + 1);
        cmb.Items.Add(new CBItem { Text = sz1, Value = _thisRace.GetRun(i) });
      }
      cmb.SelectedIndex = 0;
    }

    private void CmbRaceRun_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      CBItem selected = (sender as ComboBox).SelectedValue as CBItem;
      RaceRun selectedRaceRun = selected?.Value as RaceRun;

      _currentRaceRun = selectedRaceRun;
      if (_currentRaceRun != null)
        _dataModel.SetCurrentRaceRun(_currentRaceRun);

      ConnectUiToRaceRun(_currentRaceRun);
    }


    private void ConnectUiToRaceRun(RaceRun raceRun)
    {
      if (raceRun != null)
      {
        dgStartList.ItemsSource = _thisRace.GetParticipants();

        _rslVP = new RemainingStartListViewProvider();
        _rslVP.Init(raceRun.GetStartListProvider(), raceRun);
        dgRemainingStarters.ItemsSource = _rslVP.GetView();

        dgRunning.ItemsSource = raceRun.GetOnTrackList();
        dgResults.ItemsSource = raceRun.GetResultViewProvider().GetView();
        dgResultsScrollBehavior = new ScrollToMeasuredItemBehavior(dgResults, _dataModel);

        cmbStartListGrouping.SelectCBItem(_rslVP.ActiveGrouping);
        cmbResultGrouping.SelectCBItem(raceRun.GetResultViewProvider().ActiveGrouping);
      }
      else
      {
        dgStartList.ItemsSource = null;
        dgRemainingStarters.ItemsSource = null;
        dgRunning.ItemsSource = null;
        dgResults.ItemsSource = null;
        dgResultsScrollBehavior = null;
      }
    }


    /// <summary>
    /// Enables / disable the recerun combobox depending on whether LiveTimingMeasurement is performed or not
    /// </summary>
    private void OnLiveTimingMeasurementStatusChanged(object sender, bool isRunning)
    {
      cmbRaceRun.IsEnabled = !isRunning;

      RaceRun selRRUI = (cmbRaceRun.SelectedValue as CBItem)?.Value as RaceRun ;
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
      if (ts == null && txtbox.IsEnabled)
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
        if (run!=null)
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
      }
      else
        txtParticipant.Text = "";
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

      txtStartNumber.Focus();
    }


    private void CmbStartListGrouping_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbStartListGrouping.SelectedValue is CBItem grouping)
        _rslVP.ChangeGrouping((string)grouping.Value);
    }

    private void CmbResultGrouping_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbResultGrouping.SelectedValue is CBItem grouping)
        _currentRaceRun.GetResultViewProvider().ChangeGrouping((string)grouping.Value);
    }

    #endregion


    #region TotalResults

    private void InitializeTotalResults()
    {
      RaceResultViewProvider vp = _thisRace.GetResultViewProvider();

      FillGrouping(cmbTotalResultGrouping, vp.ActiveGrouping);
      FillCmbRaceRun(cmbTotalResult);
      cmbTotalResult.Items.Add(new CBItem { Text = "Rennergebnis", Value = null });
      cmbTotalResult.SelectedIndex = cmbTotalResult.Items.Count - 1;
    }


    private void CmbTotalResultGrouping_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ViewProvider vp = null;
      if (cmbTotalResult.SelectedValue is CBItem selected)
      {
        if (selected.Value is RaceRun selectedRaceRun)
          vp = selectedRaceRun.GetResultViewProvider();
        else
          // Total Results
          vp = _thisRace.GetResultViewProvider();
      }

      if (cmbTotalResultGrouping.SelectedValue is CBItem grouping)
        vp?.ChangeGrouping((string)grouping.Value);
    }

    private void CmbTotalResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbTotalResult.SelectedValue is CBItem selected)
      {
        while (dgTotalResults.Columns.Count > 7)
          dgTotalResults.Columns.RemoveAt(dgTotalResults.Columns.Count - 1);


        ViewProvider vp;
        if (selected.Value is RaceRun selectedRaceRun)
        {
          vp = selectedRaceRun.GetResultViewProvider();

          DataGridTextColumn dgc = new DataGridTextColumn
          {
            Header = "Zeit"
          };
          Binding b = new Binding("Runtime")
          {
            Mode = BindingMode.OneWay,
            StringFormat = @"{0:mm\:ss\,ff}"
          };
          dgc.Binding = b;
          dgTotalResults.Columns.Add(dgc);
        }
        else
        {
          // Total Results
          vp = _thisRace.GetResultViewProvider();

          for(int i=0; i<2; i++)
          {
            DataGridTextColumn dgc2 = new DataGridTextColumn
            {
              Header = string.Format("Zeit {0}", i + 1)
            };
            Binding b2 = new Binding(string.Format("RunTimes[{0}]", i+1))
            {
              Mode = BindingMode.OneWay,
              StringFormat = @"{0:mm\:ss\,ff}"
            };
            dgc2.Binding = b2;
            dgTotalResults.Columns.Add(dgc2);
          }

          DataGridTextColumn dgc = new DataGridTextColumn
          {
            Header = "Total"
          };
          Binding b = new Binding("TotalTime")
          {
            Mode = BindingMode.OneWay,
            StringFormat = @"{0:mm\:ss\,ff}"
          };
          dgc.Binding = b;
          dgTotalResults.Columns.Add(dgc);
        }


        dgTotalResults.ItemsSource = vp.GetView();
        dgTotalResultsScrollBehavior = new ScrollToMeasuredItemBehavior(dgTotalResults, _dataModel);
        cmbTotalResultGrouping.SelectCBItem(vp.ActiveGrouping);
      }
    }

    #endregion



    public static void FillGrouping(ComboBox comboBox, string selected = null)
    {
      comboBox.Items.Clear();
      comboBox.Items.Add(new CBItem { Text = "---", Value = null });
      comboBox.Items.Add(new CBItem { Text = "Klasse", Value = "Participant.Class" });
      comboBox.Items.Add(new CBItem { Text = "Gruppe", Value = "Participant.Group" });
      comboBox.Items.Add(new CBItem { Text = "Kategorie", Value = "Participant.Sex" });

      if (string.IsNullOrEmpty(selected))
        comboBox.SelectedIndex = 0;
      else
        comboBox.SelectCBItem(selected);
    }

  }



  #region Utilities

  public class ScrollToMeasuredItemBehavior
  {
    DataGrid _dataGrid;
    AppDataModel _dataModel;
    Participant _scrollToParticipant;

    System.Timers.Timer _timer;


    public ScrollToMeasuredItemBehavior(DataGrid dataGrid, AppDataModel dataModel)
    {
      _dataGrid = dataGrid;
      _dataModel = dataModel;
      _dataModel.ParticipantMeasuredEvent += OnParticipantMeasured;
      _timer = null;
    }


    void OnParticipantMeasured(object sender, Participant participant)
    {
      _scrollToParticipant = participant;

      _timer = new System.Timers.Timer(200);
      _timer.Elapsed += OnTimedEvent;
      _timer.AutoReset = false;
      _timer.Enabled = true;

    }

    void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        if (_dataGrid.ItemsSource != null)
        {
          foreach (var x in _dataGrid.ItemsSource)
          {
            Participant xp = null;
            xp = (x as RunResult)?.Participant.Participant;
            if (xp == null)
              xp = (x as RaceResultItem)?.Participant.Participant;

            if (xp == _scrollToParticipant)
            {
              _dataGrid.ScrollIntoView(x);
              break;
            }
          }
        }
      });
    }
  }

  #endregion

}
