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
  /// Interaction logic for ImportTimeUC.xaml
  /// </summary>
  public partial class ImportTimeUC : UserControl
  {

    ImportTimeEntryVM _importTimeVM;
    IImportTime _importTimeDevice;
    public event EventHandler Finished;

    public ImportTimeUC()
    {
      InitializeComponent();
    }

    public void Init(AppDataModel dm, Race race, IImportTime importTimeDevice)
    {
      _importTimeVM = new ImportTimeEntryVM(race, importTimeDevice);
      _importTimeDevice = importTimeDevice;

      dgImportTime.ItemsSource = _importTimeVM.ImportEntries;

      cmbRun.SelectedValuePath = "Value";
      UiUtilities.FillCmbRaceRun(cmbRun, race);

      if ((importTimeDevice.SupportedImportTimeFlags() & EImportTimeFlags.RemoteDownload) != EImportTimeFlags.None)
      {
        importTimeDevice.DownloadImportTimes();
        lblHeader.Content = "Drücke Download um den Transfer erneut zu starten.";
      }
      else
      {
        lblHeader.Content = "Starte den Transfer über das Zeitnahmegerät (Classement Senden)";
        btnDownload.Visibility = Visibility.Collapsed;
        lblHeader.HorizontalAlignment = HorizontalAlignment.Center;
      }

      if ((importTimeDevice.SupportedImportTimeFlags() & EImportTimeFlags.StartFinishTime) != EImportTimeFlags.None)
      {
        dgImportTime.ColumnByName("RunTime").Visibility = Visibility.Collapsed;
      }
      else
      {
        dgImportTime.ColumnByName("StartTime").Visibility = Visibility.Collapsed;
        dgImportTime.ColumnByName("FinishTime").Visibility = Visibility.Collapsed;
      }

    }

    private void close()
    {
      Finished?.Invoke(this, new EventArgs());
      _importTimeVM.Dispose();
      _importTimeVM = null;
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
      if (cmbRun.SelectedValue is RaceRun rr)
      {
        if (dgImportTime.SelectedItems.Count > 0)
        {
          bool overwriteAlreadyImportedParticipantAssignment = chkOverwriteManuallyAdjusted.IsChecked == true;
          List<ImportTimeEntryWithParticipant> entries = new List<ImportTimeEntryWithParticipant>();
          foreach (var i in dgImportTime.SelectedItems)
            entries.Add(i as ImportTimeEntryWithParticipant);

          var count = _importTimeVM.Save(rr, entries, overwriteAlreadyImportedParticipantAssignment);
          MessageBox.Show(String.Format("Es wurden {0} Einträge in Durchgang {1} importiert.", count, rr.Run), "Import von Zeiten", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
          MessageBox.Show("Kein Eintrag ausgewählt.\n\nBitte wähle die Einträge aus, die Importiert werden sollen.", "Kein Eintrag ausgewählt", MessageBoxButton.OK, MessageBoxImage.Warning);
      }
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
      close();
    }

    private void cmbRun_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void btnDownload_Click(object sender, RoutedEventArgs e)
    {
      _importTimeVM.Clear();
      _importTimeDevice.DownloadImportTimes();
    }
  }
}
