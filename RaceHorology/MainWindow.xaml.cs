using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

using RaceHorologyLib;
using System.Collections.ObjectModel;
using QRCoder;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// Main entry point of the application
  /// </summary>
  public partial class MainWindow : Window
  {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    // Private data structures
    AppDataModel _dataModel;

    MruList _mruList;
    DSVAlpin2HTTPServer _alpinServer;
    string _appTitle;


    /// <summary>
    /// Constructor of MainWindow
    /// </summary>
    public MainWindow()
    {
      Logger.Info("Application started");

      InitializeComponent();

      // Remember the Application Name
      _appTitle = this.Title;

      // Last recently used files in menu
      _mruList = new MruList("RaceHorology", mnuRecentFiles, 10);
      _mruList.FileSelected += OpenDatabase;

      StartDSVAlpinServer();
    }

    protected override void OnClosed(EventArgs e)
    {
      CloseDatabase();
      StopDSVAlpinServer();
    }

    /// <summary>
    /// "File Open" callback - opens a data base
    /// </summary>
    private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {

      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.DefaultExt = ".mdb";
      openFileDialog.Filter = "DSVAlpin Daten|*.mdb";
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

    private void OptionsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      SettingsDlg dlg = new SettingsDlg();
      dlg.Owner = this;
      dlg.ShowDialog();
    }
    private void HelpCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      AboutDlg dlg = new AboutDlg();
      dlg.Owner = this;
      dlg.ShowDialog();
    }


    /// <summary>
    /// Opens the database and does all jobs to work with the application (connect DatagRids, start web server, ...)
    /// </summary>
    /// <param name="dbPath">Path to the database (Access File)</param>
    private void OpenDatabase(string dbPath)
    {
      //try
      {
        Logger.Info("Open DSVAlpin database: {dbpath}", dbPath);

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

        InitializeTiming();

        // Connect all GUI lists and so on ...
        ConnectGUIToDataModel();

        // Restart DSVALpinServer (for having the lists on mobile devices)
        _alpinServer.UseDataModel(_dataModel);

        _mruList.AddFile(dbPath);
      }
      //catch (Exception ex)
      //{
      //  Logger.Error(ex, "during database loading");
      //  throw;
      //}
    }

    /// <summary>
    /// Closes the data base and performs all shutdown operations (disconnect DatagRids, stop web server, ...)
    /// </summary>
    private void CloseDatabase()
    {
      _alpinServer.UseDataModel(null);

      DisconnectGUIFromDataModel();

      DeinitializeTiming();

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

      foreach (var r in _dataModel.GetRaces())
      {
        TabItem tabRace = new TabItem { Header = r.RaceType.ToString(), Name = r.RaceType.ToString() };
        tabControlTopLevel.Items.Insert(1, tabRace);

        tabRace.FontSize = 16;

        RaceUC raceUC = new RaceUC(_dataModel, r, _liveTimingMeasurement, txtLiveTimingStatus);
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

      _alpinServer = new DSVAlpin2HTTPServer(8081);
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

        if (!string.IsNullOrEmpty(url))
        {
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


    #region LiveTiming

    ALGETdC8001TimeMeasurement _alge;
    LiveTimingMeasurement _liveTimingMeasurement;
    System.Timers.Timer _liveTimingStatusTimer;

    private void InitializeTiming()
    {
      _liveTimingMeasurement = new LiveTimingMeasurement(_dataModel);
      _liveTimingMeasurement.LiveTimingMeasurementStatusChanged += OnLiveTimingMeasurementStatusChanged;

      _alge = new ALGETdC8001TimeMeasurement(Properties.Settings.Default.TimingDevice_Port);
      _alge.RawMessageReceived += Alge_OnMessageReceived;

      _liveTimingMeasurement.SetTimingDevice(_alge, _alge);

      _liveTimingStatusTimer = new System.Timers.Timer(300);
      _liveTimingStatusTimer.Elapsed += UpdateLiveTimingDeviceStatus;
      _liveTimingStatusTimer.AutoReset = true;
      _liveTimingStatusTimer.Enabled = true;

      _alge.Start();
    }

    private void DeinitializeTiming()
    {
      if (_alge == null)
        return;

      _alge.Stop();

      _liveTimingStatusTimer.Elapsed -= UpdateLiveTimingDeviceStatus;
      _liveTimingStatusTimer = null;

      _liveTimingMeasurement.LiveTimingMeasurementStatusChanged -= OnLiveTimingMeasurementStatusChanged;
      _liveTimingMeasurement = null;

      _alge.RawMessageReceived -= Alge_OnMessageReceived;
      _alge = null;
    }


    private void LiveTimingStart_Click(object sender, RoutedEventArgs e)
    {
      _liveTimingMeasurement.Start();
    }

    private void LiveTimingStop_Click(object sender, RoutedEventArgs e)
    {
      _liveTimingMeasurement.Stop();
    }

    private void UpdateLiveTimingStartStopButtons(bool isRunning)
    {
      btnLiveTimingStart.IsChecked = isRunning;
      btnLiveTimingStop.IsChecked = !isRunning;
    }

    private void OnLiveTimingMeasurementStatusChanged(object sender, bool isRunning)
    {
      EnsureOnlyCurrentRaceCanBeSelected(isRunning);
      UpdateLiveTimingStartStopButtons(isRunning);
    }


    private void Alge_OnMessageReceived(object sender, string message)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        txtCOMPort.Text += message + "\n";
        txtCOMPort.ScrollToEnd();
      });
    }

    private void UpdateLiveTimingDeviceStatus(object sender, System.Timers.ElapsedEventArgs e)
    {
      string str = _alge.GetInfo() + ", " + _alge.GetStatusInfo() + ", " + _alge.GetCurrentDayTime().ToString(@"hh\:mm\:ss");
      Application.Current.Dispatcher.Invoke(() =>
      {
        lblTimingDevice.Content = str;
      });
    }

    #endregion


    #region Tab Management

    private void EnsureOnlyCurrentRaceCanBeSelected(bool onlyCurrentRace)
    {
      foreach (TabItem tab in tabControlTopLevel.Items)
      {
        RaceUC raceUC = tab.Content as RaceUC;
        if (raceUC != null)
        {
          bool isEnabled = !onlyCurrentRace || (_dataModel.GetCurrentRace() == raceUC.GetRace());
          tab.IsEnabled = isEnabled;
        }
      }
    }

    private void TabControlTopLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var selected = tabControlTopLevel.SelectedContent as RaceUC;
      if (selected != null)
      {
        _dataModel.SetCurrentRace(selected.GetRace());
        _dataModel.SetCurrentRaceRun(selected.GetRaceRun());
      }
    }

    #endregion

    private void LogoRH_png_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      System.Diagnostics.Process.Start("http://www.race-horology.com");
    }
  }
}
