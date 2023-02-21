/*
 *  Copyright (C) 2019 - 2023 by Sven Flossmann
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
 *  Race Horology ist Freie Software: Sie können es unter den Bedingungen
 *  der GNU Affero General Public License, wie von der Free Software Foundation,
 *  Version 3 der Lizenz oder (nach Ihrer Wahl) jeder neueren
 *  veröffentlichten Version, weiter verteilen und/oder modifizieren.
 *
 *  Race Horology wird in der Hoffnung, dass es nützlich sein wird, aber
 *  OHNE JEDE GEWÄHRLEISTUNG, bereitgestellt; sogar ohne die implizite
 *  Gewährleistung der MARKTFÄHIGKEIT oder EIGNUNG FÜR EINEN BESTIMMTEN ZWECK.
 *  Siehe die GNU Affero General Public License für weitere Details.
 *
 *  Sie sollten eine Kopie der GNU Affero General Public License zusammen mit diesem
 *  Programm erhalten haben. Wenn nicht, siehe <https://www.gnu.org/licenses/>.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  /// <summary>
  /// Stores the View Configuration Parameter for a Race
  /// </summary>
  public class RaceConfiguration
  {
    public enum EPenaltyMode { Off, BestPlusPercentage, BestPlusSeconds };
    public class PointCalcParams
    {
      public double ValueF;
      public double ValueA;
      public double ValueZ;
      public double MinimumPenalty;
      public double ValueCutOff;
    }


    public string Name;
    public CompetitionProperties.ECompetitionType? InternalDSVAlpinCompetitionTypeWrite;

    public int Runs;

    public string DefaultGrouping;

    public List<string> ActiveFields;

    public string RaceResultView;
    public Dictionary<string, object> RaceResultViewParams;

    public EPenaltyMode RaceResultView_PenaltyRuleMode;
    public double RaceResultView_PenaltyRuleCutOffValue;

    public string Run1_StartistView;
    public string Run1_StartistViewGrouping;
    public Dictionary<string, object> Run1_StartistViewParams;

    public string Run2_StartistView;
    public string Run2_StartistViewGrouping;
    public Dictionary<string, object> Run2_StartistViewParams;

    public Dictionary<string, string> LivetimingParams;

    public Dictionary<Race.ERaceType, PointCalcParams> PointCalcParamTemplate;


    public double ValueF;
    public double ValueA;
    public double ValueZ;
    public double MinimumPenalty;
    public double ValueCutOff;

    public RaceConfiguration()
    {
      ActiveFields = new List<string>();
      PointCalcParamTemplate = new Dictionary<Race.ERaceType, PointCalcParams>();
    }

    public RaceConfiguration(RaceConfiguration src)
    {
      Name = src.Name;
      InternalDSVAlpinCompetitionTypeWrite = src.InternalDSVAlpinCompetitionTypeWrite;

      Runs = src.Runs;
      DefaultGrouping = src.DefaultGrouping;
      ActiveFields = src.ActiveFields.Copy<List<string>>();
      RaceResultView = src.RaceResultView;
      RaceResultViewParams = src.RaceResultViewParams.Copy<Dictionary<string, object>>();

      RaceResultView_PenaltyRuleMode = src.RaceResultView_PenaltyRuleMode;
      RaceResultView_PenaltyRuleCutOffValue = src.RaceResultView_PenaltyRuleCutOffValue;

      Run1_StartistView = src.Run1_StartistView;
      Run1_StartistViewGrouping = src.Run1_StartistViewGrouping;
      Run1_StartistViewParams = src.Run1_StartistViewParams.Copy<Dictionary<string, object>>();

      Run2_StartistView = src.Run2_StartistView;
      Run2_StartistViewGrouping = src.Run2_StartistViewGrouping;
      Run2_StartistViewParams = src.Run2_StartistViewParams.Copy<Dictionary<string, object>>();

      LivetimingParams = src.LivetimingParams.Copy<Dictionary<string, string>>();

      ValueF = src.ValueF;
      ValueA = src.ValueA;
      ValueCutOff = src.ValueCutOff;
      MinimumPenalty = src.MinimumPenalty;
    }
  }

  public static class RaceConfigurationMerger
  {
    public static RaceConfiguration MainConfig(RaceConfiguration baseConfig, RaceConfiguration newConfig, Race.ERaceType? raceType)
    {
      RaceConfiguration mergedConfig = new RaceConfiguration(baseConfig);

      mergedConfig.Name = newConfig.Name;
      mergedConfig.Runs = newConfig.Runs;
      mergedConfig.DefaultGrouping = newConfig.DefaultGrouping;
      mergedConfig.ActiveFields = newConfig.ActiveFields.Copy<List<string>>();
      mergedConfig.RaceResultView = newConfig.RaceResultView;
      mergedConfig.RaceResultViewParams = newConfig.RaceResultViewParams.Copy<Dictionary<string, object>>();
      mergedConfig.RaceResultView_PenaltyRuleMode = newConfig.RaceResultView_PenaltyRuleMode;
      mergedConfig.RaceResultView_PenaltyRuleCutOffValue = newConfig.RaceResultView_PenaltyRuleCutOffValue;

      mergedConfig.Run1_StartistView = newConfig.Run1_StartistView;
      mergedConfig.Run1_StartistViewGrouping = newConfig.Run1_StartistViewGrouping;
      mergedConfig.Run1_StartistViewParams = newConfig.Run1_StartistViewParams.Copy<Dictionary<string, object>>();

      mergedConfig.Run2_StartistView = newConfig.Run2_StartistView;
      mergedConfig.Run2_StartistViewGrouping = newConfig.Run2_StartistViewGrouping;
      mergedConfig.Run2_StartistViewParams = newConfig.Run2_StartistViewParams.Copy<Dictionary<string, object>>();

      if (raceType != null && newConfig.PointCalcParamTemplate.ContainsKey((Race.ERaceType)raceType))
      {
        var pointTemplate = newConfig.PointCalcParamTemplate[(Race.ERaceType)raceType];
        mergedConfig.ValueF = pointTemplate.ValueF;
        mergedConfig.ValueA = pointTemplate.ValueA;
        mergedConfig.ValueZ = pointTemplate.ValueZ;
        mergedConfig.MinimumPenalty = pointTemplate.MinimumPenalty;
        mergedConfig.ValueCutOff = pointTemplate.ValueCutOff;
      }

      return mergedConfig;
    }
  }


  public static class RaceConfigurationCompare
  {
    public static bool MainConfig(RaceConfiguration config1, RaceConfiguration config2)
    {
      bool res = true;

      res &= config1.Runs == config2.Runs;

      res &= config1.DefaultGrouping == config2.DefaultGrouping;

      res &= !config1.ActiveFields.Except(config2.ActiveFields).Any();
      res &= !config2.ActiveFields.Except(config1.ActiveFields).Any();

      res &= config1.RaceResultView == config2.RaceResultView;

      res &= config1.RaceResultView_PenaltyRuleMode == config2.RaceResultView_PenaltyRuleMode;
      res &= config1.RaceResultView_PenaltyRuleCutOffValue == config2.RaceResultView_PenaltyRuleCutOffValue;

      res &= config1.Run1_StartistView == config2.Run1_StartistView;
      res &= config1.Run1_StartistViewGrouping == config2.Run1_StartistViewGrouping;

      res &= config1.Run2_StartistView == config2.Run2_StartistView;
      res &= config1.Run2_StartistViewGrouping == config2.Run2_StartistViewGrouping;

      return res;
    }
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

      loadAllConfiguration();
    }


    public Dictionary<string, RaceConfiguration> GetConfigurations()
    {
      return _configurations;
    }

    public RaceConfiguration GetConfiguration(string name)
    {
      if (name == null || !_configurations.ContainsKey(name))
        return null;

      return _configurations[name];
    }


    public void SaveConfiguration(string name, RaceConfiguration raceConfiguration)
    {
      char[] invalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();

      // Builds a string out of valid chars
      string saveName = new string(name.Where(ch => !invalidFileNameChars.Contains(ch)).ToArray());

      raceConfiguration.Name = name;

      writeConfiguration(saveName, raceConfiguration);
      loadAllConfiguration();
    }


    public void DeleteConfiguration(string name)
    {
      string filename = System.IO.Path.Combine(_directory, name + ".preset");
      System.IO.File.Delete(filename);
      loadAllConfiguration();
    }


    void loadAllConfiguration()
    {
      _configurations.Clear();

      var presetFiles = System.IO.Directory.GetFiles(_directory, "*.preset", System.IO.SearchOption.TopDirectoryOnly);
      foreach (var filename in presetFiles)
      {
        string prettyName;
        RaceConfiguration raceConfiguration;
        if (loadConfiguration(filename, out prettyName, out raceConfiguration))
        {
          var name = System.IO.Path.GetFileNameWithoutExtension(filename);
          _configurations.Add(name, raceConfiguration);
        }
      }
    }


    void writeConfiguration(string name, RaceConfiguration raceConfiguration)
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


    bool loadConfiguration(string filename, out string name, out RaceConfiguration raceConfiguration)
    {
      try
      {
        string configJSON = System.IO.File.ReadAllText(filename);

        raceConfiguration = new RaceConfiguration();
        Newtonsoft.Json.JsonConvert.PopulateObject(configJSON, raceConfiguration);

        if (string.IsNullOrEmpty(raceConfiguration.Name))
          raceConfiguration.Name = System.IO.Path.GetFileNameWithoutExtension(filename);

        name = raceConfiguration.Name;
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




}
