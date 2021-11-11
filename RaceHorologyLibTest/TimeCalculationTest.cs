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
using System;
using System.Collections.Generic;
using System.Text;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for TimeCalculationTest
  /// </summary>
  [TestClass]
  public class TimeCalculationTest
  {
    public TimeCalculationTest()
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
    /// Helper method to create test times with accuracy of 1/10.000
    /// </summary>
    /// <returns></returns>
    static TimeSpan createTestTime(int hour, int min, int sec, int sec10000th)
    {
      TimeSpan t = new TimeSpan(0, hour, min, sec);
      long ticks = sec10000th * TimeSpan.TicksPerMillisecond / 10;
      t = t.Add(new TimeSpan(ticks));
      return t;
    }


    /// <summary>
    /// Tests the helper method createTestTime()
    /// </summary>
    [TestMethod]
    public void CreatTestTime_Test()
    {
      var t = createTestTime(12, 57, 49, 0158);
      Assert.AreEqual("12:57:49.0158", t.ToString(@"hh\:mm\:ss\.ffff"));

      t = createTestTime(12, 57, 12, 5021);
      Assert.AreEqual("12:57:12.5021", t.ToString(@"hh\:mm\:ss\.ffff"));
    }


    /// <summary>
    /// Test on correct time calculation (correct diff and floor at 1/100s)
    /// </summary>
    [TestMethod]
    public void Test1()
    {
      TestDataGenerator tg = new TestDataGenerator();

      tg.createRaceParticipant();

      var race = tg.Model.GetRace(0);
      var raceRun = tg.Model.GetRace(0).GetRun(0);

      //Zielzeit: 12:57:49,0158
      //Startzeit: 12:57:12,5021
      //Laufzeit: 00:00:36,5137 → abgeschnitten: 36,51
      raceRun.SetStartTime(race.GetParticipant(1), createTestTime(12, 57, 12, 5021));
      raceRun.SetFinishTime(race.GetParticipant(1), createTestTime(12, 57, 49, 158));
      Assert.AreEqual(new TimeSpan(0, 0, 0, 36, 510), raceRun.GetRunResult(race.GetParticipant(1)).Runtime);

      //Zielzeit: 12:57:02,5620
      //Startzeit: 12:56:26,2246
      //Laufzeit: 00:00:36,3374 → abgeschnitten: 36,33
      raceRun.SetStartTime(race.GetParticipant(1), createTestTime(12, 56, 26, 2246));
      raceRun.SetFinishTime(race.GetParticipant(1), createTestTime(12, 57, 02, 5620));
      Assert.AreEqual(new TimeSpan(0, 0, 0, 36, 330), raceRun.GetRunResult(race.GetParticipant(1)).Runtime);
    }
  }
}
