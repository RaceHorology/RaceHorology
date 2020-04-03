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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for StartNumberTest
  /// </summary>
  [TestClass]
  public class StartNumberTest
  {
    public StartNumberTest()
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
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_MultipleRacesNoStartnumber.mdb")]
    public void ChangeStartNumber()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants_MultipleRacesNoStartnumber.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);
      var rps = model.GetRace(0).GetParticipants().ToList();

      // Assign startnumber from scratch
      rps.Sort(Comparer<RaceParticipant>.Create((x, y) => x.Name.CompareTo(y.Name)));
      for(int i=0; i<rps.Count; i++)
        rps[i].StartNumber = (uint)i+1;


      // TEST 1: Test whether startnumber is remembered
      for (int i = 0; i < rps.Count; i++)
        Assert.AreEqual((uint)i+1, rps[i].StartNumber);


      model = null;
      db.Close();


      // TEST 2: Cross-Check whether the startnumbers have been stored in DataBase
      RaceHorologyLib.Database db2 = new RaceHorologyLib.Database();
      db2.Connect(dbFilename);
      AppDataModel model2 = new AppDataModel(db2);
      var rps2 = model2.GetRace(0).GetParticipants().ToList();
      rps2.Sort(Comparer<RaceParticipant>.Create((x, y) => x.Name.CompareTo(y.Name)));
      for (int i = 0; i < rps.Count; i++)
        Assert.AreEqual((uint)i + 1, rps2[i].StartNumber);
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_MultipleRacesNoStartnumber.mdb")]
    public void StartNumberAssignment_Manual_Test()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants_MultipleRacesNoStartnumber.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);
      var rps = model.GetRace(0).GetParticipants().ToList();


      StartNumberAssignment sna = new StartNumberAssignment();

      uint sn = sna.AssignNextFree(rps[0]);
      Assert.AreEqual(1U, sn);
      Assert.AreEqual(rps[0], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 1).Participant);

      sn = sna.AssignNextFree(rps[1]);
      Assert.AreEqual(2U, sn);
      Assert.AreEqual(rps[1], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 2).Participant);

      sna.Assign(4U, rps[2]);
      Assert.AreEqual(rps[2], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 4).Participant);

      sn = sna.AssignNextFree(rps[3]);
      Assert.AreEqual(5U, sn);
      Assert.AreEqual(rps[3], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 5).Participant);

      sna.InsertAndShift(2U);
      Assert.AreEqual(rps[0], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 1).Participant);
      Assert.AreEqual(null,   sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 2).Participant);
      Assert.AreEqual(rps[1], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 3).Participant);
      Assert.AreEqual(null,   sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 4).Participant);
      Assert.AreEqual(rps[2], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 5).Participant);
      Assert.AreEqual(rps[3], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 6).Participant);

      sna.Assign(1U, null);
      Assert.AreEqual(null,   sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 1).Participant);
      Assert.AreEqual(null,   sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 2).Participant);
      Assert.AreEqual(rps[1], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 3).Participant);
      Assert.AreEqual(null,   sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 4).Participant);
      Assert.AreEqual(rps[2], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 5).Participant);
      Assert.AreEqual(rps[3], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 6).Participant);

      sna.Delete(1U);
      Assert.AreEqual(null,   sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 1).Participant);
      Assert.AreEqual(rps[1], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 2).Participant);
      Assert.AreEqual(null,   sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 3).Participant);
      Assert.AreEqual(rps[2], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 4).Participant);
      Assert.AreEqual(rps[3], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 5).Participant);
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_MultipleRaces.mdb")]
    public void StartNumberAssignment_LoadFromRace_Test()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants_MultipleRaces.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);
      var race = model.GetRaces().FirstOrDefault(r => r.RaceType == Race.ERaceType.GiantSlalom);

      StartNumberAssignment sna = new StartNumberAssignment();
      sna.LoadFromRace(race);

      var rps = race.GetParticipants().ToList();
      foreach(var snap in sna.ParticipantList)
      {
        if (snap.StartNumber != 0)
        {
          var rp = rps.FirstOrDefault(x => x.StartNumber == snap.StartNumber);
          Assert.AreEqual(snap.Participant, rp);
        }
      }
    }
  }
}
