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
using static DSVAlpin2Lib.RunResult;

namespace DSVAlpin2
{
  /// <summary>
  /// Interaction logic for DisqualifyUC.xaml
  /// </summary>
  public partial class DisqualifyUC : UserControl
  {
    private AppDataModel _dm;
    private Race _race;
    private RaceRun _currentRaceRun;

    CollectionViewSource _viewDisqualifications;
    FilterEventHandler _viewDisqualificationsFilterHandler;

    public List<EResultCode> ListOfResultCodes { get; } = new List<EResultCode> { EResultCode.Normal, EResultCode.DIS, EResultCode.NaS, EResultCode.NiZ, EResultCode.NQ};

    public DisqualifyUC()
    {
      InitializeComponent();
    }

    public void Init(AppDataModel dm, Race race)
    {
      _dm = dm;
      _race = race;

      UiUtilities.FillCmbRaceRun(cmbRaceRun, _race);
      UiUtilities.FillGrouping(cmbResultGrouping, _currentRaceRun.GetResultViewProvider().ActiveGrouping);

      cmbFilter.Items.Clear();
      cmbFilter.Items.Add(new CBItem { Text = "alle", Value = "all" });
      cmbFilter.Items.Add(new CBItem { Text = "ohne Zeit", Value = "no_time"});
      cmbFilter.Items.Add(new CBItem { Text = "ausgeschieden", Value = "out" });
      cmbFilter.SelectedIndex = 1;

      cmbDisqualify.ItemsSource = ListOfResultCodes;

      this.KeyDown += new KeyEventHandler(Timing_KeyDown);
    }


    private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (_viewDisqualificationsFilterHandler != null)
        _viewDisqualifications.Filter -= _viewDisqualificationsFilterHandler;

      if (cmbFilter.SelectedItem is CBItem selected)
      {
        _viewDisqualificationsFilterHandler = null;
        if (string.Equals(selected.Value, "no_time"))
          _viewDisqualificationsFilterHandler = new FilterEventHandler(delegate (object s, FilterEventArgs ea) { ea.Accepted = ((RunResult)ea.Item).RuntimeOrig == null; });
        else if (string.Equals(selected.Value, "out"))
          _viewDisqualificationsFilterHandler = new FilterEventHandler(delegate (object s, FilterEventArgs ea) { ea.Accepted = ((RunResult)ea.Item).ResultCode != RunResult.EResultCode.Normal; });
      }

      if (_viewDisqualificationsFilterHandler != null)
        _viewDisqualifications.Filter += _viewDisqualificationsFilterHandler;

      _viewDisqualifications.View.Refresh();
    }


    private void CmbRaceRun_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      CBItem selected = (sender as ComboBox).SelectedValue as CBItem;
      RaceRun selectedRaceRun = selected?.Value as RaceRun;

      _currentRaceRun = selectedRaceRun;

      ConnectUiToRaceRun(_currentRaceRun);
    }


    private void ConnectUiToRaceRun(RaceRun raceRun)
    {
      if (raceRun != null)
      {
        _viewDisqualifications = new CollectionViewSource();
        _viewDisqualifications.Source = raceRun.GetResultList();
        _viewDisqualifications.LiveFilteringProperties.Add(nameof(RunResult.Runtime));
        _viewDisqualifications.LiveFilteringProperties.Add(nameof(RunResult.ResultCode));
        _viewDisqualifications.IsLiveFilteringRequested = true;


        dgDisqualifications.ItemsSource = _viewDisqualifications.View;

        dgResults.ItemsSource = raceRun.GetResultViewProvider().GetView();
        cmbResultGrouping.SelectCBItem(raceRun.GetResultViewProvider().ActiveGrouping);
      }
      else
      {
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
          txtDisqualify.Text = rr.DisqualText;
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

      if (participant != null)
      {
        RunResult rr = _currentRaceRun.GetResultList().FirstOrDefault(r => r.Participant == participant);
        if (rr != null)
        {
          rr.DisqualText = txtDisqualify.Text;
          rr.ResultCode = (EResultCode)cmbDisqualify.SelectedValue;
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
      if (dgDisqualifications.SelectedItem is RunResult rr)
      {
        txtStartNumber.Text = rr.StartNumber.ToString();
      }
    }
  }
}
