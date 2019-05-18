using System;
using System.Collections.Generic;
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
      _conn = new System.Data.OleDb.OleDbConnection();

      _conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data source= " + filename;

      try
      {
        _conn.Open();
        // Insert code to process data.
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


  }
}
