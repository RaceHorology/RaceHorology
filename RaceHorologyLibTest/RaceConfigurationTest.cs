/*
 *  Copyright (C) 2019 - 2022 by Sven Flossmann
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
    public void RaceConfigurationCompare_MainConfig()
    {
      RaceConfiguration config1 = new RaceConfiguration
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

      RaceConfiguration config2 = new RaceConfiguration(config1);

      Assert.IsTrue(RaceConfigurationCompare.MainConfig(config1, config2));
      config1.Runs = 3;
      Assert.IsFalse(RaceConfigurationCompare.MainConfig(config1, config2));
      config1.Runs = 2;
      Assert.IsTrue(RaceConfigurationCompare.MainConfig(config1, config2));
      config2.DefaultGrouping = "DefaultC";
      Assert.IsFalse(RaceConfigurationCompare.MainConfig(config1, config2));
      config2.DefaultGrouping = "DefaultG";
      Assert.IsTrue(RaceConfigurationCompare.MainConfig(config1, config2));
      
      config2.ActiveFields = new List<string> { "eins" };
      Assert.IsFalse(RaceConfigurationCompare.MainConfig(config1, config2));
      config2.ActiveFields = new List<string> { "eins", "zwei" };
      Assert.IsTrue(RaceConfigurationCompare.MainConfig(config1, config2));
      config1.ActiveFields = new List<string> { "eins" };
      Assert.IsFalse(RaceConfigurationCompare.MainConfig(config1, config2));
      config1.ActiveFields = new List<string> { "eins", "zwei" };
      Assert.IsTrue(RaceConfigurationCompare.MainConfig(config1, config2));

      config1.RaceResultView = "RaceResultView1";
      Assert.IsFalse(RaceConfigurationCompare.MainConfig(config1, config2));
      config1.RaceResultView = "RaceResultView";
      Assert.IsTrue(RaceConfigurationCompare.MainConfig(config1, config2));
    }

    [TestMethod]
    [DeploymentItem(@"raceconfigpresets\DSV Erwachsene.preset")]
    [DeploymentItem(@"raceconfigpresets\FIS Rennen Men.preset")]
    public void RaceConfigurationPresets_Test()
    {
      RaceConfigurationPresets cfgPresets = new RaceConfigurationPresets(".");

      var configs = cfgPresets.GetConfigurations();

      Assert.AreEqual(2, configs.Count);
      Assert.IsTrue(configs.ContainsKey("DSV Erwachsene"));
      Assert.IsTrue(configs.ContainsKey("FIS Rennen Men"));

      // Create new Config
      var newConfig = new RaceConfiguration(configs["FIS Rennen Men"]);
      newConfig.Runs = 3;
      cfgPresets.SaveConfiguration("FIS Rennen - neu", newConfig);
      Assert.AreEqual(3, configs.Count);
      Assert.IsTrue(configs.ContainsKey("DSV Erwachsene"));
      Assert.IsTrue(configs.ContainsKey("FIS Rennen Men"));
      Assert.IsTrue(configs.ContainsKey("FIS Rennen - neu"));

      // Delete a config
      cfgPresets.DeleteConfiguration("FIS Rennen Men");
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
      Assert.IsTrue(configs.ContainsKey(@"abc; 123"));
      Assert.IsTrue(configs.ContainsKey("FIS Rennen - neu"));

    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_Empty.mdb")]
    [DeploymentItem(@"raceconfigpresets\Vereinsrennen - BestOfTwo.preset", @"raceconfigpresets")]
    public void GlobalRaceConfig_SaveAndLoad()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_Empty.mdb");

      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      AppDataModel model = new AppDataModel(db);

      // Default Config
      Assert.AreEqual("Vereinsrennen - BestOfTwo", model.GlobalRaceConfig.Name);

      var testConfig1 = new RaceConfiguration
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

      // Check for PropertyChanged event
      string propertyChanged = null;
      model.PropertyChanged += delegate (object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
        propertyChanged = e.PropertyName;
      };

      model.GlobalRaceConfig = testConfig1;
      Assert.AreEqual("GlobalRaceConfig", propertyChanged);

      TestUtilities.AreEqualByJson(testConfig1, model.GlobalRaceConfig);
      model.Close();

      // Check saving and loading from in DB
      RaceHorologyLib.Database db2 = new RaceHorologyLib.Database();
      db2.Connect(dbFilename);
      AppDataModel model2 = new AppDataModel(db2);
      TestUtilities.AreEqualByJson(testConfig1, model2.GlobalRaceConfig);
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_Empty.mdb")]
    public void GlobalRaceConfig_InheritToRace()
    {
      TestDataGenerator tg = new TestDataGenerator();
      var model = tg.Model;

      var testConfig1 = new RaceConfiguration
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

      model.GlobalRaceConfig = testConfig1;

      TestUtilities.AreEqualByJson(testConfig1, model.GlobalRaceConfig);
      TestUtilities.AreEqualByJson(testConfig1, model.GetRace(0).RaceConfiguration);

    }


    [TestMethod]
    [DeploymentItem(@"raceconfigpresets\DSV Erwachsene.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\DSV Schüler U14-U16.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\FIS Rennen Men.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\FIS Rennen Women.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\Inline (allgemein).preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\Inline (Punkte).preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\KSC Ebersberg -U12.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\KSC Ebersberg U14-.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\SVM Schüler U12.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\SVM Schüler U8-U10.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\Vereinsrennen - BestOfTwo.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\Vereinsrennen - Summe.preset", @"raceconfigpresets")]
    [DeploymentItem(@"TestDataBases\TestDB_Empty.mdb")]
    public void GlobalRaceConfig_DSVAlpinImport()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_Empty.mdb");

      AppDataModel configDBAndGetModel(CompetitionProperties.ECompetitionType type)
      {
        RaceHorologyLib.Database db = new RaceHorologyLib.Database();
        db.Connect(dbFilename);

        var compProp = db.GetCompetitionProperties();
        compProp.Type = type;
        db.UpdateCompetitionProperties(compProp);

        return new AppDataModel(db);
      }

      AppDataModel model;

      model = configDBAndGetModel(CompetitionProperties.ECompetitionType.FIS_Women);
      Assert.AreEqual("FIS Rennen - Damen", model.GlobalRaceConfig.Name);
      model = configDBAndGetModel(CompetitionProperties.ECompetitionType.FIS_Men);
      Assert.AreEqual("FIS Rennen - Herren", model.GlobalRaceConfig.Name);
      model = configDBAndGetModel(CompetitionProperties.ECompetitionType.DSV_Points);
      Assert.AreEqual("DSV Erwachsene", model.GlobalRaceConfig.Name);
      model = configDBAndGetModel(CompetitionProperties.ECompetitionType.DSV_NoPoints);
      Assert.AreEqual("DSV Erwachsene", model.GlobalRaceConfig.Name);
      model = configDBAndGetModel(CompetitionProperties.ECompetitionType.DSV_SchoolPoints);
      Assert.AreEqual("DSV Schüler U14-U16", model.GlobalRaceConfig.Name);
      model = configDBAndGetModel(CompetitionProperties.ECompetitionType.DSV_SchoolNoPoints);
      Assert.AreEqual("DSV Schüler U14-U16", model.GlobalRaceConfig.Name);
      model = configDBAndGetModel(CompetitionProperties.ECompetitionType.VersatilityPoints);
      Assert.AreEqual(null, model.GlobalRaceConfig.Name);
      model = configDBAndGetModel(CompetitionProperties.ECompetitionType.VersatilityNoPoints);
      Assert.AreEqual(null, model.GlobalRaceConfig.Name);
      model = configDBAndGetModel(CompetitionProperties.ECompetitionType.ClubInternal_BestRun);
      Assert.AreEqual("Vereinsrennen - BestOfTwo", model.GlobalRaceConfig.Name);
      model = configDBAndGetModel(CompetitionProperties.ECompetitionType.ClubInternal_Sum);
      Assert.AreEqual("Vereinsrennen - Summe", model.GlobalRaceConfig.Name);
      model = configDBAndGetModel(CompetitionProperties.ECompetitionType.Parallel);
      Assert.AreEqual(null, model.GlobalRaceConfig.Name);
      model = configDBAndGetModel(CompetitionProperties.ECompetitionType.Sledding_NoPoints);
      Assert.AreEqual(null, model.GlobalRaceConfig.Name);
      model = configDBAndGetModel(CompetitionProperties.ECompetitionType.Sledding_Points);
      Assert.AreEqual(null, model.GlobalRaceConfig.Name);
    }


    [TestMethod]
    [DeploymentItem(@"raceconfigpresets\DSV Erwachsene.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\DSV Schüler U14-U16.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\FIS Rennen Men.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\FIS Rennen Women.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\Inline (allgemein).preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\Inline (Punkte).preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\KSC Ebersberg -U12.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\KSC Ebersberg U14-.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\SVM Schüler U12.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\SVM Schüler U8-U10.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\Vereinsrennen - BestOfTwo.preset", @"raceconfigpresets")]
    [DeploymentItem(@"raceconfigpresets\Vereinsrennen - Summe.preset", @"raceconfigpresets")]
    [DeploymentItem(@"TestDataBases\TestDB_Empty.mdb")]
    public void GlobalRaceConfig_DSVAlpinExport()
    {
      RaceConfigurationPresets cfgPresets = new RaceConfigurationPresets("raceconfigpresets");

      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_Empty.mdb");
      Database db = new Database();
      db.Connect(dbFilename);
      AppDataModel model = new AppDataModel(db);

      RaceConfiguration config;
      config = cfgPresets.GetConfiguration("DSV Erwachsene");
      Assert.AreEqual(CompetitionProperties.ECompetitionType.DSV_Points, config.InternalDSVAlpinCompetitionTypeWrite);
      model.GlobalRaceConfig = config;
      Assert.AreEqual(CompetitionProperties.ECompetitionType.DSV_Points, db.GetCompetitionProperties().Type);

      config = cfgPresets.GetConfiguration("DSV Schüler U14-U16");
      Assert.AreEqual(CompetitionProperties.ECompetitionType.DSV_SchoolPoints, config.InternalDSVAlpinCompetitionTypeWrite);
      model.GlobalRaceConfig = config;
      Assert.AreEqual(CompetitionProperties.ECompetitionType.DSV_SchoolPoints, db.GetCompetitionProperties().Type);

      config = cfgPresets.GetConfiguration("FIS Rennen Men");
      Assert.AreEqual(CompetitionProperties.ECompetitionType.FIS_Men, config.InternalDSVAlpinCompetitionTypeWrite);
      model.GlobalRaceConfig = config;
      Assert.AreEqual(CompetitionProperties.ECompetitionType.FIS_Men, db.GetCompetitionProperties().Type);

      config = cfgPresets.GetConfiguration("FIS Rennen Women");
      Assert.AreEqual(CompetitionProperties.ECompetitionType.FIS_Women, config.InternalDSVAlpinCompetitionTypeWrite);
      model.GlobalRaceConfig = config;
      Assert.AreEqual(CompetitionProperties.ECompetitionType.FIS_Women, db.GetCompetitionProperties().Type);

      config = cfgPresets.GetConfiguration("Inline (allgemein)");
      Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_Sum, config.InternalDSVAlpinCompetitionTypeWrite);
      model.GlobalRaceConfig = config;
      Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_Sum, db.GetCompetitionProperties().Type);

      config = cfgPresets.GetConfiguration("Inline (Punkte)");
      Assert.AreEqual(CompetitionProperties.ECompetitionType.DSV_Points, config.InternalDSVAlpinCompetitionTypeWrite);
      model.GlobalRaceConfig = config;
      Assert.AreEqual(CompetitionProperties.ECompetitionType.DSV_Points, db.GetCompetitionProperties().Type);

      config = cfgPresets.GetConfiguration("KSC Ebersberg -U12");
      Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_BestRun, config.InternalDSVAlpinCompetitionTypeWrite);
      model.GlobalRaceConfig = config;
      Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_BestRun, db.GetCompetitionProperties().Type);

      config = cfgPresets.GetConfiguration("KSC Ebersberg U14-");
      Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_Sum, config.InternalDSVAlpinCompetitionTypeWrite);
      model.GlobalRaceConfig = config;
      Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_Sum, db.GetCompetitionProperties().Type);

      config = cfgPresets.GetConfiguration("SVM Schüler U12");
      Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_Sum, config.InternalDSVAlpinCompetitionTypeWrite);
      model.GlobalRaceConfig = config;
      Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_Sum, db.GetCompetitionProperties().Type);

      config = cfgPresets.GetConfiguration("SVM Schüler U8-U10");
      Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_BestRun, config.InternalDSVAlpinCompetitionTypeWrite);
      model.GlobalRaceConfig = config;
      Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_BestRun, db.GetCompetitionProperties().Type);

      config = cfgPresets.GetConfiguration("Vereinsrennen - BestOfTwo");
      Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_BestRun, config.InternalDSVAlpinCompetitionTypeWrite);
      model.GlobalRaceConfig = config;
      Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_BestRun, db.GetCompetitionProperties().Type);

      config = cfgPresets.GetConfiguration("Vereinsrennen - Summe");
      Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_Sum, config.InternalDSVAlpinCompetitionTypeWrite);
      model.GlobalRaceConfig = config;
      Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_Sum, db.GetCompetitionProperties().Type);


      config = cfgPresets.GetConfiguration("Vereinsrennen - Summe");
      Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_Sum, config.InternalDSVAlpinCompetitionTypeWrite);

      config.ActiveFields = new List<string>();
      model.GlobalRaceConfig = config;
      Assert.IsFalse(db.GetCompetitionProperties().WithPoints);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveClub);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveCode);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveYear);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveNation);

      config.ActiveFields = new List<string> { "Points" };
      model.GlobalRaceConfig = config;
      Assert.IsTrue(db.GetCompetitionProperties().WithPoints);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveClub);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveCode);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveYear);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveNation);

      config.ActiveFields = new List<string> { "Club" };
      model.GlobalRaceConfig = config;
      Assert.IsFalse(db.GetCompetitionProperties().WithPoints);
      Assert.IsTrue(db.GetCompetitionProperties().FieldActiveClub);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveCode);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveYear);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveNation);

      config.ActiveFields = new List<string> { "Code" };
      model.GlobalRaceConfig = config;
      Assert.IsFalse(db.GetCompetitionProperties().WithPoints);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveClub);
      Assert.IsTrue(db.GetCompetitionProperties().FieldActiveCode);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveYear);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveNation);

      config.ActiveFields = new List<string> { "Year" };
      model.GlobalRaceConfig = config;
      Assert.IsFalse(db.GetCompetitionProperties().WithPoints);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveClub);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveCode);
      Assert.IsTrue(db.GetCompetitionProperties().FieldActiveYear);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveNation);

      config.ActiveFields = new List<string> { "Nation" };
      model.GlobalRaceConfig = config;
      Assert.IsFalse(db.GetCompetitionProperties().WithPoints);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveClub);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveCode);
      Assert.IsFalse(db.GetCompetitionProperties().FieldActiveYear);
      Assert.IsTrue(db.GetCompetitionProperties().FieldActiveNation);

      config.ActiveFields = new List<string> { "Year", "Nation", "Code", "Points", "Club" };
      model.GlobalRaceConfig = config;
      Assert.IsTrue(db.GetCompetitionProperties().WithPoints);
      Assert.IsTrue(db.GetCompetitionProperties().FieldActiveClub);
      Assert.IsTrue(db.GetCompetitionProperties().FieldActiveCode);
      Assert.IsTrue(db.GetCompetitionProperties().FieldActiveYear);
      Assert.IsTrue(db.GetCompetitionProperties().FieldActiveNation);
    }
  }
}
