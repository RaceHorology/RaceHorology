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
using System.Windows.Shapes;

namespace RaceHorology
{

  public class TimingDeviceBaseUC : UserControl
  {
    public TimingDeviceBaseUC() : base()
    {
    }

    public virtual void Init(ILiveTimeMeasurementDeviceDebugInfo device) { }
    public virtual void Closes() { }
  }



  /// <summary>
  /// Interaction logic for TimingDeviceDlg.xaml
  /// </summary>
  public partial class TimingDeviceDlg : Window
  {
    private TimingDeviceBaseUC _deviceUC;
    public TimingDeviceDlg(TimingDeviceBaseUC deviceUC)
    {
      InitializeComponent();
      _deviceUC = deviceUC;
      grdMain.Children.Add(deviceUC);
      Grid.SetRow(deviceUC, 0);
      Grid.SetColumn(deviceUC, 0);

      Closing += TimingDeviceDlg_Closing;
    }

    private void TimingDeviceDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      _deviceUC.Closes();
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

  }
}
