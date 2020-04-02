using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for StartNumberTest
  /// </summary>
  [TestClass]
  public class StartNumberTest
  {
    public StartNumberTest()
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
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_MultipleRacesNoStartnumber.mdb")]
    public void ChangeStartNumber()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants_MultipleRacesNoStartnumber.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);
      var rps = model.GetRace(0).GetParticipants().ToList();

      // Assign startnumber from scratch
      rps.Sort(Comparer<RaceParticipant>.Create((x, y) => x.Name.CompareTo(y.Name)));
      for(int i=0; i<rps.Count; i++)
        rps[i].StartNumber = (uint)i+1;


      // TEST 1: Test whether startnumber is remembered
      for (int i = 0; i < rps.Count; i++)
        Assert.AreEqual((uint)i+1, rps[i].StartNumber);


      model = null;
      db.Close();


      // TEST 2: Cross-Check whether the startnumbers have been stored in DataBase
      RaceHorologyLib.Database db2 = new RaceHorologyLib.Database();
      db2.Connect(dbFilename);
      AppDataModel model2 = new AppDataModel(db2);
      var rps2 = model2.GetRace(0).GetParticipants().ToList();
      rps2.Sort(Comparer<RaceParticipant>.Create((x, y) => x.Name.CompareTo(y.Name)));
      for (int i = 0; i < rps.Count; i++)
        Assert.AreEqual((uint)i + 1, rps2[i].StartNumber);
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants_MultipleRacesNoStartnumber.mdb")]
    public void StartNumberAssignmentTest()
    {
      string dbFilename = TestUtilities.CreateWorkingFileFrom(testContextInstance.TestDeploymentDir, @"TestDB_LessParticipants_MultipleRacesNoStartnumber.mdb");
      RaceHorologyLib.Database db = new RaceHorologyLib.Database();
      db.Connect(dbFilename);

      AppDataModel model = new AppDataModel(db);
      var rps = model.GetRace(0).GetParticipants().ToList();


      StartNumberAssignment sna = new StartNumberAssignment();

      uint sn = sna.AssignNextFree(rps[0]);
      Assert.AreEqual(1U, sn);
      Assert.AreEqual(rps[0], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 1).Participant);

      sn = sna.AssignNextFree(rps[1]);
      Assert.AreEqual(2U, sn);
      Assert.AreEqual(rps[1], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 2).Participant);

      sna.Assign(4U, rps[2]);
      Assert.AreEqual(rps[2], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 4).Participant);

      sn = sna.AssignNextFree(rps[3]);
      Assert.AreEqual(5U, sn);
      Assert.AreEqual(rps[3], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 5).Participant);

      sna.InsertAndShift(2U);
      Assert.AreEqual(rps[0], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 1).Participant);
      Assert.AreEqual(null,   sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 2).Participant);
      Assert.AreEqual(rps[1], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 3).Participant);
      Assert.AreEqual(null,   sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 4).Participant);
      Assert.AreEqual(rps[2], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 5).Participant);
      Assert.AreEqual(rps[3], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 6).Participant);

      sna.Assign(1U, null);
      Assert.AreEqual(null,   sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 1).Participant);
      Assert.AreEqual(null,   sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 2).Participant);
      Assert.AreEqual(rps[1], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 3).Participant);
      Assert.AreEqual(null,   sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 4).Participant);
      Assert.AreEqual(rps[2], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 5).Participant);
      Assert.AreEqual(rps[3], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 6).Participant);

      sna.Delete(1U);
      Assert.AreEqual(null,   sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 1).Participant);
      Assert.AreEqual(rps[1], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 2).Participant);
      Assert.AreEqual(null,   sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 3).Participant);
      Assert.AreEqual(rps[2], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 4).Participant);
      Assert.AreEqual(rps[3], sna.ParticipantList.FirstOrDefault(v => v?.StartNumber == 5).Participant);


    }
  }
}
