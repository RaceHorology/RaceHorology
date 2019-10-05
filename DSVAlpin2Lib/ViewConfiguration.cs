using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{


  public class ViewFactory
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

      _prototypes["RaceRunResult"] = new RaceRunResultViewProvider();


    }


    public ViewProvider Create(string viewKey)
    {
      if (viewKey == null)
        return null;

      ViewProvider prototype;
      if (_prototypes.TryGetValue(viewKey, out prototype))
      {
        return prototype.Clone();
      }

      return null;
    }

    public T Create<T>(string viewKey) where T:ViewProvider
    {
      T instance = Create(viewKey) as T;
      return instance;
    }



  }



  /// <summary>
  /// Stores the View Configuration Parameter for a Race
  /// </summary>
  public class RaceConfiguration
  {
    public int Runs;

    public string DefaultGrouping;

    public string RaceResultView;
    public Dictionary<string, object> RaceResultViewParams;

    public string Run1_StartistView;
    public string Run1_StartistViewGrouping;
    public Dictionary<string, object> Run1_StartistViewParams;

    public string Run2_StartistView;
    public string Run2_StartistViewGrouping;
    public Dictionary<string, object> Run2_StartistViewParams;
  }



  public class RaceConfigurationPresets
  {
    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    string _directory;
    Dictionary<string, RaceConfiguration> _configurations;

    public RaceConfigurationPresets(string directory)
    {
      _configurations = new Dictionary<string, RaceConfiguration>();

      _directory = directory;

      // Ensures the directory exists
      System.IO.Directory.CreateDirectory(directory);

      LoadAllConfiguration();
    }


    public Dictionary<string, RaceConfiguration> GetConfigurations()
    {
      return _configurations;
    }


    public void SetConfigurations(string name, RaceConfiguration raceConfiguration)
    {
      WriteConfiguration(name, raceConfiguration);
      LoadAllConfiguration();
    }


    public void DeleteConfigurations(string name)
    {
      string filename = System.IO.Path.Combine(_directory, name + ".preset");
      System.IO.File.Delete(filename);
      LoadAllConfiguration();
    }


    void LoadAllConfiguration()
    {
      _configurations.Clear();

      var presetFiles = System.IO.Directory.GetFiles(_directory, "*.preset", System.IO.SearchOption.TopDirectoryOnly);
      foreach(var filename in presetFiles)
      {
        string name;
        RaceConfiguration raceConfiguration;
        if (LoadConfiguration(filename, out name, out raceConfiguration))
        {
          _configurations.Add(name, raceConfiguration);
        }
      }
    }


    void WriteConfiguration(string name, RaceConfiguration raceConfiguration)
    {
      string filename = System.IO.Path.Combine(_directory, name + ".preset");

      try
      {
        string configJSON = Newtonsoft.Json.JsonConvert.SerializeObject(raceConfiguration, Newtonsoft.Json.Formatting.Indented);

        System.IO.File.WriteAllText(filename, configJSON);
      }
      catch (Exception e)
      {
        logger.Info(e, "could not write race preset {name}", filename);
      }
    }


    bool LoadConfiguration(string filename, out string name, out RaceConfiguration raceConfiguration)
    {
      try
      {
        name = System.IO.Path.GetFileNameWithoutExtension(filename);

        string configJSON = System.IO.File.ReadAllText(filename);

        raceConfiguration = new RaceConfiguration();
        Newtonsoft.Json.JsonConvert.PopulateObject(configJSON, raceConfiguration);
      }
      catch (Exception e)
      {
        logger.Info(e, "could not load race preset {name}", filename);
        raceConfiguration = null;
        name = null;
        return false;
      }


      return true;
    }

  }


  public class ViewConfigurator
  {
    protected AppDataModel _dataModel;
    protected RaceConfiguration _config;

    public ViewConfigurator(AppDataModel dataModel)
    {
      _dataModel = dataModel;
    }


    public void ApplyNewConfig(RaceConfiguration config)
    {
      _config = config.Copy();
    }


    public StartListViewProvider GetStartlistViewProvider(RaceRun rr, string context = null)
    {
      ViewFactory factory = Singleton<ViewFactory>.Instance;

      StartListViewProvider slVP;

      // First Run
      if (1 == rr.Run)
      {
        FirstRunStartListViewProvider frslVP = factory.Create<FirstRunStartListViewProvider>(_config.Run1_StartistView);

        // Backup if nothing has been created
        if (frslVP == null)
          frslVP = new FirstRunStartListViewProvider();

        frslVP.SetDefaultGrouping(_config.Run1_StartistViewGrouping);

        frslVP.Init(rr.GetRace().GetParticipants());
        slVP = frslVP;
      }
      else
      // Second or later run
      {
        // Figure out previous run
        RaceRun rrPrevious = rr.GetRace().GetRuns().Where( r => r.Run == (rr.Run-  1U)).First();

        SecondRunStartListViewProvider srslVP = factory.Create<SecondRunStartListViewProvider>(_config.Run2_StartistView);

        if (srslVP == null)
          srslVP = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Ascending);

        srslVP.SetDefaultGrouping(_config.Run2_StartistViewGrouping);

        srslVP.Init(rrPrevious);
        slVP = srslVP;
      }

      return slVP;
    }


    public RaceRunResultViewProvider GetRaceRunResultViewProvider(RaceRun rr, string context = null)
    {
      ViewFactory factory = Singleton<ViewFactory>.Instance;

      RaceRunResultViewProvider rVP;

      rVP = factory.Create<RaceRunResultViewProvider>("RaceRunResult");

      if (rVP == null)
        rVP = new RaceRunResultViewProvider();

      rVP.SetDefaultGrouping(_config.DefaultGrouping);

      rVP.Init(rr, _dataModel);

      return rVP;
    }


    public RaceResultViewProvider GetRaceResultViewProvider(Race race, string context = null)
    {
      ViewFactory factory = Singleton<ViewFactory>.Instance;

      RaceResultViewProvider rVP;

      rVP = factory.Create<RaceResultViewProvider>(_config.RaceResultView);

      if (rVP == null)
        rVP = new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.BestRun);

      rVP.SetDefaultGrouping(_config.DefaultGrouping);

      rVP.Init(race, _dataModel);
      return rVP;
    }


    public void ConfigureRace(Race race)
    {
      ApplyNewConfig(race.RaceConfiguration);

      for(int i=0; i<race.GetMaxRun(); i++)
      {
        RaceRun rr = race.GetRun(i);

        StartListViewProvider slVP = GetStartlistViewProvider(rr);
        rr.SetStartListProvider(slVP);

        RaceRunResultViewProvider rrVP = GetRaceRunResultViewProvider(rr);
        rr.SetResultViewProvider(rrVP);
      }

      RaceResultViewProvider raceVP = GetRaceResultViewProvider(race);
      race.SetResultViewProvider(raceVP);
    }

  }
}