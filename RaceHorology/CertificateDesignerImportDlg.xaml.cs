using RaceHorologyLib;
using System.Windows;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for CertificateDesignerImportDlg.xaml
  /// </summary>
  public partial class CertificateDesignerImportDlg : Window
  {
    AppDataModel _importModel;
    public Race SelectedRace;
    public CertificateDesignerImportDlg(AppDataModel importModel)
    {
      _importModel = importModel;
      InitializeComponent();
      FillCombo();
    }
    protected void FillCombo()
    {
      var races = _importModel.GetRaces();
      foreach (var race in races)
      {
        var cm = _importModel.GetDB().GetCertificateModel(race);
        if (cm.TextItems.Count > 0)
        {
          cbRace.Items.Add(new CBItem { Text = race.RaceType.ToString(), Value = race });
        }
      }
    }

    private void btnImport_Click(object sender, RoutedEventArgs e)
    {
      if (cbRace.SelectedValue != null)
      {
        CBItem selected = cbRace.SelectedValue as CBItem;
        SelectedRace = selected.Value as Race;
      }
      DialogResult = true;
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
    }
  }
}

