/*
 *  Copyright (C) 2019 - 2024 by Sven Flossmann
 *  
 *  This file is part of Race Horology.
 *
 *  Race Horology is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  any later version.
 * 
 *  Race Horology is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with Race Horology.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  Diese Datei ist Teil von Race Horology.
 *
 *  Race Horology ist Freie Software: Sie können es unter den Bedingungen
 *  der GNU Affero General Public License, wie von der Free Software Foundation,
 *  Version 3 der Lizenz oder (nach Ihrer Wahl) jeder neueren
 *  veröffentlichten Version, weiter verteilen und/oder modifizieren.
 *
 *  Race Horology wird in der Hoffnung, dass es nützlich sein wird, aber
 *  OHNE JEDE GEWÄHRLEISTUNG, bereitgestellt; sogar ohne die implizite
 *  Gewährleistung der MARKTFÄHIGKEIT oder EIGNUNG FÜR EINEN BESTIMMTEN ZWECK.
 *  Siehe die GNU Affero General Public License für weitere Details.
 *
 *  Sie sollten eine Kopie der GNU Affero General Public License zusammen mit diesem
 *  Programm erhalten haben. Wenn nicht, siehe <https://www.gnu.org/licenses/>.
 * 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace RaceHorologyLib
{
  public class ResultCharts
  {
    protected string workaroundGermanUmlaut(string str)
    {
      return str
        .Replace("ä", "ae")
        .Replace("ö", "oe")
        .Replace("ü", "ue")
        .Replace("Ä", "Ae")
        .Replace("Ö", "Oe")
        .Replace("Ü", "Ue")
        .Replace("ß", "ss");
    }

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


    public void configureAxisX(Axis axis, RaceResultViewProvider results)
    {
      axis.Minimum = 0.5; // Double.NaN;
      axis.CustomLabels.Clear();
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
            var lblGroup = axis.CustomLabels.Add(x1 - 0.5, x1 + 0.5, workaroundGermanUmlaut(cvGroup.GetName()));
            lblGroup.GridTicks = GridTickTypes.None;

            // Second Level if possible
            if (cvGroup.Name is ParticipantClass @class && @class.Group != null)
            {
              name2 = @class.Group.Name;
              if (name2Last == null) // init set to same name
                name2Last = name2;

              if (!string.Equals(name2, name2Last))
              {
                var lblName2 = axis.CustomLabels.Add(x2Last - 0.5, (x2 - 1) + 0.5, workaroundGermanUmlaut(name2Last), 1, LabelMarkStyle.LineSideMark);
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
          var lblName2 = axis.CustomLabels.Add(x2Last - 0.5, (x2 - 1) + 0.5, workaroundGermanUmlaut(name2Last), 1, LabelMarkStyle.LineSideMark);
        }

        axis.Maximum = x2 - 0.5;
      }

      // Enable X axis labels automatic fitting
      axis.IsLabelAutoFit = true;
      axis.LabelAutoFitStyle = LabelAutoFitStyles.DecreaseFont | LabelAutoFitStyles.IncreaseFont | LabelAutoFitStyles.WordWrap;
      axis.MajorGrid.Enabled = false;
      axis.MinorGrid.Enabled = false;
    }


    public void configureAxisY(Axis axis, RaceResultViewProvider results)
    {
      double timeMax = double.NaN;
      double timeMin = double.NaN;
      foreach (var v in results.GetView())
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

      if (double.IsNaN(timeMin))
        timeMin = 0;
      if (double.IsNaN(timeMax))
        timeMax = 0;

      double yInt, yMinInt;
      yInt = yMinInt = 0;
      getNiceRoundNumbers(ref timeMin, ref timeMax, ref yInt, ref yMinInt);


      axis.Minimum = timeMin;
      axis.Maximum = timeMax;
      axis.Interval = yInt;
      axis.MinorTickMark.Interval = yMinInt;
      axis.MinorTickMark.TickMarkStyle = TickMarkStyle.OutsideArea;
      axis.MinorTickMark.Enabled = true;
      axis.MinorTickMark.Size = axis.MajorTickMark.Size / 2;

      axis.IsMarginVisible = false;
      axis.IsStartedFromZero = false;
      axis.Title = "Zeit [s]";
      axis.TitleFont = new System.Drawing.Font("Helvetica", 10);
      axis.LabelAutoFitStyle = LabelAutoFitStyles.None;
      //axis.LabelStyle.Format = "#.##";
      //axis.RoundAxisValues();
      //axis.IntervalType = DateTimeIntervalType.= IntervalAutoMode.

      axis.MajorGrid.LineDashStyle = ChartDashStyle.Solid;
      axis.MajorGrid.LineColor = System.Drawing.Color.Gray;
      axis.MajorGrid.Interval = yInt;

      axis.MinorGrid.Enabled = true;
      axis.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
      axis.MinorGrid.LineColor = System.Drawing.Color.LightGray;
      axis.MinorGrid.Interval = yMinInt;


      // Enable scale breaks
      axis.ScaleBreakStyle.Enabled = false;
      //axis.ScaleBreakStyle.BreakLineStyle = BreakLineStyle.Wave;
      //axis.ScaleBreakStyle.Spacing = 2;
      //axis.ScaleBreakStyle.LineWidth = 2;
      //axis.ScaleBreakStyle.LineColor = System.Drawing.Color.Red;
      //axis.ScaleBreakStyle.CollapsibleSpaceThreshold = 10;
      //// If all data points are significantly far from zero, the Chart will calculate the scale minimum value
      //axis.ScaleBreakStyle.StartFromZero = StartFromZero.Yes;
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
        p.Label = string.Format("{0} ({1}{2})", item.Participant.StartNumber, item.Participant.Name.Substring(0, 2), item.Participant.Firstname.Substring(0, 1));

        if (item.Participant.Sex?.Name == 'M')
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
        //area.AlignmentOrientation = AreaAlignmentOrientations.Horizontal;
        //area.Position.X = 5;
        //area.Position.Y = 5;
        //area.Position.Width = 80;
        //area.Position.Height = 90;

        //ChartArea areaBB = new ChartArea();
        //chart.ChartAreas.Add(areaBB);
        //areaBB.BorderColor = Color.FromArgb(0x3f, 0x43, 0x4b);
        //areaBB.BorderDashStyle = ChartDashStyle.Solid;
        //areaBB.BorderWidth = 1;
        //areaBB.AlignmentOrientation = AreaAlignmentOrientations.Horizontal;
        //areaBB.AlignmentStyle = AreaAlignmentStyles.AxesView;
        //areaBB.Position.X = 85;
        //areaBB.Position.Y = 5;
        //areaBB.Position.Width = 20;
        //areaBB.Position.Height = 90;
        //areaBB.AxisY.LabelStyle.Enabled = false;

        chart.Customize += Chart_Customize;
      }

      configureAxisX(chart.ChartAreas[0].AxisX, results);
      configureAxisY(chart.ChartAreas[0].AxisY, results);
      //configureAxisY(chart.ChartAreas[1].AxisY, results);
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
            System.Windows.Forms.DataVisualization.Charting.Series ds = new Series(cvGroup.GetName());

            if (cvGroup.Items.Count > 0)
            {
              formatDataSeries(ds);
              fillDataSeries(ds, cvGroup.Items, x);
            }
            x++;

            ds.ChartArea = chart.ChartAreas[0].Name;
            chart.Series.Add(ds);
            ds.Enabled = true;

            //Series dsBB = new Series("BoxPlot" + cvGroup.GetName());
            //dsBB.ChartType = SeriesChartType.BoxPlot;
            //dsBB["BoxPlotSeries"] = ds.Name;
            //dsBB["BoxPlotWhiskerPercentile"] = "5";
            //dsBB["BoxPlotPercentile"] = "30";
            //dsBB["BoxPlotShowAverage"] = "true";
            //dsBB["BoxPlotShowMedian"] = "true";
            //dsBB["BoxPlotShowUnusualValues"] = "true";
            ////dsBB["MaxPixelPointWidth"] = "15";
            //chart.Series.Add(dsBB);
          }
        }
      }
    }

    private void Chart_Customize(object sender, EventArgs e)
    {
      if (sender is Chart chart)
      {
        int x = 1;
        foreach( var s in chart.Series )
        {
          if (s.Name.Contains("BoxPlot"))
          {
            foreach (var p in s.Points)
            {
              p.XValue = x;
            }
            x++;
          }
        }
      }
    }

    void addBoxPLots(Chart chart)
    {

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
      ds.MarkerSize = 10;

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



  public class OfflineChart : ResultCharts
  {
    Chart _chart;

    public OfflineChart(int width, int height)
    {
      _chart = new Chart()
      {
        Width = width * 300 / 72 / 2,   // Constants figured out empirically 
        Height = height * 300 / 72 / 2, // Constants figured out empirically 
        AntiAliasing = AntiAliasingStyles.All,
        TextAntiAliasingQuality = TextAntiAliasingQuality.High
      };
    }


    public void RenderToWmf(Stream wmfStream, RaceResultViewProvider results)
    {

      SetupChart(_chart, results);

      //_chart.SaveImage(path, ChartImageFormat.Png);
  
      MemoryStream emfStream = new MemoryStream();
      _chart.SaveImage(emfStream, ChartImageFormat.Emf);

      emfStream.Seek(0, SeekOrigin.Begin);
      ConvertToWMF(emfStream, wmfStream);

      //_chart.SaveImage(wmfStream, System.Drawing.Imaging.ImageFormat.Wmf);
      //wmfStream.Seek(0, SeekOrigin.Begin);
      //_chart.SaveImage(@"c:\trash\test.wmf", System.Drawing.Imaging.ImageFormat.Wmf);
    }


    public void RenderToImage(Stream imgStream, RaceResultViewProvider results)
    {

      SetupChart(_chart, results);

      _chart.SaveImage(imgStream, ChartImageFormat.Png);
    }



    #region EMF to WMF (iText can only WMF)
    private enum EmfToWmfBitsFlags
    {
      EmfToWmfBitsFlagsDefault = 0x00000000,
      EmfToWmfBitsFlagsEmbedEmf = 0x00000001,
      EmfToWmfBitsFlagsIncludePlaceable = 0x00000002,
      EmfToWmfBitsFlagsNoXORClip = 0x00000004
    }

    [System.Runtime.InteropServices.DllImport("gdiplus.dll", SetLastError = true)]
    static extern int GdipEmfToWmfBits(int hEmf, int uBufferSize, byte[] bBuffer, int iMappingMode, EmfToWmfBitsFlags flags);
    
    void ConvertToWMF(Stream emfStream, Stream wmfStream)
    {
      const int MM_ANISOTROPIC = 8;
      System.Drawing.Imaging.Metafile mf = new System.Drawing.Imaging.Metafile(emfStream);
      System.Drawing.Imaging.Metafile mf2 = new System.Drawing.Imaging.Metafile(@"c:\trash\test.emf");

      int handle = mf.GetHenhmetafile().ToInt32();
      int handle2 = mf2.GetHenhmetafile().ToInt32();



      int bufferSize = GdipEmfToWmfBits(handle, 0, null, MM_ANISOTROPIC, EmfToWmfBitsFlags.EmfToWmfBitsFlagsIncludePlaceable);

      byte[] buf = new byte[bufferSize];
      GdipEmfToWmfBits(handle, bufferSize, buf, MM_ANISOTROPIC, EmfToWmfBitsFlags.EmfToWmfBitsFlagsIncludePlaceable);
      
      wmfStream.Write(buf, 0, bufferSize);
    }
    #endregion


  }
}
