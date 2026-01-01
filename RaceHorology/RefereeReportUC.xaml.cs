using RaceHorologyLib;
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
      ucSaveOrReset.Init("SR Bericht", null, null, null, storeData, resetData);

      _race = race;
      resetData();
    }

    private void storeData()
    {
      _race.GetDataModel().GetDB().SaveRefereeReport(_race, ReportItems);
    }
    private void resetData()
    {
      ReportItems = _race.GetDataModel().GetDB().GetRefereeReport(_race);
      this.DataContext = ReportItems;
    }
  }
}
