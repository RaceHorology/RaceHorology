using RaceHorologyLib;
using System.Windows.Controls;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for StdListPrintUC.xaml
  /// </summary>
  public partial class StdListPrintUC : UserControl, IReportSubUC
  {
    public StdListPrintUC()
    {
      InitializeComponent();
    }

    public void Apply(IPDFReport reportGenerator)
    {
      if (reportGenerator is PDFRaceReport raceReport)
      {
        raceReport.OneGroupPerPage = cbOneGroupPerPage.IsChecked == true;
      }
    }

    public void HandleReportGenerator(IPDFReport reportGenerator)
    {
      if (reportGenerator is PDFRaceReport raceReport)
      {
        cbOneGroupPerPage.IsChecked = raceReport.OneGroupPerPage;
      }
    }
  }
}
