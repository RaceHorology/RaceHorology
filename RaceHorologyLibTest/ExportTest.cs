/*
 *  Copyright (C) 2019 - 2026 by Sven Flossmann & Co-Authors (CREDITS.TXT)
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for ExportTest
  /// </summary>
  [TestClass]
  public class ExportTest
  {
    public ExportTest()
    {
      SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
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
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();
      Race race = tg.Model.GetRace(0);

      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: 1.0); // 1
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: 2.0);
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: 3.0);

      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"), points: 1.5); // 4
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"), points: 2.5);
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"), points: 3.5); // 6

      RaceRun rr1 = race.GetRun(0);
      RaceRun rr2 = race.GetRun(1);
      rr1.SetStartFinishTime(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0));
      rr2.SetRunTime(race.GetParticipant(1), new TimeSpan(0, 0, 2, 0, 123));

      rr1.SetRunTime(race.GetParticipant(2), new TimeSpan(0, 1, 1));
      rr1.SetResultCode(race.GetParticipant(3), RunResult.EResultCode.NiZ);

      RaceExport export = new RaceExport(tg.Model.GetRace(0));
      DataSet ds = export.ExportToDataSet();

      Assert.AreEqual("Name 1", ds.Tables[0].Rows[0]["Name"]);
      Assert.AreEqual("Name 2", ds.Tables[0].Rows[1]["Name"]);
      Assert.AreEqual("Name 3", ds.Tables[0].Rows[2]["Name"]);
      Assert.AreEqual("Name 4", ds.Tables[0].Rows[3]["Name"]);
      Assert.AreEqual("Name 5", ds.Tables[0].Rows[4]["Name"]);
      Assert.AreEqual("Name 6", ds.Tables[0].Rows[5]["Name"]);

      Assert.AreEqual("Firstname 1", ds.Tables[0].Rows[0]["Firstname"]);
      Assert.AreEqual(1.0, ds.Tables[0].Rows[0]["Points"]);
      Assert.AreEqual(new TimeSpan(0, 1, 0), ds.Tables[0].Rows[0]["Runtime_1"]);
      Assert.AreEqual("Normal", ds.Tables[0].Rows[0]["Resultcode_1"]);
      Assert.AreEqual(new TimeSpan(0, 0, 2, 0, 120), ds.Tables[0].Rows[0]["Runtime_2"]);
      Assert.AreEqual("Normal", ds.Tables[0].Rows[0]["Resultcode_2"]);
      Assert.AreEqual("NiZ", ds.Tables[0].Rows[2]["Resultcode_1"]);

      Assert.AreEqual(new TimeSpan(0, 0, 1, 0, 0), ds.Tables[0].Rows[0]["Totaltime"]);
      Assert.AreEqual(60.0, ds.Tables[0].Rows[0]["Totaltime_Seconds"]);
      Assert.AreEqual(1, ds.Tables[0].Rows[0]["Total_Position"]);
      Assert.AreEqual(2, ds.Tables[0].Rows[1]["Total_Position"]);

      var excelExport = new ExcelExport();
      excelExport.Export(@"c:\trash\test.xlsx", ds);

      var csvExport = new CsvExport();
      csvExport.Export(@"c:\trash\test.csv", ds, true);

      var tsvExport = new TsvExport();
      tsvExport.Export(@"c:\trash\test.txt", ds, true);
    }



    [TestMethod]
    public void DSVAlpinExport()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();
      Race race = tg.Model.GetRace(0);

      var rp = tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: 1.0); // 1
      rp.Participant.SvId = "123";
      rp.Participant.Year = 2010;
      rp.Participant.Nation = "Nation";
      rp.Participant.Club = "Verein";
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: 2.0);
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: 3.0);

      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"), points: 1.5); // 4
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"), points: 2.5);
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"), points: 3.5); // 6

      RaceRun rr1 = race.GetRun(0);
      RaceRun rr2 = race.GetRun(1);
      rr1.SetStartFinishTime(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0));
      rr2.SetRunTime(race.GetParticipant(1), new TimeSpan(0, 0, 2, 0, 123));

      rr1.SetRunTime(race.GetParticipant(2), new TimeSpan(0, 1, 1));
      rr1.SetResultCode(race.GetParticipant(3), RunResult.EResultCode.NiZ);

      DSVAlpinExport export = new DSVAlpinExport(tg.Model.GetRace(0));
      DataSet ds = export.ExportToDataSet();

      // Check Column Names
      int i = 0;
      Assert.AreEqual("Idnr", ds.Tables[0].Columns[i++].ColumnName);
      Assert.AreEqual("Stnr", ds.Tables[0].Columns[i++].ColumnName);
      Assert.AreEqual("DSV-ID", ds.Tables[0].Columns[i++].ColumnName);
      Assert.AreEqual("Name", ds.Tables[0].Columns[i++].ColumnName);
      Assert.AreEqual("Kateg", ds.Tables[0].Columns[i++].ColumnName);
      Assert.AreEqual("JG", ds.Tables[0].Columns[i++].ColumnName);
      Assert.AreEqual("V/G", ds.Tables[0].Columns[i++].ColumnName);
      Assert.AreEqual("Verein", ds.Tables[0].Columns[i++].ColumnName);
      Assert.AreEqual("LPkte", ds.Tables[0].Columns[i++].ColumnName);
      Assert.AreEqual("Total", ds.Tables[0].Columns[i++].ColumnName);
      Assert.AreEqual("Zeit 1", ds.Tables[0].Columns[i++].ColumnName);
      Assert.AreEqual("Zeit 2", ds.Tables[0].Columns[i++].ColumnName);
      Assert.AreEqual("Klasse", ds.Tables[0].Columns[i++].ColumnName);
      Assert.AreEqual("Gruppe", ds.Tables[0].Columns[i++].ColumnName);
      Assert.AreEqual("RPkte", ds.Tables[0].Columns[i++].ColumnName);

      // Check first participant
      Assert.AreEqual(1U, ds.Tables[0].Rows[0]["Stnr"]);
      Assert.AreEqual("123", ds.Tables[0].Rows[0]["DSV-ID"]);
      Assert.AreEqual("Name 1, Firstname 1", ds.Tables[0].Rows[0]["Name"]);
      Assert.AreEqual("M", ds.Tables[0].Rows[0]["Kateg"]);
      Assert.AreEqual(2010u, ds.Tables[0].Rows[0]["JG"]);
      Assert.AreEqual("Nation", ds.Tables[0].Rows[0]["V/G"]);
      Assert.AreEqual("Verein", ds.Tables[0].Rows[0]["Verein"]);
      Assert.AreEqual("1,00", ds.Tables[0].Rows[0]["LPkte"]);
      Assert.AreEqual("1:00,00", ds.Tables[0].Rows[0]["Total"]); // BestRun => 60.0
      Assert.AreEqual("1:00,00", ds.Tables[0].Rows[0]["Zeit 1"]);
      Assert.AreEqual("2:00,12", ds.Tables[0].Rows[0]["Zeit 2"]);
      Assert.AreEqual("Class 2M (2010)", ds.Tables[0].Rows[0]["Klasse"]);
      Assert.AreEqual("Group 2M", ds.Tables[0].Rows[0]["Gruppe"]);
      Assert.AreEqual("---", ds.Tables[0].Rows[0]["RPkte"]);

      // Participant 3, Run1: NIZ
      Assert.AreEqual("NIZ1", ds.Tables[0].Rows[2]["Total"]);
      Assert.AreEqual("NIZ", ds.Tables[0].Rows[2]["Zeit 1"]);
      Assert.AreEqual(DBNull.Value, ds.Tables[0].Rows[2]["Zeit 2"]);
    }
  }
}
