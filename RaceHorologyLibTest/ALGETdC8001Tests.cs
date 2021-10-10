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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaceHorologyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace RaceHorologyLibTest
{

  public sealed class TestSynchronizationContext : SynchronizationContext
  {
    public override void Post(SendOrPostCallback d, object state)
    {
      d(state);
    }

    public override void Send(SendOrPostCallback d, object state)
    {
      d(state);
    }
  }

  public class ALGETdC8001TimeMeasurementSimulate : ALGETdC8001TimeMeasurementBase
  {
    string _filePath;
    System.IO.StreamReader _dumpFile;


    public ALGETdC8001TimeMeasurementSimulate(string filePath)
    {
      _filePath = filePath;
    }

    public override void Start()
    {
      _dumpFile = new System.IO.StreamReader(_filePath);

      StatusChanged.Invoke(this, true);
    }

    public override void Stop()
    {
      StatusChanged.Invoke(this, false);
      _dumpFile = null;
    }

    public override bool IsOnline { get => true; }
    public override event LiveTimingMeasurementDeviceStatusEventHandler StatusChanged;


    public bool ProcessNextLine()
    {
      string dataLine = _dumpFile.ReadLine();
      if (dataLine == null)
        return false;

      processLine(dataLine);
      return true;
    }


    public void SimulteProcessLine(string line)
    {
      processLine(line);
    }
  }


  [TestClass]
  public class ALGETdC8001Tests
  {
    public ALGETdC8001Tests()
    {
    }

    [TestInitialize]
    public void Init()
    {
      SynchronizationContext.SetSynchronizationContext(new TestSynchronizationContext());
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
    public void ParserTest()
    {
      ALGETdC8001LineParser parser = new ALGETdC8001LineParser();

      {
        var pd = parser.Parse(" 0035 C0M 21:46:36.3900 00");
        Assert.AreEqual(' ', pd.Flag);
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual("C0", pd.Channel);
        Assert.AreEqual('M', pd.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 36, 390), pd.Time);
      }

      {
        var pd = parser.Parse(" 0035 C0  21:46:36.3910 00");
        Assert.AreEqual(' ', pd.Flag);
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual("C0", pd.Channel);
        Assert.AreEqual(' ', pd.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 36, 391), pd.Time);
      }

      {
        var pd = parser.Parse("?0034 C1M 21:46:48.3300 00");
        Assert.AreEqual('?', pd.Flag);
        Assert.AreEqual(34U, pd.StartNumber);
        Assert.AreEqual("C1", pd.Channel);
        Assert.AreEqual('M', pd.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 330), pd.Time);
      }

      {
        var pd = parser.Parse("n0034");
        Assert.AreEqual('n', pd.Flag);
        Assert.AreEqual(34U, pd.StartNumber);
        Assert.AreEqual("", pd.Channel);
        Assert.AreEqual(' ', pd.ChannelModifier);
        Assert.AreEqual(new TimeSpan(), pd.Time);
      }

      // Uncommon input for parser
      Assert.ThrowsException<FormatException>(() => { parser.Parse("                                "); });

      #region Different Time Accuracy
      {
        var pd = parser.Parse("?0034 C1M 21:46:48.1230 00");
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 123), pd.Time);
      }
      {
        var pd = parser.Parse("?0034 C1M 21:46:48.123  00");
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 123), pd.Time);
      }
      {
        var pd = parser.Parse("?0034 C1M 21:46:48.12   00");
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 120), pd.Time);
      }
      {
        var pd = parser.Parse("?0034 C1M 21:46:48.1    00");
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 100), pd.Time);
      }
      #endregion

      #region ALGE WTN 
      {
        var pd = parser.Parse("t0003 C1  16:01:56.6585 00");
        Assert.AreEqual('t', pd.Flag);
        Assert.AreEqual(3U, pd.StartNumber);
        Assert.AreEqual("C1", pd.Channel);
        Assert.AreEqual(' ', pd.ChannelModifier);
        Assert.AreEqual((new TimeSpan(0, 16, 01, 56, 658)).AddMicroseconds(500), pd.Time);
      }
      #endregion
    }


    [TestMethod]
    public void HandleUncommonInput()
    {
      ALGETdC8001TimeMeasurementSimulate alge = new ALGETdC8001TimeMeasurementSimulate(string.Empty);

      alge.SimulteProcessLine("                                ");
    }

    [TestMethod]
    public void ParserAndTransferToTimemeasurementDataTest()
    {

      TimeMeasurementEventArgs ParseAndTransfer(string line)
      {
        ALGETdC8001LineParser parser = new ALGETdC8001LineParser();
        return ALGETdC8001TimeMeasurement.TransferToTimemeasurementData(parser.Parse(line));
      }

      { 
        var pd = ParseAndTransfer(" 0035 C0M 21:46:36.3900 00");
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 36, 390), pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
      }

      {
        var pd = ParseAndTransfer(" 0035 C0  21:46:36.3910 00");
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 36, 391), pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
      }

      {
        var pd = ParseAndTransfer(" 0001 RTM 00:00:20.1    00");
        Assert.AreEqual(1U, pd.StartNumber);
        Assert.AreEqual(true, pd.BRunTime);
        Assert.AreEqual(new TimeSpan(0, 0, 0, 20, 100), pd.RunTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BStartTime);
      }

      { // Disqualified
        var pd = ParseAndTransfer("d0035 C0  21:46:36.3910 00");
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(null, pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
      }
      { // Cleared data
        var pd = ParseAndTransfer("c0035 C0  21:46:36.3910 00");
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(null, pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
      }

      // Ignored data (first character)
      { // Invalid startnumber
        var pd = ParseAndTransfer("?0034 C1M 21:46:48.3300 00");
        Assert.IsNull(pd);
      }
      { // penalty time (parallelslalom)
        var pd = ParseAndTransfer("p0034 C1M 21:46:48.3300 00");
        Assert.IsNull(pd);
      }
      { // time was blocked with block key)
        var pd = ParseAndTransfer("b0034 C1M 21:46:48.3300 00");
        Assert.IsNull(pd);
      }
      { // memory time TODO: Check
        var pd = ParseAndTransfer("m0034 C1M 21:46:48.3300 00");
        Assert.IsNull(pd);
      }
      {
        var pd = ParseAndTransfer("n0034");
        Assert.IsNull(pd);
      }
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case1\KSC4--U12.mdb")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case1\KSC4--U12_GiantSlalom.config")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case1\KSC4--U12_ALGE_Run1.txt")]
    [DeploymentItem(@"TestDataBases\FullTestCases\Case1\KSC4--U12_ALGE_Run2.txt")]
    public void FullTest()
    {
      // Preparation & Test Description
      // - Copy DB
      // - Delete the tlbZeit
      // - Run ALGE DG 1
      // - Compare results DG1
      // - Run ALGE DG 2
      // - Compare results DG2

      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"KSC4--U12.mdb");

      // Create working copy
      string dbFilenameWork = TestUtilities.Copy(dbFilename, @"KSC4--U12_work.mdb");
      DBTestUtilities dbtuDst = new DBTestUtilities(dbFilenameWork);
      dbtuDst.ClearTimeMeasurements();
      dbtuDst.Close();

      testRun(0, @"KSC4--U12_ALGE_Run1.txt");
      testRun(1, @"KSC4--U12_ALGE_Run2.txt");

      void testRun(int run, string testfile)
      {
        // Setup Data Model & Co for Simulating ALGE
        Database dbWork = new Database();
        dbWork.Connect(dbFilenameWork);
        AppDataModel modelWork = new AppDataModel(dbWork);
        LiveTimingMeasurement liveTimingMeasurement = new LiveTimingMeasurement(modelWork);

        ALGETdC8001TimeMeasurementSimulate algeSimulator = new ALGETdC8001TimeMeasurementSimulate(testfile);
        liveTimingMeasurement.SetTimingDevice(algeSimulator, algeSimulator);
        algeSimulator.Start();

        modelWork.SetCurrentRaceRun(modelWork.GetRace(0).GetRun(run));

        liveTimingMeasurement.Start();
        while (algeSimulator.ProcessNextLine())
        {
        }
        liveTimingMeasurement.Stop();
        algeSimulator.Stop();

        dbWork.Close();

        // Compare the generated DB with the ground truth DB
        Database dbCmpWork = new Database();
        dbCmpWork.Connect(dbFilenameWork);
        AppDataModel modelCmpWork = new AppDataModel(dbCmpWork);

        Database dbSrc = new Database();
        dbSrc.Connect(dbFilename);
        AppDataModel modelSrc = new AppDataModel(dbSrc);

        foreach (var res in modelSrc.GetRace(0).GetRun(run).GetResultList())
        {
          if (res.ResultCode == RunResult.EResultCode.Normal)
          {
            var resWork = modelCmpWork.GetRace(0).GetRun(run).GetResultList().FirstOrDefault(r => r.StartNumber == res.StartNumber);

            Assert.AreEqual(res.GetStartTime(), resWork.GetStartTime());
            Assert.AreEqual(res.GetFinishTime(), resWork.GetFinishTime());
            Assert.AreEqual(res.GetRunTime(), resWork.GetRunTime());
          }
        }
      }
    }
  }
}
