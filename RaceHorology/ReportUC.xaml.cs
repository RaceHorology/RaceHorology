using CefSharp;
using CefSharp.Wpf;
using RaceHorology.Properties;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RaceHorology
{
  internal class ReportItem
  {
    public string Text;
    public bool NeedsRaceRun = false;
    public Func<Race, RaceRun, IPDFReport> CreateReport;

    public override string ToString()
    {
      return Text;
    }
  }


  /// <summary>
  /// Interaction logic for ReportUC.xaml
  /// </summary>
  public partial class ReportUC : UserControl
  {
    private Race _race;
    private DelayedEventHandler _refreshDelay;
    IWarningLabelHandler _lblHandler;


    public ReportUC()
    {
      InitializeComponent();

      _refreshDelay = new DelayedEventHandler(300, refreshTimout);

      IsVisibleChanged += ReportUC_IsVisibleChanged;
    }

    private void ReportUC_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(bool)e.OldValue && (bool)e.NewValue)
      {
        UiUtilities.FillCmbRaceRun(cmbRaceRun, _race);
        cmbRaceRun.SelectCBItem(_race.GetDataModel().GetCurrentRaceRun());

        triggerRefresh();
      }
    }

    public void Init(Race race)
    {
      _race = race;

      List<ReportItem> items = new List<ReportItem>();
      items.Add(new ReportItem { Text = "Ergebnisliste", NeedsRaceRun = false, CreateReport = (r, rr) => { return (r.GetResultViewProvider() is DSVSchoolRaceResultViewProvider) ? new DSVSchoolRaceResultReport(r) : new RaceResultReport(r); } });
      items.Add(new ReportItem { Text = "Teilergebnisliste", NeedsRaceRun = true, CreateReport = (r, rr) => { return new RaceRunResultReport(rr); } });
      items.Add(new ReportItem { Text = "Startliste", NeedsRaceRun = true, CreateReport = (r, rr) => { return (rr.Run == 1) ? (IPDFReport)new StartListReport(rr) : (IPDFReport)new StartListReport2ndRun(rr); } });
      items.Add(new ReportItem { Text = "Schiedsrichter Protokoll", NeedsRaceRun = true, CreateReport = (r, rr) => { return new RefereeProtocol(rr); } });

      cmbReport.ItemsSource = items;
      cmbReport.SelectedIndex = 0;

      UiUtilities.FillCmbRaceRun(cmbRaceRun, _race);

      _lblHandler = new RaceCompletedWarningLabelHandler(_race, lblWarning);

    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
      GenerateReportPreview();
    }

    private void btnPrint_Click(object sender, RoutedEventArgs e)
    {
      cefBrowser.PrintCommand.Execute(null);
    }

    private void btnRefresh_Click(object sender, RoutedEventArgs e)
    {
      refreshPdf();
    }

    private void cmbReport_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbReport.SelectedItem is ReportItem ri)
      {
        cmbRaceRun.IsEnabled = ri.NeedsRaceRun;

        triggerRefresh();
      }
    }

    private void cmbRaceRun_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      CBItem selected = (sender as ComboBox).SelectedValue as CBItem;
      RaceRun selectedRaceRun = selected?.Value as RaceRun;

      triggerRefresh();
    }


    private void GenerateReportPreview()
    {
      RaceListsUC.CreateAndOpenReport(getSelectedReport());
    }


    private void triggerRefresh()
    {
      if (IsVisible)
        _refreshDelay.Delayed(null, null);
    }

    private void refreshPdf()
    {
      var reportGenerator = getSelectedReport();
      if (reportGenerator != null)
      {
        MemoryStream ms = new MemoryStream();
        reportGenerator.Generate(ms);

        var settings = new CefSettings();
        settings.CefCommandLineArgs.Add("no-proxy-server", "1"); //Don't use a proxy server, always make direct connections. Overrides any other proxy server flags that are passed.
        settings.UserAgent = "Race Horology"; // RH User Agent
        settings.CefCommandLineArgs.Add("disable-plugins-discovery", "1"); //Disable discovering third-party plugins. Effectively loading only ones shipped with the browser plus third-party ones as specified by --extra-plugin-dir and --load-plugin switches
        settings.SetOffScreenRenderingBestPerformanceArgs();
        settings.CefCommandLineArgs.Add("disable-direct-write", "1");
        settings.CefCommandLineArgs.Add("disable-gpu-vsync", "1"); //Disable Vsync
        settings.CefCommandLineArgs.Add("disable-extensions", "1"); //Extension support can be disabled

        settings.WindowlessRenderingEnabled = true;

        if (cefBrowser.ResourceRequestHandlerFactory == null)
          cefBrowser.ResourceRequestHandlerFactory = new ResourceRequestHandlerFactory();
        var handler = cefBrowser.ResourceRequestHandlerFactory as ResourceRequestHandlerFactory;
        string url = string.Format("file://{0}", reportGenerator.ProposeFilePath());
        if (handler != null)
          handler.RegisterHandler(url, ms.ToArray(), "application/pdf", true);

        cefBrowser.Load(url);
      }
    }

    private void refreshTimout(object sender, TextChangedEventArgs e)
    {
      refreshPdf();
    }

    private IPDFReport getSelectedReport()
    {
      if (cmbReport.SelectedItem is ReportItem ri)
      {
        RaceRun selectedRaceRun = null;
        if (ri.NeedsRaceRun)
        {
          CBItem selected = cmbRaceRun.SelectedValue as CBItem;
          selectedRaceRun = selected?.Value as RaceRun;
        }

        return ri.CreateReport(_race, selectedRaceRun);
      }
      return null;
    }
  }
}
