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
  /// Summary description for ClassAssignmentTest
  /// </summary>
  [TestClass]
  public class AppDataModelCalculationsTest
  {
    public AppDataModelCalculationsTest()
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
    public void ClassAssignmentTest()
    {
      List<ParticipantClass> classes = new List<ParticipantClass>();
      classes.Add(new ParticipantClass("1M", null, "Class1", new ParticipantCategory('M'), 2009, 1));
      classes.Add(new ParticipantClass("1W", null, "Class1", new ParticipantCategory('W'), 2009, 2));
      classes.Add(new ParticipantClass("2M", null, "Class1", new ParticipantCategory('M'), 2011, 3));
      classes.Add(new ParticipantClass("2W", null, "Class1", new ParticipantCategory('W'), 2011, 4));

      Participant p2008M = new Participant { Year = 2008, Sex = new ParticipantCategory('M') };
      Participant p2008W = new Participant { Year = 2008, Sex = new ParticipantCategory('W') };
      Participant p2009M = new Participant { Year = 2009, Sex = new ParticipantCategory('M') };
      Participant p2009W = new Participant { Year = 2009, Sex = new ParticipantCategory('W') };
      Participant p2010M = new Participant { Year = 2010, Sex = new ParticipantCategory('M') };
      Participant p2010W = new Participant { Year = 2010, Sex = new ParticipantCategory('W') };
      Participant p2011M = new Participant { Year = 2011, Sex = new ParticipantCategory('M') };
      Participant p2011W = new Participant { Year = 2011, Sex = new ParticipantCategory('W') };
      Participant p2012M = new Participant { Year = 2012, Sex = new ParticipantCategory('M') };
      Participant p2012W = new Participant { Year = 2012, Sex = new ParticipantCategory('W') };

      ClassAssignment ca = new ClassAssignment(classes);

      // Test ClassAssignment.DetermineClass
      Assert.AreEqual("1M", ca.DetermineClass(p2008M).Id);
      Assert.AreEqual("1W", ca.DetermineClass(p2008W).Id);
      Assert.AreEqual("1M", ca.DetermineClass(p2009M).Id);
      Assert.AreEqual("1W", ca.DetermineClass(p2009W).Id);
      Assert.AreEqual("2M", ca.DetermineClass(p2010M).Id);
      Assert.AreEqual("2W", ca.DetermineClass(p2010W).Id);
      Assert.AreEqual("2M", ca.DetermineClass(p2011M).Id);
      Assert.AreEqual("2W", ca.DetermineClass(p2011W).Id);
      Assert.IsNull(ca.DetermineClass(p2012M));
      Assert.IsNull(ca.DetermineClass(p2012W));


      // Test ClassAssignment.Assign
      List<Participant> participants = new List<Participant>();
      participants.Add(p2008M);
      participants.Add(p2008W);
      participants.Add(p2009M);
      participants.Add(p2009W);
      participants.Add(p2010M);
      participants.Add(p2010W);
      participants.Add(p2011M);
      participants.Add(p2011W);
      participants.Add(p2012M);
      participants.Add(p2012W);
      ca.Assign(participants);
      foreach (var p in participants)
        Assert.AreEqual(ca.DetermineClass(p), p.Class);
    }
  }
}
