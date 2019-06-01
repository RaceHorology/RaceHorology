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
using System.ComponentModel;
using QRCoder;

namespace DSVAlpin2
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    AppDataModel _dataModel;
    MruList _mruList;
    DSVAlpin2HTTPServer _alpinServer;
    string _appTitle;

    public MainWindow()
    {
      InitializeComponent();

      // Remember the Application Name
      _appTitle = this.Title;

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

        // Create new Data Model
        _dataModel = new AppDataModel(db);

        // Change the Application Window to contain the opened DataBase
        this.Title = _appTitle + " - " + System.IO.Path.GetFileName(dbPath);

        // Connect with GUI DataGrids
        ObservableCollection<Participant> participants = _dataModel.GetParticipants();
        dgParticipants.ItemsSource = participants;

        var run = _dataModel.GetRun(0);
        dgStartList.ItemsSource = run.GetStartList();
        dgRunning.ItemsSource = run.GetOnTrackList();
        dgResults.ItemsSource = run.GetResultList();

        // Restart DSVALpinServer (for having the lists on mobile devices)
        StartDSVAlpinServer();

        SetupTesting();

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

    private void StartDSVAlpinServer()
    {
      if (_alpinServer != null)
      {
        // Stop and re-create
        _alpinServer.Stop();
        _alpinServer = null;
      }

      _alpinServer = new DSVAlpin2HTTPServer(8081, _dataModel);
      _alpinServer.Start();

      DisplayURL();
    }

    private void DisplayURL()
    {
      string url = _alpinServer.GetUrl();

      QRCodeGenerator qrGenerator = new QRCodeGenerator();
      QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
      QRCode qrCode = new QRCode(qrCodeData);
      System.Drawing.Bitmap bitmap = qrCode.GetGraphic(10);

      BitmapImage bitmapimage = new BitmapImage();
      using (System.IO.MemoryStream memory = new System.IO.MemoryStream())
      {
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
        memory.Position = 0;
        bitmapimage.BeginInit();
        bitmapimage.StreamSource = memory;
        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapimage.EndInit();
      }

      imgQRCode.Source = bitmapimage;
      lblURL.Content = url;
    }


    private void LblURL_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (_alpinServer != null)
        System.Diagnostics.Process.Start(_alpinServer.GetUrl());
    }


    private void Button_Click(object sender, RoutedEventArgs e)
    {
      var run = _dataModel.GetRun(0);
      var startList = run.GetStartList();
      startList.Insert(0, new Participant
        {
          Name = "Temp",
          Firstname = "Vorname",
          Sex = "M",
          Club = "Club",
          Nation = "",
          Class = "unknown",
          Year = 2000,
          StartNumber = 999
        });
    }


    #region Testing

    private CollectionViewSource testParticipantsSrc;

    private void SetupTesting()
    {
      var participants = _dataModel.GetParticipants();

      testParticipantsSrc = new CollectionViewSource();
      testParticipantsSrc.Source = participants;

      testParticipantsSrc.SortDescriptions.Clear();
      testParticipantsSrc.SortDescriptions.Add(new SortDescription(nameof(Participant.Year), ListSortDirection.Ascending));

      testParticipantsSrc.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Participant.Class)));

      dgTest1.ItemsSource = testParticipantsSrc.View;

      string output = Newtonsoft.Json.JsonConvert.SerializeObject(testParticipantsSrc.View);
      System.Diagnostics.Debug.Write(output);
    }


    private void TxtTest1_TextChanged(object sender, TextChangedEventArgs e)
    {
      string text = txtTest1.Text;
      testParticipantsSrc.Filter += new FilterEventHandler(delegate (object s, FilterEventArgs ea) { ea.Accepted = ((Participant)ea.Item).Firstname.Contains(text); });
    }


    #endregion
  }
}
