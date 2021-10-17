/*
 *  Copyright (C) 2019 - 2021 by Sven Flossmann
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for RaceConfigurationTest
  /// </summary>
  [TestClass]
  public class RaceConfigurationTest
  {
    public RaceConfigurationTest()
    {
    }

    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get
      {
        return testContextInstance;
      }
      set
      {
        testContextInstance = value;
      }
    }

    #region Additional test attributes
    //
    // You can use the following additional attributes as you write your tests:
    //
    // Use ClassInitialize to run code before running the first test in the class
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
    //
    // Use ClassCleanup to run code after all tests in a class have run
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    // Use TestInitialize to run code before running each test 
    // [TestInitialize()]
    // public void MyTestInitialize() { }
    //
    // Use TestCleanup to run code after each test has run
    // [TestCleanup()]
    // public void MyTestCleanup() { }
    //
    #endregion

    [TestMethod]
    public void RaceConfigurationMerger_MainConfig()
    {
      RaceConfiguration baseConfig = new RaceConfiguration
      {
        Name = "BaseName",
        Runs = 2,
        DefaultGrouping = "DefaultG",
        ActiveFields = new List<string> { "eins", "zwei" },
        RaceResultView = "RaceResultView",
        RaceResultViewParams = new Dictionary<string, object>(),

        Run1_StartistView = "Run1_StartistView",
        Run1_StartistViewGrouping = "Run1_StartistViewGrouping",
        Run1_StartistViewParams = new Dictionary<string, object>(),

        Run2_StartistView = "Run2_StartistView",
        Run2_StartistViewGrouping = "Run2_StartistViewGrouping",
        Run2_StartistViewParams = new Dictionary<string, object>(),

        LivetimingParams = new Dictionary<string, string> { { "key", "value" } },

        ValueF = 100,
        ValueA = 200,
        MinimumPenalty = 300
      };

      RaceConfiguration newConfig = new RaceConfiguration
      {
        Name = "NewName",
        Runs = 3,
        DefaultGrouping = "DefaultH",
        ActiveFields = new List<string> { "drei", "view" },
        RaceResultView = "RaceResultView2",
        RaceResultViewParams = new Dictionary<string, object>(),

        Run1_StartistView = "Run1_StartistView2",
        Run1_StartistViewGrouping = "Run1_StartistViewGrouping2",
        Run1_StartistViewParams = new Dictionary<string, object>(),

        Run2_StartistView = "Run2_StartistView2",
        Run2_StartistViewGrouping = "Run2_StartistViewGrouping2",
        Run2_StartistViewParams = new Dictionary<string, object>(),

        LivetimingParams = new Dictionary<string, string> { { "key2", "value2" } },

        ValueF = 200,
        ValueA = 300,
        MinimumPenalty = 400
      };


      RaceConfiguration mergedConfig = RaceConfigurationMerger.MainConfig(baseConfig, newConfig);

      Assert.AreEqual("NewName", mergedConfig.Name);

      Assert.AreEqual("DefaultH", mergedConfig.DefaultGrouping);
      Assert.AreEqual("drei", mergedConfig.ActiveFields[0]);
      Assert.AreEqual("view", mergedConfig.ActiveFields[1]);
      Assert.AreEqual("RaceResultView2", mergedConfig.RaceResultView);
      Assert.AreEqual(0, mergedConfig.RaceResultViewParams.Count);

      Assert.AreEqual("Run1_StartistView2", mergedConfig.Run1_StartistView);
      Assert.AreEqual("Run1_StartistViewGrouping2", mergedConfig.Run1_StartistViewGrouping);
      Assert.AreEqual(0, mergedConfig.Run1_StartistViewParams.Count);
      Assert.AreEqual("Run2_StartistView2", mergedConfig.Run2_StartistView);
      Assert.AreEqual("Run2_StartistViewGrouping2", mergedConfig.Run2_StartistViewGrouping);
      Assert.AreEqual(0, mergedConfig.Run2_StartistViewParams.Count);

      // After this line, there must be the old values
      Assert.AreEqual(1, mergedConfig.LivetimingParams.Count);
      Assert.AreEqual("value", mergedConfig.LivetimingParams["key"]);
      Assert.AreEqual(100, mergedConfig.ValueF);
      Assert.AreEqual(200, mergedConfig.ValueA);
      Assert.AreEqual(300, mergedConfig.MinimumPenalty);
    }

    [TestMethod]
    [DeploymentItem(@"raceconfigpresets\DSV Erwachsene.preset")]
    [DeploymentItem(@"raceconfigpresets\FIS Rennen.preset")]
    public void RaceConfigurationPresets_Test()
    {
      RaceConfigurationPresets cfgPresets = new RaceConfigurationPresets(".");

      var configs = cfgPresets.GetConfigurations();

      Assert.AreEqual(2, configs.Count);
      Assert.IsTrue(configs.ContainsKey("DSV Erwachsene"));
      Assert.IsTrue(configs.ContainsKey("FIS Rennen"));

      // Create new Config
      var newConfig = new RaceConfiguration(configs["FIS Rennen"]);
      newConfig.Runs = 3;
      cfgPresets.SaveConfiguration("FIS Rennen - neu", newConfig);
      Assert.AreEqual(3, configs.Count);
      Assert.IsTrue(configs.ContainsKey("DSV Erwachsene"));
      Assert.IsTrue(configs.ContainsKey("FIS Rennen"));
      Assert.IsTrue(configs.ContainsKey("FIS Rennen - neu"));

      // Delete a config
      cfgPresets.DeleteConfiguration("FIS Rennen");
      Assert.AreEqual(2, configs.Count);
      Assert.IsTrue(configs.ContainsKey("DSV Erwachsene"));
      Assert.IsTrue(configs.ContainsKey("FIS Rennen - neu"));
      Assert.AreEqual(3, cfgPresets.GetConfigurations()["FIS Rennen - neu"].Runs);

      // Create new Config with unsafe name
      var newConfig2 = new RaceConfiguration(configs["DSV Erwachsene"]);
      newConfig.Runs = 3;
      cfgPresets.SaveConfiguration(@"abc\*:;? 123", newConfig);
      Assert.AreEqual(3, configs.Count);
      Assert.IsTrue(configs.ContainsKey("DSV Erwachsene"));
      Assert.IsTrue(configs.ContainsKey("abc; 123"));
      Assert.IsTrue(configs.ContainsKey("FIS Rennen - neu"));

    }
  }
}
