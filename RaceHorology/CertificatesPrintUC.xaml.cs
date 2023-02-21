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
