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
    public void ImportResultsClass()
    {
      ImportResults ir1 = new ImportResults();

      Assert.AreEqual(0, ir1.SuccessCount);
      Assert.AreEqual(0, ir1.ErrorCount);

      ir1.AddError();
      Assert.AreEqual(0, ir1.SuccessCount);
      Assert.AreEqual(1, ir1.ErrorCount);

      ir1.AddSuccess();
      Assert.AreEqual(1, ir1.SuccessCount);
      Assert.AreEqual(1, ir1.ErrorCount);
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.csv")]
    public void ImportParticpants()
    {
      TestDataGenerator tg = new TestDataGenerator();

      var ir = new ImportReader(@"Teilnehmer_V1_202001301844.csv");

      ParticipantMapping mapping = new ParticipantMapping(ir.Columns);

      List<Participant> participants = new List<Participant>();
      ParticipantImport im = new ParticipantImport(ir.Data, participants, mapping, tg.createCategories());
      var impRes = im.DoImport();

      Assert.AreEqual(153, impRes.SuccessCount);
      Assert.AreEqual(0, impRes.ErrorCount);

      for (int i=0; i<153; i++)
      {
        Assert.AreEqual(string.Format("Name {0}", i + 1), participants[i].Name);
        Assert.IsNull(participants[i].Class);
      }

      Assert.AreEqual('W', participants[0].Sex.Name);
      Assert.AreEqual('W', participants[1].Sex.Name);
      Assert.AreEqual('M', participants[2].Sex.Name);
      Assert.AreEqual('M', participants[3].Sex.Name);

      // Check synonyms
      Assert.AreEqual('M', participants[4].Sex.Name);//h
      Assert.AreEqual('M', participants[6].Sex.Name);//H
      Assert.AreEqual('M', participants[7].Sex.Name);//x
      Assert.AreEqual('M', participants[10].Sex.Name);//X
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.csv")]
    public void ImportParticpantsForRace()
    {
      TestDataGenerator tg = new TestDataGenerator();

      var ir = new ImportReader(@"Teilnehmer_V1_202001301844.csv");

      RaceMapping mapping = new RaceMapping(ir.Columns);

      RaceImport im = new RaceImport(ir.Data, tg.Model.GetRace(0), mapping);
      var impRes = im.DoImport();

      Assert.AreEqual(153, impRes.SuccessCount);
      Assert.AreEqual(0, impRes.ErrorCount);

      for (int i = 0; i < 153; i++)
      {
        Participant p = tg.Model.GetParticipants()[i];
        RaceParticipant rp = tg.Model.GetRace(0).GetParticipants()[i];

        Assert.AreEqual(string.Format("Name {0}", i + 1), p.Name);
        Assert.AreEqual(string.Format("Name {0}", i + 1), rp.Name);
        Assert.IsTrue(rp.Participant == p);
      }
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.csv")]
    [DeploymentItem(@"TestDataBases\TestDB_EmptyManyClasses.mdb")]
    public void ImportParticpantsWithClassAssignment()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_EmptyManyClasses.mdb");

      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);
      AppDataModel dm = new AppDataModel(db);

      var ir = new ImportReader(@"Teilnehmer_V1_202001301844.csv");

      RaceMapping mapping = new RaceMapping(ir.Columns);

      ClassAssignment cla = new ClassAssignment(dm.GetParticipantClasses());
      RaceImport im = new RaceImport(ir.Data, dm.GetRace(0), mapping, cla);
      var impRes = im.DoImport();

      Assert.AreEqual(153, impRes.SuccessCount);
      Assert.AreEqual(0, impRes.ErrorCount);

      for (int i = 0; i < 153; i++)
      {
        Participant p = dm.GetParticipants()[i];
        RaceParticipant rp = dm.GetRace(0).GetParticipants()[i];

        Assert.AreEqual(string.Format("Name {0}", i + 1), p.Name);
        Assert.AreEqual(string.Format("Name {0}", i + 1), rp.Name);
        Assert.IsTrue(rp.Participant == p);
        Assert.AreSame(cla.DetermineClass(p), p.Class);
        Assert.IsNotNull(p.Class);
      }
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\Import\1557MRBR.zip")]
    public void ImportParticpantsForRaceViaZip()
    {
      string[] columnShall =
      {
        "id",
        "nachname",
        "vorname",
        "geschlecht",
        "verein",
        "verbandskürzel",
        "jg",
        "punkte",
        "adr_str",
        "adr_plz",
        "adr_ort"
      };

      void checkColumns(List<string> columns)
      {
        for (int i = 0; i < columnShall.Length; i++)
          Assert.AreEqual(columnShall[i], columns[i]);
      }

      TestDataGenerator tg = new TestDataGenerator();

      var ir = new ImportZipReader(@"1557MRBR.zip");

      checkColumns(ir.Columns);

      RaceMapping mapping = new RaceMapping(ir.Columns);

      RaceImport im = new RaceImport(ir.Data, tg.Model.GetRace(0), mapping);
      var impRes = im.DoImport();

      Assert.AreEqual(3, impRes.SuccessCount);
      Assert.AreEqual(0, impRes.ErrorCount);

      Assert.AreEqual(3, ir.Data.Tables[0].Rows.Count);

      for (int i = 0; i < 2; i++)
      {
        Participant p = tg.Model.GetParticipants()[i];
        RaceParticipant rp = tg.Model.GetRace(0).GetParticipants()[i];

        Assert.AreEqual(string.Format("NACHNAME{0}", i + 1), p.Name);
        Assert.AreEqual(string.Format("Vorname{0}", i + 1), rp.Firstname);
        Assert.IsTrue(rp.Participant == p);
      }
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_Import_Race.xlsx")]
    public void ImportParticpantsForRaceWithPoints()
    {
      TestDataGenerator tg = new TestDataGenerator();

      var ir = new ImportReader(@"Teilnehmer_Import_Race.xlsx");

      RaceMapping mapping = new RaceMapping(ir.Columns);

      RaceImport im = new RaceImport(ir.Data, tg.Model.GetRace(0), mapping);
      var impRes = im.DoImport();

      Assert.AreEqual(20, impRes.SuccessCount);
      Assert.AreEqual(0, impRes.ErrorCount);

      for (int i = 0; i < 20; i++)
      {
        Participant p = tg.Model.GetParticipants()[i];
        RaceParticipant rp = tg.Model.GetRace(0).GetParticipants()[i];

        Assert.AreEqual(string.Format("Name {0}", i + 1), p.Name);
      
        Assert.AreEqual(string.Format("Name {0}", i + 1), rp.Name);

        if (i==0)
          Assert.AreEqual((double)(1), rp.Points);
        else if (i == 1)
          Assert.AreEqual((double)(2), rp.Points);
        else if (i == 2)
          Assert.AreEqual((double)(3.3), rp.Points);
        else if (i == 3)
          Assert.AreEqual((double)(-1), rp.Points);
        else
          Assert.AreEqual((double)(i + 1), rp.Points);

        if (i == 3)
          Assert.AreEqual((uint)(0), rp.StartNumber);
        else
          Assert.AreEqual((uint)(20 - i), rp.StartNumber);

        Assert.IsTrue(rp.Participant == p);
      }
    }




    [TestMethod]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.csv")]
    public void ImportPointsForParticpant()
    {
      return;

      TestDataGenerator tg = new TestDataGenerator();

      var ir = new ImportReader(@"Teilnehmer_V1_202001301844.csv");

      ParticipantMapping mapping = new ParticipantMapping(ir.Columns);

      List<Participant> participants = new List<Participant>();
      ParticipantImport im = new ParticipantImport(ir.Data, participants, mapping, tg.createCategories());
      var impRes = im.DoImport();

      Assert.AreEqual(153, impRes.SuccessCount);
      Assert.AreEqual(0, impRes.ErrorCount);

      for (int i = 0; i < 153; i++)
      {
        Assert.AreEqual(string.Format("Name {0}", i + 1), participants[i].Name);
      }

    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\Import\DSV\DSVA2008.txt")]
    public void ReadDSVPoints()
    {

    }


  }
}
