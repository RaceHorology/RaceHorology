using RaceHorologyLib;
using System.ComponentModel;
using System.Windows;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for CertificateDesignerDlg.xaml
  /// </summary>
  public partial class CertificateDesignerDlg : Window
  {
    public CertificateDesignerDlg()
    {
      InitializeComponent();
    }

    public void Init(AppDataModel dm, Race race)
    {
      ucCertDesigner.Init(dm, race);
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      ucCertDesigner.SaveOrResetNow();
    }
  }
}
