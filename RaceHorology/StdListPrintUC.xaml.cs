using RaceHorologyLib;
using System.Windows.Controls;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for StdListPrintUC.xaml
  /// </summary>
  public partial class StdListPrintUC : UserControl, IReportSubUC
  {
    public StdListPrintUC(bool showDiagram)
    {
      InitializeComponent();
      cbWithDiagram.Visibility = showDiagram ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    public void Apply(IPDFReport reportGenerator)
    {
      if (reportGenerator is PDFRaceReport raceReport)
      {
        raceReport.OneGroupPerPage = cbOneGroupPerPage.IsChecked == true;
      }
      if (reportGenerator is PDFReport raceReport2)
      {
        raceReport2.WithDiagram = cbWithDiagram.IsChecked == true;
        raceReport2.WithRaceHeader = cbWithRaceHeader.IsChecked == true;
      }
    }

    public void HandleReportGenerator(IPDFReport reportGenerator)
    {
      if (reportGenerator is PDFRaceReport raceReport)
      {
        cbOneGroupPerPage.IsChecked = raceReport.OneGroupPerPage;
      }
      if (reportGenerator is PDFReport raceReport2)
      {
        cbWithDiagram.IsChecked = raceReport2.WithDiagram;
        cbWithRaceHeader.IsChecked = raceReport2.WithRaceHeader;
      }
    }
  }
}
