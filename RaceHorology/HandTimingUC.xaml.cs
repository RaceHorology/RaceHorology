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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for HandTimingUC.xaml
  /// </summary>
  public partial class HandTimingUC : UserControl
  {
    COMPortViewModel _comPorts;

    public HandTimingUC()
    {
      InitializeComponent();

      fillComboDevices();
      fillComboStartFinish(cmbDeviceStartOrFinish);
      cmbDeviceStartOrFinish.SelectedIndex = 0;
      fillComboStartFinish(cmbCalcDeviceStartOrFinish);
      cmbCalcDeviceStartOrFinish.SelectedIndex = 0;

      Unloaded += OnUnloaded;

      _comPorts = new COMPortViewModel();
      cmbDevicePort.ItemsSource = _comPorts.Items;
      cmbDevicePort.SelectedValuePath = "Port";

      loadLastConfig();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      storeLastConfig();
    }


    private void loadLastConfig()
    {
      cmbDevice.SelectCBItem(Properties.Settings.Default.HandTiming_LastDevice);
      cmbDevicePort.SelectedValue = Properties.Settings.Default.HandTiming_LastDevicePort;
    }

    private void storeLastConfig()
    {
      Properties.Settings.Default.HandTiming_LastDevice = (cmbDevice.SelectedItem as CBItem)?.Value.ToString();
      Properties.Settings.Default.HandTiming_LastDevicePort = cmbDevicePort.SelectedValue?.ToString();
    }


    private void fillComboDevices()
    {
      cmbDevice.Items.Add(new CBItem { Text = "ALGE Timy", Value = "ALGETimy" });
      cmbDevice.Items.Add(new CBItem { Text = "Tag Heuer (Pocket Pro)", Value = "TagHeuerPPro" });
      cmbDevice.Items.Add(new CBItem { Text = "Datei", Value = "File" });
    }

    private void fillComboStartFinish(ComboBox cmb)
    {
      cmb.Items.Add(new CBItem { Text = "Start", Value = "Start" });
      cmb.Items.Add(new CBItem { Text = "Ziel", Value = "Finish" });
    }

    private void cmbDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      bool bFile= object.Equals((cmbDevice.SelectedItem as CBItem)?.Value, "File");

      cmbDevicePort.IsEnabled = !bFile;
    }

    private void btnDeviceLoad_Click(object sender, RoutedEventArgs e)
    {
      string device = (cmbDevice.SelectedItem as CBItem)?.Value.ToString();
      string devicePort = cmbDevicePort.SelectedValue?.ToString();
      string deviceStartOrFinish = (cmbDeviceStartOrFinish.SelectedItem as CBItem)?.Value.ToString();

      if (device=="File")
      {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.DefaultExt = ".txt";
        openFileDialog.Filter = "Textdatei|*.txt";
        if (openFileDialog.ShowDialog() == true)
          devicePort = openFileDialog.FileName;
        else
          return;
      }

      IHandTiming handTiming = HandTiming.CreateHandTiming(device, devicePort);
      handTiming.Connect();
      handTiming.StartGetTimingData();
      foreach (var t in handTiming.TimingData())
      {
        dgHandTiming.Items.Add(t);
      }

    }

  }
}
