using RaceHorologyLib;
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

namespace RaceHorology
{

  internal interface IWarningLabelHandler : IDisposable
  { }

  /// <summary>
  /// Watches out a race and sets the corresponding warning text if, e.g. startnumbers are inconsistent.
  /// </summary>
  internal class RaceWarningLabelHandler : IWarningLabelHandler
  {
    Race _race;
    Label _label;

    public RaceWarningLabelHandler(Race race, Label label)
    {
      _race = race;
      _label = label;

      _race.PropertyChanged += OnChanged;
      OnChanged(null, null);
    }
  

    private void OnChanged(object sender, PropertyChangedEventArgs e)
    {
      if (_race.IsConsistent)
        _label.Content = "";
      else
        _label.Content = "Startnummernvergabe noch nicht abgeschlossen";
    }


    #region Disposable implementation
    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          _race.PropertyChanged -= OnChanged;
        }

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }

  /// <summary>
  /// Watches out a race run and sets the corresponding warning text if, e.g. race is not yet completed.
  /// </summary>
  internal class RaceRunCompletedWarningLabelHandler : IWarningLabelHandler
  {
    RaceRun _raceRun;
    Label _label;

    public RaceRunCompletedWarningLabelHandler(RaceRun raceRun, Label label)
    {
      _raceRun = raceRun;
      _label = label;

      _raceRun.PropertyChanged += OnChanged;
      OnChanged(null, null);
    }


    private void OnChanged(object sender, PropertyChangedEventArgs e)
    {
      if (_raceRun.IsComplete)
        _label.Content = "";
      else
        _label.Content = string.Format("{0}. Durchgang ist noch nicht abgeschlossen", _raceRun.Run);
    }


    #region Disposable implementation
    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          _raceRun.PropertyChanged -= OnChanged;
        }

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }

  /// <summary>
  /// Watches out a race and sets the corresponding warning text if, e.g. race is not yet completed.
  /// </summary>
  internal class RaceCompletedWarningLabelHandler : IWarningLabelHandler
  {
    Race _race;
    Label _label;

    public RaceCompletedWarningLabelHandler(Race race, Label label)
    {
      _race = race;
      _label = label;

      _race.PropertyChanged += OnChanged;
      OnChanged(null, null);
    }


    private void OnChanged(object sender, PropertyChangedEventArgs e)
    {
      if (_race.IsComplete)
        _label.Content = "";
      else
        _label.Content = string.Format("Das Rennen ist noch nicht abgeschlossen.");
    }


    #region Disposable implementation
    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          _race.PropertyChanged -= OnChanged;
        }

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }



  /// <summary>
  /// Interaction logic for RaceListsUC.xaml
  /// </summary>
  public partial class RaceListsUC : UserControl
  {
    Race _thisRace;

    IWarningLabelHandler _lblHandler;

    DataGridColumnVisibilityContextMenu _gridColumnHandler;

    public RaceListsUC()
    {
      _gridColumnHandler = null;
      InitializeComponent();
    }

    public void Init(Race race)
    {
      _thisRace = race;

      initialize();
    }

    public void UpdateAll()
    {
      initialize();
    }


    private void initialize()
    {
      RaceResultViewProvider vp = _thisRace.GetResultViewProvider();

      UiUtilities.FillGrouping(cmbTotalResultGrouping, vp.ActiveGrouping);

      cmbTotalResult.Items.Clear();
      cmbTotalResult.Items.Add(new CBItem { Text = "Teilnehmer", Value = new CBObjectTotalResults { Type = "participants" } });
      FillCmbTotalsResultsWithRaceSpecifics(cmbTotalResult);
      cmbTotalResult.Items.Add(new CBItem { Text = "Rennergebnis", Value = new CBObjectTotalResults { Type = "raceresults" } });
      cmbTotalResult.SelectedIndex = cmbTotalResult.Items.Count - 1;
    }


    private void setWarningLabelHandler(IWarningLabelHandler handler)
    {
      if (_lblHandler != null)
        _lblHandler.Dispose();

      _lblHandler = handler;
    }


    class CBObjectTotalResults
    {
      public string Type;
      public RaceRun RaceRun;
    }

    ViewProvider _viewProvider = null;


    private void FillCmbTotalsResultsWithRaceSpecifics(ComboBox cmb)
    {
      // Fill Runs
      for (int i = 0; i < _thisRace.GetMaxRun(); i++)
      {
        cmb.Items.Add(new CBItem
        {
          Text = String.Format("Startliste {0}. Durchgang", i + 1),
          Value = new CBObjectTotalResults { Type = "startlist_run", RaceRun = _thisRace.GetRun(i) }
        });

        cmb.Items.Add(new CBItem
        {
          Text = String.Format("Ergebnis {0}. Durchgang", i + 1),
          Value = new CBObjectTotalResults { Type = "results_run", RaceRun = _thisRace.GetRun(i) }
        });
      }

      cmb.SelectedIndex = 0;
    }


    private void CmbTotalResultGrouping_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbTotalResultGrouping.SelectedValue is CBItem grouping)
        _viewProvider?.ChangeGrouping((string)grouping.Value);
    }


    private void CmbTotalResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbTotalResult.SelectedValue is CBItem selected)
      {
        CBObjectTotalResults selObj = selected.Value as CBObjectTotalResults;
        if (selObj == null) // Fallback
          displayView(_thisRace.GetResultViewProvider());
        else if (selObj.Type == "participants")
          displayParticipants();
        else if (selObj.Type == "raceresults")
          displayView(_thisRace.GetResultViewProvider());
        else if (selObj.Type == "results_run")
          displayView(selObj.RaceRun.GetResultViewProvider());
        else if (selObj.Type == "startlist_run")
          displayView(selObj.RaceRun.GetStartListProvider());
      }
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

    DataGridTextColumn createColumnPoints(string columnName, string property)
    {
      DataGridTextColumn dgc = new DataGridTextColumn
      {
        Header = "Punkte"
      };
      Binding b = new Binding(property)
      {
        Mode = BindingMode.OneWay,
      };

      b.Converter = new PointsConverter();
      dgc.Binding = b;
      dgc.CellStyle = new Style();
      dgc.CellStyle.Setters.Add(new Setter { Property = TextBlock.TextAlignmentProperty, Value = TextAlignment.Right });

      DataGridUtil.SetName(dgc, columnName);

      return dgc;
    }


    DataGridTextColumn createColumnPosition(string header, string columnName, string property, bool inParantheses)
    {
      DataGridTextColumn dgc = new DataGridTextColumn
      {
        Header = header
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


    void clearDataGrid()
    {
      dgView.ItemsSource = null;
      dgView.Columns.Clear();

      dgView.CanUserSortColumns = false;
    }

    void displayParticipants()
    {
      clearDataGrid();

      _viewProvider = null;

      dgView.CanUserSortColumns = true;

      dgView.Columns.Add(createColumn("StartNumber", "StartNumber", "StNr"));
      dgView.Columns.Add(createColumn("Name", "Name", "Name"));
      dgView.Columns.Add(createColumn("Firstname", "Firstname", "Vorname"));
      dgView.Columns.Add(createColumn("Year", "Year", "Jg."));
      dgView.Columns.Add(createColumn("Class", "Class", "Klasse"));
      dgView.Columns.Add(createColumn("Club", "Club", "Verein"));
      dgView.Columns.Add(createColumn("Nation", "Nation", "Nat."));
      dgView.Columns.Add(createColumnPoints("Points", "Points"));

      dgView.ItemsSource = _thisRace.GetParticipants();

      setWarningLabelHandler(new RaceWarningLabelHandler(_thisRace, lblWarning));
    }


    private void displayView(ViewProvider viewProvider)
    {
      clearDataGrid();

      _viewProvider = viewProvider;

      if (_viewProvider == null)
        return;

      if (!(_viewProvider is StartListViewProvider))
        dgView.Columns.Add(createColumnPosition("Pos", "Position", "Position", false));

      dgView.Columns.Add(createColumn("StartNumber", "Participant.StartNumber", "StNr"));
      dgView.Columns.Add(createColumn("Name", "Participant.Name", "Name"));
      dgView.Columns.Add(createColumn("Firstname", "Participant.Firstname", "Vorname"));
      dgView.Columns.Add(createColumn("Year", "Participant.Year", "Jg."));
      dgView.Columns.Add(createColumn("Class", "Participant.Class", "Klasse"));
      dgView.Columns.Add(createColumn("Club", "Participant.Club", "Verein"));
      dgView.Columns.Add(createColumn("Nation", "Participant.Nation", "Nat."));

      // Race Run Results
      if (_viewProvider is RaceRunResultViewProvider rrrVP)
      {
        dgView.Columns.Add(createColumnTime("Zeit", "Runtime", "ResultCode"));
        dgView.Columns.Add(createColumnDiff("Diff", "DiffToFirst"));
        dgView.Columns.Add(createColumnDiffInPercentage("[%]", "DiffToFirstPercentage"));
        dgView.Columns.Add(createColumnAnmerkung());

        setWarningLabelHandler(new RaceRunCompletedWarningLabelHandler(rrrVP.RaceRun, lblWarning));
      }

      // Total Results
      else if (_viewProvider is RaceResultViewProvider)
      {
        foreach (var r in _thisRace.GetRuns())
        {
          dgView.Columns.Add(createColumnTime(string.Format("Zeit {0}", r.Run), string.Format("SubResults[{0}].Runtime", r.Run), string.Format("SubResults[{0}].RunResultCode ", r.Run)));
          dgView.Columns.Add(createColumnDiff(string.Format("Diff {0}", r.Run), string.Format("SubResults[{0}].DiffToFirst", r.Run)));
          dgView.Columns.Add(createColumnDiffInPercentage(string.Format("[%] {0}", r.Run), string.Format("SubResults[{0}].DiffToFirstPercentage", r.Run)));
          dgView.Columns.Add(createColumnPosition(string.Format("Pos {0}", r.Run), string.Format("SubResults[{0}].Position", r.Run), string.Format("SubResults[{0}].Position", r.Run), true));
        }

        dgView.Columns.Add(createColumnTime("Total", "TotalTime", "ResultCode"));
        dgView.Columns.Add(createColumnDiff("Diff", "DiffToFirst"));
        dgView.Columns.Add(createColumnDiffInPercentage("[%]", "DiffToFirstPercentage"));
        dgView.Columns.Add(createColumnAnmerkung());

        setWarningLabelHandler(new RaceCompletedWarningLabelHandler(_thisRace, lblWarning));
      }
      // Start List
      else if (_viewProvider is StartListViewProvider)
      {
        if (_viewProvider is BasedOnResultsFirstRunStartListViewProvider slVP2)
        {
          dgView.Columns.Add(createColumnTime("Zeit", "Runtime", "ResultCode"));
          setWarningLabelHandler(new RaceRunCompletedWarningLabelHandler(slVP2.BasedOnRun, lblWarning));
        }
        else
        {
          setWarningLabelHandler(new RaceWarningLabelHandler(_thisRace, lblWarning));
        }
      }

      dgView.ItemsSource = _viewProvider.GetView();
      cmbTotalResultGrouping.SelectCBItem(_viewProvider.ActiveGrouping);

      UiUtilities.EnableOrDisableColumns(_thisRace, dgView);
      _gridColumnHandler = new DataGridColumnVisibilityContextMenu(dgView, "racelist");
    }


    private void BtnPrint_Click(object sender, RoutedEventArgs e)
    {
      PDFReport report = null;

      if (cmbTotalResult.SelectedValue is CBItem selected)
      {
        CBObjectTotalResults selObj = selected.Value as CBObjectTotalResults;
        if ( selObj == null                     // Fallback
          || selObj.Type == "raceresults")      // ResultList
        {
          if (_thisRace.GetResultViewProvider() is DSVSchoolRaceResultViewProvider)
            report = new DSVSchoolRaceResultReport(_thisRace);
          else
            report = new RaceResultReport(_thisRace);
        }
        else if (selObj.Type == "raceresults")
          displayView(_thisRace.GetResultViewProvider());
        else if (selObj.Type == "results_run")
        {
          report = new RaceRunResultReport(selObj.RaceRun);
        }
        else if (selObj.Type == "startlist_run")
        {
          if (selObj.RaceRun.GetStartListProvider() is BasedOnResultsFirstRunStartListViewProvider)
            report = new StartListReport2ndRun(selObj.RaceRun);
          else
            report = new StartListReport(selObj.RaceRun);
        }
      }

      if (report != null)
      {
        report.WithDiagram = chkPrintOptionWithDiagram.IsChecked == true;
        report.WithRaceHeader = chkPrintOptionWithRaceHeader.IsChecked == true;
        CreateAndOpenReport(report);
      }
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
        ("DSVAlpin - Tab Separated Text File (.txt)|*.txt|" +
         "DSVAlpin - Tab Separated Text File - UTF-8 (.txt)|*.txt"
        ,".txt",
        (Race race, string filePath, bool utf8) =>
        {
          DSVAlpinExport exp = new DSVAlpinExport(race);
          TsvExport tsvExp = new TsvExport();
          tsvExp.Export(filePath, exp.ExportToDataSet(), utf8);
        }
      );
    }


    private void BtnExportCsv_Click(object sender, RoutedEventArgs e)
    {

      exportToTextFile
        ("Comma Separated Text File (.csv)|*.csv|" +
         "Comma Separated Text File - UTF-8 (.csv)|*.csv"
        ,".csv",
        (Race race, string filePath, bool utf8) =>
        {
          Export exp = new Export(race);
          CsvExport csvExp = new CsvExport();
          csvExp.Export(filePath, exp.ExportToDataSet(), utf8);
        }
      );
    }

    private void BtnExportXlsx_Click(object sender, RoutedEventArgs e)
    {
      exportToTextFile
        ("Microsoft Excel (.xlsx)|*.xslx",
        ".xlsx",
        (Race race, string filePath, bool utf8) =>
        {
          Export exp = new Export(race);
          ExcelExport csvExp = new ExcelExport();
          csvExp.Export(filePath, exp.ExportToDataSet());
        }
      );
    }


    delegate void exportDelegate(Race race, string filepath, bool utf8);
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

          string appliedFilter;
          string[] filterstring = openFileDialog.Filter.Split('|');
          appliedFilter = filterstring[(openFileDialog.FilterIndex - 1) * 2];
          bool utf8 = appliedFilter.Contains("UTF-8");

          expDelegate(_thisRace, filePath, utf8);
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
