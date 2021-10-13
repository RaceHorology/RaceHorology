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

namespace RaceHorology
{
  /// <summary>
  /// Interaction logic for ALGEDebugDlg.xaml
  /// </summary>
  public partial class ALGEDebugDlg : Window
  {
    ALGETdC8001TimeMeasurement _algeDevice;

    public ALGEDebugDlg(ALGETdC8001TimeMeasurement algeDevice)
    {
      _algeDevice = algeDevice;
      InitializeComponent();

      connectToAlge();

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
      _algeDevice.RawMessageReceived += Alge_OnMessageReceived;
    }

    private void disconnectFromAlge()
    {
      _algeDevice.RawMessageReceived -= Alge_OnMessageReceived;
    }

    private void Alge_OnMessageReceived(object sender, string message)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        tbAlgeLog.Text += message + "\n";
        tbAlgeLog.ScrollToEnd();
      });
    }

  }
}
