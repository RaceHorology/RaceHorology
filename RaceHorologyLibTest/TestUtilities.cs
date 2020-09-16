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

      string dstDirectory = Path.Combine(srcDirectory, Path.GetRandomFileName());
      Directory.CreateDirectory(dstDirectory);

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
    

    public DummyDataBase()
    {
      _races = new List<Race.RaceProperties>();
      _races.Add(new Race.RaceProperties 
      {
        RaceType = Race.ERaceType.GiantSlalom,
        Runs = 1
      });
    }

    public string GetDBPath() { return "dummy"; }
    public string GetDBFileName() { return "dummy"; }
    public string GetDBPathDirectory() { return "dummy"; }


    public ItemsChangeObservableCollection<Participant> GetParticipants() { return new ItemsChangeObservableCollection<Participant>(); }

    public List<ParticipantGroup> GetParticipantGroups() { return new List<ParticipantGroup>(); }
    public List<ParticipantClass> GetParticipantClasses() { return new List<ParticipantClass>(); }
    public List<ParticipantCategory> GetParticipantCategories() { return new List<ParticipantCategory>(); }


    public List<Race.RaceProperties> GetRaces() { return _races; }
    public List<RaceParticipant> GetRaceParticipants(Race race) { return new List<RaceParticipant>(); }

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

    public void StoreKeyValue(string key, string value) { }
    public string GetKeyValue(string key) { return null; }
  };



  public class TestDataGenerator
  {
    Race _race;

    public TestDataGenerator()
    {
      Model = new AppDataModel(new DummyDataBase());
      _race = Model.GetRace(0);
    }

    public AppDataModel Model { get; private set; }

    public List<ParticipantCategory> createCategories()
    {
      List<ParticipantCategory> cats = new List<ParticipantCategory>();
      cats.Add(new ParticipantCategory('M'));
      cats.Add(new ParticipantCategory('W'));
      return cats;
    }


    public List<RaceParticipant> createRaceParticipants(int n)
    {
      List<RaceParticipant> participants = new List<RaceParticipant>();

      for (int i = 0; i < n; i++)
        participants.Add(createRaceParticipant());

      return participants;
    }

    public RaceParticipant createRaceParticipant()
    {
      return _race.AddParticipant(createParticipant());
    }


    uint _participantSerial = 0;
    public Participant createParticipant()
    {
      _participantSerial++;

      return new Participant
      {
        Name = string.Format("Name {0}", _participantSerial),
        Firstname = string.Format("Firstname {0}", _participantSerial),
        Id = _participantSerial.ToString()
      };
    }

  }

}
