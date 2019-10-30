using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DSVAlpin2Lib;

namespace DSVAlpin2LibTest
{
  /// <summary>
  /// Summary description for LiveTimingRMTest
  /// </summary>
  [TestClass]
  public class LiveTimingRMTest
  {
    public LiveTimingRMTest()
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
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_LiveTiming.mdb")]
    [DeploymentItem(@"3rdparty\DSVAlpinX.liz", "3rdparty")]
    public void TestSerialization()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants_LiveTiming.mdb");
      DSVAlpin2Lib.Database db = new DSVAlpin2Lib.Database();
      db.Connect(dbFilename);
      AppDataModel model = new AppDataModel(db);


      LiveTimingRM cl = new LiveTimingRM(model, "01122", "livetiming", "livetiming");
      //cl.Init();

      model.SetCurrentRace(model.GetRaces()[0]);
      model.SetCurrentRaceRun(model.GetCurrentRace().GetRun(0));

      string classes = cl.getClasses();
      Assert.AreEqual(classes, "Klasse|5|Mädchen 2012|5\nKlasse|6|Mädchen 2011|7\nKlasse|7|Buben 2012|6\nKlasse|8|Buben 2011|8\nKlasse|9|Mädchen 2010|9\nKlasse|10|Mädchen 2009|11\nKlasse|11|Buben 2010|10\nKlasse|12|Buben 2009|12\nKlasse|17|Buben 2013|4\nKlasse|18|Buben 2014|2\nKlasse|19|Mädchen 2013|3\nKlasse|20|Mädchen 2014|1");

      string groups = cl.getGroups();
      Assert.AreEqual(groups, "Gruppe|2|Bambini männlich|2\nGruppe|3|U8 weiblich|3\nGruppe|4|U8 männlich|4\nGruppe|5|U10 weiblich|5\nGruppe|6|U10 männlich|6\nGruppe|9|Bambini weiblich|1");

      string categories = cl.getCategories();
      Assert.AreEqual(categories, "Kategorie|M|M|1\nKategorie|W|W|2");

      string participants = cl.getParticipantsData();
      Assert.AreEqual(participants, "W|5|10|1|1||Nachname 1, Vorname 1|2009|Nation 1|Verein 1|9999,99\nM|2|17|2|2||Nachname 2, Vorname 2|2013|Nation 2|Verein 2|9999,99\nM|4|8|3|3||Nachname 3, Vorname 3|2011|Nation 3|Verein 3|9999,99\nW|9|20|4|4||Nachname 4, Vorname 4|2014|Nation 4|Verein 4|9999,99\nM|4|7|5|5||Nachname 5, Vorname 5|2012|Nation 5|Verein 5|9999,99");

      string startList = cl.getStartListData(model.GetCurrentRaceRun());
      Assert.AreEqual(startList, "  1\n  2\n  3\n  4\n  5");

      string timingData = cl.getTimingData(model.GetCurrentRaceRun());
      Assert.AreEqual(timingData, "  10000010,23\n  29000000,01\n  31999999,99\n  42999999,99\n  53999999,99");
    }

    [TestMethod]
    //[Ignore]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_LiveTiming.mdb")]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_LiveTiming_GiantSlalom.config")]
    [DeploymentItem(@"3rdparty\DSVAlpinX.liz", "3rdparty")]
    public void TestOnline()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants_LiveTiming.mdb");
      DSVAlpin2Lib.Database db = new DSVAlpin2Lib.Database();
      db.Connect(dbFilename);
      AppDataModel model = new AppDataModel(db);

      LiveTimingRM cl = new LiveTimingRM(model, "01122", "livetiming", "livetiming");
      cl.Init();

      model.SetCurrentRace(model.GetRaces()[0]);
      model.SetCurrentRaceRun(model.GetCurrentRace().GetRun(0));

      cl.startLiveTiming(model.GetCurrentRace());

      cl.Test1();
      cl.Test2();

    }
  }
}
