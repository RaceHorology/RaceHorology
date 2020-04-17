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
using System.IO;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for ImportTest
  /// </summary>
  [TestClass]
  public class ImportTest
  {
    public ImportTest()
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
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.csv")]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.tsv")]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.txt")]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.xls")]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.xlsx")]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844_comma.csv")]
    public void DetectColumns()
    {
      string[] columnShall =
      {
        "FIS-Code-Nr.",
        "Name",
        "Vorname",
        "Verband",
        "Verein",
        "Jahrgang",
        "Geschlecht",
        "FIS-Distanzpunkte",
        "FIS-Sprintpunkte",
        "Startnummer",
        "Gruppe",
        "DSV-Id",
        "Startpass",
        "Leer",
        "Nation",
        "Transponder",
        "Melder-Name",
        "Melder-Adresse",
        "Melder-Telefon",
        "Melder-Mobil",
        "Melder-Email",
        "Nachname Vorname"
      };

      void checkColumns(List<string> columns)
      {
        for (int i = 0; i < columnShall.Length; i++)
          Assert.AreEqual(columnShall[i], columns[i]);
      }

      {
        var ir = new ImportReader(@"Teilnehmer_V1_202001301844.xls");
        checkColumns(ir.Columns);
      }
      { 
        var ir = new ImportReader(@"Teilnehmer_V1_202001301844.xlsx");
        checkColumns(ir.Columns);
      }
      {
        var ir = new ImportReader(@"Teilnehmer_V1_202001301844.csv");
        checkColumns(ir.Columns);
      }
      {
        var ir = new ImportReader(@"Teilnehmer_V1_202001301844.txt");
        checkColumns(ir.Columns);
      }
      {
        var ir = new ImportReader(@"Teilnehmer_V1_202001301844.tsv");
        checkColumns(ir.Columns);
      }
      {
        var ir = new ImportReader(@"Teilnehmer_V1_202001301844_comma.csv");
        checkColumns(ir.Columns);
      }
    }


    [TestMethod]
    [DeploymentItem(@"TestDataBases\Import\Teilnehmer_V1_202001301844.csv")]
    public void ImportParticpants()
    {
      var ir = new ImportReader(@"Teilnehmer_V1_202001301844.csv");

      ParticipantMapping mapping = new ParticipantMapping(ir.Columns);

      List<Participant> participants = new List<Participant>();
      Import im = new Import(ir.Data, participants, mapping);
      im.DoImport();

      for(int i=0; i<153; i++)
      {
        Assert.AreEqual(string.Format("Name {0}", i + 1), participants[i].Name);
      }

    }
  }
}
