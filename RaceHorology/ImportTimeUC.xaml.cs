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
    public event EventHandler Finished;

    public ImportTimeUC()
    {
      InitializeComponent();
    }

    public void Init(AppDataModel dm, Race race, IImportTime importTimeDevice)
    {
      _importTimeVM = new ImportTimeEntryVM(race, importTimeDevice);

      dgImportTime.ItemsSource = _importTimeVM.ImportEntries;

      cmbRun.SelectedValuePath = "Value";
      UiUtilities.FillCmbRaceRun(cmbRun, race);

      importTimeDevice.DownloadImportTimes();
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
          List<ImportTimeEntryWithParticipant> entries = new List<ImportTimeEntryWithParticipant>();
          foreach (var i in dgImportTime.SelectedItems)
            entries.Add(i as ImportTimeEntryWithParticipant);

          var count = _importTimeVM.Save(rr, entries);
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
  }
}
