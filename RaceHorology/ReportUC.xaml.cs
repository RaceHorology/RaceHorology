using RaceHorologyLib;
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

    public ReportUC()
    {
      InitializeComponent();
    }

    public void Init(Race race)
    {
      _race = race;

      List<ReportItem> items = new List<ReportItem>();
      items.Add(new ReportItem { Text = "Ergebnisliste", NeedsRaceRun = false, CreateReport = (r, rr) => { return new RaceResultReport(r); } });
      items.Add(new ReportItem { Text = "Teilergebnisliste", NeedsRaceRun = true, CreateReport = (r, rr) => { return new RaceRunResultReport(rr); } });
      items.Add(new ReportItem { Text = "Startliste", NeedsRaceRun = true, CreateReport = (r, rr) => { return (rr.Run == 1) ? (IPDFReport)new StartListReport(rr) : (IPDFReport)new StartListReport2ndRun(rr); } });
      items.Add(new ReportItem { Text = "Schiedsrichter Report", NeedsRaceRun = true, CreateReport = (r, rr) => { return new RefereeProtocol(rr); } });

      cmbReport.ItemsSource = items;

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
      var reportGenerator = getSelectedReport();
      if (reportGenerator != null)
      {
        MemoryStream ms = new MemoryStream();
        reportGenerator.Generate(ms);
        var ms2 = new MemoryStream(ms.ToArray(), false);

        pdfViewer.Load(ms2);
      }
    }

    private void cmbReport_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbReport.SelectedItem is ReportItem ri)
      {
        cmbRaceRun.IsEnabled = ri.NeedsRaceRun;
      }
    }

    private void cmbRaceRun_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      CBItem selected = (sender as ComboBox).SelectedValue as CBItem;
      RaceRun selectedRaceRun = selected?.Value as RaceRun;

    }


    private void GenerateReportPreview()
    {
      RaceListsUC.CreateAndOpenReport(getSelectedReport());
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
