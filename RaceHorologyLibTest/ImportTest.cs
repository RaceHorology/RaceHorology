using RaceHorologyLib;
using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for ImportTest
  /// </summary>
  [TestClass]
  public class ImportTest
  {
    public ImportTest()
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
    public void TestMethod1()
    {

    }

    [TestMethod]
    public void DetectColumn()
    {
      //var stream = File.Open(@"C:\Users\sven\Dropbox\SyncFolder\WSVGlonn_Zeitnahme\Rennen 2020\2020-02-02 - KC4\Bewerbsdaten\Anmeldung\Teilnehmer_V1_202001301844.xls", FileMode.Open, FileAccess.Read);

      var ir = new ImportReader(@"C:\Users\sven\Dropbox\SyncFolder\WSVGlonn_Zeitnahme\Rennen 2020\2020-02-02 - KC4\Bewerbsdaten\Anmeldung\Teilnehmer_V1_202001301844-U14-.csv");
      var columns = ir.Columns;
    }
  }
}
