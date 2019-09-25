using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{


  class ViewFactory
  {
    protected Dictionary<string, ViewProvider> _prototypes;

    public ViewFactory()
    {
      _prototypes = new Dictionary<string, ViewProvider>();

      _prototypes["Startlist_1stRun_StartnumberAscending"] = new FirstRunStartListViewProvider();
      _prototypes["Startlist_1stRun_Points_15"] = new DSVFirstRunStartListViewProvider(15);
      _prototypes["Startlist_1stRun_Points_30"] = new DSVFirstRunStartListViewProvider(30);

      _prototypes["Startlist_2nd_StartnumberAscending"] = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Ascending);
      //_prototypes["Startlist_2nd_StartnumberAscending"] = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Ascending);
      _prototypes["Startlist_2nd_StartnumberDescending"] = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Descending);
      //_prototypes["Startlist_2nd_StartnumberDescending"] = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Descending);
      _prototypes["Startlist_2nd_PreviousRunOnlyWithResults"] = new BasedOnResultsFirstRunStartListViewProvider(15, false);
      _prototypes["Startlist_2nd_PreviousRunAlsoWithoutResults"] = new BasedOnResultsFirstRunStartListViewProvider(15, true);

      _prototypes["RaceResult_BestOfTwo"] = new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.BestRun);
      _prototypes["RaceResult_Sum"] = new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.Sum);

    }


    public ViewProvider Create(string viewKey)
    {
      ViewProvider prototype;
      if (_prototypes.TryGetValue(viewKey, out prototype))
      {
        return prototype.Clone();
      }

      return null;
    }


  }




  class ViewConfiguration
  {
  }
}
