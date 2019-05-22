using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;

using DSVAlpin2Lib;
using System;

namespace DSVAlpin2LibTest
{
  [TestClass]
  public class UnitTest1
  {
    string databaseRoot = @"C:\src\DSVAlpin2\work\DSVAlpin2\SampleDatabases";

    [TestMethod]
    public void TestMethod1()
    {
      Participant p = new Participant();
      p.Year = 1900;

      Assert.AreEqual(1900, p.Year);

    }

    [TestMethod]
    public void TimeMeasurement()
    {
      TimeMeasurement t1 = new TimeMeasurement(0.000638078703703704);
      TimeSpan ts1 = t1.GetTimeSpan();
      Assert.AreEqual(new TimeSpan(0, 0, 0, 55, 130), ts1);

      TimeMeasurement t2 = new TimeMeasurement(0.000728819444444444);
      TimeSpan ts2 = t2.GetTimeSpan();
      Assert.AreEqual(new TimeSpan(0, 0, 1, 2, 970), ts2);
    }

    [TestMethod]
    public void DatabaseBasics()
    {
      DSVAlpin2Lib.Database db = new DSVAlpin2Lib.Database();
      db.Connect(Path.Combine(databaseRoot, @"KSC2019-2-PSL.mdb"));

      db.GetParticipants();

      db.Close();
    }
  }
}
