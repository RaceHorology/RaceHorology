using RaceHorologyLib;
using System;
using System.IO;
using System.Windows;

namespace RaceHorology
{
  public partial class TimingDeviceAlpenhundeUC : TimingDeviceBaseUC
  {
    private TimingDeviceAlpenhunde _timingDevice;
    public TimingDeviceAlpenhundeUC()
    {
      InitializeComponent();

      cmbChannel.Items.Clear();
      cmbChannel.Items.Add(new CBItem { Text = "Channel 0", Value = 0 });
      cmbChannel.Items.Add(new CBItem { Text = "Channel 1", Value = 1 });
      cmbChannel.Items.Add(new CBItem { Text = "Channel 2", Value = 2 });
    }

    public override void Init(ILiveTimeMeasurementDeviceDebugInfo timingDevice)
    {
      _timingDevice = timingDevice as TimingDeviceAlpenhunde;
      ucDebug.Init(timingDevice);
      DataContext = _timingDevice.SystemInfo;

      _timingDevice.StatusChanged += timingDevice_StatusChanged;
      enableDisableControls(_timingDevice.IsOnline);
    }

    private void timingDevice_StatusChanged(object sender, bool isRunning)
    {
      System.Windows.Application.Current.Dispatcher.Invoke(() =>
      {
        enableDisableControls(isRunning);
      });
    }

    private void enableDisableControls(bool enable)
    {
      btnChangeChannel.IsEnabled = enable;
      btnFISExport.IsEnabled = enable;
      btnSynchronize.IsEnabled = enable;
      cmbChannel.IsEnabled = enable;
    }

    private void btnFISExport_Click(object sender, RoutedEventArgs e)
    {
      var wnd = this;
      _timingDevice.DownloadFIS((data) =>
      {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
          Microsoft.Win32.SaveFileDialog openFileDialog = new Microsoft.Win32.SaveFileDialog();

          string filePath = "timestamps.alp";
          openFileDialog.FileName = System.IO.Path.GetFileName(filePath);
          //openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(filePath);
          openFileDialog.DefaultExt = ".alp";
          openFileDialog.Filter = "Alpenhunde Zeitstempel (.alp)|*.alp";
          bool saveSuceeded = false;
          try
          {
            if (openFileDialog.ShowDialog() == true)
            {
              filePath = openFileDialog.FileName;
              using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
              {
                fs.Write(data, 0, data.Length);
              }
              saveSuceeded = true;
            }
          }
          catch (Exception ex)
          {
            System.Windows.MessageBox.Show(
              "Datei " + System.IO.Path.GetFileName(filePath) + " konnte nicht gespeichert werden.\n\n" + ex.Message,
              "Fehler",
              System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return false;
          }
          if (saveSuceeded)
          {
            var dlg = new ExportResultDlg("FIS Zeitstempel Export", filePath, string.Format("Der Export war erfolgreich."));
            dlg.Owner = Window.GetWindow(wnd);
            dlg.ShowDialog();
          }
          return true;
        });
        return true;
      });
    }

    private void btnSynchronize_Click(object sender, RoutedEventArgs e)
    {
      var doIt = ShowMessage("Soll wirklich neu synchronisiert werden?");
      if (doIt)
        _timingDevice.Synchronize();
    }

    private void btnChangeChannel_Click(object sender, RoutedEventArgs e)
    {
      var doIt = ShowMessage("Soll der Kanal wirklich gewechselt werden?");
      if (doIt && cmbChannel.SelectedItem is CBItem selected)
      {
        _timingDevice.SetChannel(Convert.ToInt32(selected.Value));
      }
    }

    private bool ShowMessage(string message)
    {
      var res = MessageBox.Show(string.Format("{0}\n\nDie Zeitmessanlage wird dazu neu gestartet.", message), "Alpenhunde startet neu", MessageBoxButton.YesNo, MessageBoxImage.Warning);
      return res == MessageBoxResult.Yes;
    }

  }
}
