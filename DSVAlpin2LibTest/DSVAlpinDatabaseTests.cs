using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DSVAlpin2Lib;
using System.IO;
using System.Data.OleDb;
using System.Linq;

namespace DSVAlpin2LibTest
{
  /// <summary>
  /// Summary description for DSVAlpinDatabaseTests
  /// </summary>
  [TestClass]
  public class DSVAlpinDatabaseTests
  {
    public DSVAlpinDatabaseTests()
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
    //[DeploymentItem(@"TestDataBases\KSC2019-2-PSL.mdb")]
    [DeploymentItem(@"TestDataBases\Kirchberg U8 U10 10.02.19 RS Neu.mdb")]
    public void DatabaseOpenClose()
    {
      DSVAlpin2Lib.Database db = new DSVAlpin2Lib.Database();
      //db.Connect(Path.Combine(_testContext.TestDeploymentDir, @"KSC2019-2-PSL.mdb"));
      db.Connect(Path.Combine(testContextInstance.TestDeploymentDir, @"Kirchberg U8 U10 10.02.19 RS Neu.mdb"));

      db.GetParticipants();

      db.Close();
    }

    [TestMethod]
    //[DeploymentItem(@"TestDataBases\KSC2019-2-PSL.mdb")]
    [DeploymentItem(@"TestDataBases\Kirchberg U8 U10 10.02.19 RS Neu.mdb")]
    public void DatabaseRaceRuns()
    {
      DSVAlpin2Lib.Database db = new DSVAlpin2Lib.Database();
      //db.Connect(Path.Combine(_testContext.TestDeploymentDir, @"KSC2019-2-PSL.mdb"));
      db.Connect(Path.Combine(testContextInstance.TestDeploymentDir, @"Kirchberg U8 U10 10.02.19 RS Neu.mdb"));

      db.GetParticipants();
      RaceRun rr1 = db.GetRaceRun(1);
      RaceRun rr2 = db.GetRaceRun(2);

      db.Close();
    }


    [TestMethod]
    //[DeploymentItem(@"TestDataBases\KSC2019-2-PSL.mdb")]
    [DeploymentItem(@"TestDataBases\Kirchberg U8 U10 10.02.19 RS Neu.mdb")]
    public void InitializeApplicationModel()
    {
      DSVAlpin2Lib.Database db = new DSVAlpin2Lib.Database();
      //db.Connect(Path.Combine(testContextInstance.TestDeploymentDir, @"KSC2019-2-PSL.mdb"));
      db.Connect(Path.Combine(testContextInstance.TestDeploymentDir, @"Kirchberg U8 U10 10.02.19 RS Neu.mdb"));

      AppDataModel model = new AppDataModel(db);
    }

    [TestMethod]
    [DeploymentItem(@"TestDataBases\TestDB_LessParticipants.mdb")]
    [DeploymentItem(@"TestDataBases\TestDB_Empty.mdb")]
    public void CreateAndUpdateParticipants()
    {
      string dbFilename = Path.Combine(testContextInstance.TestDeploymentDir, @"TestDB_Empty.mdb");
      DSVAlpin2Lib.Database db = new DSVAlpin2Lib.Database();
      db.Connect(dbFilename);

      var participants = db.GetParticipants();

      void DBCacheWorkaround()
      {
        db.Close(); // WORKAROUND: OleDB caches the update, so the Check would not see the changes
        db.Connect(dbFilename);
        participants = db.GetParticipants();
      }

      Participant pNew1 = new Participant
      {
        Name = "Nachname 6",
        Firstname = "Vorname 6",
        Sex = "M",
        Club = "Verein 6",
        Nation = "GER",
        Class = "Testklasse 1",
        Year = 2009
      };
      db.CreateOrUpdateParticipant(pNew1);
      DBCacheWorkaround();
      Assert.IsTrue(CheckParticipant(dbFilename, pNew1, 1));
      

      Participant pNew2 = new Participant
      {
        Name = "Nachname 7",
        Firstname = "Vorname 7",
        Sex = "M",
        Club = "Verein 7",
        Nation = "GER",
        Class = "Testklasse 1",
        Year = 2010
      };
      db.CreateOrUpdateParticipant(pNew2);
      DBCacheWorkaround();
      Assert.IsTrue(CheckParticipant(dbFilename, pNew2, 2));


      // Update a Participant
      pNew1 = participants.Where(x => x.Name == "Nachname 6").FirstOrDefault();
      pNew1.Name = "Nachname 6.1";
      pNew1.Firstname = "Vorname 6.1";
      pNew1.Sex = "W";
      pNew1.Club = "Verein 6.1";
      pNew1.Nation = "GDR";
      pNew1.Class = "Testklasse 1.1";
      pNew1.Year = 2008;
      db.CreateOrUpdateParticipant(pNew1);
      DBCacheWorkaround();
      Assert.IsTrue(CheckParticipant(dbFilename, pNew1, 1));
    }


    private bool CheckParticipant(string dbFilename, Participant participant, int id)
    {
      bool bRes = true;

      OleDbConnection conn = new OleDbConnection { ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data source= " + dbFilename };
      conn.Open();

      string sql = @"SELECT * FROM tblTeilnehmer WHERE id = @id";
      OleDbCommand command = new OleDbCommand(sql, conn);
      command.Parameters.Add(new OleDbParameter("@id", id));

      // Execute command  
      using (OleDbDataReader reader = command.ExecuteReader())
      {
        if (reader.Read())
        {
          bRes &= participant.Name == reader["nachname"].ToString();
          bRes &= participant.Firstname == reader["vorname"].ToString();
          bRes &= participant.Sex == reader["sex"].ToString();
          bRes &= participant.Club == reader["verein"].ToString();
          bRes &= participant.Nation == reader["nation"].ToString();
          //bRes &= participant.Class == GetClass(GetValueUInt(reader, "klasse"));
          bRes &= participant.Year == reader.GetInt16(reader.GetOrdinal("jahrgang"));
          //bRes &= participant.StartNumber == GetStartNumber(reader);
        }
        else
          bRes = false;
      }

      conn.Close();

      return bRes;
    }
  }
}
