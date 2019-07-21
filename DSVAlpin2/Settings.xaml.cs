using System;
using System.Collections.Generic;
using System.Configuration;
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
using System.Windows.Shapes;

namespace DSVAlpin2
{
  /// <summary>
  /// Interaction logic for Settings.xaml
  /// </summary>
  public partial class SettingsDlg : Window
  {
    public SettingsDlg()
    {
      InitializeComponent();

      FillCOMPorts(cbTimingDevicePort);

      cbTimingDevice.Items.Add("ALGE TdC8000/8001");
      cbTimingDevice.SelectedValue = ConfigurationManager.AppSettings.Get("TimingDevice.Type");
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
      var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      var settings = configFile.AppSettings.Settings;

      StoreSetting(settings, "TimingDevice.Type", (string)cbTimingDevice.SelectedValue);
      StoreSetting(settings, "TimingDevice.Port", (string)cbTimingDevicePort.SelectedValue);

      configFile.Save(ConfigurationSaveMode.Modified);
      ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);

      DialogResult = true;
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

      string cfgPort = ConfigurationManager.AppSettings.Get("TimingDevice.Port");
      combo.SelectedValue = cfgPort;
    }


    static public void StoreSetting(KeyValueConfigurationCollection settings, string key, string value)
    {
      if (settings[key] == null)
      {
        settings.Add(key, value);
      }
      else
      {
        settings[key].Value = value;
      }
    }

  }
}
