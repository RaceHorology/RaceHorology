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

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for ValueConverterTest
  /// </summary>
  [TestClass]
  public class ValueConverterTest
  {
    public ValueConverterTest()
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
    public void AgeToYearInputConverterTest()
    {
      var converter = new AgeToYearInputConverter();

      // Standard forward conversion, no change in object
      Assert.AreEqual(10, converter.Convert(10, null, null, null));

      // Check years stay years
      Assert.AreEqual(2010, converter.ConvertBack(2010, null, null, null));
      Assert.AreEqual(2020, converter.ConvertBack(2020, null, null, null));

      // Check ages get years
      Assert.AreEqual(DateTime.Now.AddMonths(3).Year - 10, converter.ConvertBack(10, null, null, null));
    }

    [TestMethod]
    public void TimeSpanConverterTest()
    {
      var converter = new TimeSpanConverter();

      TimeSpan? t1 = new TimeSpan(0, 0, 0, 30, 126);
      Assert.AreEqual("30,12", converter.Convert(t1, null, null, null));
      Assert.AreEqual("0:30,12", converter.Convert(t1, null, "m", null));
      Assert.AreEqual("00:30,12", converter.Convert(t1, null, "mm", null));

      TimeSpan? t2 = new TimeSpan(0, 0, 1, 30, 126);
      Assert.AreEqual("1:30,12", converter.Convert(t2, null, null, null));
      Assert.AreEqual("01:30,12", converter.Convert(t2, null, "mm", null));

      TimeSpan? t3 = new TimeSpan(0, 1, 1, 30, 126);
      Assert.AreEqual("01:01:30,12", converter.Convert(t3, null, null, null));
    }
  }
}
