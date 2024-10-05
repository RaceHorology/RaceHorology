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

using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
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
      configureExport(null);
    }

    public void UpdateAll()
    {
      initialize();
    }


    private void initialize()
    {
      RaceResultViewProvider vp = _thisRace.GetResultViewProvider();

      UiUtilities.FillGrouping(cmbTotalResultGrouping, vp.ActiveGrouping);
      cmbTotalResultGrouping.Items.Add(new CBItem { Text = "Mannschafts-Gruppe", Value = "Team.Group" });


      cmbTotalResult.Items.Clear();
      cmbTotalResult.Items.Add(new CBItem { Text = "Teilnehmer", Value = new CBObjectTotalResults { Type = "participants" } });
      FillCmbTotalsResultsWithRaceSpecifics(cmbTotalResult);
      cmbTotalResult.Items.Add(new CBItem { Text = "Rennergebnis", Value = new CBObjectTotalResults { Type = "raceresults" } });
      cmbTotalResult.SelectedIndex = cmbTotalResult.Items.Count - 1;

      if (_thisRace.GetTeamResultsViewProvider()!= null)
        cmbTotalResult.Items.Add(new CBItem { Text = "Mannschaftswertung", Value = new CBObjectTotalResults { Type = "teamresults" } });
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
        else if (selObj.Type == "teamresults")
          displayView(_thisRace.GetTeamResultsViewProvider());

        configureExport(selected);
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
    DataGridCheckBoxColumn createColumnCheckbox(string columnName, string property, string header)
    {
      var dgc = new DataGridCheckBoxColumn
      {
        Header = header
      };
      Binding b = new Binding(property)
      {
        Mode = BindingMode.TwoWay,
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

      void addCommonColumns()
      {
        dgView.Columns.Add(createColumn("StartNumber", "Participant.StartNumber", "StNr"));
        dgView.Columns.Add(createColumn("Name", "Participant.Name", "Name"));
        dgView.Columns.Add(createColumn("Firstname", "Participant.Firstname", "Vorname"));
        dgView.Columns.Add(createColumn("Year", "Participant.Year", "Jg."));
        dgView.Columns.Add(createColumn("Class", "Participant.Class", "Klasse"));
        dgView.Columns.Add(createColumn("Club", "Participant.Club", "Verein"));
        dgView.Columns.Add(createColumn("Nation", "Participant.Nation", "Nat."));
      }

      // Race Run Results
      if (_viewProvider is RaceRunResultViewProvider rrrVP)
      {
        dgView.Columns.Add(createColumnPosition("Pos", "Position", "Position", false));
        addCommonColumns();
        dgView.Columns.Add(createColumnTime("Zeit", "Runtime", "ResultCode"));
        dgView.Columns.Add(createColumnDiff("Diff", "DiffToFirst"));
        dgView.Columns.Add(createColumnDiffInPercentage("[%]", "DiffToFirstPercentage"));
        dgView.Columns.Add(createColumnAnmerkung());

        setWarningLabelHandler(new RaceRunCompletedWarningLabelHandler(rrrVP.RaceRun, lblWarning));
      }
      // Total Results
      else if (_viewProvider is RaceResultViewProvider)
      {
        dgView.Columns.Add(createColumnPosition("Pos", "Position", "Position", false));
        addCommonColumns();
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
      // Team Results
      else if (_viewProvider is TeamRaceResultViewProvider)
      {
        dgView.Columns.Add(createColumn("StartNumber", "Participant.StartNumber", "StNr"));
        dgView.Columns.Add(createColumn("Name", "Name", "Name"));
        dgView.Columns.Add(createColumn("Firstname", "Participant.Firstname", "Vorname"));
        dgView.Columns.Add(createColumn("Year", "Participant.Year", "Jg."));
        dgView.Columns.Add(createColumn("Class", "Participant.Class", "Klasse"));
        dgView.Columns.Add(createColumn("Club", "Participant.Club", "Verein"));
        dgView.Columns.Add(createColumn("Nation", "Participant.Nation", "Nat."));

        dgView.Columns.Add(createColumnCheckbox("Consider", "Consider", "In Wertung"));
        dgView.Columns.Add(createColumn("Team", "Team", "Team"));
        dgView.Columns.Add(createColumn("TeamGroup", "Team.Group", "Team"));

        dgView.Columns.Add(createColumnTime("Zeit", "Runtime", "ResultCode"));
        dgView.Columns.Add(createColumnDiff("Diff", "DiffToFirst"));
        dgView.Columns.Add(createColumn("Points", "Points", "Punkte"));

        dgView.Columns.Add(createColumnAnmerkung());

        setWarningLabelHandler(new RaceCompletedWarningLabelHandler(_thisRace, lblWarning));
      }
      // Start List
      else if (_viewProvider is StartListViewProvider)
      {
        addCommonColumns();
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
          else if (_thisRace.GetResultViewProvider() is FISRaceResultViewProvider)
            report = new FISRaceResultReport(_thisRace);
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


    struct ExportConfig
    {
      public string Name;
      public Func<CBItem, bool> MatchSelectedListFunc;
      public Func<Race, ICollectionView, string> ExportFunc;
    };

    private static bool MatchSelected(CBItem selected, string type)
    {
      CBObjectTotalResults selObj = selected.Value as CBObjectTotalResults;
      if (selObj != null && selObj.Type == type)
        return true;
      return false;
    }


    private void configureExport(CBItem selectedList)
    {
      List<ExportConfig> exportConfigs = new List<ExportConfig>
      {
        { new ExportConfig { Name = "Alpenhunde - Startliste", ExportFunc = ExportUI.ExportAlpenhundeStartList, MatchSelectedListFunc = (selList) => MatchSelected(selList, "startlist_run") } },
        { new ExportConfig { Name = "Excel - Startliste", ExportFunc = ExportUI.ExportGenericStartListXLSX, MatchSelectedListFunc = (selList) => MatchSelected(selList, "startlist_run") } },
        { new ExportConfig { Name = "CSV - Startliste", ExportFunc = ExportUI.ExportGenericStartListCSV, MatchSelectedListFunc = (selList) => MatchSelected(selList, "startlist_run") } },
      };
      mbtnExport.Items.Clear();
      foreach (var config in exportConfigs)
      {
        bool enabled = selectedList != null && config.MatchSelectedListFunc(selectedList);
        var item = new RibbonMenuItem();
        item.Header = config.Name;
        item.Click += ExportItem_Click;
        item.Tag = config;
        item.IsEnabled = enabled;
        mbtnExport.Items.Add(item);
      }
    }

    private void ExportItem_Click(object sender, RoutedEventArgs e)
    {
      MenuItem menu_item = sender as MenuItem;
      if (menu_item != null && menu_item.Tag != null)
      {
        ExportConfig exportConfig = (ExportConfig)menu_item.Tag;
        var exportedFile = exportConfig.ExportFunc(_thisRace, dgView.ItemsSource as ICollectionView);
        if (exportedFile != null)
        {
          var dlg = new ExportResultDlg(String.Format("Export - {0}", exportConfig.Name), exportedFile, string.Format("Der Export war erfolgreich."));
          dlg.Owner = Window.GetWindow(this);
          dlg.ShowDialog();
        }
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
