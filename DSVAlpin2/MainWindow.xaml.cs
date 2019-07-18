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
  /// Main entry point of the application
  /// </summary>
  public partial class MainWindow : Window
  {
    // Private data structures
    AppDataModel _dataModel;
    Race _currentRace;
    RaceRun _currentRaceRun;

    MruList _mruList;
    DSVAlpin2HTTPServer _alpinServer;
    string _appTitle;


    /// <summary>
    /// Constructor of MainWindow
    /// </summary>
    public MainWindow()
    {
      InitializeComponent();

      // Remember the Application Name
      _appTitle = this.Title;

      // Last recently used files in menu
      _mruList = new MruList("DSVAlpin2", mnuRecentFiles, 10);
      _mruList.FileSelected += OpenDatabase;

      SetupTesting();

    }

    /// <summary>
    /// "File Open" callback - opens a data base
    /// </summary>
    private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {

      OpenFileDialog openFileDialog = new OpenFileDialog();
      if (openFileDialog.ShowDialog() == true)
      {
        string dbPath = openFileDialog.FileName;
        OpenDatabase(dbPath);
      }
    }

    /// <summary>
    /// File Close - closes the data base
    /// </summary>
    private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      CloseDatabase();
    }


    /// <summary>
    /// Opens the database and does all jobs to work with the application (connect DatagRids, start web server, ...)
    /// </summary>
    /// <param name="dbPath">Path to the database (Access File)</param>
    private void OpenDatabase(string dbPath)
    {
      //try
      {
        // Close database if it was already open
        if (_dataModel != null)
          CloseDatabase();

        // Open the database ...
        Database db = new Database();
        db.Connect(dbPath);

        // ... and create the corresponding data model
        _dataModel = new AppDataModel(db);

        // Change the Application Window to contain the opened DataBase
        this.Title = _appTitle + " - " + System.IO.Path.GetFileName(dbPath);

        // Connect all GUI lists and so on ...
        ConnectGUIToDataModel();

        // Restart DSVALpinServer (for having the lists on mobile devices)
        StartDSVAlpinServer();


        _mruList.AddFile(dbPath);
      }
      /*
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Datei kann nicht geöffnet werden", MessageBoxButton.OK, MessageBoxImage.Error);

        // Close eveything again
        CloseDatabase();
      }*/
    }

    /// <summary>
    /// Closes the data base and performs all shutdown operations (disconnect DatagRids, stop web server, ...)
    /// </summary>
    private void CloseDatabase()
    {
      StopDSVAlpinServer();

      DisconnectGUIFromDataModel();

      if (_dataModel != null)
      {
        _dataModel = null;
      }

    }

    /// <summary>
    /// Connects the GUI (e.g. Data Grids, ...) to the data model
    /// </summary>
    private void ConnectGUIToDataModel()
    {
      // Connect with GUI DataGrids
      ObservableCollection<Participant> participants = _dataModel.GetParticipants();
      dgParticipants.ItemsSource = participants;

      // TODO: Hide not needed columns
      //dgStartList.Columns[5].Visibility = Visibility.Collapsed;

      foreach (var r in _dataModel.GetRaces())
      {
        TabItem tabRace = new TabItem { Header = r.RaceType.ToString(), Name = r.RaceType.ToString() };
        tabControlTopLevel.Items.Insert(1, tabRace);

        tabRace.FontSize = 16;

        RaceUC raceUC = new RaceUC(_dataModel, r);
        tabRace.Content = raceUC;
      }
    }


    /// <summary>
    /// Disconnects the GUI (e.g. Data Grids, ...) from the data model
    /// </summary>
    private void DisconnectGUIFromDataModel()
    {
      dgParticipants.ItemsSource = null;

      while (tabControlTopLevel.Items.Count > 2)
        tabControlTopLevel.Items.RemoveAt(1);


      _currentRace = null;
      _currentRaceRun = null;
    }


    /// <summary>
    /// Starts the web backend for the mobile clients (e.g. for speaker) 
    /// </summary>
    /// <remarks>
    /// Also triggers the display of the URL to use / the QR code for the mobile clients
    /// </remarks>
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

    /// <summary>
    /// Stops the web backend for the mobile clients (e.g. for speaker) 
    /// </summary>
    private void StopDSVAlpinServer()
    {
      if (_alpinServer != null)
      {
        // Stop and re-create
        _alpinServer.Stop();
        _alpinServer = null;
      }

      DisplayURL();
    }

    /// <summary>
    /// Display of the URL to use / the QR code for the mobile clients
    /// </summary>
    /// <remarks>
    /// If the server is not running, the QR code will be removed and the label displays "not available".
    /// </remarks>
    private void DisplayURL()
    {
      if (_alpinServer == null)
      {
        imgQRCode.Source = null;
        lblURL.Content = "nicht verfügbar";
      }
      else
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
    }


    /// <summary>
    /// Callback for the URL label / QRCode image to open the web browser
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LblURL_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (_alpinServer != null)
        System.Diagnostics.Process.Start(_alpinServer.GetUrl());
    }



    #region Testing

    ALGETdC8001TimeMeasurement _alge;

    private void SetupTesting()
    {
      FillCOMPorts(cmbCOMPort);

    }

    protected void FillCOMPorts(ComboBox combo)
    {
      // Get a list of serial port names.
      string[] ports = System.IO.Ports.SerialPort.GetPortNames();

      // Display each port name to the console.
      foreach (string port in ports)
      {
        combo.Items.Add(port);
      }
    }

    private void CmbCOMPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmbCOMPort.SelectedItem != null)
        _alge = new ALGETdC8001TimeMeasurement(cmbCOMPort.SelectedItem?.ToString());

      _alge.RawMessageReceived += Alge_OnMessageReceived;

      _alge.TimeMeasurementReceived += OnTimeMeasurementReceived;
    }

    private void Alge_OnMessageReceived(object sender, string message)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        txtCOMPort.Text += message + "\n";
        txtCOMPort.ScrollToEnd();
      });
    }


    private void OnTimeMeasurementReceived(object sender, TimeMeasurementEventArgs e)
    {
      RaceParticipant participant = _currentRace.GetParticipant(e.StartNumber);

      Application.Current.Dispatcher.Invoke(() =>
      {
        if (participant != null)
        {

          if (e.BStartTime)
            _currentRaceRun.SetStartTime(participant, e.StartTime);

          if (e.BFinishTime)
            _currentRaceRun.SetFinishTime(participant, e.FinishTime);

          if (e.BRunTime)
            _currentRaceRun.SetRunTime(participant, e.RunTime);
        }
      });
    }


    private void TxtTest1_TextChanged(object sender, TextChangedEventArgs e)
    {
      //string text = txtTest1.Text;
      //testParticipantsSrc.Filter += new FilterEventHandler(delegate (object s, FilterEventArgs ea) { ea.Accepted = ((Participant)ea.Item).Firstname.Contains(text); });
    }


    #endregion

    private void TabControlTopLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var selected = tabControlTopLevel.SelectedContent as RaceUC;
      if (selected != null)
        _currentRace = selected.GetRace();

    }
  }
}
