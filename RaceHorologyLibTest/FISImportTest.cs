/*
 *  Copyright (C) 2019 - 2021 by Sven Flossmann
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
using System.Data;
using System.Linq;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for FISImportTest
  /// </summary>
  [TestClass]
  public class FISImportTest
  {
    public FISImportTest()
    {
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
    [DeploymentItem(@"TestDataBases\Import\FIS\FIS-points-list-AL-2022-330.xlsx")]
    public void ImportPointList()
    {
      var reader = new FISImportReader(@"FIS-points-list-AL-2022-330.xlsx");

      reader.Columns.Contains("Fiscode");

      Assert.AreEqual("5th FIS points list 2021/2022", reader.UsedFISList);
      Assert.AreEqual(new DateTime(2021, 9, 14), reader.Date);

      //Assert.IsNotNull(reader.Mapping);

      Assert.AreEqual("Fiscode", reader.Columns[0]);
      Assert.AreEqual("Lastname", reader.Columns[1]);
      Assert.AreEqual("Firstname", reader.Columns[2]);
      Assert.AreEqual("Nationcode", reader.Columns[3]);
      Assert.AreEqual("Gender", reader.Columns[4]);
      Assert.AreEqual("Birthdate", reader.Columns[5]);
      Assert.AreEqual("Skiclub", reader.Columns[6]);
      Assert.AreEqual("Birthyear", reader.Columns[7]);

      {
        DataRow row = reader.Data.Tables[0].Rows[0];
        Assert.AreEqual("10000001", row["Fiscode"]);
        Assert.AreEqual("FERRETTI", row["Lastname"]);
        Assert.AreEqual("Jacopo", row["Firstname"]);
        Assert.AreEqual("2004", row["Birthyear"]);
        Assert.AreEqual("SKIING A.S.D.", row["Skiclub"]);
        Assert.AreEqual("ITA", row["Nationcode"]);
        Assert.AreEqual("M", row["Gender"]);
        Assert.AreEqual(145.06, row["DHpoints"]);
        Assert.AreEqual(66.38, row["SLpoints"]);
        Assert.AreEqual(66.48, row["GSpoints"]);
        Assert.AreEqual(99.09, row["SGpoints"]);
      }
    }


    //[TestMethod]
    //[DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS.mdb")]
    //[DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS_Slalom.config")]
    //[DeploymentItem(@"TestDataBases\Import\DSV\Punktelisten.zip")]
    //public void UpdatePoints1()
    //{
    //  // Setup Data Model & Co
    //  string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"1554MSBS.mdb");
    //  Database db = new Database();
    //  db.Connect(dbFilename);
    //  AppDataModel model = new AppDataModel(db);
    //  Race race = model.GetRace(0);

    //  // Import DSV Point List
    //  DSVImportReader dsvImportReader = new DSVImportReader(new DSVImportReaderZip(@"Punktelisten.zip", DSVImportReaderZipBase.EDSVListType.Pupils_U14U16));

    //  // Check two prior
    //  Assert.AreEqual(148.86, race.GetParticipants().First(r => r.SvId == "24438").Points);
    //  Assert.AreEqual(129.12, race.GetParticipants().First(r => r.SvId == "25399").Points);

    //  UpdatePointsImport import = new UpdatePointsImport(race, dsvImportReader.Mapping);
    //  ImportResults impRes = import.DoImport(dsvImportReader.Data);

    //  Assert.AreEqual(125, impRes.SuccessCount);
    //  Assert.AreEqual(2, impRes.ErrorCount);

    //  Assert.AreEqual(110.96, race.GetParticipants().First(r => r.SvId == "24438").Points);
    //  Assert.AreEqual(100.33, race.GetParticipants().First(r => r.SvId == "25399").Points);
    //}


    //[TestMethod]
    //[DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS.mdb")]
    //[DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS_Slalom.config")]
    //[DeploymentItem(@"TestDataBases\Import\DSV\Punktelisten.zip")]
    //public void UpdatePoints2()
    //{
    //  // Setup Data Model & Co
    //  string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"1554MSBS.mdb");
    //  Database db = new Database();
    //  db.Connect(dbFilename);
    //  AppDataModel model = new AppDataModel(db);
    //  Race race = model.GetRace(0);

    //  // Import DSV Point List
    //  DSVImportReader dsvImportReader = new DSVImportReader(new DSVImportReaderZip(@"Punktelisten.zip", DSVImportReaderZipBase.EDSVListType.Pupils_U14U16));

    //  // Check two prior
    //  Assert.AreEqual(148.86, race.GetParticipants().First(r => r.SvId == "24438").Points);
    //  Assert.AreEqual(129.12, race.GetParticipants().First(r => r.SvId == "25399").Points);

    //  var impRes = DSVUpdatePoints.UpdatePoints(model, dsvImportReader.Data, dsvImportReader.Mapping, dsvImportReader.UsedDSVList);

    //  Assert.AreEqual(1, impRes.Count);
    //  Assert.AreEqual(125, impRes[0].SuccessCount);
    //  Assert.AreEqual(2, impRes[0].ErrorCount);

    //  Assert.AreEqual(110.96, race.GetParticipants().First(r => r.SvId == "24438").Points);
    //  Assert.AreEqual(100.33, race.GetParticipants().First(r => r.SvId == "25399").Points);

    //  Assert.AreEqual("DSVSA20END", model.GetDB().GetKeyValue("DSV_UsedDSVList"));
    //}




    //[TestMethod]
    //[DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS.mdb")]
    //[DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS_Slalom.config")]
    //[DeploymentItem(@"TestDataBases\Import\DSV\Punktelisten.zip")]
    //public void DSVInterfaceModel_Test1()
    //{
    //  // Setup Data Model & Co
    //  string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"1554MSBS.mdb");
    //  Database db = new Database();
    //  db.Connect(dbFilename);
    //  AppDataModel dm = new AppDataModel(db);

    //  {
    //    // Initially, there aren't any data available
    //    DSVInterfaceModel dsvIF = new DSVInterfaceModel(dm);
    //    Assert.IsNull(dsvIF.Data, "Is null initially");
    //    Assert.IsNull(dsvIF.Date, "Is null initially");

    //    var reader = new DSVImportReader(new DSVImportReaderZip("Punktelisten.zip", DSVImportReaderZipBase.EDSVListType.Pupils_U14U16));

    //    dsvIF.UpdateDSVList(new DSVImportReaderZip("Punktelisten.zip", DSVImportReaderZipBase.EDSVListType.Pupils_U14U16));

    //    Assert.AreEqual(reader.Data.Tables[0].Rows.Count, dsvIF.Data.Tables[0].Rows.Count);
    //    Assert.AreEqual(reader.Date, dsvIF.Date);
    //    Assert.AreEqual(reader.UsedDSVList, dsvIF.UsedDSVList);
    //  }
    //}

    //[TestMethod]
    //[DeploymentItem(@"TestDataBases\Import\DSV\Punktelisten.zip")]
    //public void DSVInterfaceModel_Test_ContainsParticipant()
    //{
    //  // Setup Data Model & Co
    //  var tg = new TestDataGenerator();
    //  var dm = tg.Model;

    //  DSVInterfaceModel dsvIF = new DSVInterfaceModel(dm);
    //  dsvIF.UpdateDSVList(new DSVImportReaderZip("Punktelisten.zip", DSVImportReaderZipBase.EDSVListType.Pupils_U14U16));

    //  var imp = new ParticipantImport(dm.GetParticipants(), dsvIF.Mapping, dm.GetParticipantCategories(), new ClassAssignment(dm.GetParticipantClasses()));
    //  var participant = imp.ImportRow(dsvIF.Data.Tables[0].Rows[0]);

    //  // Check if imported participant is available
    //  Assert.IsTrue(dsvIF.ContainsParticipant(participant));
    //  string storedName = participant.Name;
      
    //  // Modify participant, check if detected as not existing anymore
    //  participant.Name = "123";
    //  Assert.IsFalse(dsvIF.ContainsParticipant(participant));

    //  // Correct it again, check whether existing again
    //  participant.Name = storedName;
    //  Assert.IsTrue(dsvIF.ContainsParticipant(participant));

    //}
  }
}
