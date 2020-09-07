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
      classes.Add(new ParticipantClass("1M", null, "Class1", "M", 2009, 1));
      classes.Add(new ParticipantClass("1W", null, "Class1", "W", 2009, 2));
      classes.Add(new ParticipantClass("2M", null, "Class1", "M", 2011, 3));
      classes.Add(new ParticipantClass("2W", null, "Class1", "W", 2011, 4));

      Participant p2008M = new Participant { Year = 2008, Sex = "M" };
      Participant p2008W = new Participant { Year = 2008, Sex = "W" };
      Participant p2009M = new Participant { Year = 2009, Sex = "m" };
      Participant p2009W = new Participant { Year = 2009, Sex = "w" };
      Participant p2010M = new Participant { Year = 2010, Sex = "M" };
      Participant p2010W = new Participant { Year = 2010, Sex = "W" };
      Participant p2011M = new Participant { Year = 2011, Sex = "M" };
      Participant p2011W = new Participant { Year = 2011, Sex = "W" };
      Participant p2012M = new Participant { Year = 2012, Sex = "M" };
      Participant p2012W = new Participant { Year = 2012, Sex = "W" };

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
