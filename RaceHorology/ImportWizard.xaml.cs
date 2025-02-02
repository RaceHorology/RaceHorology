using Microsoft.Win32;
using RaceHorologyLib;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for ImportWizard.xaml
  /// </summary>
  public partial class ImportWizard : Window
  {
    AppDataModel _dm;

    IImportReader _importReader;
    Mapping _importMapping;


    public ImportWizard(AppDataModel dm)
    {
      _dm = dm;

      InitializeComponent();

    }

    private void ImportWizard_Loaded(object sender, EventArgs e)
    {
      if (!selectImportFile())
        DialogResult = false;
    }

    protected bool selectImportFile()
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter =
        "Import Files (*.csv;*.tsv;*.txt;*xls;*.xlsx;*.zip)|*.csv;*.tsv;*.txt;*xls;*.xlsx;*.zip" + "|" +
        "Alle Dateien|*.*";
      if (openFileDialog.ShowDialog() == true)
      {
        string path = openFileDialog.FileName;

        if (System.IO.Path.GetExtension(path).ToLowerInvariant() == ".zip")
          _importReader = new ImportZipReader(path);
        else
          _importReader = new ImportReader(path);

        _importMapping = new RaceMapping(_importReader.Columns);
        mappingUC.Mapping = _importMapping;

        bindDataGrid(_importReader.Data.Tables[0].DefaultView);

        FillRaceList(lbRaces, _dm);

        return true;
      }

      return false;
    }

    void bindDataGrid(System.Data.DataView source)
    {
      dgImport.AutoGenerateColumns = false;
      dgImport.Columns.Clear();
      dgImport.ItemsSource = source;

      // Create columns
      foreach (var col in source.Table.Columns)
      {
        DataGridTextColumn dgc = new DataGridTextColumn
        {
          Header = col.ToString()
        };

        Binding b = new Binding()
        {
          Mode = BindingMode.OneWay,
          Path = new PropertyPath(string.Format("[{0}]", col.ToString()))
        };

        dgc.Binding = b;

        dgImport.Columns.Add(dgc);
      }

    }


    public static void FillRaceList(ListBox lbRaces, AppDataModel dm)
    {
      lbRaces.SelectionMode = SelectionMode.Multiple;
      lbRaces.Items.Clear();
      lbRaces.SelectedItems.Clear();
      foreach (var r in dm.GetRaces())
      {
        lbRaces.Items.Add(r);
        lbRaces.SelectedItems.Add(r);
      }
    }


    private void btnImport_Click(object sender, RoutedEventArgs e)
    {
      string messageTextDetails = "";

      var onlyUpdateExisting = chkOnlyUpdateExisting.IsChecked == true;

      if (lbRaces.SelectedItems.Count > 0)
      {
        foreach (var r in lbRaces.SelectedItems)
        {
          Race race = r as Race;
          if (race != null)
          {
            RaceImport imp = new RaceImport(race, _importMapping, new ClassAssignment(_dm.GetParticipantClasses()), _dm.GetTeams());
            imp.OnlyUpdateExisting = onlyUpdateExisting;
            ImportResults results = imp.DoImport(_importReader.Data);

            messageTextDetails += string.Format(
              "Zusammenfassung für das Rennen {0}:\n" +
              "- Erfolgreich importierte Teilnehmer: {1}\n" +
              "- Übersprungene Teilnehmer: {2}\n",
              "- Fehlerhafte Teilnehmer (nicht importiert): {3}\n\n",
              race.ToString(), results.SuccessCount, results.SkipCount, results.ErrorCount);
          }
        }
      }
      else
      {
        ParticipantImport imp = new ParticipantImport(_dm.GetParticipants(), _importMapping, _dm.GetParticipantCategories(), new ClassAssignment(_dm.GetParticipantClasses()), _dm.GetTeams());
        imp.OnlyUpdateExisting = onlyUpdateExisting;
        ImportResults results = imp.DoImport(_importReader.Data);

        messageTextDetails += string.Format(
          "Zusammenfassung für den allgemeinen Teilnehmerimport:\n" +
          "- Erfolgreich importierte Teilnehmer: {0}\n" +
          "- Übersprungene Teilnehmer: {1}\n",
          "- Fehlerhafte Teilnehmer (nicht importiert): {2}\n\n",
          results.SuccessCount, results.SkipCount, results.ErrorCount);
      }
      MessageBox.Show("Der Importvorgang wurde abgeschlossen: \n\n" + messageTextDetails, "Importvorgang", MessageBoxButton.OK, MessageBoxImage.Information);

      DialogResult = true;
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }

    private void btnSelectImportFile_Click(object sender, RoutedEventArgs e)
    {
      selectImportFile();
    }
  }
}
