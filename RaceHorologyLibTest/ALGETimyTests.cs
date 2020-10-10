using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for ALGETimyTests
  /// </summary>
  [TestClass]
  public class ALGETimyTests
  {
    public ALGETimyTests()
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
    public void Parser()
    {
      // Not really tested, because the ALGETdC8001LineParser is tested in ALGETdC8001Tests
      ALGETdC8001LineParser parser = new ALGETdC8001LineParser();
      {
        var pd = parser.Parse(" 0035 C0  21:46:36.3910 00");
        Assert.AreEqual(' ', pd.Flag);
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual("C0", pd.Channel);
        Assert.AreEqual(' ', pd.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 36, 391), pd.Time);
      }
    }


    [TestMethod, TestCategory("HardwareDependent")]
    public void RetrieveTimingData()
    {
      string comport = "COM4";

      ALGETimy timy = new ALGETimy(comport);

      timy.Connect();
      timy.StartGetTimingData();

      foreach(var t in timy.TimingData())
      {
        TestContext.WriteLine(t.Time.ToString());
      }

    }
  }
}
