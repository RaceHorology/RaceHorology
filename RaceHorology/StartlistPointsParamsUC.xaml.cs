using RaceHorologyLib;
using System.Collections.Generic;
using System.Windows.Controls;

namespace RaceHorology
{
  /// <summary>
  /// Parameters for start list 1st run based on points (StartlistViewConfig.Run1_Points)
  /// </summary>
  public partial class StartlistPointsParamsUC : UserControl, IViewParamsUC
  {
    public StartlistPointsParamsUC()
    {
      InitializeComponent();
    }

    public void SetParams(Dictionary<string, object> viewParams)
    {
      cmbDrawFirstN.Text = StartlistViewConfig.GetInt(viewParams, StartlistViewConfig.ParamDrawFirstN, 0).ToString();
    }

    public Dictionary<string, object> GetParams()
    {
      int.TryParse(cmbDrawFirstN.Text.Trim(), out int drawFirstN);

      return new Dictionary<string, object>
      {
        { StartlistViewConfig.ParamDrawFirstN, System.Math.Max(0, drawFirstN) }
      };
    }
  }
}
