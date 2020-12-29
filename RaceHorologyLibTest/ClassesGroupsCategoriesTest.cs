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


    [TestMethod]
    public void GroupVMTest()
    {
      GroupVM g1 = new GroupVM();

      List<ParticipantGroup> l1 = new List<ParticipantGroup>
      {
        new ParticipantGroup("1", "G_U10", 0)
      };
      List<ParticipantGroup> l2 = new List<ParticipantGroup>
      {
        new ParticipantGroup("2", "G_U12", 0)
      };
      List<ParticipantGroup> l12 = new List<ParticipantGroup>
      {
        new ParticipantGroup("1", "G_U10", 0),
        new ParticipantGroup("2", "G_U12", 1)
      };


      g1.Assign(l1);
      Assert.AreEqual(1, g1.Items.Count);
      Assert.AreEqual("G_U10", g1.Items[0].Name);

      g1.Add(l2);
      Assert.AreEqual(2, g1.Items.Count);
      Assert.AreEqual("G_U10", g1.Items[0].Name);
      Assert.AreEqual("G_U12", g1.Items[1].Name);

      g1.Assign(l1);
      Assert.AreEqual(1, g1.Items.Count);
      Assert.AreEqual("G_U10", g1.Items[0].Name);

      g1.Assign(l12);
      Assert.AreEqual(2, g1.Items.Count);
      Assert.AreEqual("G_U10", g1.Items[0].Name);
      Assert.AreEqual("G_U12", g1.Items[1].Name);

      g1.Assign(l2);
      g1.Merge(l12);
      Assert.AreEqual(2, g1.Items.Count);
      Assert.AreEqual("G_U10", g1.Items[0].Name);
      Assert.AreEqual("G_U12", g1.Items[1].Name);

      g1.Clear();
      Assert.AreEqual(0, g1.Items.Count);
    }




    [TestMethod]
    public void ClassVMTest()
    {
      ClassVM c1 = new ClassVM();

      List<ParticipantClass> l1 = new List<ParticipantClass>
      {
        new ParticipantClass("1", null, "U10", null, 2010, 0)
      };
      List<ParticipantClass> l2 = new List<ParticipantClass>
      {
        new ParticipantClass("2", null, "U12", null, 2008, 0)
      };
      List<ParticipantClass> l12 = new List<ParticipantClass>
      {
        new ParticipantClass("1", null, "U10", null, 2010, 0),
        new ParticipantClass("2", null, "U12", null, 2008, 1)
      };


      c1.Assign(l1);
      Assert.AreEqual(1, c1.Items.Count);
      Assert.AreEqual("U10", c1.Items[0].Name);

      c1.Add(l2);
      Assert.AreEqual(2, c1.Items.Count);
      Assert.AreEqual("U10", c1.Items[0].Name);
      Assert.AreEqual("U12", c1.Items[1].Name);

      c1.Assign(l1);
      Assert.AreEqual(1, c1.Items.Count);
      Assert.AreEqual("U10", c1.Items[0].Name);

      c1.Assign(l12);
      Assert.AreEqual(2, c1.Items.Count);
      Assert.AreEqual("U10", c1.Items[0].Name);
      Assert.AreEqual("U12", c1.Items[1].Name);

      c1.Assign(l2);
      c1.Merge(l12);
      Assert.AreEqual(2, c1.Items.Count);
      Assert.AreEqual("U10", c1.Items[0].Name);
      Assert.AreEqual("U12", c1.Items[1].Name);

      c1.Clear();
      Assert.AreEqual(0, c1.Items.Count);
    }

    [TestMethod]
    public void CgcTest_Load()
    {
      AppDataModel dm = createTestDataModel1();

      ClassesGroupsCategoriesEditVM cge = new ClassesGroupsCategoriesEditVM(dm);
      Assert.AreEqual(4, cge.ClassViewModel.Items.Count);
      Assert.AreEqual(2, cge.GroupViewModel.Items.Count);
      Assert.AreEqual(2, cge.CategoryViewModel.Items.Count);
    }


    [TestMethod]
    public void CgcTest_Store_ManualEdit()
    {
      AppDataModel dm = createTestDataModel1();

      ClassesGroupsCategoriesEditVM cge = new ClassesGroupsCategoriesEditVM(dm);

      // Per XXX add, modify, delete
      cge.CategoryViewModel.Items.Add(new ParticipantCategory('U', "Unbekannt", 2));
      cge.CategoryViewModel.Items.RemoveAt(0);
      cge.CategoryViewModel.Items[0].PrettyName = "WEIBLICH";

      cge.GroupViewModel.Items.Add(new ParticipantGroup("xxx", "G_U14", 2));
      cge.GroupViewModel.Items.RemoveAt(0);
      cge.GroupViewModel.Items[0].Name = "B_U12";

      cge.ClassViewModel.Items.Add(new ParticipantClass("xxx", cge.GroupViewModel.Items[1], "U14", cge.CategoryViewModel.Items[0], 2006, 0));
      cge.ClassViewModel.Items.RemoveAt(0);
      cge.ClassViewModel.Items[0].Name = "u10";

      cge.Store();

      // Verify Data Model
      Assert.AreEqual(dm.GetParticipantCategories().Count, 2);
      Assert.AreEqual("WEIBLICH", dm.GetParticipantCategories()[0].PrettyName);
      Assert.AreEqual("Unbekannt", dm.GetParticipantCategories()[1].PrettyName);

      Assert.AreEqual(2, dm.GetParticipantGroups().Count);
      Assert.AreEqual("B_U12", dm.GetParticipantGroups()[0].Name);
      Assert.AreEqual("G_U14", dm.GetParticipantGroups()[1].Name);

      Assert.AreEqual(4, dm.GetParticipantClasses().Count);
      Assert.AreEqual("u10", dm.GetParticipantClasses()[0].Name);
      Assert.AreEqual("U12", dm.GetParticipantClasses()[1].Name);
      Assert.AreEqual("U12", dm.GetParticipantClasses()[2].Name);
      Assert.AreEqual("U14", dm.GetParticipantClasses()[3].Name);
    }

    [TestMethod]
    public void CgcTest_Store_Import()
    {
      AppDataModel dm = createTestDataModel1();

      ClassesGroupsCategoriesEditVM cge = new ClassesGroupsCategoriesEditVM(dm);

      cge.Import(createTestDataModel12());

      cge.Store();

      // Verify Data Model
      Assert.AreEqual(2, dm.GetParticipantCategories().Count);
      Assert.AreEqual("Männlich", dm.GetParticipantCategories()[0].PrettyName);
      Assert.AreEqual("Weiblich", dm.GetParticipantCategories()[1].PrettyName);

      Assert.AreEqual(4, dm.GetParticipantGroups().Count);
      Assert.AreEqual("G_U10", dm.GetParticipantGroups()[0].Name);
      Assert.AreEqual("G_U12", dm.GetParticipantGroups()[1].Name);
      Assert.AreEqual("G_U14", dm.GetParticipantGroups()[2].Name);
      Assert.AreEqual("G_U16", dm.GetParticipantGroups()[3].Name);

      Assert.AreEqual(8, dm.GetParticipantClasses().Count);
      Assert.AreEqual("U10", dm.GetParticipantClasses()[0].Name);
      Assert.AreEqual("U10", dm.GetParticipantClasses()[1].Name);
      Assert.AreEqual("U12", dm.GetParticipantClasses()[2].Name);
      Assert.AreEqual("U12", dm.GetParticipantClasses()[3].Name);
      Assert.AreEqual("U14", dm.GetParticipantClasses()[4].Name);
      Assert.AreEqual("U14", dm.GetParticipantClasses()[5].Name);
      Assert.AreEqual("U16", dm.GetParticipantClasses()[6].Name);
      Assert.AreEqual("U16", dm.GetParticipantClasses()[7].Name);
    }

    [TestMethod]
    public void CgcTest_Store_ImportReplace()
    {
      AppDataModel dm = createTestDataModel12();

      ClassesGroupsCategoriesEditVM cge = new ClassesGroupsCategoriesEditVM(dm);

      cge.Clear();
      cge.Import(createTestDataModel1());

      cge.Store();

      // Verify Data Model
      Assert.AreEqual(2, dm.GetParticipantCategories().Count);
      Assert.AreEqual("Männlich", dm.GetParticipantCategories()[0].PrettyName);
      Assert.AreEqual("Weiblich", dm.GetParticipantCategories()[1].PrettyName);

      Assert.AreEqual(2, dm.GetParticipantGroups().Count);
      Assert.AreEqual("G_U10", dm.GetParticipantGroups()[0].Name);
      Assert.AreEqual("G_U12", dm.GetParticipantGroups()[1].Name);

      Assert.AreEqual(4, dm.GetParticipantClasses().Count);
      Assert.AreEqual("U10", dm.GetParticipantClasses()[0].Name);
      Assert.AreEqual("U10", dm.GetParticipantClasses()[1].Name);
      Assert.AreEqual("U12", dm.GetParticipantClasses()[2].Name);
      Assert.AreEqual("U12", dm.GetParticipantClasses()[3].Name);
    }



    [TestMethod]
    public void CgcTest_ClearAndReset()
    {
      AppDataModel dm = createTestDataModel1();

      ClassesGroupsCategoriesEditVM cge = new ClassesGroupsCategoriesEditVM(dm);
      Assert.AreEqual(4, cge.ClassViewModel.Items.Count);
      Assert.AreEqual(2, cge.GroupViewModel.Items.Count);
      Assert.AreEqual(2, cge.CategoryViewModel.Items.Count);

      cge.Clear();
      Assert.AreEqual(0, cge.ClassViewModel.Items.Count);
      Assert.AreEqual(0, cge.GroupViewModel.Items.Count);
      Assert.AreEqual(0, cge.CategoryViewModel.Items.Count);

      cge.Reset();
      Assert.AreEqual(4, cge.ClassViewModel.Items.Count);
      Assert.AreEqual(2, cge.GroupViewModel.Items.Count);
      Assert.AreEqual(2, cge.CategoryViewModel.Items.Count);
    }


    [TestMethod]
    public void CgcTest_ImportEmpty()
    {
      AppDataModel dm = createTestDataModelEmpty();

      ClassesGroupsCategoriesEditVM cge = new ClassesGroupsCategoriesEditVM(dm);
      Assert.AreEqual(0, cge.ClassViewModel.Items.Count);
      Assert.AreEqual(0, cge.GroupViewModel.Items.Count);
      Assert.AreEqual(0, cge.CategoryViewModel.Items.Count);

      cge.Import(createTestDataModel1());
      Assert.AreEqual(4, cge.ClassViewModel.Items.Count, 4);
      Assert.AreEqual(2, cge.GroupViewModel.Items.Count, 2);
      Assert.AreEqual(2, cge.CategoryViewModel.Items.Count, 2);

      cge.Reset();
      Assert.AreEqual(0, cge.ClassViewModel.Items.Count);
      Assert.AreEqual(0, cge.GroupViewModel.Items.Count);
      Assert.AreEqual(0, cge.CategoryViewModel.Items.Count);
    }


    [TestMethod]
    public void CgcTest_ImportDisjunct()
    {
      AppDataModel dm = createTestDataModel1();

      ClassesGroupsCategoriesEditVM cge = new ClassesGroupsCategoriesEditVM(dm);
      Assert.AreEqual(4, cge.ClassViewModel.Items.Count);
      Assert.AreEqual(2, cge.GroupViewModel.Items.Count);
      Assert.AreEqual(2, cge.CategoryViewModel.Items.Count);

      cge.Import(createTestDataModel2());
      Assert.AreEqual(8, cge.ClassViewModel.Items.Count);
      Assert.AreEqual(4, cge.GroupViewModel.Items.Count);
      Assert.AreEqual(2, cge.CategoryViewModel.Items.Count);

      cge.Reset();
      Assert.AreEqual(4, cge.ClassViewModel.Items.Count);
      Assert.AreEqual(2, cge.GroupViewModel.Items.Count);
      Assert.AreEqual(2, cge.CategoryViewModel.Items.Count);
    }



    [TestMethod]
    public void CgcTest_ImportJoin()
    {
      AppDataModel dm = createTestDataModel1();

      ClassesGroupsCategoriesEditVM cge = new ClassesGroupsCategoriesEditVM(dm);
      Assert.AreEqual(4, cge.ClassViewModel.Items.Count);
      Assert.AreEqual(2, cge.GroupViewModel.Items.Count);
      Assert.AreEqual(2, cge.CategoryViewModel.Items.Count);

      cge.Import(createTestDataModel12());
      Assert.AreEqual(8, cge.ClassViewModel.Items.Count);
      Assert.AreEqual(4, cge.GroupViewModel.Items.Count);
      Assert.AreEqual(2, cge.CategoryViewModel.Items.Count);

      cge.Reset();
      Assert.AreEqual(4, cge.ClassViewModel.Items.Count);
      Assert.AreEqual(2, cge.GroupViewModel.Items.Count);
      Assert.AreEqual(2, cge.CategoryViewModel.Items.Count);
    }



    private AppDataModel createTestDataModelEmpty()
    {
      AppDataModel dm = new AppDataModel(new DummyDataBase("dummy"));
      return dm;
    }

    private AppDataModel createTestDataModel1()
    {
      AppDataModel dm = new AppDataModel(new DummyDataBase("dummy"));

      foreach (var o in new List<ParticipantCategory>
      {
          new ParticipantCategory('M', "Männlich", 0),
          new ParticipantCategory('W', "Weiblich", 1)
      })
      {
        dm.GetParticipantCategories().Add(o);
      }

      foreach (var o in new List<ParticipantGroup>
      {
        new ParticipantGroup("1", "G_U10", 0),
        new ParticipantGroup("2", "G_U12", 1)
      })
      {
        dm.GetParticipantGroups().Add(o);
      }

      foreach (var o in new List<ParticipantClass>
      {
        new ParticipantClass("1", dm.GetParticipantGroups()[0], "U10", dm.GetParticipantCategories()[0], 2010, 0),
        new ParticipantClass("1", dm.GetParticipantGroups()[0], "U10", dm.GetParticipantCategories()[1], 2010, 0),
        new ParticipantClass("2", dm.GetParticipantGroups()[1], "U12", dm.GetParticipantCategories()[0], 2008, 1),
        new ParticipantClass("2", dm.GetParticipantGroups()[1], "U12", dm.GetParticipantCategories()[1], 2008, 1)
      })
      {
        dm.GetParticipantClasses().Add(o);
      }

      return dm;
    }


    private AppDataModel createTestDataModel2()
    {
      AppDataModel dm = new AppDataModel(new DummyDataBase("dummy"));

      foreach (var o in new List<ParticipantCategory>
      {
          new ParticipantCategory('M', "Männlich", 0),
          new ParticipantCategory('W', "Weiblich", 1)
      })
      {
        dm.GetParticipantCategories().Add(o);
      }

      foreach (var o in new List<ParticipantGroup>
      {
        new ParticipantGroup("1", "G_U14", 0),
        new ParticipantGroup("2", "G_U16", 1)
      })
      {
        dm.GetParticipantGroups().Add(o);
      }

      foreach (var o in new List<ParticipantClass>
      {
        new ParticipantClass("1", dm.GetParticipantGroups()[0], "U14", dm.GetParticipantCategories()[0], 2006, 0),
        new ParticipantClass("1", dm.GetParticipantGroups()[0], "U14", dm.GetParticipantCategories()[1], 2006, 0),
        new ParticipantClass("2", dm.GetParticipantGroups()[1], "U16", dm.GetParticipantCategories()[0], 2004, 1),
        new ParticipantClass("2", dm.GetParticipantGroups()[1], "U16", dm.GetParticipantCategories()[1], 2004, 1)
      })
      {
        dm.GetParticipantClasses().Add(o);
      }

      return dm;
    }


    private AppDataModel createTestDataModel12()
    {
      AppDataModel dm = new AppDataModel(new DummyDataBase("dummy"));

      foreach (var o in new List<ParticipantCategory>
      {
          new ParticipantCategory('M', "Männlich", 0),
          new ParticipantCategory('W', "Weiblich", 1)
      })
      {
        dm.GetParticipantCategories().Add(o);
      }

      foreach (var o in new List<ParticipantGroup>
      {
        new ParticipantGroup("1", "G_U10", 0),
        new ParticipantGroup("2", "G_U12", 1),
        new ParticipantGroup("3", "G_U14", 2),
        new ParticipantGroup("4", "G_U16", 3)
      })
      {
        dm.GetParticipantGroups().Add(o);
      }

      foreach (var o in new List<ParticipantClass>
      {
        new ParticipantClass("1", dm.GetParticipantGroups()[0], "U10", dm.GetParticipantCategories()[0], 2010, 0),
        new ParticipantClass("1", dm.GetParticipantGroups()[0], "U10", dm.GetParticipantCategories()[1], 2010, 1),
        new ParticipantClass("2", dm.GetParticipantGroups()[1], "U12", dm.GetParticipantCategories()[0], 2008, 2),
        new ParticipantClass("2", dm.GetParticipantGroups()[1], "U12", dm.GetParticipantCategories()[1], 2008, 3),

        new ParticipantClass("1", dm.GetParticipantGroups()[2], "U14", dm.GetParticipantCategories()[0], 2006, 4),
        new ParticipantClass("1", dm.GetParticipantGroups()[2], "U14", dm.GetParticipantCategories()[1], 2006, 5),
        new ParticipantClass("2", dm.GetParticipantGroups()[3], "U16", dm.GetParticipantCategories()[0], 2004, 6),
        new ParticipantClass("2", dm.GetParticipantGroups()[3], "U16", dm.GetParticipantCategories()[1], 2004, 7)
      })
      {
        dm.GetParticipantClasses().Add(o);
      }

      return dm;
    }


  }
}
