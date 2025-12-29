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
    public CertificateDesignerImportDlg(AppDataModel importModel)
    {
      _importModel = importModel;
      InitializeComponent();
    }
    protected void Init()
    {
      var races = _importModel.GetRaces();
      foreach (var race in races)
      {
        var cm = _importModel.GetDB().GetCertificateModel(race);
        if (cm.TextItems.Count > 0)
        {

        }
      }
    }

    private void btnImport_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}

