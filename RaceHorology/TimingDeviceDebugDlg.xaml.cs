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
  public partial class TimingDeviceDebugDlg : Window
  {
    ILiveTimeMeasurementDeviceDebugInfo _debugableTimingDevice;

    public TimingDeviceDebugDlg(ILiveTimeMeasurementDeviceDebugInfo debugableTimingDevice)
    {
      _debugableTimingDevice = debugableTimingDevice;
      InitializeComponent();

      connectToAlge();
      tbAlgeLog.Text = _debugableTimingDevice.GetProtocol();
      ensureNewLineAtEnd();

      Closing += onWindowClosing;
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void onWindowClosing(object sender, CancelEventArgs e)
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
      Application.Current.Dispatcher.Invoke(() =>
      {
        DateTime dt1 = DateTime.Now;
        tbAlgeLog.AppendText(dt1.ToString("dd.MM.yyyy HH:mm:ss.ffff") + " | " + message);
        ensureNewLineAtEnd();
        if (ckAutoScrollEnabled.IsChecked == true) {
          tbAlgeLog.CaretIndex = tbAlgeLog.Text.Length;
          tbAlgeLog.ScrollToEnd();
        }
      });
    }

    private void ensureNewLineAtEnd()
    {
      if (tbAlgeLog.Text.Length > 0 && !tbAlgeLog.Text.EndsWith("\n"))
      tbAlgeLog.AppendText("\n");
    }

    private void ckAutoScrollEnabled_Checked(object sender, RoutedEventArgs e)
    {
      tbAlgeLog.CaretIndex = tbAlgeLog.Text.Length;
      tbAlgeLog.ScrollToEnd();
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e)
    {
      tbAlgeLog.Clear();
    }
  }
}
