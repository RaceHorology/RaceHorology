using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for StartNumberTest
  /// </summary>
  [TestClass]
  public class StartNumberTest
  {
    public StartNumberTest()
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
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_MultipleRacesNoStartnumber.mdb")]
    public void ChangeStartNumber()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants_MultipleRacesNoStartnumber.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);
      var rps = model.GetRace(0).GetParticipants().ToList();

      // Assign startnumber from scratch
      rps.Sort(Comparer<RaceParticipant>.Create((x, y) => x.Name.CompareTo(y.Name)));
      for(int i=0; i<rps.Count; i++)
        rps[i].StartNumber = (uint)i+1;

      // Test whether startnumber is remembered
      for (int i = 0; i < rps.Count; i++)
        Assert.AreEqual((uint)i+1, rps[i].StartNumber);


    }
  }
}
