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

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for DSVCalculationTests
  /// </summary>
  [TestClass]
  public class DSVCalculationTests
  {
    public DSVCalculationTests()
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
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS.mdb")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS_Slalom.config")]
    public void RudimentaryTest()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"1554MSBS.mdb");

      // Setup Data Model & Co
      Database db = new Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);

      DSVRaceCalculation raceCalcW = new DSVRaceCalculation(model.GetRace(0), model.GetRace(0).GetResultViewProvider(), 'W');
      raceCalcW.CalculatePenalty();
      Assert.AreEqual(28.56, raceCalcW.CalculatedPenalty);

      DSVRaceCalculation raceCalcM = new DSVRaceCalculation(model.GetRace(0), model.GetRace(0).GetResultViewProvider(), 'M');
      raceCalcM.CalculatePenalty();
      Assert.AreEqual(51.18, raceCalcM.CalculatedPenalty);
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case4-DSV-less-participants\2801DSHS.mdb")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case4-DSV-less-participants\2801DSHS_Slalom.config")]
    public void LessThen10ValidResults_Test()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"2801DSHS.mdb");

      // Setup Data Model & Co
      Database db = new Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);

      DSVRaceCalculation raceCalcW = new DSVRaceCalculation(model.GetRace(0), model.GetRace(0).GetResultViewProvider(), 'W');
      raceCalcW.CalculatePenalty();
      Assert.AreEqual(93.99, raceCalcW.CalculatedPenalty);

      DSVRaceCalculation raceCalcM = new DSVRaceCalculation(model.GetRace(0), model.GetRace(0).GetResultViewProvider(), 'M');
      raceCalcM.CalculatePenalty();
      Assert.AreEqual(91.51, raceCalcM.CalculatedPenalty);
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case5-DSV-less-qualified-CutOffPoints\2852MSBS.mdb")]
    public void CutOffPointsValidResults_Test()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"2852MSBS.mdb");

      // Setup Data Model & Co
      Database db = new Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);

      model.GetRace(0).RaceConfiguration.ValueCutOff = 250.0;

      DSVRaceCalculation raceCalcW = new DSVRaceCalculation(model.GetRace(0), model.GetRace(0).GetResultViewProvider(), 'W');
      raceCalcW.CalculatePenalty();
      Assert.AreEqual(32.12, raceCalcW.CalculatedPenalty);

      DSVRaceCalculation raceCalcM = new DSVRaceCalculation(model.GetRace(0), model.GetRace(0).GetResultViewProvider(), 'M');
      raceCalcM.CalculatePenalty();
      Assert.AreEqual(27.98, raceCalcM.CalculatedPenalty);
    }


    [TestMethod]
    public void MockTests()
    {
      DSVRaceCalculation getCalc(List<TestData> td, double valueF = 0.0, double valueZ = 0.0, double valueA = 0.0, double minPenalty = 0.0)
      {
        var race = createTestData(td);
        race.RaceConfiguration.ValueCutOff = 250.0;
        race.RaceConfiguration.ValueF = valueF;
        race.RaceConfiguration.ValueA = valueA;
        race.RaceConfiguration.ValueZ = valueZ;
        race.RaceConfiguration.MinimumPenalty= minPenalty;
        DSVRaceCalculation raceCalcW = new DSVRaceCalculation(race, race.GetResultViewProvider(), 'W');
        raceCalcW.CalculatePenalty();

        return raceCalcW;
      }

      var td1 = new List<TestData>
      {
        new TestData{ Points = 10.0, RunTime = 60.0},
        new TestData{ Points = 11.0, RunTime = 59.0},
        new TestData{ Points = 12.0, RunTime = 58.0},
        new TestData{ Points = 13.0, RunTime = 57.0},
        new TestData{ Points = 14.0, RunTime = 56.0},
        new TestData{ Points = 15.0, RunTime = 55.0},
        new TestData{ Points = 16.0, RunTime = 54.0},
        new TestData{ Points = 17.0, RunTime = 53.0},
        new TestData{ Points = 18.0, RunTime = 52.0},
        new TestData{ Points = 19.0, RunTime = 51.0}
      };
      Assert.AreEqual(12.00, getCalc(td1).CalculatedPenalty);

      // Check some variants of valueA, valueZ, minPenalty
      Assert.AreEqual(12.00, getCalc(td1, valueZ: 10.0).CalculatedPenalty);
      Assert.AreEqual(22.00, getCalc(td1, valueZ: 10.0).CalculatedPenaltyWithAdded);
      Assert.AreEqual(22.00, getCalc(td1, valueZ: 10.0).AppliedPenalty);

      Assert.AreEqual(12.00, getCalc(td1, valueZ: 10.0, valueA: -5.0).CalculatedPenalty);
      Assert.AreEqual(17.00, getCalc(td1, valueZ: 10.0, valueA: -5.0).CalculatedPenaltyWithAdded);
      Assert.AreEqual(17.00, getCalc(td1, valueZ: 10.0, valueA: -5.0).AppliedPenalty);

      Assert.AreEqual(12.00, getCalc(td1, valueZ: 10.0, valueA: -5.0, minPenalty: 25.0).CalculatedPenalty);
      Assert.AreEqual(17.00, getCalc(td1, valueZ: 10.0, valueA: -5.0, minPenalty: 25.0).CalculatedPenaltyWithAdded);
      Assert.AreEqual(25.00, getCalc(td1, valueZ: 10.0, valueA: -5.0, minPenalty: 25.0).AppliedPenalty);

      var td2 = new List<TestData>
      {
        new TestData{ Points = 9999.0, RunTime = 60.0},
        new TestData{ Points = 9999.0, RunTime = 59.0},
        new TestData{ Points = 9999.0, RunTime = 58.0},
        new TestData{ Points = 9999.0, RunTime = 57.0},
        new TestData{ Points = 9999.0, RunTime = 56.0},
        new TestData{ Points = -1.0, RunTime = 55.0},
        new TestData{ Points = -1.0, RunTime = 54.0},
        new TestData{ Points = -1.0, RunTime = 53.0},
        new TestData{ Points = -1.0, RunTime = 52.0},
        new TestData{ Points = -1.0, RunTime = 51.0}
      };
      Assert.AreEqual(124.5, getCalc(td2).CalculatedPenalty);

      var td3 = new List<TestData>
      {
        new TestData{ Points = 9999.0, RunTime = 60.0},
        new TestData{ Points = 9999.0, RunTime = 59.0},
        new TestData{ Points = 9999.0, RunTime = 58.0},
        new TestData{ Points = 9999.0, RunTime = 57.0},
        new TestData{ Points = 9999.0, RunTime = 56.0},
        new TestData{ Points = -1.0, RunTime = 55.0},
        new TestData{ Points = -1.0, RunTime = 54.0},
        new TestData{ Points = 10.0, RunTime = 53.0},
        new TestData{ Points = 11.0, RunTime = 52.0},
        new TestData{ Points = 12.0, RunTime = 51.0}
      };
      Assert.AreEqual(56.4, getCalc(td3).CalculatedPenalty);

      // Test for FIS Points Rules §4.4.5 (more then 1 participant at position 10)
      var td4 = new List<TestData>
      {
        new TestData{ Points = 9999.0, RunTime = 60.0},
        new TestData{ Points = 10.0, RunTime = 60.0},
        new TestData{ Points = 9999.0, RunTime = 59.0},
        new TestData{ Points = 9999.0, RunTime = 58.0},
        new TestData{ Points = 9999.0, RunTime = 57.0},
        new TestData{ Points = 9999.0, RunTime = 56.0},
        new TestData{ Points = -1.0, RunTime = 55.0},
        new TestData{ Points = -1.0, RunTime = 54.0},
        new TestData{ Points = 10.0, RunTime = 53.0},
        new TestData{ Points = 11.0, RunTime = 52.0},
        new TestData{ Points = 12.0, RunTime = 51.0}
      };
      Assert.AreEqual(11, getCalc(td4).TopTen.Count);
      Assert.AreEqual(32.2, getCalc(td4).CalculatedPenalty);

      // CalculationValid checks
      Assert.IsTrue(getCalc(td4).CalculationValid);
      // Check whether calculation is invalid if no data is available
      var td5 = new List<TestData>
      {
      };
      Assert.IsFalse(getCalc(td5).CalculationValid);
    }


    class TestData 
    { 
      public double Points;
      public double RunTime;
    }

    Race createTestData(IEnumerable<TestData> testData)
    {
      TestDataGenerator tg = new TestDataGenerator();

      Race race = tg.Model.GetRace(0);

      var rvp = new DSVSchoolRaceResultViewProvider();
      rvp.Init(race, tg.Model);
      race.SetResultViewProvider(rvp);

      foreach (var td in testData)
      {
        var rp = tg.createRaceParticipant(cat: tg.findCat('W'));
        rp.Points = td.Points;
        race.GetRun(0).SetRunTime(rp, TimeSpan.FromSeconds(td.RunTime));
      }
      return race;
    }
  }
}
