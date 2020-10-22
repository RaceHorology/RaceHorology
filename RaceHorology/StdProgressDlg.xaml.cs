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
  /// Interaction logic for StdProgressDlg.xaml
  /// </summary>
  public partial class StdProgressDlg : Window
  {
    public StdProgressDlg()
    {
      InitializeComponent();
    }

    public void ShowAndClose(Progress<StdProgress> progress)
    {
      progressUC.Init(progress);

      progress.ProgressChanged += Progress_ProgressChanged;

      Show();
    }

    private void Progress_ProgressChanged(object sender, StdProgress e)
    {
      if (e.Finished)
        Close();
    }
  }
}
