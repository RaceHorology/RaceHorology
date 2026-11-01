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

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;
using System.Collections.ObjectModel;
using System.Threading;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for AppDataModelViewTests
  /// </summary>
  [TestClass]
  public class AppDataModelViewTests
  {
    public AppDataModelViewTests()
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


    /// Overview of Start List Providers
    /// (StartListViewProvider)
    /// [X] FirstRunStartListViewProvider
    /// [X] DSVFirstRunStartListViewProvider
    /// (SecondRunStartListViewProvider)
    /// [X] - SimpleSecondRunStartListViewProvider
    /// [ ] - BasedOnResultsFirstRunStartListViewProvider
    /// [X] RemainingStartListViewProvider

    /// <summary>
    /// FirstRunStartListViewProvider compares the StartNumber based on Sorting and Grouping
    /// </summary>
    [TestMethod]
    public void FirstRunStartListViewProvider_Test()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();

      var participants = tg.Model.GetRace(0).GetParticipants();

      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));

      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));

      FirstRunStartListViewProvider provider = new FirstRunStartListViewProvider();
      provider.Init(participants);

      provider.ChangeGrouping(null);

      // Test initial order
      Assert.AreEqual(6, provider.GetViewList().Count);
      Assert.AreEqual("Name 1", provider.GetViewList()[0].Name);
      Assert.AreEqual("Name 2", provider.GetViewList()[1].Name);
      Assert.AreEqual("Name 3", provider.GetViewList()[2].Name);
      Assert.AreEqual("Name 4", provider.GetViewList()[3].Name);
      Assert.AreEqual("Name 5", provider.GetViewList()[4].Name);
      Assert.AreEqual("Name 6", provider.GetViewList()[5].Name);

      // Test Update when inserting
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      Assert.AreEqual(7, provider.GetViewList().Count);
      Assert.AreEqual("Name 7", provider.GetViewList()[6].Name);

      // Change the start numbers
      tg.Model.GetRace(0).GetParticipants()[0].StartNumber = 3; // Name 1
      tg.Model.GetRace(0).GetParticipants()[1].StartNumber = 2; // Name 2
      tg.Model.GetRace(0).GetParticipants()[2].StartNumber = 1; // Name 3
      Assert.AreEqual(7, provider.GetViewList().Count);
      Assert.AreEqual("Name 3", provider.GetViewList()[0].Name);
      Assert.AreEqual("Name 2", provider.GetViewList()[1].Name);
      Assert.AreEqual("Name 1", provider.GetViewList()[2].Name);
      Assert.AreEqual("Name 4", provider.GetViewList()[3].Name);
      Assert.AreEqual("Name 5", provider.GetViewList()[4].Name);
      Assert.AreEqual("Name 6", provider.GetViewList()[5].Name);
      Assert.AreEqual("Name 7", provider.GetViewList()[6].Name);


      // Delete RaceParticipants
      tg.Model.GetRace(0).GetParticipants().RemoveAt(0);
      Assert.AreEqual(6, provider.GetViewList().Count);
      Assert.AreEqual("Name 3", provider.GetViewList()[0].Name);
      Assert.AreEqual("Name 2", provider.GetViewList()[1].Name);
      Assert.AreEqual("Name 4", provider.GetViewList()[2].Name);
      Assert.AreEqual("Name 5", provider.GetViewList()[3].Name);
      Assert.AreEqual("Name 6", provider.GetViewList()[4].Name);
      Assert.AreEqual("Name 7", provider.GetViewList()[5].Name);

      // Change Grouping
      provider.ChangeGrouping("Participant.Class");
      Assert.AreEqual("Name 3", provider.GetViewList()[0].Name);
      Assert.AreEqual("Name 2", provider.GetViewList()[1].Name);
      Assert.AreEqual("Name 7", provider.GetViewList()[2].Name);
      Assert.AreEqual("Name 4", provider.GetViewList()[3].Name);
      Assert.AreEqual("Name 5", provider.GetViewList()[4].Name);
      Assert.AreEqual("Name 6", provider.GetViewList()[5].Name);
    }


    /// <summary>
    /// Test for DSVFirstRunStartListViewProvider
    ///
    /// Which provides a start list based on startnumber and points following the criterias:
    /// - Best first firstNStartnumbers (5) based on the points are randomized
    /// - Succeeding start list entries are sorted based on the points i.e. startnumber is ignored
    ///   (this enables late nominated starters to start at the right slot)
    /// </summary>
    [TestMethod]
    public void DSVFirstRunStartListViewProvider_Test()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();

      var participants = tg.Model.GetRace(0).GetParticipants();

      // Startnumber 1..10, women
      for (int j = 0; j < 10; ++j)
        tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: (double)(j + 1)); // points: 1.0 ... 10.0

      // Startnumber 11..20, men
      for (int j = 0; j < 10; ++j)
        tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"), points: (double)(j + 1)); // points: 1.0 ... 10.0

      DSVFirstRunStartListViewProvider provider = new DSVFirstRunStartListViewProvider(5);
      provider.Init(participants);
      provider.ChangeGrouping("Participant.Class");

      var startlist = provider.GetViewList();
      // Check initial order by startnumber
      for (int j = 0; j < 10; ++j)
      {
        Assert.AreEqual(string.Format("Name {0}", j + 1), startlist[j].Name);
        Assert.AreEqual((uint)(j+1), startlist[j].StartNumber);
      }
      // Check initial order by startnumber
      for (int j = 0; j < 10; ++j)
      {
        Assert.AreEqual(string.Format("Name {0}", j + 11), startlist[j+10].Name);
        Assert.AreEqual((uint)(j + 11), startlist[j+10].StartNumber);
      }

      // Add two additional starter, one below first 5, one within the remaining
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: 2.0); // StNr 21 => Pos 6
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: 8.1); // StNr 22 => Pos 10
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"), points: 2.0); // StNr 23 => Pos 6
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"), points: 8.1); // StNr 24 => Pos 10

      int i = 0;
      // Women
      Assert.AreEqual("Name 1", startlist[i = 0].Name);
      Assert.AreEqual("Name 2", startlist[++i].Name);
      Assert.AreEqual("Name 3", startlist[++i].Name);
      Assert.AreEqual("Name 4", startlist[++i].Name);
      Assert.AreEqual("Name 5", startlist[++i].Name);
      Assert.AreEqual("Name 21", startlist[++i].Name);
      Assert.AreEqual("Name 6", startlist[++i].Name);
      Assert.AreEqual("Name 7", startlist[++i].Name);
      Assert.AreEqual("Name 8", startlist[++i].Name);
      Assert.AreEqual("Name 22", startlist[++i].Name);
      Assert.AreEqual("Name 9", startlist[++i].Name);
      Assert.AreEqual("Name 10", startlist[++i].Name);
      // Men
      Assert.AreEqual("Name 11", startlist[++i].Name);
      Assert.AreEqual("Name 12", startlist[++i].Name);
      Assert.AreEqual("Name 13", startlist[++i].Name);
      Assert.AreEqual("Name 14", startlist[++i].Name);
      Assert.AreEqual("Name 15", startlist[++i].Name);
      Assert.AreEqual("Name 23", startlist[++i].Name);
      Assert.AreEqual("Name 16", startlist[++i].Name);
      Assert.AreEqual("Name 17", startlist[++i].Name);
      Assert.AreEqual("Name 18", startlist[++i].Name);
      Assert.AreEqual("Name 24", startlist[++i].Name);
      Assert.AreEqual("Name 19", startlist[++i].Name);
      Assert.AreEqual("Name 20", startlist[++i].Name);
    }



    /// <summary>
    /// Test for SimpleSecondRunStartListViewProvider, descending start numbers
    /// Includes bordercases like: adding / removing / changing participants
    /// </summary>
    [TestMethod]
    public void SimpleSecondRunStartListViewProvider_Test_Descending()
    {
      int i;
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();

      var participants = tg.Model.GetRace(0).GetParticipants();
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));

      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));

      FirstRunStartListViewProvider provider1strun = new FirstRunStartListViewProvider();
      provider1strun.Init(participants);
      tg.Model.GetRace(0).GetRun(0).SetStartListProvider(provider1strun);

      SimpleSecondRunStartListViewProvider provider = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Descending);
      provider.Init(tg.Model.GetRace(0).GetRun(0));

      // Test initial order
      Assert.AreEqual(6, provider.GetViewList().Count);
      Assert.AreEqual("Name 6", provider.GetViewList()[i=0].Name);
      Assert.AreEqual("Name 5", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 4", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 3", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 2", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 1", provider.GetViewList()[++i].Name);

      // Test Update when inserting
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      Assert.AreEqual(7, provider.GetViewList().Count);
      Assert.AreEqual("Name 7", provider.GetViewList()[i = 0].Name);
      Assert.AreEqual("Name 6", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 5", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 4", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 3", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 2", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 1", provider.GetViewList()[++i].Name);

      // Change the start numbers
      tg.Model.GetRace(0).GetParticipants()[0].StartNumber = 3; // Name 1
      tg.Model.GetRace(0).GetParticipants()[1].StartNumber = 2; // Name 2
      tg.Model.GetRace(0).GetParticipants()[2].StartNumber = 1; // Name 3
      Assert.AreEqual(7, provider.GetViewList().Count);
      Assert.AreEqual("Name 7", provider.GetViewList()[i = 0].Name);
      Assert.AreEqual("Name 6", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 5", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 4", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 1", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 2", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 3", provider.GetViewList()[++i].Name);

      // Delete RaceParticipants
      tg.Model.GetRace(0).GetParticipants().RemoveAt(0);
      Assert.AreEqual(6, provider.GetViewList().Count);
      Assert.AreEqual("Name 7", provider.GetViewList()[i = 0].Name);
      Assert.AreEqual("Name 6", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 5", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 4", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 2", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 3", provider.GetViewList()[++i].Name);

      // Change Grouping
      provider.ChangeGrouping("Participant.Class");
      Assert.AreEqual(6, provider.GetViewList().Count);
      Assert.AreEqual("Name 7", provider.GetViewList()[i = 0].Name);
      Assert.AreEqual("Name 2", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 3", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 6", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 5", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 4", provider.GetViewList()[++i].Name);
    }

    /// <summary>
    /// Test for SimpleSecondRunStartListViewProvider, ascending start numbers
    /// (just cross-check, main scenarios and border cases are tested by SimpleSecondRunStartListViewProvider_Test_Descending
    /// </summary>
    [TestMethod]
    public void SimpleSecondRunStartListViewProvider_Test_Ascending()
    {
      int i;
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();

      var participants = tg.Model.GetRace(0).GetParticipants();
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));

      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));

      FirstRunStartListViewProvider provider1strun = new FirstRunStartListViewProvider();
      provider1strun.Init(participants);
      tg.Model.GetRace(0).GetRun(0).SetStartListProvider(provider1strun);

      SimpleSecondRunStartListViewProvider provider = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Ascending);
      provider.Init(tg.Model.GetRace(0).GetRun(0));

      // Test initial order
      Assert.AreEqual(6, provider.GetViewList().Count);
      Assert.AreEqual("Name 1", provider.GetViewList()[i = 0].Name);
      Assert.AreEqual("Name 2", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 3", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 4", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 5", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 6", provider.GetViewList()[++i].Name);
    }


    /// <summary>
    /// Test for SimpleSecondRunStartListViewProvider without participants lacking a valid result in the previous run (Issue #613)
    /// </summary>
    [TestMethod]
    public void SimpleSecondRunStartListViewProvider_Test_OnlyWithResults()
    {
      TestDataGenerator tg = new TestDataGenerator();
      var race = tg.Model.GetRace(0);
      for (int i = 0; i < 6; i++)
        tg.createRaceParticipant();

      var run1 = race.GetRun(0);
      FirstRunStartListViewProvider provider1strun = new FirstRunStartListViewProvider();
      provider1strun.Init(race.GetParticipants());
      run1.SetStartListProvider(provider1strun);

      for (int i = 1; i <= 6; i++)
        run1.SetRunTime(race.GetParticipant((uint)i), TimeSpan.FromSeconds(50 + i));
      run1.SetResultCode(race.GetParticipant(2), RunResult.EResultCode.NaS);
      run1.SetResultCode(race.GetParticipant(4), RunResult.EResultCode.DIS);
      var run1Results = new RaceRunResultViewProvider();
      run1Results.Init(run1, tg.Model);
      run1.SetResultViewProvider(run1Results);

      List<uint> startNumbers(StartListViewProvider p) => new List<uint>(System.Linq.Enumerable.Select(p.GetViewList(), x => x.StartNumber));

      var provider = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Descending, allowNonResults: false);
      provider.Init(run1);
      CollectionAssert.AreEqual(new uint[] { 6, 5, 3, 1 }, startNumbers(provider));

      // Later changes of the previous run are reflected
      run1.SetResultCode(race.GetParticipant(5), RunResult.EResultCode.NiZ);
      CollectionAssert.AreEqual(new uint[] { 6, 3, 1 }, startNumbers(provider));
      run1.SetRunTime(race.GetParticipant(2), TimeSpan.FromSeconds(60));
      CollectionAssert.AreEqual(new uint[] { 6, 3, 2, 1 }, startNumbers(provider));

      // Ascending order works the same way
      var providerAsc = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Ascending, allowNonResults: false);
      providerAsc.Init(run1);
      CollectionAssert.AreEqual(new uint[] { 1, 2, 3, 6 }, startNumbers(providerAsc));

      // Default still contains all participants
      var providerAll = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Descending);
      providerAll.Init(run1);
      CollectionAssert.AreEqual(new uint[] { 6, 5, 4, 3, 2, 1 }, startNumbers(providerAll));
    }


    [TestMethod]
    public void BasedOnResultsFirstRunStartListViewProvider_Test()
    {
      var tg = new TestDataGenerator();
      var race = tg.Model.GetRace(0);
      for (int i = 0; i < 7; i++)
        tg.createRaceParticipant();

      var run1 = race.GetRun(0);
      for (int i = 1; i <= 7; i++)
        run1.SetRunTime(race.GetParticipant((uint)i), TimeSpan.FromSeconds(50 + 4 * (i - 1)));
      var run1Results = new RaceRunResultViewProvider();
      run1Results.Init(run1, tg.Model);
      run1.SetResultViewProvider(run1Results);

      var provider = new BasedOnResultsFirstRunStartListViewProvider(5, false);
      provider.Init(run1);
      CollectionAssert.AreEqual(new uint[] { 5, 4, 3, 2, 1, 6, 7 },
        new List<uint>(System.Linq.Enumerable.Select(provider.GetViewList(), x => x.StartNumber)));
    }

    [TestMethod]
    public void BasedOnCumulativeResultsStartListViewProvider_ThirdRun_Test()
    {
      var tg = new TestDataGenerator();
      var race = tg.Model.GetRace(0);
      race.AddRaceRun();
      race.AddRaceRun();
      for (int i = 0; i < 7; i++)
        tg.createRaceParticipant();

      var run1 = race.GetRun(0);
      var run2 = race.GetRun(1);
      for (int i = 1; i <= 7; i++)
      {
        run1.SetRunTime(race.GetParticipant((uint)i), TimeSpan.FromSeconds(50 + 4 * (i - 1)));
        run2.SetRunTime(race.GetParticipant((uint)i), TimeSpan.FromSeconds(74 - 2 * (i - 1)));
      }
      var run1Results = new RaceRunResultViewProvider();
      run1Results.Init(run1, tg.Model);
      run1.SetResultViewProvider(run1Results);
      var run2Results = new RaceRunResultViewProvider();
      run2Results.Init(run2, tg.Model);
      run2.SetResultViewProvider(run2Results);

      var provider = new BasedOnResultsFirstRunStartListViewProvider(5, false, true);
      provider.Init(run2); // Generates the start list for run 3, using runs 1+2.
      CollectionAssert.AreEqual(new uint[] { 5, 4, 3, 2, 1, 6, 7 },
        new List<uint>(System.Linq.Enumerable.Select(provider.GetViewList(), x => x.StartNumber)));

      // A correction to an earlier run must update the start order immediately.
      run2.SetRunTime(race.GetParticipant(7), TimeSpan.FromSeconds(40));
      CollectionAssert.AreEqual(new uint[] { 4, 3, 2, 1, 7, 5, 6 },
        new List<uint>(System.Linq.Enumerable.Select(provider.GetViewList(), x => x.StartNumber)));
    }


    /// <summary>
    /// Cumulative mode with penalty rule: participants with penalty time stay in the start list,
    /// placed by their sum incl. penalty time, but are never reversed (Issue #253)
    /// </summary>
    [TestMethod]
    public void BasedOnCumulativeResultsStartListViewProvider_Penalty_Test()
    {
      var tg = new TestDataGenerator();
      var race = tg.Model.GetRace(0);
      race.AddRaceRun();
      race.AddRaceRun();
      for (int i = 0; i < 5; i++)
        tg.createRaceParticipant();

      var run1 = race.GetRun(0);
      var run2 = race.GetRun(1);

      // Penalty: best + 10s
      // Run 1: best 50 => cut off 60: 5 (80s) and 4 (DIS) get 60s penalty time
      run1.SetRunTime(race.GetParticipant(1), TimeSpan.FromSeconds(50));
      run1.SetRunTime(race.GetParticipant(2), TimeSpan.FromSeconds(52));
      run1.SetRunTime(race.GetParticipant(3), TimeSpan.FromSeconds(54));
      run1.SetResultCode(race.GetParticipant(4), RunResult.EResultCode.DIS);
      run1.SetRunTime(race.GetParticipant(5), TimeSpan.FromSeconds(80));
      // Run 2: best 41 => cut off 51: no penalty
      run2.SetRunTime(race.GetParticipant(1), TimeSpan.FromSeconds(50));
      run2.SetRunTime(race.GetParticipant(2), TimeSpan.FromSeconds(50));
      run2.SetRunTime(race.GetParticipant(3), TimeSpan.FromSeconds(50));
      run2.SetRunTime(race.GetParticipant(4), TimeSpan.FromSeconds(50));
      run2.SetRunTime(race.GetParticipant(5), TimeSpan.FromSeconds(41));
      foreach (var run in new[] { run1, run2 })
      {
        var results = new PenaltyRaceRunResultViewProvider(PenaltyRaceRunResultViewProvider.EMode.BestPlusSeconds, 10);
        results.Init(run, tg.Model);
        run.SetResultViewProvider(results);
      }

      // Sum: 1 => 100, 5 => 101 (penalty), 2 => 102, 3 => 104, 4 => 110 (penalty)
      var provider = new BasedOnResultsFirstRunStartListViewProvider(3, false, true);
      provider.Init(run2);
      // Best 3 without penalty (1, 2, 3) reversed, 5 is skipped but stays in the list by its sum
      CollectionAssert.AreEqual(new uint[] { 3, 2, 1, 5, 4 },
        new List<uint>(System.Linq.Enumerable.Select(provider.GetViewList(), x => x.StartNumber)));

      // Shown time is the sum incl. penalty time
      var entry5 = System.Linq.Enumerable.First(provider.GetViewList(), x => x.StartNumber == 5) as StartListEntryAdditionalRun;
      Assert.AreEqual(TimeSpan.FromSeconds(101), entry5.Runtime);
    }


    /// <summary>
    /// Start list based on the best single run of all previous runs (idea from PR #690), incl. penalty rule
    /// </summary>
    [TestMethod]
    public void BasedOnBestRunResultsStartListViewProvider_Test()
    {
      var tg = new TestDataGenerator();
      var race = tg.Model.GetRace(0);
      race.AddRaceRun();
      race.AddRaceRun();
      for (int i = 0; i < 7; i++)
        tg.createRaceParticipant();

      var run1 = race.GetRun(0);
      var run2 = race.GetRun(1);

      // Penalty: best + 10s
      // Run 1: best 50 => cut off 60
      run1.SetRunTime(race.GetParticipant(1), TimeSpan.FromSeconds(50));
      run1.SetRunTime(race.GetParticipant(2), TimeSpan.FromSeconds(60));
      run1.SetRunTime(race.GetParticipant(3), TimeSpan.FromSeconds(52));
      run1.SetResultCode(race.GetParticipant(4), RunResult.EResultCode.DIS);  // penalty 60
      run1.SetRunTime(race.GetParticipant(5), TimeSpan.FromSeconds(54));
      run1.SetResultCode(race.GetParticipant(6), RunResult.EResultCode.NaS);
      run1.SetResultCode(race.GetParticipant(7), RunResult.EResultCode.NaS);
      // Run 2: best 45 => cut off 55
      run2.SetRunTime(race.GetParticipant(1), TimeSpan.FromSeconds(55));
      run2.SetRunTime(race.GetParticipant(2), TimeSpan.FromSeconds(45));
      run2.SetResultCode(race.GetParticipant(3), RunResult.EResultCode.NiZ);  // penalty 55
      run2.SetRunTime(race.GetParticipant(4), TimeSpan.FromSeconds(49));
      run2.SetRunTime(race.GetParticipant(5), TimeSpan.FromSeconds(47));
      run2.SetResultCode(race.GetParticipant(6), RunResult.EResultCode.NiZ);  // penalty 55, only valid run
      run2.SetResultCode(race.GetParticipant(7), RunResult.EResultCode.NaS);  // no valid run at all
      foreach (var run in new[] { run1, run2 })
      {
        var results = new PenaltyRaceRunResultViewProvider(PenaltyRaceRunResultViewProvider.EMode.BestPlusSeconds, 10);
        results.Init(run, tg.Model);
        run.SetResultViewProvider(results);
      }

      List<uint> startNumbers(StartListViewProvider p) => new List<uint>(System.Linq.Enumerable.Select(p.GetViewList(), x => x.StartNumber));

      // Best run: 2 => 45, 5 => 47, 4 => 49, 1 => 50, 3 => 52, 6 => 55 (penalty)
      var provider = new BasedOnResultsFirstRunStartListViewProvider(3, false, BasedOnResultsFirstRunStartListViewProvider.EBasedOn.BestOfPreviousRuns);
      provider.Init(run2);
      // Best 3 reversed, then the rest by best run; 6 stays (penalty, not reversed), 7 without any valid run is omitted
      CollectionAssert.AreEqual(new uint[] { 4, 5, 2, 1, 3, 6 }, startNumbers(provider));

      var entry1 = System.Linq.Enumerable.First(provider.GetViewList(), x => x.StartNumber == 1) as StartListEntryAdditionalRun;
      Assert.AreEqual(TimeSpan.FromSeconds(50), entry1.Runtime);

      // Participants without any valid run are appended if requested
      var providerAll = new BasedOnResultsFirstRunStartListViewProvider(3, true, BasedOnResultsFirstRunStartListViewProvider.EBasedOn.BestOfPreviousRuns);
      providerAll.Init(run2);
      CollectionAssert.AreEqual(new uint[] { 4, 5, 2, 1, 3, 6, 7 }, startNumbers(providerAll));

      // A later correction of an earlier run updates the start order
      run1.SetRunTime(race.GetParticipant(3), TimeSpan.FromSeconds(44));
      CollectionAssert.AreEqual(new uint[] { 5, 2, 3, 4, 1, 6 }, startNumbers(provider));
    }


    /// <summary>
    /// Race configuration "Summe aller vorherigen Läufe" applies to all runs >= 2: run 3 is based on the sum of runs 1 + 2 (Issue #253)
    /// </summary>
    [TestMethod]
    public void ViewConfigurator_CumulativeStartList_ThirdRun_Test()
    {
      var tg = new TestDataGenerator();
      var race = tg.Model.GetRace(0);
      race.AddRaceRun();
      race.AddRaceRun();
      for (int i = 0; i < 5; i++)
        tg.createRaceParticipant();

      var run1 = race.GetRun(0);
      var run2 = race.GetRun(1);
      var run3 = race.GetRun(2);
      // Sum: 1 => 100, 2 => 98, 3 => 96, 4 => 94, 5 => 92 (run 2 alone would give the opposite order)
      for (int i = 1; i <= 5; i++)
      {
        run1.SetRunTime(race.GetParticipant((uint)i), TimeSpan.FromSeconds(60 - 4 * (i - 1)));
        run2.SetRunTime(race.GetParticipant((uint)i), TimeSpan.FromSeconds(40 + 2 * (i - 1)));
      }
      foreach (var run in new[] { run1, run2 })
      {
        var results = new RaceRunResultViewProvider();
        results.Init(run, tg.Model);
        run.SetResultViewProvider(results);
      }

      var cfg = race.RaceConfiguration.Copy();
      cfg.Run2_StartistView = StartlistViewConfig.Run2_Cumulative;
      cfg.Run2_StartistViewParams = new Dictionary<string, object> { { StartlistViewConfig.ParamReverseBestN, 3 }, { StartlistViewConfig.ParamAllowNonResults, false } };
      race.RaceConfiguration = cfg;

      var provider = new ViewConfigurator(race).GetStartlistViewProvider(run3);
      // Best 3 by sum (5, 4, 3) reversed, then the rest by sum
      CollectionAssert.AreEqual(new uint[] { 3, 4, 5, 2, 1 },
        new List<uint>(System.Linq.Enumerable.Select(provider.GetViewList(), x => x.StartNumber)));
    }


    /// <summary>
    /// RemainingStartListViewProvider compares the StartNumber based on Sorting and Grouping
    /// </summary>
    [TestMethod]
    public void RemainingStartListViewProvider_Test_AdaptToStartList()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.Model.SetCurrentRace(tg.Model.GetRace(0));
      tg.Model.SetCurrentRaceRun(tg.Model.GetCurrentRace().GetRun(0));
      tg.createCatsClassesGroups();

      var participants = tg.Model.GetRace(0).GetParticipants();

      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));

      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));

      FirstRunStartListViewProvider masterProvider = new FirstRunStartListViewProvider();
      masterProvider.Init(participants);
      RemainingStartListViewProvider provider = new RemainingStartListViewProvider();
      provider.Init(masterProvider, tg.Model.GetCurrentRaceRun());

      provider.ChangeGrouping(null);

      // Test initial order
      Assert.AreEqual(6, provider.GetView().ViewToList<StartListEntry>().Count);
      Assert.AreEqual("Name 1", provider.GetView().ViewToList<StartListEntry>()[0].Name);
      Assert.AreEqual("Name 2", provider.GetView().ViewToList<StartListEntry>()[1].Name);
      Assert.AreEqual("Name 3", provider.GetView().ViewToList<StartListEntry>()[2].Name);
      Assert.AreEqual("Name 4", provider.GetView().ViewToList<StartListEntry>()[3].Name);
      Assert.AreEqual("Name 5", provider.GetView().ViewToList<StartListEntry>()[4].Name);
      Assert.AreEqual("Name 6", provider.GetView().ViewToList<StartListEntry>()[5].Name);

      // Test Update when inserting
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      Assert.AreEqual(7, provider.GetView().ViewToList<StartListEntry>().Count);
      Assert.AreEqual("Name 7", provider.GetView().ViewToList<StartListEntry>()[6].Name);

      // Change the start numbers
      tg.Model.GetRace(0).GetParticipants()[0].StartNumber = 3; // Name 1
      tg.Model.GetRace(0).GetParticipants()[1].StartNumber = 2; // Name 2
      tg.Model.GetRace(0).GetParticipants()[2].StartNumber = 1; // Name 3
      Assert.AreEqual(7, provider.GetView().ViewToList<StartListEntry>().Count);
      Assert.AreEqual("Name 3", provider.GetView().ViewToList<StartListEntry>()[0].Name);
      Assert.AreEqual("Name 2", provider.GetView().ViewToList<StartListEntry>()[1].Name);
      Assert.AreEqual("Name 1", provider.GetView().ViewToList<StartListEntry>()[2].Name);
      Assert.AreEqual("Name 4", provider.GetView().ViewToList<StartListEntry>()[3].Name);
      Assert.AreEqual("Name 5", provider.GetView().ViewToList<StartListEntry>()[4].Name);
      Assert.AreEqual("Name 6", provider.GetView().ViewToList<StartListEntry>()[5].Name);
      Assert.AreEqual("Name 7", provider.GetView().ViewToList<StartListEntry>()[6].Name);


      // Delete RaceParticipants
      tg.Model.GetRace(0).GetParticipants().RemoveAt(0);
      Assert.AreEqual(6, provider.GetView().ViewToList<StartListEntry>().Count);
      Assert.AreEqual("Name 3", provider.GetView().ViewToList<StartListEntry>()[0].Name);
      Assert.AreEqual("Name 2", provider.GetView().ViewToList<StartListEntry>()[1].Name);
      Assert.AreEqual("Name 4", provider.GetView().ViewToList<StartListEntry>()[2].Name);
      Assert.AreEqual("Name 5", provider.GetView().ViewToList<StartListEntry>()[3].Name);
      Assert.AreEqual("Name 6", provider.GetView().ViewToList<StartListEntry>()[4].Name);
      Assert.AreEqual("Name 7", provider.GetView().ViewToList<StartListEntry>()[5].Name);

      // Change Grouping
      provider.ChangeGrouping("Participant.Class");
      Assert.AreEqual("Name 3", provider.GetView().ViewToList<StartListEntry>()[0].Name);
      Assert.AreEqual("Name 2", provider.GetView().ViewToList<StartListEntry>()[1].Name);
      Assert.AreEqual("Name 7", provider.GetView().ViewToList<StartListEntry>()[2].Name);
      Assert.AreEqual("Name 4", provider.GetView().ViewToList<StartListEntry>()[3].Name);
      Assert.AreEqual("Name 5", provider.GetView().ViewToList<StartListEntry>()[4].Name);
      Assert.AreEqual("Name 6", provider.GetView().ViewToList<StartListEntry>()[5].Name);
    }

    /// <summary>
    /// RemainingStartListViewProvider proxies a start list
    /// If the starter already started, the flag Started of the StartListEntry is set to true.
    /// </summary>
    [TestMethod]
    public void RemainingStartListViewProvider_Test_AdaptToRunResults()
    {
      int i;
      TestDataGenerator tg = new TestDataGenerator();
      tg.Model.SetCurrentRace(tg.Model.GetRace(0));
      tg.Model.SetCurrentRaceRun(tg.Model.GetCurrentRace().GetRun(0));

      Race race = tg.Model.GetCurrentRace();
      RaceRun rr = tg.Model.GetCurrentRaceRun();

      tg.createCatsClassesGroups();

      var participants = tg.Model.GetRace(0).GetParticipants();

      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));

      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));

      FirstRunStartListViewProvider masterProvider = new FirstRunStartListViewProvider();
      masterProvider.Init(participants);
      RemainingStartListViewProvider provider = new RemainingStartListViewProvider();
      provider.Init(masterProvider, tg.Model.GetCurrentRaceRun());

      provider.ChangeGrouping(null);

      // Test initial order
      Assert.AreEqual(6, provider.GetView().ViewToList<StartListEntry>().Count);
      Assert.AreEqual("Name 1", provider.GetView().ViewToList<StartListEntry>()[i=0].Name);
      Assert.AreEqual("Name 2", provider.GetView().ViewToList<StartListEntry>()[++i].Name);
      Assert.AreEqual("Name 3", provider.GetView().ViewToList<StartListEntry>()[++i].Name);
      Assert.AreEqual("Name 4", provider.GetView().ViewToList<StartListEntry>()[++i].Name);
      Assert.AreEqual("Name 5", provider.GetView().ViewToList<StartListEntry>()[++i].Name);
      Assert.AreEqual("Name 6", provider.GetView().ViewToList<StartListEntry>()[++i].Name);

      // Test initial "Started"
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[i=0].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);

      // Start of StNr1
      rr.SetStartTime(race.GetParticipant(1), new TimeSpan(8, 0, 0));
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[i=0].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);

      // Start of StNr2
      // Finish of StNr1
      rr.SetFinishTime(race.GetParticipant(1), new TimeSpan(8, 1, 0));
      rr.SetStartTime(race.GetParticipant(2), new TimeSpan(8, 1, 0));
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[i=0].Started);
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);

      // NaS of StNr3
      // Finish of StNr2
      rr.SetFinishTime(race.GetParticipant(2), new TimeSpan(8, 2, 0));
      rr.SetResultCode(race.GetParticipant(3), RunResult.EResultCode.NaS);
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[i=0].Started);
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);

      // Clear Finish StNr1
      rr.SetFinishTime(race.GetParticipant(1), null);
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[i=0].Started);
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);

      // Clear Start StNr1
      rr.SetStartTime(race.GetParticipant(1), null);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[i=0].Started);
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);

      // ReStart and Finsih of StNr 1
      rr.SetStartFinishTime(race.GetParticipant(1), new TimeSpan(8, 1, 0), new TimeSpan(8, 1, 10));
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[i=0].Started);
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);

      // Delete RunResult
      rr.DeleteRunResult(race.GetParticipant(1));
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[i=0].Started);
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(true, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);

      // Delete all RunResult
      rr.DeleteRunResults();
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[i=0].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
      Assert.AreEqual(false, provider.GetView().ViewToList<StartListEntry>()[++i].Started);
    }




    /// Overview of ResultViewProvider
    ///
    /// (ResultViewProvider)
    /// [X] RaceRunResultViewProvider
    /// [X] RaceResultViewProvider
    /// [ ] DSVSchoolRaceResultViewProvider <- RaceResultViewProvider
    ///
    /// Basis of all:
    /// [X] RuntimeSorter
    /// [X] TotalTimeSorter


    /// <summary>
    /// Test for RuntimeSorter
    ///
    /// Compares two RunResults, taking into account:
    /// - Group (Class, Group, Category)
    /// - Runtime
    /// - ResultCode
    /// - StartNumber
    /// </summary>
    [TestMethod]
    public void RuntimeSorterTest()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();

      tg.Model.SetCurrentRace(tg.Model.GetRace(0));
      tg.Model.SetCurrentRaceRun(tg.Model.GetCurrentRace().GetRun(0));
      Race race = tg.Model.GetCurrentRace();
      RaceRun rr = tg.Model.GetCurrentRaceRun();

      var participants = tg.Model.GetRace(0).GetParticipants();

      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));

      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));

      var rr1 = tg.createRunResult(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0));
      var rr2 = tg.createRunResult(race.GetParticipant(2), new TimeSpan(8, 1, 0), new TimeSpan(8, 2, 1));
      var rr3 = tg.createRunResult(race.GetParticipant(3), new TimeSpan(8, 2, 0), new TimeSpan(8, 2, 59));
      var rr4 = tg.createRunResult(race.GetParticipant(4), new TimeSpan(8, 3, 0), new TimeSpan(8, 4, 0));

      var rr1w = tg.createRunResult(race.GetParticipant(5), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0));
      var rr2w = tg.createRunResult(race.GetParticipant(6), new TimeSpan(8, 1, 0), new TimeSpan(8, 2, 1));
      var rr3w = tg.createRunResult(race.GetParticipant(7), new TimeSpan(8, 2, 0), new TimeSpan(8, 2, 59));
      var rr4w = tg.createRunResult(race.GetParticipant(8), new TimeSpan(8, 3, 0), new TimeSpan(8, 4, 0));

      RuntimeSorter rs = new RuntimeSorter();

      // Standard order
      Assert.AreEqual(-1, rs.Compare(rr1, rr2));
      Assert.AreEqual(1, rs.Compare(rr2, rr1));

      // ... including transitivity: rr3 < rr1 < rr2 => rr3 < rr2
      Assert.AreEqual(-1, rs.Compare(rr3, rr1));
      Assert.AreEqual(-1, rs.Compare(rr1, rr2));
      Assert.AreEqual(-1, rs.Compare(rr3, rr2));

      // Equality (same time, same startnumber)
      Assert.AreEqual(0, rs.Compare(rr1, rr1));

      // Same time, different startnumber
      Assert.AreEqual(rr1.Runtime, rr4.Runtime);
      Assert.AreEqual(-1, rs.Compare(rr1, rr4));


      // Some Flags
      var rrF1 = tg.createRunResult(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0));
      var rrF1e1 = tg.createRunResult(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0)); rrF1e1.ResultCode = RunResult.EResultCode.NaS;
      var rrF1e2 = tg.createRunResult(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0)); rrF1e2.ResultCode = RunResult.EResultCode.NiZ;
      var rrF1e3 = tg.createRunResult(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0)); rrF1e3.ResultCode = RunResult.EResultCode.DIS;
      var rrF1e4 = tg.createRunResult(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0)); rrF1e4.ResultCode = RunResult.EResultCode.NQ;
      var rrF1e5 = tg.createRunResult(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0)); rrF1e5.ResultCode = RunResult.EResultCode.NotSet;
      var rrF2e1 = tg.createRunResult(race.GetParticipant(2), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0)); rrF2e1.ResultCode = RunResult.EResultCode.NaS;
      Assert.AreEqual(-1, rs.Compare(rrF1, rrF1e1));
      Assert.AreEqual(-1, rs.Compare(rrF1, rrF1e2));
      Assert.AreEqual(-1, rs.Compare(rrF1, rrF1e3));
      Assert.AreEqual(-1, rs.Compare(rrF1, rrF1e4));
      Assert.AreEqual(-1, rs.Compare(rrF1, rrF1e5));

      // No time, same startnumber
      Assert.AreEqual(0, rs.Compare(rrF1e1, rrF1e5));
      // No time, different startnumber
      Assert.IsNull(rrF1e1.Runtime);
      Assert.IsNull(rrF2e1.Runtime);
      Assert.AreEqual(-1, rs.Compare(rrF1e1, rrF2e1));

      // Grouping
      Assert.AreEqual(-1, rs.Compare(rr3w, rr1));
      rs.SetGrouping("Participant.Class");
      Assert.AreEqual(1, rs.Compare(rr3w, rr1));
    }


    /// <summary>
    /// Test for RaceRunResultViewProvider
    ///
    /// What it does:
    /// - Checks the RunResultWithPosition of RaceRunResultViewProvider
    /// - Based on simulated race data
    /// - Check correct handling of changing participant as well as RunResult
    /// - Checks DeleteRunResult
    /// </summary>
    [TestMethod]
    public void RaceRunResultViewProviderTest_Dynamic()
    {
      int i = 0;
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();

      tg.Model.SetCurrentRace(tg.Model.GetRace(0));
      tg.Model.SetCurrentRaceRun(tg.Model.GetCurrentRace().GetRun(0));
      Race race = tg.Model.GetCurrentRace();
      RaceRun rr = tg.Model.GetCurrentRaceRun();

      var participants = tg.Model.GetRace(0).GetParticipants();

      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));

      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));


      RaceRunResultViewProvider vp = new RaceRunResultViewProvider();
      vp.ChangeGrouping("Participant.Class");
      vp.Init(rr, tg.Model);

      // All race participants shall be in the view, even if no results are existing
      Assert.AreEqual(12, vp.GetView().ViewToList<RunResultWithPosition>().Count);


      // Class 2M...
      rr.SetStartFinishTime(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0));  // 1:00,00
      rr.SetStartFinishTime(race.GetParticipant(2), new TimeSpan(8, 1, 0), new TimeSpan(8, 2, 1));  // 1:01,00
      rr.SetStartFinishTime(race.GetParticipant(3), new TimeSpan(8, 2, 0), new TimeSpan(8, 2, 59)); // 0:59,00
      rr.SetResultCode(race.GetParticipant(4), RunResult.EResultCode.NaS);
      rr.SetStartFinishTime(race.GetParticipant(5), new TimeSpan(8, 3, 0), new TimeSpan(0, 8, 3, 59, 990));  // 0:59,99
      rr.SetStartFinishTime(race.GetParticipant(6), new TimeSpan(8, 4, 0), new TimeSpan(8, 5, 0));  // 1:00,00

      // Class 2W...
      rr.SetStartFinishTime(race.GetParticipant(7), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0));  // 1:00,00
      rr.SetStartFinishTime(race.GetParticipant(8), new TimeSpan(8, 1, 0), new TimeSpan(8, 2, 1));  // 1:01,00
      rr.SetStartFinishTime(race.GetParticipant(9), new TimeSpan(8, 2, 0), new TimeSpan(8, 2, 59)); // 0:59,00
      rr.SetResultCode(race.GetParticipant(10), RunResult.EResultCode.NaS);
      rr.SetStartFinishTime(race.GetParticipant(11), new TimeSpan(8, 3, 0), new TimeSpan(0, 8, 3, 59, 990));  // 0:59,99
      rr.SetStartFinishTime(race.GetParticipant(12), new TimeSpan(8, 4, 0), new TimeSpan(8, 5, 0));  // 1:00,00


      Assert.AreEqual(12, vp.GetView().ViewToList<RunResultWithPosition>().Count);

      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i = 0].StartNumber);
      Assert.AreEqual(1U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(5U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(2U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(1U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(6U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(2U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(5U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(4U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(0U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);

      Assert.AreEqual(9U , vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(1U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(11U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(2U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(7U , vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(12U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(8U , vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(5U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(10U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(0U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);

      // Activities:

      // Update of RunResult
      rr.SetFinishTime(race.GetParticipant(1), new TimeSpan(8, 1, 2));  // 1:02,00
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i = 0].StartNumber);
      Assert.AreEqual(1U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(5U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(2U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(6U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(2U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(4U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(1U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(5U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(4U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(0U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);

      // Disqualify
      rr.SetResultCode(race.GetParticipant(5), RunResult.EResultCode.DIS, "Test");
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i = 0].StartNumber);
      Assert.AreEqual(1U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(6U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(2U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(2U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(1U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(4U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(4U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(0U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(5U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(0U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);

      // Add Participant
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)")); // StNr 13
      rr.SetStartFinishTime(race.GetParticipant(13), new TimeSpan(8, 13, 0), new TimeSpan(8, 13, 10));  // 0:10,00
      Assert.AreEqual(13, vp.GetView().ViewToList<RunResultWithPosition>().Count);
      Assert.AreEqual(13U, vp.GetView().ViewToList<RunResultWithPosition>()[i = 0].StartNumber);
      Assert.AreEqual(1U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(2U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(6U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(2U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(4U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(1U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(5U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(4U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(0U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(5U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(0U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);

      // Delete of RunResult(s)
      rr.DeleteRunResult(race.GetParticipant(3));
      Assert.AreEqual(13, vp.GetView().ViewToList<RunResultWithPosition>().Count);
      Assert.AreEqual(13U, vp.GetView().ViewToList<RunResultWithPosition>()[i = 0].StartNumber);
      Assert.AreEqual(1U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(6U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(2U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(2U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(1U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(4U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(0U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(4U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(0U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(5U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(0U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
    }

    /// <summary>
    /// Test for RaceRunResultViewProvider
    ///
    /// What it does:
    /// - Check correct handling directly after Init() based on simulated data
    /// </summary>
    [TestMethod]
    public void RaceRunResultViewProviderTest_Init()
    {
      int i = 0;
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();

      tg.Model.SetCurrentRace(tg.Model.GetRace(0));
      tg.Model.SetCurrentRaceRun(tg.Model.GetCurrentRace().GetRun(0));
      Race race = tg.Model.GetCurrentRace();
      RaceRun rr = tg.Model.GetCurrentRaceRun();

      var participants = tg.Model.GetRace(0).GetParticipants();

      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));

      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));

      // Class 2M...
      rr.SetStartFinishTime(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0));  // 1:00,00
      rr.SetStartFinishTime(race.GetParticipant(2), new TimeSpan(8, 1, 0), new TimeSpan(8, 2, 1));  // 1:01,00
      rr.SetStartFinishTime(race.GetParticipant(3), new TimeSpan(8, 2, 0), new TimeSpan(8, 2, 59)); // 0:59,00
      rr.SetResultCode(race.GetParticipant(4), RunResult.EResultCode.NaS);
      rr.SetStartFinishTime(race.GetParticipant(5), new TimeSpan(8, 3, 0), new TimeSpan(0, 8, 3, 59, 990));  // 0:59,99
      rr.SetStartFinishTime(race.GetParticipant(6), new TimeSpan(8, 4, 0), new TimeSpan(8, 5, 0));  // 1:00,00

      // Class 2W...
      rr.SetStartFinishTime(race.GetParticipant(7), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0));  // 1:00,00
      rr.SetStartFinishTime(race.GetParticipant(8), new TimeSpan(8, 1, 0), new TimeSpan(8, 2, 1));  // 1:01,00
      rr.SetStartFinishTime(race.GetParticipant(9), new TimeSpan(8, 2, 0), new TimeSpan(8, 2, 59)); // 0:59,00
      rr.SetResultCode(race.GetParticipant(10), RunResult.EResultCode.NaS);
      rr.SetStartFinishTime(race.GetParticipant(11), new TimeSpan(8, 3, 0), new TimeSpan(0, 8, 3, 59, 990));  // 0:59,99
      rr.SetStartFinishTime(race.GetParticipant(12), new TimeSpan(8, 4, 0), new TimeSpan(8, 5, 0));  // 1:00,00

      RaceRunResultViewProvider vp = new RaceRunResultViewProvider();
      vp.ChangeGrouping("Participant.Class");
      vp.Init(rr, tg.Model);

      // All race participants shall be in the view, even if no results are existing
      Assert.AreEqual(12, vp.GetView().ViewToList<RunResultWithPosition>().Count);

      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i = 0].StartNumber);
      Assert.AreEqual(1U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(5U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(2U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(1U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(6U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(2U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(5U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(4U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(0U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);

      Assert.AreEqual(9U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(1U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(11U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(2U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(7U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(12U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(3U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(8U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(5U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
      Assert.AreEqual(10U, vp.GetView().ViewToList<RunResultWithPosition>()[++i].StartNumber);
      Assert.AreEqual(0U, vp.GetView().ViewToList<RunResultWithPosition>()[i].Position);
    }


    /// <summary>
    /// Tests whether the positions are correct in case two or more participants have the same runtime
    /// </summary>
    [TestMethod]
    public void RaceRunResultViewProviderTest_SameRunTime()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.Model.SetCurrentRace(tg.Model.GetRace(0));
      tg.Model.SetCurrentRaceRun(tg.Model.GetCurrentRace().GetRun(0));
      Race race = tg.Model.GetCurrentRace();
      RaceRun rr = tg.Model.GetCurrentRaceRun();

      var participants = tg.Model.GetRace(0).GetParticipants();

      tg.createRaceParticipant(); // StNr 1
      tg.createRaceParticipant(); // StNr 2
      tg.createRaceParticipant(); // StNr 3
      tg.createRaceParticipant(); // StNr 4

      rr.SetRunTime(race.GetParticipant(1), new TimeSpan(0, 0, 0, 11, 0));
      rr.SetRunTime(race.GetParticipant(2), new TimeSpan(0, 0, 0, 11, 100));
      rr.SetRunTime(race.GetParticipant(3), new TimeSpan(0, 0, 0, 11, 0));
      rr.SetRunTime(race.GetParticipant(4), new TimeSpan(0, 0, 0, 10, 990));

      RaceRunResultViewProvider vp = new RaceRunResultViewProvider();
      vp.Init(rr, tg.Model);

      var ranking = vp.GetView().ViewToList<RunResultWithPosition>();
      Assert.AreEqual(1U, ranking[0].Position);
      Assert.AreEqual(4U, ranking[0].StartNumber);

      Assert.AreEqual(2U, ranking[1].Position);
      Assert.AreEqual(1U, ranking[1].StartNumber);

      Assert.AreEqual(2U, ranking[2].Position);
      Assert.AreEqual(3U, ranking[2].StartNumber);

      Assert.AreEqual(4U, ranking[3].Position);
      Assert.AreEqual(2U, ranking[3].StartNumber);
    }



    /// <summary>
    /// Test for RaceRunResultViewProvider
    ///
    /// What it does:
    /// - Checks the RunResultWithPosition of RaceRunResultViewProvider
    /// - Based on simulated race data
    /// - Check correct handling of changing participant as well as RunResult
    /// - Checks DeleteRunResult
    /// </summary>
    [TestMethod]
    public void PenaltyRaceRunResultViewProviderTest_Dynamic()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();
      tg.Model.SetCurrentRace(tg.Model.GetRace(0));
      tg.Model.SetCurrentRaceRun(tg.Model.GetCurrentRace().GetRun(0));
      Race race = tg.Model.GetCurrentRace();
      RaceRun rr = tg.Model.GetCurrentRaceRun();

      var participants = tg.Model.GetRace(0).GetParticipants();

      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));

      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));


      RaceRunResultViewProvider vp = new PenaltyRaceRunResultViewProvider(PenaltyRaceRunResultViewProvider.EMode.BestPlusPercentage, 30.0);
      vp.ChangeGrouping("Participant.Class");
      vp.Init(rr, tg.Model);

      void checkResults(IList<dynamic> results)
      {
        int i = 0;
        foreach (var r in results)
        {
          var entry = vp.GetView().ViewToList<RunResultWithPosition>()[i];
          Assert.AreEqual(r.StNr, entry.StartNumber);
          Assert.AreEqual(r.Position, entry.Position);
          Assert.AreEqual(r.Time, entry.Runtime);
          Assert.AreEqual(r.Code, entry.ResultCode);
          i++;
        }
      }


      // Class 2M...
      rr.SetStartFinishTime(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 1));  // 1:01,00
      checkResults(new[] {
        new { StNr = 1U, Position = 1U, Time = (TimeSpan?)new TimeSpan(0, 1, 1), Code = RunResult.EResultCode.Normal },
        new { StNr = 2U, Position = 0U, Time = (TimeSpan?)null, Code = RunResult.EResultCode.NotSet },
      });
      rr.SetStartFinishTime(race.GetParticipant(2), new TimeSpan(8, 1, 0), new TimeSpan(8, 2, 0));  // 1:00,00
      checkResults(new[] {
        new { StNr = 2U, Position = 1U, Time = (TimeSpan?)new TimeSpan(0, 1, 0), Code = RunResult.EResultCode.Normal },
        new { StNr = 1U, Position = 2U, Time = (TimeSpan?)new TimeSpan(0, 1, 1), Code = RunResult.EResultCode.Normal },
        new { StNr = 3U, Position = 0U, Time = (TimeSpan?)null, Code = RunResult.EResultCode.NotSet },
      });
      rr.SetResultCode(race.GetParticipant(3), RunResult.EResultCode.NiZ);
      checkResults(new[] {
        new { StNr = 2U, Position = 1U, Time = (TimeSpan?)new TimeSpan(0, 1, 0), Code = RunResult.EResultCode.Normal },
        new { StNr = 1U, Position = 2U, Time = (TimeSpan?)new TimeSpan(0, 1, 1), Code = RunResult.EResultCode.Normal },
        new { StNr = 3U, Position = 3U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },
        new { StNr = 4U, Position = 0U, Time = (TimeSpan?)null, Code = RunResult.EResultCode.NotSet },
      });
      rr.SetStartFinishTime(race.GetParticipant(4), new TimeSpan(8, 10, 0), new TimeSpan(8, 12, 0));  // 2:00,00
      checkResults(new[] {
        new { StNr = 2U, Position = 1U, Time = (TimeSpan?)new TimeSpan(0, 1, 0), Code = RunResult.EResultCode.Normal },
        new { StNr = 1U, Position = 2U, Time = (TimeSpan?)new TimeSpan(0, 1, 1), Code = RunResult.EResultCode.Normal },
        new { StNr = 3U, Position = 3U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },
        new { StNr = 4U, Position = 3U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },
        new { StNr = 5U, Position = 0U, Time = (TimeSpan?)null, Code = RunResult.EResultCode.NotSet },
      });
      rr.SetStartFinishTime(race.GetParticipant(5), new TimeSpan(8, 15, 0), new TimeSpan(8, 15, 30));  // 0:30 - Test shrink CutOff Time
      checkResults(new[] {
        new { StNr = 5U, Position = 1U, Time = (TimeSpan?)TimeSpan.FromSeconds(30), Code = RunResult.EResultCode.Normal },
        new { StNr = 1U, Position = 2U, Time = (TimeSpan?)TimeSpan.FromSeconds(39), Code = RunResult.EResultCode.Normal },
        new { StNr = 2U, Position = 2U, Time = (TimeSpan?)TimeSpan.FromSeconds(39), Code = RunResult.EResultCode.Normal },
        new { StNr = 3U, Position = 2U, Time = (TimeSpan?)TimeSpan.FromSeconds(39), Code = RunResult.EResultCode.Normal },
        new { StNr = 4U, Position = 2U, Time = (TimeSpan?)TimeSpan.FromSeconds(39), Code = RunResult.EResultCode.Normal },
        new { StNr = 6U, Position = 0U, Time = (TimeSpan?)null, Code = RunResult.EResultCode.NotSet },
      });
      rr.SetStartFinishTime(race.GetParticipant(5), new TimeSpan(8, 15, 0), new TimeSpan(8, 16, 3));  // 1:03 - Test grow CutOff Time
      checkResults(new[] {
        new { StNr = 2U, Position = 1U, Time = (TimeSpan?)new TimeSpan(0, 1, 0), Code = RunResult.EResultCode.Normal },
        new { StNr = 1U, Position = 2U, Time = (TimeSpan?)new TimeSpan(0, 1, 1), Code = RunResult.EResultCode.Normal },
        new { StNr = 5U, Position = 3U, Time = (TimeSpan?)new TimeSpan(0, 1, 3), Code = RunResult.EResultCode.Normal },
        new { StNr = 3U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },
        new { StNr = 4U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },
        new { StNr = 6U, Position = 0U, Time = (TimeSpan?)null, Code = RunResult.EResultCode.NotSet },
      });

      rr.SetStartFinishTime(race.GetParticipant(7), new TimeSpan(8, 0, 0), new TimeSpan(8, 2, 1));  // 2:01,00
      rr.SetStartFinishTime(race.GetParticipant(8), new TimeSpan(8, 1, 0), new TimeSpan(8, 3, 0));  // 2:00,00
      rr.SetResultCode(race.GetParticipant(9), RunResult.EResultCode.NiZ);
      rr.SetStartFinishTime(race.GetParticipant(10), new TimeSpan(8, 10, 0), new TimeSpan(8, 14, 0));  // 4:00,00
      rr.SetStartFinishTime(race.GetParticipant(11), new TimeSpan(8, 15, 0), new TimeSpan(8, 17, 3));  // 2:03
      checkResults(new[] {
        new { StNr = 2U, Position = 1U, Time = (TimeSpan?)new TimeSpan(0, 1, 0), Code = RunResult.EResultCode.Normal },
        new { StNr = 1U, Position = 2U, Time = (TimeSpan?)new TimeSpan(0, 1, 1), Code = RunResult.EResultCode.Normal },
        new { StNr = 5U, Position = 3U, Time = (TimeSpan?)new TimeSpan(0, 1, 3), Code = RunResult.EResultCode.Normal },
        new { StNr = 3U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },
        new { StNr = 4U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },
        new { StNr = 6U, Position = 0U, Time = (TimeSpan?)null, Code = RunResult.EResultCode.NotSet },

        new { StNr = 8U, Position = 1U, Time = (TimeSpan?)new TimeSpan(0, 2, 0), Code = RunResult.EResultCode.Normal },
        new { StNr = 7U, Position = 2U, Time = (TimeSpan?)new TimeSpan(0, 2, 1), Code = RunResult.EResultCode.Normal },
        new { StNr = 11U, Position = 3U, Time = (TimeSpan?)new TimeSpan(0, 2, 3), Code = RunResult.EResultCode.Normal },
        new { StNr = 9U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(156), Code = RunResult.EResultCode.Normal },
        new { StNr = 10U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(156), Code = RunResult.EResultCode.Normal },
        new { StNr = 12U, Position = 0U, Time = (TimeSpan?)null, Code = RunResult.EResultCode.NotSet },
      });
      vp.ChangeGrouping(null);
      checkResults(new[] {
        new { StNr = 2U, Position = 1U, Time = (TimeSpan?)new TimeSpan(0, 1, 0), Code = RunResult.EResultCode.Normal },
        new { StNr = 1U, Position = 2U, Time = (TimeSpan?)new TimeSpan(0, 1, 1), Code = RunResult.EResultCode.Normal },
        new { StNr = 5U, Position = 3U, Time = (TimeSpan?)new TimeSpan(0, 1, 3), Code = RunResult.EResultCode.Normal },
        new { StNr = 3U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },
        new { StNr = 4U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },

        new { StNr = 7U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },
        new { StNr = 8U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },
        new { StNr = 9U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },
        new { StNr = 10U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },
        new { StNr = 11U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },

        new { StNr = 6U, Position = 0U, Time = (TimeSpan?)null, Code = RunResult.EResultCode.NotSet },
        new { StNr = 12U, Position = 0U, Time = (TimeSpan?)null, Code = RunResult.EResultCode.NotSet },
      });

      vp.ChangeGrouping("Participant.Class");
      checkResults(new[] {
        new { StNr = 2U, Position = 1U, Time = (TimeSpan?)new TimeSpan(0, 1, 0), Code = RunResult.EResultCode.Normal },
        new { StNr = 1U, Position = 2U, Time = (TimeSpan?)new TimeSpan(0, 1, 1), Code = RunResult.EResultCode.Normal },
        new { StNr = 5U, Position = 3U, Time = (TimeSpan?)new TimeSpan(0, 1, 3), Code = RunResult.EResultCode.Normal },
        new { StNr = 3U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },
        new { StNr = 4U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(78), Code = RunResult.EResultCode.Normal },
        new { StNr = 6U, Position = 0U, Time = (TimeSpan?)null, Code = RunResult.EResultCode.NotSet },

        new { StNr = 8U, Position = 1U, Time = (TimeSpan?)new TimeSpan(0, 2, 0), Code = RunResult.EResultCode.Normal },
        new { StNr = 7U, Position = 2U, Time = (TimeSpan?)new TimeSpan(0, 2, 1), Code = RunResult.EResultCode.Normal },
        new { StNr = 11U, Position = 3U, Time = (TimeSpan?)new TimeSpan(0, 2, 3), Code = RunResult.EResultCode.Normal },
        new { StNr = 9U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(156), Code = RunResult.EResultCode.Normal },
        new { StNr = 10U, Position = 4U, Time = (TimeSpan?)TimeSpan.FromSeconds(156), Code = RunResult.EResultCode.Normal },
        new { StNr = 12U, Position = 0U, Time = (TimeSpan?)null, Code = RunResult.EResultCode.NotSet },
      });
    }


    /// <summary>
    /// Tests whether the positions are correct in case two or more participants have the same runtime
    /// </summary>
    [TestMethod]
    public void BasedOnResultsFirstRunStartListViewProviderTest_40Participants()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.Model.SetCurrentRace(tg.Model.GetRace(0));
      tg.Model.SetCurrentRaceRun(tg.Model.GetCurrentRace().GetRun(0));
      Race race = tg.Model.GetCurrentRace();
      RaceRun rr = tg.Model.GetCurrentRaceRun();

      var participants = tg.Model.GetRace(0).GetParticipants();

      tg.createRaceParticipants(40); // Create 40 Participants

      for (uint stnr = 1; stnr <= 40; ++stnr)
        rr.SetRunTime(race.GetParticipant(stnr), new TimeSpan(0, 0, 0, 60 - (int)stnr, 0));

      RaceRunResultViewProvider vp = new RaceRunResultViewProvider();
      vp.Init(rr, tg.Model);

      BasedOnResultsFirstRunStartListViewProvider slp = new BasedOnResultsFirstRunStartListViewProvider(20, true);
      slp.Init(rr);

      var ranking = vp.GetView().ViewToList<RunResultWithPosition>();
      var starter = slp.GetView().ViewToList<StartListEntryAdditionalRun>();

      for (int i = 0; i < 40; ++i)
      {
        Assert.AreEqual((uint)(i + 1), ranking[i].Position);
        Assert.AreEqual((uint)(40 - i), ranking[i].StartNumber);
      }

      // Start List
      for (int i = 0; i < 40; ++i)
      {
        if (i < 20)
          Assert.AreEqual((uint)(21 + i), starter[i].StartNumber);
        else
          Assert.AreEqual((uint)(40 - i), starter[i].StartNumber);
      }
    }


    /// <summary>
    /// Tests whether the positions are correct in case two or more participants have the same runtime
    /// Rule 621.11 IWO
    /// </summary>
    [TestMethod]
    public void BasedOnResultsFirstRunStartListViewProviderTest_EqualRankAt30()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.Model.SetCurrentRace(tg.Model.GetRace(0));
      tg.Model.SetCurrentRaceRun(tg.Model.GetCurrentRace().GetRun(0));
      Race race = tg.Model.GetCurrentRace();
      RaceRun rr = tg.Model.GetCurrentRaceRun();

      var participants = tg.Model.GetRace(0).GetParticipants();

      tg.createRaceParticipants(40); // Create 40 Participants

      for (uint stnr = 1; stnr <= 40; ++stnr)
        rr.SetRunTime(race.GetParticipant(stnr), new TimeSpan(0, 0, 0, (int)stnr, 0));

      rr.SetRunTime(race.GetParticipant(30), new TimeSpan(0, 0, 0, 30, 0));
      rr.SetRunTime(race.GetParticipant(31), new TimeSpan(0, 0, 0, 30, 0));

      RaceRunResultViewProvider vp = new RaceRunResultViewProvider();
      vp.Init(rr, tg.Model);

      BasedOnResultsFirstRunStartListViewProvider slp = new BasedOnResultsFirstRunStartListViewProvider(30, true);
      slp.Init(rr);

      var ranking = vp.GetView().ViewToList<RunResultWithPosition>();
      var starter = slp.GetView().ViewToList<StartListEntryAdditionalRun>();

      for (int i = 0; i < 40; ++i)
      {
        if (ranking[i].StartNumber == 30 || ranking[i].StartNumber == 31)
        {
          Assert.AreEqual(30U, ranking[i].Position);
        }
        else
        {
          Assert.AreEqual((uint)(i + 1), ranking[i].Position);
          Assert.AreEqual((uint)(i + 1), ranking[i].StartNumber);
        }
      }

      // Start List
      for (int i = 0; i < 40; ++i)
      {
        // 31, 30, 29 ... 1
        if (i == 0)
          Assert.AreEqual(30U, starter[i].StartNumber);
        else if (i == 1)
          Assert.AreEqual(31U, starter[i].StartNumber);
        else if (i < 31)
          Assert.AreEqual((uint)(31 - i), starter[i].StartNumber);
        else
          Assert.AreEqual((uint)(i + 1), starter[i].StartNumber);
      }
    }



    /// <summary>
    /// Tests whether the positions are correct in case two or more participants have the same runtime
    /// </summary>
    [TestMethod]
    public void BasedOnResultsFirstRunStartListViewProviderTest_SameRunTime()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.Model.SetCurrentRace(tg.Model.GetRace(0));
      tg.Model.SetCurrentRaceRun(tg.Model.GetCurrentRace().GetRun(0));
      Race race = tg.Model.GetCurrentRace();
      RaceRun rr = tg.Model.GetCurrentRaceRun();

      var participants = tg.Model.GetRace(0).GetParticipants();

      tg.createRaceParticipant(); // StNr 1
      tg.createRaceParticipant(); // StNr 2
      tg.createRaceParticipant(); // StNr 3
      tg.createRaceParticipant(); // StNr 4

      rr.SetRunTime(race.GetParticipant(1), new TimeSpan(0, 0, 0, 11, 0));
      rr.SetRunTime(race.GetParticipant(2), new TimeSpan(0, 0, 0, 11, 100));
      rr.SetRunTime(race.GetParticipant(3), new TimeSpan(0, 0, 0, 11, 0));
      rr.SetRunTime(race.GetParticipant(4), new TimeSpan(0, 0, 0, 10, 990));

      RaceRunResultViewProvider vp = new RaceRunResultViewProvider();
      vp.Init(rr, tg.Model);

      BasedOnResultsFirstRunStartListViewProvider slp = new BasedOnResultsFirstRunStartListViewProvider(0, true);
      slp.Init(rr);

      var ranking = vp.GetView().ViewToList<RunResultWithPosition>();
      var starter = slp.GetView().ViewToList<StartListEntryAdditionalRun>();
      Assert.AreEqual(1U, ranking[0].Position);
      Assert.AreEqual(4U, ranking[0].StartNumber);

      Assert.AreEqual(2U, ranking[1].Position);
      Assert.AreEqual(1U, ranking[1].StartNumber);

      Assert.AreEqual(2U, ranking[2].Position);
      Assert.AreEqual(3U, ranking[2].StartNumber);

      Assert.AreEqual(4U, ranking[3].Position);
      Assert.AreEqual(2U, ranking[3].StartNumber);

      // Start List
      Assert.AreEqual(4U, starter[0].StartNumber);
      Assert.AreEqual(3U, starter[1].StartNumber);
      Assert.AreEqual(1U, starter[2].StartNumber);
      Assert.AreEqual(2U, starter[3].StartNumber);
    }


    /// <summary>
    /// Test for TotalTimeSorter
    ///
    /// Compares two RunResults, taking into account:
    /// - Group (Class, Group, Category)
    /// - Runtime
    /// - ResultCode
    /// - StartNumber
    /// </summary>
    [TestMethod]
    public void TotalTimeSorterTest()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();

      tg.Model.SetCurrentRace(tg.Model.GetRace(0));
      tg.Model.SetCurrentRaceRun(tg.Model.GetCurrentRace().GetRun(0));
      Race race = tg.Model.GetCurrentRace();
      RaceRun rr = tg.Model.GetCurrentRaceRun();

      var participants = tg.Model.GetRace(0).GetParticipants();

      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));

      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));

      var rr1 = new RaceResultItem(race.GetParticipant(1));
      rr1.TotalTime = new TimeSpan(0, 1, 0);
      var rr2 = new RaceResultItem(race.GetParticipant(2));
      rr2.TotalTime = new TimeSpan(0, 1, 1);
      var rr3 = new RaceResultItem(race.GetParticipant(3));
      rr3.TotalTime = new TimeSpan(0, 0, 59);
      var rr4 = new RaceResultItem(race.GetParticipant(4));
      rr4.TotalTime = new TimeSpan(0, 1, 0);

      var rr1w = new RaceResultItem(race.GetParticipant(5));
      rr1w.TotalTime = new TimeSpan(0, 1, 0);
      var rr2w = new RaceResultItem(race.GetParticipant(6));
      rr2w.TotalTime = new TimeSpan(0, 1, 1);
      var rr3w = new RaceResultItem(race.GetParticipant(7));
      rr3w.TotalTime = new TimeSpan(0, 0, 59);
      var rr4w = new RaceResultItem(race.GetParticipant(8));
      rr4w.TotalTime = new TimeSpan(0, 1, 0);

      TotalTimeSorter ts = new TotalTimeSorter();

      // Standard order
      Assert.AreEqual(-1, ts.Compare(rr1, rr2));
      Assert.AreEqual(1, ts.Compare(rr2, rr1));

      // ... including transitivity: rr3 < rr1 < rr2 => rr3 < rr2
      Assert.AreEqual(-1, ts.Compare(rr3, rr1));
      Assert.AreEqual(-1, ts.Compare(rr1, rr2));
      Assert.AreEqual(-1, ts.Compare(rr3, rr2));

      // Equality (same time, same startnumber)
      Assert.AreEqual(0, ts.Compare(rr1, rr1));

      // Same time, different startnumber
      Assert.AreEqual(rr1.TotalTime, rr4.TotalTime);
      Assert.AreEqual(-1, ts.Compare(rr1, rr4));

      // Grouping
      Assert.AreEqual(-1, ts.Compare(rr3w, rr1));
      ts.SetGrouping("Participant.Class");
      Assert.AreEqual(1, ts.Compare(rr3w, rr1));


      // No time, same startnumber
      rr1.TotalTime = null;
      Assert.IsNull(rr1.TotalTime);
      Assert.AreEqual(0, ts.Compare(rr1, rr1));

      // No time, different startnumber
      rr2.TotalTime = null;
      Assert.IsNull(rr1.TotalTime);
      Assert.IsNull(rr2.TotalTime);
      Assert.AreEqual(-1, ts.Compare(rr1, rr2));
    }


    /// <summary>
    /// Test for
    /// - RaceResultViewProvider.MinimumTime
    /// - RaceResultViewProvider.SumTime
    ///
    /// What it does:
    /// - Combines different scenarios of run times
    /// </summary>
    [TestMethod]
    public void RaceResultViewProviderTest_CombineTime()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();
      tg.Model.SetCurrentRace(tg.Model.GetRace(0));
      tg.Model.SetCurrentRaceRun(tg.Model.GetCurrentRace().GetRun(0));
      Race race = tg.Model.GetCurrentRace();
      RaceRun rr = tg.Model.GetCurrentRaceRun();

      var participants = tg.Model.GetRace(0).GetParticipants();

      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));

      var rr1m00 = new RunResultWithPosition(tg.createRunResult(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0)));
      var rr0m59 = new RunResultWithPosition(tg.createRunResult(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 59)));
      var rr1m01 = new RunResultWithPosition(tg.createRunResult(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 1)));

      var rr1m00NIZ = new RunResultWithPosition(tg.createRunResult(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0)));
      rr1m00NIZ.ResultCode = RunResult.EResultCode.NiZ;
      var rr1m00NAS = new RunResultWithPosition(tg.createRunResult(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0)));
      rr1m00NAS.ResultCode = RunResult.EResultCode.NaS;
      var rr1m00DIS1 = new RunResultWithPosition(tg.createRunResult(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0)));
      rr1m00DIS1.ResultCode = RunResult.EResultCode.DIS;
      rr1m00DIS1.DisqualText = "Tor 1";
      var rr1m00DIS2 = new RunResultWithPosition(tg.createRunResult(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 1, 0)));
      rr1m00DIS2.ResultCode = RunResult.EResultCode.DIS;
      rr1m00DIS2.DisqualText = "Tor 2";


      Dictionary<uint, RunResultWithPosition> results = new Dictionary<uint, RunResultWithPosition>();
      RunResult.EResultCode resCode = RunResult.EResultCode.NotSet;
      string disqualText = string.Empty;

      {
        results.Clear();
        results.Add(1, rr1m00);
        results.Add(2, rr0m59);
        {
          var tM = RaceResultViewProvider.MinimumTime(results.Values, out resCode, out disqualText);
          Assert.AreEqual(new TimeSpan(0, 0, 59), tM);

          var tS = RaceResultViewProvider.SumTime(results.Values, out resCode, out disqualText);
          Assert.AreEqual(new TimeSpan(0, 1, 59), tS);
        }
        results.Add(3, rr1m01);
        {
          var tM = RaceResultViewProvider.MinimumTime(results.Values, out resCode, out disqualText);
          Assert.AreEqual(new TimeSpan(0, 0, 59), tM);

          var tS = RaceResultViewProvider.SumTime(results.Values, out resCode, out disqualText);
          Assert.AreEqual(new TimeSpan(0, 3, 00), tS);
        }
      }

      {
        results.Clear();
        results.Add(1, rr1m00);
        results.Add(2, rr1m00NIZ);

        var tM = RaceResultViewProvider.MinimumTime(results.Values, out resCode, out disqualText);
        Assert.AreEqual(new TimeSpan(0, 1, 00), tM);

        var tS = RaceResultViewProvider.SumTime(results.Values, out resCode, out disqualText);
        Assert.AreEqual(null, tS);
        Assert.AreEqual(RunResult.EResultCode.NiZ, resCode);
        Assert.IsTrue(string.IsNullOrEmpty(disqualText));
      }

      {
        results.Clear();
        results.Add(1, rr1m00NAS);
        results.Add(2, rr1m00NIZ);

        var tM = RaceResultViewProvider.MinimumTime(results.Values, out resCode, out disqualText);
        Assert.AreEqual(null, tM);

        var tS = RaceResultViewProvider.SumTime(results.Values, out resCode, out disqualText);
        Assert.AreEqual(null, tS);
        Assert.AreEqual(RunResult.EResultCode.NaS, resCode);  // First run rules
        Assert.IsTrue(string.IsNullOrEmpty(disqualText));
      }

      {
        results.Clear();
        results.Add(1, rr1m00);
        results.Add(2, rr0m59);
        results.Add(3, rr1m00NIZ);

        var tM = RaceResultViewProvider.MinimumTime(results.Values, out resCode, out disqualText);
        Assert.AreEqual(new TimeSpan(0, 0, 59), tM);

        var tS = RaceResultViewProvider.SumTime(results.Values, out resCode, out disqualText);
        Assert.AreEqual(null, tS);
        Assert.AreEqual(RunResult.EResultCode.NiZ, resCode);
        Assert.IsTrue(string.IsNullOrEmpty(disqualText));
      }

      {
        results.Clear();
        results.Add(1, rr1m00);
        results.Add(2, rr1m00DIS1);
        results.Add(3, rr1m01);

        var tM = RaceResultViewProvider.MinimumTime(results.Values, out resCode, out disqualText);
        Assert.AreEqual(new TimeSpan(0, 1, 0), tM);

        var tS = RaceResultViewProvider.SumTime(results.Values, out resCode, out disqualText);
        Assert.AreEqual(null, tS);
        Assert.AreEqual(RunResult.EResultCode.DIS, resCode);
        Assert.AreEqual("Tor 1", disqualText);
      }

      {
        results.Clear();
        results.Add(1, rr1m00DIS2);
        results.Add(2, rr1m00DIS1);

        var tM = RaceResultViewProvider.MinimumTime(results.Values, out resCode, out disqualText);
        Assert.AreEqual(null, tM);
        Assert.AreEqual(RunResult.EResultCode.DIS, resCode);
        Assert.AreEqual("Tor 2, Tor 1", disqualText);

        var tS = RaceResultViewProvider.SumTime(results.Values, out resCode, out disqualText);
        Assert.AreEqual(null, tS);
        Assert.AreEqual(RunResult.EResultCode.DIS, resCode);
        Assert.AreEqual("Tor 2", disqualText);
      }

    }


    /// <summary>
    /// Test for RaceResultViewProvider.SumTimeMxBestofN ("2x Best of 2"):
    /// best of runs 1+2 plus best of runs 3+4 (fix taken from PR #690)
    /// </summary>
    [TestMethod]
    public void RaceResultViewProviderTest_SumTimeMxBestofN()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.createRaceParticipant();
      Race race = tg.Model.GetRace(0);

      RunResultWithPosition result(int seconds)
        => new RunResultWithPosition(tg.createRunResult(race.GetParticipant(1), new TimeSpan(8, 0, 0), new TimeSpan(8, 0, 0) + TimeSpan.FromSeconds(seconds)));

      TimeSpan? sum(params int[] runTimes)
        => RaceResultViewProvider.SumTimeMxBestofN(System.Linq.Enumerable.Select(runTimes, result), out _, out _);

      // Runs 1..4: 40, 50 | 70, 65 => 40 + 65
      Assert.AreEqual(TimeSpan.FromSeconds(105), sum(40, 50, 70, 65));
      // Runs 1..4: 40, 50 | 60, 70 => 40 + 60
      Assert.AreEqual(TimeSpan.FromSeconds(100), sum(40, 50, 60, 70));
      // Runs 1..4: 50, 40 | 70, 60 => 40 + 60
      Assert.AreEqual(TimeSpan.FromSeconds(100), sum(50, 40, 70, 60));

      // Interim results: only runs with results are passed in
      // Runs 1..3: 50, 40 | 70 => 40 + 70
      Assert.AreEqual(TimeSpan.FromSeconds(110), sum(50, 40, 70));
      // Runs 1..2: 50, 40 => 40
      Assert.AreEqual(TimeSpan.FromSeconds(40), sum(50, 40));
      // Runs 1..2: 40, 50 => 40
      Assert.AreEqual(TimeSpan.FromSeconds(40), sum(40, 50));
    }

    /// <summary>
    /// Test for RaceResultViewProvider
    ///
    /// What it does:
    /// - Checks the RunResultWithPosition of RaceRunResultViewProvider
    /// - Based on simulated race data
    /// - Check correct handling of changing participant as well as RunResult
    /// - Checks DeleteRunResult
    /// </summary>
    [TestMethod]
    public void RaceResultViewProviderTest_Dynamic()
    {
      int i = 0;
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();

      tg.Model.SetCurrentRace(tg.Model.GetRace(0));
      tg.Model.SetCurrentRaceRun(tg.Model.GetCurrentRace().GetRun(0));
      Race race = tg.Model.GetCurrentRace();
      RaceRun rr1 = tg.Model.GetCurrentRace().GetRun(0);
      RaceRun rr2 = tg.Model.GetCurrentRace().GetRun(1);

      var participants = tg.Model.GetRace(0).GetParticipants();

      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));

      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));

      // Setup some initial values
      rr1.SetRunTime(race.GetParticipant(1), new TimeSpan(0, 1, 2));
      rr1.SetRunTime(race.GetParticipant(2), new TimeSpan(0, 1, 0));

      rr2.SetRunTime(race.GetParticipant(1), new TimeSpan(0, 0, 57)); // 1:59; 0:57
      rr2.SetRunTime(race.GetParticipant(2), new TimeSpan(0, 0, 58)); // 1:58; 0:58

      RaceResultViewProvider vpB = new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.BestRun);
      vpB.ChangeGrouping(null);
      //vpB.ChangeGrouping("Participant.Class");
      vpB.Init(race, tg.Model);
      RaceResultViewProvider vpS = new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.Sum);
      //vpS.ChangeGrouping("Participant.Class");
      vpS.ChangeGrouping(null);
      vpS.Init(race, tg.Model);

      Assert.AreEqual(4, vpB.GetView().ViewToList<RaceResultItem>().Count);
      Assert.AreEqual("Name 1", vpB.GetView().ViewToList<RaceResultItem>()[i = 0].Participant.Name);
      Assert.AreEqual(1U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 57), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 2", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(2U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 58), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 1), vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 3", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 4", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);

      Assert.AreEqual(4, vpS.GetView().ViewToList<RaceResultItem>().Count);
      Assert.AreEqual("Name 2", vpS.GetView().ViewToList<RaceResultItem>()[i = 0].Participant.Name);
      Assert.AreEqual(1U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 1, 58), vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 1", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(2U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 1, 59), vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 1), vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 3", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 4", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);

      // Start Nr3 Run1
      rr1.SetRunTime(race.GetParticipant(3), new TimeSpan(0, 0, 56));

      Assert.AreEqual(4, vpB.GetView().ViewToList<RaceResultItem>().Count);
      Assert.AreEqual("Name 3", vpB.GetView().ViewToList<RaceResultItem>()[i = 0].Participant.Name);
      Assert.AreEqual(1U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 56), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 1", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(2U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 57), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 1), vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 2", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(3U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 58), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 2), vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 4", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);

      Assert.AreEqual(4, vpS.GetView().ViewToList<RaceResultItem>().Count);
      Assert.AreEqual("Name 2", vpS.GetView().ViewToList<RaceResultItem>()[i = 0].Participant.Name);
      Assert.AreEqual(1U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 1, 58), vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 1", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(2U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 1, 59), vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 1), vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 3", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 4", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);

      // Start Nr3 Run2
      rr2.SetRunTime(race.GetParticipant(3), new TimeSpan(0, 1, 10)); // 2:06

      Assert.AreEqual(4, vpB.GetView().ViewToList<RaceResultItem>().Count);
      Assert.AreEqual("Name 3", vpB.GetView().ViewToList<RaceResultItem>()[i = 0].Participant.Name);
      Assert.AreEqual(1U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 56), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 1", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(2U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 57), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 1), vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 2", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(3U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 58), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 2), vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 4", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);

      Assert.AreEqual(4, vpS.GetView().ViewToList<RaceResultItem>().Count);
      Assert.AreEqual("Name 2", vpS.GetView().ViewToList<RaceResultItem>()[i = 0].Participant.Name);
      Assert.AreEqual(1U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 1, 58), vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 1", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(2U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 1, 59), vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 1), vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 3", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(3U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 2, 6), vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 8), vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 4", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);

      rr1.SetResultCode(race.GetParticipant(1), RunResult.EResultCode.DIS, "Tor 1");

      Assert.AreEqual(4, vpB.GetView().ViewToList<RaceResultItem>().Count);
      Assert.AreEqual("Name 3", vpB.GetView().ViewToList<RaceResultItem>()[i = 0].Participant.Name);
      Assert.AreEqual(1U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 56), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 1", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(2U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 57), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 1), vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 2", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(3U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 58), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 2), vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 4", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);

      Assert.AreEqual(4, vpS.GetView().ViewToList<RaceResultItem>().Count);
      Assert.AreEqual("Name 2", vpS.GetView().ViewToList<RaceResultItem>()[i = 0].Participant.Name);
      Assert.AreEqual(1U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 1, 58), vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 3", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(2U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 2, 6), vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 8), vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 1", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 4", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);

      vpB.ChangeGrouping("Participant.Class");
      Assert.AreEqual(4, vpB.GetView().ViewToList<RaceResultItem>().Count);
      Assert.AreEqual("Name 1", vpB.GetView().ViewToList<RaceResultItem>()[i=0].Participant.Name);
      Assert.AreEqual(1U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 57), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 2", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(2U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 58), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 1), vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 3", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(1U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 56), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 4", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);

      rr1.DeleteRunResults();
      Assert.AreEqual(4, vpB.GetView().ViewToList<RaceResultItem>().Count);
      Assert.AreEqual("Name 1", vpB.GetView().ViewToList<RaceResultItem>()[i = 0].Participant.Name);
      Assert.AreEqual(1U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 57), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 2", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(2U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 58), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 1), vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 3", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(1U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 1, 10), vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 4", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);

      // If all particpants do not have a result in run 2, run 2 will be ignored, resulting in order of run 1 only
      Assert.AreEqual(4, vpS.GetView().ViewToList<RaceResultItem>().Count);
      Assert.AreEqual("Name 1", vpS.GetView().ViewToList<RaceResultItem>()[i = 0].Participant.Name);
      Assert.AreEqual(1U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 57), vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 2", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(2U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 0, 58), vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 1), vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 3", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(3U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(new TimeSpan(0, 1, 10), vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(new TimeSpan(0, 0, 13), vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 4", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);

      rr2.DeleteRunResults();
      Assert.AreEqual("Name 1", vpB.GetView().ViewToList<RaceResultItem>()[i = 0].Participant.Name);
      Assert.AreEqual(0U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 2", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 3", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 4", vpB.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpB.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpB.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);

      Assert.AreEqual("Name 1", vpS.GetView().ViewToList<RaceResultItem>()[i = 0].Participant.Name);
      Assert.AreEqual(0U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 2", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 3", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 4", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);

      // Delete Participant
      tg.Model.GetParticipants().RemoveAt(0);
      Assert.AreEqual("Name 2", vpS.GetView().ViewToList<RaceResultItem>()[i=0].Participant.Name);
      Assert.AreEqual(0U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 3", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
      Assert.AreEqual("Name 4", vpS.GetView().ViewToList<RaceResultItem>()[++i].Participant.Name);
      Assert.AreEqual(0U, vpS.GetView().ViewToList<RaceResultItem>()[i].Position);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].TotalTime);
      Assert.AreEqual(null, vpS.GetView().ViewToList<RaceResultItem>()[i].DiffToFirst);
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    public void RaceResultViewProviderTest_Integration_Issue48()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);

      var vpRace = model.GetRace(0).GetTotalResultView();
      var vpRun1 = model.GetRace(0).GetRun(0).GetResultView();
      var vpRun2 = model.GetRace(0).GetRun(1).GetResultView();
      foreach (var rr in model.GetRace(0).GetRuns())
        rr.DeleteRunResults();


      for (int i = 0; i < model.GetParticipants().Count; i++)
      {
        Assert.AreEqual(null, vpRun1.ViewToList<RunResultWithPosition>()[i].Runtime);

        Assert.AreEqual(null, vpRun2.ViewToList<RunResultWithPosition>()[i].Runtime);

        Assert.AreEqual(0U, vpRace.ViewToList<RaceResultItem>()[i].Position);
        Assert.AreEqual(null, vpRace.ViewToList<RaceResultItem>()[i].TotalTime);
        Assert.AreEqual(null, vpRace.ViewToList<RaceResultItem>()[i].DiffToFirst);

        foreach (var sr in vpRace.ViewToList<RaceResultItem>()[i].SubResults)
        {
          Assert.AreEqual(0U, sr.Value.Position);
          Assert.AreEqual(null, sr.Value.Runtime);
          //Assert.AreEqual(null, sr.Value.DiffToFirst);
        }
      }
    }



    [TestMethod]
    public void PointsViaTableRaceResultViewProvider_Test()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();

      tg.Model.SetCurrentRace(tg.Model.GetRace(0));
      tg.Model.SetCurrentRaceRun(tg.Model.GetCurrentRace().GetRun(0));
      Race race = tg.Model.GetCurrentRace();
      RaceRun rr1 = tg.Model.GetCurrentRace().GetRun(0);
      RaceRun rr2 = tg.Model.GetCurrentRace().GetRun(1);

      var participants = tg.Model.GetRace(0).GetParticipants();

      // Create 20 participants
      for(int i=0; i<20; i++)
      {
        var rp = tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));

        rr1.SetRunTime(rp, new TimeSpan(0, 1, 20-i));
        rr2.SetRunTime(rp, new TimeSpan(0, 1, 20-i));
      }


      // Test Case 1: Default table
      System.IO.File.Delete("PointsTable.txt");
      PointsViaTableRaceResultViewProvider vpA = new PointsViaTableRaceResultViewProvider(PointsViaTableRaceResultViewProvider.EMode.ApplyPointsTotally);
      vpA.Init(race, race.GetDataModel());

      Assert.AreEqual(15.0, vpA.GetView().ViewToList<RaceResultItem>()[0].Points);
      Assert.AreEqual(1U, vpA.GetView().ViewToList<RaceResultItem>()[0].Position);
      Assert.AreEqual(12.0, vpA.GetView().ViewToList<RaceResultItem>()[1].Points);
      Assert.AreEqual(2U, vpA.GetView().ViewToList<RaceResultItem>()[1].Position);
      Assert.AreEqual(10.0, vpA.GetView().ViewToList<RaceResultItem>()[2].Points);
      Assert.AreEqual(3U, vpA.GetView().ViewToList<RaceResultItem>()[2].Position);
      Assert.AreEqual(8.0, vpA.GetView().ViewToList<RaceResultItem>()[3].Points);
      Assert.AreEqual(4U, vpA.GetView().ViewToList<RaceResultItem>()[3].Position);
      Assert.AreEqual(6.0, vpA.GetView().ViewToList<RaceResultItem>()[4].Points);
      Assert.AreEqual(5U, vpA.GetView().ViewToList<RaceResultItem>()[4].Position);
      Assert.AreEqual(5.0, vpA.GetView().ViewToList<RaceResultItem>()[5].Points);
      Assert.AreEqual(6U, vpA.GetView().ViewToList<RaceResultItem>()[5].Position);
      Assert.AreEqual(4.0, vpA.GetView().ViewToList<RaceResultItem>()[6].Points);
      Assert.AreEqual(7U, vpA.GetView().ViewToList<RaceResultItem>()[6].Position);
      Assert.AreEqual(3.0, vpA.GetView().ViewToList<RaceResultItem>()[7].Points);
      Assert.AreEqual(8U, vpA.GetView().ViewToList<RaceResultItem>()[7].Position);
      Assert.AreEqual(2.0, vpA.GetView().ViewToList<RaceResultItem>()[8].Points);
      Assert.AreEqual(9U, vpA.GetView().ViewToList<RaceResultItem>()[8].Position);
      Assert.AreEqual(1.0, vpA.GetView().ViewToList<RaceResultItem>()[9].Points);
      Assert.AreEqual(10U, vpA.GetView().ViewToList<RaceResultItem>()[9].Position);
      Assert.AreEqual(.0, vpA.GetView().ViewToList<RaceResultItem>()[10].Points);
      Assert.AreEqual(11U, vpA.GetView().ViewToList<RaceResultItem>()[10].Position);


      // Test Case 2: Points File existing
      System.IO.File.WriteAllText(@"PointsTable.txt",
@"200.0
100.0
50.1
");

      vpA = new PointsViaTableRaceResultViewProvider(PointsViaTableRaceResultViewProvider.EMode.ApplyPointsTotally);
      vpA.Init(race, race.GetDataModel());

      Assert.AreEqual(200.0, vpA.GetView().ViewToList<RaceResultItem>()[0].Points);
      Assert.AreEqual(1U, vpA.GetView().ViewToList<RaceResultItem>()[0].Position);
      Assert.AreEqual(100.0, vpA.GetView().ViewToList<RaceResultItem>()[1].Points);
      Assert.AreEqual(2U, vpA.GetView().ViewToList<RaceResultItem>()[1].Position);
      Assert.AreEqual(50.1, vpA.GetView().ViewToList<RaceResultItem>()[2].Points);
      Assert.AreEqual(3U, vpA.GetView().ViewToList<RaceResultItem>()[2].Position);
      Assert.AreEqual(.0, vpA.GetView().ViewToList<RaceResultItem>()[3].Points);
      Assert.AreEqual(4U, vpA.GetView().ViewToList<RaceResultItem>()[3].Position);
      Assert.AreEqual(.0, vpA.GetView().ViewToList<RaceResultItem>()[4].Points);
      Assert.AreEqual(5U, vpA.GetView().ViewToList<RaceResultItem>()[4].Position);
      Assert.AreEqual(.0, vpA.GetView().ViewToList<RaceResultItem>()[5].Points);
      Assert.AreEqual(6U, vpA.GetView().ViewToList<RaceResultItem>()[5].Position);
      Assert.AreEqual(.0, vpA.GetView().ViewToList<RaceResultItem>()[6].Points);
      Assert.AreEqual(7U, vpA.GetView().ViewToList<RaceResultItem>()[6].Position);
      Assert.AreEqual(.0, vpA.GetView().ViewToList<RaceResultItem>()[7].Points);
      Assert.AreEqual(8U, vpA.GetView().ViewToList<RaceResultItem>()[7].Position);
      Assert.AreEqual(.0, vpA.GetView().ViewToList<RaceResultItem>()[8].Points);
      Assert.AreEqual(9U, vpA.GetView().ViewToList<RaceResultItem>()[8].Position);
      Assert.AreEqual(.0, vpA.GetView().ViewToList<RaceResultItem>()[9].Points);
      Assert.AreEqual(10U, vpA.GetView().ViewToList<RaceResultItem>()[9].Position);
      Assert.AreEqual(.0, vpA.GetView().ViewToList<RaceResultItem>()[10].Points);
      Assert.AreEqual(11U, vpA.GetView().ViewToList<RaceResultItem>()[10].Position);


      System.IO.File.Delete("PointsTable.txt");
      vpA = new PointsViaTableRaceResultViewProvider(PointsViaTableRaceResultViewProvider.EMode.ApplyPointsPerRun);
      vpA.Init(race, race.GetDataModel());

      Assert.AreEqual(30.0, vpA.GetView().ViewToList<RaceResultItem>()[0].Points);
      Assert.AreEqual(1U, vpA.GetView().ViewToList<RaceResultItem>()[0].Position);
      Assert.AreEqual(24.0, vpA.GetView().ViewToList<RaceResultItem>()[1].Points);
      Assert.AreEqual(2U, vpA.GetView().ViewToList<RaceResultItem>()[1].Position);
      Assert.AreEqual(20.0, vpA.GetView().ViewToList<RaceResultItem>()[2].Points);
      Assert.AreEqual(3U, vpA.GetView().ViewToList<RaceResultItem>()[2].Position);
      Assert.AreEqual(16.0, vpA.GetView().ViewToList<RaceResultItem>()[3].Points);
      Assert.AreEqual(4U, vpA.GetView().ViewToList<RaceResultItem>()[3].Position);
      Assert.AreEqual(12.0, vpA.GetView().ViewToList<RaceResultItem>()[4].Points);
      Assert.AreEqual(5U, vpA.GetView().ViewToList<RaceResultItem>()[4].Position);
      Assert.AreEqual(10.0, vpA.GetView().ViewToList<RaceResultItem>()[5].Points);
      Assert.AreEqual(6U, vpA.GetView().ViewToList<RaceResultItem>()[5].Position);
      Assert.AreEqual(8.0, vpA.GetView().ViewToList<RaceResultItem>()[6].Points);
      Assert.AreEqual(7U, vpA.GetView().ViewToList<RaceResultItem>()[6].Position);
      Assert.AreEqual(6.0, vpA.GetView().ViewToList<RaceResultItem>()[7].Points);
      Assert.AreEqual(8U, vpA.GetView().ViewToList<RaceResultItem>()[7].Position);
      Assert.AreEqual(4.0, vpA.GetView().ViewToList<RaceResultItem>()[8].Points);
      Assert.AreEqual(9U, vpA.GetView().ViewToList<RaceResultItem>()[8].Position);
      Assert.AreEqual(2.0, vpA.GetView().ViewToList<RaceResultItem>()[9].Points);
      Assert.AreEqual(10U, vpA.GetView().ViewToList<RaceResultItem>()[9].Position);
      Assert.AreEqual(.0, vpA.GetView().ViewToList<RaceResultItem>()[10].Points);
      Assert.AreEqual(11U, vpA.GetView().ViewToList<RaceResultItem>()[10].Position);


    }


    /// <summary>
    /// A disposed view provider does not observe its sources anymore (no calculation, no memory leak via event handlers)
    /// </summary>
    [TestMethod]
    public void ViewProviders_Dispose_StopObserving()
    {
      var tg = new TestDataGenerator();
      var race = tg.Model.GetRace(0);
      while (race.GetMaxRun() < 2)
        race.AddRaceRun();
      for (int i = 0; i < 3; i++)
        tg.createRaceParticipant();

      var run1 = race.GetRun(0);
      run1.SetRunTime(race.GetParticipant(1), TimeSpan.FromSeconds(50));

      List<ViewProvider> createProviders()
      {
        var firstRun = new FirstRunStartListViewProvider();
        firstRun.Init(race.GetParticipants());
        var secondRunAll = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Ascending, true);
        secondRunAll.Init(run1);
        var secondRunFiltered = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Ascending, false);
        secondRunFiltered.Init(run1);
        var basedOnPrevious = new BasedOnResultsFirstRunStartListViewProvider(2, true);
        basedOnPrevious.Init(run1);
        var basedOnSum = new BasedOnResultsFirstRunStartListViewProvider(2, true, BasedOnResultsFirstRunStartListViewProvider.EBasedOn.SumOfPreviousRuns);
        basedOnSum.Init(run1);
        var runResults = new RaceRunResultViewProvider();
        runResults.Init(run1, tg.Model);
        var raceResults = new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.Sum);
        raceResults.Init(race, tg.Model);
        var teamResults = new TeamRaceResultViewProvider(new TeamRaceResultConfig { Modus = PointOrTime.Time, NumberOfMembersMax = 3 });
        teamResults.Init(race, tg.Model);
        return new List<ViewProvider> { firstRun, secondRunAll, secondRunFiltered, basedOnPrevious, basedOnSum, runResults, raceResults, teamResults };
      }

      // Counts the changes of the provider's view list
      int[] observeChanges(List<ViewProvider> providers)
      {
        var counts = new int[providers.Count];
        for (int i = 0; i < providers.Count; i++)
        {
          int index = i;
          ((System.Collections.Specialized.INotifyCollectionChanged)providers[i].GetView().SourceCollection).CollectionChanged += (s, e) => counts[index]++;
        }
        return counts;
      }

      var active = createProviders();
      var disposed = createProviders();
      disposed.ForEach(p => p.Dispose());
      disposed.ForEach(p => p.Dispose()); // Disposing twice is fine

      int[] activeChanges = observeChanges(active);
      int[] disposedChanges = observeChanges(disposed);

      tg.createRaceParticipant();
      run1.SetRunTime(race.GetParticipant(2), TimeSpan.FromSeconds(49));

      for (int i = 0; i < active.Count; i++)
      {
        Assert.IsTrue(activeChanges[i] > 0, "active " + active[i].GetType().Name);
        Assert.AreEqual(0, disposedChanges[i], "disposed " + disposed[i].GetType().Name);
      }
      Assert.AreEqual(3, disposed[0].GetView().ViewToList<StartListEntry>().Count, "disposed provider keeps its last state");
    }


    /// <summary>
    /// RemainingStartListViewProvider stops observing on Dispose() and disposes its source provider only if requested
    /// </summary>
    [TestMethod]
    public void RemainingStartListViewProvider_Dispose()
    {
      var tg = new TestDataGenerator();
      var race = tg.Model.GetRace(0);
      var run1 = race.GetRun(0);
      for (int i = 0; i < 3; i++)
        tg.createRaceParticipant();

      var sharedStartList = new FirstRunStartListViewProvider();
      sharedStartList.Init(race.GetParticipants());
      var notOwning = new RemainingStartListViewProvider();
      notOwning.Init(sharedStartList, run1);

      var ownedStartList = new FirstRunStartListViewProvider();
      ownedStartList.Init(race.GetParticipants());
      var owning = new RemainingStartListViewProvider();
      owning.Init(ownedStartList, run1, disposeStartListProvider: true);

      notOwning.Dispose();
      owning.Dispose();

      tg.createRaceParticipant();
      run1.SetStartTime(race.GetParticipant(1), TimeSpan.FromSeconds(10));

      // Shared source is still alive, owned source has been disposed
      Assert.AreEqual(4, sharedStartList.GetViewList().Count);
      Assert.AreEqual(3, ownedStartList.GetViewList().Count);

      // Remaining lists do not follow the source and the results anymore
      Assert.AreEqual(3, notOwning.GetViewList().Count);
      Assert.AreEqual(3, owning.GetViewList().Count);
      Assert.IsFalse(notOwning.GetViewList()[0].Started);
      Assert.IsFalse(owning.GetViewList()[0].Started);
    }


    /// <summary>
    /// ViewProviderListObserver follows the view provider of a race run, even if it gets replaced (new race configuration)
    /// </summary>
    [TestMethod]
    public void ViewProviderListObserver_FollowsReplacedProvider()
    {
      var tg = new TestDataGenerator();
      var race = tg.Model.GetRace(0);
      var run1 = race.GetRun(0);
      tg.createRaceParticipant();

      var senders = new List<object>();
      var actions = new List<System.Collections.Specialized.NotifyCollectionChangedAction>();
      var observer = ViewProviderListObserver.ForStartList(run1);
      observer.CollectionChanged += (s, e) => { senders.Add(s); actions.Add(e.Action); };

      var oldList = run1.GetStartListProvider().GetViewList();
      tg.createRaceParticipant();
      CollectionAssert.Contains(senders, oldList);

      // Replace the provider => Reset
      var newProvider = new FirstRunStartListViewProvider();
      newProvider.Init(race.GetParticipants());
      senders.Clear(); actions.Clear();
      run1.SetStartListProvider(newProvider);
      CollectionAssert.AreEqual(new[] { System.Collections.Specialized.NotifyCollectionChangedAction.Reset }, actions);

      // Changes of the new list are forwarded, the old list is not observed anymore
      senders.Clear(); actions.Clear();
      tg.createRaceParticipant();
      CollectionAssert.Contains(senders, newProvider.GetViewList());
      CollectionAssert.DoesNotContain(senders, oldList);

      // Results of the race follow as well
      int raceResultChanges = 0;
      var raceObserver = ViewProviderListObserver.ForResults(race);
      raceObserver.CollectionChanged += (s, e) => raceResultChanges++;
      var newRaceResults = new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.BestRun);
      newRaceResults.Init(race, tg.Model);
      race.SetResultViewProvider(newRaceResults);
      Assert.AreEqual(1, raceResultChanges);

      // Disposed observer does not forward anything anymore
      observer.Dispose();
      raceObserver.Dispose();
      senders.Clear(); raceResultChanges = 0;
      tg.createRaceParticipant();
      var anotherProvider = new FirstRunStartListViewProvider();
      anotherProvider.Init(race.GetParticipants());
      run1.SetStartListProvider(anotherProvider);
      Assert.AreEqual(0, senders.Count);
      Assert.AreEqual(0, raceResultChanges);
    }


    /// <summary>
    /// A new race configuration replaces all view providers; the replaced ones get disposed, the new ones work
    /// </summary>
    [TestMethod]
    public void ViewConfigurator_ConfigureRace_DisposesReplacedProviders()
    {
      var tg = new TestDataGenerator();
      var race = tg.Model.GetRace(0);
      while (race.GetMaxRun() < 2)
        race.AddRaceRun();
      (new ViewConfigurator(race)).ConfigureRace(race);
      for (int i = 0; i < 3; i++)
        tg.createRaceParticipant();

      var run1 = race.GetRun(0);
      var run2 = race.GetRun(1);

      var oldProviders = new List<ViewProvider> { run1.GetStartListProvider(), run1.GetResultViewProvider(), run2.GetStartListProvider(), run2.GetResultViewProvider(), race.GetResultViewProvider() };
      int oldChanges = 0;
      foreach (var p in oldProviders)
        ((System.Collections.Specialized.INotifyCollectionChanged)p.GetView().SourceCollection).CollectionChanged += (s, e) => oldChanges++;

      (new ViewConfigurator(race)).ConfigureRace(race);
      foreach (var p in oldProviders)
        CollectionAssert.DoesNotContain(new ViewProvider[] { run1.GetStartListProvider(), run1.GetResultViewProvider(), run2.GetStartListProvider(), run2.GetResultViewProvider(), race.GetResultViewProvider() }, p);

      tg.createRaceParticipant();
      run1.SetRunTime(race.GetParticipant(1), TimeSpan.FromSeconds(50));

      // Old providers do not calculate anymore, the new ones do
      Assert.AreEqual(0, oldChanges);
      Assert.AreEqual(4, run1.GetStartListProvider().GetViewList().Count);
      var raceResults = race.GetResultViewProvider().GetView().ViewToList<RaceResultItem>();
      Assert.AreEqual(4, raceResults.Count);
      Assert.AreEqual(TimeSpan.FromSeconds(50), raceResults[0].TotalTime);

      // Deleting a run disposes its providers
      var run2StartList = run2.GetStartListProvider();
      int run2Changes = 0;
      ((System.Collections.Specialized.INotifyCollectionChanged)run2StartList.GetView().SourceCollection).CollectionChanged += (s, e) => run2Changes++;
      race.DeleteRaceRun();
      tg.createRaceParticipant();
      Assert.AreEqual(0, run2Changes);
    }


    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    static WeakReference CreateUnobservedStartListEntry(RaceParticipant rp)
    {
      return new WeakReference(new StartListEntry(rp));
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    static List<WeakReference> WeakReferencesTo(IEnumerable<StartListEntry> entries)
    {
      var refs = new List<WeakReference>();
      foreach (var e in entries)
        refs.Add(new WeakReference(e));
      return refs;
    }

    static void CollectGarbage()
    {
      GC.Collect();
      GC.WaitForPendingFinalizers();
      GC.Collect();
    }


    /// <summary>
    /// A StartListEntry observes its participant only while it is observed itself.
    /// Otherwise, the participant keeps every entry ever created alive.
    /// </summary>
    [TestMethod]
    public void StartListEntry_ObservesParticipantOnlyWhileObserved()
    {
      var tg = new TestDataGenerator();
      var rp = tg.createRaceParticipant();

      var unobserved = CreateUnobservedStartListEntry(rp);
      CollectGarbage();
      Assert.IsFalse(unobserved.IsAlive);

      // Observed entry passes through the participant's changes
      var entry = new StartListEntry(rp);
      var changed = new List<string>();
      System.ComponentModel.PropertyChangedEventHandler handler = (s, e) => changed.Add(e.PropertyName);
      entry.PropertyChanged += handler;
      rp.Participant.Name = "Changed";
      CollectionAssert.Contains(changed, "Name");

      entry.PropertyChanged -= handler;
      changed.Clear();
      rp.Participant.Name = "Changed again";
      Assert.AreEqual(0, changed.Count);
    }


    /// <summary>
    /// A start list being rebuilt on every change (based on results) does not keep its previous entries alive,
    /// also if observed like by the live timing (ItemsChangedNotifier) and the remaining starters (RemainingStartListViewProvider)
    /// </summary>
    [TestMethod]
    public void BasedOnResultsStartListViewProvider_RebuildReleasesOldEntries()
    {
      var tg = new TestDataGenerator();
      var race = tg.Model.GetRace(0);
      while (race.GetMaxRun() < 2)
        race.AddRaceRun();
      for (int i = 0; i < 3; i++)
        tg.createRaceParticipant();

      var run1 = race.GetRun(0);
      run1.SetRunTime(race.GetParticipant(1), TimeSpan.FromSeconds(50));
      run1.SetRunTime(race.GetParticipant(2), TimeSpan.FromSeconds(51));

      var provider = new BasedOnResultsFirstRunStartListViewProvider(0, true);
      provider.Init(run1);
      provider.ChangeGrouping("Participant.Class");

      var notifier = new ItemsChangedNotifier(provider.GetViewList());
      var itemChangedSenders = new List<object>();
      notifier.ItemChanged += (s, e) => itemChangedSenders.Add(s);
      var remaining = new RemainingStartListViewProvider();
      remaining.Init(provider, race.GetRun(1));

      var oldEntries = WeakReferencesTo(provider.GetViewList());
      Assert.AreEqual(3, oldEntries.Count);

      // Rebuilds the start list
      run1.SetRunTime(race.GetParticipant(3), TimeSpan.FromSeconds(49));
      CollectGarbage();
      foreach (var e in oldEntries)
        Assert.IsFalse(e.IsAlive);

      // The entries created by the rebuild are observed
      var entry1 = System.Linq.Enumerable.First(provider.GetViewList(), x => x.StartNumber == 1);
      itemChangedSenders.Clear();
      entry1.Started = true;
      CollectionAssert.Contains(itemChangedSenders, entry1);

      GC.KeepAlive(notifier);
      GC.KeepAlive(remaining);
    }


    /// <summary>
    /// A new time moves the participant in the result lists with a single Move each,
    /// and the moves within the run results do not trigger any recalculation of the race results
    /// </summary>
    [TestMethod]
    public void ResultViewProviders_NewTime_SingleMove()
    {
      var tg = new TestDataGenerator();
      var race = tg.Model.GetRace(0);
      var run1 = race.GetRun(0);
      for (int i = 0; i < 20; i++)
        tg.createRaceParticipant();
      for (uint i = 1; i < 20; i++)
        run1.SetRunTime(race.GetParticipant(i), TimeSpan.FromSeconds(50 + i));

      var runResults = (run1.GetResultViewProvider() as RaceRunResultViewProvider).GetViewList();
      var raceResults = race.GetResultViewProvider().GetViewList();
      var runEvents = new List<System.Collections.Specialized.NotifyCollectionChangedAction>();
      var raceEvents = new List<System.Collections.Specialized.NotifyCollectionChangedAction>();
      runResults.CollectionChanged += (s, e) => runEvents.Add(e.Action);
      raceResults.CollectionChanged += (s, e) => raceEvents.Add(e.Action);

      // Participant 20 (no time yet, last) becomes the fastest
      run1.SetRunTime(race.GetParticipant(20), TimeSpan.FromSeconds(50));

      CollectionAssert.AreEqual(new[] { System.Collections.Specialized.NotifyCollectionChangedAction.Move }, runEvents);
      CollectionAssert.AreEqual(new[] { System.Collections.Specialized.NotifyCollectionChangedAction.Move }, raceEvents);
      Assert.AreEqual(20U, runResults[0].StartNumber);
      Assert.AreEqual(20U, raceResults[0].Participant.StartNumber);
      Assert.AreEqual(1U, raceResults[0].Position);
      Assert.AreEqual(2U, raceResults[1].Position);
    }


    // Count the recalculations of the race results
    class CountingRaceResultViewProvider : RaceResultViewProvider
    {
      public int Resorts;
      public CountingRaceResultViewProvider() : base(TimeCombination.Sum) { }
      protected override void ResortResults() { Resorts++; base.ResortResults(); }
    }

    class CountingDSVSchoolRaceResultViewProvider : DSVSchoolRaceResultViewProvider
    {
      public int Resorts;
      protected override void ResortResults() { Resorts++; base.ResortResults(); }
    }


    /// <summary>
    /// A new best time changes position and difference to the first of every participant.
    /// The providers depending on the run results must not recalculate for each of these changes.
    /// </summary>
    [TestMethod]
    public void ViewProviders_NewBestTime_NoRecalculationPerPositionChange()
    {
      var tg = new TestDataGenerator();
      var race = tg.Model.GetRace(0);
      while (race.GetMaxRun() < 2)
        race.AddRaceRun();
      (new ViewConfigurator(race)).ConfigureRace(race);
      for (int i = 0; i < 20; i++)
        tg.createRaceParticipant(cat: tg.findCat(i % 2 == 0 ? 'M' : 'W'), points: 10.0 * i);
      var run1 = race.GetRun(0);
      var run2 = race.GetRun(1);
      for (uint i = 1; i < 20; i++)
      {
        run1.SetRunTime(race.GetParticipant(i), TimeSpan.FromSeconds(50 + i));
        run2.SetRunTime(race.GetParticipant(i), TimeSpan.FromSeconds(60 + i));
      }
      run2.SetRunTime(race.GetParticipant(20), TimeSpan.FromSeconds(80));

      var basedOnPrevious = new BasedOnResultsFirstRunStartListViewProvider(0, true);
      basedOnPrevious.Init(run1);
      var basedOnSum = new BasedOnResultsFirstRunStartListViewProvider(0, true, BasedOnResultsFirstRunStartListViewProvider.EBasedOn.SumOfPreviousRuns);
      basedOnSum.Init(run1);
      var secondRunFiltered = new SimpleSecondRunStartListViewProvider(StartListEntryComparer.Direction.Ascending, false);
      secondRunFiltered.Init(run1);
      var raceResults = new CountingRaceResultViewProvider();
      raceResults.Init(race, tg.Model);
      race.RaceConfiguration.ValueF = 720.0; // Factor for DSV points
      var dsvResults = new CountingDSVSchoolRaceResultViewProvider();
      dsvResults.Init(race, tg.Model);
      var teamResults = new TeamRaceResultViewProvider(new TeamRaceResultConfig { Modus = PointOrTime.Time, NumberOfMembersMax = 3 });
      teamResults.Init(race, tg.Model);

      // The start lists and the team results rebuild their lists completely (Reset) when recalculating
      var lists = new Dictionary<string, System.Collections.IEnumerable>
      {
        { "basedOnPrevious", basedOnPrevious.GetViewList() },
        { "basedOnSum", basedOnSum.GetViewList() },
        { "secondRunFiltered", secondRunFiltered.GetViewList() },
        { "teamResults", (System.Collections.IEnumerable)teamResults.GetView().SourceCollection },
      };
      var resets = new Dictionary<string, int>();
      foreach (var l in lists)
        resets[l.Key] = 0;
      foreach (var l in lists)
        ((System.Collections.Specialized.INotifyCollectionChanged)l.Value).CollectionChanged += (s, e) => { if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset) resets[l.Key]++; };
      raceResults.Resorts = 0;
      dsvResults.Resorts = 0;

      // Participant 20 becomes the fastest in run 1 and moves up in the race results (from last to the middle)
      run1.SetRunTime(race.GetParticipant(20), TimeSpan.FromSeconds(50));

      string counts = string.Join(", ", System.Linq.Enumerable.Select(resets, r => r.Key + "=" + r.Value))
        + string.Format(", raceResults={0}, dsvResults={1}", raceResults.Resorts, dsvResults.Resorts);
      foreach (var r in resets)
        Assert.IsTrue(r.Value <= 4, counts); // Was 138 resets for each start list and 130 for the team results
      // A single recalculation (incl. DSV points) for the new time
      Assert.AreEqual(1, raceResults.Resorts, counts);
      Assert.AreEqual(1, dsvResults.Resorts, counts);

      // Still correct: the new best starts first (no reverse)
      Assert.AreEqual(20U, basedOnPrevious.GetViewList()[0].StartNumber);
      Assert.AreEqual(20U, basedOnSum.GetViewList()[0].StartNumber);

      // Incrementally updated race results equal the ones of newly created providers (incl. positions within the runs and DSV points)
      void assertSameAsNew(RaceResultViewProvider incremental, RaceResultViewProvider fresh)
      {
        fresh.Init(race, tg.Model);
        var expected = fresh.GetViewList();
        var actual = incremental.GetViewList();
        Assert.AreEqual(expected.Count, actual.Count);
        for (int i = 0; i < expected.Count; i++)
        {
          Assert.AreSame(expected[i].Participant, actual[i].Participant);
          Assert.AreEqual(expected[i].TotalTime, actual[i].TotalTime);
          Assert.AreEqual(expected[i].Position, actual[i].Position);
          Assert.AreEqual(expected[i].Points, actual[i].Points);
          foreach (var sr in expected[i].SubResults)
          {
            Assert.AreEqual(sr.Value.Position, actual[i].SubResults[sr.Key].Position);
            Assert.AreEqual(sr.Value.DiffToFirst, actual[i].SubResults[sr.Key].DiffToFirst);
          }
        }
      }
      assertSameAsNew(raceResults, new RaceResultViewProvider(RaceResultViewProvider.TimeCombination.Sum));
      assertSameAsNew(dsvResults, new DSVSchoolRaceResultViewProvider());
      Assert.IsTrue(System.Linq.Enumerable.Any(dsvResults.GetViewList(), r => r.Points > 0), "DSV points calculated");

      var rri1 = System.Linq.Enumerable.First(raceResults.GetViewList(), r => r.Participant.StartNumber == 1);
      Assert.AreEqual(2U, rri1.SubResults[1].Position);
      Assert.AreEqual(TimeSpan.FromSeconds(1), rri1.SubResults[1].DiffToFirst);
    }


    /// <summary>
    /// Points per run depend on the position within the run. A new time shifts other participants
    /// in the run, their points have to follow immediately.
    /// </summary>
    [TestMethod]
    public void PointsViaTableRaceResultViewProvider_PerRun_FollowsPositionChanges()
    {
      var tg = new TestDataGenerator();
      var race = tg.Model.GetRace(0);
      while (race.GetMaxRun() < 2)
        race.AddRaceRun();
      (new ViewConfigurator(race)).ConfigureRace(race);
      for (int i = 0; i < 5; i++)
        tg.createRaceParticipant();
      var run1 = race.GetRun(0);
      var run2 = race.GetRun(1);
      for (uint i = 1; i <= 5; i++)
      {
        run1.SetRunTime(race.GetParticipant(i), TimeSpan.FromSeconds(50 + i));
        run2.SetRunTime(race.GetParticipant(i), TimeSpan.FromSeconds(60 + i));
      }

      var vp = new PointsViaTableRaceResultViewProvider(PointsViaTableRaceResultViewProvider.EMode.ApplyPointsPerRun);
      vp.Init(race, tg.Model);

      double pointsOf(PointsViaTableRaceResultViewProvider p, uint startNumber)
        => System.Linq.Enumerable.First(p.GetViewList(), r => r.Participant.StartNumber == startNumber).Points;

      // Participant 1: 1st in both runs => 15 + 15
      Assert.AreEqual(30.0, pointsOf(vp, 1));

      // Participant 5 gets the best time in run 2 (total stays the slowest): all others move down one position in run 2
      run2.SetRunTime(race.GetParticipant(5), TimeSpan.FromSeconds(55));

      var fresh = new PointsViaTableRaceResultViewProvider(PointsViaTableRaceResultViewProvider.EMode.ApplyPointsPerRun);
      fresh.Init(race, tg.Model);
      Assert.AreEqual(27.0, pointsOf(fresh, 1)); // 15 (run 1) + 12 (run 2)
      for (uint sn = 1; sn <= 5; sn++)
        Assert.AreEqual(pointsOf(fresh, sn), pointsOf(vp, sn), string.Format("start number {0}", sn));
    }


    /// <summary>
    /// Start lists and results group by the same properties (one shared group comparison)
    /// </summary>
    [TestMethod]
    public void ParticipantGroupComparer_SameGroupingForStartListsAndResults()
    {
      var tg = new TestDataGenerator();
      var race = tg.Model.GetRace(0);
      var run1 = race.GetRun(0);
      string[] clubs = { "B", "A", "B", "A" };
      for (int i = 0; i < clubs.Length; i++)
        tg.createRaceParticipant().Participant.Club = clubs[i];
      for (uint i = 1; i <= 4; i++)
        run1.SetRunTime(race.GetParticipant(i), TimeSpan.FromSeconds(50 + i));

      var startList = new FirstRunStartListViewProvider();
      startList.Init(race.GetParticipants());
      startList.ChangeGrouping("Participant.Club");
      var results = new RaceRunResultViewProvider();
      results.Init(run1, tg.Model);
      results.ChangeGrouping("Participant.Club");

      CollectionAssert.AreEqual(new uint[] { 2, 4, 1, 3 }, System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(startList.GetViewList(), e => e.StartNumber)));
      CollectionAssert.AreEqual(new uint[] { 2, 4, 1, 3 }, System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(results.GetViewList(), e => e.StartNumber)));

      // Without a group, nulls go last
      Assert.AreEqual(0, ParticipantGroupComparer.Compare(null, race.GetParticipant(1), race.GetParticipant(2)));
      race.GetParticipant(1).Participant.Club = null;
      Assert.IsTrue(ParticipantGroupComparer.Compare("Participant.Club", race.GetParticipant(1), race.GetParticipant(2)) > 0);
    }


    /// <summary>
    /// RemainingStartListViewProvider takes over the active grouping of its source,
    /// but must not overwrite the source's default grouping (e.g. the start list of a run shown in the UI)
    /// </summary>
    [TestMethod]
    public void RemainingStartListViewProvider_KeepsDefaultGroupingOfSource()
    {
      var tg = new TestDataGenerator();
      tg.createCatsClassesGroups();
      var race = tg.Model.GetRace(0);
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"));
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"));

      var source = new FirstRunStartListViewProvider();
      source.SetDefaultGrouping("Participant.Class");
      source.Init(race.GetParticipants());
      source.ChangeGrouping("Participant.Sex");

      var remaining = new RemainingStartListViewProvider();
      remaining.Init(source, race.GetRun(0));
      Assert.AreEqual("Participant.Sex", remaining.ActiveGrouping);
      Assert.AreEqual(1, remaining.GetView().GroupDescriptions.Count);

      source.ResetToDefaultGrouping();
      Assert.AreEqual("Participant.Class", source.ActiveGrouping);

      // Grouping of the remaining starters changes the grouping (i.e. the order) of the source
      remaining.ChangeGrouping(null);
      Assert.IsNull(source.ActiveGrouping);
      Assert.AreEqual(0, remaining.GetView().GroupDescriptions.Count);
    }
  }
}
