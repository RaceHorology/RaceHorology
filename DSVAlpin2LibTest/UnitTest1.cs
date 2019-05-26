using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;

using DSVAlpin2Lib;
using System;

namespace DSVAlpin2LibTest
{


  [TestClass]
  public class UnitTest1
  {
    public TestContext TestContext
    {
      get { return _testContext; }
      set { _testContext = value; }
    }

    private TestContext _testContext;

    [TestMethod]
    public void TestMethod1()
    {
      Participant p = new Participant();
      p.Year = 1900;

      Assert.AreEqual(1900, p.Year);

    }


    [TestMethod]
    public void TimeSpanAndFractions()
    {
      const double f1 = 0.000638078703703704;
      TimeSpan ts1 = DSVAlpin2Lib.Database.CreateTimeSpan(f1);
      Assert.AreEqual(new TimeSpan(0, 0, 0, 55, 130), ts1);
      TimeSpan ts2 = DSVAlpin2Lib.Database.CreateTimeSpan(DSVAlpin2Lib.Database.FractionForTimeSpan(ts1));
      Assert.AreEqual(ts1, ts2);
    }


    [TestMethod]
    //[DeploymentItem(@"TestDataBases\KSC2019-2-PSL.mdb")]
    [DeploymentItem(@"TestDataBases\Kirchberg U8 U10 10.02.19 RS Neu.mdb")]
    public void DatabaseBasics()
    {
      DSVAlpin2Lib.Database db = new DSVAlpin2Lib.Database();
      //db.Connect(Path.Combine(_testContext.TestDeploymentDir, @"KSC2019-2-PSL.mdb"));
      db.Connect(Path.Combine(_testContext.TestDeploymentDir, @"Kirchberg U8 U10 10.02.19 RS Neu.mdb"));

      db.GetParticipants();

      db.Close();
    }

    [TestMethod]
    //[DeploymentItem(@"TestDataBases\KSC2019-2-PSL.mdb")]
    [DeploymentItem(@"TestDataBases\Kirchberg U8 U10 10.02.19 RS Neu.mdb")]
    public void DatabaseRaceRuns()
    {
      DSVAlpin2Lib.Database db = new DSVAlpin2Lib.Database();
      //db.Connect(Path.Combine(_testContext.TestDeploymentDir, @"KSC2019-2-PSL.mdb"));
      db.Connect(Path.Combine(_testContext.TestDeploymentDir, @"Kirchberg U8 U10 10.02.19 RS Neu.mdb"));

      db.GetParticipants();
      RaceRun rr1 = db.GetRaceRun(1);
      RaceRun rr2 = db.GetRaceRun(2);

      db.Close();
    }

  }
}
