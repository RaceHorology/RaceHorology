using RaceHorologyLib;
using System.Collections.Generic;
using System.Windows.Controls;

namespace RaceHorology
{
  /// <summary>
  /// Parameters for start list 2nd run based on start numbers (StartlistViewConfig.Run2_StartnumberAscending / Run2_StartnumberDescending)
  /// </summary>
  public partial class StartlistStartnumberParamsUC : UserControl, IViewParamsUC
  {
    public StartlistStartnumberParamsUC()
    {
      InitializeComponent();
    }

    public void SetParams(Dictionary<string, object> viewParams)
    {
      chkAllowNonResults.IsChecked = StartlistViewConfig.GetBool(viewParams, StartlistViewConfig.ParamAllowNonResults, true);
    }

    public Dictionary<string, object> GetParams()
    {
      return new Dictionary<string, object>
      {
        { StartlistViewConfig.ParamAllowNonResults, chkAllowNonResults.IsChecked == true }
      };
    }
  }
}
