using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for TagHeuerTests
  /// </summary>
  [TestClass]
  public class TagHeuerTests
  {
    public TagHeuerTests()
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
    public void Parse()
    {
      TagHeuerParser parser = new TagHeuerParser();

      {
        var r = parser.ParseRR("\nRR 0010 0232   05:27:51.01040\t");
        Assert.AreEqual(10, r.Rank);
        Assert.AreEqual(232, r.Number);
        Assert.AreEqual(new TimeSpan(0, 5, 27, 51, 10).AddMicroseconds(400), r.Time);
      }

      {
        var r = parser.ParseSynchroTime("\n!T 08:14:00 01/03/20\t");
        Assert.AreEqual(new DateTime(2020,3,1,8,14,0), r);
      }
    }

    [TestMethod, TestCategory("HardwareDependent")]
    public void RetrieveTimingData()
    {
      string comport = "COM6";

      TagHeuer tagHeuer = new TagHeuer(comport);

      tagHeuer.Connect();
      tagHeuer.StartGetTimingData();

      foreach (var t in tagHeuer.TimingData())
      {
        TestContext.WriteLine(t.Time.ToString());
      }
    }
  }
}
