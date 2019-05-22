using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{
  public class Database
  {
    private System.Data.OleDb.OleDbConnection _conn;

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
    }

    public void Close()
    {
      _conn.Close();
      _conn = null;
    }

    public ObservableCollection<Participant> GetParticipants()
    {
      ObservableCollection<Participant> participants = new ObservableCollection<Participant>();

      string sql = @"SELECT * FROM tblTeilnehmer";

      OleDbCommand command = new OleDbCommand(sql, _conn);
      // Execute command  
      using (OleDbDataReader reader = command.ExecuteReader())
      {
        Debug.WriteLine("------------Original data----------------");
        while (reader.Read())
        {
          Debug.WriteLine("{0} {1}", reader["nachname"].ToString(), reader["vorname"].ToString());
          participants.Add(CreateParticipantFromDB(reader));
        }
      }

      return participants;
    }


    static private Participant CreateParticipantFromDB(OleDbDataReader reader)
    {
      Participant p = new Participant
      {
        Name      = reader["nachname"].ToString(),
        Firstname = reader["vorname"].ToString(),
        Club      = reader["verein"].ToString(),
        Year      = reader.GetInt16(reader.GetOrdinal("jahrgang"))
      };
      return p;
    }
  }
}
