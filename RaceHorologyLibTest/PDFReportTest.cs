/*
 *  Copyright (C) 2019 - 2023 by Sven Flossmann
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
using iText.Kernel.Utils;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for PDFReportTest
  /// </summary>
  [TestClass]
  public class PDFReportTest
  {
    public PDFReportTest()
    {
      //
      // TODO: Add constructor logic here
      //

      System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
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




    internal class DummyRaceReport : PDFRaceReport
    {
      public DummyRaceReport(Race race) : base(race) { }
      protected override string getReportName() { return "DummyName"; }
      protected override string getTitle() { return "DummyTitle"; }
      protected override void addContent(PdfDocument pdf, Document document) { document.Add(new Paragraph("DummyContent")); }
    }
    [TestMethod]
    [DeploymentItem(@"TestOutputs\Base_RaceReport.pdf")]
    [DeploymentItem(@"resources\FreeSans.ttf", @"resources")]
    [DeploymentItem(@"resources\FreeSansBold.ttf", @"resources")]
    [DeploymentItem(@"resources\FreeSansOblique.ttf", @"resources")]
    public void Base_RaceReport()
    {
      TestDataGenerator tg = new TestDataGenerator(testContextInstance.TestResultsDirectory);
      {
        IPDFReport report = new DummyRaceReport(tg.Model.GetRace(0));
        Assert.IsTrue(TestUtilities.GenerateAndCompareAgainstPdf(TestContext, report, @"Base_RaceReport.pdf", 8));
      }
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS.mdb")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS_Slalom.config")]
    [DeploymentItem(@"TestOutputs\1554MSBS\1554MSBS - Startliste 1. Durchgang.pdf")]
    [DeploymentItem(@"resources\FreeSans.ttf", @"resources")]
    [DeploymentItem(@"resources\FreeSansBold.ttf", @"resources")]
    [DeploymentItem(@"resources\FreeSansOblique.ttf" , @"resources")]
    public void Integration_1554MSBS_StartlistRun1()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"1554MSBS.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      AppDataModel model = new AppDataModel(db);

      Race race = model.GetRace(0);

      {
        IPDFReport report = new StartListReport(race.GetRun(0));
        Assert.IsTrue(TestUtilities.GenerateAndCompareAgainstPdf(TestContext, report, @"1554MSBS - Startliste 1. Durchgang.pdf", 7));
      }
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS.mdb")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS_Slalom.config")]
    [DeploymentItem(@"TestOutputs\1554MSBS\1554MSBS - Startliste 2. Durchgang.pdf")]
    [DeploymentItem(@"resources\FreeSans.ttf", @"resources")]
    [DeploymentItem(@"resources\FreeSansBold.ttf", @"resources")]
    [DeploymentItem(@"resources\FreeSansOblique.ttf", @"resources")]
    public void Integration_1554MSBS_StartlistRun2()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"1554MSBS.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      AppDataModel model = new AppDataModel(db);

      Race race = model.GetRace(0);

      {
        IPDFReport report = new StartListReport2ndRun(race.GetRun(1));
        Assert.IsTrue(TestUtilities.GenerateAndCompareAgainstPdf(TestContext, report, @"1554MSBS - Startliste 2. Durchgang.pdf", 6));
      }
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS.mdb")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS_Slalom.config")]
    [DeploymentItem(@"TestOutputs\1554MSBS\1554MSBS - Ergebnis 1. Durchgang.pdf")]
    [DeploymentItem(@"resources\FreeSans.ttf", @"resources")]
    [DeploymentItem(@"resources\FreeSansBold.ttf", @"resources")]
    [DeploymentItem(@"resources\FreeSansOblique.ttf", @"resources")]
    public void Integration_1554MSBS_ResultRun1()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"1554MSBS.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      AppDataModel model = new AppDataModel(db);

      Race race = model.GetRace(0);

      {
        IPDFReport report = new RaceRunResultReport(race.GetRun(0));
        Assert.IsTrue(TestUtilities.GenerateAndCompareAgainstPdf(TestContext, report, @"1554MSBS - Ergebnis 1. Durchgang.pdf", 10));
      }
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS.mdb")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS_Slalom.config")]
    [DeploymentItem(@"TestOutputs\1554MSBS\1554MSBS - Ergebnis 2. Durchgang.pdf")]
    [DeploymentItem(@"resources\FreeSans.ttf", @"resources")]
    [DeploymentItem(@"resources\FreeSansBold.ttf", @"resources")]
    [DeploymentItem(@"resources\FreeSansOblique.ttf", @"resources")]
    public void Integration_1554MSBS_ResultRun2()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"1554MSBS.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      AppDataModel model = new AppDataModel(db);

      Race race = model.GetRace(0);

      {
        IPDFReport report = new RaceRunResultReport(race.GetRun(1));
        Assert.IsTrue(TestUtilities.GenerateAndCompareAgainstPdf(TestContext, report, @"1554MSBS - Ergebnis 2. Durchgang.pdf", 6));
      }
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS.mdb")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS_Slalom.config")]
    [DeploymentItem(@"TestOutputs\1554MSBS\1554MSBS - Ergebnis Gesamt.pdf")]
    [DeploymentItem(@"resources\FreeSans.ttf", @"resources")]
    [DeploymentItem(@"resources\FreeSansBold.ttf", @"resources")]
    [DeploymentItem(@"resources\FreeSansOblique.ttf", @"resources")]
    public void Integration_1554MSBS_ResultFinal()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"1554MSBS.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      AppDataModel model = new AppDataModel(db);

      Race race = model.GetRace(0);

      {
        IPDFReport report = new RaceResultReport(race);
        Assert.IsTrue(TestUtilities.GenerateAndCompareAgainstPdf(TestContext, report, @"1554MSBS - Ergebnis Gesamt.pdf", 10));
      }
    }
  }
}
