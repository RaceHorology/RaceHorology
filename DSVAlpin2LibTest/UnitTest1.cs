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

      TimeSpan ts3 = new TimeSpan(0, 0, 1, 55, 130);
      string s3 = ts3.ToString(@"mm\:s\,ff");
    }




  }
}
