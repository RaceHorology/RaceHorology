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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for ImportTimeTest
  /// </summary>
  [TestClass]
  public class ImportTimeTest
  {
    public class ImportTimeMock : IImportTime
    {
      public event ImportTimeEntryEventHandler ImportTimeEntryReceived;

      public void TriggerImportTimeEntryReceived(ImportTimeEntry entry)
      {
        ImportTimeEntryReceived.Invoke(this, entry);
      }
    }



    public ImportTimeTest()
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
    [ClassInitialize()]
    public static void MyClassInitialize(TestContext testContext) 
    {
      if (System.Windows.Application.Current == null)
      {
        new System.Windows.Application { ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown };
      }
    }
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
    public void ImportTimeEntryWithParticipant()
    {
      TestDataGenerator tg = new TestDataGenerator();
      var rp = tg.createRaceParticipant();

      ImportTimeEntry ie = new ImportTimeEntry(1U, new TimeSpan(0, 0, 10));

      ImportTimeEntryWithParticipant entry1 = new ImportTimeEntryWithParticipant(ie, rp);
      Assert.AreEqual(1U, entry1.StartNumber);
      Assert.AreEqual("Name 1", entry1.Name);

      // ImportTimeEntryWithParticipant and no patient
      ImportTimeEntryWithParticipant entry2 = new ImportTimeEntryWithParticipant(ie, null);
      Assert.AreEqual(1U, entry2.StartNumber);
      Assert.AreEqual(null, entry2.Name);
    }


    [TestMethod]
    public void ImportTimeEntryVM()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.createRaceParticipants(5);
      var race = tg.Model.GetRace(0);

      ImportTimeMock importTimeMock = new ImportTimeMock();

      ImportTimeEntryVM vm = new ImportTimeEntryVM(race, importTimeMock);
      vm.AddEntry(new ImportTimeEntry (1, new TimeSpan(0, 0, 10)));

      Assert.AreEqual(1, vm.ImportEntries.Count);
      Assert.AreEqual(1U, vm.ImportEntries[0].StartNumber);
      Assert.AreEqual(new TimeSpan(0,0,0,10), vm.ImportEntries[0].RunTime);

      vm.AddEntry(new ImportTimeEntry (3, new TimeSpan(0, 0, 13)));
      Assert.AreEqual(2, vm.ImportEntries.Count);
      Assert.AreEqual(1U, vm.ImportEntries[0].StartNumber);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 10), vm.ImportEntries[0].RunTime);
      Assert.AreEqual(3U, vm.ImportEntries[1].StartNumber);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 13), vm.ImportEntries[1].RunTime);

      // Update startnumber 1
      importTimeMock.TriggerImportTimeEntryReceived(new ImportTimeEntry (1, new TimeSpan(0, 0, 11)));
      Assert.AreEqual(2, vm.ImportEntries.Count);
      Assert.AreEqual(3U, vm.ImportEntries[0].StartNumber);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 13), vm.ImportEntries[0].RunTime);
      Assert.AreEqual(1U, vm.ImportEntries[1].StartNumber);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 11), vm.ImportEntries[1].RunTime);

      // Add entry without participant
      importTimeMock.TriggerImportTimeEntryReceived(new ImportTimeEntry(999, new TimeSpan(0, 0, 9)));
      Assert.AreEqual(3, vm.ImportEntries.Count);
      Assert.AreEqual(3U, vm.ImportEntries[0].StartNumber);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 13), vm.ImportEntries[0].RunTime);
      Assert.AreEqual(1U, vm.ImportEntries[1].StartNumber);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 11), vm.ImportEntries[1].RunTime);
      Assert.AreEqual(999U, vm.ImportEntries[2].StartNumber);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 9), vm.ImportEntries[2].RunTime);

      // Add second entry without participant
      importTimeMock.TriggerImportTimeEntryReceived(new ImportTimeEntry(998, new TimeSpan(0, 0, 8)));
      Assert.AreEqual(4, vm.ImportEntries.Count);
      Assert.AreEqual(3U, vm.ImportEntries[0].StartNumber);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 13), vm.ImportEntries[0].RunTime);
      Assert.AreEqual(1U, vm.ImportEntries[1].StartNumber);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 11), vm.ImportEntries[1].RunTime);
      Assert.AreEqual(999U, vm.ImportEntries[2].StartNumber);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 9), vm.ImportEntries[2].RunTime);
      Assert.AreEqual(998U, vm.ImportEntries[3].StartNumber);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 8), vm.ImportEntries[3].RunTime);


      // Save to racerun, only time for real articipants should be taken over
      // StNr 1, 3 have time
      // StNr 2 doesn't have a time
      var rr1 = race.GetRun(0);
      Assert.AreEqual(null, rr1.GetRunResult(race.GetParticipant(1))?.Runtime);
      vm.Save(rr1);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 11), rr1.GetRunResult(race.GetParticipant(1)).Runtime);
      Assert.AreEqual(null, rr1.GetRunResult(race.GetParticipant(2))?.Runtime);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 13), rr1.GetRunResult(race.GetParticipant(3)).Runtime);
    }
  }
}
