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

using Microsoft.Win32;

using DSVAlpin2Lib;
using System.Collections.ObjectModel;

namespace DSVAlpin2
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    AppDataModel _dataModel;
    MruList _mruList;
       
    public MainWindow()
    {
      InitializeComponent();

      _mruList = new MruList("DSVAlpin2", mnuRecentFiles, 10);
      _mruList.FileSelected += OpenDatabase;
    }

    private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {

      OpenFileDialog openFileDialog = new OpenFileDialog();
      if (openFileDialog.ShowDialog() == true)
      {
        string dbPath = openFileDialog.FileName;
        OpenDatabase(dbPath);
      }
    }
    private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      if (_dataModel != null)
      {
        _dataModel = null;
      }
    }


    private void OpenDatabase(string dbPath)
    {
      try
      {
        if (_dataModel != null)
          _dataModel = null;

        Database db = new Database();
        db.Connect(dbPath);

        _dataModel = new AppDataModel(db);

        ObservableCollection<Participant> participants = _dataModel.GetParticipants();
        dgParticipants.ItemsSource = participants;

        var run = _dataModel.GetRun(0);
        dgStartList.ItemsSource = run.GetStartList();
        dgRunning.ItemsSource = run.GetOnTrackList();
        dgResults.ItemsSource = run.GetResultList();

        _mruList.AddFile(dbPath);
      }
      catch (Exception ex)
      {
        // Remove the file from the MRU list.
        _mruList.RemoveFile(dbPath);

        // Tell the user what happened.
        MessageBox.Show(ex.Message);
      }
    }
  }
}
