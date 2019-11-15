using DSVAlpin2Lib;
using LiveCharts;
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


      var lr = results.GetView() as System.Windows.Data.ListCollectionView;
      if (lr.Groups != null)
      {
        foreach (var group in lr.Groups)
        {
          System.Windows.Data.CollectionViewGroup cvGroup = group as System.Windows.Data.CollectionViewGroup;
          //addLineToTable(table, cvGroup.Name.ToString());


          StackedColumnSeries series = new StackedColumnSeries
          {
            Values = new ChartValues<double> { },
            StackMode = StackMode.Values, // this is not necessary, values is the default stack mode
            DataLabels = true
          };

          int i = 0;
          foreach (var result in cvGroup.Items)
          {
            RaceResultItem item = result as RaceResultItem;
            if (item == null)
              return;

            double timeValue = 0.0;
            if (item.TotalTime != null)
              timeValue = ((TimeSpan)item.TotalTime).TotalMilliseconds / 1000.0;

            series.Values.Add(timeValue);
            //addLineToTable(table, result, i++);
          }

          seriesCollection.Add(series);
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

      DataContext = this;
    }


    public SeriesCollection SeriesCollection { get; set; }
    public string[] Labels { get; set; }
    public Func<double, string> Formatter { get; set; }
  }
}
