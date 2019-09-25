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



  /// <summary>
  /// Stores the View Configuration Parameter for a Race
  /// </summary>
  public class ViewConfiguration
  {
    public int Runs;

    public string DefaultGrouping;

    public string RaceResultView;
    public Dictionary<string,object> RaceResultViewParams;

    public string Run1_StartistView;
    public Dictionary<string, object> Run1_StartistViewParams;

    public string Run2_StartistView;
    public Dictionary<string, object> Run2_StartistViewParams;

  }



  /*
   * Wie bekommen die existierenden Views eine Änderung der Konfiguration mit?
   * a) beim GetView ... einen Callback registrieren
   * b) beim GetView ... in einen Container zurückgeben
   * c) fire Event => WPF UI oder HTML5 UI baut sich neu auf
   * 
   * Wie wird eine Configänderung in das AppDataModel zurückgespielt?
   * 
   * Sollen die Views Teil des AppDataModels sein?
   * Nein:
   * - ViewConfigurator kennt Abhängigkeiten
   * 
   * Ja:
   * - AppDataModel kann einfach Accesoren bereitstellen
   */

  public class ViewConfigurator
  {
    public ViewProvider GetStartlistViewProvider(string context = null)
    {
      throw new NotImplementedException();
    }

    public ViewProvider GetRaceRunResultViewProvider(string context = null)
    {
      throw new NotImplementedException();
    }

    public ViewProvider GetRaceResultViewProvider(string context = null)
    {
      throw new NotImplementedException();
    }

    public void ConfigureAppDataModel(AppDataModel model)
    {
      throw new NotImplementedException();
    }

  }
}
