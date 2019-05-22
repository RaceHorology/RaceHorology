using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;

using DSVAlpin2Lib;

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
    public void DatabaseBasics()
    {
      DSVAlpin2Lib.Database db = new DSVAlpin2Lib.Database();
      db.Connect(Path.Combine(databaseRoot, @"KSC2019-2-PSL.mdb"));

      db.GetParticipants();

      db.Close();
    }
  }
}
