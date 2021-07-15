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
    HandTimingVMManager _handTimingVMManager;
    HandTimingVM _currentHandTimingVM;

    private AppDataModel _dm;
    private Race _race;


    public event EventHandler Finished;

    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


    public HandTimingUC()
    {
      InitializeComponent();


      fillComboDevices();
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

      _handTimingVMManager = new HandTimingVMManager(_dm);
      _handTimingVMManager.LoadHandTimingFromFile();

      _race = race;

      fillCmbCalcRun();

      updateHandTimingVM();
    }


    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      storeLastConfig();

      _handTimingVMManager.SaveHandTimingToFile();
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
      
      cmbCalcRun.SelectedIndex = 0;
    }


    private void updateHandTimingVM()
    {
      RaceRun rr = cmbCalcRun.SelectedValue as RaceRun;
      var timeModus = (string)(cmbCalcDeviceStartOrFinish.SelectedItem as CBItem).Value == "Start"
        ? HandTimingVMEntry.ETimeModus.EStartTime : HandTimingVMEntry.ETimeModus.EFinishTime;

      if (_race == null && rr == null)
        return;

      _currentHandTimingVM = _handTimingVMManager.GetHandTimingVM(_race, rr, timeModus);

      dgHandTiming.ItemsSource = _currentHandTimingVM.Items;
      updateButtonEnableState();
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
      string deviceStartOrFinish = (cmbCalcDeviceStartOrFinish.SelectedItem as CBItem)?.Value.ToString();

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

      Progress<StdProgress> progress = new Progress<StdProgress>();
      StdProgressDlg dlgProgress = new StdProgressDlg();
      dlgProgress.ShowAndClose(progress);

      handTiming.DoProgressReport(progress);

      Task.Run(() =>
      {
        using (handTiming)
        {
          try
          {
            handTiming.Connect();
            handTiming.StartGetTimingData();
            _currentHandTimingVM.AddHandTimings(handTiming.TimingData());
          }
          catch(Exception)
          { throw; }
        }
      }).ContinueWith((t) =>
      {
        if (t.IsFaulted)
        {
          MessageBox.Show(
            string.Format("Daten konnten nicht geladen werden.\n({0}).\nWeitere Details im Logfile.", t.Exception?.InnerException?.Message), 
            "Fehler", 
            MessageBoxButton.OK, 
            MessageBoxImage.Error
          );
          Logger.Error(t.Exception, "loading handtime failed");
        }
      }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void cmbCalcRun_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      updateHandTimingVM();
    }

    private void cmbCalcDeviceStartOrFinish_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      updateHandTimingVM();
    }

    private void dgHandTiming_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      updateButtonEnableState();
    }

    private void updateButtonEnableState()
    {
      HandTimingVMEntry entry = dgHandTiming.SelectedItem as HandTimingVMEntry;

      btnCalc.IsEnabled = entry != null;
      
      btnCalcAssign.IsEnabled = entry?.HandTime != null;
      btnCalcDissolve.IsEnabled = entry?.HandTime != null && entry?.StartNumber > 0;
    }

    private void btnCalcAssign_Click(object sender, RoutedEventArgs e)
    {
      if (dgHandTiming.SelectedItem is HandTimingVMEntry selEntry)
      {
        uint startNumber = 0U;
        try { startNumber = uint.Parse(txtCalcStartNumber.Text); } catch (Exception) { }
        if (startNumber>0)
          _currentHandTimingVM.AssignStartNumber(selEntry, startNumber);
      }
    }

    private void btnCalcDissolve_Click(object sender, RoutedEventArgs e)
    {
      if (dgHandTiming.SelectedItem is HandTimingVMEntry selEntry)
      {
        _currentHandTimingVM.Dissolve(selEntry);
      }
    }

    private void btnCalc_Click(object sender, RoutedEventArgs e)
    {
      if (dgHandTiming.SelectedItem is HandTimingVMEntry selEntry)
      {
        HandTimingCalc calc = new HandTimingCalc(selEntry, _currentHandTimingVM.Items);

        HandTimingCalcReport report = new HandTimingCalcReport(calc, _race);

        Microsoft.Win32.SaveFileDialog openFileDialog = new Microsoft.Win32.SaveFileDialog();
        string filePath = report.ProposeFilePath();
        openFileDialog.FileName = System.IO.Path.GetFileName(filePath);
        openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(filePath);
        openFileDialog.DefaultExt = ".pdf";
        openFileDialog.Filter = "PDF documents (.pdf)|*.pdf";
        try
        {
          if (openFileDialog.ShowDialog() == true)
          {
            filePath = openFileDialog.FileName;
            report.Generate(filePath);
            System.Diagnostics.Process.Start(filePath);
          }
        }
        catch (Exception ex)
        {
          System.Windows.MessageBox.Show(
            "Datei " + System.IO.Path.GetFileName(filePath) + " konnte nicht gespeichert werden.\n\n" + ex.Message,
            "Fehler",
            System.Windows.MessageBoxButton.OK, MessageBoxImage.Exclamation);

        }

        selEntry.SetCalulatedHandTime(calc.CalculatedTime);
      }
    }

    private void btnDeviceDelete_Click(object sender, RoutedEventArgs e)
    {
      _currentHandTimingVM.DeleteHandTimings();
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
      _handTimingVMManager.SaveToDataModel();

      Finished?.Invoke(this, new EventArgs());
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      Finished?.Invoke(this, new EventArgs());
    }
  }
}
