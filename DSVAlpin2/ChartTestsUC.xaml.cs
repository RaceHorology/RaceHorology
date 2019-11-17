using DSVAlpin2Lib;
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
using System.Windows.Forms.DataVisualization.Charting;
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
      DisplayMSCHart(results);
    }


    public void DisplayMSCHart(RaceResultViewProvider results)
    {

      msChart.GetChart().Series.Clear();

      if (msChart.GetChart().ChartAreas.Count() == 0)
      {
        ChartArea area = new ChartArea();
        area.AxisX.Minimum = Double.NaN;
        area.AxisX.Maximum = Double.NaN;

        var element = area.AxisX.CustomLabels.Add(0.5, 1.5, "ABC");
        element.GridTicks = GridTickTypes.All;
        element = area.AxisX.CustomLabels.Add(1.5, 2.5, "DEF");
        element.GridTicks = GridTickTypes.TickMark;

        element = area.AxisX.CustomLabels.Add(2.5, 3.5, "GEH");
        element.GridTicks = GridTickTypes.TickMark;
        element = area.AxisX.CustomLabels.Add(3.5, 4.5, "JKL");
        element.GridTicks = GridTickTypes.TickMark;

        element = area.AxisX.CustomLabels.Add(0.5, 2.5, "G1", 1, LabelMarkStyle.LineSideMark);
        element = area.AxisX.CustomLabels.Add(2.5, 4.5, "G2", 1, LabelMarkStyle.LineSideMark);

        area.AxisY.Minimum = Double.NaN;
        area.AxisY.Maximum = Double.NaN;
        area.AxisY.IsMarginVisible = false;
        area.AxisY.IsStartedFromZero = true;
        area.AxisY.Title = "Zeit";


        // Enable scale breaks
        area.AxisY.ScaleBreakStyle.Enabled = true;

        // Set the scale break type
        area.AxisY.ScaleBreakStyle.BreakLineStyle = BreakLineStyle.Wave;

        // Set the spacing gap between the lines of the scale break (as a percentage of y-axis)
        area.AxisY.ScaleBreakStyle.Spacing = 2;

        // Set the line width of the scale break
        area.AxisY.ScaleBreakStyle.LineWidth = 2;

        // Set the color of the scale break
        area.AxisY.ScaleBreakStyle.LineColor = System.Drawing.Color.Red;

        // Show scale break if more than 25% of the chart is empty space
        area.AxisY.ScaleBreakStyle.CollapsibleSpaceThreshold = 25;

        // If all data points are significantly far from zero, 
        // the Chart will calculate the scale minimum value
        //area.AxisY.ScaleBreakStyle.IsStartedFromZero = AutoBool.Auto;

        msChart.GetChart().ChartAreas.Add(area);

        //msChart.GetChart().SaveImage("", ChartImageFormat.)
      }

      var ds = new System.Windows.Forms.DataVisualization.Charting.Series();

      // Populate series data with random data
      var lr = results.GetView() as System.Windows.Data.ListCollectionView;
      if (lr.Groups != null)
      {
        int x = 1;
        foreach (var group in lr.Groups)
        {
          System.Windows.Data.CollectionViewGroup cvGroup = group as System.Windows.Data.CollectionViewGroup;

          foreach (var result in cvGroup.Items)
          {
            if (!(result is RaceResultItem item))
              return;

            double timeValue = 0.0;
            if (item.TotalTime != null)
              timeValue = ((TimeSpan)item.TotalTime).TotalMilliseconds / 1000.0;

            if (timeValue > 0.0)
            {
              DataPoint p = new DataPoint(x, timeValue);
              p.Label = item.Participant.Fullname;

              ds.Points.Add(p);
              //ds.Points.AddXY(x, timeValue);
            }
          }

          x++;
        }
      }

      // Set point chart type
      ds.ChartType = SeriesChartType.Point;

      // Enable data points labels
      ds.IsValueShownAsLabel = true;
      ds["LabelStyle"] = "Center";

      // Set marker size
      ds.MarkerSize = 15;

      // Set marker shape
      ds.MarkerStyle = MarkerStyle.Diamond;

      // Set to 3D
      //msChart.GetChart().ChartAreas[strChartArea].Area3DStyle.Enable3D = true;

      msChart.GetChart().Series.Add(ds);
      //MsSeriesCollection.Add(ds);
    }
  }
}
