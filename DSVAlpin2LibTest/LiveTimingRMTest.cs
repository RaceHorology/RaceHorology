using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DSVAlpin2Lib;

namespace DSVAlpin2LibTest
{
  /// <summary>
  /// Summary description for LiveTimingRMTest
  /// </summary>
  [TestClass]
  public class LiveTimingRMTest
  {
    public LiveTimingRMTest()
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
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    [DeploymentItem(@"3rdparty\DSVAlpinX.liz", "3rdparty")]
    public void TestMethod1()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants.mdb");
      DSVAlpin2Lib.Database db = new DSVAlpin2Lib.Database();
      db.Connect(dbFilename);
      AppDataModel model = new AppDataModel(db);


      LiveTimingRM cl = new LiveTimingRM(model, "01122", "livetiming", "livetiming");
      cl.Init();

      //
      // TODO: Add test logic here
      //
    }
  }
}
