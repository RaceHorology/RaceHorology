/*
 *  Copyright (C) 2019 - 2023 by Sven Flossmann
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
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLibTest
{
  static class TestUtilities
  {
    public static string CreateWorkingFileFrom(string srcDirectory, string srcFilename)
    {
      string srcPath = Path.Combine(srcDirectory, srcFilename);

      string dstDirectory = CreateWorkingFolder(srcDirectory);

      string dstPath = Path.Combine(dstDirectory, srcFilename);
      File.Copy(srcPath, dstPath);

      var additionalFiles = Directory.GetFiles(srcDirectory, Path.GetFileNameWithoutExtension(srcFilename) + "*");
      foreach(var f in additionalFiles)
      {
        if (f == srcPath)
          continue;

        string dstF = Path.Combine(dstDirectory, Path.GetFileName(f));
        File.Copy(f, dstF);
      }

      return dstPath;
    }

    public static string CreateWorkingFolder(string srcDirectory)
    {
      string dstDirectory = Path.Combine(srcDirectory, Path.GetRandomFileName());
      Directory.CreateDirectory(dstDirectory);
      return dstDirectory;
    }

    public static string Copy(string srcFilepath, string dstFilename)
    {
      string dstFilepath = Path.Combine(Path.GetDirectoryName(srcFilepath), dstFilename);
      File.Copy(srcFilepath, dstFilepath);

      return dstFilepath;
    }


    public static bool IsStringEqualDB(string valueShall, object valueIs)
    {
      if (valueShall == null)
        if (DBNull.Value.Equals(valueIs))
          return true;
        else
          return false;

      return string.Equals(valueShall, valueIs);
    }

    public static bool IsDateTimeEqualDB(DateTime? valueShall, object valueIs)
    {
      if (valueShall == null)
        if (DBNull.Value.Equals(valueIs))
          return true;
        else
          return false;

      return valueShall == (DateTime)valueIs;
    }

    public static TimeSpan Time(Action action)
    {
      Stopwatch stopwatch = Stopwatch.StartNew();
      action();
      stopwatch.Stop();
      return stopwatch.Elapsed;
    }




    public static bool GenerateAndCompareAgainstPdf(TestContext testContext, IPDFReport report, string filenameShall, int nAcceptedDifferences = 0)
    {
      string filenameOutput = report.ProposeFilePath();
      report.Generate(filenameOutput);

#pragma warning disable CS0162 // Unreachable code detected
      if (true)
        System.Diagnostics.Process.Start(filenameOutput);
#pragma warning restore CS0162 // Unreachable code detected

      return CompareAgainstPdf(testContext, filenameOutput, filenameShall, nAcceptedDifferences);
    }


    public static bool CompareAgainstPdf(TestContext testContext, string filenameOutput, string filenameShall, int nAcceptedDifferences = 0)
    {

      var pdfReaderOutput = new iText.Kernel.Pdf.PdfReader(filenameOutput);
      var pdfOutput = new iText.Kernel.Pdf.PdfDocument(pdfReaderOutput);

      var pdfReaderShall = new iText.Kernel.Pdf.PdfReader(filenameShall);
      var pdfShall = new iText.Kernel.Pdf.PdfDocument(pdfReaderShall);

      var ct = new iText.Kernel.Utils.CompareTool();
      var result = ct.CompareByCatalog(pdfOutput, pdfShall);

      testContext.WriteLine(string.Format("Diff of {0} <-> {1}", filenameOutput, filenameShall));
      foreach (var dif in result.GetDifferences())
      {
        testContext.WriteLine(dif.Value);
      }
      Debug.WriteLine("Found differences: " + result.GetDifferences().Count);
      Debug.WriteLine("Accepted differences: " + nAcceptedDifferences);
      return result.GetDifferences().Count <= nAcceptedDifferences;
    }

    public static void AreEqualByJson(object expected, object actual)
    {
      var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
      var expectedJson = serializer.Serialize(expected);
      var actualJson = serializer.Serialize(actual);
      Assert.AreEqual(expectedJson, actualJson);
    }
  }

  public class DBTestUtilities
  {
    string _filename;
    OleDbConnection _conn;
    public DBTestUtilities(string filename)
    {
      _filename = filename;
      _conn = new OleDbConnection
      {
        ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data source= " + filename
      };
      _conn.Open();
    }

    public void Close()
    {
      _conn.Close();
    }

    public void ClearTimeMeasurements()
    {
      string sql = @"DELETE FROM tblZeit";
      var cmd = new OleDbCommand(sql, _conn);
      cmd.CommandType = System.Data.CommandType.Text;
      int temp = cmd.ExecuteNonQuery();
    }
  }


  public class DummyDataBase : IAppDataModelDataBase
  {
    List<Race.RaceProperties> _races;
    string _basePath;

    public DummyDataBase(string basePath, bool createRace = false)
    {
      _races = new List<Race.RaceProperties>();
      if (createRace)
      {
        _races.Add(new Race.RaceProperties
        {
          RaceType = Race.ERaceType.GiantSlalom,
          Runs = 2
        });
      }
      _basePath = basePath;
    }


    public void Close()
    {
      // Simply nothing todo
    }

    public string GetDBPath() { return System.IO.Path.Combine(_basePath, GetDBFileName()); }
    public string GetDBFileName() { return "dummy.mdb"; }
    public string GetDBPathDirectory() { return _basePath; }


    public ItemsChangeObservableCollection<Participant> GetParticipants() { return new ItemsChangeObservableCollection<Participant>(); }

    public List<ParticipantGroup> GetParticipantGroups() { return new List<ParticipantGroup>(); }
    public List<ParticipantClass> GetParticipantClasses() { return new List<ParticipantClass>(); }
    public List<ParticipantCategory> GetParticipantCategories() { return new List<ParticipantCategory>(); }


    public List<Race.RaceProperties> GetRaces() { return _races; }
    public List<RaceParticipant> GetRaceParticipants(Race race, bool ignoreActiveFlag = false) { return new List<RaceParticipant>(); }

    public List<RunResult> GetRaceRun(Race race, uint run) { return new List<RunResult>(); }

    public AdditionalRaceProperties GetRaceProperties(Race race) { return null; }
    public void StoreRaceProperties(Race race, AdditionalRaceProperties props) { }

    public void CreateOrUpdateParticipant(Participant participant) { }
    public void RemoveParticipant(Participant participant) { }

    public void CreateOrUpdateRaceParticipant(RaceParticipant participant) { }
    public void RemoveRaceParticipant(RaceParticipant raceParticipant) { }

    public void CreateOrUpdateRunResult(Race race, RaceRun raceRun, RunResult result) { }
    public void DeleteRunResult(Race race, RaceRun raceRun, RunResult result) { }

    public void UpdateRace(Race race, bool active) { }

    public void CreateOrUpdateClass(ParticipantClass c) { }

    public void RemoveClass(ParticipantClass c) { }

    public void CreateOrUpdateGroup(ParticipantGroup g) { }

    public void RemoveGroup(ParticipantGroup g) { }
    public void CreateOrUpdateCategory(ParticipantCategory c) { }
    public void RemoveCategory(ParticipantCategory c) { }


    Dictionary<string, string> _keyValueStore = new Dictionary<string, string>();
    public void StoreKeyValue(string key, string value) 
    {
      _keyValueStore[key] = value;
    }

    public string GetKeyValue(string key) 
    {
      string value = null;
      _keyValueStore.TryGetValue(key, out value);
      return value; 
    }

    public void CreateOrUpdateTimestamp(RaceRun raceRun, Timestamp timestamp) { }

    public List<Timestamp> GetTimestamps(Race race, uint run) { return new List<Timestamp>(); }

    public void RemoveTimestamp(RaceRun raceRun, Timestamp timestamp) { }
  }


  public class TestDataGenerator
  {
    Race _race;

    public TestDataGenerator(string path = ".")
    {
      Model = new AppDataModel(new DummyDataBase(path, true));
      _race = Model.GetRace(0);
      createCategories();
    }

    public AppDataModel Model { get; private set; }


    public void createCatsClassesGroups()
    {
      //createCategories(); done in consructor
      createGroups();
      createClasses();
    }


    public ParticipantCategory findCat(char name)
    {
      return Model.GetParticipantCategories().FirstOrDefault(c => c.Name == name);
    }
    public ParticipantClass findClass(string name)
    {
      return Model.GetParticipantClasses().FirstOrDefault(c => c.Name.Contains(name));
    }
    public ParticipantGroup findGroup(string name)
    {
      return Model.GetParticipantGroups().FirstOrDefault(c => c.Name .Contains(name));
    }


    public IList<ParticipantCategory> createCategories()
    {
      IList<ParticipantCategory> cats = Model.GetParticipantCategories();
      cats.Add(new ParticipantCategory('M', "Männlich", 0, "hx"));
      cats.Add(new ParticipantCategory('W'));
      return cats;
    }

    public IList<ParticipantGroup> createGroups()
    {
      var groups = Model.GetParticipantGroups();
      groups.Add(new ParticipantGroup("1", "Group 2M", 0));
      groups.Add(new ParticipantGroup("2", "Group 2W", 0));
      groups.Add(new ParticipantGroup("3", "Group 1M", 0));
      groups.Add(new ParticipantGroup("4", "Group 1W", 0));
      return groups;
    }

    public IList<ParticipantClass> createClasses()
    {
      var groups = Model.GetParticipantGroups();

      var classes = Model.GetParticipantClasses();
      classes.Add(new ParticipantClass("1", findGroup("2M"), "Class 2M (2010)", new ParticipantCategory('M'), 2010, 1));
      classes.Add(new ParticipantClass("2", findGroup("2W"), "Class 2W (2010)", new ParticipantCategory('W'), 2010, 2));
      classes.Add(new ParticipantClass("3", findGroup("2M"), "Class 2M (2011)", new ParticipantCategory('M'), 2011, 3));
      classes.Add(new ParticipantClass("4", findGroup("2W"), "Class 2W (2011)", new ParticipantCategory('W'), 2011, 4));
      classes.Add(new ParticipantClass("5", findGroup("1M"), "Class 1M (2012)", new ParticipantCategory('M'), 2012, 5));
      classes.Add(new ParticipantClass("6", findGroup("1W"), "Class 1W (2012)", new ParticipantCategory('W'), 2012, 6));
      classes.Add(new ParticipantClass("7", findGroup("1M"), "Class 1M (2013)", new ParticipantCategory('M'), 2013, 7));
      classes.Add(new ParticipantClass("8", findGroup("1W"), "Class 1W (2013)", new ParticipantCategory('W'), 2013, 8));
      return classes;
    }


    public List<RaceParticipant> createRaceParticipants(int n, ParticipantClass cla = null, ParticipantCategory cat = null)
    {
      List<RaceParticipant> participants = new List<RaceParticipant>();

      for (int i = 0; i < n; i++)
        participants.Add(createRaceParticipant(cla: cla, cat: cat));

      return participants;
    }

    public RaceParticipant createRaceParticipant(ParticipantClass cla = null, ParticipantCategory cat = null, double points = -1.0)
    {
      Participant p = createParticipant(cla: cla, cat: cat);
      RaceParticipant rp = _race.AddParticipant(p);

      if (points > 0)
        rp.Points = points;
      rp.StartNumber = uint.Parse(p.Id);

      return rp;
    }


    uint _participantSerial = 0;
    public Participant createParticipant(ParticipantClass cla = null, ParticipantCategory cat = null)
    {
      _participantSerial++;

      var p = new Participant
      {
        Name = string.Format("Name {0}", _participantSerial),
        Firstname = string.Format("Firstname {0}", _participantSerial),
        Sex = cat,
        Id = _participantSerial.ToString(),
        Class = cla
      };

      Model.GetParticipants().Add(p);

      return p;
    }

    public RunResult createRunResult(RaceParticipant rp, TimeSpan? startTime, TimeSpan? endTime)
    {
      RunResult rr = new RunResult(rp);
      rr.SetStartTime(startTime);
      rr.SetFinishTime(endTime);
      return rr;
    }

  }

}
