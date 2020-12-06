/*
 *  Copyright (C) 2019 - 2020 by Sven Flossmann
 *  
 *  This file is part of Race Horology.
 *
 *  Race Horology is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  any later version.
 * 
 *  Race Horology is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with Race Horology.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  Diese Datei ist Teil von Race Horology.
 *
 *  Race Horology ist Freie Software: Sie können es unter den Bedingungen
 *  der GNU Affero General Public License, wie von der Free Software Foundation,
 *  Version 3 der Lizenz oder (nach Ihrer Wahl) jeder neueren
 *  veröffentlichten Version, weiter verteilen und/oder modifizieren.
 *
 *  Race Horology wird in der Hoffnung, dass es nützlich sein wird, aber
 *  OHNE JEDE GEWÄHRLEISTUNG, bereitgestellt; sogar ohne die implizite
 *  Gewährleistung der MARKTFÄHIGKEIT oder EIGNUNG FÜR EINEN BESTIMMTEN ZWECK.
 *  Siehe die GNU Affero General Public License für weitere Details.
 *
 *  Sie sollten eine Kopie der GNU Affero General Public License zusammen mit diesem
 *  Programm erhalten haben. Wenn nicht, siehe <https://www.gnu.org/licenses/>.
 * 
 */

using RaceHorologyLib;
using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for IntegrationTests
  /// </summary>
  [TestClass]
  public class IntegrationTests
  {
    public IntegrationTests()
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
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.csv")]
    public void ImportRace()
    {
      var db = new RaceHorologyLib.Database();
      string dbFilename = db.CreateDatabase("new.mdb");
      db.Connect(dbFilename);

      //RaceHorologyLib.IAppDataModelDataBase db = new RaceHorologyLib.DatabaseDummy("./");

      AppDataModel dm = new AppDataModel(db);

      // Create a Race
      dm.AddRace(new Race.RaceProperties { RaceType = Race.ERaceType.GiantSlalom, Runs = 2 });

      ImportResults impRes = new ImportResults();

      TimeSpan time = TestUtilities.Time(() =>
      {
        var ir = new ImportReader(@"Teilnehmer_V1_202001301844.csv");
        RaceMapping mapping = new RaceMapping(ir.Columns);

        RaceImport im = new RaceImport(dm.GetRace(0), mapping);
        impRes = im.DoImport(ir.Data);
      });

      Assert.AreEqual(153, impRes.SuccessCount);
      Assert.AreEqual(0, impRes.ErrorCount);

      Assert.IsTrue(dm.GetParticipants().Count() == 153);
      Assert.IsTrue(dm.GetRace(0).GetParticipants().Count() == 153);

      TestContext.WriteLine(string.Format("Import took: {0:0.00} sec", time.TotalSeconds));
      Assert.IsTrue(time.TotalSeconds < 4);

      //db.Close();

    }
  }
}
