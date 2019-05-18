using Microsoft.VisualStudio.TestTools.UnitTesting;

using DSVAlpin2Lib;

namespace DSVAlpin2LibTest
{
  [TestClass]
  public class UnitTest1
  {
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
      db.Connect("test_open.mdb");
      db.Close();
    }
  }
}
