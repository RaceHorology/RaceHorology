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

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceHorologyLib;
using System.Data;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for DSVImportTest
  /// </summary>
  [TestClass]
  public class DSVImportTest
  {
    public DSVImportTest()
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
    [DeploymentItem(@"TestDataBases\Import\DSV\DSVSA2008.txt")]
    public void ImportPointList()
    {
      DSVImportReader reader = new DSVImportReader(@"DSVSA2008.txt");

      Assert.AreEqual("Code", reader.Columns[0]);
      Assert.AreEqual("Name", reader.Columns[1]);
      Assert.AreEqual("Firstname", reader.Columns[2]);
      Assert.AreEqual("Year", reader.Columns[3]);
      Assert.AreEqual("Club", reader.Columns[4]);
      Assert.AreEqual("Verband", reader.Columns[5]);
      Assert.AreEqual("Points", reader.Columns[6]);
      Assert.AreEqual("Sex", reader.Columns[7]);

      {
        DataRow row = reader.Data.Tables[0].Rows[0];
        Assert.AreEqual("22444", row["Code"]);
        Assert.AreEqual("ABBOLD", row["Name"]);
        Assert.AreEqual("Markus", row["Firstname"]);
        Assert.AreEqual(2004U, row["Year"]);
        Assert.AreEqual("SC Garmisch", row["Club"]);
        Assert.AreEqual("BSV-WF", row["Verband"]);
        Assert.AreEqual(181.61, row["Points"]);
        Assert.AreEqual("M", row["Sex"]);
      }
      {
        DataRow row = reader.Data.Tables[0].Rows[1881];
        Assert.AreEqual("26134", row["Code"]);
        Assert.AreEqual("OETSCHMANN", row["Name"]);
        Assert.AreEqual("Sophie", row["Firstname"]);
        Assert.AreEqual(2005U, row["Year"]);
        Assert.AreEqual("DAV Peissenberg", row["Club"]);
        Assert.AreEqual("BSV-WF", row["Verband"]);
        Assert.AreEqual(177.98, row["Points"]);
        Assert.AreEqual("F", row["Sex"]);
      }
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\Import\DSV\Punktelisten.zip")]
    public void ImportPointListViaZIP()
    {

      DSVImportReader reader = new DSVImportReaderZip(@"Punktelisten.zip");

      Assert.AreEqual("Code", reader.Columns[0]);
      Assert.AreEqual("Name", reader.Columns[1]);
      Assert.AreEqual("Firstname", reader.Columns[2]);
      Assert.AreEqual("Year", reader.Columns[3]);
      Assert.AreEqual("Club", reader.Columns[4]);
      Assert.AreEqual("Verband", reader.Columns[5]);
      Assert.AreEqual("Points", reader.Columns[6]);
      Assert.AreEqual("Sex", reader.Columns[7]);

      {
        DataRow row = reader.Data.Tables[0].Rows[0];
        Assert.AreEqual("22444", row["Code"]);
        Assert.AreEqual("ABBOLD", row["Name"]);
        Assert.AreEqual("Markus", row["Firstname"]);
        Assert.AreEqual(2004U, row["Year"]);
        Assert.AreEqual("SC Garmisch", row["Club"]);
        Assert.AreEqual("BSV-WF", row["Verband"]);
        Assert.AreEqual(211.61, row["Points"]);
        Assert.AreEqual("M", row["Sex"]);
      }
      {
        DataRow row = reader.Data.Tables[0].Rows[reader.Data.Tables[0].Rows.Count-1];
        Assert.AreEqual("26134", row["Code"]);
        Assert.AreEqual("OETSCHMANN", row["Name"]);
        Assert.AreEqual("Sophie", row["Firstname"]);
        Assert.AreEqual(2005U, row["Year"]);
        Assert.AreEqual("DAV Peissenberg", row["Club"]);
        Assert.AreEqual("BSV-WF", row["Verband"]);
        Assert.AreEqual(207.98, row["Points"]);
        Assert.AreEqual("F", row["Sex"]);
      }
    }

    [TestMethod]
    public void ImportPointListViaWeb()
    {
      DSVImportReader reader = new DSVImportReaderOnline();

      Assert.AreEqual("Code", reader.Columns[0]);
      Assert.AreEqual("Name", reader.Columns[1]);
      Assert.AreEqual("Firstname", reader.Columns[2]);
      Assert.AreEqual("Year", reader.Columns[3]);
      Assert.AreEqual("Club", reader.Columns[4]);
      Assert.AreEqual("Verband", reader.Columns[5]);
      Assert.AreEqual("Points", reader.Columns[6]);
      Assert.AreEqual("Sex", reader.Columns[7]);

      Assert.IsTrue(reader.Data.Tables[0].Rows.Count > 0, "Some rows generated");
    }

  }
}
