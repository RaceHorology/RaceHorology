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
    HandTimingVM _handTimingVM;

    private AppDataModel _dm;
    private Race _race;


    public HandTimingUC()
    {
      InitializeComponent();

      _handTimingVM = new HandTimingVM(HandTimingVMEntry.ETimeModus.EStartTime);
      dgHandTiming.ItemsSource = _handTimingVM.Items;

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


    public void Init(AppDataModel dm, Race race)
    {
      _dm = dm;
      _race = race;

      fillCmbCalcRun();
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

    private void fillCmbCalcRun() 
    {
      cmbCalcRun.Items.Clear();
      cmbCalcRun.SelectedValuePath = "Value";
      foreach (var r in _race.GetRuns())
        cmbCalcRun.Items.Add(new CBItem { Text = string.Format("Lauf {0}", r.Run), Value = r });
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

      _handTimingVM.AddHandTimings(handTiming.TimingData());
    }

    private void cmbCalcRun_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      RaceRun rr = cmbCalcRun.SelectedValue as RaceRun;

      _handTimingVM.AddRunResults(rr.GetResultList());
    }

    private void cmbCalcDeviceStartOrFinish_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      _handTimingVM.TimeModus = (string)(cmbCalcDeviceStartOrFinish.SelectedItem as CBItem).Value == "Start" 
        ? HandTimingVMEntry.ETimeModus.EStartTime : HandTimingVMEntry.ETimeModus.EFinishTime;
    }

    private void dgHandTiming_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void btnCalcAssign_Click(object sender, RoutedEventArgs e)
    {
      if (dgHandTiming.SelectedItem is HandTimingVMEntry selEntry)
      {
        uint startNumber = 0U;
        try { startNumber = uint.Parse(txtCalcStartNumber.Text); } catch (Exception) { }
        if (startNumber>0)
          _handTimingVM.AssignStartNumber(selEntry, startNumber);
      }
    }

    private void btnCalcDissolve_Click(object sender, RoutedEventArgs e)
    {
      if (dgHandTiming.SelectedItem is HandTimingVMEntry selEntry)
      {
        _handTimingVM.Dissolve(selEntry);
      }
    }

    private void btnCalc_Click(object sender, RoutedEventArgs e)
    {
      if (dgHandTiming.SelectedItem is HandTimingVMEntry selEntry)
      {
        HandTimingCalc calc = new HandTimingCalc(selEntry, _handTimingVM.Items);

        selEntry.SetCalulatedHandTime(calc.CalculatedTime);
      }
    }

  }
}
