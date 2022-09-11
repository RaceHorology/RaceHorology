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
  /// Interaction logic for QRCodeDlg.xaml
  /// </summary>
  public partial class QRCodeDlg : Window
  {
    public QRCodeDlg()
    {
      InitializeComponent();
    }

    private void onClickQRCode(object sender, MouseButtonEventArgs e)
    {
      //System.Diagnostics.Process.Start(_alpinServer.GetUrl());

    }
  }
}
