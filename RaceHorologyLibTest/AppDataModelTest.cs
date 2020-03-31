using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for AppDataModelTest
  /// </summary>
  [TestClass]
  public class AppDataModelTest
  {
    public AppDataModelTest()
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
    public void RaceParticpant1()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants_MultipleRacesNoStartnumber.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);
      var race = model.GetRace(0);
      Assert.AreEqual(Race.ERaceType.DownHill, race.RaceType); // Check that the correct race has been selected

      var rps = race.GetParticipants().ToList();
      rps.Sort(Comparer<RaceParticipant>.Create((x, y) => x.Name.CompareTo(y.Name)));

      // TEST: Remove particpant
      race.GetParticipants().Remove(rps[0]); // "Nachname 1"
      Assert.IsNull(race.GetParticipants().FirstOrDefault(p => p.Name == "Nachname 1"));

      // TEST: Add particpant
      var p7 = model.GetParticipants().First(p => p.Name == "Nachname 7");
      race.AddParticipant(p7);
      Assert.IsNotNull(race.GetParticipants().FirstOrDefault(p => p.Name == "Nachname 7"));

      model = null;
      db.Close();


      // TEST: Cross-Check whether the startnumbers have been stored in DataBase
      RaceHorologyLib.Database db2 = new RaceHorologyLib.Database();
      db2.Connect(dbFilename);
      AppDataModel model2 = new AppDataModel(db2);
      Assert.IsNull(model2.GetRace(0).GetParticipants().FirstOrDefault(p => p.Name == "Nachname 1"));
      Assert.IsNotNull(model2.GetRace(0).GetParticipants().FirstOrDefault(p => p.Name == "Nachname 7"));
      return;
    }
  }
}
