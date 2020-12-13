using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for ClassesGroupsCategoriesTest
  /// </summary>
  [TestClass]
  public class ClassesGroupsCategoriesTest
  {
    public ClassesGroupsCategoriesTest()
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
    public void CategoryVMTest()
    {
      CategoryVM c1 = new CategoryVM();

      List<ParticipantCategory> l1 = new List<ParticipantCategory>
      {
        new ParticipantCategory('M', "Männlich", 0)
      };
      List<ParticipantCategory> l2 = new List<ParticipantCategory>
      {
        new ParticipantCategory('W', "Weiblich", 1)
      };
      List<ParticipantCategory> l12 = new List<ParticipantCategory>
      {
        new ParticipantCategory('M', "Männlich", 0),
        new ParticipantCategory('W', "Weiblich", 1)
      };


      c1.Assign(l1);
      Assert.AreEqual(1, c1.Items.Count);
      Assert.AreEqual('M', c1.Items[0].Name);

      c1.Add(l2);
      Assert.AreEqual(2, c1.Items.Count);
      Assert.AreEqual('M', c1.Items[0].Name);
      Assert.AreEqual('W', c1.Items[1].Name);

      c1.Assign(l1);
      Assert.AreEqual(1, c1.Items.Count);
      Assert.AreEqual('M', c1.Items[0].Name);

      c1.Assign(l12);
      Assert.AreEqual(2, c1.Items.Count);
      Assert.AreEqual('M', c1.Items[0].Name);
      Assert.AreEqual('W', c1.Items[1].Name);

      c1.Assign(l2);
      c1.Merge(l12);
      Assert.AreEqual(2, c1.Items.Count);
      Assert.AreEqual('M', c1.Items[0].Name);
      Assert.AreEqual('W', c1.Items[1].Name);

      c1.Clear();
      Assert.AreEqual(0, c1.Items.Count);
    }
  }
}
