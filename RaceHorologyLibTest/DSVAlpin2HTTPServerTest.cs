using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using RaceHorologyLib;

namespace DSVAlpin2LibTest
{
  /// <summary>
  /// Summary description for DSVAlpin2HTTPServerTest
  /// </summary>
  [TestClass]
  public class DSVAlpin2HTTPServerTest
  {
    public DSVAlpin2HTTPServerTest()
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
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    public void JsonConversion()
    {
      string dbFilename = Path.Combine(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel dataModel = new AppDataModel(db);
      Race race = dataModel.GetCurrentRace();
      RaceRun rr1 = race.GetRun(0);

      string jsonStart = RaceHorologyLib.JsonConversion.ConvertStartList(rr1.GetStartList());
      string jsonResult = RaceHorologyLib.JsonConversion.ConvertRunResults(rr1.GetResultView());

      string jsonStartExpected = @"{""type"":""startlist"",""data"":[{""Id"":""1"",""StartNumber"":1,""Name"":""Nachname 1"",""Firstname"":""Vorname 1"",""Sex"":""W"",""Year"":2009,""Club"":""Verein 1"",""Nation"":""Nation 1"",""Class"":""Mädchen 2009"",""Group"":""U10 weiblich""},{""Id"":""2"",""StartNumber"":2,""Name"":""Nachname 2"",""Firstname"":""Vorname 2"",""Sex"":""M"",""Year"":2013,""Club"":""Verein 2"",""Nation"":""Nation 2"",""Class"":""Buben 2013"",""Group"":""Bambini männlich""},{""Id"":""3"",""StartNumber"":3,""Name"":""Nachname 3"",""Firstname"":""Vorname 3"",""Sex"":""M"",""Year"":2011,""Club"":""Verein 3"",""Nation"":""Nation 3"",""Class"":""Buben 2011"",""Group"":""U8 männlich""},{""Id"":""4"",""StartNumber"":4,""Name"":""Nachname 4"",""Firstname"":""Vorname 4"",""Sex"":""W"",""Year"":2014,""Club"":""Verein 4"",""Nation"":""Nation 4"",""Class"":""Mädchen 2014"",""Group"":""Bambini weiblich""},{""Id"":""5"",""StartNumber"":5,""Name"":""Nachname 5"",""Firstname"":""Vorname 5"",""Sex"":""M"",""Year"":2012,""Club"":""Verein 5"",""Nation"":""Nation 5"",""Class"":""Buben 2012"",""Group"":""U8 männlich""}]}";
      string jsonResultExpected = @"{""type"":""racerunresult"",""data"":[{""Id"":""1"",""Position"":1,""StartNumber"":""1"",""Name"":""Nachname 1"",""Firstname"":""Vorname 1"",""Sex"":""W"",""Year"":2009,""Club"":""Verein 1"",""Nation"":""Nation 1"",""Class"":""Mädchen 2009"",""Group"":""U10 weiblich"",""Runtime"":""00:22,85"",""DisqualText"":null,""JustModified"":false},{""Id"":""4"",""Position"":2,""StartNumber"":""4"",""Name"":""Nachname 4"",""Firstname"":""Vorname 4"",""Sex"":""W"",""Year"":2014,""Club"":""Verein 4"",""Nation"":""Nation 4"",""Class"":""Mädchen 2014"",""Group"":""Bambini weiblich"",""Runtime"":""00:29,88"",""DisqualText"":null,""JustModified"":false},{""Id"":""2"",""Position"":3,""StartNumber"":""2"",""Name"":""Nachname 2"",""Firstname"":""Vorname 2"",""Sex"":""M"",""Year"":2013,""Club"":""Verein 2"",""Nation"":""Nation 2"",""Class"":""Buben 2013"",""Group"":""Bambini männlich"",""Runtime"":""00:39,11"",""DisqualText"":null,""JustModified"":false},{""Id"":""3"",""Position"":0,""StartNumber"":""3"",""Name"":""Nachname 3"",""Firstname"":""Vorname 3"",""Sex"":""M"",""Year"":2011,""Club"":""Verein 3"",""Nation"":""Nation 3"",""Class"":""Buben 2011"",""Group"":""U8 männlich"",""Runtime"":null,""DisqualText"":null,""JustModified"":false}]}";

      Assert.AreEqual(jsonStartExpected, jsonStart);
      Assert.AreEqual(jsonResultExpected, jsonResult);
    }
  }
}
