using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{

  /// <summary>
  /// Implements the data base access to and from the "old" DSVAlpin Access Data Base
  /// </summary>
  /// <remarks>not yet fully implemented</remarks>
  public class Database
    : IAppDataModelDataBase
  {
    private System.Data.OleDb.OleDbConnection _conn;

    private Dictionary<uint, Participant> _id2Participant;
    private Dictionary<uint, ParticipantGroup> _id2ParticipantGroups;
    private Dictionary<uint, ParticipantClass> _id2ParticipantClasses;

    public void Connect(string filename)
    {
      _conn = new OleDbConnection
      {
        ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data source= " + filename
      };

      try
      {
        _conn.Open();
      }
      catch (Exception ex)
      {
        Console.WriteLine("Failed to connect to data source", ex.Message);
        _conn = null;
        throw;
      }

      // Setup internal daat structures
      _id2Participant = new Dictionary<uint, Participant>();
      _id2ParticipantGroups = new Dictionary<uint, ParticipantGroup>();
      _id2ParticipantClasses = new Dictionary<uint, ParticipantClass>();

    }

    public void Close()
    {
      // Cleanup internal data structures
      _id2Participant = null;
      _id2ParticipantGroups = null;
      _id2ParticipantClasses = null;

      _conn.Close();
      _conn = null;
    }


    #region IAppDataModelDataBase implementation


    public ItemsChangeObservableCollection<Participant> GetParticipants()
    {
      ItemsChangeObservableCollection<Participant> participants = new ItemsChangeObservableCollection<Participant>();

      string sql = @"SELECT * FROM tblTeilnehmer";

      OleDbCommand command = new OleDbCommand(sql, _conn);
      // Execute command  
      using (OleDbDataReader reader = command.ExecuteReader())
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
      ReadParticipantClasses();

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


    public List<Race.RaceProperties> GetRaces()
    {
      List<Race.RaceProperties> races = new List<Race.RaceProperties>();

      string sql = @"SELECT * FROM tblDisziplin WHERE aktiv = true";

      OleDbCommand command = new OleDbCommand(sql, _conn);
      // Execute command  
      using (OleDbDataReader reader = command.ExecuteReader())
      {
        while (reader.Read())
        {
          Race.RaceProperties race = new Race.RaceProperties();

          race.RaceType = (Race.ERaceType)(byte)reader.GetValue(reader.GetOrdinal("dtyp"));
          race.Runs = (uint)(byte)reader.GetValue(reader.GetOrdinal("durchgaenge"));
          if (!reader.IsDBNull(reader.GetOrdinal("bewerbsnummer")))
            race.RaceNumber = reader["bewerbsnummer"].ToString();
          if (!reader.IsDBNull(reader.GetOrdinal("bewerbsbezeichnung")))
            race.Description = reader["bewerbsbezeichnung"].ToString();
          if (!reader.IsDBNull(reader.GetOrdinal("datum_startliste")))
            race.DateStart = (DateTime)reader.GetValue(reader.GetOrdinal("datum_startliste"));
          if (!reader.IsDBNull(reader.GetOrdinal("datum_rangliste")))
            race.DateResult= (DateTime)reader.GetValue(reader.GetOrdinal("datum_rangliste"));

          //string s7 = reader["finalisten"].ToString();
          //string s8 = reader["freier_listenkopf"].ToString();

          races.Add(race);
        }
      }

      return races;
    }


    public List<RaceParticipant> GetRaceParticipants(Race race)
    {
      List<RaceParticipant> participants = new List<RaceParticipant>();

      string sql = @"SELECT * FROM tblTeilnehmer";

      string startNumberField = null;
      string pointsField = null;
      switch (race.RaceType)
      {
        case Race.ERaceType.DownHill:
          sql += " WHERE dhaktiv = true";
          startNumberField = "startnrdh";
          pointsField = "pktedh";
          break;
        case Race.ERaceType.SuperG:
          sql += " WHERE sgaktiv = true";
          startNumberField = "startnrsg";
          pointsField = "pktesg";
          break;
        case Race.ERaceType.GiantSlalom:
          sql += " WHERE gsaktiv = true";
          startNumberField = "startnrgs";
          pointsField = "pktegs";
          break;
        case Race.ERaceType.Slalom:
          sql += " WHERE slaktiv = true";
          startNumberField = "startnrsl";
          pointsField = "pktesl";
          break;
        case Race.ERaceType.KOSlalom:
          sql += " WHERE ksaktiv = true";
          startNumberField = "startnrks";
          pointsField = "pkteks";
          break;
        case Race.ERaceType.ParallelSlalom:
          sql += " WHERE psaktiv = true";
          startNumberField = "startnrps";
          pointsField = "pkteps";
          break;
      }

      OleDbCommand command = new OleDbCommand(sql, _conn);
      // Execute command  
      using (OleDbDataReader reader = command.ExecuteReader())
      {
        while (reader.Read())
        {
          Participant participant = CreateParticipantFromDB(reader);
          uint startNo = GetValueUInt(reader, startNumberField);
          double points = GetValueDouble(reader, pointsField);

          RaceParticipant raceParticpant = new RaceParticipant(participant, startNo, points);
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
                   @"WHERE(((tblZeit.durchgang) = @durchgang) AND((tblZeit.disziplin) = @disziplin))";

      OleDbCommand command = new OleDbCommand(sql, _conn);
      command.Parameters.Add(new OleDbParameter("@durchgang", run));
      command.Parameters.Add(new OleDbParameter("@disziplin", (int) race.RaceType));

      // Execute command  
      using (OleDbDataReader reader = command.ExecuteReader())
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
            r.SetStartTime(startTime);
            r.SetFinishTime(finishTime);
          }
          else if (runTime != null)
            r.SetRunTime(runTime);

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

      OleDbCommand cmd;

      if (!bNew)
      {
        string sql = @"UPDATE tblTeilnehmer " +
                     @"SET nachname = @nachname, vorname = @vorname, sex = @sex, verein = @verein, nation = @nation, klasse = @klasse, jahrgang = @jahrgang " +
                     @"WHERE id = @id";
        cmd = new OleDbCommand(sql, _conn);
      }
      else
      {
        // Figure out the new ID
        using (OleDbCommand command = new OleDbCommand("SELECT MAX(id) FROM tblTeilnehmer;", _conn))
        {
          object oId = command.ExecuteScalar();
          if (oId == DBNull.Value)
            id = 0;
          else
            id = Convert.ToUInt32(oId);
          id++;
        }
               
        string sql = @"INSERT INTO tblTeilnehmer (nachname, vorname, sex, verein, nation, klasse, jahrgang, id) " +
                     @"VALUES (@nachname, @vorname, @sex, @verein, @nation, @klasse, @jahrgang, @id) ";
        cmd = new OleDbCommand(sql, _conn);
      }

      cmd.Parameters.Add(new OleDbParameter("@nachname", participant.Name));
      cmd.Parameters.Add(new OleDbParameter("@vorname", participant.Firstname));
      if (string.IsNullOrEmpty(participant.Sex))
        cmd.Parameters.Add(new OleDbParameter("@sex", DBNull.Value));
      else
        cmd.Parameters.Add(new OleDbParameter("@sex", participant.Sex));

      if (string.IsNullOrEmpty(participant.Club))
        cmd.Parameters.Add(new OleDbParameter("@verein", DBNull.Value));
      else
        cmd.Parameters.Add(new OleDbParameter("@verein", participant.Club));
      if (string.IsNullOrEmpty(participant.Nation))
        cmd.Parameters.Add(new OleDbParameter("@nation", DBNull.Value));
      else
        cmd.Parameters.Add(new OleDbParameter("@nation", participant.Nation));

      cmd.Parameters.Add(new OleDbParameter("@klasse", 10)); // TODO: Add correct id for klasse
      cmd.Parameters.Add(new OleDbParameter("@jahrgang", participant.Year));
      cmd.Parameters.Add(new OleDbParameter("@id", (ulong)id));

      cmd.CommandType = CommandType.Text;

      try
      {

        int temp = cmd.ExecuteNonQuery();
        Debug.Assert(temp == 1, "Database could not be updated");

        if (bNew)
        {
          _id2Participant.Add((uint)id, participant);
          participant.Id = id.ToString();
        }
      }catch(Exception e)
      {
        Debug.Print(e.Message);
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
      using (OleDbCommand command = new OleDbCommand("SELECT COUNT(*) FROM tblZeit WHERE teilnehmer = @teilnehmer AND disziplin = @disziplin AND durchgang = @durchgang", _conn))
      {
        command.Parameters.Add(new OleDbParameter("@teilnehmer", idParticipant));
        command.Parameters.Add(new OleDbParameter("@disziplin", (int)race.RaceType)); 
        command.Parameters.Add(new OleDbParameter("@durchgang", raceRun.Run));
        object oId = command.ExecuteScalar();

        bNew = (oId == DBNull.Value || (int)oId == 0);
      }


      OleDbCommand cmd;
      if (!bNew)
      {
        string sql = @"UPDATE tblZeit " +
                     @"SET ergcode = @ergcode, start = @start, ziel = @ziel, netto = @netto, disqualtext = @disqualtext " +
                     @"WHERE teilnehmer = @teilnehmer AND disziplin = @disziplin AND durchgang = @durchgang";
        cmd = new OleDbCommand(sql, _conn);
      }
      else
      {
        string sql = @"INSERT INTO tblZeit (ergcode, start, ziel, netto, disqualtext, teilnehmer, disziplin, durchgang) " +
                     @"VALUES (@ergcode, @start, @ziel, @netto, @disqualtext, @teilnehmer, @disziplin, @durchgang) ";
        cmd = new OleDbCommand(sql, _conn);
      }
      cmd.Parameters.Add(new OleDbParameter("@ergcode", (byte)result.ResultCode));
      if (result.GetStartTime() == null)
        cmd.Parameters.Add(new OleDbParameter("@start", DBNull.Value));
      else
        cmd.Parameters.Add(new OleDbParameter("@start", FractionForTimeSpan((TimeSpan)result.GetStartTime())));
      if (result.GetFinishTime() == null)
        cmd.Parameters.Add(new OleDbParameter("@ziel", DBNull.Value));
      else
        cmd.Parameters.Add(new OleDbParameter("@ziel", FractionForTimeSpan((TimeSpan)result.GetFinishTime())));
      if (result.GetRunTime() == null)
        cmd.Parameters.Add(new OleDbParameter("@netto", DBNull.Value));
      else
        cmd.Parameters.Add(new OleDbParameter("@netto", FractionForTimeSpan((TimeSpan)result.GetRunTime())));
      if (result.DisqualText == null || result.DisqualText == "")
        cmd.Parameters.Add(new OleDbParameter("@disqualtext", DBNull.Value));
      else
        cmd.Parameters.Add(new OleDbParameter("@disqualtext", result.DisqualText));

      cmd.Parameters.Add(new OleDbParameter("@teilnehmer", idParticipant));
      cmd.Parameters.Add(new OleDbParameter("@disziplin", (int)race.RaceType)); 
      cmd.Parameters.Add(new OleDbParameter("@durchgang", raceRun.Run));

      cmd.CommandType = CommandType.Text;
      int temp = cmd.ExecuteNonQuery();
      Debug.Assert(temp == 1, "Database could not be updated");
    }

    #endregion

    #region Internal Implementation

    /* ************************ Participant ********************* */
    private Participant CreateParticipantFromDB(OleDbDataReader reader)
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
          Sex = reader["sex"].ToString(),
          Club = reader["verein"].ToString(),
          Nation = reader["nation"].ToString(),
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

      OleDbCommand command = new OleDbCommand(sql, _conn);
      // Execute command  
      using (OleDbDataReader reader = command.ExecuteReader())
      {
        while (reader.Read())
        {
          CreateParticipantGroupFromDB(reader);
        }
      }
    }


    private ParticipantGroup CreateParticipantGroupFromDB(OleDbDataReader reader)
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
      return _id2ParticipantGroups[id];
    }



    /* ************************ Classes ********************* */
    public void ReadParticipantClasses()
    {
      if (_id2ParticipantClasses.Count() > 0)
        return;

      string sql = @"SELECT * FROM tblKlasse";
      OleDbCommand command = new OleDbCommand(sql, _conn);
      // Execute command  
      using (OleDbDataReader reader = command.ExecuteReader())
      {
        while (reader.Read())
        {
          CreateParticipantClassFromDB(reader);
        }
      }
    }

    private ParticipantClass CreateParticipantClassFromDB(OleDbDataReader reader)
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
          reader["geschlecht"].ToString(),
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
      return _id2ParticipantClasses[id];
    }




    private static int GetLatestAutonumber(OleDbConnection connection)
    {
      using (OleDbCommand command = new OleDbCommand("SELECT @@IDENTITY;", connection))
      {
        return (int)command.ExecuteScalar();
      }
    }

    static private uint GetValueUInt(OleDbDataReader reader, string field)
    {
      if (!reader.IsDBNull(reader.GetOrdinal(field)))
      {
        var v = reader.GetValue(reader.GetOrdinal(field));
        return Convert.ToUInt32(v);
      }

      return 0;
    }

    static private double GetValueDouble(OleDbDataReader reader, string field)
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
    static private uint GetStartNumber(OleDbDataReader reader)
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
