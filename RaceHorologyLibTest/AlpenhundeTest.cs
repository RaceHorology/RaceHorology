using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for AlpenhundeTest
  /// </summary>
  [TestClass]
  public class AlpenhundeTest
  {
    public AlpenhundeTest()
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
    public void ParseTest()
    {
      AlpenhundeParser parser = new AlpenhundeParser();
      var o = parser.ParseMessage("{ 'type': 'timestamp', 'data': { 'i': 876, 'c': 1, 'n': '2', 't': '10:12:30.1234' } }");
      Assert.AreEqual("timestamp", o.type);
      Assert.AreEqual(876, o.data.i);
      Assert.AreEqual(1, o.data.c);
      Assert.AreEqual("2", o.data.n);
      Assert.AreEqual("10:12:30.1234", o.data.t);
    }

    [TestMethod]
    public void ConvertToTimestamp()
    {
      AlpenhundeParser parser = new AlpenhundeParser();
      { // Start Case
        var o = parser.ParseMessage("{ 'type': 'timestamp', 'data': { 'i': 876, 'c': 1, 'n': '2', 't': '10:12:30.1230' } }");
        var t = TimingDeviceAlpenhunde.ConvertToTimemeasurementData(o.data);
        Assert.AreEqual(2U, t.StartNumber);
        Assert.AreEqual(new TimeSpan(0, 10, 12, 30, 123), t.StartTime);
        Assert.IsTrue(t.BStartTime);
        Assert.IsFalse(t.BRunTime);
        Assert.IsFalse(t.BFinishTime);
      }
      { // Finish Case
        var o = parser.ParseMessage("{ 'type': 'timestamp', 'data': { 'i': 876, 'c': 128, 'n': '2', 't': '10:12:30.1230' } }");
        var t = TimingDeviceAlpenhunde.ConvertToTimemeasurementData(o.data);
        Assert.AreEqual(2U, t.StartNumber);
        Assert.AreEqual(new TimeSpan(0, 10, 12, 30, 123), t.FinishTime);
        Assert.IsTrue(t.BFinishTime);
        Assert.IsFalse(t.BRunTime);
        Assert.IsFalse(t.BStartTime);
      }
      { // Intermediate Case
        var o = parser.ParseMessage("{ 'type': 'timestamp', 'data': { 'i': 876, 'c': 2, 'n': '2', 't': '10:12:30.1230' } }");
        var t = TimingDeviceAlpenhunde.ConvertToTimemeasurementData(o.data);
        Assert.IsNull(t); // Not supported
      }
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\Alpenhunde\events_01.json")]
    public void ParseEventsJson()
    {
      var eventsJson = System.IO.File.ReadAllText(@"events_01.json");

      AlpenhundeParser parser = new AlpenhundeParser();
      var data = parser.ParseEvents(eventsJson);

      Assert.AreEqual(5, data.Count);
     
      Assert.AreEqual(7, data[0].i);
      Assert.AreEqual(1, data[0].c);
      Assert.AreEqual("1", data[0].n);
      Assert.AreEqual("20:01:13.3432", data[0].t);

      Assert.AreEqual(8, data[1].i);
      Assert.AreEqual(1, data[1].c);
      Assert.AreEqual("abc2", data[1].n);
      Assert.AreEqual("20:01:27.5397", data[1].t);

      Assert.AreEqual(7, data[2].i);
      Assert.AreEqual(128, data[2].c);
      Assert.AreEqual("1", data[2].n);
      Assert.AreEqual("20:01:41.4285", data[2].t);

      Assert.AreEqual(8, data[3].i);
      Assert.AreEqual(128, data[3].c);
      Assert.AreEqual("abc2", data[3].n);
      Assert.AreEqual("20:01:59.7163", data[3].t);

      Assert.AreEqual(-1, data[4].i);
      Assert.AreEqual(128, data[4].c);
      Assert.AreEqual("", data[4].n);
      Assert.AreEqual("00:47:34.9711", data[4].t);

    }
  }
}
