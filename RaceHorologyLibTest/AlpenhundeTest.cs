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
  }
}
