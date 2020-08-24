using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for ValueConverterTest
  /// </summary>
  [TestClass]
  public class ValueConverterTest
  {
    public ValueConverterTest()
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
    public void AgeToYearInputConverterTest()
    {
      var converter = new AgeToYearInputConverter();

      // Standard forward conversion, no change in object
      Assert.AreEqual(10, converter.Convert(10, null, null, null));

      // Check years stay years
      Assert.AreEqual(2010, converter.ConvertBack(2010, null, null, null));
      Assert.AreEqual(2020, converter.ConvertBack(2020, null, null, null));

      // Check ages get years
      Assert.AreEqual(DateTime.Now.AddMonths(3).Year - 10, converter.ConvertBack(10, null, null, null));

    }
  }
}
