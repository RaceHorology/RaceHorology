﻿/*
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

using DocumentFormat.OpenXml.Office.Word;
using Org.BouncyCastle.Crypto.Agreement.JPake;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  /// <summary>
  /// Implements the data base access to and from the "old" DSVAlpin Access Data Base
  /// </summary>
  /// <remarks>not yet fully implemented</remarks>
  public class Database
    : IAppDataModelDataBase
  {
    private string _dbPath;
    private OdbcConnection _conn;

    private Dictionary<uint, Participant> _id2Participant;
    private Dictionary<uint, ParticipantGroup> _id2ParticipantGroups;
    private Dictionary<uint, ParticipantClass> _id2ParticipantClasses;
    private Dictionary<char, ParticipantCategory> _id2ParticipantCategory;

    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Create a new database file for RaceHorology
    /// </summary>
    /// <param name="dbPath">The target path name.</param>
    /// <returns>The target path name</returns>
    public string CreateDatabase(string dbPath)
    {
      var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("RaceHorologyLib.dbtemplates.TemplateDB_Standard.mdb");
      var fileStream = System.IO.File.Create(dbPath);
      stream.Seek(0, System.IO.SeekOrigin.Begin);
      stream.CopyTo(fileStream);
      fileStream.Close();

      // Store some correct Value in the DB
      Connect(dbPath);
      // Update the right name within the DB - to make DSVAlpin and DSVAlpinX happy
      var prop = GetCompetitionProperties();
      prop.Name = System.IO.Path.GetFileNameWithoutExtension(GetDBFileName());
      // A default country - to make DSVAlpinX happy
      prop.Nation = "GER"; 
      UpdateCompetitionProperties(prop);
      // A Default location - to make DSVAlpinX happy
      storeRacePropertyInternal(null, "");

      Close();

      return dbPath;
    }


    public void Connect(string dbPath)
    {
      Logger.Info("Connect to database: {0}", dbPath);

      _dbPath = dbPath;
      _conn = new OdbcConnection
      {
        ConnectionString = @"Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=" + dbPath + ";Jet OLEDB:Flush Transaction Timeout=10"

      };

      try
      {
        _conn.Open();

        checkOrUpgradeSchema();
      }
      catch (Exception ex)
      {
        Logger.Error(ex, "Failed to connect to database: {0} ", dbPath);
        _conn = null;
        throw;
      }

      // Setup internal data structures
      _id2Participant = new Dictionary<uint, Participant>();
      _id2ParticipantGroups = new Dictionary<uint, ParticipantGroup>();
      _id2ParticipantClasses = new Dictionary<uint, ParticipantClass>();
      _id2ParticipantCategory = new Dictionary<char, ParticipantCategory>();
    }

    public void Close()
    {
      Logger.Info("Close database: {0}", _dbPath);

      // Cleanup internal data structures
      _id2Participant = null;
      _id2ParticipantGroups = null;
      _id2ParticipantClasses = null;
      _id2ParticipantCategory = null;

      _conn.Close();
      _conn.Dispose();

      // Force closing the connection and destroy the connection pool
      OdbcConnection.ReleaseObjectPool();
      GC.Collect();
      GC.WaitForPendingFinalizers(); 

      _conn = null;
    }


    void dropTable(string table)
    {
      string sql = "DROP TABLE RHMisc";
      //OdbcCommand cmd = new OdbcCommand(sql, _conn);
      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      cmd.ExecuteNonQuery();
    }

    void checkOrUpgradeSchema()
    {
      checkOrUpgradeSchema_RHMisc();
      checkOrUpgradeSchema_tblKategorie();
      checkOrUpgradeSchema_RHTimestamps();
      checkOrUpgradeDBVersion();
    }

    void checkOrUpgradeSchema_RHMisc()
    {
      if (existsTable("RHMisc"))
        return;

      // Create TABLE RHMisc 
      string sql = @"CREATE TABLE RHMisc ([key] TEXT(255) NOT NULL, [val] LONGTEXT, PRIMARY KEY ([key]) )";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      cmd.ExecuteNonQuery();
    }

    void checkOrUpgradeSchema_tblKategorie()
    {
      // Check if table already existing
      if (existsColumn("tblKategorie", "RHSynonyms"))
        return;

      // add RHSynonyms column to tblKategorie
      //string sql = @"ALTER TABLE [tblKategorie] ADD COLUMN [RHSynonyms] TEXT(255) DEFAULT NULL";
      string sql = @"ALTER TABLE [tblKategorie] ADD COLUMN [RHSynonyms] TEXT(255)";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      cmd.ExecuteNonQuery();
    }

    void checkOrUpgradeSchema_RHTimestamps()
    {
      // Check if table already existing
      if (existsTable("RHTimestamps"))
        return;

      // Create TABLE 
      string sql = @"
        CREATE TABLE RHTimestamps (
          [disziplin] BYTE NOT NULL, 
          [durchgang] BYTE NOT NULL, 
          [zeit] DOUBLE NOT NULL, 
          [startnummer] LONG,
          [valid] BIT,
          [kanal] TEXT(10) NOT NULL
        )";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      int res = cmd.ExecuteNonQuery();
    }


    void checkOrUpgradeDBVersion()
    {
      string sql = @"UPDATE tblVersion SET version = ?";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);

      cmd.Parameters.Add(new OdbcParameter("@version", 18));
      cmd.CommandType = CommandType.Text;

      try
      {
        Logger.Debug("checkOrUpgradeDBVersion(), SQL: {0}", GetDebugSqlString(cmd));

        int temp = cmd.ExecuteNonQuery();
        Debug.Assert(temp == 1, "Database could not be updated");
      }
      catch (Exception e)
      {
        Logger.Warn(e, "checkOrUpgradeDBVersion failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }


    bool existsTable(string tableName)
    {
      bool exists;
      /*var schema = _conn.GetSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
      return
        schema.Rows
          .OfType<System.Data.DataRow>()
          .Any(r => r.ItemArray[2].ToString().ToLower() == tableName.ToLower());*/
      try
      {
        exists = true;
        var cmdOthers = new OdbcCommand("select 1 from " + tableName + " where 1 = 0", _conn);
        cmdOthers.ExecuteNonQuery();

      }
      catch
      {
        exists = false;
      }
      return exists;
    }

    bool existsColumn(string tableName, string column)
    {
      System.Data.DataTable schema = _conn.GetSchema("COLUMNS");
      var col = schema.Select("TABLE_NAME='" + tableName + "' AND COLUMN_NAME='" + column + "'");
      return col.Length > 0;
    }

    #region IAppDataModelDataBase implementation

    public string GetDBPath()
    {
      return _dbPath;
    }

    public string GetDBFileName()
    {
      return System.IO.Path.GetFileNameWithoutExtension(_dbPath);
    }


    public string GetDBPathDirectory()
    {
      return new System.IO.FileInfo(_dbPath).Directory.FullName;
    }


    public ItemsChangeObservableCollection<Participant> GetParticipants()
    {
      Logger.Debug("GetParticipants()");

      ItemsChangeObservableCollection<Participant> participants = new ItemsChangeObservableCollection<Participant>();

      string sql = @"SELECT * FROM tblTeilnehmer";

      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      // Execute command  
      using (OdbcDataReader reader = cmd.ExecuteReader())
      {
        while (reader.Read())
        {
          participants.Add(CreateParticipantFromDB(reader));
        }
      }

      return participants;
    }


    public List<ParticipantGroup> GetParticipantGroups()
    {
      ReadParticipantGroups();

      List<ParticipantGroup> groups = new List<ParticipantGroup>();
      foreach (var p in _id2ParticipantGroups)
        groups.Add(p.Value);

      return groups;
    }


    public List<ParticipantClass> GetParticipantClasses()
    {
      ReadParticipantClasses();

      List<ParticipantClass> classes = new List<ParticipantClass>();
      foreach (var p in _id2ParticipantClasses)
        classes.Add(p.Value);

      return classes;
    }

    public List<ParticipantCategory> GetParticipantCategories()
    {
      ReadParticipantCategories();

      List<ParticipantCategory> cats = new List<ParticipantCategory>();
      foreach (var p in _id2ParticipantCategory)
        cats.Add(p.Value);

      return cats;
    }


    public List<Race.RaceProperties> GetRaces()
    {
      Logger.Debug("GetRaces()");

      List<Race.RaceProperties> races = new List<Race.RaceProperties>();

      string sql = @"SELECT * FROM tblDisziplin WHERE aktiv = true";

      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      // Execute command  
      using (OdbcDataReader reader = cmd.ExecuteReader())
      {
        while (reader.Read())
        {
          Race.RaceProperties race = new Race.RaceProperties();

          race.RaceType = (Race.ERaceType)(byte)reader.GetValue(reader.GetOrdinal("dtyp"));
          race.Runs = (uint)(byte)reader.GetValue(reader.GetOrdinal("durchgaenge"));

          Logger.Debug("Reading race: {0}", race);

          races.Add(race);
        }
      }

      return races;
    }


    public List<RaceParticipant> GetRaceParticipants(Race race, bool ignoreActiveFlag = false)
    {
      List<RaceParticipant> participants = new List<RaceParticipant>();

      string sql = @"SELECT * FROM tblTeilnehmer";

      string startNumberField = null;
      string pointsField = null;
      string activeField = null;
      switch (race.RaceType)
      {
        case Race.ERaceType.DownHill:
          activeField = "dhaktiv";
          startNumberField = "startnrdh";
          pointsField = "pktedh";
          break;
        case Race.ERaceType.SuperG:
          activeField = "sgaktiv";
          startNumberField = "startnrsg";
          pointsField = "pktesg";
          break;
        case Race.ERaceType.GiantSlalom:
          activeField = "gsaktiv";
          startNumberField = "startnrgs";
          pointsField = "pktegs";
          break;
        case Race.ERaceType.Slalom:
          activeField = "slaktiv";
          startNumberField = "startnrsl";
          pointsField = "pktesl";
          break;
        case Race.ERaceType.KOSlalom:
          activeField = "ksaktiv";
          startNumberField = "startnrks";
          pointsField = "pkteks";
          break;
        case Race.ERaceType.ParallelSlalom:
          activeField = "psaktiv";
          startNumberField = "startnrps";
          pointsField = "pkteps";
          break;
      }

      if (!ignoreActiveFlag)
        sql += " WHERE " + activeField + " = true";

      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      // Execute command  
      using (OdbcDataReader reader = cmd.ExecuteReader())
      {
        while (reader.Read())
        {
          Participant participant = CreateParticipantFromDB(reader);
          uint startNo = GetValueUInt(reader, startNumberField);
          double points = GetValueDouble(reader, pointsField);

          RaceParticipant raceParticpant = new RaceParticipant(race, participant, startNo, points);
          participants.Add(raceParticpant);
        }
      }

      return participants;
    }



    public List<RunResult> GetRaceRun(Race race, uint run)
    {
      List<RunResult> runResult = new List<RunResult>();

      string sql = @"SELECT tblZeit.*, tblZeit.durchgang, tblZeit.disziplin, tblTeilnehmer.startnrsg, tblTeilnehmer.startnrgs, tblTeilnehmer.startnrsl, tblTeilnehmer.startnrks, tblTeilnehmer.startnrps "+
                   @"FROM tblTeilnehmer INNER JOIN tblZeit ON tblTeilnehmer.id = tblZeit.teilnehmer "+
                   @"WHERE(((tblZeit.durchgang) = ?) AND((tblZeit.disziplin) = ?))";

      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      cmd.Parameters.Add(new OdbcParameter("@durchgang", (int) run));
      cmd.Parameters.Add(new OdbcParameter("@disziplin", (int) race.RaceType));

      // Execute command  
      using (OdbcDataReader reader = cmd.ExecuteReader())
      {
        while (reader.Read())
        {
          // Get Participant
          uint id = (uint)(int)reader.GetValue(reader.GetOrdinal("teilnehmer"));
          Participant p = _id2Participant[id];
          RaceParticipant rp = race.GetParticipant(p);

          // Build Result
          RunResult r = new RunResult(rp);

          TimeSpan? runTime = null, startTime = null, finishTime = null;
          if (!reader.IsDBNull(reader.GetOrdinal("netto")))
            runTime = CreateTimeSpan((double)reader.GetValue(reader.GetOrdinal("netto")));
          if (!reader.IsDBNull(reader.GetOrdinal("start")))
            startTime = CreateTimeSpan((double)reader.GetValue(reader.GetOrdinal("start")));
          if (!reader.IsDBNull(reader.GetOrdinal("ziel")))
            finishTime = CreateTimeSpan((double)reader.GetValue(reader.GetOrdinal("ziel")));

          if (startTime != null || finishTime != null)
          {
            r.SetStartTime(startTime, false);
            r.SetFinishTime(finishTime, false);
          }
          if (runTime != null)
            r.SetRunTime(runTime, false);

          if (!reader.IsDBNull(reader.GetOrdinal("ergcode")))
            r.ResultCode = (RunResult.EResultCode)(byte)reader.GetValue(reader.GetOrdinal("ergcode"));

          if (!reader.IsDBNull(reader.GetOrdinal("disqualtext")))
            r.DisqualText = reader["disqualtext"].ToString();

          runResult.Add(r);
        }
      }

      return runResult;
    }


    /// <summary>
    /// Creates or updates a participant in the DataBase
    /// </summary>
    /// <param name="participant">The participant to store.</param>
    public void CreateOrUpdateParticipant(Participant participant)
    {
      // Test whether the participant exists
      uint id = GetParticipantId(participant);
      bool bNew = (id == 0);

      OdbcCommand cmd;

      if (!bNew)
      {
        string sql = @"UPDATE tblTeilnehmer " +
                     @"SET nachname = ?, vorname = ?, sex = ?, verein = ?, nation = ?, svid = ?, code = ?, klasse = ?, jahrgang = ? " +
                     @"WHERE id = ?";
        cmd = new OdbcCommand(sql, _conn);
      }
      else
      {
        id = GetNewId("tblTeilnehmer"); // Figure out the new ID

        string sql = @"INSERT INTO tblTeilnehmer (nachname, vorname, sex, verein, nation, svid, code, klasse, jahrgang, id) " +
                     @"VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?) ";
        cmd = new OdbcCommand(sql, _conn);
      }

      cmd.Parameters.Add(new OdbcParameter("@nachname", participant.Name));
      cmd.Parameters.Add(new OdbcParameter("@vorname", participant.Firstname));
      if (participant.Sex == null)
        cmd.Parameters.Add(new OdbcParameter("@sex", DBNull.Value));
      else
        cmd.Parameters.Add(new OdbcParameter("@sex", participant.Sex.Name));

      if (string.IsNullOrEmpty(participant.Club))
        cmd.Parameters.Add(new OdbcParameter("@verein", DBNull.Value));
      else
        cmd.Parameters.Add(new OdbcParameter("@verein", participant.Club));
      if (string.IsNullOrEmpty(participant.Nation))
        cmd.Parameters.Add(new OdbcParameter("@nation", DBNull.Value));
      else
        cmd.Parameters.Add(new OdbcParameter("@nation", participant.Nation));
      long svid = 0;
      if (long.TryParse(participant.SvId, out svid))
        cmd.Parameters.Add(new OdbcParameter("@svid", svid));
      else
        cmd.Parameters.Add(new OdbcParameter("@svid", DBNull.Value));

      if (string.IsNullOrEmpty(participant.Code))
        cmd.Parameters.Add(new OdbcParameter("@code", DBNull.Value));
      else
        cmd.Parameters.Add(new OdbcParameter("@code", participant.Code));

      cmd.Parameters.Add(new OdbcParameter("@klasse", GetParticipantClassId(participant.Class))); 
      cmd.Parameters.Add(new OdbcParameter("@jahrgang", participant.Year));
      cmd.Parameters.Add(new OdbcParameter("@id", (ulong)id));

      cmd.CommandType = CommandType.Text;

      try
      {
        Logger.Debug("CreateOrUpdateParticipant(), SQL: {0}", GetDebugSqlString(cmd));

        int temp = cmd.ExecuteNonQuery();
        Debug.Assert(temp == 1, "Database could not be updated");

        if (bNew)
        {
          _id2Participant.Add((uint)id, participant);
          participant.Id = id.ToString();
        }
      }
      catch(Exception e)
      {
        Logger.Warn(e, "CreateOrUpdateParticipant failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }

    public void RemoveParticipant(Participant participant)
    {
      uint id = GetParticipantId(participant);

      if (id == 0)
      {
        Logger.Debug("RemoveParticipant(), id was not found, skipping delete for participant: '{0}'", participant);
        return;
      }

      // First, delete all dependent data
      DeleteRunResultsForParticipant(participant);

      // Second, delete participant itself
      string sql = @"DELETE FROM tblTeilnehmer " +
                   @"WHERE id = ?";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      cmd.CommandType = CommandType.Text;

      cmd.Parameters.Add(new OdbcParameter("@id", id));
      try
      {
        Logger.Debug("RemoveParticipant(), SQL: {0}", GetDebugSqlString(cmd));
        int temp = cmd.ExecuteNonQuery();
        Logger.Debug("... affected rows: {0}", temp);

        // Successfully deleted, remove participant from key list
        _id2Participant.Remove(id); 
      }
      catch (Exception e)
      {
        Logger.Warn(e, "RemoveParticipant failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }


    public void CreateOrUpdateRaceParticipant(RaceParticipant raceParticipant)
    {
      // Test whether the participant exists
      uint id = GetParticipantId(raceParticipant.Participant);
      
      if (id == 0) // Store first
      {
        Debug.Assert(false, "just for testing whether this happens");
        CreateOrUpdateParticipant(raceParticipant.Participant);
        id = GetParticipantId(raceParticipant.Participant);
      }

      string sql = @"UPDATE tblTeilnehmer SET ";
      switch (raceParticipant.Race.RaceType)
      {
        case Race.ERaceType.DownHill:
          sql += " dhaktiv = true, ";
          sql += " startnrdh = ?, ";
          sql += " pktedh = ? ";
          break;
        case Race.ERaceType.SuperG:
          sql += " sgaktiv = true, ";
          sql += " startnrsg = ?, ";
          sql += " pktesg = ? ";
          break;
        case Race.ERaceType.GiantSlalom:
          sql += " gsaktiv = true, ";
          sql += " startnrgs = ?, ";
          sql += " pktegs = ? ";
          break;
        case Race.ERaceType.Slalom:
          sql += " slaktiv = true, ";
          sql += " startnrsl = ?, ";
          sql += " pktesl = ? ";
          break;
        case Race.ERaceType.KOSlalom:
          sql += " ksaktiv = true, ";
          sql += " startnrks = ?, ";
          sql += " pkteks = ? ";
          break;
        case Race.ERaceType.ParallelSlalom:
          sql += " psaktiv = true, ";
          sql += " startnrps = ?, ";
          sql += " pkteps = ? ";
          break;
      }

      sql += " WHERE id = ?";

      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      cmd.CommandType = CommandType.Text;
      cmd.Parameters.Add(new OdbcParameter("@startnr", raceParticipant.StartNumber));
      cmd.Parameters.Add(new OdbcParameter("@punkte", raceParticipant.Points));
      cmd.Parameters.Add(new OdbcParameter("@id", (ulong)id));

      try
      {
        Logger.Debug("CreateOrUpdateRaceParticipant(), SQL: {0}", GetDebugSqlString(cmd));

        int temp = cmd.ExecuteNonQuery();
        Debug.Assert(temp == 1, "Database could not be updated");
      }
      catch (Exception e)
      {
        Logger.Warn(e, "CreateOrUpdateRaceParticipant failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }

    public void RemoveRaceParticipant(RaceParticipant raceParticipant)
    {
      // Test whether the participant exists
      uint id = GetParticipantId(raceParticipant.Participant);

      if (id == 0) // Particpant not existing => no updates 
        return;

      string sql = @"UPDATE tblTeilnehmer SET ";
      switch (raceParticipant.Race.RaceType)
      {
        case Race.ERaceType.DownHill:
          sql += " dhaktiv = false ";
          break;
        case Race.ERaceType.SuperG:
          sql += " sgaktiv = false ";
          break;
        case Race.ERaceType.GiantSlalom:
          sql += " gsaktiv = false ";
          break;
        case Race.ERaceType.Slalom:
          sql += " slaktiv = false ";
          break;
        case Race.ERaceType.KOSlalom:
          sql += " ksaktiv = false ";
          break;
        case Race.ERaceType.ParallelSlalom:
          sql += " psaktiv = false ";
          break;
      }

      sql += " WHERE id = ?";

      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      cmd.CommandType = CommandType.Text;
      cmd.Parameters.Add(new OdbcParameter("@id", (ulong)id));

      try
      {
        Logger.Debug("RemoveRaceParticipant(), SQL: {0}", GetDebugSqlString(cmd));

        int temp = cmd.ExecuteNonQuery();
        Debug.Assert(temp == 1, "Database could not be updated");
      }
      catch (Exception e)
      {
        Logger.Warn(e, "RemoveRaceParticipant failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }



    /// <summary>
    /// Stores the RunResult
    /// </summary>
    /// <param name="raceRun">The correlated RaceRun the reuslt is associated with.</param>
    /// <param name="result">The RunResult to store.</param>
    public void CreateOrUpdateRunResult(Race race, RaceRun raceRun, RunResult result)
    {
      uint idParticipant = GetParticipantId(result.Participant.Participant);

      bool bNew = true;
      using (OdbcCommand command = new OdbcCommand("SELECT COUNT(*) FROM tblZeit WHERE teilnehmer = ? AND disziplin = ? AND durchgang = ?", _conn))
      {
        command.Parameters.Add(new OdbcParameter("@teilnehmer", (int)idParticipant));
        command.Parameters.Add(new OdbcParameter("@disziplin", (int)race.RaceType)); 
        command.Parameters.Add(new OdbcParameter("@durchgang", (int)raceRun.Run));
        object oId = command.ExecuteScalar();

        bNew = (oId == DBNull.Value || (int)oId == 0);
      }


      OdbcCommand cmd;
      if (!bNew)
      {
        string sql = @"UPDATE tblZeit " +
                     @"SET ergcode = ?, start = ?, ziel = ?, netto = ?, disqualtext = ? " +
                     @"WHERE teilnehmer = ? AND disziplin = ? AND durchgang = ?";
        cmd = new OdbcCommand(sql, _conn);
      }
      else
      {
        string sql = @"INSERT INTO tblZeit (ergcode, start, ziel, netto, disqualtext, teilnehmer, disziplin, durchgang) " +
                     @"VALUES (?, ?, ?, ?, ?, ?, ?, ?) ";
        cmd = new OdbcCommand(sql, _conn);
      }
      cmd.Parameters.Add(new OdbcParameter("@ergcode", (byte)result.ResultCode));
      if (result.GetStartTime() == null)
        cmd.Parameters.Add(new OdbcParameter("@start", DBNull.Value));
      else
        cmd.Parameters.Add(new OdbcParameter("@start", FractionForTimeSpan((TimeSpan)result.GetStartTime())));
      if (result.GetFinishTime() == null)
        cmd.Parameters.Add(new OdbcParameter("@ziel", DBNull.Value));
      else
        cmd.Parameters.Add(new OdbcParameter("@ziel", FractionForTimeSpan((TimeSpan)result.GetFinishTime())));
      if (result.GetRunTime(false,false) == null)
        cmd.Parameters.Add(new OdbcParameter("@netto", DBNull.Value));
      else
        cmd.Parameters.Add(new OdbcParameter("@netto", FractionForTimeSpan((TimeSpan)result.GetRunTime(false, false))));
      if (result.DisqualText == null || result.DisqualText == "")
        cmd.Parameters.Add(new OdbcParameter("@disqualtext", DBNull.Value));
      else
        cmd.Parameters.Add(new OdbcParameter("@disqualtext", result.DisqualText));

      cmd.Parameters.Add(new OdbcParameter("@teilnehmer", idParticipant));
      cmd.Parameters.Add(new OdbcParameter("@disziplin", (int)race.RaceType)); 
      cmd.Parameters.Add(new OdbcParameter("@durchgang", raceRun.Run));

      cmd.CommandType = CommandType.Text;
      try
      {
        Logger.Debug("CreateOrUpdateRunResult(), SQL: {0}", GetDebugSqlString(cmd));
        int temp = cmd.ExecuteNonQuery();
        Logger.Debug("... affected rows: {0}", temp);
        Debug.Assert(temp == 1, "Database could not be updated");
      }
      catch (Exception e)
      {
        Logger.Warn(e, "CreateOrUpdateRunResult failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }

    /// <summary>
    /// Deletes the RunResult
    /// </summary>
    /// <param name="raceRun">The correlated RaceRun the reuslt is associated with.</param>
    /// <param name="result">The RunResult to store.</param>
    public void DeleteRunResult(Race race, RaceRun raceRun, RunResult result)
    {
      uint idParticipant = GetParticipantId(result.Participant.Participant);

      if (idParticipant == 0 || raceRun.Run == 0)
        throw new Exception("DeleteRunResult is wrong");

      string sql = @"DELETE FROM tblZeit " +
                   @"WHERE teilnehmer = ? AND disziplin = ? AND durchgang = ?";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);

      cmd.Parameters.Add(new OdbcParameter("@teilnehmer", idParticipant));
      cmd.Parameters.Add(new OdbcParameter("@disziplin", (int)race.RaceType));
      cmd.Parameters.Add(new OdbcParameter("@durchgang", raceRun.Run));

      cmd.CommandType = CommandType.Text;
      try
      {
        Logger.Debug("DeleteRunResult(), SQL: {0}", GetDebugSqlString(cmd));
        int temp = cmd.ExecuteNonQuery();
        Logger.Debug("... affected rows: {0}", temp);
      }
      catch (Exception e)
      {
        Logger.Warn(e, "DeleteRunResult failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }


    /// <summary>
    /// Deletes all RunResults for a participant
    /// </summary>
    /// <param name="participant">The participant of the run rsults to be deleted.</param>
    protected void DeleteRunResultsForParticipant(Participant participant)
    {
      uint idParticipant = GetParticipantId(participant);

      if (idParticipant == 0)
        throw new Exception("DeleteRunResultsForParticipant is wrong");

      string sql = @"DELETE FROM tblZeit " +
                   @"WHERE teilnehmer = ?";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);

      cmd.Parameters.Add(new OdbcParameter("@teilnehmer", idParticipant));
      cmd.CommandType = CommandType.Text;
      try
      {
        Logger.Debug("DeleteRunResultsForParticipant(), SQL: {0}", GetDebugSqlString(cmd));
        int temp = cmd.ExecuteNonQuery();
        Logger.Debug("... affected rows: {0}", temp);
      }
      catch (Exception e)
      {
        Logger.Warn(e, "DeleteRunResultsForParticipant failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }


    public void UpdateRace(Race race, bool active)
    {
      string sql = @"UPDATE tblDisziplin " +
                    @"SET aktiv = ?, durchgaenge = ? " +
                    @"WHERE dtyp = ?";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);

      cmd.Parameters.Add(new OdbcParameter("@aktiv", active));
      cmd.Parameters.Add(new OdbcParameter("@durchgaenge", race.GetMaxRun()));
      cmd.Parameters.Add(new OdbcParameter("@dtyp", (int)race.RaceType));

      cmd.CommandType = CommandType.Text;
      try
      {
        Logger.Debug("DeleteRunResult(), SQL: {0}", GetDebugSqlString(cmd));
        int temp = cmd.ExecuteNonQuery();
        Logger.Debug("... affected rows: {0}", temp);
        Debug.Assert(temp == 1, "Database could not be updated");
      }
      catch (Exception e)
      {
        Logger.Warn(e, "DeleteRunResult failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }


    public AdditionalRaceProperties GetRaceProperties(Race race)
    {
      AdditionalRaceProperties props = new AdditionalRaceProperties();

      string sql = @"SELECT * FROM tblListenkopf WHERE disziplin = ?";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      cmd.Parameters.Add(new OdbcParameter("@disziplin", (int)race.RaceType));

      // Execute command  
      using (OdbcDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
      {
        while (reader.Read())
        {
          uint id = GetValueUInt(reader, "id");
          string value = reader["value"].ToString();

          try
          {
            switch (id)
            {
              case 0: props.Analyzer = value; break;
              case 1: break; // skip, was: timing device
              case 2: props.Organizer = value; break;
              case 3: props.RaceReferee.Name = value; break;
              case 4: props.RaceReferee.Club = value; break;
              case 5: props.RaceManager.Name = value; break;
              case 6: props.RaceManager.Club = value; break;
              case 7: props.TrainerRepresentative.Name = value; break;
              case 8: props.TrainerRepresentative.Club = value; break;

              case 15: props.CoarseName = value; break;
              case 16: props.StartHeight = int.Parse(value); break;
              case 17: props.FinishHeight = int.Parse(value); break;
              case 18: break; // skip, was: HeightDifference, can be calculated
              case 19: props.CoarseLength = int.Parse(value); break;
              case 20: props.CoarseHomologNo = value; break;

              // Run 1
              case 21: props.RaceRun1.CoarseSetter.Name = value; break;
              case 22: props.RaceRun1.CoarseSetter.Club = value; break;
              case 23: props.RaceRun1.Forerunner1.Name = value; break;
              case 24: props.RaceRun1.Forerunner1.Club = value; break;
              case 25: props.RaceRun1.Forerunner2.Name = value; break;
              case 26: props.RaceRun1.Forerunner2.Club = value; break;
              case 27: props.RaceRun1.Forerunner3.Name = value; break;
              case 28: props.RaceRun1.Forerunner3.Club = value; break;
              case 29: props.RaceRun1.Gates = int.Parse(value); break;
              case 30: props.RaceRun1.Turns = int.Parse(value); break;
              case 31: props.RaceRun1.StartTime = value; break;

              // Run 2
              case 32: props.RaceRun2.CoarseSetter.Name = value; break;
              case 33: props.RaceRun2.CoarseSetter.Club = value; break;
              case 34: props.RaceRun2.Forerunner1.Name = value; break;
              case 35: props.RaceRun2.Forerunner1.Club = value; break;
              case 36: props.RaceRun2.Forerunner2.Name = value; break;
              case 37: props.RaceRun2.Forerunner2.Club = value; break;
              case 38: props.RaceRun2.Forerunner3.Name = value; break;
              case 39: props.RaceRun2.Forerunner3.Club = value; break;
              case 40: props.RaceRun2.Gates = int.Parse(value); break;
              case 41: props.RaceRun2.Turns = int.Parse(value); break;
              case 42: props.RaceRun2.StartTime = value; break;

              case 43: props.Weather = value; break;
              case 44: props.Snow = value; break;
              case 45: props.TempStart = value; break;
              case 46: props.TempFinish = value; break;

              default:
                break;
            }
          }
          catch (InvalidCastException){} 
        }
      }

      string sql2 = @"SELECT * FROM tblBewerb";
      OdbcCommand command2 = new OdbcCommand(sql2, _conn);
      using (OdbcDataReader reader = command2.ExecuteReader())
      {
        if (reader.Read())
        {
          props.Location = reader["ort"].ToString();
        }
      }


      string sql3 = @"SELECT * FROM tblDisziplin WHERE dtyp = ?";
      OdbcCommand command3 = new OdbcCommand(sql3, _conn);
      command3.Parameters.Add(new OdbcParameter("@dtyp", (int)race.RaceType));
      using (OdbcDataReader reader = command3.ExecuteReader())
      {
        if (reader.Read())
        {
          if (!reader.IsDBNull(reader.GetOrdinal("bewerbsnummer")))
            props.RaceNumber = reader["bewerbsnummer"].ToString();
          if (!reader.IsDBNull(reader.GetOrdinal("bewerbsbezeichnung")))
            props.Description = reader["bewerbsbezeichnung"].ToString();
          if (!reader.IsDBNull(reader.GetOrdinal("datum_startliste")))
            props.DateStartList = (DateTime)reader.GetValue(reader.GetOrdinal("datum_startliste"));
          if (!reader.IsDBNull(reader.GetOrdinal("datum_rangliste")))
            props.DateResultList = (DateTime)reader.GetValue(reader.GetOrdinal("datum_rangliste"));
        }
      }

      return props;
    }


    public void StoreRaceProperties(Race race, AdditionalRaceProperties props)
    {
      // Name and Number are stored in tblDisziplin
      storeRacePropertyInternal(race, props.RaceNumber, props.Description, props.DateStartList, props.DateResultList);

      // Location is stored in tblBewerb
      storeRacePropertyInternal(race, props.Location);

      storeRacePropertyInternal(race,  0, props.Analyzer);
      storeRacePropertyInternal(race,  2, props.Organizer);
      storeRacePropertyInternal(race,  3, props.RaceReferee.Name );
      storeRacePropertyInternal(race,  4, props.RaceReferee.Club );
      storeRacePropertyInternal(race,  5, props.RaceManager.Name );
      storeRacePropertyInternal(race,  6, props.RaceManager.Club );
      storeRacePropertyInternal(race,  7, props.TrainerRepresentative.Name );
      storeRacePropertyInternal(race,  8, props.TrainerRepresentative.Club );

      // Coarse
      storeRacePropertyInternal(race, 15, props.CoarseName );
      storeRacePropertyInternal(race, 16, props.StartHeight.ToString() );
      storeRacePropertyInternal(race, 17, props.FinishHeight.ToString() );
      storeRacePropertyInternal(race, 18, (props.StartHeight - props.FinishHeight).ToString());
      storeRacePropertyInternal(race, 19, props.CoarseLength.ToString() );
      storeRacePropertyInternal(race, 20, props.CoarseHomologNo );

      // Run 1
      storeRacePropertyInternal(race, 21, props.RaceRun1.CoarseSetter.Name );
      storeRacePropertyInternal(race, 22, props.RaceRun1.CoarseSetter.Club );
      storeRacePropertyInternal(race, 23, props.RaceRun1.Forerunner1.Name );
      storeRacePropertyInternal(race, 24, props.RaceRun1.Forerunner1.Club );
      storeRacePropertyInternal(race, 25, props.RaceRun1.Forerunner2.Name );
      storeRacePropertyInternal(race, 26, props.RaceRun1.Forerunner2.Club );
      storeRacePropertyInternal(race, 27, props.RaceRun1.Forerunner3.Name );
      storeRacePropertyInternal(race, 28, props.RaceRun1.Forerunner3.Club );
      storeRacePropertyInternal(race, 29, props.RaceRun1.Gates.ToString() );
      storeRacePropertyInternal(race, 30, props.RaceRun1.Turns.ToString() );
      storeRacePropertyInternal(race, 31, props.RaceRun1.StartTime );

      // Run 2
      storeRacePropertyInternal(race, 32, props.RaceRun2.CoarseSetter.Name );
      storeRacePropertyInternal(race, 33, props.RaceRun2.CoarseSetter.Club );
      storeRacePropertyInternal(race, 34, props.RaceRun2.Forerunner1.Name );
      storeRacePropertyInternal(race, 35, props.RaceRun2.Forerunner1.Club );
      storeRacePropertyInternal(race, 36, props.RaceRun2.Forerunner2.Name );
      storeRacePropertyInternal(race, 37, props.RaceRun2.Forerunner2.Club );
      storeRacePropertyInternal(race, 38, props.RaceRun2.Forerunner3.Name );
      storeRacePropertyInternal(race, 39, props.RaceRun2.Forerunner3.Club );
      storeRacePropertyInternal(race, 40, props.RaceRun2.Gates.ToString() );
      storeRacePropertyInternal(race, 41, props.RaceRun2.Turns.ToString() );
      storeRacePropertyInternal(race, 42, props.RaceRun2.StartTime );

      // Weather
      storeRacePropertyInternal(race, 43, props.Weather );
      storeRacePropertyInternal(race, 44, props.Snow );
      storeRacePropertyInternal(race, 45, props.TempStart );
      storeRacePropertyInternal(race, 46, props.TempFinish );
    }


    private void storeRacePropertyInternal(Race race, uint id, string value)
    {
      // Delete and Insert
      OdbcCommand cmdDelete = null, cmdInsert = null;
      try
      {
        string sqlDelete = @"DELETE from tblListenkopf WHERE id = ? AND disziplin = ?";
        cmdDelete = new OdbcCommand(sqlDelete, _conn);
        cmdDelete.Parameters.Add(new OdbcParameter("@id", (long)id));
        cmdDelete.Parameters.Add(new OdbcParameter("@disziplin", (int)race.RaceType));
        cmdDelete.CommandType = CommandType.Text;
        int temp1 = cmdDelete.ExecuteNonQuery();

        string sqlInsert = @"INSERT INTO tblListenkopf (id, disziplin, [value]) VALUES (?, ?, ?)";
        cmdInsert = new OdbcCommand(sqlInsert, _conn);
        cmdInsert.Parameters.Add(new OdbcParameter("@id", (long)id));
        cmdInsert.Parameters.Add(new OdbcParameter("@disziplin", (int)race.RaceType));
        cmdInsert.Parameters.Add(new OdbcParameter("@value", value));
        cmdInsert.CommandType = CommandType.Text;
        int temp2 = cmdInsert.ExecuteNonQuery();
      }
      catch (Exception e)
      {
        Logger.Warn(e, "storeRacePropertyInternal failed, SQL delete: {0}, SQL insert: {0}", GetDebugSqlString(cmdDelete), GetDebugSqlString(cmdDelete));
      }
    }


    private void storeRacePropertyInternal(Race r, string raceNumber, string description, DateTime? dateStart, DateTime? dateResult)
    {
      string sql = @"UPDATE tblDisziplin " +
                    @"SET bewerbsnummer = , bewerbsbezeichnung = `?, datum_startliste = ?, datum_rangliste = ? " +
                    @"WHERE dtyp = ?";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);

      if (string.IsNullOrEmpty(raceNumber))
        cmd.Parameters.Add(new OdbcParameter("@bewerbsnummer", DBNull.Value));
      else
        cmd.Parameters.Add(new OdbcParameter("@bewerbsnummer", raceNumber));

      if (string.IsNullOrEmpty(description))
        cmd.Parameters.Add(new OdbcParameter("@bewerbsbezeichnung", DBNull.Value));
      else
        cmd.Parameters.Add(new OdbcParameter("@bewerbsbezeichnung", description));

      if (dateStart == null)
        cmd.Parameters.Add(new OdbcParameter("@datum_startliste", DBNull.Value));
      else
        cmd.Parameters.Add(new OdbcParameter("@datum_startliste", ((DateTime)dateStart).Date));

      if (dateResult == null)
        cmd.Parameters.Add(new OdbcParameter("@datum_rangliste", DBNull.Value));
      else
        cmd.Parameters.Add(new OdbcParameter("@datum_rangliste", ((DateTime)dateResult).Date));

      cmd.Parameters.Add(new OdbcParameter("@dtyp", (int)r.RaceType));

      cmd.CommandType = CommandType.Text;
      try
      {
        Logger.Debug("storeRacePropertyInternal(), SQL: {0}", GetDebugSqlString(cmd));
        int temp = cmd.ExecuteNonQuery();
        Logger.Debug("... affected rows: {0}", temp);
        Debug.Assert(temp == 1, "Database could not be updated");
      }
      catch (Exception e)
      {
        Logger.Warn(e, "storeRacePropertyInternal failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }


    private void storeRacePropertyInternal(Race r, string location)
    {
      string sql = @"UPDATE tblBewerb " +
                    @"SET ort = ?";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);

      if (location == null)
        cmd.Parameters.Add(new OdbcParameter("@ort", DBNull.Value));
      else
        cmd.Parameters.Add(new OdbcParameter("@ort", location));

      cmd.CommandType = CommandType.Text;
      try
      {
        Logger.Debug("storeRacePropertyInternal(), SQL: {0}", GetDebugSqlString(cmd));
        int temp = cmd.ExecuteNonQuery();
        Logger.Debug("... affected rows: {0}", temp);
        Debug.Assert(temp == 1, "Database could not be updated");
      }
      catch (Exception e)
      {
        Logger.Warn(e, "storeRacePropertyInternal failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }


    public CompetitionProperties GetCompetitionProperties()
    {
      CompetitionProperties competitionProps = new CompetitionProperties();

      string sql2 = @"SELECT * FROM tblBewerb";
      OdbcCommand cmd = new OdbcCommand(sql2, _conn);
      using (OdbcDataReader reader = cmd.ExecuteReader())
      {
        if (reader.Read())
        {
          competitionProps.Name = reader["bname"].ToString();

          if (!reader.IsDBNull(reader.GetOrdinal("typ")))
            competitionProps.Type = (CompetitionProperties.ECompetitionType)(byte)reader.GetValue(reader.GetOrdinal("typ"));

          competitionProps.WithPoints = (bool)reader.GetValue(reader.GetOrdinal("punktewertung"));

          if (reader["nation"].ToString().Length == 3)
            competitionProps.Nation = reader["nation"].ToString();
          else
            competitionProps.Nation = null;

          if (!reader.IsDBNull(reader.GetOrdinal("saison")))
            competitionProps.Saeson = (uint)((short)reader.GetValue(reader.GetOrdinal("saison")));

          competitionProps.KlassenWertung = (bool)reader.GetValue(reader.GetOrdinal("klassenwertung"));
          competitionProps.MannschaftsWertung = (bool)reader.GetValue(reader.GetOrdinal("mannschaftswertung"));
          competitionProps.ZwischenZeit = (bool)reader.GetValue(reader.GetOrdinal("zwischenzeiterfassung"));
          competitionProps.FreierListenKopf = (bool)reader.GetValue(reader.GetOrdinal("freien_lk_benutzen"));
          competitionProps.FISSuperCombi = (bool)reader.GetValue(reader.GetOrdinal("supercombi"));

          competitionProps.FieldActiveYear = (bool)reader.GetValue(reader.GetOrdinal("jahrgang_aktiv"));
          competitionProps.FieldActiveClub = (bool)reader.GetValue(reader.GetOrdinal("verein_aktiv"));
          competitionProps.FieldActiveNation = (bool)reader.GetValue(reader.GetOrdinal("nation_aktiv"));
          competitionProps.FieldActiveCode = (bool)reader.GetValue(reader.GetOrdinal("code_aktiv"));

          if (!reader.IsDBNull(reader.GetOrdinal("nenngeld")))
            competitionProps.Nenngeld = (double)((decimal)reader.GetValue(reader.GetOrdinal("nenngeld")));
          else
            competitionProps.Nenngeld = 0.0;
        }
      }

      return competitionProps;
    }

    public void UpdateCompetitionProperties(CompetitionProperties competitionProps)
    {
      string sql = @"UPDATE tblBewerb SET " +
                    @"bname = ?, " +
                    @"typ = ?, " +
                    @"punktewertung = ?, " +
                    @"nation = ?, " +
                    @"saison = ?, " +
                    @"klassenwertung = ?, " +
                    @"mannschaftswertung = ?, " +
                    @"zwischenzeiterfassung = ?, " +
                    @"freien_lk_benutzen = ?, " +
                    @"supercombi = ?, " +
                    @"jahrgang_aktiv = ?, " +
                    @"verein_aktiv = @´?, " +
                    @"nation_aktiv = ?, " +
                    @"code_aktiv = ?, " +
                    @"nenngeld = ?";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      if (string.IsNullOrEmpty(competitionProps.Name))
        cmd.Parameters.Add(new OdbcParameter("@bname", ""));
      else
        cmd.Parameters.Add(new OdbcParameter("@bname", competitionProps.Name));

      cmd.Parameters.Add(new OdbcParameter("@typ", (byte)competitionProps.Type));
      
      cmd.Parameters.Add(new OdbcParameter("@punktewertung", competitionProps.WithPoints));
      
      if (string.IsNullOrEmpty(competitionProps.Nation) || competitionProps.Nation.Length != 3)
        cmd.Parameters.Add(new OdbcParameter("@nation", DBNull.Value));
      else
        cmd.Parameters.Add("@nation", OdbcType.Char).Value = competitionProps.Nation;
      
      cmd.Parameters.Add(new OdbcParameter("@saison", (short)competitionProps.Saeson));

      cmd.Parameters.Add(new OdbcParameter("@klassenwertung", competitionProps.KlassenWertung));
      cmd.Parameters.Add(new OdbcParameter("@mannschaftswertung", competitionProps.MannschaftsWertung));
      cmd.Parameters.Add(new OdbcParameter("@zwischenzeiterfassung", competitionProps.ZwischenZeit));
      cmd.Parameters.Add(new OdbcParameter("@freien_lk_benutzen", competitionProps.FreierListenKopf));
      cmd.Parameters.Add(new OdbcParameter("@supercombi", competitionProps.FISSuperCombi));

      cmd.Parameters.Add(new OdbcParameter("@jahrgangAktiv", competitionProps.FieldActiveYear));
      cmd.Parameters.Add(new OdbcParameter("@vereinAktiv", competitionProps.FieldActiveClub));
      cmd.Parameters.Add(new OdbcParameter("@natAktiv", competitionProps.FieldActiveNation));
      cmd.Parameters.Add(new OdbcParameter("@codeAktiv", competitionProps.FieldActiveCode));

      cmd.Parameters.Add(new OdbcParameter("@nenngeld", (decimal)competitionProps.Nenngeld));

      cmd.CommandType = CommandType.Text;
      try
      {
        Logger.Debug("UpdateCompetitionProperties(), SQL: {0}", GetDebugSqlString(cmd));
        int temp = cmd.ExecuteNonQuery();
        Logger.Debug("... affected rows: {0}", temp);
        Debug.Assert(temp == 1, "Database could not be updated");
      }
      catch (Exception e)
      {
        Logger.Warn(e, "UpdateCompetitionProperties() failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }


    public void CreateOrUpdateTimestamp(RaceRun raceRun, Timestamp timestamp)
    {
      string kanal(EMeasurementPoint m)
      {
        switch (m)
        {
          case EMeasurementPoint.Start: return "START";
          case EMeasurementPoint.Finish: return "ZIEL";
          default: return "---";
        }
      }


      bool bNew = true;
      using (OdbcCommand cmdQ = new OdbcCommand("SELECT COUNT(*) FROM RHTimestamps WHERE disziplin = ? AND durchgang = ? AND zeit = ? AND kanal = ?", _conn))
      {
        cmdQ.Parameters.Add(new OdbcParameter("@disziplin", (int)raceRun.GetRace().RaceType));
        cmdQ.Parameters.Add(new OdbcParameter("@durchgang", (int)raceRun.Run));
        cmdQ.Parameters.Add(new OdbcParameter("@zeit", FractionForTimeSpan(timestamp.Time)));
        cmdQ.Parameters.Add(new OdbcParameter("@kanal", kanal(timestamp.MeasurementPoint)));
        object oId = cmdQ.ExecuteScalar();

        bNew = (oId == DBNull.Value || (int)oId == 0);
      }

      OdbcCommand cmd;
      if (!bNew)
      {
        string sql = @"UPDATE RHTimestamps " +
                     @"SET startnummer = ?, valid = ? " +
                     @"WHERE disziplin = ? AND durchgang = ? AND zeit = ? AND kanal = ?";
        cmd = new OdbcCommand(sql, _conn);
      }
      else
      {
        string sql = @"INSERT INTO RHTimestamps (startnummer, valid, disziplin, durchgang, zeit, kanal) " +
                     @"VALUES (?, ?, ?, ?, ?, ?) ";
        cmd = new OdbcCommand(sql, _conn);
      }
      cmd.Parameters.Add(new OdbcParameter("@startnummer", timestamp.StartNumber));
      cmd.Parameters.Add(new OdbcParameter("@valid", timestamp.Valid));
      cmd.Parameters.Add(new OdbcParameter("@disziplin", (int)raceRun.GetRace().RaceType));
      cmd.Parameters.Add(new OdbcParameter("@durchgang", raceRun.Run));
      cmd.Parameters.Add(new OdbcParameter("@zeit", FractionForTimeSpan(timestamp.Time)));
      cmd.Parameters.Add(new OdbcParameter("@kanal", kanal(timestamp.MeasurementPoint)));

      cmd.CommandType = CommandType.Text;
      try
      {
        Logger.Debug("CreateOrUpdateTimestamp(), SQL: {0}", GetDebugSqlString(cmd));
        int temp = cmd.ExecuteNonQuery();
        Logger.Debug("... affected rows: {0}", temp);
        Debug.Assert(temp == 1, "Database could not be updated");
      }
      catch (Exception e)
      {
        Logger.Warn(e, "CreateOrUpdateTimestamp failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }

    public List<Timestamp> GetTimestamps(Race race, uint run)
    {
      EMeasurementPoint measurementPoint(string dbText){
        switch (dbText)
        {
          case "START": return EMeasurementPoint.Start;
          case "ZIEL": return EMeasurementPoint.Finish;
        }
        return EMeasurementPoint.Undefined;
      }


      List<Timestamp> result = new List<Timestamp>();

      string sql = @"SELECT * FROM RHTimestamps " +
                   @"WHERE disziplin = ? AND durchgang = ?";

      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      cmd.Parameters.Add(new OdbcParameter("@disziplin", (int)race.RaceType));
      cmd.Parameters.Add(new OdbcParameter("@durchgang", (int) run));

      // Execute command  
      using (OdbcDataReader reader = cmd.ExecuteReader())
      {
        while (reader.Read())
        {
          TimeSpan? time = null;
          if (!reader.IsDBNull(reader.GetOrdinal("zeit")))
            time = Database.CreateTimeSpan((double)reader.GetValue(reader.GetOrdinal("zeit")));

          bool valid = false;
          if (!reader.IsDBNull(reader.GetOrdinal("valid")))
            valid = reader.GetBoolean(reader.GetOrdinal("valid"));

            EMeasurementPoint mp = measurementPoint(reader["kanal"].ToString());
          uint stnr = 0;
          if (!reader.IsDBNull(reader.GetOrdinal("startnummer")))
            stnr = (uint)(int)reader.GetValue(reader.GetOrdinal("startnummer"));

          if (time != null)
            result.Add(new Timestamp((TimeSpan)time, mp, stnr, valid));
        }
      }

      return result;
    }

    public void RemoveTimestamp(RaceRun raceRun, Timestamp timestamp)
    {
      throw new NotImplementedException();
    }



    #endregion

    #region Store / Get Key Value
    public void StoreKeyValue(string key, string value)
    {
      // Delete and Insert
      OdbcCommand cmdDelete = null, cmdInsert = null;
      try
      {
        string sqlDelete = @"DELETE from RHMisc WHERE [key] = ?";
        cmdDelete = new OdbcCommand(sqlDelete, _conn);
        cmdDelete.Parameters.Add(new OdbcParameter("@key", key));
        cmdDelete.CommandType = CommandType.Text;
        int temp1 = cmdDelete.ExecuteNonQuery();

        string sqlInsert = @"INSERT INTO RHMisc ([key], [val]) VALUES (?, ?)";
        cmdInsert = new OdbcCommand(sqlInsert, _conn);
        cmdInsert.Parameters.Add(new OdbcParameter("@key", key));
        cmdInsert.Parameters.Add(new OdbcParameter("@val", value));
        cmdInsert.CommandType = CommandType.Text;
        int temp2 = cmdInsert.ExecuteNonQuery();
      }
      catch (Exception e)
      {
        Logger.Warn(e, "StoreKeyValue failed, SQL delete: {0}, SQL insert: {0}", GetDebugSqlString(cmdDelete), GetDebugSqlString(cmdDelete));
      }
    }

    public string GetKeyValue(string key)
    {
      string value = null;
      string sql = @"SELECT * FROM RHMisc WHERE [key] = ?";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      cmd.Parameters.Add(new OdbcParameter("@key", key));

      // Execute command  
      using (OdbcDataReader reader = cmd.ExecuteReader())
      {
        if (reader.Read())
        {
          value = reader["val"].ToString();
        }
      }

      return value;
    }

    #endregion

    #region Internal Implementation

    /* ************************ Participant ********************* */
    private Participant CreateParticipantFromDB(OdbcDataReader reader)
    {
      uint id = (uint)(int)reader.GetValue(reader.GetOrdinal("id"));

      if (_id2Participant.ContainsKey(id))
        return _id2Participant[id];
      else
      {
        Participant p = new Participant
        {
          Id = reader["id"].ToString(),
          Name = reader["nachname"].ToString(),
          Firstname = reader["vorname"].ToString(),
          Sex = GetParticipantCategory(reader["sex"].ToString()),
          Club = reader["verein"].ToString(),
          Nation = reader["nation"].ToString(),
          SvId = reader["svid"].ToString(),
          Code = reader["code"].ToString(),
          Class = GetParticipantClass(GetValueUInt(reader, "klasse")),
          Year = GetValueUInt(reader, "jahrgang"),
        };
        _id2Participant.Add(id, p);

        return p;
      }
    }

    private uint GetParticipantId(Participant participant)
    {
      return _id2Participant.Where(x => x.Value == participant).FirstOrDefault().Key;
    }





    /* ************************ Groups ********************* */
    public void ReadParticipantGroups()
    {
      if (_id2ParticipantGroups.Count() > 0)
        return;

      string sql = @"SELECT * FROM tblGruppe";

      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      // Execute command  
      using (OdbcDataReader reader = cmd.ExecuteReader())
      {
        while (reader.Read())
        {
          CreateParticipantGroupFromDB(reader);
        }
      }
    }


    private ParticipantGroup CreateParticipantGroupFromDB(OdbcDataReader reader)
    {
      uint id = (uint)(int)reader.GetValue(reader.GetOrdinal("id"));

      if (_id2ParticipantGroups.ContainsKey(id))
        return _id2ParticipantGroups[id];
      else
      {
        ParticipantGroup p = new ParticipantGroup(
          reader["id"].ToString(),
          reader["grpname"].ToString(),
          GetValueUInt(reader, "sortpos")
        );
        _id2ParticipantGroups.Add(id, p);

        return p;
      }
    }

    private uint GetParticipantGroupId(ParticipantGroup group)
    {
      ReadParticipantGroups();
      return _id2ParticipantGroups.Where(x => x.Value == group).FirstOrDefault().Key;
    }

    private ParticipantGroup GetParticipantGroup(uint id)
    {
      ReadParticipantGroups();
      
      if (_id2ParticipantGroups.ContainsKey(id))
        return _id2ParticipantGroups[id];

      return null;
    }

    public void CreateOrUpdateGroup(ParticipantGroup g)
    {
      // Test whether the participant exists
      uint id = GetParticipantGroupId(g);
      bool bNew = (id == 0);

      OdbcCommand cmd;
      if (!bNew)
      {
        string sql = @"UPDATE tblGruppe " +
                     @"SET grpname = ?, sortpos = ? " +
                     @"WHERE id = ?";
        cmd = new OdbcCommand(sql, _conn);
      }
      else
      {
        id = GetNewId("tblGruppe"); // Figure out the new ID

        string sql = @"INSERT INTO tblGruppe (grpname, sortpos, id) " +
                     @"VALUES (?, ?, ?) ";
        cmd = new OdbcCommand(sql, _conn);
      }

      cmd.Parameters.Add(new OdbcParameter("@grpname", g.Name));
      cmd.Parameters.Add(new OdbcParameter("@sortpos", g.SortPos));
      cmd.Parameters.Add(new OdbcParameter("@id", (ulong)id));
      cmd.CommandType = CommandType.Text;

      try
      {
        Logger.Debug("CreateOrUpdateGroup(), SQL: {0}", GetDebugSqlString(cmd));

        int temp = cmd.ExecuteNonQuery();
        Debug.Assert(temp == 1, "Database could not be updated");

        if (bNew)
        {
          _id2ParticipantGroups.Add((uint)id, g);
          g.Id = id.ToString();
        }
      }
      catch (Exception e)
      {
        Logger.Warn(e, "CreateOrUpdateGroup failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }

    public void RemoveGroup(ParticipantGroup g)
    {
      uint id = GetParticipantGroupId(g);

      if (id == 0)
        throw new Exception("RemoveGroup: id not found");

      string sql = @"DELETE FROM tblGruppe " +
                   @"WHERE id = ?";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      cmd.CommandType = CommandType.Text;

      cmd.Parameters.Add(new OdbcParameter("@id", id));
      try
      {
        Logger.Debug("RemoveGroup(), SQL: {0}", GetDebugSqlString(cmd));
        int temp = cmd.ExecuteNonQuery();
        Logger.Debug("... affected rows: {0}", temp);
      }
      catch (Exception e)
      {
        Logger.Warn(e, "RemoveGroup failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }


    /* ************************ Classes ********************* */
    public void ReadParticipantClasses()
    {
      if (_id2ParticipantClasses.Count() > 0)
        return;

      string sql = @"SELECT * FROM tblKlasse";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      // Execute command  
      using (OdbcDataReader reader = cmd.ExecuteReader())
      {
        while (reader.Read())
        {
          CreateParticipantClassFromDB(reader);
        }
      }
    }

    public void CreateOrUpdateClass(ParticipantClass c)
    {
      // Test whether the participant exists
      uint id = GetParticipantClassId(c);
      bool bNew = (id == 0);

      OdbcCommand cmd;
      if (!bNew)
      {
        string sql = @"UPDATE tblKlasse " +
                     @"SET klname = ?, geschlecht = ?, bis_jahrgang = ?, gruppe = ?, sortpos = ? " +
                     @"WHERE id = ?";
        cmd = new OdbcCommand(sql, _conn);
      }
      else
      {
        id = GetNewId("tblKlasse"); // Figure out the new ID

        string sql = @"INSERT INTO tblKlasse (klname, geschlecht, bis_jahrgang, gruppe, sortpos, id) " +
                     @"VALUES (?, ?, ?, ?, ?, ?) ";
        cmd = new OdbcCommand(sql, _conn);
      }

      uint gid = GetParticipantGroupId(c.Group);
      cmd.Parameters.Add(new OdbcParameter("@klname", c.Name));
      cmd.Parameters.Add(new OdbcParameter("@geschlecht", c.Sex == null ? (object)DBNull.Value : (object)c.Sex.Name));
      cmd.Parameters.Add(new OdbcParameter("@bis_jahrgang", c.Year));
      if (gid == 0)
        cmd.Parameters.Add(new OdbcParameter("@gruppe", DBNull.Value));
      else
        cmd.Parameters.Add(new OdbcParameter("@gruppe", gid));
      cmd.Parameters.Add(new OdbcParameter("@sortpos", c.SortPos));
      cmd.Parameters.Add(new OdbcParameter("@id", (ulong)id));
      cmd.CommandType = CommandType.Text;

      try
      {
        Logger.Debug("CreateOrUpdateClass(), SQL: {0}", GetDebugSqlString(cmd));

        int temp = cmd.ExecuteNonQuery();
        Debug.Assert(temp == 1, "Database could not be updated");

        if (bNew)
        {
          _id2ParticipantClasses.Add((uint)id, c);
          c.Id = id.ToString();
        }
      }
      catch (Exception e)
      {
        Logger.Warn(e, "CreateOrUpdateClass failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }

    public void RemoveClass(ParticipantClass c)
    {
      uint id = GetParticipantClassId(c);

      if (id == 0)
        throw new Exception("RemoveClass: id not found");

      string sql = @"DELETE FROM tblKlasse " +
                   @"WHERE id = ?";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      cmd.CommandType = CommandType.Text;

      cmd.Parameters.Add(new OdbcParameter("@id", id));
      try
      {
        Logger.Debug("RemoveClass(), SQL: {0}", GetDebugSqlString(cmd));
        int temp = cmd.ExecuteNonQuery();
        Logger.Debug("... affected rows: {0}", temp);
      }
      catch (Exception e)
      {
        Logger.Warn(e, "RemoveClass failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }


    private ParticipantClass CreateParticipantClassFromDB(OdbcDataReader reader)
    {
      uint id = (uint)(int)reader.GetValue(reader.GetOrdinal("id"));

      if (_id2ParticipantClasses.ContainsKey(id))
        return _id2ParticipantClasses[id];
      else
      {
        ParticipantClass p = new ParticipantClass(
          reader["id"].ToString(),
          GetParticipantGroup(GetValueUInt(reader, "gruppe")),
          reader["klname"].ToString(),
          GetParticipantCategory(reader["geschlecht"].ToString()),
          GetValueUInt(reader, "bis_jahrgang"),
          GetValueUInt(reader, "sortpos")
        );
        _id2ParticipantClasses.Add(id, p);

        return p;
      }
    }

    private uint GetParticipantClassId(ParticipantClass participantClass)
    {
      ReadParticipantClasses();
      return _id2ParticipantClasses.Where(x => x.Value == participantClass).FirstOrDefault().Key;
    }

    private ParticipantClass GetParticipantClass(uint id)
    {
      ReadParticipantClasses();
      if (_id2ParticipantClasses.ContainsKey(id))
        return _id2ParticipantClasses[id];

      return null;
    }



    /* ************************ Category ********************* */
    private void ReadParticipantCategories()
    {
      if (_id2ParticipantCategory.Count() > 0)
        return;

      string sql = @"SELECT * FROM tblKategorie";

      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      // Execute command  
      using (OdbcDataReader reader = cmd.ExecuteReader())
      {
        while (reader.Read())
        {
          CreateParticipantCategoryFromDB(reader);
        }
      }
    }


    private ParticipantCategory CreateParticipantCategoryFromDB(OdbcDataReader reader)
    {
      char id = ConvertToParticipantCategoryId(reader["kat"].ToString());

      if (_id2ParticipantCategory.ContainsKey(id))
        return _id2ParticipantCategory[id];
      else
      {
        ParticipantCategory p = new ParticipantCategory(
          ConvertToParticipantCategoryId(reader["kat"].ToString()),
          reader["kname"].ToString(),
          GetValueUInt(reader, "sortpos"),
          reader["RHSynonyms"].ToString()
        );
        _id2ParticipantCategory.Add(id, p);

        return p;
      }
    }

    private char GetParticipantCategoryId(ParticipantCategory cat)
    {
      ReadParticipantGroups();
      return _id2ParticipantCategory.Where(x => x.Value == cat).FirstOrDefault().Key;
    }

    private char ConvertToParticipantCategoryId(string id)
    {
      if (id.Length != 1)
        return char.MinValue;
      return id[0];
    }

    private ParticipantCategory GetParticipantCategory(string id)
    {
      return GetParticipantCategory(ConvertToParticipantCategoryId(id));
    }

    private ParticipantCategory GetParticipantCategory(char id)
    {
      ReadParticipantCategories();

      if (_id2ParticipantCategory.ContainsKey(id))
        return _id2ParticipantCategory[id];

      return null;
    }

    public void CreateOrUpdateCategory(ParticipantCategory c)
    {
      // Test whether the participant exists
      char id = GetParticipantCategoryId(c);

      // Check whether category already existed
      string sqlQuery = @"SELECT COUNT(*) FROM tblKategorie WHERE kat = ?";
      bool bNew;
      using (OdbcCommand cmdQuery = new OdbcCommand(sqlQuery, _conn))
      {
        cmdQuery.Parameters.Add(new OdbcParameter("@id", id));
        bNew = ((int)cmdQuery.ExecuteScalar() == 0);
      }

      OdbcCommand cmd;
      if (!bNew)
      {
        string sql = @"UPDATE tblKategorie " +
                     @"SET kname = ?, sortpos = ?, RHSynonyms = ? " +
                     @"WHERE kat = ?";
        cmd = new OdbcCommand(sql, _conn);
      }
      else
      {
        Debug.Assert(id == char.MinValue);
        id = c.Name; // Name is the ID
        string sql = @"INSERT INTO tblKategorie (kname, sortpos, RHSynonyms, kat) " +
                     @"VALUES (?, ?, ?, ?) ";
        cmd = new OdbcCommand(sql, _conn);
      }

      cmd.Parameters.Add(new OdbcParameter("@kname", c.PrettyName));
      cmd.Parameters.Add(new OdbcParameter("@sortpos", c.SortPos));
      cmd.Parameters.Add(new OdbcParameter("@synonyms", string.IsNullOrEmpty(c.Synonyms)? (object)DBNull.Value : (object)c.Synonyms));
      cmd.Parameters.Add(new OdbcParameter("@id", id));
      cmd.CommandType = CommandType.Text;

      try
      {
        Logger.Debug("CreateOrUpdateCategory(), SQL: {0}", GetDebugSqlString(cmd));

        int temp = cmd.ExecuteNonQuery();
        Debug.Assert(temp == 1, "Database could not be updated");

        if (bNew)
        {
          _id2ParticipantCategory.Add(id, c);
        }
      }
      catch (Exception e)
      {
        Logger.Warn(e, "CreateOrUpdateCategory failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }

    public void RemoveCategory(ParticipantCategory c)
    {
      char id = GetParticipantCategoryId(c);

      if (id == char.MinValue)
        throw new Exception("RemoveCategory: id not found");

      string sql = @"DELETE FROM tblKategorie " +
                   @"WHERE kat = ?";
      OdbcCommand cmd = new OdbcCommand(sql, _conn);
      cmd.CommandType = CommandType.Text;

      cmd.Parameters.Add(new OdbcParameter("@id", id));
      try
      {
        Logger.Debug("RemoveCategory(), SQL: {0}", GetDebugSqlString(cmd));
        int temp = cmd.ExecuteNonQuery();
        Logger.Debug("... affected rows: {0}", temp);
      }
      catch (Exception e)
      {
        Logger.Warn(e, "RemoveCategory failed, SQL: {0}", GetDebugSqlString(cmd));
      }
    }






    private static int GetLatestAutonumber(OdbcConnection connection)
    {
      using (OdbcCommand command = new OdbcCommand("SELECT @@IDENTITY;", connection))
      {
        return (int)command.ExecuteScalar();
      }
    }


    private uint GetNewId(string table, string field = "id")
    {
      uint id = 0;
      using (OdbcCommand command = new OdbcCommand(string.Format("SELECT MAX({0}) FROM {1};", field, table), _conn))
      {
        object oId = command.ExecuteScalar();
        if (oId == DBNull.Value)
          id = 0;
        else
          id = Convert.ToUInt32(oId);
        id++;
      }

      return id;
    }


    static private uint GetValueUInt(OdbcDataReader reader, string field)
    {
      if (!reader.IsDBNull(reader.GetOrdinal(field)))
      {
        var v = reader.GetValue(reader.GetOrdinal(field));
        return Convert.ToUInt32(v);
      }

      return 0;
    }

    static private double GetValueDouble(OdbcDataReader reader, string field)
    {
      if (!reader.IsDBNull(reader.GetOrdinal(field)))
      {
        var v = reader.GetValue(reader.GetOrdinal(field));
        return Convert.ToDouble(v);
      }

      return 0;
    }


    /// <summary>
    /// Determines and returns the startnumber
    /// </summary>
    /// <returns>
    /// 0 if no startnumber is assigned
    /// </returns>
    static private uint GetStartNumber(OdbcDataReader reader)
    {
      uint sn = 0;
      if (sn == 0)
        sn = GetValueUInt(reader, "startnrdh");
      if (sn == 0)
        sn = GetValueUInt(reader, "startnrsg");
      if (sn == 0)
        sn = GetValueUInt(reader, "startnrgs");
      if (sn == 0)
        sn = GetValueUInt(reader, "startnrsl");
      if (sn == 0)
        sn = GetValueUInt(reader, "startnrks");
      if (sn == 0)
        sn = GetValueUInt(reader, "startnrps");

      return sn;
    }


    #endregion

    #region Debugging

    public string GetDebugSqlString(OdbcCommand cmd)
    {
      if (cmd == null)
        return "null";

      string sql = cmd.CommandText;

      foreach (var param in cmd.Parameters)
      {
        if (param is OdbcParameter dbParam)
          if (sql.Contains(dbParam.ParameterName))
          {
            string value;
            if (dbParam.Value == DBNull.Value || dbParam.Value == null)
              value = "null";
            else
              value = dbParam.Value.ToString();

            sql = sql.Replace(dbParam.ParameterName, value);
          }
      }

      return sql;
    }

    #endregion

    #region TimeSpan and Fraction
    const Int64 nanosecondsPerDay = 24L * 60 * 60 * 1000 * 1000 * 10;

    static public TimeSpan CreateTimeSpan(double fractionPerDay)
    {
      Int64 ticks = (Int64)(nanosecondsPerDay * fractionPerDay + .5);
      TimeSpan ts = new TimeSpan(ticks); // unit: 1 tick = 100 nanoseconds
      return ts;
    }

    static public double FractionForTimeSpan(TimeSpan ts)
    {
      return (double)ts.Ticks / nanosecondsPerDay;
    }
    #endregion

  }
}
