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
  /// Interaction logic for StdProgressUC.xaml
  /// </summary>
  public partial class StdProgressUC : UserControl
  {
    public StdProgressUC()
    {
      InitializeComponent();
    }

    Progress<StdProgress> _progress;
    public void Init(Progress<StdProgress> progress)
    {
      _progress = progress;

      _progress.ProgressChanged += onProgressChanged;
    }

    private void onProgressChanged(object source, StdProgress progress)
    {
      if (progress.Percentage < 0)
      {
        progressBar.IsIndeterminate = true;
        progressBar.Minimum = 0;
        progressBar.Maximum = 100;
        progressBar.Value = progress.Percentage;
      }
      else
      {
        progressBar.IsIndeterminate = false;
      }

      lblStatus.Content = progress.CurrentStatus;
    }
  }
}
