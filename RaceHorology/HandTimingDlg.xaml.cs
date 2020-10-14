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
  /// <summary>
  /// Interaction logic for HandTimingDlg.xaml
  /// </summary>
  public partial class HandTimingDlg : Window
  {
    public HandTimingDlg()
    {
      InitializeComponent();
    }

    public void Init(AppDataModel dm, Race race)
    {
      ucHandTiming.Init(dm, race);

      ucHandTiming.Finished += UcHandTiming_Finished;
    }

    private void UcHandTiming_Finished(object sender, EventArgs e)
    {
      this.Close();
    }
  }
}
