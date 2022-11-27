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

    private void DeInit()
    {
      _importTimeVM.Dispose();
      _importTimeVM = null;
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
      if (cmbRun.SelectedValue is RaceRun rr)
        _importTimeVM.Save(rr);

      DeInit();
      Finished?.Invoke(this, new EventArgs());
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      DeInit();
      Finished?.Invoke(this, new EventArgs());
    }

    private void cmbRun_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }
  }
}
