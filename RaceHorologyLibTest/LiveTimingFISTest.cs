﻿/*
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
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using XmlUnit.Xunit;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for LiveTimingFISTest
  /// </summary>
  [TestClass]
  public class LiveTimingFISTest
  {
    public LiveTimingFISTest()
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
    public void XmlSerializer()
    {
      string xml;

      LiveTimingFIS.LiveTimingFIS lt = new LiveTimingFIS.LiveTimingFIS();
      
      xml = lt.getXmlKeepAlive();
      XmlAssertion.AssertXPathExists("/livetiming/command/keepalive", xml);


      xml = lt.getXmlClearRace();
      XmlAssertion.AssertXPathExists("/livetiming/command/clear", xml);


      xml = lt.getXmlStatusUpdateInfo("test");
      XmlAssertion.AssertXPathExists("/livetiming/message", xml);
      XmlAssertion.AssertXPathEvaluatesTo("/livetiming/message", xml, "test");
    }

    [TestMethod]
    public void XmlSerializer_EventResult()
    {
      string getXmlEventResult(uint _startNumber, RaceRun _rr, LiveTimingFIS.LiveTimingFIS _lt)
      {
        var _results = ViewUtilities.ViewToList<RunResultWithPosition>(_rr.GetResultView());
        var _res = _results.FirstOrDefault(_r => _r.StartNumber == _startNumber);
        return _lt.getXmlEventResult(_rr, _res);
      }

      string xml;
      LiveTimingFIS.LiveTimingFIS lt = new LiveTimingFIS.LiveTimingFIS();

      TestDataGenerator tg = new TestDataGenerator();
      tg.createRaceParticipant();
      var r = tg.Model.GetRace(0);
      var rr1 = r.GetRun(0);

      xml = getXmlEventResult(1, rr1, lt);
      XmlAssertion.AssertXPathEvaluatesTo("/livetiming/raceevent/finish/@bib", xml, "1");
      XmlAssertion.AssertXPathEvaluatesTo("/livetiming/raceevent/finish/@correction", xml, "y");
      XmlAssertion.AssertXPathEvaluatesTo("/livetiming/raceevent/finish/time", xml, "0.00");

      rr1.SetStartFinishTime(r.GetParticipant(1), new TimeSpan(0, 08, 0, 0, 0), new TimeSpan(0, 08, 0, 0, 100));
      xml = getXmlEventResult(1, rr1, lt);
      XmlAssertion.AssertXPathEvaluatesTo("/livetiming/raceevent/finish/time", xml, "0.10");

      rr1.SetStartFinishTime(r.GetParticipant(1), new TimeSpan(0, 08, 0, 0, 0), new TimeSpan(0, 08, 0, 30, 100));
      xml = getXmlEventResult(1, rr1, lt);
      XmlAssertion.AssertXPathEvaluatesTo("/livetiming/raceevent/finish/time", xml, "30.10");

      rr1.SetStartFinishTime(r.GetParticipant(1), new TimeSpan(0, 08, 0, 0, 0), new TimeSpan(0, 08, 1, 30, 100));
      xml = getXmlEventResult(1, rr1, lt);
      XmlAssertion.AssertXPathEvaluatesTo("/livetiming/raceevent/finish/time", xml, "1:30.10");
    }
  }
}
