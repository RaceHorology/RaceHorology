using RaceHorologyLib;
using System.Windows;
using System.Windows.Controls;

namespace RaceHorology
{
  /// <summary>
  /// Interaktionslogik für RefereeReportUC.xaml
  /// </summary>
  public partial class RefereeReportUC : UserControl
  {
    public RefereeReportItems ReportItems { get; set; }
    private Race _race;

    public RefereeReportUC()
    {
      InitializeComponent();
    }

    public void Init(Race race)
    {
      _race = race;
      ReportItems = new RefereeReportItems(_race);
      this.DataContext = ReportItems;
    }

    private void btnApply_Click(object sender, RoutedEventArgs e)
    {
      ReportItems.updateList(_race);
    }

    private void btnReset_Click(object sender, RoutedEventArgs e)
    {
    }
  }
}
