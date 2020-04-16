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
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.csv")]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.tsv")]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.txt")]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.xls")]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.xlsx")]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844_comma.csv")]
    public void DetectColumns()
    {
      string[] columnShall =
      {
        "FIS-Code-Nr.",
        "Name",
        "Vorname",
        "Verband",
        "Verein",
        "Jahrgang",
        "Geschlecht",
        "FIS-Distanzpunkte",
        "FIS-Sprintpunkte",
        "Startnummer",
        "Gruppe",
        "DSV-Id",
        "Startpass",
        "Leer",
        "Nation",
        "Transponder",
        "Melder-Name",
        "Melder-Adresse",
        "Melder-Telefon",
        "Melder-Mobil",
        "Melder-Email",
        "Nachname Vorname"
      };

      void checkColumns(List<string> columns)
      {
        for (int i = 0; i < columnShall.Length; i++)
          Assert.AreEqual(columnShall[i], columns[i]);
      }

      {
        var ir = new ImportReader(@"Teilnehmer_V1_202001301844.xls");
        checkColumns(ir.Columns);
      }
      { 
        var ir = new ImportReader(@"Teilnehmer_V1_202001301844.xlsx");
        checkColumns(ir.Columns);
      }
      {
        var ir = new ImportReader(@"Teilnehmer_V1_202001301844.csv");
        checkColumns(ir.Columns);
      }
      {
        var ir = new ImportReader(@"Teilnehmer_V1_202001301844.txt");
        checkColumns(ir.Columns);
      }
      {
        var ir = new ImportReader(@"Teilnehmer_V1_202001301844.tsv");
        checkColumns(ir.Columns);
      }
      {
        var ir = new ImportReader(@"Teilnehmer_V1_202001301844_comma.csv");
        checkColumns(ir.Columns);
      }
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.csv")]
    public void ImportParticpant()
    {
      var ir = new ImportReader(@"Teilnehmer_V1_202001301844.csv");

      ParticipantMapping mapping = new ParticipantMapping(ir.Columns);

      List<Participant> participants = new List<Participant>();
      Import im = new Import(ir.Data, participants, mapping);
      im.DoImport();

    }
  }
}
