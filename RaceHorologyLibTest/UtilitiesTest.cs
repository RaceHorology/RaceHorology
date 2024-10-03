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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for UtilitiesTest
  /// </summary>
  [TestClass]
  public class UtilitiesTest
  {
    public UtilitiesTest()
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

    public class TestClass
    {
      public int Attr1 { get; set; }

      public TestClass ShallowCopy()
      {
        return (TestClass)this.MemberwiseClone();
      }
    }


    [TestMethod]
    public void CopyObservableCollectionTest()
    {
      ObservableCollection<TestClass> sc = new ObservableCollection<TestClass>();

      CopyObservableCollection<TestClass, TestClass> coc = new CopyObservableCollection<TestClass, TestClass>(sc, item => item.ShallowCopy(), true);

      // Test empty
      Assert.AreEqual(0, coc.Count);

      // Test add
      sc.Add(new TestClass { Attr1 = 10 });
      Assert.AreEqual(sc.Count, coc.Count);
      Assert.AreEqual(10, coc[0].Attr1);

      // Test that cloner is used
      Assert.AreNotSame(sc[0], coc[0]);


      // Test insert at front
      sc.Insert(0, new TestClass { Attr1 = 20 });
      Assert.AreEqual(sc.Count, coc.Count);
      Assert.AreEqual(20, coc[0].Attr1);
      Assert.AreEqual(10, coc[1].Attr1);

      // Test insert at middle
      sc.Insert(1, new TestClass { Attr1 = 30 });
      Assert.AreEqual(sc.Count, coc.Count);
      Assert.AreEqual(20, coc[0].Attr1);
      Assert.AreEqual(30, coc[1].Attr1);
      Assert.AreEqual(10, coc[2].Attr1);

      // Test initialize with elements
      {
        CopyObservableCollection<TestClass, TestClass> coc2 = new CopyObservableCollection<TestClass, TestClass>(sc, item => item.ShallowCopy(), true);
        Assert.AreEqual(sc.Count, coc2.Count);
        Assert.AreEqual(20, coc2[0].Attr1);
        Assert.AreEqual(30, coc2[1].Attr1);
        Assert.AreEqual(10, coc2[2].Attr1);
      }

      // Test Remove
      sc.RemoveAt(1);
      Assert.AreEqual(sc.Count, coc.Count);
      Assert.AreEqual(20, coc[0].Attr1);
      Assert.AreEqual(10, coc[1].Attr1);

      // Test move
      sc.Move(0, 1);
      Assert.AreEqual(sc.Count, coc.Count);
      Assert.AreEqual(10, coc[0].Attr1);
      Assert.AreEqual(20, coc[1].Attr1);

      sc.Clear();
      Assert.AreEqual(0, sc.Count);
      Assert.AreEqual(0, coc.Count);
    }


    [TestMethod]
    public void ObservableCollectionExtensions_Sort()
    {
      Collection<int> collection = new Collection<int> { 2, 3, 1, 8, 7, 9, 4, 6, 5 };

      collection.Sort(Comparer<int>.Default, 0, 2);
      CollectionAssert.AreEqual(new Collection<int> { 1, 2, 3, 8, 7, 9, 4, 6, 5 }, collection);

      collection.Sort(Comparer<int>.Default, 6, 9);
      CollectionAssert.AreEqual(new Collection<int> { 1, 2, 3, 8, 7, 9, 4, 5, 6 }, collection);

      collection.Sort(Comparer<int>.Default);
      CollectionAssert.AreEqual(new Collection<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, collection);
    }



    [TestMethod]
    public void ToRaceTimeStringTest()
    {
      TimeSpan? t1 = new TimeSpan(0, 0, 0, 30, 126);
      Assert.AreEqual("30,12", t1.ToRaceTimeString());
      Assert.AreEqual("30,13", t1.ToRaceTimeString(roundType: RoundedTimeSpan.ERoundType.Round));
      Assert.AreEqual("0:30,12", t1.ToRaceTimeString(formatString: "m"));
      Assert.AreEqual("00:30,12", t1.ToRaceTimeString(formatString: "mm"));

      TimeSpan? t1n = new TimeSpan(((TimeSpan)t1).Ticks * -1);
      Assert.AreEqual("-30,12", t1n.ToRaceTimeString());
      Assert.AreEqual("-30,13", t1n.ToRaceTimeString(roundType: RoundedTimeSpan.ERoundType.Round));


      TimeSpan? t2 = new TimeSpan(0, 0, 1, 30, 126);
      Assert.AreEqual("1:30,12", t2.ToRaceTimeString());
      Assert.AreEqual("01:30,12", t2.ToRaceTimeString(formatString: "mm"));

      TimeSpan? t3 = new TimeSpan(0, 1, 1, 30, 126);
      Assert.AreEqual("01:01:30,12", t3.ToRaceTimeString());

    }

    [TestMethod]
    public void UnixTimeTest()
    {
      var t1 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
      var offset1 = TimeZoneInfo.Local.GetUtcOffset(t1);
      Assert.AreEqual(-offset1.TotalSeconds, t1.UnixEpoch(false));
      Assert.AreEqual(0, t1.UnixEpoch(true));

      var t2Local = new DateTime(2024, 9, 15, 0, 0, 0, 0, DateTimeKind.Local);
      var t2UTC = new DateTime(2024, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc);
      var t2UTCEpoch = 1726358400;
      var t2UTCLocal = 1726351200;


      var offset2 = TimeZoneInfo.Local.GetUtcOffset(t2Local);
      Assert.AreEqual(t2UTCLocal, t2Local.UnixEpoch(false));
      Assert.AreEqual(t2UTCLocal + offset2.TotalSeconds, t2Local.UnixEpoch(true));

      Assert.AreEqual(t2UTCEpoch, t2UTC.UnixEpoch(false));
      Assert.AreEqual(t2UTCEpoch + offset2.TotalSeconds, t2UTC.UnixEpoch(true));

      Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), DateTimeExtensions.ConvertFromUnixTimestamp(0, false));
      Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), DateTimeExtensions.ConvertFromUnixTimestamp(0, true));
    }
  }
}
