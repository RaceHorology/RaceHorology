/*
 *  Copyright (C) 2019 - 2024 by Sven Flossmann
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

using System.IO;

using RaceHorologyLib;
using System;

namespace RaceHorologyLibTest
{


  [TestClass]
  public class UnitTest1
  {
    public TestContext TestContext
    {
      get { return _testContext; }
      set { _testContext = value; }
    }

    private TestContext _testContext;

    [TestMethod]
    public void TestMethod1()
    {
      Participant p = new Participant();
      p.Year = 1900;

      Assert.AreEqual(1900U, p.Year);
    }


    [TestMethod]
    public void TimeSpanAndFractions()
    {
      const double f1 = 0.000638078703703704;
      TimeSpan ts1 = RaceHorologyLib.Database.CreateTimeSpan(f1);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 55, 130), ts1);
      TimeSpan ts2 = RaceHorologyLib.Database.CreateTimeSpan(RaceHorologyLib.Database.FractionForTimeSpan(ts1));
      Assert.AreEqual(ts1, ts2);

      TimeSpan ts3 = new TimeSpan(0, 0, 1, 55, 130);
      string s3 = ts3.ToString(@"mm\:s\,ff");
    }

    [TestMethod]
    public void ParseTimeSpan()
    {
      Assert.AreEqual(TimeSpanExtensions.ParseTimeSpan("01.211"), new TimeSpan(0, 0, 0, 1, 211));
      Assert.AreEqual(TimeSpanExtensions.ParseTimeSpan("01.11"), new TimeSpan(0, 0, 0, 1, 110));
      Assert.AreEqual(TimeSpanExtensions.ParseTimeSpan("01.1"), new TimeSpan(0, 0, 0, 1, 100));
      Assert.AreEqual(TimeSpanExtensions.ParseTimeSpan("02,21"), new TimeSpan(0, 0, 0, 2, 210));
      Assert.AreEqual(TimeSpanExtensions.ParseTimeSpan("02,3"), new TimeSpan(0, 0, 0, 2, 300));
      Assert.AreEqual(TimeSpanExtensions.ParseTimeSpan("1:01,111"), new TimeSpan(0, 0, 1, 1, 111));
      Assert.AreEqual(TimeSpanExtensions.ParseTimeSpan("61.111"), new TimeSpan(0, 0, 1, 1, 111));
      Assert.AreEqual(TimeSpanExtensions.ParseTimeSpan("121,11"), new TimeSpan(0, 0, 2, 1, 110));
    }





  }
}
