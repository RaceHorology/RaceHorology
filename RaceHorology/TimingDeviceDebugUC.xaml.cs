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

    public TimingDeviceDebugUC(ILiveTimeMeasurementDeviceDebugInfo debugableTimingDevice)
    {
      _debugableTimingDevice = debugableTimingDevice;
      InitializeComponent();

      connectToAlge();
      tbAlgeLog.Text = _debugableTimingDevice.GetProtocol();
      ensureNewLineAtEnd();
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
      Application.Current.Dispatcher.Invoke(() =>
      {
        tbAlgeLog.Text += message;
        ensureNewLineAtEnd();
        tbAlgeLog.ScrollToEnd();
      });
    }

    private void ensureNewLineAtEnd()
    {
      if (tbAlgeLog.Text.Length > 0 && !tbAlgeLog.Text.EndsWith("\n"))
        tbAlgeLog.Text += "\n";
    }

  }
}
