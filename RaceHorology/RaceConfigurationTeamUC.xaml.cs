using CefSharp.DevTools.CSS;
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
  /// Interaction logic for RaceConfigurationTeamUC.xaml
  /// </summary>
  public partial class RaceConfigurationTeamUC : UserControl
  {
    TeamRaceResultConfig _config;
    public RaceConfigurationTeamUC()
    {
      InitializeComponent();

      for (int i = 2; i < 10; i++)
        cmbTeamSize.Items.Add(new CBItem { Text = i.ToString(), Value = i });

      for (int i = 0;i<6;i++)
        cmbPenaltySex.Items.Add(new CBItem { Text = i==0?"keine":i.ToString(), Value = i });
    }
    public TeamRaceResultConfig GetConfig()
    {
      var cfg = new TeamRaceResultConfig { Modus = PointOrTime.Time };
      try { cfg.Penalty_TimeInSeconds = double.Parse(txtPenaltyTime.Text); } catch (Exception) { }

      if (cmbTeamSize.SelectedItem is CBItem selectedSize)
        cfg.NumberOfMembersMax = (int) selectedSize.Value;

      if (cmbPenaltySex.SelectedItem is CBItem selectedSex)
        cfg.Penalty_NumberOfMembersMinDifferentSex = (int)selectedSex.Value;
      _config = cfg;
      return _config;
    }
    public void SetConfig(TeamRaceResultConfig config)
    {
      _config = config;
      if (_config != null)
      {
        if (_config.Modus == PointOrTime.Time)
          cmbMode.SelectedIndex = 0;
        cmbTeamSize.SelectCBItem(_config.NumberOfMembersMax);
        cmbPenaltySex.SelectCBItem(_config.Penalty_NumberOfMembersMinDifferentSex);
        txtPenaltyTime.Text = string.Format("{0}", _config.Penalty_TimeInSeconds);
      }
    }
  }

}
