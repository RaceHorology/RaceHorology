using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
  public partial class TimingDeviceAlpenhundeUC : TimingDeviceBaseUC
  {
    private TimingDeviceAlpenhunde _timingDevice;
    public TimingDeviceAlpenhundeUC()
    {
      InitializeComponent();
    }

    public override void Init(ILiveTimeMeasurementDeviceDebugInfo timingDevice)
    {
      _timingDevice = timingDevice as TimingDeviceAlpenhunde;
      ucDebug.Init(timingDevice);


      DataContext = _timingDevice.SystemInfo;
    }
  }
}
