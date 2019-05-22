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
    Database _db;
       
    public MainWindow()
    {
      InitializeComponent();

    }

    private void MenuOpen_Click(object sender, RoutedEventArgs e)
    {

      OpenFileDialog openFileDialog = new OpenFileDialog();
      if (openFileDialog.ShowDialog() == true)
      {
        string dbPath = openFileDialog.FileName;

        if (_db != null)
        {
          _db.Close();
          _db = null;
        }

        _db = new Database();
        _db.Connect(dbPath);

        ObservableCollection<Participant> participants = _db.GetParticipants();
        dgParticipants.ItemsSource = participants;
      }
    }
    private void MenuClose_Click(object sender, RoutedEventArgs e)
    {
      if (_db != null)
      {
        _db.Close();
        _db = null;
      }
    }
  }
}
