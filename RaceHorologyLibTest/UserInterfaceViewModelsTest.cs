using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for UserInterfaceViewModelsTest
  /// </summary>
  [TestClass]
  public class UserInterfaceViewModelsTest
  {
    public UserInterfaceViewModelsTest()
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

    [TestMethod]
    public void DiqualifyVM()
    {
      TestDataGenerator tg = new TestDataGenerator();

      tg.createRaceParticipants(10);

      var race = tg.Model.GetRace(0);
      var run = race.GetRun(0);

      var disqualifyVM = new DiqualifyVM(run);

      // Test whether all participants are part of disqualify
      Assert.AreEqual(10, disqualifyVM.GetGridView().Count);
      foreach (var rr in disqualifyVM.GetGridView())
        Assert.IsNull(rr.Runtime);

      // Test for updating RunResult
      {
        run.SetRunTime(race.GetParticipant(3), new TimeSpan(0, 1, 3));
        var rr = disqualifyVM.GetGridView().First(r => r.StartNumber == 3);

        Assert.AreEqual(new TimeSpan(0, 1, 3), rr.Runtime);
      }

      // Test for delete RunResult
      {
        run.DeleteRunResult(race.GetParticipant(3));
        var rr = disqualifyVM.GetGridView().First(r => r.StartNumber == 3);
        Assert.IsNull(rr.Runtime);
      }
    }
  }
}
