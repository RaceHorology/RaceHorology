/*
 *  Copyright (C) 2019 - 2020 by Sven Flossmann
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
      dsvExport.ExportXML(xmlData, model.GetRace(0));

      xmlData.Position = 0;
      StreamReader reader = new StreamReader(xmlData);
      string s = reader.ReadToEnd();
    }


    [TestMethod]
    public void BasicExceptions()
    {
      var model = createTestDataModel1Race1Run();

      DSVExport dsvExport = new DSVExport();
      MemoryStream xmlData = new MemoryStream();


      var raceProps = new AdditionalRaceProperties();
      model.GetRace(0).AdditionalProperties = raceProps;
      Assert.AreEqual("missing coarsename", 
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.CoarseName = "Kurs 1";

      Assert.AreEqual("missing number_of_gates", 
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.RaceRun1.Gates = 10;

      Assert.AreEqual("missing number_of_turninggates",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.RaceRun1.Turns = 9;

      Assert.AreEqual("missing startaltitude",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.StartHeight = 1000;

      Assert.AreEqual("missing finishaltitude",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.FinishHeight = 100;

      Assert.AreEqual("missing courselength",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.CoarseLength = 1000;

      Assert.AreEqual("missing coursesetter",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.RaceRun1.CoarseSetter = new AdditionalRaceProperties.Person { Name = null, Club = null };
    }




    [TestMethod]
    public void VerifyRaceDescription()
    {
      var model = createTestDataModel1Race1Run();

      DSVExport dsvExport = new DSVExport();
      MemoryStream xmlData = new MemoryStream();
      dsvExport.ExportXML(xmlData, model.GetRace(0));

      xmlData.Position = 0;
      StreamReader reader = new StreamReader(xmlData);
      string s = reader.ReadToEnd();

    }


    private AppDataModel createTestDataModel1Race1Run()
    {
      AppDataModel dm = new AppDataModel(new DummyDataBase("dummy"));

      dm.AddRace(new Race.RaceProperties { RaceType = Race.ERaceType.GiantSlalom, Runs = 1 });

      return dm;
    }

  }
}
