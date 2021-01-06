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
using System.Collections.ObjectModel;

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
    /// [ ] - SimpleSecondRunStartListViewProvider
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
    /// - Best first firstNStartnumbers (15) based on the points are randomized
    /// - Succeeding start list entries are sorted based on the points
    /// </summary>
    [TestMethod]
    public void DSVFirstRunStartListViewProvider_Test()
    {
      int i;
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();

      var participants = tg.Model.GetRace(0).GetParticipants();

      double points = 1.0;
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: points++); // points: 1.0
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: points++);
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: points++);
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: points++);
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: points++);
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: points++);
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: points++);
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: points++);
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: points++);
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: points++); // points: 10.0

      DSVFirstRunStartListViewProvider provider = new DSVFirstRunStartListViewProvider(5);
      provider.Init(participants);

      provider.ChangeGrouping(null);

      Assert.AreEqual("Name 1", provider.GetViewList()[i = 0].Name);
      Assert.AreEqual("Name 2", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 3", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 4", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 5", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 6", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 7", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 8", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 9", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 10", provider.GetViewList()[++i].Name);

      // Add two additional starter, one below first 5, one within the remaining 
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: 2.0); // StNr 11 => Pos 6
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: 8.1); // StNr 12 => Pos 10
      Assert.AreEqual("Name 1",  provider.GetViewList()[i = 0].Name);
      Assert.AreEqual("Name 2",  provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 3",  provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 4",  provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 5",  provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 11", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 6",  provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 7",  provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 8",  provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 12", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 9",  provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 10", provider.GetViewList()[++i].Name);
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


  }
}
