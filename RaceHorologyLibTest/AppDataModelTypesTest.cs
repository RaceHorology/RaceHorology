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

using RaceHorologyLib;
using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for AppDataModelTypesTest
  /// </summary>
  [TestClass]
  public class AppDataModelTypesTest
  {
    public AppDataModelTypesTest()
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
    public void Category()
    {
      ParticipantCategory c1 = new ParticipantCategory();
      Assert.AreEqual(char.MinValue, c1.Name);
      Assert.IsNull(c1.Synonyms);
      Assert.AreEqual("", c1.PrettyName);
      Assert.AreEqual(uint.MaxValue, c1.SortPos);

      ParticipantCategory c2 = new ParticipantCategory('W');
      Assert.AreEqual('W', c2.Name);
      Assert.IsNull(c2.Synonyms);
      Assert.AreEqual("W", c2.PrettyName);
      Assert.AreEqual(uint.MaxValue, c2.SortPos);

      ParticipantCategory c3 = new ParticipantCategory('W', "Weiblich", 1);
      Assert.AreEqual('W', c3.Name);
      Assert.IsNull(c3.Synonyms);
      Assert.AreEqual("Weiblich", c3.PrettyName);
      Assert.AreEqual(1U, c3.SortPos);

      ParticipantCategory c4 = new ParticipantCategory('M', "Männlich", 2);
      Assert.AreEqual('M', c4.Name);
      Assert.IsNull(c4.Synonyms);
      Assert.AreEqual("Männlich", c4.PrettyName);
      Assert.AreEqual(2U, c4.SortPos);

      ParticipantCategory c5 = new ParticipantCategory('M', "Männlich", 2, "mHh");
      Assert.AreEqual('M', c5.Name);
      Assert.AreEqual("mHh", c5.Synonyms);
      Assert.AreEqual("Männlich", c5.PrettyName);
      Assert.AreEqual(2U, c5.SortPos);


      Assert.IsTrue(c1 != c2);
      Assert.IsTrue(c3 == c2);
      Assert.IsTrue(c3 != c4);
      Assert.IsTrue(c3.GetHashCode() == c2.GetHashCode());

      Assert.IsTrue(c3.CompareTo(c4) == -1);
      Assert.IsTrue(c4.CompareTo(c3) ==  1);
      Assert.IsTrue(c3.CompareTo(c3) ==  0);
    }


    [TestMethod]
    public void Participant_IsEqualTo()
    {
      var sex1 = new ParticipantCategory('M');
      var sex2 = new ParticipantCategory('W');
      var class1 = new ParticipantClass("c1", null, "Class 1", sex1, 2000, 0);
      var class2 = new ParticipantClass("c2", null, "Class 1", sex2, 2000, 0);

      Participant p1 = new Participant
      {
        Name = "Name",
        Firstname = "Firstname",
        Sex = sex1,
        Year = 2000,
        Club = "Club",
        SvId = "SvId",
        Code = "Code",
        Nation = "Nation",
        Class = class1
      };

      Participant p2 = new Participant
      {
        Name = "Name",
        Firstname = "Firstname",
        Sex = sex1,
        Year = 2000,
        Club = "Club",
        SvId = "SvId",
        Code = "Code",
        Nation = "Nation",
        Class = class1
      };

      void performCheck()
      {
        Assert.IsFalse(p1.IsEqualTo(p2));
        p2.Assign(p1); 
        Assert.IsTrue(p1.IsEqualTo(p2));
      }

      Assert.IsTrue(p1.IsEqualTo(p2));

      p2.Name = "name"; performCheck();
      p2.Firstname = "fname"; performCheck();
      p2.Sex = sex2; performCheck();
      p2.Year = 1900; performCheck();
      p2.Club = "c"; performCheck();
      p2.SvId = "xyz"; performCheck();
      p2.Code = "xyz"; performCheck();
      p2.Nation = "xyz"; performCheck();
      p2.Class = class2; performCheck();
      p2.Class = null; performCheck();
    }



    [TestMethod]
    public void RunResultExtension()
    {
      string s1 = RaceHorologyLib.RunResultExtension.JoinDisqualifyText("Vorbei am Tor", "12");
      Assert.AreEqual("Vorbei am Tor 12", s1);

      {
        string r, g;
        RaceHorologyLib.RunResultExtension.SplitDisqualifyText("Vorbei am Tor 12", out r, out g);
        Assert.AreEqual("Vorbei am Tor", r);
        Assert.AreEqual("12", g);
      }
      {
        string r, g;
        RaceHorologyLib.RunResultExtension.SplitDisqualifyText("Vorbei am Tor 12 aaa", out r, out g);
        Assert.AreEqual("Vorbei am Tor 12 aaa", r);
        Assert.AreEqual("", g);
      }
      {
        string r, g;
        RaceHorologyLib.RunResultExtension.SplitDisqualifyText("12", out r, out g);
        Assert.AreEqual("", r);
        Assert.AreEqual("12", g);
      }
      {
        string r, g;
        RaceHorologyLib.RunResultExtension.SplitDisqualifyText("Vorbei am Tor 12 aaa 13", out r, out g);
        Assert.AreEqual("Vorbei am Tor 12 aaa", r);
        Assert.AreEqual("13", g);
      }

      RunResult rr = new RunResult(null);
      rr.DisqualText = "Vorbei am Tor 12";
      Assert.AreEqual("Vorbei am Tor", rr.GetDisqualifyText());
      Assert.AreEqual("12", rr.GetDisqualifyGoal());

      // Error Conditions
      {
        string r, g;
        RaceHorologyLib.RunResultExtension.SplitDisqualifyText("", out r, out g);
        Assert.AreEqual("", r);
        Assert.AreEqual("", g);
      }
      {
        string r, g;
        RaceHorologyLib.RunResultExtension.SplitDisqualifyText(null, out r, out g);
        Assert.AreEqual("", r);
        Assert.AreEqual("", g);
      }


    }

  }
}
