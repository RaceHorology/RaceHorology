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
  /// Interaction logic for MeasurementLogAndParticipantAssignment.xaml
  /// </summary>
  public partial class MeasurementLogAndParticipantAssignment : UserControl
  {
    private Race _race;
    private LiveTimeParticipantAssigning _tdAssigning;

    public MeasurementLogAndParticipantAssignment()
    {
      InitializeComponent();
    }

    public void Init(LiveTimeParticipantAssigning tdAssigning, Race race)
    {
      _race = race;
      _tdAssigning = tdAssigning;
      dgParticipantAssigning.ItemsSource = tdAssigning.Timestamps;
    }

    private void TxtStartNumber_TextChanged(object sender, TextChangedEventArgs e)
    {
      uint startNumber = 0U;
      try { startNumber = uint.Parse(txtStartNumber.Text); } catch (Exception) { }
      RaceParticipant participant = _race.GetParticipant(startNumber);
      if (participant != null)
      {
        txtParticipant.Text = participant.Fullname;
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
      try 
      { 
        uint startNumber = uint.Parse(txtStartNumber.Text);
        var ts = dgParticipantAssigning.SelectedItem as Timestamp;

        if (ts != null)
          _tdAssigning.Assign(ts, startNumber);
      } 
      catch (Exception) { }
    }


  }
}
