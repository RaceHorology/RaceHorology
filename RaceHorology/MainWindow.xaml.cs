/*
 *  Copyright (C) 2019 - 2023 by Sven Flossmann
 *  
 *  This file is part of Race Horology.
 *
 *  Race Horology is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  any later version.
 * 
 *  Race Horology is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with Race Horology.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  Diese Datei ist Teil von Race Horology.
 *
 *  Race Horology ist Freie Software: Sie können es unter den Bedingungen
 *  der GNU Affero General Public License, wie von der Free Software Foundation,
 *  Version 3 der Lizenz oder (nach Ihrer Wahl) jeder neueren
 *  veröffentlichten Version, weiter verteilen und/oder modifizieren.
 *
 *  Race Horology wird in der Hoffnung, dass es nützlich sein wird, aber
 *  OHNE JEDE GEWÄHRLEISTUNG, bereitgestellt; sogar ohne die implizite
 *  Gewährleistung der MARKTFÄHIGKEIT oder EIGNUNG FÜR EINEN BESTIMMTEN ZWECK.
 *  Siehe die GNU Affero General Public License für weitere Details.
 *
 *  Sie sollten eine Kopie der GNU Affero General Public License zusammen mit diesem
 *  Programm erhalten haben. Wenn nicht, siehe <https://www.gnu.org/licenses/>.
 * 
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

using RaceHorologyLib;
using System.Collections.ObjectModel;
using QRCoder;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace RaceHorology
{

  // Shortcut Singleton
  public sealed class ShortcutSingleton
  {
    private ShortcutSingleton() { }
    private static ShortcutSingleton instance = null;
    public static ShortcutSingleton Instance
    {
      get
      {
        if (instance == null)
        {
          instance = new ShortcutSingleton();
        }
        return instance;
      }
    }

    public void EmitSave()
    {
      var handler = Save;
      if (handler != null) Save.Invoke();
    }

    public delegate void SaveHandler();
    public event SaveHandler Save;
  }

  
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// Main entry point of the application
  /// </summary>
  public partial class MainWindow : Window
  {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    // Private data structures
    AppDataModel _dataModel;
    MainWindowMenuVM _menuVM;

    MruList _mruList;
    DSVAlpin2HTTPServer _alpinServer;
    string _appTitle;

    RHAlgeTimyUSB.AlgeTimyUSB _timyUSB;

    /// <summary>
    /// Constructor of MainWindow
    /// </summary>
    public MainWindow()
    {
      Logger.Info("Application started");


      InitializeComponent();

      // Remember the Application Name
      _appTitle = this.Title;
      updateAppTitle();

      // Last recently used files in menu
      _mruList = new MruList("RaceHorology", mnuRecentFiles, 10);
      _mruList.FileSelected += OpenDatabase;

      _menuVM = new MainWindowMenuVM();
      mnuMain.DataContext = _menuVM;

      configureEpxortMenu(mnuExport);

      StartDSVAlpinServer();

      UpdateLiveTimingDeviceStatus(null, null);

      _timyUSB = new RHAlgeTimyUSB.AlgeTimyUSB();
    }

    protected override void OnClosed(EventArgs e)
    {
      CloseDatabase();
      StopDSVAlpinServer();
      RaceHorology.Properties.Settings.Default.Save(); // TODO: Need to add later on to ExitApplication function when it was merged.
    }

    /// <summary>
    /// "File Create" callback - opens a data base
    /// </summary>
    private void NewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      SaveFileDialog openFileDialog = new SaveFileDialog();
      openFileDialog.DefaultExt = ".mdb";
      openFileDialog.Filter = "Race Horology Daten|*.mdb";
      openFileDialog.OverwritePrompt = true;
      if (openFileDialog.ShowDialog() == true)
      {
        string dbPath = openFileDialog.FileName;
        OpenDatabase(new Database().CreateDatabase(dbPath));
      }
    }

    /// <summary>
    /// "File Open" callback - opens a data base
    /// </summary>
    private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.DefaultExt = ".mdb";
      openFileDialog.Filter = "Race Horology Dateien|*.mdb";
      if (openFileDialog.ShowDialog() == true)
      {
        string dbPath = openFileDialog.FileName;
        OpenDatabase(dbPath);
      }
    }


    private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      ShortcutSingleton.Instance.EmitSave();
    }


    /// <summary>
    /// File Close - closes the data base
    /// </summary>
    private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      CloseDatabase();
    }

    /// <summary>
    /// Applicaton Exit
    /// </summary>
    private void ApplicationClose(object sender, RoutedEventArgs e)
    {
      CloseDatabase();
      StopDSVAlpinServer();
      Environment.Exit(0);
    }

    private void OptionsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      SettingsDlg dlg = new SettingsDlg();
      dlg.Owner = this;
      dlg.ShowDialog();
    }


    private void HandTimeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      var race = _dataModel.GetCurrentRace();

      if (race == null)
        return;

      HandTimingDlg dlg = new HandTimingDlg();
      dlg.Init(_dataModel, race);
      dlg.Owner = this;
      dlg.Show();
    }


    private void ImportTimeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      var race = _dataModel.GetCurrentRace();

      if (race == null)
        return;

      if (_timingDevice is IImportTime importDevice)
      {
        ImportTimeDlg dlg = new ImportTimeDlg();
        dlg.Init(_dataModel, race, importDevice);
        dlg.Owner = this;
        dlg.Show();
      }
      else
        MessageBox.Show("Das aktuelle Zeitmessgeräte erlaubt kein Importieren von Zeiten.");
    }


    private void DeleteRunResultsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      var race = _dataModel.GetCurrentRace();

      if (race == null)
        return;

      var res = MessageBox.Show(
        string.Format("Sollen wirklich alle Zeiten des Rennens {0} gelöscht werden?", race.ToString()), 
        "Zeiten löschen?", 
        MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

      if (res == MessageBoxResult.No)
        return;

      foreach (var rr in race.GetRuns())
      {
        rr.DeleteRunResults();
      }
    }


    private void HelpCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      AboutDlg dlg = new AboutDlg();
      dlg.Owner = this;
      dlg.ShowDialog();
    }


    private void OnlineDocumentationCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      System.Diagnostics.Process.Start("https://docs.race-horology.com");
    }

    /// <summary>
    /// Opens the database and does all jobs to work with the application (connect DatagRids, start web server, ...)
    /// </summary>
    /// <param name="dbPath">Path to the database (Access File)</param>
    private void OpenDatabase(string dbPath)
    {
      try
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
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Die Datei konnte nicht geöffnet werden.\n\n{0}", ex.Message), "Fehler beim Öffnen", MessageBoxButton.OK, MessageBoxImage.Error);
        Logger.Error(ex, "during database loading");
      }

      if (_dataModel != null)
      {
        updateAppTitle();

        InitializeTiming();

        // Connect all GUI lists and so on ...
        ConnectGUIToDataModel();

        // Restart DSVALpinServer (for having the lists on mobile devices)
        _alpinServer.UseDataModel(_dataModel);

        _menuVM.SetDataModel(_dataModel);

        _mruList.AddFile(dbPath);
      }
    }

    /// <summary>
    /// Closes the data base and performs all shutdown operations (disconnect DatagRids, stop web server, ...)
    /// </summary>
    private void CloseDatabase()
    {
      _menuVM.SetDataModel(null);

      _alpinServer.UseDataModel(null);

      DisconnectGUIFromDataModel();

      DeInitializeTiming();

      if (_dataModel != null)
      {
        _dataModel.Close();
        _dataModel = null;
      }

      updateAppTitle();
    }

    private void updateAppTitle()
    {
      string dbFile = "keine Datei geöffnet";
      if (_dataModel != null)
      {
        dbFile = _dataModel.GetDB().GetDBPath();
      }

      // Change the Application Window to contain the opened DataBase
      this.Title = string.Format("{0} - {1}", _appTitle, dbFile);
    }

    /// <summary>
    /// Connects the GUI (e.g. Data Grids, ...) to the data model
    /// </summary>
    private void ConnectGUIToDataModel()
    {
      CompetitionUC competitionUC = new CompetitionUC(_dataModel, _liveTimingMeasurement, txtLiveTimingStatus);
      ucMainArea.Children.Add(competitionUC);
    }


    /// <summary>
    /// Disconnects the GUI (e.g. Data Grids, ...) from the data model
    /// </summary>
    private void DisconnectGUIFromDataModel()
    {
      ucMainArea.Children.Clear();
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
      imgQRCode.Source = QRCodeUtils.GetUrlQR(_alpinServer);
    }


    /// <summary>
    /// Callback for the URL label / QRCode image to open the web browser
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LblURL_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (_alpinServer != null)
      {
        QRCodeDlg dlg = new QRCodeDlg(_alpinServer);
        dlg.Owner = Window.GetWindow(this);
        dlg.ShowDialog();
      }
    }


    #region LiveTiming

    ILiveTimeMeasurementDevice _timingDevice;
    LiveTimingMeasurement _liveTimingMeasurement;
    System.Timers.Timer _liveTimingStatusTimer;

    private void InitializeTiming()
    {
      _liveTimingMeasurement = new LiveTimingMeasurement(_dataModel, Properties.Settings.Default.AutoAddParticipants);

      _liveTimingStatusTimer = new System.Timers.Timer(300);
      _liveTimingStatusTimer.Elapsed += UpdateLiveTimingDeviceStatus;
      _liveTimingStatusTimer.AutoReset = true;
      _liveTimingStatusTimer.Enabled = true;

      Properties.Settings.Default.PropertyChanged += SettingChangingHandler;

      InitializeTimingDevice();
    }

    private void DeInitializeTiming()
    {
      Properties.Settings.Default.PropertyChanged -= SettingChangingHandler;

      if (_liveTimingStatusTimer != null)
        _liveTimingStatusTimer.Elapsed -= UpdateLiveTimingDeviceStatus;

      DeInitializeTimingDevice();

      _liveTimingStatusTimer = null;
      _liveTimingMeasurement = null;
    }

    private void InitializeTimingDevice()
    {
      if (_timingDevice != null)
        throw new Exception("timing device already initialized");

      string dumpDir = null;
      if (Properties.Settings.Default.TimingDevice_Debug_Dump)
        dumpDir = _dataModel.GetDB().GetDBPathDirectory();

      ILiveTimeMeasurementDevice newTimingDevice = null;
      if (Properties.Settings.Default.TimingDevice_Type.Contains("ALGE TdC")) {
        newTimingDevice = new ALGETdC8001TimeMeasurement(Properties.Settings.Default.TimingDevice_Port, dumpDir);
      }
      else if (Properties.Settings.Default.TimingDevice_Type.Contains("ALGE Timy (via USB)"))
      {
        newTimingDevice = new RHAlgeTimyUSB.AlgeTimyUSB();
      }
      else if (Properties.Settings.Default.TimingDevice_Type.Contains("Alpenhunde")) {
        var hostname = Properties.Settings.Default.TimingDevice_Url;
        newTimingDevice = new TimingDeviceAlpenhunde(hostname);
      }

      if (newTimingDevice != null)
      {
        // Cleanup old devices
        if (_timingDevice != null)
        {
          _liveTimingMeasurement.RemoveTimingDevice(_timingDevice);
          _liveTimingMeasurement.SetLiveDateTimeProvider(null);
        }

        // Create new devices
        _liveTimingMeasurement.AddTimingDevice(newTimingDevice, true);

        if (newTimingDevice is ILiveDateTimeProvider)
          _liveTimingMeasurement.SetLiveDateTimeProvider(newTimingDevice as ILiveDateTimeProvider);

        _timingDevice = newTimingDevice;
        _timingDevice.Start();
      }
    }

    private void DeInitializeTimingDevice()
    {
      if (_timingDevice != null)
      {
        _liveTimingMeasurement.RemoveTimingDevice(_timingDevice);
        _liveTimingMeasurement.SetLiveDateTimeProvider(null);

        _timingDevice.Stop();
        _timingDevice = null;
      }
    }


    private void ReInitializeTimingDevice()
    {
      DeInitializeTimingDevice();
      InitializeTimingDevice();
    }


    private void SettingChangingHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case "AutoAddParticipants":
          if (_liveTimingMeasurement != null)
            _liveTimingMeasurement.AutoAddParticipants = Properties.Settings.Default.AutoAddParticipants;
          break;
        case "TimingDevice_Port":
        case "TimingDevice_Type":
        case "TimingDevice_Url":
        case "TimingDevice_Debug_Dump":
          ReInitializeTimingDevice();
          break;
        default:
          break;
      }
    }


    private void btnTimingDeviceStartStop_Click(object sender, RoutedEventArgs e)
    {
      if (_liveTimingMeasurement == null)
        return;

      var timingDevice = _liveTimingMeasurement.LiveTimingDevice;
      if (timingDevice == null)
        return;

      if (timingDevice.IsOnline)
        timingDevice.Stop();
      else
        timingDevice.Start();
    }


    private void UpdateLiveTimingDeviceStatus(object sender, System.Timers.ElapsedEventArgs e)
    {
      bool timingDeviceOnline = false;
      var timingDevice = _liveTimingMeasurement != null ? _liveTimingMeasurement.LiveTimingDevice : null;
      var dateTimeProvider = _liveTimingMeasurement != null ? _liveTimingMeasurement.LiveDateTimeProvider : null;
      bool connectInProgress = false;

      string str = "---";
      if (timingDevice!=null && dateTimeProvider!=null)
      { 
        str = timingDevice.GetDeviceInfo() + ", " + timingDevice.GetStatusInfo() + ", " + dateTimeProvider.GetCurrentDayTime().ToString(@"hh\:mm\:ss");
        timingDeviceOnline = timingDevice.IsOnline;
        connectInProgress = timingDevice.IsStarted != timingDevice.IsOnline;
      }

      Application.Current.Dispatcher.Invoke(() =>
      {
        lblTimingDevice.Content = str;
        btnTimingDeviceStartStop.Content = timingDeviceOnline ? "Trennen" : "Verbinden";
        btnTimingDeviceStartStop.IsEnabled = timingDevice != null && !connectInProgress;
        btnTimingDeviceDebug.IsEnabled = timingDevice != null;
      });
    }

    #endregion

    private void LogoRH_png_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      System.Diagnostics.Process.Start("http://www.race-horology.com");
    }

    private void btnTimingDeviceDebug_Click(object sender, RoutedEventArgs e)
    {
      if (_timingDevice == null)
      {
        MessageBox.Show("Zeitmessgerät nicht verfügbar.", "Protokoll", MessageBoxButton.OK, MessageBoxImage.Information);
        return;
      }

      if (_timingDevice is ILiveTimeMeasurementDeviceDebugInfo debugableTimingDevice)
      {
        TimingDeviceDebugDlg debugDlg = new TimingDeviceDebugDlg(debugableTimingDevice);
        debugDlg.Show();
      }
    }

    struct ExportConfig
    {
      public string Name;
      public Func<Race, string> ExportFunc;
    };

    private void configureEpxortMenu(MenuItem menuExport)
    {
      List<ExportConfig> exportConfigs = new List<ExportConfig> {
        { new ExportConfig { Name = "DSV (XML Format)", ExportFunc = ExportUI.ExportDsv } },
        { new ExportConfig { Name = "Excel Export", ExportFunc = ExportUI.ExportXLSX } },
        { new ExportConfig { Name = "CSV - Export", ExportFunc = ExportUI.ExportCSV } },
        { new ExportConfig { Name = "rennmeldung.de", ExportFunc = ExportUI.ExportDsvAlpin } },
        { new ExportConfig { Name = "DSV-Alpin (altes Format)", ExportFunc = ExportUI.ExportDsvAlpin } },
      };

      foreach (var config in exportConfigs)
      {
        MenuItem subMenu = new MenuItem();
        subMenu.Header = config.Name;
        subMenu.Tag = config;
        subMenu.Click += exportMenu_Click;
        menuExport.Items.Add(subMenu);
      }
    }

    private void exportMenu_Click(object sender, RoutedEventArgs e)
    {
      MenuItem menu_item = sender as MenuItem;
      if (menu_item != null && menu_item.Tag != null)
      {
        var race = _dataModel.GetCurrentRace();
        ExportConfig exportConfig = (ExportConfig)menu_item.Tag;
        if (race != null)
        {
          var exportedFile = exportConfig.ExportFunc(race);
          if (exportedFile != null)
          {
            var dlg = new ExportResultDlg(String.Format("Export - {0}", exportConfig.Name), exportedFile, string.Format("Der Export war erfolgreich."));
            dlg.Owner = Window.GetWindow(this);
            dlg.ShowDialog();
          }
        }
      }
    }
  }


  public class MainWindowMenuVM : INotifyPropertyChanged
  {
    AppDataModel _dm;

    bool _hasActiveRace = false;
    bool _hasDataLoaded = false;

    public void SetDataModel(AppDataModel dm)
    {
      if (_dm != null)
      {
        _dm.CurrentRaceChanged -= onCurrentRaceChanged;
      }

      _dm = dm;

      if (_dm != null)
      {
        _dm.CurrentRaceChanged += onCurrentRaceChanged;
      }

      HasDataLoaded = _dm != null;

      onCurrentRaceChanged(null, null);
    }


    private void onCurrentRaceChanged(object sender, AppDataModel.CurrentRaceEventArgs args)
    {
      HasActiveRace = _dm?.GetCurrentRace() != null;
    }

    public bool HasActiveRace
    {
      get { return _hasActiveRace; }
      private set { _hasActiveRace = value; NotifyPropertyChanged(); }
    }

    public bool HasDataLoaded
    {
      get { return _hasDataLoaded; }
      private set { _hasDataLoaded = value; NotifyPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
