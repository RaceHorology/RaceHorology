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
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for AppDataModelTest
  /// </summary>
  [TestClass]
  public class AppDataModelTest
  {
    public AppDataModelTest()
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
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_MultipleRacesNoStartnumber.mdb")]
    public void RaceParticpant1()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants_MultipleRacesNoStartnumber.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);
      var race = model.GetRace(0);
      Assert.AreEqual(Race.ERaceType.DownHill, race.RaceType); // Check that the correct race has been selected

      var rps = race.GetParticipants().ToList();
      rps.Sort(Comparer<RaceParticipant>.Create((x, y) => x.Name.CompareTo(y.Name)));

      // TEST: Remove particpant
      race.GetParticipants().Remove(rps[0]); // "Nachname 1"
      Assert.IsNull(race.GetParticipants().FirstOrDefault(p => p.Name == "Nachname 1"));

      // TEST: Add particpant
      var p7 = model.GetParticipants().First(p => p.Name == "Nachname 7");
      race.AddParticipant(p7);
      Assert.IsNotNull(race.GetParticipants().FirstOrDefault(p => p.Name == "Nachname 7"));

      model = null;
      db.Close();


      // TEST: Cross-Check whether the startnumbers have been stored in DataBase
      RaceHorologyLib.Database db2 = new RaceHorologyLib.Database();
      db2.Connect(dbFilename);
      AppDataModel model2 = new AppDataModel(db2);
      Assert.IsNull(model2.GetRace(0).GetParticipants().FirstOrDefault(p => p.Name == "Nachname 1"));
      Assert.IsNotNull(model2.GetRace(0).GetParticipants().FirstOrDefault(p => p.Name == "Nachname 7"));
      return;
    }


    class TestEventFired
    {
      RaceParticipant firedAdd, firedRemove;
      public TestEventFired(RaceRun rr, Action<RaceRun, RaceRun.OnTrackChangedHandler> register)
      {
        register(rr, OnFiredHandler);
        firedAdd = null;
        firedRemove = null;
      }

      void OnFiredHandler(object o, RaceParticipant participantEnteredTrack, RaceParticipant participantLeftTrack, RunResult currentRunResult)
      {
        firedAdd = participantEnteredTrack;
        firedRemove = participantLeftTrack;
      }

      public RaceParticipant HasFiredAdd(bool reset = true)
      {
        RaceParticipant b = firedAdd;
        if (reset)
          firedAdd = null;
        return b;
      }

      public RaceParticipant HasFiredRemove(bool reset = true)
      {
        RaceParticipant b = firedRemove;
        if (reset)
          firedRemove = null;
        return b;
      }

    }


    [TestMethod]
    public void Race_ParticpantDeleted()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.createRaceParticipants(2);

      Assert.AreEqual(2, tg.Model.GetRace(0).GetParticipants().Count);

      tg.Model.GetParticipants().RemoveAt(0);
      Assert.AreEqual(1, tg.Model.GetRace(0).GetParticipants().Count);
    }



    /// <summary>
    /// Tests RaceRun SetXXXTime() methods which are typically called by
    /// (a) Time computer (e.g. @see LiveTimingMeasurement)
    /// (b) Interactive entering timing
    /// </summary>
    [TestMethod]
    public void RaceRun_Timing_Scenario()
    {
      TestDataGenerator tg = new TestDataGenerator();

      tg.createRaceParticipants(2);

      var race = tg.Model.GetRace(0);
      var run = race.GetRun(0);

      TestEventFired eventTesterOnTrack = new TestEventFired(run, (r, h) => r.OnTrackChanged += h);
      TestEventFired eventTesterInFinish = new TestEventFired(run, (r, h) => r.InFinishChanged += h);

      // Initially, there aren't any results
      Assert.AreEqual(0, run.GetResultList().Count);
      Assert.AreEqual(0, run.GetOnTrackList().Count);
      Assert.IsFalse(run.IsOrWasOnTrack(race.GetParticipant(1)));
      Assert.IsFalse(run.HasResults());
      Assert.IsNull(eventTesterOnTrack.HasFiredAdd());
      Assert.IsNull(eventTesterInFinish.HasFiredAdd());


      // Scenario A: Start / FinishTime
      run.SetStartTime(race.GetParticipant(1), new TimeSpan(8, 0, 0));
      Assert.AreEqual(1, run.GetResultList().Count);
      Assert.AreEqual(1U, run.GetResultList()[0].StartNumber);
      Assert.AreEqual(1, run.GetOnTrackList().Count);
      Assert.AreEqual(1U, run.GetOnTrackList()[0].StartNumber);
      Assert.AreEqual(1U, eventTesterOnTrack.HasFiredAdd().StartNumber);
      Assert.AreEqual(0, run.GetInFinishList().Count);
      Assert.IsNull(eventTesterInFinish.HasFiredAdd());

      run.SetFinishTime(race.GetParticipant(1), new TimeSpan(8, 1, 0));
      Assert.AreEqual(1, run.GetResultList().Count);
      Assert.AreEqual(1U, run.GetResultList()[0].StartNumber);
      Assert.AreEqual(0, run.GetOnTrackList().Count);
      Assert.IsNull(eventTesterOnTrack.HasFiredAdd());
      Assert.AreEqual(1U, eventTesterOnTrack.HasFiredRemove().StartNumber);
      Assert.AreEqual(1, run.GetInFinishList().Count);
      Assert.AreEqual(1U, eventTesterInFinish.HasFiredAdd().StartNumber);
      Assert.IsNull(eventTesterInFinish.HasFiredRemove());

      // Scenario B: Runtime
      run.SetRunTime(race.GetParticipant(2), new TimeSpan(0, 2, 0));
      Assert.AreEqual(2, run.GetResultList().Count);
      Assert.AreEqual(1U, run.GetResultList()[0].StartNumber);
      Assert.AreEqual(2U, run.GetResultList()[1].StartNumber);
      Assert.AreEqual(0, run.GetOnTrackList().Count);
      Assert.AreEqual(2, run.GetInFinishList().Count);

      Assert.IsNull(eventTesterOnTrack.HasFiredAdd());
      Assert.IsNull(eventTesterOnTrack.HasFiredRemove());
      Assert.AreEqual(2U, eventTesterInFinish.HasFiredAdd().StartNumber);
      Assert.IsNull(eventTesterInFinish.HasFiredRemove());
    }


    /// <summary>
    /// Special handling of ResultCode in case a new time arrives
    /// </summary>
    [TestMethod]
    public void RaceRun_Timing_SpecialHandling_ResultCode()
    {
      TestDataGenerator tg = new TestDataGenerator();

      tg.createRaceParticipants(2);

      var race = tg.Model.GetRace(0);
      var run = race.GetRun(0);

      // Initially, there aren't any results
      Assert.AreEqual(0, run.GetResultList().Count);
      Assert.AreEqual(0, run.GetOnTrackList().Count);
      Assert.IsFalse(run.IsOrWasOnTrack(race.GetParticipant(1)));
      Assert.IsFalse(run.HasResults());
      Assert.IsFalse(RaceRunUtil.IsComplete(run));

      // NiZ
      run.SetStartTime(race.GetParticipant(1), new TimeSpan(8, 0, 0));
      Assert.AreEqual(1, run.GetResultList().Count);
      Assert.AreEqual(1U, run.GetResultList()[0].StartNumber);
      Assert.AreEqual(1, run.GetOnTrackList().Count);
      Assert.AreEqual(1U, run.GetOnTrackList()[0].StartNumber);
      Assert.IsFalse(RaceRunUtil.IsComplete(run));

      run.SetResultCode(race.GetParticipant(1), RunResult.EResultCode.NiZ);
      Assert.AreEqual(1, run.GetResultList().Count);
      Assert.AreEqual(1U, run.GetResultList()[0].StartNumber);
      Assert.AreEqual(0, run.GetOnTrackList().Count);
      Assert.IsFalse(RaceRunUtil.IsComplete(run));

      // ... and came later to the finish
      Assert.AreEqual(RunResult.EResultCode.NiZ, run.GetResultList().FirstOrDefault(p => p.StartNumber == 1).ResultCode);
      run.SetFinishTime(race.GetParticipant(1), new TimeSpan(8, 1, 0));
      Assert.AreEqual(RunResult.EResultCode.Normal, run.GetResultList().FirstOrDefault(p => p.StartNumber == 1).ResultCode);
      // Set NiZ again for later test
      run.SetResultCode(race.GetParticipant(1), RunResult.EResultCode.NiZ);
      Assert.AreEqual(RunResult.EResultCode.NiZ, run.GetResultList().FirstOrDefault(p => p.StartNumber == 1).ResultCode);
      Assert.IsFalse(RaceRunUtil.IsComplete(run));


      // NaS
      run.SetResultCode(race.GetParticipant(2), RunResult.EResultCode.NaS);
      Assert.AreEqual(2, run.GetResultList().Count);
      Assert.AreEqual(1U, run.GetResultList()[0].StartNumber);
      Assert.AreEqual(2U, run.GetResultList()[1].StartNumber);
      Assert.AreEqual(0, run.GetOnTrackList().Count);
      Assert.IsTrue(RaceRunUtil.IsComplete(run));


      // Special handling: Participant is allowed to restart and gets a new time => ResultCode shall be deleted
      Assert.AreEqual(RunResult.EResultCode.NiZ, run.GetResultList().FirstOrDefault(p => p.StartNumber == 1).ResultCode);
      run.SetStartTime(race.GetParticipant(1), new TimeSpan(9, 0, 0));
      Assert.IsNotNull(run.GetResultList().FirstOrDefault(p => p.StartNumber == 1));
      Assert.AreEqual(RunResult.EResultCode.Normal, run.GetResultList().FirstOrDefault(p => p.StartNumber == 1).ResultCode);
      Assert.IsNotNull(run.GetOnTrackList().FirstOrDefault(p => p.StartNumber == 1));
      Assert.IsFalse(RaceRunUtil.IsComplete(run));

      run.SetFinishTime(race.GetParticipant(1), new TimeSpan(9, 1, 0));
      Assert.IsNotNull(run.GetResultList().FirstOrDefault(p => p.StartNumber == 1));
      Assert.IsNull(run.GetOnTrackList().FirstOrDefault(p => p.StartNumber == 1));
      Assert.IsTrue(RaceRunUtil.IsComplete(run));

      Assert.AreEqual(RunResult.EResultCode.NaS, run.GetResultList().FirstOrDefault(p => p.StartNumber == 2).ResultCode);
      run.SetRunTime(race.GetParticipant(2), new TimeSpan(0, 1, 0));
      Assert.AreEqual(RunResult.EResultCode.Normal, run.GetResultList().FirstOrDefault(p => p.StartNumber == 2).ResultCode);
      Assert.IsTrue(RaceRunUtil.IsComplete(run));
    }


    /// <summary>
    /// Tests RaceRun with some sample scenarios.
    /// Main focus are the lists:
    /// - GetResultList()
    /// - GetOnTrackList()
    /// - RaceRunUtil.IsComplete()
    /// </summary>
    [TestMethod]
    public void RaceRun_RunResult()
    {
      TestDataGenerator tg = new TestDataGenerator();

      tg.createRaceParticipants(5);

      var race = tg.Model.GetRace(0);
      var run = race.GetRun(0);

      // Initially, there aren't any results
      Assert.AreEqual(0, run.GetResultList().Count);
      Assert.IsFalse(run.IsOrWasOnTrack(race.GetParticipant(1)));
      Assert.IsFalse(run.HasResults());
      Assert.IsFalse(RaceRunUtil.IsComplete(run));

      run.SetStartTime(race.GetParticipant(1), new TimeSpan(8, 0, 0));
      Assert.AreEqual(1, run.GetResultList().Count);
      Assert.AreEqual(1U, run.GetOnTrackList()[0].StartNumber);
      
      Assert.IsTrue(run.HasResults());
      Assert.IsFalse(RaceRunUtil.IsComplete(run));

      run.SetFinishTime(race.GetParticipant(1), new TimeSpan(8, 1, 1));
      Assert.AreEqual(1, run.GetResultList().Count);
      Assert.AreEqual(0, run.GetOnTrackList().Count);
      Assert.IsFalse(RaceRunUtil.IsComplete(run));

      run.SetFinishTime(race.GetParticipant(1), null);
      Assert.AreEqual(1, run.GetResultList().Count);
      Assert.AreEqual(1U, run.GetOnTrackList()[0].StartNumber);
      Assert.IsFalse(RaceRunUtil.IsComplete(run));

      run.SetFinishTime(race.GetParticipant(1), new TimeSpan(8, 1, 1));
      Assert.AreEqual(1, run.GetResultList().Count);
      Assert.AreEqual(new TimeSpan(0, 1, 1), run.GetResultList()[0].Runtime);
      Assert.AreEqual(0, run.GetOnTrackList().Count);
      Assert.IsTrue(run.IsOrWasOnTrack(race.GetParticipant(1)));
      Assert.IsFalse(RaceRunUtil.IsComplete(run));

      Assert.IsFalse(run.IsOrWasOnTrack(race.GetParticipant(2)));
      run.SetStartFinishTime(race.GetParticipant(2), new TimeSpan(8, 2, 0), new TimeSpan(8, 3, 2));
      Assert.AreEqual(2, run.GetResultList().Count);
      Assert.AreEqual(new TimeSpan(0, 1, 2), run.GetResultList()[1].Runtime);
      Assert.AreEqual(0, run.GetOnTrackList().Count);
      Assert.IsTrue(run.IsOrWasOnTrack(race.GetParticipant(2)));
      Assert.IsFalse(RaceRunUtil.IsComplete(run));


      run.SetRunTime(race.GetParticipant(3), new TimeSpan(0, 1, 3));
      Assert.AreEqual(3, run.GetResultList().Count);
      Assert.AreEqual(new TimeSpan(0, 1, 3), run.GetResultList()[2].Runtime);
      Assert.AreEqual(0, run.GetOnTrackList().Count);
      Assert.IsFalse(RaceRunUtil.IsComplete(run));
      Assert.IsFalse(RaceRunUtil.IsComplete(run));


      run.SetResultCode(race.GetParticipant(4), RunResult.EResultCode.NaS);
      Assert.AreEqual(4, run.GetResultList().Count);
      Assert.AreEqual(null, run.GetResultList()[3].Runtime);
      Assert.AreEqual(RunResult.EResultCode.NaS, run.GetResultList()[3].ResultCode);
      Assert.AreEqual(0, run.GetOnTrackList().Count);
      Assert.IsFalse(RaceRunUtil.IsComplete(run));

      run.SetResultCode(race.GetParticipant(5), RunResult.EResultCode.DIS, "Tor 5");
      Assert.AreEqual(5, run.GetResultList().Count);
      Assert.AreEqual(null, run.GetResultList()[4].Runtime);
      Assert.AreEqual(RunResult.EResultCode.DIS, run.GetResultList()[4].ResultCode);
      Assert.AreEqual("Tor 5", run.GetResultList()[4].DisqualText);
      Assert.AreEqual(0, run.GetOnTrackList().Count);
      Assert.IsTrue(RaceRunUtil.IsComplete(run));


      var tmp1 = run.DeleteRunResult(race.GetParticipant(1));
      Assert.AreEqual(1U, tmp1.StartNumber);
      Assert.AreEqual(4, run.GetResultList().Count);
      Assert.AreNotEqual(1U, run.GetResultList()[0].StartNumber);
      Assert.AreEqual(2U, run.GetResultList()[0].StartNumber);
      Assert.IsFalse(RaceRunUtil.IsComplete(run));


      Assert.IsTrue(run.HasResults());
      run.DeleteRunResults();
      Assert.IsFalse(run.HasResults());
      Assert.AreEqual(0, run.GetResultList().Count);
      Assert.IsFalse(RaceRunUtil.IsComplete(run));
    }


    [TestMethod]
    public void Race_IsConsistent()
    {
      TestDataGenerator tg = new TestDataGenerator();
      Race race = tg.Model.GetRace(0);

      // Empty race
      Assert.IsTrue(RaceUtil.IsConsistent(race));

      // 1 Participant, no startnumber
      var rp1 = tg.createRaceParticipant();
      Assert.IsTrue(RaceUtil.IsConsistent(race));
      rp1.StartNumber = 0;
      Assert.IsFalse(RaceUtil.IsConsistent(race));

      // 2 Participants, no startnumber
      var rp2 = tg.createRaceParticipant();
      Assert.IsFalse(RaceUtil.IsConsistent(race));
      rp2.StartNumber = 0;
      Assert.IsFalse(RaceUtil.IsConsistent(race));

      // 2 Participants, same startnumber
      rp1.StartNumber = 1U;
      rp2.StartNumber = 1U;
      Assert.IsFalse(RaceUtil.IsConsistent(race));

      // 2 Participants, different startnumber
      rp2.StartNumber = 2U;
      Assert.IsTrue(RaceUtil.IsConsistent(race));
    }


    [TestMethod]
    public void Race_ManageRun()
    {
      TestDataGenerator tg = new TestDataGenerator();

      var dm = new AppDataModel(new DummyDataBase(".", false)); // Create empty model

      Assert.AreEqual(0, dm.GetRaces().Count);

      dm.AddRace(new Race.RaceProperties { RaceType = Race.ERaceType.GiantSlalom, Runs = 1 });

      Assert.AreEqual(1, dm.GetRaces().Count);

      Race race = dm.GetRace(0);
      Assert.AreEqual(1, race.GetMaxRun());

      race.AddRaceRun();
      Assert.AreEqual(2, race.GetMaxRun());

      race.DeleteRaceRun();
      Assert.AreEqual(1, race.GetMaxRun());

      race.UpdateNumberOfRuns(3);
      Assert.AreEqual(3, race.GetMaxRun());

      race.UpdateNumberOfRuns(2);
      Assert.AreEqual(2, race.GetMaxRun());
    }


    /// <summary>
    /// Removing and adding a run shall preserve any run data (RunResult) which has been stored in the DB
    /// </summary>
    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_MultipleRaces.mdb")]
    public void Race_ManageRun_DataHandling()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants_MultipleRaces.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel dm= new AppDataModel(db);
      var race = dm.GetRaces().FirstOrDefault(r => r.RaceType == Race.ERaceType.GiantSlalom);

      // Check correct initial assumptions
      Assert.AreEqual(4, dm.GetRaces().Count); 
      Assert.AreEqual(2, race.GetMaxRun());

      RaceParticipant p1 = race.GetParticipant(1);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 22, 850), race.GetRun(0).GetRunResult(p1).GetRunTime());
      Assert.AreEqual(new TimeSpan(0, 0, 0, 21, 950), race.GetRun(1).GetRunResult(p1).GetRunTime());

      race.DeleteRaceRun();
      Assert.AreEqual(1, race.GetMaxRun());
      Assert.AreEqual(new TimeSpan(0, 0, 0, 22, 850), race.GetRun(0).GetRunResult(p1).GetRunTime());


      race.AddRaceRun();
      Assert.AreEqual(2, race.GetMaxRun());
      Assert.AreEqual(new TimeSpan(0, 0, 0, 22, 850), race.GetRun(0).GetRunResult(p1).GetRunTime());
      Assert.AreEqual(new TimeSpan(0, 0, 0, 21, 950), race.GetRun(1).GetRunResult(p1).GetRunTime());


      // Perform Modification and Check again
      race.GetRun(1).SetRunTime(p1, new TimeSpan(0, 0, 0, 10, 110));
      Assert.AreEqual(new TimeSpan(0, 0, 0, 10, 110), race.GetRun(1).GetRunResult(p1).GetRunTime());

      race.DeleteRaceRun();
      Assert.AreEqual(1, race.GetMaxRun());
      Assert.AreEqual(new TimeSpan(0, 0, 0, 22, 850), race.GetRun(0).GetRunResult(p1).GetRunTime());

      race.AddRaceRun();
      Assert.AreEqual(2, race.GetMaxRun());
      Assert.AreEqual(new TimeSpan(0, 0, 0, 22, 850), race.GetRun(0).GetRunResult(p1).GetRunTime());
      Assert.AreEqual(new TimeSpan(0, 0, 0, 10, 110), race.GetRun(1).GetRunResult(p1).GetRunTime());

      //race.UpdateNumberOfRuns(3);
      //Assert.AreEqual(3, race.GetMaxRun());

      //race.UpdateNumberOfRuns(2);
      //Assert.AreEqual(2, race.GetMaxRun());
    }



  }
}
