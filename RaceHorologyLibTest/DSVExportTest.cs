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
using System.IO;
using XmlUnit.Xunit;

namespace RaceHorologyLibTest
{
  /// <summary>
  /// Summary description for DSVExportTest
  /// </summary>
  [TestClass]
  public class DSVExportTest
  {
    public DSVExportTest()
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



    /// <summary>
    /// Tests for exception on cases a mandatory field is not specified
    /// </summary>
    [TestMethod]
    public void BasicExceptions_MandatoryFields()
    {
      var model = createTestDataModel1Race1Run();

      DSVExport dsvExport = new DSVExport();
      MemoryStream xmlData = null;

      var raceProps = new AdditionalRaceProperties();
      model.GetRace(0).AdditionalProperties = raceProps;

      xmlData = new MemoryStream();
      Assert.AreEqual("missing racedate",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.DateResultList = DateTime.Today;

      xmlData = new MemoryStream();
      Assert.AreEqual("missing raceid",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      model.GetRace(0).AdditionalProperties.RaceNumber = "1234";

      xmlData = new MemoryStream();
      Assert.AreEqual("missing raceorganizer",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      model.GetRace(0).AdditionalProperties.Organizer = "WSV Glonn";

      xmlData = new MemoryStream();
      Assert.AreEqual("missing racename",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      model.GetRace(0).AdditionalProperties.Description = "Test Race";

      xmlData = new MemoryStream();
      Assert.AreEqual("missing raceplace",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      model.GetRace(0).AdditionalProperties.Location = "Test Location";



      xmlData = new MemoryStream();
      Assert.AreEqual("missing useddsvlist",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      model.GetDB().StoreKeyValue("DSV_UsedDSVList", "123");



      xmlData = new MemoryStream();
      Assert.AreEqual("missing racejury ChiefRace",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      model.GetRace(0).AdditionalProperties.RaceManager = new AdditionalRaceProperties.Person { Name = "Race Manager", Club = "Club" };

      xmlData = new MemoryStream();
      Assert.AreEqual("missing racejury Referee",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      model.GetRace(0).AdditionalProperties.RaceReferee = new AdditionalRaceProperties.Person { Name = "Race Referee", Club = "Club" };

      xmlData = new MemoryStream();
      Assert.AreEqual("missing racejury RepresentativeTrainer",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      model.GetRace(0).AdditionalProperties.TrainerRepresentative = new AdditionalRaceProperties.Person { Name = "Trainer Rep", Club = "Club" };



      xmlData = new MemoryStream();
      Assert.AreEqual("missing coarsename",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.CoarseName = "Kurs 1";

      xmlData = new MemoryStream();
      Assert.AreEqual("missing number_of_gates",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.RaceRun1.Gates = 10;

      xmlData = new MemoryStream();
      Assert.AreEqual("missing number_of_turninggates",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.RaceRun1.Turns = 9;

      xmlData = new MemoryStream();
      Assert.AreEqual("missing startaltitude",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.StartHeight = 1000;

      xmlData = new MemoryStream();
      Assert.AreEqual("missing finishaltitude",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.FinishHeight = 100;

      xmlData = new MemoryStream();
      Assert.AreEqual("missing courselength",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.CoarseLength = 1000;

      xmlData = new MemoryStream();
      Assert.AreEqual("missing coursesetter",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.RaceRun1.CoarseSetter = new AdditionalRaceProperties.Person { Name = "Sven Flossmann", Club = "WSV Glonn" };

      xmlData = new MemoryStream();
      Assert.AreEqual("missing forerunner",
        Assert.ThrowsException<DSVExportException>(() => dsvExport.ExportXML(xmlData, model.GetRace(0))).Message);
      raceProps.RaceRun1.Forerunner1 = new AdditionalRaceProperties.Person { Name = "Fore Runner", Club = "WSV Glonn" };

      // No exception is allow to occur here
      xmlData = new MemoryStream();
      dsvExport.ExportXML(xmlData, model.GetRace(0));
    }

    [TestMethod]
    public void VerifyXML_MandatoryFields()
    {
      var model = createTestDataModel1Race1Run();
      fillMandatoryFields(model);

      string s = exportToXML(model.GetRace(0));

      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedescription/racedate", s, DateTime.Today.ToString("yyyy-MM-dd"));
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedescription/gender", s, "A");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedescription/raceid", s, "1234");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedescription/raceorganizer", s, "WSV Glonn");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedescription/discipline", s, "RS");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedescription/category", s, "SO");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedescription/racename", s, "Test Race");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedescription/raceplace", s, "Test Location");

      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/useddsvlist", s, "123");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/racejury[@function='ChiefRace']/lastname", s, "Race Manager");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/racejury[@function='Referee']/lastname", s, "Race Referee");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/racejury[@function='RepresentativeTrainer']/lastname", s, "Trainer Rep");

      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/rundata[1]/coursedata/coursename", s, "Kurs 1");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/rundata[1]/coursedata/number_of_gates", s, "10");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/rundata[1]/coursedata/number_of_turninggates", s, "9");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/rundata[1]/coursedata/startaltitude", s, "1000");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/rundata[1]/coursedata/finishaltitude", s, "100");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/rundata[1]/coursedata/courselength", s, "1000");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/rundata[1]/coursedata/coursesetter/lastname", s, "Sven Flossmann");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/rundata[1]/coursedata/coursesetter/club", s, "WSV Glonn");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/rundata[1]/coursedata/forerunner[1]/lastname", s, "Fore Runner");
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/rundata[1]/coursedata/forerunner[1]/club", s, "WSV Glonn");
    }


    [TestMethod]
    public void VerifyXML_MeteoData()
    {
      var model = createTestDataModel1Race1Run();
      fillMandatoryFields(model);

      // no eather set, check if weather is absent
      string s = exportToXML(model.GetRace(0));

      Assert.ThrowsException<Xunit.Sdk.TrueException>(()=> XmlAssertion.AssertXPathExists("/dsv_alpine_raceresults/racedata/rundata[1]/meteodata", s));

      model.GetRace(0).AdditionalProperties.Weather = "sunny";
      s = exportToXML(model.GetRace(0));
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/rundata[1]/meteodata/weather", s, "sunny");


      Assert.ThrowsException<Xunit.Sdk.TrueException>(() => XmlAssertion.AssertXPathExists("/dsv_alpine_raceresults/racedata/rundata[1]/meteodata/snowtexture", s));
      model.GetRace(0).AdditionalProperties.Snow = "griffig";
      s = exportToXML(model.GetRace(0));
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/rundata[1]/meteodata/snowtexture", s, "griffig");


      Assert.ThrowsException<Xunit.Sdk.TrueException>(() => XmlAssertion.AssertXPathExists("/dsv_alpine_raceresults/racedata/rundata[1]/meteodata/temperature_startaltitude", s));
      model.GetRace(0).AdditionalProperties.TempStart = "-2";
      s = exportToXML(model.GetRace(0));
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/rundata[1]/meteodata/temperature_startaltitude", s, "-2");


      Assert.ThrowsException<Xunit.Sdk.TrueException>(() => XmlAssertion.AssertXPathExists("/dsv_alpine_raceresults/racedata/rundata[1]/meteodata/temperature_finishaltitude", s));
      model.GetRace(0).AdditionalProperties.TempFinish = "-1";
      s = exportToXML(model.GetRace(0));
      XmlAssertion.AssertXPathEvaluatesTo("/dsv_alpine_raceresults/racedata/rundata[1]/meteodata/temperature_finishaltitude", s, "-1");
    }

    string exportToXML(Race race)
    {
      DSVExport dsvExport = new DSVExport();
      MemoryStream xmlData = new MemoryStream();
      dsvExport.ExportXML(xmlData, race);

      // Convert to string
      xmlData.Position = 0;
      StreamReader reader = new StreamReader(xmlData);
      return reader.ReadToEnd();
    }


    private AppDataModel createTestDataModel1Race1Run()
    {
      AppDataModel dm = new AppDataModel(new DummyDataBase("dummy"));

      dm.AddRace(new Race.RaceProperties { RaceType = Race.ERaceType.GiantSlalom, Runs = 1 });

      return dm;
    }


    void fillMandatoryFields(AppDataModel model)
    {
      var raceProps = new AdditionalRaceProperties();
      model.GetRace(0).AdditionalProperties = raceProps;
      raceProps.DateResultList = DateTime.Today;
      model.GetRace(0).AdditionalProperties.RaceNumber = "1234";
      model.GetRace(0).AdditionalProperties.Organizer = "WSV Glonn";
      model.GetRace(0).AdditionalProperties.Description = "Test Race";
      model.GetRace(0).AdditionalProperties.Location = "Test Location";

      model.GetDB().StoreKeyValue("DSV_UsedDSVList", "123");

      model.GetRace(0).AdditionalProperties.RaceManager = new AdditionalRaceProperties.Person { Name = "Race Manager", Club = "Club" };
      model.GetRace(0).AdditionalProperties.RaceReferee = new AdditionalRaceProperties.Person { Name = "Race Referee", Club = "Club" };
      model.GetRace(0).AdditionalProperties.TrainerRepresentative = new AdditionalRaceProperties.Person { Name = "Trainer Rep", Club = "Club" };

      raceProps.CoarseName = "Kurs 1";
      raceProps.StartHeight = 1000;
      raceProps.FinishHeight = 100;
      raceProps.CoarseLength = 1000;

      raceProps.RaceRun1.Gates = 10;
      raceProps.RaceRun1.Turns = 9;
      raceProps.RaceRun1.CoarseSetter = new AdditionalRaceProperties.Person { Name = "Sven Flossmann", Club = "WSV Glonn" };
      raceProps.RaceRun1.Forerunner1 = new AdditionalRaceProperties.Person { Name = "Fore Runner", Club = "WSV Glonn" };

      if (model.GetRace(0).GetMaxRun() > 1)
      {
        raceProps.RaceRun2.Gates = 10;
        raceProps.RaceRun2.Turns = 9;
        raceProps.RaceRun2.CoarseSetter = new AdditionalRaceProperties.Person { Name = "Sven Flossmann", Club = "WSV Glonn" };
        raceProps.RaceRun2.Forerunner1 = new AdditionalRaceProperties.Person { Name = "Fore Runner", Club = "WSV Glonn" };
      }
    }

  }
}
