﻿using RaceHorologyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
  public partial class TimingDeviceAlpenhundeUC : TimingDeviceBaseUC
  {
    private TimingDeviceAlpenhunde _timingDevice;
    public TimingDeviceAlpenhundeUC()
    {
      InitializeComponent();
    }

    public override void Init(ILiveTimeMeasurementDeviceDebugInfo timingDevice)
    {
      _timingDevice = timingDevice as TimingDeviceAlpenhunde;
      ucDebug.Init(timingDevice);


      DataContext = _timingDevice.SystemInfo;
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
          try
          {
            if (openFileDialog.ShowDialog() == true)
            {
              filePath = openFileDialog.FileName;
              using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
              {
                fs.Write(data, 0, data.Length);
              }
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
          var dlg = new ExportResultDlg("FIS Zeitstempel Export", filePath, string.Format("Der Export war erfolgreich."));
          dlg.Owner = Window.GetWindow(wnd);
          dlg.ShowDialog();
          return true;
        });
        return true;
      });
    }
  }
}
