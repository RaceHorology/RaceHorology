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

    public HandTimingUC()
    {
      InitializeComponent();

      fillComboDevices();
      fillComboStartFinish(cmbDeviceStartOrFinish);
      fillComboStartFinish(cmbCalcDeviceStartOrFinish);

      _comPorts = new COMPortViewModel();
      cmbDevicePort.ItemsSource = _comPorts.Items;
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
  }
}
