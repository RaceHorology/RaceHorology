/*
 *  Copyright (C) 2019 - 2026 by Sven Flossmann & Co-Authors (CREDITS.TXT)
 *
 *  This file is part of Race Horology.
 *
 *  Race Horology is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  any later version.
 *
 *  Race Horology is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with Race Horology.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  Diese Datei ist Teil von Race Horology.
 *
 *  Race Horology ist Freie Software: Sie k�nnen es unter den Bedingungen
 *  der GNU Affero General Public License, wie von der Free Software Foundation,
 *  Version 3 der Lizenz oder (nach Ihrer Wahl) jeder neueren
 *  ver�ffentlichten Version, weiter verteilen und/oder modifizieren.
 *
 *  Race Horology wird in der Hoffnung, dass es n�tzlich sein wird, aber
 *  OHNE JEDE GEW�HRLEISTUNG, bereitgestellt; sogar ohne die implizite
 *  Gew�hrleistung der MARKTF�HIGKEIT oder EIGNUNG F�R EINEN BESTIMMTEN ZWECK.
 *  Siehe die GNU Affero General Public License f�r weitere Details.
 *
 *  Sie sollten eine Kopie der GNU Affero General Public License zusammen mit diesem
 *  Programm erhalten haben. Wenn nicht, siehe <https://www.gnu.org/licenses/>.
 *
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace RaceHorologyLib
{

  /// <summary>
  /// Keys and parameters of the start list views (RaceConfiguration.Run1_StartistView / Run2_StartistView).
  ///
  /// Older configurations encoded the parameters within the key (e.g. "Startlist_2nd_PreviousRun_30_OnlyWithResults").
  /// Normalize() maps these legacy keys onto a base key plus parameters.
  /// </summary>
  public static class StartlistViewConfig
  {
    public const string Run1_StartnumberAscending = "Startlist_1stRun_StartnumberAscending";
    public const string Run1_StartnumberDescending = "Startlist_1stRun_StartnumberDescending";
    public const string Run1_Points = "Startlist_1stRun_Points";

    public const string Run2_StartnumberAscending = "Startlist_2nd_StartnumberAscending";
    public const string Run2_StartnumberDescending = "Startlist_2nd_StartnumberDescending";
    public const string Run2_PreviousRun = "Startlist_2nd_PreviousRun";
    /// <summary>Like Run2_PreviousRun, but based on the sum of all previous runs (Issue #475)</summary>
    public const string Run2_Cumulative = "Startlist_2nd_Cumulative";

    /// <summary>Run1_Points: number of participants (best points) whose start order is drawn</summary>
    public const string ParamDrawFirstN = "DrawFirstN";
    /// <summary>Run2_PreviousRun / Run2_Cumulative: number of best participants starting in reversed order (int.MaxValue = all)</summary>
    public const string ParamReverseBestN = "ReverseBestN";
    /// <summary>Run2_PreviousRun / Run2_Cumulative: participants without result are part of the start list as well</summary>
    public const string ParamAllowNonResults = "AllowNonResults";

    static readonly Regex _legacyRun1Points = new Regex(@"^Startlist_1stRun_Points_(\d+)$");
    static readonly Regex _legacyRun2BasedOnResults = new Regex(@"^Startlist_2nd_(PreviousRun|Cumulative)_(\d+|all)_(OnlyWithResults|AlsoWithoutResults)$");


    /// <summary>
    /// Maps legacy keys onto base key + parameters. The returned parameters are always a new (non-null) dictionary.
    /// </summary>
    public static void Normalize(string viewKey, Dictionary<string, object> viewParams, out string normKey, out Dictionary<string, object> normParams)
    {
      normKey = viewKey;
      normParams = viewParams != null ? new Dictionary<string, object>(viewParams) : new Dictionary<string, object>();

      if (viewKey == null)
        return;

      var m = _legacyRun1Points.Match(viewKey);
      if (m.Success)
      {
        normKey = Run1_Points;
        normParams[ParamDrawFirstN] = int.Parse(m.Groups[1].Value);
        return;
      }

      m = _legacyRun2BasedOnResults.Match(viewKey);
      if (m.Success)
      {
        normKey = m.Groups[1].Value == "Cumulative" ? Run2_Cumulative : Run2_PreviousRun;
        normParams[ParamReverseBestN] = m.Groups[2].Value == "all" ? int.MaxValue : int.Parse(m.Groups[2].Value);
        normParams[ParamAllowNonResults] = m.Groups[3].Value == "AlsoWithoutResults";
      }
    }


    /// <summary>
    /// Compares two view configurations taking legacy keys into account.
    /// </summary>
    public static bool AreEqual(string viewKey1, Dictionary<string, object> viewParams1, string viewKey2, Dictionary<string, object> viewParams2)
    {
      Normalize(viewKey1, viewParams1, out string key1, out var params1);
      Normalize(viewKey2, viewParams2, out string key2, out var params2);

      if (key1 != key2 || params1.Count != params2.Count)
        return false;

      // Value types might differ (e.g. int vs. long after JSON deserialization)
      string toString(object o) => Convert.ToString(o, CultureInfo.InvariantCulture);
      foreach (var p in params1)
        if (!params2.TryGetValue(p.Key, out object v2) || toString(p.Value) != toString(v2))
          return false;

      return true;
    }


    public static int GetInt(Dictionary<string, object> viewParams, string name, int defaultValue)
    {
      if (viewParams != null && viewParams.TryGetValue(name, out object value) && value != null)
        try { return Convert.ToInt32(value, CultureInfo.InvariantCulture); } catch (Exception) { }
      return defaultValue;
    }

    public static bool GetBool(Dictionary<string, object> viewParams, string name, bool defaultValue)
    {
      if (viewParams != null && viewParams.TryGetValue(name, out object value) && value != null)
        try { return Convert.ToBoolean(value, CultureInfo.InvariantCulture); } catch (Exception) { }
      return defaultValue;
    }
  }


  internal class ViewFactory
  {
    protected Dictionary<string, Func<Dictionary<string, object>, ViewProvider>> _creators;

    public ViewFactory()
    {
      _creators = new Dictionary<string, Func<Dictionary<string, object>, ViewProvider>>();

      _creators[StartlistViewConfig.Run1_StartnumberAscending] = p => new FirstRunStartListViewProvider();
      _creators[StartlistViewConfig.Run1_StartnumberDescending] = p => new FirstRunStartListViewProvider(StartListEntryComparer.Direction.Descending);
      _creators[StartlistViewConfig.Run1_Points] = p => new DSVFirstRunStartListViewProvider(
        StartlistViewConfig.GetInt(p, StartlistViewConfig.ParamDrawFirstN, 0));

      _creators[StartlistViewConfig.Run2_StartnumberAscending] = p => new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Ascending);
      _creators[StartlistViewConfig.Run2_StartnumberDescending] = p => new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Descending);
      _creators[StartlistViewConfig.Run2_PreviousRun] = p => new BasedOnResultsFirstRunStartListViewProvider(
        StartlistViewConfig.GetInt(p, StartlistViewConfig.ParamReverseBestN, 0),
        StartlistViewConfig.GetBool(p, StartlistViewConfig.ParamAllowNonResults, false));
      _creators[StartlistViewConfig.Run2_Cumulative] = p => new BasedOnResultsFirstRunStartListViewProvider(
        StartlistViewConfig.GetInt(p, StartlistViewConfig.ParamReverseBestN, 0),
        StartlistViewConfig.GetBool(p, StartlistViewConfig.ParamAllowNonResults, false),
        useCumulativeResults: true);

      _creators["RaceResult_BestOfTwo"] = p => new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.BestRun);
      _creators["RaceResult_Sum"] = p => new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.Sum);
      _creators["RaceResult_SumBest2"] = p => new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.SumBest2);
      _creators["RaceResult_2xBestOf2"] = p => new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.SumMxBestOfN);
      _creators["RaceResult_SumDSVPointsSchool"] = p => new DSVSchoolRaceResultViewProvider();
      _creators["RaceResult_SumFISPoints"] = p => new FISRaceResultViewProvider();
      _creators["RaceResult_SumPointsViaTable"] = p => new PointsViaTableRaceResultViewProvider(PointsViaTableRaceResultViewProvider.EMode.ApplyPointsTotally);
      _creators["RaceResult_SumPointsViaTablePerRun"] = p => new PointsViaTableRaceResultViewProvider(PointsViaTableRaceResultViewProvider.EMode.ApplyPointsPerRun);
    }


    public ViewProvider Create(string viewKey, Dictionary<string, object> viewParams = null)
    {
      if (viewKey == null)
        return null;

      StartlistViewConfig.Normalize(viewKey, viewParams, out string key, out var normParams);

      if (_creators.TryGetValue(key, out var creator))
        return creator(normParams);

      return null;
    }

    public T Create<T>(string viewKey, Dictionary<string, object> viewParams = null) where T : ViewProvider
    {
      T instance = Create(viewKey, viewParams) as T;
      return instance;
    }



  }


  public class ViewConfigurator
  {
    protected Race _race;
    protected AppDataModel _dataModel;
    protected RaceConfiguration _config;

    public ViewConfigurator(Race race)
    {
      _race = race;
      _dataModel = race.GetDataModel();
      _config = race.RaceConfiguration.Copy();
    }


    public StartListViewProvider GetStartlistViewProvider(RaceRun rr, string context = null)
    {
      ViewFactory factory = Singleton<ViewFactory>.Instance;

      StartListViewProvider slVP;

      // First Run
      if (1 == rr.Run)
      {
        FirstRunStartListViewProvider frslVP = factory.Create<FirstRunStartListViewProvider>(_config.Run1_StartistView, _config.Run1_StartistViewParams);

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
        RaceRun rrPrevious = rr.GetRace().GetRuns().Where(r => r.Run == (rr.Run - 1U)).First();

        SecondRunStartListViewProvider srslVP = factory.Create<SecondRunStartListViewProvider>(_config.Run2_StartistView, _config.Run2_StartistViewParams);

        if (srslVP == null)
          srslVP = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Ascending);

        srslVP.SetDefaultGrouping(_config.Run2_StartistViewGrouping);

        srslVP.Init(rrPrevious);
        slVP = srslVP;
      }

      return slVP;
    }


    public RemainingStartListViewProvider GetRemainingStartersViewProvider(RaceRun rr, string context = null)
    {
      var slVP = GetStartlistViewProvider(rr, context);

      if (slVP == null)
        return null;

      var rslVP = new RemainingStartListViewProvider();
      rslVP.Init(slVP, rr);

      return rslVP;
    }


    public RaceRunResultViewProvider GetRaceRunResultViewProvider(RaceRun rr, string context = null)
    {
      ViewFactory factory = Singleton<ViewFactory>.Instance;

      RaceRunResultViewProvider rVP;

      if (_config.RaceResultView_PenaltyRuleMode != RaceConfiguration.EPenaltyMode.Off)
      {
        if (_config.RaceResultView_PenaltyRuleMode == RaceConfiguration.EPenaltyMode.BestPlusPercentage)
          rVP = new PenaltyRaceRunResultViewProvider(PenaltyRaceRunResultViewProvider.EMode.BestPlusPercentage, _config.RaceResultView_PenaltyRuleCutOffValue);
        else
          rVP = new PenaltyRaceRunResultViewProvider(PenaltyRaceRunResultViewProvider.EMode.BestPlusSeconds, _config.RaceResultView_PenaltyRuleCutOffValue);
      }
      else
        rVP = new RaceRunResultViewProvider();

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
      for (int i = 0; i < race.GetMaxRun(); i++)
      {
        RaceRun rr = race.GetRun(i);

        StartListViewProvider slVP = GetStartlistViewProvider(rr);
        rr.SetStartListProvider(slVP);

        RaceRunResultViewProvider rrVP = GetRaceRunResultViewProvider(rr);
        rr.SetResultViewProvider(rrVP);
      }

      RaceResultViewProvider raceVP = GetRaceResultViewProvider(race);
      race.SetResultViewProvider(raceVP);

      if (_config.TeamRaceResultConfig != null)
      {
        var teamVP = new TeamRaceResultViewProvider(_config.TeamRaceResultConfig);
        teamVP.Init(race, _dataModel);
        race.SetTeamResultsViewProvider(teamVP);
      }
      else
      {
        race.SetTeamResultsViewProvider(null);
      }

    }

  }
}
