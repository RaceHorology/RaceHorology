/*
 *  Copyright (C) 2019 - 2021 by Sven Flossmann
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for ExportTest
  /// </summary>
  [TestClass]
  public class ExportTest
  {
    public ExportTest()
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
    public void TestMethod1()
    {
      TestDataGenerator tg = new TestDataGenerator();
      tg.createCatsClassesGroups();

      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: 1.0);
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: 2.0);
      tg.createRaceParticipant(cat: tg.findCat('M'), cla: tg.findClass("2M (2010)"), points: 3.0);

      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"), points: 1.5);
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"), points: 2.5);
      tg.createRaceParticipant(cat: tg.findCat('W'), cla: tg.findClass("2W (2010)"), points: 3.5);

      Export export = new Export();
      DataSet ds = export.ExportToDataSet(tg.Model.GetRace(0));

      Assert.AreEqual("Name 1", ds.Tables[0].Rows[0]["Name"]);
      Assert.AreEqual("Name 2", ds.Tables[0].Rows[1]["Name"]);
      Assert.AreEqual("Name 3", ds.Tables[0].Rows[2]["Name"]);
      Assert.AreEqual("Name 4", ds.Tables[0].Rows[3]["Name"]);
      Assert.AreEqual("Name 5", ds.Tables[0].Rows[4]["Name"]);
      Assert.AreEqual("Name 6", ds.Tables[0].Rows[5]["Name"]);

      Assert.AreEqual("Firstname 1", ds.Tables[0].Rows[0]["Firstname"]);
      Assert.AreEqual(1.0, ds.Tables[0].Rows[0]["Points"]);


      var excelExport = new ExcelExport();
      excelExport.Export(@"c:\trash\test.xlsx", ds);

      var csvExport = new CsvExport();
      csvExport.Export(@"c:\trash\test.csv", ds);
    }
  }
}
