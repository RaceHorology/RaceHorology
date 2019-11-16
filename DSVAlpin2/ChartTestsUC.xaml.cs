using DSVAlpin2Lib;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace DSVAlpin2
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
      SeriesCollection seriesCollection = new SeriesCollection();

      List<string> labels = new List<string>();
      var lr = results.GetView() as System.Windows.Data.ListCollectionView;
      if (lr.Groups != null)
      {
        int x = 1;
        foreach (var group in lr.Groups)
        {
          System.Windows.Data.CollectionViewGroup cvGroup = group as System.Windows.Data.CollectionViewGroup;

          ScatterSeries series = new ScatterSeries();
          //series.DataLabels = true;
          series.Title = cvGroup.Name.ToString();
          //labels.Add(cvGroup.Name.ToString());
          series.PointGeometry = DefaultGeometries.Circle;

          var values = new ChartValues<ObservablePoint>();
          series.Values = values;

          int i = 0;
          foreach (var result in cvGroup.Items)
          {
            RaceResultItem item = result as RaceResultItem;
            if (item == null)
              return;

            double timeValue = 0.0;
            if (item.TotalTime != null)
              timeValue = ((TimeSpan)item.TotalTime).TotalMilliseconds / 1000.0;

            if (timeValue > 30.0)
            {
              var p = new ObservablePoint(x, timeValue);
              values.Add(p);
            }

            seriesCollection.Add(series);
          }

          x++;
        }
      }
      else
      {
        int i = 0;
        foreach (var result in lr.SourceCollection)
        {
          //addLineToTable(table, result, i++);
        }
      }

      SeriesCollection = seriesCollection;
      Labels = labels.ToArray();
      DataContext = this;

      lvChart.Update(true, true);
    }


    public SeriesCollection SeriesCollection { get; set; }
    public string[] Labels { get; set; }
    public Func<double, string> Formatter { get; set; }
  }
}
