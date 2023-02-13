/*
 *  Copyright (C) 2019 - 2023 by Sven Flossmann
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
    public override bool IsStarted { get => _dumpFile != null; }
    public override bool IsBroken { get { return false; } }
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
        parser.Parse(" 0035 C0M 21:46:36.3900 00");
        Assert.AreEqual(' ', parser.TimingData.Flag);
        Assert.AreEqual(35U, parser.TimingData.StartNumber);
        Assert.AreEqual(' ', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("C0", parser.TimingData.Channel);
        Assert.AreEqual('M', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 36, 390), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }

      {
        parser.Parse(" 0035 C0  21:46:36.3910 00");
        Assert.AreEqual(' ', parser.TimingData.Flag);
        Assert.AreEqual(35U, parser.TimingData.StartNumber);
        Assert.AreEqual(' ', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("C0", parser.TimingData.Channel);
        Assert.AreEqual(' ', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 36, 391), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }

      {
        parser.Parse("?0034 C1M 21:46:48.3300 00");
        Assert.AreEqual('?', parser.TimingData.Flag);
        Assert.AreEqual(34U, parser.TimingData.StartNumber);
        Assert.AreEqual(' ', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("C1", parser.TimingData.Channel);
        Assert.AreEqual('M', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 330), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }

      {
        parser.Parse("n0034");
        Assert.AreEqual('n', parser.TimingData.Flag);
        Assert.AreEqual(34U, parser.TimingData.StartNumber);
        Assert.AreEqual(' ', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("", parser.TimingData.Channel);
        Assert.AreEqual(' ', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }

      {
        // Uncommon input for parser
        parser.Parse("                                ");
        Assert.IsNull(parser.TimingData);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      #region Different Time Accuracy
      {
        parser.Parse("?0034 C1M 21:46:48.1230 00");
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 123), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse("?0034 C1M 21:46:48.123  00");
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 123), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse("?0034 C1M 21:46:48.12   00");
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 120), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse("?0034 C1M 21:46:48.1    00");
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 100), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      #endregion

      #region ALGE WTN 
      {
        parser.Parse("t0003 C1  16:01:56.6585 00");
        Assert.AreEqual('t', parser.TimingData.Flag);
        Assert.AreEqual(3U, parser.TimingData.StartNumber);
        Assert.AreEqual("C1", parser.TimingData.Channel);
        Assert.AreEqual(' ', parser.TimingData.ChannelModifier);
        Assert.AreEqual((new TimeSpan(0, 16, 01, 56, 658)).AddMicroseconds(500), parser.TimingData.Time);
      }
      #endregion

      #region Short Lines 
      {
        parser.Parse("n0003");
        Assert.AreEqual('n', parser.TimingData.Flag);
        Assert.AreEqual(3U, parser.TimingData.StartNumber);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      { 
        parser.Parse("s0003");
        Assert.AreEqual('s', parser.TimingData.Flag);
        Assert.AreEqual(3U, parser.TimingData.StartNumber);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      #endregion

    }


    [TestMethod]
    public void ParserTest_ParallelSlalom()
    {

      ALGETdC8001LineParser parser = new ALGETdC8001LineParser();

      {
        parser.Parse("n0000b");
        Assert.AreEqual('n', parser.TimingData.Flag);
        Assert.AreEqual(0U, parser.TimingData.StartNumber);
        Assert.AreEqual('b', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("", parser.TimingData.Channel);
        Assert.AreEqual(' ', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse("n0015b");
        Assert.AreEqual('n', parser.TimingData.Flag);
        Assert.AreEqual(15U, parser.TimingData.StartNumber);
        Assert.AreEqual('b', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("", parser.TimingData.Channel);
        Assert.AreEqual(' ', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse("n0000r");
        Assert.AreEqual('n', parser.TimingData.Flag);
        Assert.AreEqual(0U, parser.TimingData.StartNumber);
        Assert.AreEqual('r', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("", parser.TimingData.Channel);
        Assert.AreEqual(' ', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse("n0016r");
        Assert.AreEqual('n', parser.TimingData.Flag);
        Assert.AreEqual(16U, parser.TimingData.StartNumber);
        Assert.AreEqual('r', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("", parser.TimingData.Channel);
        Assert.AreEqual(' ', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse(" 0016rC0  19:52:15.1620 09");
        Assert.AreEqual(' ', parser.TimingData.Flag);
        Assert.AreEqual(16U, parser.TimingData.StartNumber);
        Assert.AreEqual('r', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("C0", parser.TimingData.Channel);
        Assert.AreEqual(' ', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 19, 52, 15, 162), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse(" 0015bC3  19:52:15.1620 09");
        Assert.AreEqual(' ', parser.TimingData.Flag);
        Assert.AreEqual(15U, parser.TimingData.StartNumber);
        Assert.AreEqual('b', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("C3", parser.TimingData.Channel);
        Assert.AreEqual(' ', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 19, 52, 15, 162), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse(" 0016rC1  19:52:20.3900 09");
        Assert.AreEqual(' ', parser.TimingData.Flag);
        Assert.AreEqual(16U, parser.TimingData.StartNumber);
        Assert.AreEqual('r', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("C1", parser.TimingData.Channel);
        Assert.AreEqual(' ', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 19, 52, 20, 390), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse(" 0016rRT  00:00:05.22   09");
        Assert.AreEqual(' ', parser.TimingData.Flag);
        Assert.AreEqual(16U, parser.TimingData.StartNumber);
        Assert.AreEqual('r', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("RT", parser.TimingData.Channel);
        Assert.AreEqual(' ', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 0, 0, 5, 220), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse(" 0015bC4  19:52:23.4010 09");
        Assert.AreEqual(' ', parser.TimingData.Flag);
        Assert.AreEqual(15U, parser.TimingData.StartNumber);
        Assert.AreEqual('b', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("C4", parser.TimingData.Channel);
        Assert.AreEqual(' ', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 19, 52, 23, 401), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse(" 0015bRT  00:00:08.23   09");
        Assert.AreEqual(' ', parser.TimingData.Flag);
        Assert.AreEqual(15U, parser.TimingData.StartNumber);
        Assert.AreEqual('b', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("RT", parser.TimingData.Channel);
        Assert.AreEqual(' ', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 0, 0, 8, 230), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse(" 0016rDTR 00:00:03.01   09");
        Assert.AreEqual(' ', parser.TimingData.Flag);
        Assert.AreEqual(16U, parser.TimingData.StartNumber);
        Assert.AreEqual('r', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("DT", parser.TimingData.Channel);
        Assert.AreEqual('R', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 0, 0, 3, 010), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse(" 0008bDTT 10:12:45.23   09");
        Assert.AreEqual(' ', parser.TimingData.Flag);
        Assert.AreEqual(8U, parser.TimingData.StartNumber);
        Assert.AreEqual('b', parser.TimingData.StartNumberModifier);
        Assert.AreEqual("DT", parser.TimingData.Channel);
        Assert.AreEqual('T', parser.TimingData.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 10, 12, 45, 230), parser.TimingData.Time);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }

    }


    [TestMethod]
    public void ParserClassementTest()
    {
      ALGETdC8001LineParser parser = new ALGETdC8001LineParser();

      {
        parser.Parse("                                ");
        Assert.IsNull(parser.TimingData);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse("CLASSEMENT:");
        Assert.IsNull(parser.TimingData);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.Classement, parser.Mode);
      }
      {
        parser.Parse("ALL");
        Assert.IsNull(parser.TimingData);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.Classement, parser.Mode);
      }
      {
        parser.Parse("RUN TIME");
        Assert.IsNull(parser.TimingData);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.Classement, parser.Mode);
      }
      {
        parser.Parse(" 0001 RTM 00:00:13.39   00 0001");
        Assert.AreEqual(' ', parser.TimingData.Flag);
        Assert.AreEqual(1U, parser.TimingData.StartNumber);
        Assert.AreEqual(new TimeSpan(0, 0, 0, 13, 390), parser.TimingData.Time);
        Assert.AreEqual("RT", parser.TimingData.Channel);
        Assert.AreEqual('M', parser.TimingData.ChannelModifier);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.Classement, parser.Mode);
      }
      {
        parser.Parse(" 0002 RTM 00:00:13.68   00 0002");
        Assert.AreEqual(' ', parser.TimingData.Flag);
        Assert.AreEqual(2U, parser.TimingData.StartNumber);
        Assert.AreEqual(new TimeSpan(0, 0, 0, 13, 680), parser.TimingData.Time);
        Assert.AreEqual("RT", parser.TimingData.Channel);
        Assert.AreEqual('M', parser.TimingData.ChannelModifier);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.Classement, parser.Mode);
      }
      {
        parser.Parse(" 0004 RTM 00:00:13.89   00 0003");
        Assert.AreEqual(' ', parser.TimingData.Flag);
        Assert.AreEqual(4U, parser.TimingData.StartNumber);
        Assert.AreEqual(new TimeSpan(0, 0, 0, 13, 890), parser.TimingData.Time);
        Assert.AreEqual("RT", parser.TimingData.Channel);
        Assert.AreEqual('M', parser.TimingData.ChannelModifier);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.Classement, parser.Mode);
      }
      {
        parser.Parse(" 0003 RTM 00:00:14.05   00 0004");
        Assert.AreEqual(' ', parser.TimingData.Flag);
        Assert.AreEqual(3U, parser.TimingData.StartNumber);
        Assert.AreEqual(new TimeSpan(0, 0, 0, 14, 50), parser.TimingData.Time);
        Assert.AreEqual("RT", parser.TimingData.Channel);
        Assert.AreEqual('M', parser.TimingData.ChannelModifier);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.Classement, parser.Mode);
      }
      {
        parser.Parse("  ALGE TIMING");
        Assert.IsNull(parser.TimingData);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse("   TdC  8001");
        Assert.IsNull(parser.TimingData);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse("  DEU V 18.92");
        Assert.IsNull(parser.TimingData);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
      {
        parser.Parse("21-11-28  17:04");
        Assert.IsNull(parser.TimingData);
        Assert.AreEqual(ALGETdC8001LineParser.EMode.LiveTiming, parser.Mode);
      }
    }

    /// <summary>
    /// Tests whether the parser switches between Classement and LiveTiming correctly
    /// </summary>
    [TestMethod]
    public void ParserModeTest()
    {
      List<Tuple<string, ALGETdC8001LineParser.EMode>> testData = new List<Tuple<string, ALGETdC8001LineParser.EMode>>
      {
        Tuple.Create(" 0035 C0M 21:46:36.3900 00", ALGETdC8001LineParser.EMode.LiveTiming),
        Tuple.Create("                                ", ALGETdC8001LineParser.EMode.LiveTiming),
        Tuple.Create("CLASSEMENT:", ALGETdC8001LineParser.EMode.Classement),
        Tuple.Create("                                ", ALGETdC8001LineParser.EMode.Classement),
        Tuple.Create("ALL", ALGETdC8001LineParser.EMode.Classement),
        Tuple.Create("                                ", ALGETdC8001LineParser.EMode.Classement),
        Tuple.Create("RUN TIME", ALGETdC8001LineParser.EMode.Classement),
        Tuple.Create("                                ", ALGETdC8001LineParser.EMode.Classement),
        Tuple.Create("                                ", ALGETdC8001LineParser.EMode.Classement),
        Tuple.Create(" 0001 RTM 00:00:13.39   00 0001", ALGETdC8001LineParser.EMode.Classement),
        Tuple.Create(" 0002 RTM 00:00:13.68   00 0002", ALGETdC8001LineParser.EMode.Classement),
        Tuple.Create(" 0004 RTM 00:00:13.89   00 0003", ALGETdC8001LineParser.EMode.Classement),
        Tuple.Create(" 0003 RTM 00:00:14.05   00 0004", ALGETdC8001LineParser.EMode.Classement),
        Tuple.Create("                                ", ALGETdC8001LineParser.EMode.Classement),
        Tuple.Create("                                ", ALGETdC8001LineParser.EMode.Classement),
        Tuple.Create("  ALGE TIMING", ALGETdC8001LineParser.EMode.LiveTiming),
        Tuple.Create("   TdC  8001", ALGETdC8001LineParser.EMode.LiveTiming),
        Tuple.Create("  DEU V 18.92", ALGETdC8001LineParser.EMode.LiveTiming),
        Tuple.Create("21 - 11 - 28  17:04", ALGETdC8001LineParser.EMode.LiveTiming),
        Tuple.Create(" 0035 C0  21:46:36.3910 00", ALGETdC8001LineParser.EMode.LiveTiming)
      };

      ALGETdC8001LineParser parser = new ALGETdC8001LineParser();

      int line = 0;
      foreach( var item in testData)
      {
        parser.Parse(item.Item1);
        Assert.AreEqual(item.Item2, parser.Mode);
        line++;
      }
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
        parser.Parse(line);
        return ALGETdC8001TimeMeasurement.TransferToTimemeasurementData(parser.TimingData);
      }

      { 
        var pd = ParseAndTransfer(" 0035 C0M 21:46:36.3900 00");
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 36, 390), pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
        Assert.IsTrue(pd.Valid);
      }

      {
        var pd = ParseAndTransfer(" 0035 C0  21:46:36.3910 00");
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 36, 391), pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
        Assert.IsTrue(pd.Valid);
      }

      {
        var pd = ParseAndTransfer(" 0001 RTM 00:00:20.1    00");
        Assert.AreEqual(1U, pd.StartNumber);
        Assert.AreEqual(true, pd.BRunTime);
        Assert.AreEqual(new TimeSpan(0, 0, 0, 20, 100), pd.RunTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BStartTime);
        Assert.IsTrue(pd.Valid);
      }

      { // Disqualified
        var pd = ParseAndTransfer("d0035 C0  21:46:36.3910 00");
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(null, pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
        Assert.IsTrue(pd.Valid);
      }
      { // Cleared data
        var pd = ParseAndTransfer("c0035 C0  21:46:36.3910 00");
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(null, pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
        Assert.IsTrue(pd.Valid);
      }

      // Ignored data (first character)
      { // Invalid startnumber
        var pd = ParseAndTransfer("?0034 C1M 21:46:48.3300 00");
        Assert.AreEqual(34U, pd.StartNumber);
        Assert.AreEqual(false, pd.BStartTime);
        Assert.AreEqual(null, pd.StartTime);
        Assert.AreEqual(true, pd.BFinishTime);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 330), pd.FinishTime);
        Assert.AreEqual(false, pd.BRunTime);
        Assert.AreEqual(null, pd.RunTime);
        Assert.IsFalse(pd.Valid);
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
      {
        var pd = ParseAndTransfer("s0003");
        Assert.IsNull(pd);
      }
    }


    [TestMethod]
    public void ParserAndTransferToTimemeasurementDataTest_ParallelSlalom()
    {

      TimeMeasurementEventArgs ParseAndTransfer(string line)
      {
        ALGETdC8001LineParser parser = new ALGETdC8001LineParser();
        parser.Parse(line);
        return ALGETdC8001TimeMeasurement.TransferToTimemeasurementData(parser.TimingData);
      }

      {
        var pd = ParseAndTransfer(" 0016rC0  19:52:15.1620 09");
        Assert.AreEqual(16U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(new TimeSpan(0, 19, 52, 15, 162), pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
        Assert.IsTrue(pd.Valid);
      }

      {
        var pd = ParseAndTransfer(" 0015bC3  19:52:15.1620 09");
        Assert.AreEqual(15U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(new TimeSpan(0, 19, 52, 15, 162), pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
        Assert.IsTrue(pd.Valid);
      }

      {
        var pd = ParseAndTransfer(" 0016rC1  19:52:20.3900 09");
        Assert.AreEqual(16U, pd.StartNumber);
        Assert.AreEqual(true, pd.BFinishTime);
        Assert.AreEqual(new TimeSpan(0, 19, 52, 20, 390), pd.FinishTime);
        Assert.AreEqual(false, pd.BStartTime);
        Assert.AreEqual(false, pd.BRunTime);
        Assert.IsTrue(pd.Valid);
      }

      {
        var pd = ParseAndTransfer(" 0015bC4  19:52:23.4010 09");
        Assert.AreEqual(15U, pd.StartNumber);
        Assert.AreEqual(true, pd.BFinishTime);
        Assert.AreEqual(new TimeSpan(0, 19, 52, 23, 401), pd.FinishTime);
        Assert.AreEqual(false, pd.BStartTime);
        Assert.AreEqual(false, pd.BRunTime);
        Assert.IsTrue(pd.Valid);
      }

      {
        var pd = ParseAndTransfer(" 0016rRT  00:00:05.22   09");
        Assert.AreEqual(16U, pd.StartNumber);
        Assert.AreEqual(true, pd.BRunTime);
        Assert.AreEqual(new TimeSpan(0, 0, 0, 5, 220), pd.RunTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BStartTime);
        Assert.IsTrue(pd.Valid);
      }

      {
        var pd = ParseAndTransfer(" 0015bRT  00:00:08.23   09");
        Assert.AreEqual(15U, pd.StartNumber);
        Assert.AreEqual(true, pd.BRunTime);
        Assert.AreEqual(new TimeSpan(0, 0, 0, 8, 230), pd.RunTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BStartTime);
        Assert.IsTrue(pd.Valid);
      }

      { // Disqualified
        var pd = ParseAndTransfer("d0035 C0  21:46:36.3910 00");
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(null, pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
        Assert.IsTrue(pd.Valid);
      }
      { // Cleared data
        var pd = ParseAndTransfer("c0035 C0  21:46:36.3910 00");
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(null, pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
        Assert.IsTrue(pd.Valid);
      }

      // Ignored data (first character)
      { // Invalid startnumber
        var pd = ParseAndTransfer("?0034 C1M 21:46:48.3300 00");
        Assert.AreEqual(34U, pd.StartNumber);
        Assert.AreEqual(false, pd.BStartTime);
        Assert.AreEqual(null, pd.StartTime);
        Assert.AreEqual(true, pd.BFinishTime);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 330), pd.FinishTime);
        Assert.AreEqual(false, pd.BRunTime);
        Assert.AreEqual(null, pd.RunTime);
        Assert.IsFalse(pd.Valid);
      }
      { // penalty time (parallelslalom)
        var pd = ParseAndTransfer("p0034 C1M 21:46:48.3300 00");
        Assert.IsNull(pd);
      }
      { // time was blocked with block key)
        var pd = ParseAndTransfer("b0034 C1M 21:46:48.3300 00");
        Assert.IsNull(pd);
      }
      { // parallel slalom: time difference run
        var pd = ParseAndTransfer(" 0016rDTR 00:00:03.01   09");
        Assert.IsNull(pd);
      }
      { // parallel slalom: time difference total
        var pd = ParseAndTransfer(" 0008bDTT 10:12:45.23   09");
        Assert.IsNull(pd);
      }
      {
        var pd = ParseAndTransfer("n0000b");
        Assert.IsNull(pd);
      }
      {
        var pd = ParseAndTransfer("n0015b");
        Assert.IsNull(pd);
      }
      {
        var pd = ParseAndTransfer("n0016r");
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
        liveTimingMeasurement.AddTimingDevice(algeSimulator, true);
        liveTimingMeasurement.SetLiveDateTimeProvider(algeSimulator);
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
