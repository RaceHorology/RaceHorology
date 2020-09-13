using Microsoft.Win32;
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
using System.Windows.Shapes;

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

        dgImport.ItemsSource = _importReader.Data.Tables[0].DefaultView;

        FillRaceList(lbRaces, _dm);

        return true;
      }

      return false;
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

      if (lbRaces.SelectedItems.Count > 0)
      { 
        foreach (var r in lbRaces.SelectedItems)
        {
          Race race = r as Race;
          if (race != null)
          {
            RaceImport imp = new RaceImport(_importReader.Data, race, _importMapping, new ClassAssignment(_dm.GetParticipantClasses()));
            ImportResults results = imp.DoImport();

            messageTextDetails += string.Format(
              "Zusammenfassung für das Rennen {0}:\n" +
              "- Erfolgreich importierte Teilnehmer: {1}\n" +
              "- Nicht importierte Teilnehmer: {2}\n\n",
              race.ToString(), results.SuccessCount, results.ErrorCount);
          }
        }
      }
      else
      {
        ParticipantImport imp = new ParticipantImport(_importReader.Data, _dm.GetParticipants(), _importMapping, new ClassAssignment(_dm.GetParticipantClasses()));
        ImportResults results = imp.DoImport();

        messageTextDetails += string.Format(
          "Zusammenfassung für den allgemeinen Teilnehmerimport:\n" +
          "- Erfolgreich importierte Teilnehmer: {0}\n" +
          "- Nicht importierte Teilnehmer: {1}\n\n",
          results.SuccessCount, results.ErrorCount);
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
