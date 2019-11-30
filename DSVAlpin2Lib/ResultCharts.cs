using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace DSVAlpin2Lib
{
  public class ResultCharts
  {


    //Base10Exponent returns the integer exponent (N) that would yield a
    //number of the form A x Exp10(N), where 1.0 <= |A| < 10.0
    protected static int base10Exponent(double num)
    {
      if (num == 0)
        return -Int32.MaxValue;
      else
        return Convert.ToInt32(Math.Floor(Math.Log10(Math.Abs(num))));
    }

    double[] roundMantissa = { 1.00d, 1.20d, 1.40d, 1.60d, 1.80d, 2.00d, 2.50d, 3.00d, 4.00d, 5.00d, 6.00d, 8.00d, 10.00d };
    double[] roundInterval = { 0.20d, 0.20d, 0.20d, 0.20d, 0.20d, 0.50d, 0.50d, 0.50d, 0.50d, 1.00d, 1.00d, 2.00d, 2.00d };
    double[] roundIntMinor = { 0.05d, 0.05d, 0.05d, 0.05d, 0.05d, 0.10d, 0.10d, 0.10d, 0.10d, 0.20d, 0.20d, 0.50d, 0.50d };

    /// <summary>
    /// Gets nice round numbers for the axes. For the horizontal axis, minValue is always 0.
    /// </summary>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <param name="axisInterval"></param>
    protected void getNiceRoundNumbers(ref double minValue, ref double maxValue, ref double interval, ref double intMinor)
    {
      double min = Math.Min(minValue, maxValue);
      double max = Math.Max(minValue, maxValue);
      double delta = max - min; //The full range
                                //Special handling for zero full range
      if (delta == 0)
      {
        //When min == max == 0, choose arbitrary range of 0 - 1
        if (min == 0)
        {
          minValue = 0;
          maxValue = 1;
          interval = 0.2;
          intMinor = 0.5;
          return;
        }
        //min == max, but not zero, so set one to zero
        if (min < 0)
          max = 0; //min-max are -|min| to 0
        else
          min = 0; //min-max are 0 to +|max|
        delta = max - min;
      }

      int N = base10Exponent(delta);
      double tenToN = Math.Pow(10, N);
      double A = delta / tenToN;
      //At this point delta = A x Exp10(N), where
      // 1.0 <= A < 10.0 and N = integer exponent value
      //Now, based on A select a nice round interval and maximum value
      for (int i = 0; i < roundMantissa.Length; i++)
        if (A <= roundMantissa[i])
        {
          interval = roundInterval[i] * tenToN;
          intMinor = roundIntMinor[i] * tenToN;
          break;
        }
      minValue = interval * Math.Floor(min / interval);
      maxValue = interval * Math.Ceiling(max / interval);
    }


    public void configureAxis(ChartArea area, RaceResultViewProvider results)
    {
      area.AxisX.Minimum = 0.5; // Double.NaN;

      area.AxisX.CustomLabels.Clear();
      var lr = results.GetView() as System.Windows.Data.ListCollectionView;
      if (lr.Groups != null)
      {
        int x1 = 1;
        int x2Last = 1;
        int x2 = 1;
        string name2 = null, name2Last = null;
        foreach (var group in lr.Groups)
        {
          if (group is System.Windows.Data.CollectionViewGroup cvGroup)
          {
            var lblGroup = area.AxisX.CustomLabels.Add(x1 - 0.5, x1 + 0.5, cvGroup.Name.ToString());
            lblGroup.GridTicks = GridTickTypes.None;

            // Second Level if possible
            if (cvGroup.Name is ParticipantClass @class)
            {
              name2 = @class.Group.Name;
              if (name2Last == null) // init set to same name
                name2Last = name2;

              if (!string.Equals(name2, name2Last))
              {
                var lblName2 = area.AxisX.CustomLabels.Add(x2Last - 0.5, (x2 - 1) + 0.5, name2Last, 1, LabelMarkStyle.LineSideMark);
                name2Last = name2;
                x2Last = x2;
              }
            }
          }

          x1++;
          x2++;
        }

        // Final 2nd group
        if (name2 != null && name2Last != null)
        {
          var lblName2 = area.AxisX.CustomLabels.Add(x2Last - 0.5, (x2 - 1) + 0.5, name2Last, 1, LabelMarkStyle.LineSideMark);
        }

        area.AxisX.Maximum = x2 - 0.5;
      }


      double timeMax = double.NaN;
      double timeMin = double.NaN;
      foreach ( var v in results.GetView())
      {
        if (v is RaceResultItem result)
        {
          if (result.TotalTime != null)
          {
            double timeValue = ((TimeSpan)result.TotalTime).TotalMilliseconds / 1000.0;

            if (double.IsNaN(timeMax) || timeMax < timeValue)
              timeMax = timeValue;

            if (double.IsNaN(timeMin) || timeMin > timeValue)
              timeMin = timeValue;
          }
        }
      }

      // Enable X axis labels automatic fitting
      area.AxisX.IsLabelAutoFit = true;
      area.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.DecreaseFont | LabelAutoFitStyles.IncreaseFont | LabelAutoFitStyles.WordWrap;
      area.AxisX.MajorGrid.Enabled = false;
      area.AxisX.MinorGrid.Enabled = false;


      double yInt, yMinInt;
      yInt = yMinInt = 0;
      getNiceRoundNumbers(ref timeMin, ref timeMax, ref yInt, ref yMinInt);


      area.AxisY.Minimum = timeMin;
      area.AxisY.Maximum = timeMax;
      area.AxisY.Interval = yInt;
      area.AxisY.MinorTickMark.Interval = yMinInt;
      area.AxisY.MinorTickMark.TickMarkStyle = TickMarkStyle.OutsideArea;
      area.AxisY.MinorTickMark.Enabled = true;
      area.AxisY.MinorTickMark.Size = area.AxisY.MajorTickMark.Size / 2;
      
      area.AxisY.IsMarginVisible = false;
      area.AxisY.IsStartedFromZero = false;
      area.AxisY.Title = "Zeit [s]";
      area.AxisY.TitleFont = new System.Drawing.Font("Helvetica", 10);
      area.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.None;
      //area.AxisY.LabelStyle.Format = "#.##";
      //area.AxisY.RoundAxisValues();
      //area.AxisY.IntervalType = DateTimeIntervalType.= IntervalAutoMode.

      area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Solid;
      area.AxisY.MajorGrid.LineColor = System.Drawing.Color.Gray;
      //area.AxisY.MajorGrid.Interval = 2.0;

      area.AxisY.MinorGrid.Enabled = true;
      area.AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
      area.AxisY.MinorGrid.LineColor = System.Drawing.Color.LightGray;
      //area.AxisY.MinorGrid.Interval = 0.5;


      // Enable scale breaks
      area.AxisY.ScaleBreakStyle.Enabled = false;
      //area.AxisY.ScaleBreakStyle.BreakLineStyle = BreakLineStyle.Wave;
      //area.AxisY.ScaleBreakStyle.Spacing = 2;
      //area.AxisY.ScaleBreakStyle.LineWidth = 2;
      //area.AxisY.ScaleBreakStyle.LineColor = System.Drawing.Color.Red;
      //area.AxisY.ScaleBreakStyle.CollapsibleSpaceThreshold = 10;
      //// If all data points are significantly far from zero, the Chart will calculate the scale minimum value
      //area.AxisY.ScaleBreakStyle.StartFromZero = StartFromZero.Yes;
    }


    public void addResult(System.Windows.Forms.DataVisualization.Charting.Series ds, object result, int x)
    {
      if (!(result is RaceResultItem item))
        return;

      double timeValue = 0.0;
      if (item.TotalTime != null)
        timeValue = ((TimeSpan)item.TotalTime).TotalMilliseconds / 1000.0;

      if (timeValue > 0.0)
      {
        DataPoint p = new DataPoint(x, timeValue);
        //DataPoint p = new DataPoint(timeValue, x);
        //p.Label = item.Participant.Fullname;
        //p.Label = string.Format("{0} {1}.", item.Participant.Name, item.Participant.Firstname.Substring(0, 1));
        p.Label = string.Format("{0} ({1}{2})", item.Participant.StartNumber, item.Participant.Name.Substring(0, 1), item.Participant.Firstname.Substring(0, 1));

        if (item.Participant.Sex == "M")
        {
          p.MarkerColor = System.Drawing.Color.FromArgb(0x26, 0xb5, 0xd9); //93abc6, 26b5d9
          p.MarkerStyle = MarkerStyle.Circle;
        }
        else
        {
          p.MarkerColor = System.Drawing.Color.FromArgb(0xf0, 0x4a, 0xea); // c493c6, f04aea
          p.MarkerStyle = MarkerStyle.Diamond;
        }

        ds.Points.Add(p);
      }
    }


    protected void createAndAddChartArea(Chart chart, RaceResultViewProvider results)
    {
      if (chart.ChartAreas.Count() == 0)
      {
        ChartArea area = new ChartArea();
        chart.ChartAreas.Add(area);

        area.BorderColor = Color.FromArgb(0x3f, 0x43, 0x4b);
        area.BorderDashStyle = ChartDashStyle.Solid;
        area.BorderWidth = 1;
      }

      configureAxis(chart.ChartAreas[0], results);
    }


    protected void createDataSerieses(RaceResultViewProvider results, Chart chart)
    {
      // Populate series data with random data
      var lr = results.GetView() as System.Windows.Data.ListCollectionView;
      if (lr.Groups != null)
      {
        int x = 1;
        foreach (var group in lr.Groups)
        {
          if (group is System.Windows.Data.CollectionViewGroup cvGroup)
          {
            System.Windows.Forms.DataVisualization.Charting.Series ds = new Series(cvGroup.Name.ToString());

            if (cvGroup.Items.Count > 0)
            {
              formatDataSeries(ds);
              fillDataSeries(ds, cvGroup.Items, x);
            }
            x++;

            chart.Series.Add(ds);
          }
        }
      }


    }



    protected void fillDataSeries(System.Windows.Forms.DataVisualization.Charting.Series ds, System.Collections.IEnumerable items, int xValue)
    {
      foreach (var result in items)
        addResult(ds, result, xValue);
    }


    protected void formatDataSeries(System.Windows.Forms.DataVisualization.Charting.Series ds)
    {
      // Set point chart type
      ds.ChartType = SeriesChartType.Point;

      // Enable data points labels
      ds.IsValueShownAsLabel = true;
      ds["LabelStyle"] = "Bottom";

      ds.SmartLabelStyle.Enabled = true;
      ds.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Partial;
      ds.SmartLabelStyle.CalloutLineAnchorCapStyle = LineAnchorCapStyle.Arrow;
      ds.SmartLabelStyle.CalloutLineColor = System.Drawing.Color.Red;
      ds.SmartLabelStyle.CalloutLineWidth = 1;
      ds.SmartLabelStyle.CalloutStyle = LabelCalloutStyle.Underlined;

      // Set marker size
      ds.MarkerSize = 15;

      // Set marker shape
      ds.MarkerStyle = MarkerStyle.Circle;
    }


    public void SetupChart(Chart chart, RaceResultViewProvider results)
    {
      createAndAddChartArea(chart, results);

      // Setup Data Series
      chart.Series.Clear();
      createDataSerieses(results, chart);

      //chart.SaveImage("", ChartImageFormat.)
    }
  }



  //public class OfflineChart
  //{
  //  Chart _chart;

  //  public OfflineChart()
  //  {
  //    _chart = new Chart
  //    {
  //      Width = 300,
  //      Height = 450,
  //      RenderType = RenderType.ImageTag,
  //      AntiAliasing = AntiAliasingStyles.All,
  //      TextAntiAliasingQuality = TextAntiAliasingQuality.High
  //    };

  //    _chart.


  //  }


  //  public void RenderToFile(string path)
  //  {

  //  }


  //}
}
