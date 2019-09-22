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
    Race _currentRace;
    LiveTimingMeasurement _liveTimingMeasurement;

    // Working Data
    RaceRun _currentRaceRun;

    RemainingStartListViewProvider _rslVP;

    ScrollToMeasuredItemBehavior dgResultsScrollBehavior;
    ScrollToMeasuredItemBehavior dgTotalResultsScrollBehavior;


    public RaceUC(AppDataModel dm, Race race, LiveTimingMeasurement liveTimingMeasurement)
    {
      _dataModel = dm;
      _currentRace = race;
      _liveTimingMeasurement = liveTimingMeasurement;

      _liveTimingMeasurement.LiveTimingMeasurementStatusChanged += OnLiveTimingMeasurementStatusChanged;

      InitializeComponent();

      FillCmbRaceRun();

      // Configuration Screen
      FillGrouping(cmbConfigStartlist1Grouping);
      FillGrouping(cmbConfigStartlist2Grouping);

      // Timing
      FillGrouping(cmbStartListGrouping);
      FillGrouping(cmbResultGrouping);

      // Race Results
      FillGrouping(cmbTotalResultGrouping);

      dgTotalResults.ItemsSource = _currentRace.GetTotalResultView();
      dgTotalResultsScrollBehavior = new ScrollToMeasuredItemBehavior(dgTotalResults, _dataModel);
    }


    public Race GetRace() { return _currentRace; }
    public RaceRun GetRaceRun() { return _currentRaceRun; }


    private void FillCmbRaceRun()
    {
      // Fill Runs
      List<KeyValuePair<RaceRun, string>> races = new List<KeyValuePair<RaceRun, string>>();
      for (int i = 0; i < _currentRace.GetMaxRun(); i++)
      {
        string sz1 = String.Format("{0}. Durchgang", i + 1);
        races.Add(new KeyValuePair<RaceRun, string>(_currentRace.GetRun(i), sz1));
      }
      cmbRaceRun.ItemsSource = races;
      cmbRaceRun.DisplayMemberPath = "Value";
      cmbRaceRun.SelectedValuePath = "Key";

      cmbRaceRun.SelectedIndex = 0;
    }

    private void CmbRaceRun_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      _currentRaceRun = (sender as ComboBox).SelectedValue as RaceRun;
      if (_currentRaceRun != null)
      {
        _dataModel.SetCurrentRaceRun(_currentRaceRun);

        dgStartList.ItemsSource = _currentRace.GetParticipants();

        _rslVP = new RemainingStartListViewProvider();
        _rslVP.Init(_currentRaceRun.GetStartListProvider(), _currentRaceRun);
        dgRemainingStarters.ItemsSource = _rslVP.GetView();

        dgRemainingStartersSrc.ItemsSource = _currentRaceRun.GetStartListProvider().GetView();

        dgRunning.ItemsSource = _currentRaceRun.GetOnTrackList();
        dgResults.ItemsSource = _currentRaceRun.GetResultView();
        dgResultsScrollBehavior = new ScrollToMeasuredItemBehavior(dgResults, _dataModel);
      }
      else
      {
        dgStartList.ItemsSource = null;
        dgRemainingStarters.ItemsSource = null;
        dgRemainingStartersSrc.ItemsSource = null;
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

      System.Diagnostics.Debug.Assert(cmbRaceRun.SelectedValue == _currentRaceRun);
    }



    private void BtnManualTimeStore_Click(object sender, RoutedEventArgs e)
    {
      TimeSpan? start = null, finish = null, run = null;

      try { start = TimeSpan.Parse(txtStart.Text); } catch (Exception) { }
      try { finish = TimeSpan.Parse(txtFinish.Text); } catch (Exception) { }
      try { run = TimeSpan.Parse(txtRun.Text); } catch (Exception) { }

      uint startNumber = 0U;
      try { startNumber = uint.Parse(txtStartNumber.Text); } catch (Exception) { }
      RaceParticipant participant = _currentRace.GetParticipant(startNumber);

      if (participant != null)
      {
        if (start != null || finish != null)
          _currentRaceRun.SetStartFinishTime(participant, start, finish);
        else if (run != null)
          _currentRaceRun.SetRunTime(participant, run);
      }
    }

    private void BtnManualTimeFinish_Click(object sender, RoutedEventArgs e)
    {
      TimeSpan finish = DateTime.Now - DateTime.Today;
      txtFinish.Text = finish.ToString();
      UpdateRunTime();
    }

    private void BtnManualTimeStart_Click(object sender, RoutedEventArgs e)
    {
      TimeSpan start = DateTime.Now - DateTime.Today;
      txtStart.Text = start.ToString();
      UpdateRunTime();
    }

    private void UpdateRunTime()
    {
      try
      {
        TimeSpan start = TimeSpan.Parse(txtStart.Text);
        TimeSpan finish = TimeSpan.Parse(txtFinish.Text);
        TimeSpan run = finish - start;
        txtRun.Text = run.ToString(@"mm\:ss\,ff");
      }
      catch (Exception)
      { }

    }

    private void DgStartList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void DgRemainingStarters_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      RaceParticipant participant = (dgRemainingStarters.SelectedItem as StartListEntry)?.Participant;

      if (participant != null)
      {
        RunResult result = _currentRaceRun.GetResultList().SingleOrDefault(r => r._participant == participant);

        if (result != null)
        {
          txtStart.Text = result.GetStartTime()?.ToString();
          txtFinish.Text = result.GetFinishTime()?.ToString();
          txtRun.Text = result.GetRunTime()?.ToString();
        }
        else
        {
          txtStart.Text = txtFinish.Text = txtRun.Text = "";
        }

        txtStartNumber.Text = participant.StartNumber.ToString();
      }
    }

    public class GroupingCBItem
    {
      public string Text { get; set; }
      public string Value { get; set; }

      public override string ToString()
      {
        return Text;
      }
    }

    private void CmbTotalResultGrouping_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbTotalResultGrouping.SelectedValue is GroupingCBItem grouping)
        _currentRace.GetResultViewProvider().ChangeGrouping(grouping.Value);
    }
    private void CmbStartListGrouping_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbStartListGrouping.SelectedValue is GroupingCBItem grouping)
        _rslVP.ChangeGrouping(grouping.Value);
    }
    private void CmbResultGrouping_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbResultGrouping.SelectedValue is GroupingCBItem grouping)
        _currentRaceRun.GetResultViewProvider().ChangeGrouping(grouping.Value);
    }


    public static void FillGrouping(ComboBox comboBox)
    {
      comboBox.Items.Add(new GroupingCBItem { Text = "---", Value = null });
      comboBox.Items.Add(new GroupingCBItem { Text = "Klasse", Value = "Participant.Class" });
      comboBox.Items.Add(new GroupingCBItem { Text = "Gruppe", Value = "Participant.Group" });
      comboBox.Items.Add(new GroupingCBItem { Text = "Kategorie", Value = "Participant.Sex" });
      comboBox.SelectedIndex = 0;
    }
  }

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


}
