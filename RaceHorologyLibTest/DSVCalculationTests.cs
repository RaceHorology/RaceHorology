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
      //
      // TODO: Add constructor logic here
      //
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
    public void TestMethod1()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"1554MSBS.mdb");

      // Setup Data Model & Co
      Database db = new Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);

      DSVRaceCalculation raceCalcW = new DSVRaceCalculation(model.GetRace(0), model.GetRace(0).GetResultViewProvider(), "W");
      raceCalcW.CalculatePenalty();
      Assert.AreEqual(28.56, raceCalcW.CalculatedPenalty);

      DSVRaceCalculation raceCalcM = new DSVRaceCalculation(model.GetRace(0), model.GetRace(0).GetResultViewProvider(), "M");
      raceCalcM.CalculatePenalty();
      Assert.AreEqual(51.18, raceCalcM.CalculatedPenalty);
    }
  }
}
