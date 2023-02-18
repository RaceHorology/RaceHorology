using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for PrintCertificateTest
  /// </summary>
  [TestClass]
  public class PrintCertificateTest
  {
    public PrintCertificateTest()
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
    //[DeploymentItem(@"TestOutputs\RefereeProtocol_Empty.pdf")]
    //[DeploymentItem(@"resources\FreeSans.ttf", @"resources")]
    //[DeploymentItem(@"resources\FreeSansBold.ttf", @"resources")]
    //[DeploymentItem(@"resources\FreeSansOblique.ttf", @"resources")]
    public void Certificate_Empty()
    {
      string workingDir = TestUtilities.CreateWorkingFolder(testContextInstance.TestDeploymentDir);

      TestDataGenerator tg = new TestDataGenerator(workingDir);
      {
        IPDFReport report = new Certificates(tg.Model.GetRace(0), 10);

        string filenameOutput = report.ProposeFilePath();
        report.Generate(filenameOutput);
        
        System.Diagnostics.Process.Start(filenameOutput);

        //Assert.IsTrue(TestUtilities.GenerateAndCompareAgainstPdf(TestContext, report, @"RefereeProtocol_Empty.pdf", 0));
      }
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS.mdb")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS_Slalom.config")]
    [DeploymentItem(@"TestOutputs\1554MSBS\1554MSBS - Startliste 1. Durchgang.pdf")]
    public void Integration_1554MSBS_Certificates()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"1554MSBS.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      AppDataModel model = new AppDataModel(db);

      Race race = model.GetRace(0);
      {
        IPDFReport report = new Certificates(race, 10);

        string filenameOutput = report.ProposeFilePath();
        report.Generate(filenameOutput);

        System.Diagnostics.Process.Start(filenameOutput);

        //Assert.IsTrue(TestUtilities.GenerateAndCompareAgainstPdf(TestContext, report, @"RefereeProtocol_Empty.pdf", 0));
      }
    }


    [TestMethod]
    public void Certificate_Template()
    {
      string workingDir = TestUtilities.CreateWorkingFolder(testContextInstance.TestDeploymentDir);

      TestDataGenerator tg = new TestDataGenerator(workingDir);
      {
        IPDFReport report = new Certificates(tg.Model.GetRace(0), 10, true);

        string filenameOutput = report.ProposeFilePath();
        report.Generate(filenameOutput);

        System.Diagnostics.Process.Start(filenameOutput);

        //Assert.IsTrue(TestUtilities.GenerateAndCompareAgainstPdf(TestContext, report, @"RefereeProtocol_Empty.pdf", 0));
      }
    }

  }
}
