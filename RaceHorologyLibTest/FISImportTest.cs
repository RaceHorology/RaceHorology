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
    public void ImportFISList()
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

      // Test first line of Input
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
      // Test first line of Input
      {
        DataRow row = reader.Data.Tables[0].Rows[364];
        Assert.AreEqual("107747", row["Fiscode"]);
        Assert.AreEqual("SMART", row["Lastname"]);
        Assert.AreEqual("Amelia", row["Firstname"]);
        Assert.AreEqual("1998", row["Birthyear"]);
        Assert.AreEqual("WINDERMERE", row["Skiclub"]);
        Assert.AreEqual("CAN", row["Nationcode"]);
        Assert.AreEqual("W", row["Gender"]);
        Assert.AreEqual(999.99, row["DHpoints"]);
        Assert.AreEqual(17.82, row["SLpoints"]);
        Assert.AreEqual(41.29, row["GSpoints"]);
        Assert.AreEqual(144.08, row["SGpoints"]);
      }
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\Import\FIS\FIS-points-list-AL-2022-330.xlsx")]
    public void ImportFISParticipant()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.Model.AddRace(new Race.RaceProperties { RaceType = Race.ERaceType.DownHill, Runs = 1 });

      var reader = new FISImportReader(@"FIS-points-list-AL-2022-330.xlsx");

      {
        RaceImport imp = new RaceImport(
          tg.Model.GetRace(0),
          reader.GetMapping(tg.Model.GetRace(0)),
          new ClassAssignment(tg.Model.GetParticipantClasses()));

        var row = reader.Data.Tables[0].Rows[0];
        RaceParticipant rp = imp.ImportRow(row);
        Assert.AreEqual("10000001", rp.Code);
        Assert.AreEqual("FERRETTI", rp.Name);
        Assert.AreEqual("Jacopo", rp.Firstname);
        Assert.AreEqual(2004U, rp.Year);
        Assert.AreEqual("SKIING A.S.D.", rp.Club);
        Assert.AreEqual("ITA", rp.Nation);
        Assert.AreEqual('M', rp.Sex.Name);
        Assert.AreEqual(66.48, rp.Points); // GSpoints
      }

      {
        RaceImport imp = new RaceImport(
          tg.Model.GetRace(1),
          reader.GetMapping(tg.Model.GetRace(1)),
          new ClassAssignment(tg.Model.GetParticipantClasses()));

        var row = reader.Data.Tables[0].Rows[0];
        RaceParticipant rp = imp.ImportRow(row);
        Assert.AreEqual("10000001", rp.Code);
        Assert.AreEqual("FERRETTI", rp.Name);
        Assert.AreEqual("Jacopo", rp.Firstname);
        Assert.AreEqual(2004U, rp.Year);
        Assert.AreEqual("SKIING A.S.D.", rp.Club);
        Assert.AreEqual("ITA", rp.Nation);
        Assert.AreEqual('M', rp.Sex.Name);
        Assert.AreEqual(145.06, rp.Points); // DHpoints
      }



      ///////////////////////////////////////////////////////////////////
      /// FISUpdatePoints.UpdatePoints
      ///////////////////////////////////////////////////////////////////

      tg.Model.GetRace(0).GetParticipants()[0].Points = -1;
      tg.Model.GetRace(1).GetParticipants()[0].Points = -1;

      Assert.AreEqual(-1.0, tg.Model.GetRace(0).GetParticipants()[0].Points); // DHpoints
      Assert.AreEqual(-1.0, tg.Model.GetRace(1).GetParticipants()[0].Points); // DHpoints

      FISInterfaceModel fisImp = new FISInterfaceModel(tg.Model);
      fisImp.UpdateFISList(reader);
      FISUpdatePoints.UpdatePoints(tg.Model, fisImp);

      Assert.AreEqual(66.48, tg.Model.GetRace(0).GetParticipants()[0].Points); // DHpoints
      Assert.AreEqual(145.06, tg.Model.GetRace(1).GetParticipants()[0].Points); // DHpoints
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\Import\FIS\FIS-points-list-AL-2022-330.xlsx")]
    [DeploymentItem(@"TestDataBases\Import\FIS\FIS-points-list-AL-2022-331.xlsx")]
    public void ImportFIS_ErrorCase_DoubleImport()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.Model.AddRace(new Race.RaceProperties { RaceType = Race.ERaceType.DownHill, Runs = 1 });

      var reader1 = new FISImportReader(@"FIS-points-list-AL-2022-330.xlsx");

      FISInterfaceModel fisImp = new FISInterfaceModel(tg.Model);
      fisImp.UpdateFISList(reader1);
      Assert.AreEqual("5th FIS points list 2021/2022", fisImp.UsedList);

      var reader2 = new FISImportReader(@"FIS-points-list-AL-2022-331.xlsx");
      fisImp.UpdateFISList(reader2);
      Assert.AreEqual("6th FIS points list 2021/2022", fisImp.UsedList);
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\Import\FIS\FIS-points-list-AL-2022-331 - missing_fields.xlsx")]
    public void ImportFIS_ErrorCase_WrongExcel()
    {
      Assert.ThrowsException<Exception>(() =>
      {
        var reader = new FISImportReader(@"FIS-points-list-AL-2022-331 - missing_fields.xlsx");
      });
    }
  }
}
