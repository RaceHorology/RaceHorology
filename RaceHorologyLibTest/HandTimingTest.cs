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
  /// Contains all tests related to HandTiming
  /// </summary>
  [TestClass]
  public class HandTimingTest
  {
    public HandTimingTest()
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


    /// <summary>
    /// Tests the FromFileParser
    /// Simulated input is passed to parser and returned TimeSpan is verified
    /// </summary>
    [TestMethod]
    [DeploymentItem(@"TestDataBases\HandTime\--Handzeit-Start.txt")]
    public void FromFileParser()
    {
      FromFileParser parser = new FromFileParser();

      // Period "."
      Assert.AreEqual(new TimeSpan(0, 8, 48, 0, 500), parser.ParseTime(@"08:48:00.5"));
      Assert.AreEqual(new TimeSpan(0, 8, 48, 0, 570), parser.ParseTime(@"08:48:00.57"));
      Assert.AreEqual(new TimeSpan(0, 8, 48, 0, 578), parser.ParseTime(@"08:48:00.578"));
      Assert.AreEqual(new TimeSpan(0, 8, 48, 0, 578).AddMicroseconds(900), parser.ParseTime(@"08:48:00.5789"));
      Assert.AreEqual(new TimeSpan(0, 8, 48, 0, 578).AddMicroseconds(910), parser.ParseTime(@"08:48:00.57891"));
      
      // Comma ","
      Assert.AreEqual(new TimeSpan(0, 8, 48, 0, 500), parser.ParseTime(@"08:48:00,5"));
      Assert.AreEqual(new TimeSpan(0, 8, 48, 0, 570), parser.ParseTime(@"08:48:00,57"));
      Assert.AreEqual(new TimeSpan(0, 8, 48, 0, 578), parser.ParseTime(@"08:48:00,578"));
      Assert.AreEqual(new TimeSpan(0, 8, 48, 0, 578).AddMicroseconds(900), parser.ParseTime(@"08:48:00,5789"));
      Assert.AreEqual(new TimeSpan(0, 8, 48, 0, 578).AddMicroseconds(910), parser.ParseTime(@"08:48:00,57891"));
    }


    /// <summary>
    /// Test the reading from file.
    /// - Reads all lines, only first parsed lines are verfied.
    /// - Internally, the FromFileParser is used.
    /// </summary>
    [TestMethod]
    [DeploymentItem(@"TestDataBases\HandTime\--Handzeit-Start.txt")]
    public void ReadFromFile()
    {

      FromFileHandTiming ht = new FromFileHandTiming(@"--Handzeit-Start.txt");
      ht.Connect();

      TimeSpan[] shallTime =
      {
        new TimeSpan(0, 8, 48, 0, 570),
        new TimeSpan(0, 9, 32, 56, 300)
      };

      int i = 0;
      foreach (var t in ht.TimingData())
      {
        if (i < shallTime.Length)
          Assert.AreEqual(shallTime[i], t.Time);

        TestContext.WriteLine(t.Time.ToString());

        i++;
      }
    }

    /// <summary>
    /// Tests creation of corresponding IHandTiming object for specific handtiming sources (e.g. File, ALGE, TagHeuer)
    /// </summary>
    [TestMethod]
    public void CreateHandTiming()
    {
      Assert.AreEqual(typeof(FromFileHandTiming), HandTiming.CreateHandTiming("File", "abc").GetType());
      Assert.AreEqual(typeof(TagHeuer), HandTiming.CreateHandTiming("TagHeuerPPro", "abc").GetType());
      Assert.AreEqual(typeof(ALGETimy), HandTiming.CreateHandTiming("ALGETimy", "abc").GetType());
    }


    /// <summary>
    /// Tests the automatic correlation of hand timestamps with start/finish time stamps
    /// </summary>
    [TestMethod]
    public void HandTimingsVM_Correlation()
    {
      TestDataGenerator tg = new TestDataGenerator();

      HandTimingVM htVM = new HandTimingVM(HandTimingVMEntry.ETimeModus.EStartTime);

      List<RunResult> rr1 = new List<RunResult>
      {
        tg.createRunResult(tg.createRaceParticipant(), new TimeSpan(0, 8, 0, 2), new TimeSpan(0, 8, 1, 2)),
        tg.createRunResult(tg.createRaceParticipant(), new TimeSpan(0, 8, 2, 2), new TimeSpan(0, 8, 3, 2)),
        tg.createRunResult(tg.createRaceParticipant(), new TimeSpan(0, 8, 4, 2), null)
      };

      List<TimingData> hts1 = new List<TimingData>
      {
        new TimingData{Time = new TimeSpan(0, 8, 0, 2, 301)},
        new TimingData{Time = new TimeSpan(0, 8, 2, 1, 299)},
        new TimingData{Time = new TimeSpan(0, 8, 2, 1, 300)},
        new TimingData{Time = new TimeSpan(0, 8, 4, 2, 1)}
      };

      htVM.AddRunResults(rr1);
      htVM.AddHandTimings(hts1);

      Assert.AreEqual(new TimeSpan(0, 8, 0, 2), htVM.Items[0].ATime);
      Assert.AreEqual(new TimeSpan(0, 8, 0, 2, 301), htVM.Items[0].HandTime);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 0, 300), htVM.Items[0].HandTimeDiff);

      Assert.IsNull(htVM.Items[1].ATime);
      Assert.AreEqual(new TimeSpan(0, 8, 2, 1, 300), htVM.Items[1].HandTime);
      Assert.IsNull(htVM.Items[1].HandTimeDiff);

      Assert.AreEqual(new TimeSpan(0, 8, 2, 2), htVM.Items[2].ATime);
      Assert.AreEqual(new TimeSpan(0, 8, 2, 1, 299), htVM.Items[2].HandTime);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -710), htVM.Items[2].HandTimeDiff);

      Assert.AreEqual(new TimeSpan(0, 8, 4, 2), htVM.Items[3].ATime);
      Assert.AreEqual(new TimeSpan(0, 8, 4, 2, 1), htVM.Items[3].HandTime);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 0, 0), htVM.Items[3].HandTimeDiff);
    }


    /// <summary>
    /// Tests whether dissolving hand timestamps from start/finish time stamps works.
    /// </summary>
    [TestMethod]
    public void HandTimingsVM_Dissolve()
    {
      TestDataGenerator tg = new TestDataGenerator();

      HandTimingVM htVM = new HandTimingVM(HandTimingVMEntry.ETimeModus.EStartTime);

      List<RunResult> rr1 = new List<RunResult>
      {
        tg.createRunResult(tg.createRaceParticipant(), new TimeSpan(0, 8, 0, 2), new TimeSpan(0, 8, 1, 2))
      };

      List<TimingData> hts1 = new List<TimingData>
      {
        new TimingData{Time = new TimeSpan(0, 8, 0, 2, 301)}
      };

      htVM.AddRunResults(rr1);
      htVM.AddHandTimings(hts1);

      // Check Pre-Condition
      Assert.AreEqual(new TimeSpan(0, 8, 0, 2), htVM.Items[0].ATime);
      Assert.AreEqual(new TimeSpan(0, 8, 0, 2, 301), htVM.Items[0].HandTime);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 0, 300), htVM.Items[0].HandTimeDiff);

      // Operation
      htVM.Dissolve(htVM.Items[0]);

      // Check Post-Condition
      Assert.AreEqual(new TimeSpan(0, 8, 0, 2), htVM.Items[0].ATime);
      Assert.IsNull(htVM.Items[0].HandTime);
      Assert.IsNull(htVM.Items[0].HandTimeDiff);

      Assert.IsNull(htVM.Items[1].ATime);
      Assert.AreEqual(new TimeSpan(0, 8, 0, 2, 301), htVM.Items[1].HandTime);
      Assert.IsNull(htVM.Items[1].HandTimeDiff);
    }


    /// <summary>
    /// Tests manual assignment of startnumber to hand time stamps
    /// </summary>
    [TestMethod]
    public void HandTimingsVM_AssignStartNumber()
    {
      TestDataGenerator tg = new TestDataGenerator();

      HandTimingVM htVM = new HandTimingVM(HandTimingVMEntry.ETimeModus.EStartTime);

      List<RunResult> rr1 = new List<RunResult>
      {
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8, 1, 2))
      };

      List<TimingData> hts1 = new List<TimingData>
      {
        new TimingData{Time = new TimeSpan(0, 8, 0, 2, 301)},
        new TimingData{Time = new TimeSpan(0, 8, 5, 2, 300)}
      };

      htVM.AddRunResults(rr1);
      htVM.AddHandTimings(hts1);

      // ***** Case 1: Merge entries ****

      // Check Pre-Condition
      Assert.IsNull(htVM.Items[0].ATime);
      Assert.AreEqual(new TimeSpan(0, 8, 0, 2, 301), htVM.Items[0].HandTime);
      Assert.IsNull(htVM.Items[0].HandTimeDiff);

      Assert.AreEqual(1U, htVM.Items[2].StartNumber);
      Assert.IsNull(htVM.Items[2].ATime);
      Assert.IsNull(htVM.Items[2].HandTime);
      Assert.IsNull(htVM.Items[2].HandTimeDiff);

      Assert.AreEqual(3, htVM.Items.Count);

      // Operation
      htVM.AssignStartNumber(htVM.Items[0], 1);

      // Check Post-Condition
      Assert.AreEqual(1U, htVM.Items[0].StartNumber);
      Assert.IsNull(htVM.Items[0].ATime);
      Assert.AreEqual(new TimeSpan(0, 8, 0, 2, 301), htVM.Items[0].HandTime);
      Assert.IsNull(htVM.Items[0].HandTimeDiff);
      Assert.IsNull(htVM.Items[0].StartTime);
      Assert.AreEqual(new TimeSpan(0, 8, 1, 2), htVM.Items[0].FinishTime);

      Assert.AreEqual(2, htVM.Items.Count);



      // ***** Case 2: Adjust entry ****

      // Check Pre-Condition
      Assert.IsNull(htVM.Items[1].StartNumber);
      Assert.IsNull(htVM.Items[1].ATime);
      Assert.AreEqual(new TimeSpan(0, 8, 5, 2, 300), htVM.Items[1].HandTime);
      Assert.IsNull(htVM.Items[1].HandTimeDiff);

      // Operation
      htVM.AssignStartNumber(htVM.Items[1], 2);

      // Check Post-Condition
      Assert.AreEqual(2U, htVM.Items[1].StartNumber);
      Assert.IsNull(htVM.Items[1].ATime);
      Assert.AreEqual(new TimeSpan(0, 8, 5, 2, 300), htVM.Items[1].HandTime);
      Assert.IsNull(htVM.Items[1].HandTimeDiff);
      Assert.IsNull(htVM.Items[1].StartTime);
      Assert.IsNull(htVM.Items[1].FinishTime);

      Assert.AreEqual(2, htVM.Items.Count);
    }


    /// <summary>
    /// Tests hand timing calculation
    /// - This Test Case: standard case, use the previous 10 hand timings
    /// </summary>
    [TestMethod]
    public void HandTimingCalc_Test1()
    {
      TestDataGenerator tg = new TestDataGenerator();
      HandTimingVM htVM = new HandTimingVM(HandTimingVMEntry.ETimeModus.EFinishTime);

      List<RunResult> rr1 = new List<RunResult>
      {
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  0, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  1, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  2, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  3, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  4, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  5, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  6, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  7, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  8, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  9, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, null)
      };

      List<TimingData> hts1 = new List<TimingData>
      {
        new TimingData{Time = new TimeSpan(0, 8,  0, 0, 100)},
        new TimingData{Time = new TimeSpan(0, 8,  1, 0, 200)},
        new TimingData{Time = new TimeSpan(0, 8,  2, 0, 300)},
        new TimingData{Time = new TimeSpan(0, 8,  3, 0, 100)},
        new TimingData{Time = new TimeSpan(0, 8,  4, 0, 200)},
        new TimingData{Time = new TimeSpan(0, 8,  5, 0, 300)},
        new TimingData{Time = new TimeSpan(0, 8,  6, 0, 100)},
        new TimingData{Time = new TimeSpan(0, 8,  7, 0, 200)},
        new TimingData{Time = new TimeSpan(0, 8,  8, 0, 300)},
        new TimingData{Time = new TimeSpan(0, 8,  9, 0, 200)},
        new TimingData{Time = new TimeSpan(0, 8, 10, 0, 300)}
      };

      htVM.AddRunResults(rr1);
      htVM.AddHandTimings(hts1);

      htVM.AssignStartNumber(htVM.Items[10], 11);

      HandTimingCalc hc = new HandTimingCalc(htVM.Items[10], htVM.Items);
      Assert.AreEqual(new TimeSpan(0, 8, 10, 0, 100), hc.CalculatedTime);
    }

    /// <summary>
    /// Tests hand timing calculation
    /// - This Test Case: special case, there aren't 10 previous hand timings available, use the upcoming hand timings as well
    /// </summary>
    [TestMethod]
    public void HandTimingCalc_Test2()
    {
      TestDataGenerator tg = new TestDataGenerator();
      HandTimingVM htVM = new HandTimingVM(HandTimingVMEntry.ETimeModus.EFinishTime);

      List<RunResult> rr1 = new List<RunResult>
      {
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  0, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  1, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  2, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  3, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  4, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, null),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  6, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  7, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  8, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  9, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8, 10, 0))
      };

      List<TimingData> hts1 = new List<TimingData>
      {
        new TimingData{Time = new TimeSpan(0, 8,  0, 0, 100)},
        new TimingData{Time = new TimeSpan(0, 8,  1, 0, 200)},
        new TimingData{Time = new TimeSpan(0, 8,  2, 0, 300)},
        new TimingData{Time = new TimeSpan(0, 8,  3, 0, 100)},
        new TimingData{Time = new TimeSpan(0, 8,  4, 0, 200)},
        new TimingData{Time = new TimeSpan(0, 8,  5, 0, 300)},
        new TimingData{Time = new TimeSpan(0, 8,  6, 0, 100)},
        new TimingData{Time = new TimeSpan(0, 8,  7, 0, 200)},
        new TimingData{Time = new TimeSpan(0, 8,  8, 0, 300)},
        new TimingData{Time = new TimeSpan(0, 8,  9, 0, 100)},
        new TimingData{Time = new TimeSpan(0, 8, 10, 0, 200)}
      };

      htVM.AddRunResults(rr1);
      htVM.AddHandTimings(hts1);

      htVM.AssignStartNumber(htVM.Items[5], 6);

      HandTimingCalc hc = new HandTimingCalc(htVM.Items[5], htVM.Items);
      Assert.AreEqual(new TimeSpan(0, 8, 5, 0, 120), hc.CalculatedTime);
    }


    /// <summary>
    /// Tests hand timing calculation
    /// - This Test Case: special case, there less than 10 hand timings available, use as most as possible for calculation
    /// </summary>
    [TestMethod]
    public void HandTimingCalc_Test3()
    {
      TestDataGenerator tg = new TestDataGenerator();
      HandTimingVM htVM = new HandTimingVM(HandTimingVMEntry.ETimeModus.EFinishTime);

      List<RunResult> rr1 = new List<RunResult>
      {
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  0, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  1, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  2, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  3, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, new TimeSpan(0, 8,  4, 0)),
        tg.createRunResult(tg.createRaceParticipant(), null, null)
      };

      List<TimingData> hts1 = new List<TimingData>
      {
        new TimingData{Time = new TimeSpan(0, 8,  0, 0, 100)},
        new TimingData{Time = new TimeSpan(0, 8,  1, 0, 200)},
        new TimingData{Time = new TimeSpan(0, 8,  2, 0, 300)},
        new TimingData{Time = new TimeSpan(0, 8,  3, 0, 100)},
        new TimingData{Time = new TimeSpan(0, 8,  4, 0, 200)},
        new TimingData{Time = new TimeSpan(0, 8,  5, 0, 300)}
      };

      htVM.AddRunResults(rr1);
      htVM.AddHandTimings(hts1);

      htVM.AssignStartNumber(htVM.Items[5], 6);

      HandTimingCalc hc = new HandTimingCalc(htVM.Items[5], htVM.Items);
      Assert.AreEqual(new TimeSpan(0, 8, 5, 0, 120), hc.CalculatedTime);
    }


    [TestMethod]
    public void HandTimingVMManager_Manage1()
    {
      TestDataGenerator tg = new TestDataGenerator();
      HandTimingVMManager mgr = new HandTimingVMManager(tg.Model);

      var vm1S = mgr.GetHandTimingVM(tg.Model.GetRace(0), tg.Model.GetRace(0).GetRun(0), HandTimingVMEntry.ETimeModus.EStartTime);
      var vm1F = mgr.GetHandTimingVM(tg.Model.GetRace(0), tg.Model.GetRace(0).GetRun(0), HandTimingVMEntry.ETimeModus.EFinishTime);
      var vm2S = mgr.GetHandTimingVM(tg.Model.GetRace(0), tg.Model.GetRace(0).GetRun(1), HandTimingVMEntry.ETimeModus.EStartTime);
      var vm2F = mgr.GetHandTimingVM(tg.Model.GetRace(0), tg.Model.GetRace(0).GetRun(1), HandTimingVMEntry.ETimeModus.EFinishTime);

      Assert.IsNotNull(vm1S);
      Assert.IsNotNull(vm1F);
      Assert.IsNotNull(vm2S);
      Assert.IsNotNull(vm2F);
      Assert.AreNotEqual(vm1S, vm1F);
      Assert.AreNotEqual(vm1S, vm2S);
      Assert.AreNotEqual(vm2S, vm2F);
      Assert.AreNotEqual(vm2S, vm2F);

      Assert.AreEqual(vm1S, mgr.GetHandTimingVM(tg.Model.GetRace(0), tg.Model.GetRace(0).GetRun(0), HandTimingVMEntry.ETimeModus.EStartTime));
      Assert.AreEqual(vm1F, mgr.GetHandTimingVM(tg.Model.GetRace(0), tg.Model.GetRace(0).GetRun(0), HandTimingVMEntry.ETimeModus.EFinishTime));
      Assert.AreEqual(vm2S, mgr.GetHandTimingVM(tg.Model.GetRace(0), tg.Model.GetRace(0).GetRun(1), HandTimingVMEntry.ETimeModus.EStartTime));
      Assert.AreEqual(vm2F, mgr.GetHandTimingVM(tg.Model.GetRace(0), tg.Model.GetRace(0).GetRun(1), HandTimingVMEntry.ETimeModus.EFinishTime));
    }


    [TestMethod]
    public void HandTimingVMManager_StoreAndLoad()
    {

    }


    [TestMethod]
    public void HandTimingVMManager_SaveBackToDataModel()
    {
      TestDataGenerator tg = new TestDataGenerator();

      // Participant 1
      tg.Model.GetRace(0).GetRun(0).SetStartFinishTime(tg.createRaceParticipant(), new TimeSpan(0, 8, 0, 2), new TimeSpan(0, 8, 1, 2));

      // Participant 2
      var p2 = tg.createRaceParticipant();
      tg.Model.GetRace(0).GetRun(0).SetStartFinishTime(p2, new TimeSpan(0, 8, 2, 2), new TimeSpan(0, 8, 3, 2));
      tg.Model.GetRace(0).GetRun(0).SetRunTime(p2, new TimeSpan(0, 0, 1, 0));

      // Participant 3
      tg.Model.GetRace(0).GetRun(0).SetStartFinishTime(tg.createRaceParticipant(), new TimeSpan(0, 8, 4, 2), null);


      HandTimingVMManager mgr = new HandTimingVMManager(tg.Model);
      HandTimingVM htVMS = mgr.GetHandTimingVM(tg.Model.GetRace(0), tg.Model.GetRace(0).GetRun(0), HandTimingVMEntry.ETimeModus.EStartTime);
      HandTimingVM htVMF = mgr.GetHandTimingVM(tg.Model.GetRace(0), tg.Model.GetRace(0).GetRun(0), HandTimingVMEntry.ETimeModus.EFinishTime);


      // Case 1: Finish Time did not yet exist
      // a) check on finish time
      // b) Check on run time
      htVMF.Items[2].SetCalulatedHandTime(new TimeSpan(0, 8, 5, 2, 0));
      Assert.IsNull(tg.Model.GetRace(0).GetRun(0).GetResultList().First(p => p.StartNumber == 3).GetFinishTime());
      mgr.SaveToDataModel();
      Assert.AreEqual(new TimeSpan(0, 8, 5, 2, 0), tg.Model.GetRace(0).GetRun(0).GetResultList().First(p => p.StartNumber == 3).GetFinishTime());
      Assert.AreEqual(new TimeSpan(0, 0, 1, 0, 0), tg.Model.GetRace(0).GetRun(0).GetResultList().First(p => p.StartNumber == 3).GetRunTime());


      // Case 2: Finish Time did exist and runtime was already calculated which need to be correct after setting finish time
      // a) check on finish time
      // b) Check on run time
      htVMF.Items[1].SetCalulatedHandTime(new TimeSpan(0, 8, 3, 2, 200));
      Assert.AreEqual(new TimeSpan(0, 8, 3, 2, 0), tg.Model.GetRace(0).GetRun(0).GetResultList().First(p => p.StartNumber == 2).GetFinishTime());
      mgr.SaveToDataModel();
      Assert.AreEqual(new TimeSpan(0, 8, 3, 2, 200), tg.Model.GetRace(0).GetRun(0).GetResultList().First(p => p.StartNumber == 2).GetFinishTime());
      Assert.AreEqual(new TimeSpan(0, 0, 1, 0, 200), tg.Model.GetRace(0).GetRun(0).GetResultList().First(p => p.StartNumber == 2).GetRunTime());

      // Case 3: Test something with StartTime
      // a) check on finish time
      // b) Check on run time
      htVMS.Items[0].SetCalulatedHandTime(new TimeSpan(0, 8, 0, 1, 0));
      Assert.AreEqual(new TimeSpan(0, 8, 0, 2, 0), tg.Model.GetRace(0).GetRun(0).GetResultList().First(p => p.StartNumber == 1).GetStartTime());
      mgr.SaveToDataModel();
      Assert.AreEqual(new TimeSpan(0, 8, 0, 1, 0), tg.Model.GetRace(0).GetRun(0).GetResultList().First(p => p.StartNumber == 1).GetStartTime());
      Assert.AreEqual(new TimeSpan(0, 0, 1, 1, 0), tg.Model.GetRace(0).GetRun(0).GetResultList().First(p => p.StartNumber == 1).GetRunTime());
    }



    /// <summary>
    /// Full integration test
    /// </summary>
    [TestMethod]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case3\1557MRBR_RH.mdb")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case3\--Handzeit-Start.txt")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case3\--Handzeit-Ziel.txt")]
    public void HandTimingsVM()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"1557MRBR_RH.mdb");
      string hsFilename = @"--Handzeit-Start.txt";
      string hfFilename = @"--Handzeit-Ziel.txt";

      Database db = new Database();
      db.Connect(dbFilename);
      AppDataModel model = new AppDataModel(db);

      FromFileHandTiming hsTiming = new FromFileHandTiming(hsFilename);
      FromFileHandTiming hfTiming = new FromFileHandTiming(hfFilename);

      hsTiming.Connect();
      hfTiming.Connect();

      List<TimingData> hsList = new List<TimingData>(hsTiming.TimingData());
      List<TimingData> hfList = new List<TimingData>(hfTiming.TimingData());


      HandTimingVM htVM = new HandTimingVM(HandTimingVMEntry.ETimeModus.EStartTime);
      htVM.AddRunResults(model.GetRace(0).GetRun(0).GetResultList());
      htVM.AddHandTimings(hsList);

    }

  }
}
