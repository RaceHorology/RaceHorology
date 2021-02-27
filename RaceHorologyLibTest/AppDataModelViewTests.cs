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
      Assert.AreEqual("Name 1", provider.GetViewList()[i = 0].Name);
      Assert.AreEqual("Name 2", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 3", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 4", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 5", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 11", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 6", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 7", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 8", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 12", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 9", provider.GetViewList()[++i].Name);
      Assert.AreEqual("Name 10", provider.GetViewList()[++i].Name);
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


    [TestMethod]
    public void BasedOnResultsFirstRunStartListViewProvider_Test()
    { 
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
          var tM = RaceResultViewProvider.MinimumTime(results, out resCode, out disqualText);
          Assert.AreEqual(new TimeSpan(0, 0, 59), tM);

          var tS = RaceResultViewProvider.SumTime(results, out resCode, out disqualText);
          Assert.AreEqual(new TimeSpan(0, 1, 59), tS);
        }
        results.Add(3, rr1m01);
        {
          var tM = RaceResultViewProvider.MinimumTime(results, out resCode, out disqualText);
          Assert.AreEqual(new TimeSpan(0, 0, 59), tM);

          var tS = RaceResultViewProvider.SumTime(results, out resCode, out disqualText);
          Assert.AreEqual(new TimeSpan(0, 3, 00), tS);
        }
      }

      {
        results.Clear();
        results.Add(1, rr1m00);
        results.Add(2, rr1m00NIZ);

        var tM = RaceResultViewProvider.MinimumTime(results, out resCode, out disqualText);
        Assert.AreEqual(new TimeSpan(0, 1, 00), tM);

        var tS = RaceResultViewProvider.SumTime(results, out resCode, out disqualText);
        Assert.AreEqual(null, tS);
        Assert.AreEqual(RunResult.EResultCode.NiZ, resCode);
        Assert.IsTrue(string.IsNullOrEmpty(disqualText));
      }

      {
        results.Clear();
        results.Add(1, rr1m00NAS);
        results.Add(2, rr1m00NIZ);

        var tM = RaceResultViewProvider.MinimumTime(results, out resCode, out disqualText);
        Assert.AreEqual(null, tM);

        var tS = RaceResultViewProvider.SumTime(results, out resCode, out disqualText);
        Assert.AreEqual(null, tS);
        Assert.AreEqual(RunResult.EResultCode.NaS, resCode);  // First run rules
        Assert.IsTrue(string.IsNullOrEmpty(disqualText));
      }

      {
        results.Clear();
        results.Add(1, rr1m00);
        results.Add(2, rr0m59);
        results.Add(3, rr1m00NIZ);

        var tM = RaceResultViewProvider.MinimumTime(results, out resCode, out disqualText);
        Assert.AreEqual(new TimeSpan(0, 0, 59), tM);

        var tS = RaceResultViewProvider.SumTime(results, out resCode, out disqualText);
        Assert.AreEqual(null, tS);
        Assert.AreEqual(RunResult.EResultCode.NiZ, resCode);
        Assert.IsTrue(string.IsNullOrEmpty(disqualText));
      }

      {
        results.Clear();
        results.Add(1, rr1m00);
        results.Add(2, rr1m00DIS1);
        results.Add(3, rr1m01);

        var tM = RaceResultViewProvider.MinimumTime(results, out resCode, out disqualText);
        Assert.AreEqual(new TimeSpan(0, 1, 0), tM);

        var tS = RaceResultViewProvider.SumTime(results, out resCode, out disqualText);
        Assert.AreEqual(null, tS);
        Assert.AreEqual(RunResult.EResultCode.DIS, resCode);
        Assert.AreEqual("Tor 1", disqualText);
      }

      {
        results.Clear();
        results.Add(1, rr1m00DIS2);
        results.Add(2, rr1m00DIS1);

        var tM = RaceResultViewProvider.MinimumTime(results, out resCode, out disqualText);
        Assert.AreEqual(null, tM);
        Assert.AreEqual(RunResult.EResultCode.DIS, resCode);
        Assert.AreEqual("Tor 2, Tor 1", disqualText);

        var tS = RaceResultViewProvider.SumTime(results, out resCode, out disqualText);
        Assert.AreEqual(null, tS);
        Assert.AreEqual(RunResult.EResultCode.DIS, resCode);
        Assert.AreEqual("Tor 2", disqualText);
      }

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


  }
}
