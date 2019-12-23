using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using RaceHorologyLib;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for UtilitiesTest
  /// </summary>
  [TestClass]
  public class UtilitiesTest
  {
    public UtilitiesTest()
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

    public class TestClass
    {
      public int Attr1 { get; set; }

      public TestClass ShallowCopy()
      {
        return (TestClass)this.MemberwiseClone();
      }
    }


    [TestMethod]
    public void CopyObservableCollectionTest()
    {
      ObservableCollection<TestClass> sc = new ObservableCollection<TestClass>();

      CopyObservableCollection<TestClass> coc = new CopyObservableCollection<TestClass>(sc, item => item.ShallowCopy());

      // Test empty
      Assert.AreEqual(0, coc.Count);

      // Test add
      sc.Add(new TestClass { Attr1 = 10 });
      Assert.AreEqual(sc.Count, coc.Count);
      Assert.AreEqual(10, coc[0].Attr1);

      // Test that cloner is used
      Assert.AreNotSame(sc[0], coc[0]);


      // Test insert at front
      sc.Insert(0, new TestClass { Attr1 = 20 });
      Assert.AreEqual(sc.Count, coc.Count);
      Assert.AreEqual(20, coc[0].Attr1);
      Assert.AreEqual(10, coc[1].Attr1);

      // Test insert at middle
      sc.Insert(1, new TestClass { Attr1 = 30 });
      Assert.AreEqual(sc.Count, coc.Count);
      Assert.AreEqual(20, coc[0].Attr1);
      Assert.AreEqual(30, coc[1].Attr1);
      Assert.AreEqual(10, coc[2].Attr1);

      // Test initialize with elements
      {
        CopyObservableCollection<TestClass> coc2 = new CopyObservableCollection<TestClass>(sc, item => item.ShallowCopy());
        Assert.AreEqual(sc.Count, coc2.Count);
        Assert.AreEqual(20, coc2[0].Attr1);
        Assert.AreEqual(30, coc2[1].Attr1);
        Assert.AreEqual(10, coc2[2].Attr1);
      }

      // Test Remove
      sc.RemoveAt(1);
      Assert.AreEqual(sc.Count, coc.Count);
      Assert.AreEqual(20, coc[0].Attr1);
      Assert.AreEqual(10, coc[1].Attr1);

      // Test move
      sc.Move(0, 1);
      Assert.AreEqual(sc.Count, coc.Count);
      Assert.AreEqual(10, coc[0].Attr1);
      Assert.AreEqual(20, coc[1].Attr1);

      sc.Clear();
      Assert.AreEqual(0, sc.Count);
      Assert.AreEqual(0, coc.Count);
    }
  }
}
