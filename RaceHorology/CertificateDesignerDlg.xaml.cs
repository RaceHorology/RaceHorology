using RaceHorologyLib;
using System;
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

      ucCertDesigner.Finished += UcCertDesigner_Finished;
    }


    private void UcCertDesigner_Finished(object sender, EventArgs e)
    {
      this.Close();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      ucCertDesigner.SaveOrResetNow();
    }
  }
}
