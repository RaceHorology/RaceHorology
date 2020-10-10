using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for HandTimingTest
  /// </summary>
  [TestClass]
  public class HandTimingTest
  {
    public HandTimingTest()
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
    [DeploymentItem(@"TestDataBases\HandTime\--Handzeit-Start.txt")]
    public void ReadFromFile()
    {

      FromFileHandTiming ht = new FromFileHandTiming(@"--Handzeit-Start.txt");
      ht.Connect();

      TimeSpan[] shallTime =
      {
        new TimeSpan(0, 8, 48, 0, 570),
        new TimeSpan(0, 9, 32, 56, 300)
      };

      int i = 0;
      foreach (var t in ht.TimingData())
      {
        if (i < shallTime.Length)
          Assert.AreEqual(shallTime[i], t.Time);

        TestContext.WriteLine(t.Time.ToString());

        i++;
      }
    }

    [TestMethod]
    public void CreateHandTiming()
    {
      Assert.AreEqual(typeof(FromFileHandTiming), HandTiming.CreateHandTiming("File", "abc").GetType());
      Assert.AreEqual(typeof(TagHeuer), HandTiming.CreateHandTiming("TagHeuerPPro", "abc").GetType());
      Assert.AreEqual(typeof(ALGETimy), HandTiming.CreateHandTiming("ALGETimy", "abc").GetType());
    }

  }
}
