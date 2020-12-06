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

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for LiveTimingRMTest
  /// </summary>
  [TestClass]
  public class LiveTimingRMTest
  {
    public LiveTimingRMTest()
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
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_LiveTiming.mdb")]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_LiveTiming_GiantSlalom.config")]
    public void TestSerialization()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants_LiveTiming.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      AppDataModel model = new AppDataModel(db);

      model.SetCurrentRace(model.GetRaces()[0]);
      model.SetCurrentRaceRun(model.GetCurrentRace().GetRun(0));

      LiveTimingRM cl = new LiveTimingRM(model.GetCurrentRace(), "01122", "livetiming", "livetiming");
      //cl.Init();

      string classes = cl.getClasses();
      Assert.AreEqual(
        "Klasse|20|Mädchen 2014|1\n" +
        "Klasse|18|Buben 2014|2\n" +
        "Klasse|19|Mädchen 2013|3\n" +
        "Klasse|17|Buben 2013|4\n" +
        "Klasse|5|Mädchen 2012|5\n" +
        "Klasse|7|Buben 2012|6\n" +
        "Klasse|6|Mädchen 2011|7\n" +
        "Klasse|8|Buben 2011|8\n" +
        "Klasse|9|Mädchen 2010|9\n" +
        "Klasse|11|Buben 2010|10\n" +
        "Klasse|10|Mädchen 2009|11\n" +
        "Klasse|12|Buben 2009|12"
        , classes );


      string groups = cl.getGroups();
      Assert.AreEqual(
        "Gruppe|9|Bambini weiblich|1\n" +
        "Gruppe|2|Bambini männlich|2\n" +
        "Gruppe|3|U8 weiblich|3\n" +
        "Gruppe|4|U8 männlich|4\n" +
        "Gruppe|5|U10 weiblich|5\n" +
        "Gruppe|6|U10 männlich|6"
        , groups);

      string categories = cl.getCategories();
      Assert.AreEqual("Kategorie|M|M|1\nKategorie|W|W|2", categories);

      string participants = cl.getParticipantsData();
      Assert.AreEqual(
          "W|5|10|1|1||Nachname 1, Vorname 1|2009|Nation 1|Verein 1|9999,99\nM|2|17|2|2||Nachname 2, Vorname 2|2013|Nation 2|Verein 2|9999,99\nM|4|8|3|3||Nachname 3, Vorname 3|2011|Nation 3|Verein 3|9999,99\nW|9|20|4|4||Nachname 4, Vorname 4|2014|Nation 4|Verein 4|9999,99\nM|4|7|5|5||Nachname 5, Vorname 5|2012|Nation 5|Verein 5|9999,99"
        , participants);
      string startList = cl.getStartListData(model.GetCurrentRaceRun());
      Assert.AreEqual(
        "  4\n  2\n  5\n  3\n  1", 
        startList);

      string timingData = cl.getTimingData(model.GetCurrentRaceRun());
      Assert.AreEqual("  10000010,23\n  29000000,01\n  31999999,99\n  42999999,99\n  53999999,99", timingData);
    }

    [TestMethod]
    //[Ignore]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_LiveTiming.mdb")]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_LiveTiming_GiantSlalom.config")]
    public void TestOnline()
    {
      //string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants_LiveTiming.mdb");
      //RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      //db.Connect(dbFilename);
      //AppDataModel model = new AppDataModel(db);

      //model.SetCurrentRace(model.GetRaces()[0]);
      //model.SetCurrentRaceRun(model.GetCurrentRace().GetRun(0));

      //LiveTimingRM cl = new LiveTimingRM(model.GetCurrentRace(), "01122", "livetiming", "livetiming");
      //cl.Login();

      //cl.startLiveTiming();

      //cl.Test1();
      //cl.Test2();

    }
  }
}
