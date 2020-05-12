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
    
    ImportReader _importReader;
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
      if (openFileDialog.ShowDialog() == true)
      {
        string path = openFileDialog.FileName;

        _importReader = new ImportReader(path);
        _importMapping = new RaceMapping(_importReader.Columns);
        mappingUC.Mapping = _importMapping;

        dgImport.ItemsSource = _importReader.Data.Tables[0].DefaultView;

        fillRaceList();

        return true;
      }

      return false;
    }


    void fillRaceList()
    {
      lbRaces.SelectionMode = SelectionMode.Multiple;
      lbRaces.Items.Clear();
      lbRaces.SelectedItems.Clear();
      foreach (var r in _dm.GetRaces())
      {
        lbRaces.Items.Add(r);
        lbRaces.SelectedItems.Add(r);
      }
    }


    private void btnImport_Click(object sender, RoutedEventArgs e)
    {

      foreach (var r in lbRaces.SelectedItems)
      {
        Race race = r as Race;
        if (race != null)
        {
          RaceImport imp = new RaceImport(_importReader.Data, race, _importMapping);
          imp.DoImport();
        }
      }

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
