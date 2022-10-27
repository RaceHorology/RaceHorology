using RaceHorologyLib;
using Syncfusion.Windows.PdfViewer;
using System;
using System.Collections.Generic;
using System.IO;
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
    private DelayedEventHandler refreshDelay;

    public ReportUC()
    {
      InitializeComponent();

      var pdfWorkDir = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RaceHorology", "PDFViewer");
      System.IO.Directory.CreateDirectory(pdfWorkDir);
      pdfViewer.ReferencePath = pdfWorkDir;

      refreshDelay = new DelayedEventHandler(300, refreshTimout);

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

    private bool pdfControlCustimized = false;
    protected override void OnRender(DrawingContext drawingContext)
    {
      base.OnRender(drawingContext);
      // Disable some controls ...
      // Note: All other events did not work, the OnRender method seems to be late enough so that the PDFControl got initialized
      if (!pdfControlCustimized)
      {
        customizePdfControl();
        pdfControlCustimized = true;
      }
    }


    public void Init(Race race)
    {
      _race = race;

      List<ReportItem> items = new List<ReportItem>();
      items.Add(new ReportItem { Text = "Ergebnisliste", NeedsRaceRun = false, CreateReport = (r, rr) => { return new RaceResultReport(r); } });
      items.Add(new ReportItem { Text = "Teilergebnisliste", NeedsRaceRun = true, CreateReport = (r, rr) => { return new RaceRunResultReport(rr); } });
      items.Add(new ReportItem { Text = "Startliste", NeedsRaceRun = true, CreateReport = (r, rr) => { return (rr.Run == 1) ? (IPDFReport)new StartListReport(rr) : (IPDFReport)new StartListReport2ndRun(rr); } });
      items.Add(new ReportItem { Text = "Schiedsrichter Protokoll", NeedsRaceRun = true, CreateReport = (r, rr) => { return new RefereeProtocol(rr); } });

      cmbReport.ItemsSource = items;
      cmbReport.SelectedIndex = 0;

      UiUtilities.FillCmbRaceRun(cmbRaceRun, _race);
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
      GenerateReportPreview();
    }

    private void btnPrint_Click(object sender, RoutedEventArgs e)
    {
      pdfViewer.Print(true);
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
        refreshDelay.Delayed(null, null);
    }

    private void refreshPdf()
    {
      var reportGenerator = getSelectedReport();
      if (reportGenerator != null)
      {
        MemoryStream ms = new MemoryStream();
        reportGenerator.Generate(ms);
        var ms2 = new MemoryStream(ms.ToArray(), false);

        pdfViewer.Load(ms2);
        pdfViewer.ZoomMode = ZoomMode.FitPage;
        pdfViewer.CursorMode = PdfViewerCursorMode.HandTool;
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

    private void customizePdfControl()
    {
      hidePdfControls(new List<string> {
        "PART_FileToggleButton",
        "Part_NavigationToolsSeparator",
        "Part_ZoomToolsSeparator_0",
        "PART_AnnotationToolsSeparator",
        "PART_StickyNote",
        "PART_Ink",
        "PART_InkEraser",
        "PART_Shapes",
        "PART_Fill",
        "PART_FreeText",
        "PART_ButtonTextBoxFont",
        "PART_AnnotationsSeparator",
        "PART_Stamp",
        "PART_ButtonSignature",
        "PART_SelectTool",
        "PART_HandTool",
        "PART_MarqueeZoom",
        "Part_CursorTools",
        "PART_ButtonTextSearch",
        "PART_TextMarkup"
      });
    }

    private void hidePdfControls(IList<string> ids)
    {
      DocumentToolbar toolbar = pdfViewer.Template.FindName("PART_Toolbar", pdfViewer) as DocumentToolbar;
      foreach(var id in ids)
      {
        var item = toolbar.Template.FindName(id, toolbar);
        if (item is UIElement c)
          c.Visibility = Visibility.Collapsed;
        else
          System.Diagnostics.Debug.Assert(false);
      }
    }
  }
}
