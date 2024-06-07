using RaceHorologyLib;
using System;
using System.Windows.Controls;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for CertificatesPrintUC.xaml
  /// </summary>
  public partial class CertificatesPrintUC : UserControl, IReportSubUC
  {
    public CertificatesPrintUC()
    {
      InitializeComponent();
    }

    public void HandleReportGenerator(IPDFReport reportGenerator)
    {
      var certificates = reportGenerator as Certificates;
      txtNumberCertificatesPerGrouping.Text = certificates.MaxCertificatesPerGroup.ToString();
    }

    public void Apply(IPDFReport reportGenerator)
    {
      var certificates = reportGenerator as Certificates;
      if (certificates != null)
      {
        try
        {
          certificates.MaxCertificatesPerGroup = int.Parse(txtNumberCertificatesPerGrouping.Text);
        }
        catch (Exception)
        {
        }
      }
    }
  }
}
