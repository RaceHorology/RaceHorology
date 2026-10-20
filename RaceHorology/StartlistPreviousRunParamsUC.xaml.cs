using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace RaceHorology
{
  /// <summary>
  /// Parameters for start list based on previous run results (StartlistViewConfig.Run2_PreviousRun / Run2_Cumulative)
  /// </summary>
  public partial class StartlistPreviousRunParamsUC : UserControl, IViewParamsUC
  {
    const string AllText = "alle";

    public StartlistPreviousRunParamsUC()
    {
      InitializeComponent();
    }

    public void SetParams(Dictionary<string, object> viewParams)
    {
      int reverseBestN = StartlistViewConfig.GetInt(viewParams, StartlistViewConfig.ParamReverseBestN, 0);

      cmbReverseBestN.Text = reverseBestN == int.MaxValue ? AllText : reverseBestN.ToString();
      chkAllowNonResults.IsChecked = StartlistViewConfig.GetBool(viewParams, StartlistViewConfig.ParamAllowNonResults, false);
    }

    public Dictionary<string, object> GetParams()
    {
      string text = cmbReverseBestN.Text.Trim();

      int reverseBestN;
      if (string.Equals(text, AllText, StringComparison.OrdinalIgnoreCase))
        reverseBestN = int.MaxValue;
      else
      {
        int.TryParse(text, out reverseBestN);
        reverseBestN = Math.Max(0, reverseBestN);
      }

      return new Dictionary<string, object>
      {
        { StartlistViewConfig.ParamReverseBestN, reverseBestN },
        { StartlistViewConfig.ParamAllowNonResults, chkAllowNonResults.IsChecked == true }
      };
    }
  }
}
