using CefSharp;
using CefSharp.Wpf;
using RaceHorology.Properties;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
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
    public Func<UserControl> UserControl;

    public override string ToString()
    {
      return Text;
    }
  }

  interface IReportSubUC
  {
    void HandleReportGenerator(IPDFReport reportGenerator);
    void Apply(IPDFReport reportGenerator);
  }


  /// <summary>
  /// Interaction logic for ReportUC.xaml
  /// </summary>
  public partial class ReportUC : UserControl
  {
    private Race _race;
    private DelayedEventHandler _refreshDelay;
    IWarningLabelHandler _lblHandler;

    ReportItem _currentRI = null;
    RaceRun _currentRIRun = null;

    IPDFReport _currentReport;
    UserControl _currentSubUC;


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
      items.Add(new ReportItem { Text = "Startliste", NeedsRaceRun = true, CreateReport = (r, rr) => { return (rr.Run == 1) ? (IPDFReport)new StartListReport(rr) : (IPDFReport)new StartListReport2ndRun(rr); } });
      items.Add(new ReportItem { Text = "Teilergebnisliste", NeedsRaceRun = true, CreateReport = (r, rr) => { return new RaceRunResultReport(rr); } });
      items.Add(new ReportItem { Text = "Ergebnisliste", NeedsRaceRun = false, CreateReport = (r, rr) => { return (r.GetResultViewProvider() is DSVSchoolRaceResultViewProvider) ? new DSVSchoolRaceResultReport(r) : (r.GetResultViewProvider() is FISRaceResultViewProvider) ? new FISRaceResultReport(r) : new RaceResultReport(r); } });
      items.Add(new ReportItem { Text = "Mannschaftsergebnisliste", NeedsRaceRun = false, CreateReport = (r, rr) => { return new TeamRaceResultReport(r); }, UserControl = () => new TeamResultsPrintUC() });
      items.Add(new ReportItem { Text = "Urkunden", NeedsRaceRun = false, CreateReport = (r, rr) => { return new Certificates(r, 10); }, UserControl = () => new CertificatesPrintUC() });
      items.Add(new ReportItem { Text = "Zeitnehmer Checkliste", NeedsRaceRun = true, CreateReport = (r, rr) => { return (rr.Run == 1) ? (IPDFReport)new TimerReport(rr) : (IPDFReport)new TimerReport(rr); } });
      items.Add(new ReportItem { Text = "Schiedsrichter Protokoll", NeedsRaceRun = true, CreateReport = (r, rr) => { return new RefereeProtocol(rr); } });
      items.Add(new ReportItem { Text = "Schiedsrichterbericht", NeedsRaceRun = true, CreateReport = (r, rr) => { return new RefereeReport(rr); } });

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
      if (_currentReport != null)
        RaceListsUC.CreateAndOpenReport(_currentReport);
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
        var reportSubUC = _currentSubUC as IReportSubUC;
        if (reportSubUC != null)
          reportSubUC.Apply(_currentReport);

        MemoryStream ms = new MemoryStream();
        _currentReport.Generate(ms);

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
        string url = "file://abcdef.pdf";
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
        if (_currentRI == ri && (!ri.NeedsRaceRun || ri.NeedsRaceRun && _currentRIRun == selectedRaceRun))
          return _currentReport;

        if (_currentSubUC != null)
        {
          grdBottom.Children.Remove(_currentSubUC);
          _currentSubUC = null;
        }

        _currentRI = ri;
        _currentReport = ri.CreateReport(_race, selectedRaceRun);

        if (ri.UserControl != null)
        {
          var uc = ri.UserControl();
          grdBottom.Children.Add(uc);
          Grid.SetRow(uc, 0);
          Grid.SetColumn(uc, 0);
          _currentSubUC = uc;

          var reportSubUC = _currentSubUC as IReportSubUC;
          reportSubUC.HandleReportGenerator(_currentReport);
        }

        return _currentReport;
      }
      return null;
    }
  }
}
