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
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for HandTimingTest
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


    [TestMethod]
    [DeploymentItem(@"TestDataBases\HandTime\--Handzeit-Start.txt")]
    public void Parser()
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

    [TestMethod]
    public void CreateHandTiming()
    {
      Assert.AreEqual(typeof(FromFileHandTiming), HandTiming.CreateHandTiming("File", "abc").GetType());
      Assert.AreEqual(typeof(TagHeuer), HandTiming.CreateHandTiming("TagHeuerPPro", "abc").GetType());
      Assert.AreEqual(typeof(ALGETimy), HandTiming.CreateHandTiming("ALGETimy", "abc").GetType());
    }






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
