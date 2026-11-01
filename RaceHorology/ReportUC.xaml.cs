using Microsoft.Web.WebView2.Core;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

    // Virtual URL that WebResourceRequested answers with the generated report
    const string PdfViewerBaseUrl = "https://report.racehorology.example/";
    Task<bool> _pdfViewerInit;
    byte[] _pdfData;
    int _pdfVersion;


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
      items.Add(new ReportItem
      {
        Text = "Startliste",
        NeedsRaceRun = true,
        CreateReport = (r, rr) => { return (rr.Run == 1) ? (IPDFReport)new StartListReport(rr) : (IPDFReport)new StartListReport2ndRun(rr); },
        UserControl = () => new StdListPrintUC(false)
      });
      items.Add(new ReportItem
      {
        Text = "Teilergebnisliste",
        NeedsRaceRun = true,
        CreateReport = (r, rr) => { return new RaceRunResultReport(rr); },
        UserControl = () => new StdListPrintUC(false)
      });
      items.Add(new ReportItem
      {
        Text = "Ergebnisliste",
        NeedsRaceRun = false,
        CreateReport = (r, rr) => { return (r.GetResultViewProvider() is DSVSchoolRaceResultViewProvider) ? new DSVSchoolRaceResultReport(r) : (r.GetResultViewProvider() is FISRaceResultViewProvider) ? new FISRaceResultReport(r) : new RaceResultReport(r); },
        UserControl = () => new StdListPrintUC(true)
      });
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

    private async void btnPrint_Click(object sender, RoutedEventArgs e)
    {
      if (await initPdfViewer())
        pdfViewer.CoreWebView2.ShowPrintUI(CoreWebView2PrintDialogKind.Browser);
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

    private async void refreshPdf()
    {
      var reportGenerator = getSelectedReport();
      if (reportGenerator != null)
      {
        var reportSubUC = _currentSubUC as IReportSubUC;
        if (reportSubUC != null)
          reportSubUC.Apply(_currentReport);

        MemoryStream ms = new MemoryStream();
        _currentReport.Generate(ms);
        _pdfData = ms.ToArray();

        if (await initPdfViewer())
          // A new URL per refresh, otherwise the viewer may show a cached version
          pdfViewer.CoreWebView2.Navigate(PdfViewerBaseUrl + "report" + (++_pdfVersion) + ".pdf");
      }
    }

    private Task<bool> initPdfViewer()
    {
      if (_pdfViewerInit == null)
        _pdfViewerInit = doInitPdfViewer();
      return _pdfViewerInit;
    }

    private async Task<bool> doInitPdfViewer()
    {
      try
      {
        // WebView2 would store its data next to the exe by default, which is not writable below Program Files
        string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RaceHorology", "WebView2");
        var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
        await pdfViewer.EnsureCoreWebView2Async(environment);
      }
      catch (WebView2RuntimeNotFoundException)
      {
        pdfViewer.Visibility = Visibility.Collapsed;
        txtNoPdfViewer.Visibility = Visibility.Visible;
        btnPrint.IsEnabled = false;
        return false;
      }

      var settings = pdfViewer.CoreWebView2.Settings;
      settings.AreDevToolsEnabled = false;
      settings.IsStatusBarEnabled = false;

      pdfViewer.CoreWebView2.AddWebResourceRequestedFilter(PdfViewerBaseUrl + "*", CoreWebView2WebResourceContext.All);
      pdfViewer.CoreWebView2.WebResourceRequested += pdfViewer_WebResourceRequested;
      return true;
    }

    private void pdfViewer_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
    {
      var stream = new MemoryStream(_pdfData ?? new byte[0], false);
      e.Response = pdfViewer.CoreWebView2.Environment.CreateWebResourceResponse(stream, 200, "OK", "Content-Type: application/pdf");
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
        if (_currentRI == ri && (!ri.NeedsRaceRun || (ri.NeedsRaceRun && _currentRIRun == selectedRaceRun)))
          return _currentReport;

        if (_currentSubUC != null)
        {
          _currentSubUC = null;
          addOptions.Content = null;
        }

        _currentRI = ri;
        _currentRIRun = selectedRaceRun;
        _currentReport = ri.CreateReport(_race, selectedRaceRun);

        if (ri.UserControl != null)
        {
          var uc = ri.UserControl();

          addOptions.Content = uc;
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
