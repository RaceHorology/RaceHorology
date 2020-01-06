using RaceHorologyLib;
using System.Windows.Controls;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for ChartTestsUC.xaml
  /// </summary>
  public partial class ChartTestsUC : UserControl
  {
    private AppDataModel _dm;
    private Race _race;


    public ChartTestsUC()
    {
      InitializeComponent();
    }


    public void Init(AppDataModel dm, Race race)
    {
      _dm = dm;
      _race = race;


      InitializeComponent();
    }


    public void Display(RaceResultViewProvider results)
    {
      ResultCharts helper = new ResultCharts();

      helper.SetupChart(msChart.GetChart(), results);
    }



  }
}
