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
  /// Interaction logic for LiveTimingFISUC.xaml
  /// </summary>
  public partial class LiveTimingFISUC : UserControl
  {
    public LiveTimingFISUC()
    {
      InitializeComponent();
    }

    private void BtnStart_Click(object sender, RoutedEventArgs e)
    {


    private void ResetLiveTimningUI(RaceConfiguration cfg)
    {
      if (cfg.LivetimingParams == null)
        return;

      try
      {
        txtRaceCode.Text = cfg.LivetimingParams["FIS_RaceCode"];
        txtCategory.Text = cfg.LivetimingParams["FIS_Category"];
        txtPassword.Password = cfg.LivetimingParams["FIS_Pasword"];
        txtPort.Text = cfg.LivetimingParams["FIS_Port"];
      }
      catch (KeyNotFoundException) { }
    }


    private void StoreLiveTiming(ref RaceConfiguration cfg)
    {
      cfg.LivetimingParams = new Dictionary<string, string>();
      cfg.LivetimingParams["FIS_RaceCode"] = txtRaceCode.Text;
      cfg.LivetimingParams["FIS_Category"] = txtCategory.Text;
      cfg.LivetimingParams["FIS_Pasword"] = txtPassword.Password;
      cfg.LivetimingParams["FIS_Port"] = txtPort.Text;
    }


    }
  }
}
