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
    }
    public TeamRaceResultConfig GetConfig()
    {
      var cfg = new TeamRaceResultConfig { Modus = PointOrTime.Time };
      try { cfg.Penalty_TimeInSeconds = double.Parse(txtPenaltyTime.Text); } catch (Exception) { }
      cfg.NumberOfMembersMax = cmbTeamSize.SelectedIndex + 2;
      cfg.Penalty_NumberOfMembersMinDifferentSex = cmbPenaltySex.SelectedIndex;
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
        cmbTeamSize.SelectedIndex = _config.NumberOfMembersMax - 2;
        cmbPenaltySex.SelectedIndex = _config.Penalty_NumberOfMembersMinDifferentSex;
        txtPenaltyTime.Text = string.Format("{0}", _config.Penalty_TimeInSeconds);
      }
    }
  }

}
