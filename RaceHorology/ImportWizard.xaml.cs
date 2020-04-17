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
    IList<Participant> _importTarget;
    
    ImportReader _importReader;
    Mapping _importMapping;


    public ImportWizard(IList<Participant> importTarget)
    {
      _importTarget = importTarget;

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
        _importMapping = new ParticipantMapping(_importReader.Columns);
        mappingUC.Mapping = _importMapping;

        dgImport.ItemsSource = _importReader.Data.Tables[0].DefaultView;

        return true;
      }

      return false;
    }


    private void btnImport_Click(object sender, RoutedEventArgs e)
    {
      Import imp = new Import(_importReader.Data, _importTarget, _importMapping);
      imp.DoImport();

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
