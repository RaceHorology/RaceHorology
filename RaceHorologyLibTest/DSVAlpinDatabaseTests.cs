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
using System.IO;
using System.Data.OleDb;
using System.Linq;
using static RaceHorologyLib.PrintCertificateModel;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for DSVAlpinDatabaseTests
  /// </summary>
  [TestClass]
  public class DSVAlpinDatabaseTests
  {
    public DSVAlpinDatabaseTests()
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

    #region Generic Datebase Tests

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    public void DatabaseOpenClose()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");

      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      var participants = db.GetParticipants();

      Assert.IsTrue(participants.Count() == 5);
      Assert.IsTrue(participants.Where(x => x.Name == "Nachname 3").Count() == 1);


      string dbStatusFile = System.IO.Path.Combine(
        db.GetDBPathDirectory(),
        System.IO.Path.GetFileNameWithoutExtension(db.GetDBFileName()) + ".ldb");

      Assert.IsTrue(System.IO.File.Exists(dbStatusFile), "MS-Access Status File existing");

      Assert.ThrowsException<System.IO.IOException>(() => { System.IO.File.Delete(db.GetDBPath()); });

      db.Close();
      Assert.IsFalse(System.IO.File.Exists(dbStatusFile), "MS-Access Status File not existing anymore");

      System.IO.File.Delete(db.GetDBPath()); // Simply succeeds
      Assert.IsFalse(System.IO.File.Exists(db.GetDBPath()), "MS-Access file could be deleted");
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    public void DatabaseOpenCloseWithAppDataModel()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");

      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel dm = new AppDataModel(db);
      var participants = dm.GetParticipants();
      Assert.IsTrue(participants.Count() == 5);
      Assert.IsTrue(participants.Where(x => x.Name == "Nachname 3").Count() == 1);

      string dbStatusFile = System.IO.Path.Combine(
        db.GetDBPathDirectory(),
        System.IO.Path.GetFileNameWithoutExtension(db.GetDBFileName()) + ".ldb");

      Assert.IsTrue(System.IO.File.Exists(dbStatusFile), "MS-Access Status File existing");

      Assert.ThrowsException<System.IO.IOException>(() => { System.IO.File.Delete(db.GetDBPath()); });

      dm.Close();
      Assert.IsFalse(System.IO.File.Exists(dbStatusFile), "MS-Access Status File not existing anymore");

      System.IO.File.Delete(db.GetDBPath()); // Simply succeeds
      Assert.IsFalse(System.IO.File.Exists(db.GetDBPath()), "MS-Access file could be deleted");
    }


    [TestMethod]
    public void DatabaseCreate()
    {
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      string dbFilename = db.CreateDatabase("new.mdb");
      db.Connect(dbFilename);

      var participants = db.GetParticipants();
      Assert.IsTrue(participants.Count() == 0);

      Assert.AreEqual("new", db.GetCompetitionProperties().Name);

      db.Close();

      Assert.IsTrue(checkDBVersion(dbFilename));
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_Schema_V0.3.mdb")]
    public void DatabaseUpgradeSchema_RHMisc()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_Schema_V0.3.mdb");

      Assert.IsFalse(existsTable(dbFilename, "RHMisc"), "table 'RHMisc' not yet existing");

      // Open first time, upgrade will be performed
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      db.Close();

      Assert.IsTrue(existsTable(dbFilename, "RHMisc"), "table 'RHMisc' is existing");

      // open second time (when upgrade was performed)
      db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      db.Close();

      Assert.IsTrue(checkDBVersion(dbFilename));

      Assert.IsTrue(existsTable(dbFilename, "RHMisc"), "table 'RHMisc' is still existing");
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_Schema_V0.3.mdb")]
    public void DatabaseUpgradeSchema_tblKategorie()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_Schema_V0.3.mdb");

      Assert.IsFalse(existsColumn(dbFilename, "tblKategorie", "RHSynonyms"));

      // Open first time, upgrade will be performed
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      db.Close();

      Assert.IsTrue(existsColumn(dbFilename, "tblKategorie", "RHSynonyms"));

      // open second time (when upgrade was performed)
      db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      db.Close();

      Assert.IsTrue(checkDBVersion(dbFilename));

      Assert.IsTrue(existsColumn(dbFilename, "tblKategorie", "RHSynonyms"));
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_Schema_V0.3.mdb")]
    public void DatabaseUpgradeSchema_RHTimestamps()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_Schema_V0.3.mdb");

      Assert.IsFalse(existsTable(dbFilename, "RHTimestamps"));

      // Open first time, upgrade will be performed
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      db.Close();

      Assert.IsTrue(existsTable(dbFilename, "RHTimestamps"));

      // open second time (when upgrade was performed)
      db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      db.Close();

      Assert.IsTrue(checkDBVersion(dbFilename));
      Assert.IsTrue(existsTable(dbFilename, "RHTimestamps"));
    }




    bool existsTable(string dbFilename, string tableName)
    {
      OleDbConnection conn = new OleDbConnection { ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data source= " + dbFilename };
      conn.Open();

      var schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

      return
        schema.Rows
          .OfType<System.Data.DataRow>()
          .Any(r => r.ItemArray[2].ToString().ToLower() == tableName.ToLower());
    }

    bool existsColumn(string dbFilename, string tableName, string column)
    {
      using (OleDbConnection conn = new OleDbConnection { ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data source= " + dbFilename })
      {
        conn.Open();

        System.Data.DataTable schema = conn.GetSchema("COLUMNS");

        var col = schema.Select("TABLE_NAME='" + tableName + "' AND COLUMN_NAME='" + column + "'");

        return col.Length > 0;
      }
    }

    bool checkDBVersion(string dbFilename, int version = 18)
    {
      bool res = false;

      using (OleDbConnection conn = new OleDbConnection { ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data source= " + dbFilename })
      {
        conn.Open();

        string sql = @"SELECT * FROM tblVersion";
        OleDbCommand command = new OleDbCommand(sql, conn);

        using (OleDbDataReader reader = command.ExecuteReader())
        {
          if (reader.Read())
            if ((int)reader.GetValue(reader.GetOrdinal("version")) == version)
              res = true;
        }
      }

      return res;
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    public void StoreGetKeyValue()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      db.StoreKeyValue("key1", "value1");
      Assert.AreEqual("value1", db.GetKeyValue("key1"));

      db.StoreKeyValue("key2", "value2");
      Assert.AreEqual("value2", db.GetKeyValue("key2"));

      db.StoreKeyValue("key1", "value12");
      Assert.AreEqual("value12", db.GetKeyValue("key1"));

      Assert.AreEqual(null, db.GetKeyValue("keyXXX"));
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    public void InitializeApplicationModel()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);
    }

    #endregion

    #region Competition
    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case2\1554MSBS.mdb")]
    public void TestCompetitionProperties()
    {
      {
        string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");
        RaceHorologyLib.Database db = new RaceHorologyLib.Database();
        db.Connect(dbFilename);

        CompetitionProperties propShall = new CompetitionProperties();
        CompetitionProperties propIs = null;

        propShall.WithPoints = true;
        db.UpdateCompetitionProperties(propShall); propIs = db.GetCompetitionProperties(); TestUtilities.AreEqualByJson(propShall, propIs);

        propShall.Nation = "REG";
        db.UpdateCompetitionProperties(propShall); propIs = db.GetCompetitionProperties(); TestUtilities.AreEqualByJson(propShall, propIs);

        propShall.MannschaftsWertung = true;
        db.UpdateCompetitionProperties(propShall); propIs = db.GetCompetitionProperties(); TestUtilities.AreEqualByJson(propShall, propIs);

        propShall.Saeson = 1999;
        db.UpdateCompetitionProperties(propShall); propIs = db.GetCompetitionProperties(); TestUtilities.AreEqualByJson(propShall, propIs);
      }

      {
        string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");
        RaceHorologyLib.Database db = new RaceHorologyLib.Database();
        db.Connect(dbFilename);

        var competitionProps = db.GetCompetitionProperties();
        Assert.AreEqual("Zwergerlrennen 2019", competitionProps.Name);
        Assert.AreEqual(CompetitionProperties.ECompetitionType.ClubInternal_Sum, competitionProps.Type);
        Assert.AreEqual(false, competitionProps.WithPoints);
        Assert.AreEqual("GER", competitionProps.Nation);
        Assert.AreEqual(2019U, competitionProps.Saeson);
        Assert.AreEqual(true, competitionProps.KlassenWertung);
        Assert.AreEqual(false, competitionProps.MannschaftsWertung);
        Assert.AreEqual(false, competitionProps.ZwischenZeit);
        Assert.AreEqual(true, competitionProps.FreierListenKopf);
        Assert.AreEqual(false, competitionProps.FISSuperCombi);
        Assert.AreEqual(true, competitionProps.FieldActiveYear);
        Assert.AreEqual(false, competitionProps.FieldActiveClub);
        Assert.AreEqual(false, competitionProps.FieldActiveNation);
        Assert.AreEqual(false, competitionProps.FieldActiveCode);
        Assert.AreEqual(10.0, competitionProps.Nenngeld);

        db.UpdateCompetitionProperties(competitionProps);
        var propIs = db.GetCompetitionProperties();
        TestUtilities.AreEqualByJson(competitionProps, propIs);
      }
      {
        string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"1554MSBS.mdb");
        RaceHorologyLib.Database db = new RaceHorologyLib.Database();
        db.Connect(dbFilename);

        var competitionProps = db.GetCompetitionProperties();
        Assert.AreEqual("1554MSBS", competitionProps.Name);
        Assert.AreEqual(CompetitionProperties.ECompetitionType.DSV_SchoolPoints, competitionProps.Type);
        Assert.AreEqual(true, competitionProps.WithPoints);
        Assert.AreEqual("AUT", competitionProps.Nation);
        Assert.AreEqual(2020U, competitionProps.Saeson);
        Assert.AreEqual(true, competitionProps.KlassenWertung);
        Assert.AreEqual(false, competitionProps.MannschaftsWertung);
        Assert.AreEqual(false, competitionProps.ZwischenZeit);
        Assert.AreEqual(false, competitionProps.FreierListenKopf);
        Assert.AreEqual(false, competitionProps.FISSuperCombi);
        Assert.AreEqual(true, competitionProps.FieldActiveYear);
        Assert.AreEqual(true, competitionProps.FieldActiveClub);
        Assert.AreEqual(true, competitionProps.FieldActiveNation);
        Assert.AreEqual(true, competitionProps.FieldActiveCode);
        Assert.AreEqual(10.0, competitionProps.Nenngeld);


        db.UpdateCompetitionProperties(competitionProps);
        var propIs = db.GetCompetitionProperties();
        TestUtilities.AreEqualByJson(competitionProps, propIs);
      }
    }


    #endregion

    #region Races

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_MultipleRaces.mdb")]
    public void DatabaseRaces()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants_MultipleRaces.mdb");

      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      var races = db.GetRaces();

      {
        var race = races.Where(r => r.RaceType == Race.ERaceType.DownHill).First();
        Assert.AreEqual(2U, race.Runs);
      }
      {
        var race = races.Where(r => r.RaceType == Race.ERaceType.SuperG).First();
        Assert.AreEqual(1U, race.Runs);
      }
      {
        var race = races.Where(r => r.RaceType == Race.ERaceType.GiantSlalom).First();
        Assert.AreEqual(2U, race.Runs);
      }
      {
        var race = races.Where(r => r.RaceType == Race.ERaceType.Slalom).First();
        Assert.AreEqual(1U, race.Runs);
      }

      //
      Assert.AreEqual(0, races.Where(r => r.RaceType == Race.ERaceType.ParallelSlalom).Count());
      Assert.AreEqual(0, races.Where(r => r.RaceType == Race.ERaceType.KOSlalom).Count());
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_Empty.mdb")]
    public void DatabaseModifyRaces()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_Empty.mdb");

      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      AppDataModel model = new AppDataModel(db);

      void DBCacheWorkaround()
      {
        db.Close(); // WORKAROUND: OleDB caches the update, so the Check would not see the changes
        db.Connect(dbFilename);
        model = new AppDataModel(db);
      }


      Race.RaceProperties raceProp = new Race.RaceProperties
      {
        RaceType = Race.ERaceType.SuperG,
        Runs = 2
      };

      model.AddRace(raceProp);
      DBCacheWorkaround();
      Assert.IsTrue(checkRace(dbFilename, raceProp, true));

      model.RemoveRace(model.GetRaces().FirstOrDefault(r => r.RaceType == Race.ERaceType.SuperG));
      DBCacheWorkaround();
      Assert.IsTrue(checkRace(dbFilename, raceProp, false));
    }

    bool checkRace(string dbFilename, Race.RaceProperties raceProps, bool active)
    {
      bool bRes = true;

      OleDbConnection conn = new OleDbConnection { ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data source= " + dbFilename };
      conn.Open();

      string sql = @"SELECT * FROM tblDisziplin WHERE dtyp = @dtyp";
      OleDbCommand command = new OleDbCommand(sql, conn);
      command.Parameters.Add(new OleDbParameter("@dtyp", (int)raceProps.RaceType));

      using (OleDbDataReader reader = command.ExecuteReader())
      {
        if (reader.Read())
        {
          bRes &= ((bool)reader.GetValue(reader.GetOrdinal("aktiv")) == active);

          bRes &= (uint)(byte)reader.GetValue(reader.GetOrdinal("durchgaenge")) == raceProps.Runs;
        }
        else
          bRes = false;
      }

      conn.Close();

      return bRes;
    }


    /// <summary>
    /// Modifies different properties and reads them afterwards
    /// </summary>
    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_Empty.mdb")]
    public void DatabaseModifyRaceProperties()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_Empty.mdb");

      {
        RaceHorologyLib.Database db = new RaceHorologyLib.Database();
        db.Connect(dbFilename);
        AppDataModel model = new AppDataModel(db);


        Race r1 = model.GetRace(0);

        // Check initially
        Assert.AreEqual("MeinBewerb", r1.AdditionalProperties.Description);
        Assert.AreEqual(new DateTime(2019, 1, 19), r1.AdditionalProperties.DateStartList);
        Assert.AreEqual(new DateTime(2019, 1, 20), r1.AdditionalProperties.DateResultList);
        Assert.AreEqual("20190120", r1.AdditionalProperties.RaceNumber);

        // Modify
        var p1 = r1.AdditionalProperties;
        p1.Description = "Descr1";
        p1.DateStartList = new DateTime(2020, 1, 2);
        p1.DateResultList = new DateTime(2020, 1, 3);
        p1.RaceNumber = "ABCDEF123456";
        // Store
        r1.AdditionalProperties = p1; // Implicitly calls: db.StoreRaceProperties()

        db.Close();
      }

      {
        RaceHorologyLib.Database db = new RaceHorologyLib.Database();
        db.Connect(dbFilename);
        AppDataModel model = new AppDataModel(db);

        Race r1 = model.GetRace(0);

        Assert.AreEqual("Descr1", r1.AdditionalProperties.Description);
        Assert.AreEqual(new DateTime(2020, 1, 2), r1.AdditionalProperties.DateStartList);
        Assert.AreEqual(new DateTime(2020, 1, 3), r1.AdditionalProperties.DateResultList);
        Assert.AreEqual("ABCDEF123456", r1.AdditionalProperties.RaceNumber);
      }
    }

    /// <summary>
    /// Check reading different race runs
    /// </summary>
    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    public void DatabaseRaceRuns()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      db.GetParticipants();

      AppDataModel model = new AppDataModel(db);

      Race.RaceProperties rprops = new Race.RaceProperties();
      rprops.RaceType = Race.ERaceType.GiantSlalom;
      rprops.Runs = 2;
      Race race = new Race(db, model, rprops);

      var rr1 = db.GetRaceRun(race, 1);
      var rr2 = db.GetRaceRun(race, 2);

      Assert.IsTrue(rr1.Count() == 4);
      Assert.IsTrue(rr2.Count() == 4);

      Assert.IsTrue(rr1.Where(x => x.GetFinishTime() == null && x.GetStartTime() != null).First().Participant.Participant.Name == "Nachname 3");

      Assert.IsTrue(rr2.Where(x => x.GetFinishTime() == null && x.GetStartTime() != null).First().Participant.Participant.Name == "Nachname 2");

      Assert.IsTrue(rr2.Where(x => x.Participant.Participant.Name == "Nachname 5").Count() == 0);

      db.Close();
    }

    #endregion

    #region Participant
    /// <summary>
    /// Check different participants per race
    /// </summary>
    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_MultipleRaces.mdb")]
    public void DatabaseRaceParticipants()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants_MultipleRaces.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      var races = db.GetRaces();
      AppDataModel model = new AppDataModel(db);

      {
        var race = races.Where(r => r.RaceType == Race.ERaceType.DownHill).First();
        var raceParticipants = db.GetRaceParticipants(new Race(db, model, race));
        Assert.AreEqual(6, raceParticipants.Count());
        Assert.AreEqual(1, raceParticipants.Where(p => p.Participant.Name == "Nachname 6").Count());
        Assert.AreEqual(0, raceParticipants.Where(p => p.Participant.Name == "Nachname 10").Count());

        Assert.AreEqual(100.0, raceParticipants.Where(p => p.Participant.Name == "Nachname 6").First().Points);
      }
      {
        var race = races.Where(r => r.RaceType == Race.ERaceType.SuperG).First();
        var raceParticipants = db.GetRaceParticipants(new Race(db, model, race));
        Assert.AreEqual(6, raceParticipants.Count());
        Assert.AreEqual(1, raceParticipants.Where(p => p.Participant.Name == "Nachname 7").Count());
        Assert.AreEqual(0, raceParticipants.Where(p => p.Participant.Name == "Nachname 10").Count());

        Assert.AreEqual(200.1, raceParticipants.Where(p => p.Participant.Name == "Nachname 7").First().Points);
      }
      {
        var race = races.Where(r => r.RaceType == Race.ERaceType.GiantSlalom).First();
        var raceParticipants = db.GetRaceParticipants(new Race(db, model, race));
        Assert.AreEqual(6, raceParticipants.Count());
        Assert.AreEqual(1, raceParticipants.Where(p => p.Participant.Name == "Nachname 8").Count());
        Assert.AreEqual(0, raceParticipants.Where(p => p.Participant.Name == "Nachname 10").Count());

        Assert.AreEqual(9999.98, raceParticipants.Where(p => p.Participant.Name == "Nachname 8").First().Points);
      }
      {
        var race = races.Where(r => r.RaceType == Race.ERaceType.Slalom).First();
        var raceParticipants = db.GetRaceParticipants(new Race(db, model, race));
        Assert.AreEqual(6, raceParticipants.Count());
        Assert.AreEqual(1, raceParticipants.Where(p => p.Participant.Name == "Nachname 9").Count());
        Assert.AreEqual(0, raceParticipants.Where(p => p.Participant.Name == "Nachname 10").Count());

        Assert.AreEqual(0.0, raceParticipants.Where(p => p.Participant.Name == "Nachname 9").First().Points);
      }
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_Empty.mdb")]
    public void CreateAndUpdateParticipants()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_Empty.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      var participants = db.GetParticipants();

      void DBCacheWorkaround()
      {
        db.Close(); // WORKAROUND: OleDB caches the update, so the Check would not see the changes
        db.Connect(dbFilename);
        participants = db.GetParticipants();
      }

      Participant pNew1 = new Participant
      {
        Name = "Nachname 6",
        Firstname = "Vorname 6",
        Sex = new ParticipantCategory('M'),
        Club = "Verein 6",
        Nation = "GER",
        SvId = "123",
        Code = "321",
        Class = db.GetParticipantClasses()[0],
        Year = 2009
      };
      db.CreateOrUpdateParticipant(pNew1);
      DBCacheWorkaround();
      Assert.IsTrue(CheckParticipant(dbFilename, pNew1, 1));


      Participant pNew2 = new Participant
      {
        Name = "Nachname 7",
        Firstname = "Vorname 7",
        Sex = new ParticipantCategory('M'),
        Club = "Verein 7",
        Nation = "GER",
        Class = db.GetParticipantClasses()[1],
        Year = 2010
      };
      db.CreateOrUpdateParticipant(pNew2);
      DBCacheWorkaround();
      Assert.IsTrue(CheckParticipant(dbFilename, pNew2, 2));


      // Create with non-mandatory properties
      Participant pNew3 = new Participant
      {
        Name = "Nachname 8",
        Firstname = "Vorname 8",
        Sex = null,
        Club = "",
        Nation = "",
        Class = db.GetParticipantClasses()[2],
        Year = 2010
      };
      db.CreateOrUpdateParticipant(pNew3);
      DBCacheWorkaround();
      Assert.IsTrue(CheckParticipant(dbFilename, pNew3, 3));


      // Update a Participant
      pNew1 = participants.Where(x => x.Name == "Nachname 6").FirstOrDefault();
      pNew1.Name = "Nachname 6.1";
      pNew1.Firstname = "Vorname 6.1";
      pNew1.Sex = new ParticipantCategory('W');
      pNew1.Club = "Verein 6.1";
      pNew1.Nation = "GDR";
      pNew1.Class = db.GetParticipantClasses()[0];
      pNew1.Year = 2008;
      db.CreateOrUpdateParticipant(pNew1);
      DBCacheWorkaround();
      Assert.IsTrue(CheckParticipant(dbFilename, pNew1, 1));

      // Update with non-mandatory properties
      pNew1 = participants.Where(x => x.Name == "Nachname 6.1").FirstOrDefault();
      pNew1.Name = "Nachname 6.2";
      pNew1.Firstname = "Vorname 6.2";
      pNew1.Sex = null;
      pNew1.Club = "";
      pNew1.Nation = "";
      pNew1.Class = db.GetParticipantClasses()[0];
      pNew1.Year = 2008;
      db.CreateOrUpdateParticipant(pNew1);
      DBCacheWorkaround();
      Assert.IsTrue(CheckParticipant(dbFilename, pNew1, 1));

    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    public void DeleteParticipants()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      var participants = db.GetParticipants();

      void DBCacheWorkaround()
      {
        db.Close(); // WORKAROUND: OleDB caches the update, so the Check would not see the changes
        db.Connect(dbFilename);
        participants = db.GetParticipants();
      }

      Participant parDel1 = participants[0];
      db.RemoveParticipant(parDel1);

      DBCacheWorkaround();

      CheckParticipant(dbFilename, null, int.Parse(parDel1.Id));
    }



    private bool CheckParticipant(string dbFilename, Participant participant, int id)
    {
      bool bRes = true;

      OleDbConnection conn = new OleDbConnection { ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data source= " + dbFilename };
      conn.Open();

      string sql = @"SELECT * FROM tblTeilnehmer WHERE id = @id";
      OleDbCommand command = new OleDbCommand(sql, conn);
      command.Parameters.Add(new OleDbParameter("@id", id));

      // Execute command  
      using (OleDbDataReader reader = command.ExecuteReader())
      {
        if (reader.Read())
        {
          string s = reader["nachname"].ToString();
          bRes &= participant.Name == reader["nachname"].ToString();
          bRes &= participant.Firstname == reader["vorname"].ToString();

          if (participant.Sex == null)
            bRes &= reader["sex"] == DBNull.Value;
          else
            bRes &= participant.Sex.Name == reader["sex"].ToString()[0];

          bRes &= participant.Club == reader["verein"].ToString();
          bRes &= participant.Nation == reader["nation"].ToString();
          bRes &= checkStringAgainstDB(participant.SvId, reader["svid"]);
          bRes &= checkStringAgainstDB(participant.Code, reader["code"]);
          bRes &= checkStringAgainstDB(participant.Class.Id, reader["klasse"]);
          bRes &= participant.Year == reader.GetInt16(reader.GetOrdinal("jahrgang"));
          //bRes &= participant.StartNumber == GetStartNumber(reader);
        }
        else
        {
          bRes = false;

          if (participant == null)
            bRes = true;
        }
      }

      conn.Close();

      return bRes;
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    public void AppDataModelTest_EditParticipants()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      void DBCacheWorkaround()
      {
        db.Close(); // WORKAROUND: OleDB caches the update, so the Check would not see the changes
        db.Connect(dbFilename);
      }

      AppDataModel model = new AppDataModel(db);

      Participant participant1 = db.GetParticipants().Where(x => x.Name == "Nachname 1").FirstOrDefault();
      participant1.Name = "Nachname 1.1";

      Participant participant6 = new Participant
      {
        Name = "Nachname 6",
        Firstname = "Vorname 6",
        Sex = new ParticipantCategory('M'),
        Club = "Verein 6",
        Nation = "Nation 6",
        Class = new ParticipantClass("", null, "dummy", new ParticipantCategory('M'), 2019, 0),
        Year = 2000,
      };
      model.GetParticipants().Add(participant6);


      DBCacheWorkaround();


      // Test 1: Check whether database is correct
      CheckParticipant(dbFilename, participant1, 1);
      CheckParticipant(dbFilename, participant6, 6);
    }

    #endregion

    #region Categories and Classes and Groups



    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_Empty.mdb")]
    public void CreateAndUpdateAndDeleteGroups()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_Empty.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      var groups = db.GetParticipantGroups();

      void DBCacheWorkaround()
      {
        db.Close(); // WORKAROUND: OleDB caches the update, so the Check would not see the changes
        db.Connect(dbFilename);
        groups = db.GetParticipantGroups();
      }

      Assert.AreEqual(6, db.GetParticipantGroups().Count);

      // Edit existing one
      {
        var g = groups.FirstOrDefault(v => v.Id == "5");
        Assert.AreEqual("U10 weiblich", g.Name);
        g.Name = "U10 modified";
        db.CreateOrUpdateGroup(g);
        DBCacheWorkaround();
        Assert.IsTrue(CheckGroup(dbFilename, g, ulong.Parse(g.Id)));
        Assert.AreEqual(6, db.GetParticipantGroups().Count);
      }

      // Create new one
      {
        var g = new ParticipantGroup(null, "Group 1", 1);
        db.CreateOrUpdateGroup(g);
        DBCacheWorkaround();
        Assert.IsTrue(CheckGroup(dbFilename, g, ulong.Parse(g.Id)));
        Assert.AreEqual(7, db.GetParticipantGroups().Count);
      }

      // Delete one
      {
        var g = groups.FirstOrDefault(v => v.Id == "10");
        db.RemoveGroup(g);
        DBCacheWorkaround();

        g = groups.FirstOrDefault(v => v.Id == "10");
        Assert.IsNull(g);

        Assert.AreEqual(6, db.GetParticipantGroups().Count);
      }
    }
    bool CheckGroup(string dbFilename, ParticipantGroup groupShall, ulong id)
    {
      bool bRes = true;

      OleDbConnection conn = new OleDbConnection { ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data source= " + dbFilename };
      conn.Open();

      string sql = @"SELECT * FROM tblGruppe WHERE id = @id";
      OleDbCommand command = new OleDbCommand(sql, conn);
      command.Parameters.Add(new OleDbParameter("@id", id));

      // Execute command  
      using (OleDbDataReader reader = command.ExecuteReader())
      {
        if (reader.Read())
        {
          bRes &= groupShall.Name == reader["grpname"].ToString();
          bRes &= groupShall.SortPos == (double)reader["sortpos"];
        }
        else
          bRes = false;
      }
      conn.Close();

      return bRes;
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_Empty.mdb")]
    public void CreateAndUpdateAndDeleteCategories()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_Empty.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      var categories = db.GetParticipantCategories();

      void DBCacheWorkaround()
      {
        db.Close(); // WORKAROUND: OleDB caches the update, so the Check would not see the changes
        db.Connect(dbFilename);
        categories = db.GetParticipantCategories();
      }

      Assert.AreEqual(14, db.GetParticipantCategories().Count);

      // Edit existing one
      {
        var g = categories.FirstOrDefault(v => v.Name == 'M');
        Assert.AreEqual("Herren", g.PrettyName);
        Assert.AreEqual(3U, g.SortPos);
        g.PrettyName = "Herren modified";
        g.Synonyms = "H";
        db.CreateOrUpdateCategory(g);
        DBCacheWorkaround();
        Assert.IsTrue(CheckCategory(dbFilename, g, g.Name));
        Assert.AreEqual(14, db.GetParticipantCategories().Count);
      }

      // Create new one
      {
        var g = new ParticipantCategory('X', "XXX", 999);
        db.CreateOrUpdateCategory(g);
        DBCacheWorkaround();
        Assert.IsTrue(CheckCategory(dbFilename, g, g.Name));
        Assert.AreEqual(15, db.GetParticipantCategories().Count);
      }

      // Delete one
      {
        var g = categories.FirstOrDefault(v => v.Name == '0');
        db.RemoveCategory(g);
        DBCacheWorkaround();
        g = categories.FirstOrDefault(v => v.Name == '0');
        Assert.IsNull(g);
        Assert.AreEqual(14, db.GetParticipantCategories().Count);
      }
    }

    bool CheckCategory(string dbFilename, ParticipantCategory categShall, char name)
    {
      bool bRes = true;

      OleDbConnection conn = new OleDbConnection { ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data source= " + dbFilename };
      conn.Open();

      string sql = @"SELECT * FROM tblKategorie WHERE kat = @name";
      OleDbCommand command = new OleDbCommand(sql, conn);
      command.Parameters.Add(new OleDbParameter("@name", name));

      // Execute command  
      using (OleDbDataReader reader = command.ExecuteReader())
      {
        if (reader.Read())
        {
          bRes &= checkStringAgainstDB(categShall.PrettyName, reader["kname"]);
          bRes &= checkStringAgainstDB(categShall.Synonyms, reader["RHSynonyms"]);
          bRes &= categShall.SortPos == (double)reader["sortpos"];

        }
        else
          bRes = false;
      }
      conn.Close();

      return bRes;
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_Empty.mdb")]
    public void CreateAndUpdateAndDeleteClasses()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_Empty.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      var classes = db.GetParticipantClasses();

      void DBCacheWorkaround()
      {
        db.Close(); // WORKAROUND: OleDB caches the update, so the Check would not see the changes
        db.Connect(dbFilename);
        classes = db.GetParticipantClasses();
      }

      Assert.AreEqual(12, classes.Count);

      // Edit existing one
      {
        var c = classes.FirstOrDefault(v => v.Id == "9");
        Assert.AreEqual("Mädchen 2010", c.Name);
        c.Name = "Mädchen 2010 modified";
        db.CreateOrUpdateClass(c);
        DBCacheWorkaround();
        Assert.IsTrue(CheckClass(dbFilename, c, ulong.Parse(c.Id)));
        Assert.AreEqual(12, classes.Count);
      }

      {
        var c = classes.FirstOrDefault(v => v.Id == "9");
        Assert.IsNotNull(c.Sex);
        c.Sex = null;
        db.CreateOrUpdateClass(c);
        DBCacheWorkaround();
        Assert.IsTrue(CheckClass(dbFilename, c, ulong.Parse(c.Id)));
        Assert.AreEqual(12, classes.Count);
      }

      // Create new one
      {
        var c = new ParticipantClass(null, db.GetParticipantGroups()[0], "Class New 1", new ParticipantCategory('M'), 2000, 99);
        db.CreateOrUpdateClass(c);
        DBCacheWorkaround();
        Assert.IsTrue(CheckClass(dbFilename, c, ulong.Parse(c.Id)));
        Assert.AreEqual(13, classes.Count);
      }

      // Delete one
      {
        var c = classes.FirstOrDefault(v => v.Id == "21");
        db.RemoveClass(c);
        DBCacheWorkaround();

        c = classes.FirstOrDefault(v => v.Id == "21");
        Assert.IsNull(c);

        Assert.AreEqual(12, classes.Count);
      }
    }

    bool CheckClass(string dbFilename, ParticipantClass classShall, ulong id)
    {
      bool bRes = true;

      OleDbConnection conn = new OleDbConnection { ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data source= " + dbFilename };
      conn.Open();

      string sql = @"SELECT * FROM tblKlasse WHERE id = @id";
      OleDbCommand command = new OleDbCommand(sql, conn);
      command.Parameters.Add(new OleDbParameter("@id", id));

      // Execute command  
      using (OleDbDataReader reader = command.ExecuteReader())
      {
        if (reader.Read())
        {
          bRes &= classShall.Name == reader["klname"].ToString();
          if (classShall.Sex == null)
            bRes &= reader["geschlecht"] == DBNull.Value;
          else
            bRes &= classShall.Sex.Name == reader["geschlecht"].ToString()[0];
          bRes &= classShall.Year == Convert.ToUInt32(reader["bis_jahrgang"]);
          bRes &= classShall.Group.Id == reader["gruppe"].ToString();
          bRes &= classShall.SortPos == (double)reader["sortpos"];
        }
        else
          bRes = false;
      }
      conn.Close();

      return bRes;
    }


    #endregion

    #region RunResults

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    public void CreateAndUpdateRunResults()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel dataModel = new AppDataModel(db);
      Race race = dataModel.GetCurrentRace();
      RaceRun rr1 = race.GetRun(0);
      RaceRun rr2 = race.GetRun(1);

      void DBCacheWorkaround()  // WORKAROUND: OleDB caches the update, so the Check would not see the changes
      {
        db.Close();
        db = new RaceHorologyLib.Database();
        db.Connect(dbFilename);
        dataModel = new AppDataModel(db);
        race = dataModel.GetCurrentRace();
        rr1 = race.GetRun(0);
        rr2 = race.GetRun(1);
      }


      RaceParticipant participant1 = race.GetParticipants().Where(x => x.Name == "Nachname 1").FirstOrDefault();
      RunResult rr1r1 = new RunResult(participant1);

      rr1r1.SetStartTime(new TimeSpan(0, 12, 0, 0, 0)); //int days, int hours, int minutes, int seconds, int milliseconds
      db.CreateOrUpdateRunResult(race, rr1, rr1r1);
      DBCacheWorkaround();
      rr1r1._participant = participant1 = race.GetParticipants().Where(x => x.Name == "Nachname 1").FirstOrDefault();
      Assert.IsTrue(CheckRunResult(dbFilename, rr1r1, 1, 1));

      rr1r1.SetStartTime(rr1r1.GetStartTime()); //int days, int hours, int minutes, int seconds, int milliseconds
      rr1r1.SetFinishTime(new TimeSpan(0, 12, 1, 0, 0)); //int days, int hours, int minutes, int seconds, int milliseconds
      // Test whether run time is stored in DB, Part 1/2 (Preparation)
      Assert.IsNull(rr1.GetRunResult(participant1).GetRunTime(false, false)); 
      db.CreateOrUpdateRunResult(race, rr1, rr1r1);
      DBCacheWorkaround();
      rr1r1._participant = participant1 = race.GetParticipants().Where(x => x.Name == "Nachname 1").FirstOrDefault();
      Assert.IsTrue(CheckRunResult(dbFilename, rr1r1, 1, 1));

      // Test whether run time is stored in DB, Part 2/2
      Assert.AreEqual(new TimeSpan(0, 0, 1, 0, 0), rr1.GetRunResult(participant1).GetRunTime(false, false)); 

      rr1r1.SetStartTime(null); //int days, int hours, int minutes, int seconds, int milliseconds
      rr1r1.SetFinishTime(null); //int days, int hours, int minutes, int seconds, int milliseconds
      rr1r1.SetRunTime(new TimeSpan(0, 0, 1, 1, 110)); //int days, int hours, int minutes, int seconds, int milliseconds
      db.CreateOrUpdateRunResult(race, rr1, rr1r1);
      DBCacheWorkaround();
      rr1r1._participant = participant1 = race.GetParticipants().Where(x => x.Name == "Nachname 1").FirstOrDefault();
      Assert.IsTrue(CheckRunResult(dbFilename, rr1r1, 1, 1));



      rr1r1.ResultCode = RunResult.EResultCode.DIS;
      rr1r1.DisqualText = "TF Tor 9";
      db.CreateOrUpdateRunResult(race, rr1, rr1r1);
      DBCacheWorkaround();
      rr1r1._participant = participant1 = race.GetParticipants().Where(x => x.Name == "Nachname 1").FirstOrDefault();
      Assert.IsTrue(CheckRunResult(dbFilename, rr1r1, 1, 1));

      RaceParticipant participant5 = race.GetParticipants().Where(x => x.Name == "Nachname 5").FirstOrDefault();
      RunResult rr5r1 = new RunResult(participant5);
      rr5r1.SetStartTime(new TimeSpan(0, 12, 1, 1, 1)); //int days, int hours, int minutes, int seconds, int milliseconds
      rr5r1.ResultCode = RunResult.EResultCode.NiZ;
      db.CreateOrUpdateRunResult(race, rr1, rr5r1);
      DBCacheWorkaround();
      rr5r1._participant = participant5 = race.GetParticipants().Where(x => x.Name == "Nachname 5").FirstOrDefault();
      Assert.IsTrue(CheckRunResult(dbFilename, rr5r1, 5, 1));

      RunResult rr5r2 = new RunResult(participant5);
      rr5r2.ResultCode = RunResult.EResultCode.NaS;
      db.CreateOrUpdateRunResult(race, rr2, rr5r2);
      DBCacheWorkaround();
      rr5r2._participant = participant5 = race.GetParticipants().Where(x => x.Name == "Nachname 5").FirstOrDefault();
      Assert.IsTrue(CheckRunResult(dbFilename, rr5r2, 5, 2));

      // Delete
      db.DeleteRunResult(race, rr2, rr5r2);
      DBCacheWorkaround();
      Assert.IsTrue(CheckRunResult(dbFilename, null, 5, 2));
    }


    private bool CheckRunResult(string dbFilename, RunResult runResult, int idParticipant, uint raceRun)
    {
      bool bRes = true;

      OleDbConnection conn = new OleDbConnection { ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data source= " + dbFilename };
      conn.Open();

      OleDbCommand command = new OleDbCommand("SELECT * FROM tblZeit WHERE teilnehmer = @teilnehmer AND disziplin = @disziplin AND durchgang = @durchgang", conn);
      command.Parameters.Add(new OleDbParameter("@teilnehmer", idParticipant));
      command.Parameters.Add(new OleDbParameter("@disziplin", 2)); // TODO: Add correct disiziplin
      command.Parameters.Add(new OleDbParameter("@durchgang", raceRun));

      // Execute command  
      using (OleDbDataReader reader = command.ExecuteReader())
      {
        if (reader.Read())
        {
          bRes &= (byte)runResult.ResultCode == reader.GetByte(reader.GetOrdinal("ergcode"));

          TimeSpan? runTime = null, startTime = null, finishTime = null;
          if (!reader.IsDBNull(reader.GetOrdinal("netto")))
            runTime = Database.CreateTimeSpan((double)reader.GetValue(reader.GetOrdinal("netto")));
          if (!reader.IsDBNull(reader.GetOrdinal("start")))
            startTime = Database.CreateTimeSpan((double)reader.GetValue(reader.GetOrdinal("start")));
          if (!reader.IsDBNull(reader.GetOrdinal("ziel")))
            finishTime = Database.CreateTimeSpan((double)reader.GetValue(reader.GetOrdinal("ziel")));

          bRes &= runResult.GetStartTime() == startTime;
          bRes &= runResult.GetFinishTime() == finishTime;
          bRes &= runResult.GetRunTime(true, false) == runTime;

          if (reader.IsDBNull(reader.GetOrdinal("disqualtext")))
            bRes &= runResult.DisqualText == null || runResult.DisqualText == "";
          else
            bRes &= runResult.DisqualText == reader["disqualtext"].ToString();
        }
        else
        {
          if (runResult == null)
            bRes = true;
          else
            bRes = false;
        }
      }

      conn.Close();

      return bRes;
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    public void AppDataModelTest_TimingScenario1()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      void DBCacheWorkaround()
      {
        db.Close(); // WORKAROUND: OleDB caches the update, so the Check would not see the changes
        db.Connect(dbFilename);
      }

      AppDataModel model = new AppDataModel(db);
      {

        // Create a RaceRun with 2 runs
        //model.CreateRaceRun(2);
        Race race = model.GetCurrentRace();
        RaceRun rr1 = model.GetCurrentRace().GetRun(0);
        RaceRun rr2 = model.GetCurrentRace().GetRun(1);

        RaceParticipant participant1 = race.GetParticipants().Where(x => x.Participant.Name == "Nachname 1").FirstOrDefault();
        rr1.SetRunTime(participant1, null);
        rr1.SetStartTime(participant1, new TimeSpan(0, 12, 0, 0, 0)); // Start
        rr1.SetFinishTime(participant1, new TimeSpan(0, 12, 1, 0, 0)); // Finish


        RaceParticipant participant2 = race.GetParticipants().Where(x => x.Participant.Name == "Nachname 2").FirstOrDefault();
        rr1.SetRunTime(participant2, null);
        rr1.SetStartTime(participant2, new TimeSpan(0, 12, 2, 0, 0)); // Start
        rr1.SetFinishTime(participant2, null); // No Finish
                                               // TODO: Set to NiZ

        RaceParticipant participant3 = race.GetParticipants().Where(x => x.Participant.Name == "Nachname 3").FirstOrDefault();
        rr1.SetRunTime(participant3, null);
        rr1.SetStartFinishTime(participant3, null, null); // NaS

        RaceParticipant participant4 = race.GetParticipants().Where(x => x.Participant.Name == "Nachname 4").FirstOrDefault();
        rr1.SetRunTime(participant4, null);
        rr1.SetStartTime(participant4, new TimeSpan(0, 12, 4, 0, 0)); // Start
        rr1.SetFinishTime(participant4, new TimeSpan(0, 12, 4, 30, 0)); // Finish
        // TODO: Set to Disqualify

      }

      DBCacheWorkaround();

      // Test 1: Check internal app model
      // Test 2: Check whether database is correct
      {
        Race race = model.GetCurrentRace();
        RaceRun rr1 = model.GetCurrentRace().GetRun(0);
        RaceRun rr2 = model.GetCurrentRace().GetRun(1);

        // Participant 1 / Test 1
        RunResult rr1res1 = rr1.GetResultList().Where(x => x._participant.Participant.Name == "Nachname 1").FirstOrDefault();
        Assert.AreEqual(new TimeSpan(0, 12, 0, 0, 0), rr1res1.GetStartTime());
        Assert.AreEqual(new TimeSpan(0, 12, 1, 0, 0), rr1res1.GetFinishTime());
        Assert.AreEqual(new TimeSpan(0, 0, 1, 0, 0), rr1res1.GetRunTime());
        // Participant 1 / Test 2
        Assert.IsTrue(CheckRunResult(dbFilename, rr1res1, 1, 1));

        // Participant 2 / Test 1
        RunResult rr1res2 = rr1.GetResultList().Where(x => x._participant.Participant.Name == "Nachname 2").FirstOrDefault();
        Assert.AreEqual(new TimeSpan(0, 12, 2, 0, 0), rr1res2.GetStartTime());
        Assert.IsNull(rr1res2.GetFinishTime());
        Assert.IsNull(rr1res2.GetRunTime());
        //Assert.Equals(RunResult.EResultCode.NiZ, rr1res2.ResultCode);
        // Participant 2 / Test 2
        Assert.IsTrue(CheckRunResult(dbFilename, rr1res2, 2, 1));

        // Participant 3 / Test 1
        RunResult rr1res3 = rr1.GetResultList().Where(x => x._participant.Participant.Name == "Nachname 3").FirstOrDefault();
        Assert.IsNull(rr1res3.GetStartTime());
        Assert.IsNull(rr1res3.GetFinishTime());
        Assert.IsNull(rr1res3.GetRunTime());
        //Assert.Equals(RunResult.EResultCode.NaS, rr1res3.ResultCode);
        // Participant 3 / Test 2
        Assert.IsTrue(CheckRunResult(dbFilename, rr1res3, 3, 1));

        // Participant 4 / Test 1
        RunResult rr1res4 = rr1.GetResultList().Where(x => x._participant.Participant.Name == "Nachname 4").FirstOrDefault();
        Assert.AreEqual(new TimeSpan(0, 12, 4, 0, 0), rr1res4.GetStartTime());
        Assert.AreEqual(new TimeSpan(0, 12, 4, 30, 0), rr1res4.GetFinishTime());
        Assert.AreEqual(new TimeSpan(0, 0, 0, 30, 0), rr1res4.GetRunTime());
        //Assert.Equals(RunResult.EResultCode.Normal, rr1res4.ResultCode);
        // Participant 4 / Test 2
        Assert.IsTrue(CheckRunResult(dbFilename, rr1res4, 4, 1));
      }
    }

    #endregion

    #region TimeStamps

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    public void CreateAndUpdateTimestamps()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel dataModel = new AppDataModel(db);
      Race race = dataModel.GetCurrentRace();
      RaceRun rr1 = race.GetRun(0);
      RaceRun rr2 = race.GetRun(1);

      void DBCacheWorkaround()  // WORKAROUND: OleDB caches the update, so the Check would not see the changes
      {
        db.Close();
        db = new RaceHorologyLib.Database();
        db.Connect(dbFilename);
        dataModel = new AppDataModel(db);
        race = dataModel.GetCurrentRace();
        rr1 = race.GetRun(0);
        rr2 = race.GetRun(1);
      }


      // Create timestamp
      var ts1 = new Timestamp(new TimeSpan(0, 12, 0, 0, 0), EMeasurementPoint.Start, 1, true);
      db.CreateOrUpdateTimestamp(rr1, ts1);
      DBCacheWorkaround();
      Assert.IsTrue(CheckTimestamp(dbFilename, ts1, rr1, EMeasurementPoint.Start));
      Assert.AreEqual(1, db.GetTimestamps(race, 1).Count);

      // Create timestamp
      var ts2 = new Timestamp(new TimeSpan(0, 12, 0, 0, 1), EMeasurementPoint.Finish, 1, false);
      db.CreateOrUpdateTimestamp(rr1, ts2);
      DBCacheWorkaround();
      Assert.IsTrue(CheckTimestamp(dbFilename, ts2, rr1, EMeasurementPoint.Finish));

      Assert.AreNotEqual(ts1.Time, ts2.Time);
      Assert.AreEqual(2, db.GetTimestamps(race, 1).Count);

      // Modify timestamp
      var ts2b = new Timestamp(new TimeSpan(0, 12, 0, 0, 1), EMeasurementPoint.Finish, 10, true);
      db.CreateOrUpdateTimestamp(rr1, ts2b);
      DBCacheWorkaround();
      Assert.IsTrue(CheckTimestamp(dbFilename, ts2b, rr1, EMeasurementPoint.Finish));
      Assert.AreEqual(2, db.GetTimestamps(race, 1).Count);
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    public void AppDataModelTest_Timestamps()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel dataModel = new AppDataModel(db);
      Race race = dataModel.GetCurrentRace();
      RaceRun rr1 = race.GetRun(0);
      RaceRun rr2 = race.GetRun(1);

      void DBCacheWorkaround()  // WORKAROUND: OleDB caches the update, so the Check would not see the changes
      {
        db.Close();
        db = new RaceHorologyLib.Database();
        db.Connect(dbFilename);
        dataModel = new AppDataModel(db);
        race = dataModel.GetCurrentRace();
        rr1 = race.GetRun(0);
        rr2 = race.GetRun(1);
      }

      rr1.GetTimestamps().Add(new Timestamp(new TimeSpan(0, 12, 0, 0, 0), EMeasurementPoint.Start, 1, true));
      rr1.GetTimestamps().Add(new Timestamp(new TimeSpan(0, 12, 0, 0, 1), EMeasurementPoint.Finish, 1, true));
      rr2.GetTimestamps().Add(new Timestamp(new TimeSpan(0, 13, 0, 0, 0), EMeasurementPoint.Start, 2, true));
      rr2.GetTimestamps().Add(new Timestamp(new TimeSpan(0, 13, 0, 0, 1), EMeasurementPoint.Start, 0, false));
        
      DBCacheWorkaround();
      Assert.AreEqual(2, rr1.GetTimestamps().Count);
      Assert.AreEqual(new TimeSpan(0, 12, 0, 0, 0), rr1.GetTimestamps()[0].Time);
      Assert.AreEqual(1U, rr1.GetTimestamps()[0].StartNumber);
      Assert.AreEqual(new TimeSpan(0, 12, 0, 0, 1), rr1.GetTimestamps()[1].Time);
      Assert.AreEqual(1U, rr1.GetTimestamps()[1].StartNumber);
      Assert.AreEqual(true, rr1.GetTimestamps()[1].Valid);

      Assert.AreEqual(2, rr2.GetTimestamps().Count);
      Assert.AreEqual(new TimeSpan(0, 13, 0, 0, 0), rr2.GetTimestamps()[0].Time);
      Assert.AreEqual(2U, rr2.GetTimestamps()[0].StartNumber);
      Assert.AreEqual(new TimeSpan(0, 13, 0, 0, 1), rr2.GetTimestamps()[1].Time);
      Assert.AreEqual(0U, rr2.GetTimestamps()[1].StartNumber);
      Assert.AreEqual(false, rr2.GetTimestamps()[1].Valid);

      var temp = rr2.GetTimestamps().First(t => t.StartNumber == 0);
      temp.StartNumber = 10;
      temp.Valid = true;
      DBCacheWorkaround();
      Assert.AreEqual(2, rr1.GetTimestamps().Count);
      Assert.AreEqual(new TimeSpan(0, 12, 0, 0, 0), rr1.GetTimestamps()[0].Time);
      Assert.AreEqual(1U, rr1.GetTimestamps()[0].StartNumber);
      Assert.AreEqual(new TimeSpan(0, 12, 0, 0, 1), rr1.GetTimestamps()[1].Time);
      Assert.AreEqual(1U, rr1.GetTimestamps()[1].StartNumber);

      Assert.AreEqual(2, rr2.GetTimestamps().Count);
      Assert.AreEqual(new TimeSpan(0, 13, 0, 0, 0), rr2.GetTimestamps()[0].Time);
      Assert.AreEqual(2U, rr2.GetTimestamps()[0].StartNumber);
      Assert.AreEqual(new TimeSpan(0, 13, 0, 0, 1), rr2.GetTimestamps()[1].Time);
      Assert.AreEqual(10U, rr2.GetTimestamps()[1].StartNumber);
      Assert.AreEqual(true, rr2.GetTimestamps()[1].Valid);

    }

    private bool CheckTimestamp(string dbFilename, Timestamp ts, RaceRun raceRun, EMeasurementPoint measurementPoint)
    {
      bool bRes = true;

      OleDbConnection conn = new OleDbConnection { ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data source= " + dbFilename };
      conn.Open();

      OleDbCommand command = new OleDbCommand("SELECT * FROM RHTimestamps WHERE disziplin = @disziplin AND durchgang = @durchgang AND zeit = @zeit", conn);
      command.Parameters.Add(new OleDbParameter("@disziplin", (int)raceRun.GetRace().RaceType));
      command.Parameters.Add(new OleDbParameter("@durchgang", raceRun.Run));
      command.Parameters.Add(new OleDbParameter("@zeit", Database.FractionForTimeSpan(ts.Time)));

      // Execute command  
      using (OleDbDataReader reader = command.ExecuteReader())
      {
        if (reader.Read())
        {
          bRes &= (byte)raceRun.GetRace().RaceType == reader.GetByte(reader.GetOrdinal("disziplin"));
          bRes &= (byte)raceRun.Run == reader.GetByte(reader.GetOrdinal("durchgang"));

          uint stnr = (uint)(int)reader.GetValue(reader.GetOrdinal("startnummer"));
          bRes &= ts.StartNumber == stnr;

          bool valid = reader.GetBoolean(reader.GetOrdinal("valid"));
          bRes &= ts.Valid == valid;

          TimeSpan? time = null;
          if (!reader.IsDBNull(reader.GetOrdinal("zeit")))
            time = Database.CreateTimeSpan((double)reader.GetValue(reader.GetOrdinal("zeit")));
          bRes &= ts.Time == time;

          bRes &= reader["kanal"].ToString() == (measurementPoint == EMeasurementPoint.Start ? "START":"ZIEL");
        }
        else
        {
          if (ts == null)
            bRes = true;
          else
            bRes = false;
        }
      }

      conn.Close();

      return bRes;
    }

    #endregion


    #region PrintCertificateModel

    /// <summary>
    /// Check reading different race runs
    /// </summary>
    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_Certificate.mdb")]
    public void GetCertificateModel()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_Certificate.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);

      var pcm = db.GetCertificateModel(model.GetRace(0));

      Assert.AreEqual(9, pcm.TextItems.Count);

      Assert.AreEqual("SVM-Cup U12 VII", pcm.TextItems[0].Text);
      Assert.AreEqual("Haettenschweiler, kursiv, 28", pcm.TextItems[0].Font);
      Assert.AreEqual(TextItemAlignment.Center, pcm.TextItems[0].Alignment);
      Assert.AreEqual(1345, pcm.TextItems[0].VPos);
      Assert.AreEqual(1050, pcm.TextItems[0].HPos);

      db.Close();
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_Empty.mdb")]
    public void GetCertificateModel_NoTemplate()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_Empty.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);

      var pcm = db.GetCertificateModel(model.GetRace(0));

      Assert.AreEqual(0, pcm.TextItems.Count);

      db.Close();
    }



    #endregion


    #region Utilities
    bool checkStringAgainstDB(string value, object vDB)
    {
      string sDB = vDB.ToString();
      if (string.IsNullOrEmpty(value) && (vDB == DBNull.Value))
        return true;

      return value == sDB;
    }

    #endregion
  }
}
