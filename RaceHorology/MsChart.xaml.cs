using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Specialized;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for MsChart.xaml
  /// </summary>
  public partial class MsChart : UserControl
  {
    public MsChart()
    {
      InitializeComponent();
      //SeriesCollection = new BindableCollection<Series>();
    }


    public Chart GetChart() { return myChart; }

    }
}
