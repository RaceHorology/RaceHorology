using RaceHorologyLib;
using System;
using System.Collections.Generic;
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
  /// <summary>
  /// Interaction logic for RaceListsUC.xaml
  /// </summary>
  public partial class RaceListsUC : UserControl
  {
    Race _thisRace;

    ScrollToMeasuredItemBehavior dgTotalResultsScrollBehavior;


    public RaceListsUC()
    {
      InitializeComponent();
    }

    public void Init(Race race)
    {
      _thisRace = race;

      Initialize();
    }

    public void UpdateAll()
    {
      Initialize();
    }


    private void Initialize()
    {
      RaceResultViewProvider vp = _thisRace.GetResultViewProvider();

      UiUtilities.FillGrouping(cmbTotalResultGrouping, vp.ActiveGrouping);
      FillCmbTotalsResults(cmbTotalResult);
      cmbTotalResult.Items.Add(new CBItem { Text = "Rennergebnis", Value = null });
      cmbTotalResult.SelectedIndex = cmbTotalResult.Items.Count - 1;
    }


    class CBObjectTotalResults
    {
      public string Type;
      public RaceRun RaceRun;
    }

    ViewProvider _totalResultsVP = null;


    private void FillCmbTotalsResults(ComboBox cmb)
    {
      cmb.Items.Clear();

      // Fill Runs
      for (int i = 0; i < _thisRace.GetMaxRun(); i++)
      {
        cmb.Items.Add(new CBItem
        {
          Text = String.Format("Startliste {0}. Durchgang", i + 1),
          Value = new CBObjectTotalResults { Type = "startlist", RaceRun = _thisRace.GetRun(i) }
        });

        cmb.Items.Add(new CBItem
        {
          Text = String.Format("Ergebnis {0}. Durchgang", i + 1),
          Value = new CBObjectTotalResults { Type = "results", RaceRun = _thisRace.GetRun(i) }
        });
      }

      cmb.SelectedIndex = 0;
    }


    private void CmbTotalResultGrouping_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbTotalResultGrouping.SelectedValue is CBItem grouping)
        _totalResultsVP?.ChangeGrouping((string)grouping.Value);
    }


    private void CmbTotalResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ViewProvider vp = null;
      if (cmbTotalResult.SelectedValue is CBItem selected)
      {
        CBObjectTotalResults selObj = selected.Value as CBObjectTotalResults;
        if (selObj == null)
          vp = _thisRace.GetResultViewProvider();
        else if (selObj.Type == "results")
          vp = selObj.RaceRun.GetResultViewProvider();
        else if (selObj.Type == "startlist")
          vp = selObj.RaceRun.GetStartListProvider();
      }

      _totalResultsVP = vp;

      adaptTotalResultsView();
    }


    DataGridTextColumn createColumn(string columnName, string property, string header)
    {
      DataGridTextColumn dgc = new DataGridTextColumn
      {
        Header = header
      };
      Binding b = new Binding(property)
      {
        Mode = BindingMode.OneWay,
      };

      dgc.Binding = b;

      DataGridUtil.SetName(dgc, columnName);

      return dgc;
    }


    DataGridTextColumn createColumnAnmerkung()
    {
      DataGridTextColumn dgc = new DataGridTextColumn
      {
        Header = "Anmerkung"
      };
      MultiBinding b = new MultiBinding();
      b.Mode = BindingMode.OneWay;

      Binding b1 = new Binding("ResultCode")
      {
        Mode = BindingMode.OneWay,
      };
      Binding b2 = new Binding("DisqualText")
      {
        Mode = BindingMode.OneWay,
      };

      b.Bindings.Add(b1);
      b.Bindings.Add(b2);

      b.Converter = new ResultCodeWithCommentConverter();
      dgc.Binding = b;

      return dgc;
    }

    DataGridTextColumn createColumnPosition(string columnName, string property, bool inParantheses)
    {
      DataGridTextColumn dgc = new DataGridTextColumn
      {
        Header = "Pos"
      };
      Binding b = new Binding(property)
      {
        Mode = BindingMode.OneWay,
      };

      b.Converter = new PositionConverter(inParantheses);
      dgc.Binding = b;
      dgc.CellStyle = new Style();
      dgc.CellStyle.Setters.Add(new Setter { Property = TextBlock.TextAlignmentProperty, Value = TextAlignment.Right });

      DataGridUtil.SetName(dgc, columnName);

      return dgc;
    }

    DataGridTextColumn createColumnDiffInPercentage(string header, string property)
    {
      DataGridTextColumn dgc = new DataGridTextColumn
      {
        Header = header
      };
      Binding b = new Binding(property)
      {
        Mode = BindingMode.OneWay,
      };

      b.Converter = new PercentageConverter(false);
      dgc.Binding = b;
      dgc.CellStyle = new Style();
      dgc.CellStyle.Setters.Add(new Setter { Property = TextBlock.TextAlignmentProperty, Value = TextAlignment.Right });

      DataGridUtil.SetName(dgc, "Percentage");

      return dgc;
    }

    DataGridTextColumn createColumnDiff(string header, string property)
    {
      DataGridTextColumn dgc = new DataGridTextColumn
      {
        Header = header
      };

      Binding b1 = new Binding(property)
      {
        Mode = BindingMode.OneWay,
      };
      b1.Converter = new RaceHorologyLib.TimeSpanConverter();
      dgc.Binding = b1;
      dgc.CellStyle = new Style();
      dgc.CellStyle.Setters.Add(new Setter { Property = TextBlock.TextAlignmentProperty, Value = TextAlignment.Right });
      dgc.CellStyle.Setters.Add(new Setter { Property = TextBlock.MarginProperty, Value = new Thickness(15, 0, 0, 0) });
      return dgc;
    }

    DataGridTextColumn createColumnTime(string header, string runtime, string resultcode)
    {
      DataGridTextColumn dgc = new DataGridTextColumn
      {
        Header = header
      };

      MultiBinding mb = new MultiBinding();
      Binding b1 = new Binding(runtime)
      {
        Mode = BindingMode.OneWay,
      };
      Binding b2 = new Binding(resultcode)
      {
        Mode = BindingMode.OneWay,
      };
      mb.Bindings.Add(b1);
      mb.Bindings.Add(b2);
      mb.Converter = new ResultTimeAndCodeConverter();
      dgc.Binding = mb;
      dgc.CellStyle = new Style();
      dgc.CellStyle.Setters.Add(new Setter { Property = TextBlock.TextAlignmentProperty, Value = TextAlignment.Right });
      dgc.CellStyle.Setters.Add(new Setter { Property = TextBlock.MarginProperty, Value = new Thickness(15, 0, 0, 0) });
      return dgc;
    }


    private void adaptTotalResultsView()
    {
      dgTotalResults.Columns.Clear();

      dgTotalResults.Columns.Add(createColumnPosition("Position", "Position", false));
      dgTotalResults.Columns.Add(createColumn("StartNumber", "Participant.StartNumber", "StNr"));
      dgTotalResults.Columns.Add(createColumn("Name", "Participant.Name", "Name"));
      dgTotalResults.Columns.Add(createColumn("Firstname", "Participant.Firstname", "Vorname"));
      dgTotalResults.Columns.Add(createColumn("Year", "Participant.Year", "Jahrgang"));
      dgTotalResults.Columns.Add(createColumn("Class", "Participant.Class", "Klasse"));
      dgTotalResults.Columns.Add(createColumn("Club", "Participant.Club", "Verein"));

      // Race Run Results
      if (_totalResultsVP is RaceRunResultViewProvider)
      {
        dgTotalResults.Columns.Add(createColumnTime("Zeit", "Runtime", "ResultCode"));
        dgTotalResults.Columns.Add(createColumnDiff("Diff", "DiffToFirst"));
        dgTotalResults.Columns.Add(createColumnDiffInPercentage("[%]", "DiffToFirstPercentage"));
        dgTotalResults.Columns.Add(createColumnAnmerkung());
      }

      // Total Results
      else if (_totalResultsVP is RaceResultViewProvider)
      {
        foreach (var r in _thisRace.GetRuns())
        {
          dgTotalResults.Columns.Add(createColumnTime(string.Format("Zeit {0}", r.Run), string.Format("SubResults[{0}].Runtime", r.Run), string.Format("SubResults[{0}].RunResultCode ", r.Run)));
          dgTotalResults.Columns.Add(createColumnDiff(string.Format("Diff {0}", r.Run), string.Format("SubResults[{0}].DiffToFirst", r.Run)));
          dgTotalResults.Columns.Add(createColumnDiffInPercentage(string.Format("[%] {0}", r.Run), string.Format("SubResults[{0}].DiffToFirstPercentage", r.Run)));
          dgTotalResults.Columns.Add(createColumnPosition(string.Format("SubResults[{0}].Position", r.Run), string.Format("SubResults[{0}].Position", r.Run), true));
        }

        dgTotalResults.Columns.Add(createColumnTime("Total", "TotalTime", "ResultCode"));
        dgTotalResults.Columns.Add(createColumnAnmerkung());
      }
      // Start List
      else if (_totalResultsVP is StartListViewProvider)
      {
        if (_totalResultsVP is BasedOnResultsFirstRunStartListViewProvider)
        {
          dgTotalResults.Columns.Add(createColumnTime("Zeit", "Runtime", "ResultCode"));
        }
      }

      if (_totalResultsVP != null)
      {
        dgTotalResults.ItemsSource = _totalResultsVP.GetView();
        dgTotalResultsScrollBehavior = new ScrollToMeasuredItemBehavior(dgTotalResults, _thisRace.GetDataModel());
        cmbTotalResultGrouping.SelectCBItem(_totalResultsVP.ActiveGrouping);
      }

      UiUtilities.EnableOrDisableColumns(_thisRace, dgTotalResults);
    }

    private void BtnPrint_Click(object sender, RoutedEventArgs e)
    {
      IPDFReport report = null;

      if (cmbTotalResult.SelectedValue is CBItem selected)
      {
        CBObjectTotalResults selObj = selected.Value as CBObjectTotalResults;
        if (selObj == null)
        {
          if (_thisRace.GetResultViewProvider() is DSVSchoolRaceResultViewProvider)
            report = new DSVSchoolRaceResultReport(_thisRace);
          else
            report = new RaceResultReport(_thisRace);
        }
        else if (selObj.Type == "results")
          report = new RaceRunResultReport(selObj.RaceRun);
        else if (selObj.Type == "startlist")
        {
          if (selObj.RaceRun.GetStartListProvider() is BasedOnResultsFirstRunStartListViewProvider)
            report = new StartListReport2ndRun(selObj.RaceRun);
          else
            report = new StartListReport(selObj.RaceRun);
        }
      }

      CreateAndOpenReport(report);
    }

    private void BtnExportDsv_Click(object sender, RoutedEventArgs e)
    {
      string filePath = System.IO.Path.Combine(
        _thisRace.GetDataModel().GetDB().GetDBPathDirectory(),
        System.IO.Path.GetFileNameWithoutExtension(_thisRace.GetDataModel().GetDB().GetDBFileName()) + ".zip");

      Microsoft.Win32.SaveFileDialog openFileDialog = new Microsoft.Win32.SaveFileDialog();
      openFileDialog.FileName = System.IO.Path.GetFileName(filePath);
      openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(filePath);
      openFileDialog.DefaultExt = ".zip";
      openFileDialog.Filter = "DSV Results (.zip)|*.zip";
      try
      {
        if (openFileDialog.ShowDialog() == true)
        {
          filePath = openFileDialog.FileName;
          DSVExport dsvExport = new DSVExport();
          dsvExport.Export(filePath, _thisRace);
        }
      }
      catch (DSVExportException ex)
      {
        System.Windows.MessageBox.Show(
          "Datei " + System.IO.Path.GetFileName(filePath) + " konnte nicht gespeichert werden.\n\nFehlermeldung: " + ex.GetHumanReadableError(),
          "Fehler",
          System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
      }
      catch (Exception ex)
      {
        System.Windows.MessageBox.Show(
          "Datei " + System.IO.Path.GetFileName(filePath) + " konnte nicht gespeichert werden.\n\n" + ex.Message,
          "Fehler",
          System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
      }
    }


    private void BtnExportDsvAlpin_Click(object sender, RoutedEventArgs e)
    {

      exportToTextFile
        ("DSVAlpin - Tab Separated Text File (.txt)|*.txt",
        ".txt",
        (Race race, string filePath) =>
        {
          DSVAlpinExport exp = new DSVAlpinExport(race);
          TsvExport tsvExp = new TsvExport();
          tsvExp.Export(filePath, exp.ExportToDataSet());
        }
      );
    }


    private void BtnExportCsv_Click(object sender, RoutedEventArgs e)
    {

      exportToTextFile
        ("Comma Separated Text File (.csv)|*.csv",
        ".csv",
        (Race race, string filePath) =>
        {
          Export exp = new Export(race);
          CsvExport csvExp = new CsvExport();
          csvExp.Export(filePath, exp.ExportToDataSet());
        }
      );
    }

    private void BtnExportXlsx_Click(object sender, RoutedEventArgs e)
    {
      exportToTextFile
        ("Microsoft Excel (.xlsx)|*.xslx",
        ".xlsx",
        (Race race, string filePath) =>
        {
          Export exp = new Export(race);
          ExcelExport csvExp = new ExcelExport();
          csvExp.Export(filePath, exp.ExportToDataSet());
        }
      );
    }


    delegate void exportDelegate(Race race, string filepath);
    private void exportToTextFile(string fileDialogFilter, string suffix, exportDelegate expDelegate)
    {
      string filePath = System.IO.Path.Combine(
        _thisRace.GetDataModel().GetDB().GetDBPathDirectory(),
        System.IO.Path.GetFileNameWithoutExtension(_thisRace.GetDataModel().GetDB().GetDBFileName()) + suffix);

      Microsoft.Win32.SaveFileDialog openFileDialog = new Microsoft.Win32.SaveFileDialog();
      openFileDialog.FileName = System.IO.Path.GetFileName(filePath);
      openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(filePath);
      openFileDialog.DefaultExt = suffix;
      openFileDialog.Filter = fileDialogFilter;
      try
      {
        if (openFileDialog.ShowDialog() == true)
        {
          filePath = openFileDialog.FileName;

          expDelegate(_thisRace, filePath);
        }
      }
      catch (Exception ex)
      {
        System.Windows.MessageBox.Show(
          "Datei " + System.IO.Path.GetFileName(filePath) + " konnte nicht gespeichert werden.\n\n" + ex.Message,
          "Fehler",
          System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
      }
    }



    public static void CreateAndOpenReport(IPDFReport report)
    {
      if (report == null)
        return;

      Microsoft.Win32.SaveFileDialog openFileDialog = new Microsoft.Win32.SaveFileDialog();
      string filePath = report.ProposeFilePath();
      openFileDialog.FileName = System.IO.Path.GetFileName(filePath);
      openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(filePath);
      openFileDialog.DefaultExt = ".pdf";
      openFileDialog.Filter = "PDF documents (.pdf)|*.pdf";
      try
      {
        if (openFileDialog.ShowDialog() == true)
        {
          filePath = openFileDialog.FileName;
          report.Generate(filePath);
          System.Diagnostics.Process.Start(filePath);
        }
      }
      catch (Exception ex)
      {
        System.Windows.MessageBox.Show(
          "Datei " + System.IO.Path.GetFileName(filePath) + " konnte nicht gespeichert werden.\n\n" + ex.Message,
          "Fehler",
          System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);

      }
    }

  }
}
