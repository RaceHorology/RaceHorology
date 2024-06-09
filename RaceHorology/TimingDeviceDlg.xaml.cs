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
  /// <summary>
  /// Interaction logic for TimingDeviceDlg.xaml
  /// </summary>
  public partial class TimingDeviceDlg : Window
  {
    public TimingDeviceDlg(UserControl deviceUC)
    {
      InitializeComponent();
      grdMain.Children.Add(deviceUC);
      Grid.SetRow(deviceUC, 0);
      Grid.SetColumn(deviceUC, 0);
    }
  }
}
