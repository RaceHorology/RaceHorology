using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSVAlpin2Lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DSVAlpin2LibTest
{
  [TestClass]
  public class ALGETdC8001Tests
  {
    public ALGETdC8001Tests()
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
    public void ParserTest()
    {
      ALGETdC8001LineParser parser = new ALGETdC8001LineParser();

      {
        var pd = parser.Parse(" 0035 C0M 21:46:36.3900 00");
        Assert.AreEqual(' ', pd.Flag);
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual("C0", pd.Channel);
        Assert.AreEqual('M', pd.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 36, 390), pd.Time);
      }

      {
        var pd = parser.Parse(" 0035 C0  21:46:36.3910 00");
        Assert.AreEqual(' ', pd.Flag);
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual("C0", pd.Channel);
        Assert.AreEqual(' ', pd.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 36, 391), pd.Time);
      }

      {
        var pd = parser.Parse("?0034 C1M 21:46:48.3300 00");
        Assert.AreEqual('?', pd.Flag);
        Assert.AreEqual(34U, pd.StartNumber);
        Assert.AreEqual("C1", pd.Channel);
        Assert.AreEqual('M', pd.ChannelModifier);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 330), pd.Time);
      }

      {
        var pd = parser.Parse("n0034");
        Assert.AreEqual('n', pd.Flag);
        Assert.AreEqual(34U, pd.StartNumber);
        Assert.AreEqual("", pd.Channel);
        Assert.AreEqual(' ', pd.ChannelModifier);
        Assert.AreEqual(new TimeSpan(), pd.Time);
      }

      {
        var pd = parser.Parse("?0034 C1M 21:46:48.1230 00");
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 123), pd.Time);
      }
      {
        var pd = parser.Parse("?0034 C1M 21:46:48.123  00");
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 123), pd.Time);
      }
      {
        var pd = parser.Parse("?0034 C1M 21:46:48.12   00");
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 120), pd.Time);
      }
      {
        var pd = parser.Parse("?0034 C1M 21:46:48.1    00");
        Assert.AreEqual(new TimeSpan(0, 21, 46, 48, 100), pd.Time);
      }

    }

    [TestMethod]
    public void ParserAndTransferToTimemeasurementDataTest()
    {

      TimeMeasurementEventArgs ParseAndTransfer(string line)
      {
        ALGETdC8001LineParser parser = new ALGETdC8001LineParser();
        return ALGETdC8001TimeMeasurement.TransferToTimemeasurementData(parser.Parse(line));
      }

      { 
        var pd = ParseAndTransfer(" 0035 C0M 21:46:36.3900 00");
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 36, 390), pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
      }

      {
        var pd = ParseAndTransfer(" 0035 C0  21:46:36.3910 00");
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(new TimeSpan(0, 21, 46, 36, 391), pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
      }

      { // Disqualified
        var pd = ParseAndTransfer("d0035 C0  21:46:36.3910 00");
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(null, pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
      }
      { // Cleared data
        var pd = ParseAndTransfer("c0035 C0  21:46:36.3910 00");
        Assert.AreEqual(35U, pd.StartNumber);
        Assert.AreEqual(true, pd.BStartTime);
        Assert.AreEqual(null, pd.StartTime);
        Assert.AreEqual(false, pd.BFinishTime);
        Assert.AreEqual(false, pd.BRunTime);
      }

      // Ignored data
      {
        var pd = ParseAndTransfer("?0034 C1M 21:46:48.3300 00");
        Assert.IsNull(pd);
      }
      {
        var pd = ParseAndTransfer("p0034 C1M 21:46:48.3300 00");
        Assert.IsNull(pd);
      }
      {
        var pd = ParseAndTransfer("b0034 C1M 21:46:48.3300 00");
        Assert.IsNull(pd);
      }
      {
        var pd = ParseAndTransfer("m0034 C1M 21:46:48.3300 00");
        Assert.IsNull(pd);
      }
      {
        var pd = ParseAndTransfer("n0034");
        Assert.IsNull(pd);
      }
    }

  }
}
