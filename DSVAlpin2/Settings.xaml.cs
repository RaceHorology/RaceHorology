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
      cbTimingDevice.SelectedValue = Properties.Settings.Default.TimingDevice_Type;
      txtAutomaticNiZTimeout.Text = Properties.Settings.Default.AutomaticNiZTimeout.ToString();
      txtAutomaticNaSStarters.Text = Properties.Settings.Default.AutomaticNaSStarters.ToString();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
      Properties.Settings.Default.TimingDevice_Type = (string)cbTimingDevice.SelectedValue;
      Properties.Settings.Default.TimingDevice_Port = (string)cbTimingDevicePort.SelectedValue;
      try { Properties.Settings.Default.AutomaticNiZTimeout = int.Parse(txtAutomaticNiZTimeout.Text); } catch (Exception) { }
      try { Properties.Settings.Default.AutomaticNaSStarters = int.Parse(txtAutomaticNaSStarters.Text); } catch (Exception) { }

      Properties.Settings.Default.Save();

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

      string cfgPort = Properties.Settings.Default.TimingDevice_Port;
      combo.SelectedValue = cfgPort;
    }
  }
}
