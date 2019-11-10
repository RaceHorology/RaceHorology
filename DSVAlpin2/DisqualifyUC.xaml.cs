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
  /// Interaction logic for DisqualifyUC.xaml
  /// </summary>
  public partial class DisqualifyUC : UserControl
  {
    private AppDataModel _dm;
    private Race _race;
    private RaceRun _currentRaceRun;

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
        dgDisqualifications.ItemsSource = raceRun.GetResultList();

        dgResults.ItemsSource = raceRun.GetResultViewProvider().GetView();
        cmbResultGrouping.SelectCBItem(raceRun.GetResultViewProvider().ActiveGrouping);
      }
      else
      {
        dgResults.ItemsSource = null;
      }
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
      }

      txtStartNumber.Focus();
    }

    private void CmbResultGrouping_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbResultGrouping.SelectedValue is CBItem grouping)
        _currentRaceRun.GetResultViewProvider().ChangeGrouping((string)grouping.Value);
    }
  }
}
