using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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


// Protokoll Fenster auf aktuellen letzten Eintrag setzen

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for ALGEDebugDlg.xaml
  /// </summary>
  public partial class TimingDeviceDebugUC : TimingDeviceBaseUC
  {
    ILiveTimeMeasurementDeviceDebugInfo _debugableTimingDevice;

    public TimingDeviceDebugUC()
    {
      InitializeComponent();
    }

    public override void Init(ILiveTimeMeasurementDeviceDebugInfo timingDevice)
    {
      _debugableTimingDevice = timingDevice;

      connectToAlge();
      string protocol = _debugableTimingDevice.GetProtocol();
      tbAlgeLog.Text = string.IsNullOrEmpty(protocol) ? "" : protocol + "\n";
      scrollToEndIfEnabled();
    }

    public override void Closes()
    {
      disconnectFromAlge();
    }

    private void connectToAlge()
    {
      _debugableTimingDevice.RawMessageReceived += Alge_OnMessageReceived;
    }

    private void disconnectFromAlge()
    {
      _debugableTimingDevice.RawMessageReceived -= Alge_OnMessageReceived;
    }

    private void Alge_OnMessageReceived(object sender, string message)
    {
      // message is one protocol line (incl. timestamp)
      Application.Current.Dispatcher.Invoke(() =>
      {
        // AppendText instead of Text += to avoid copying the whole (possibly long) protocol per message
        tbAlgeLog.AppendText(message + "\n");
        scrollToEndIfEnabled();
      });
    }

    private void scrollToEndIfEnabled()
    {
      if (chkAutoScroll.IsChecked == true)
        tbAlgeLog.ScrollToEnd();
    }

    private void chkAutoScroll_Checked(object sender, RoutedEventArgs e)
    {
      tbAlgeLog.ScrollToEnd();
    }

    private void btnClear_Click(object sender, RoutedEventArgs e)
    {
      _debugableTimingDevice?.ClearProtocol();
      tbAlgeLog.Clear();
    }

  }
}
