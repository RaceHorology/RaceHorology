using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;
using System.IO;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for DSVExportTest
  /// </summary>
  [TestClass]
  public class DSVExportTest
  {
    public DSVExportTest()
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
    [DeploymentItem(@"TestDataBases\FullTestCases\Case1\KSC4--U12.mdb")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case1\KSC4--U12_GiantSlalom.config")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case1\KSC4--U12_ALGE_Run1.txt")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case1\KSC4--U12_ALGE_Run1.txt")]
    public void Test1()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"KSC4--U12.mdb");

      // Setup Data Model & Co
      Database db = new Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);

      DSVExport dsvExport = new DSVExport();

      MemoryStream xmlData = new MemoryStream();
      dsvExport.Export(xmlData, model.GetRace(0));

      xmlData.Position = 0;
      StreamReader reader = new StreamReader(xmlData);
      string s = reader.ReadToEnd();
    }
  }
}
