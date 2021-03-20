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

using ClosedXML.Excel;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  public class ExportBase
  {
    public delegate object GetterFunc(Race race, RaceParticipant rp);

    class ExportField
    {
      public string Name;

      public Type @Type;

      public GetterFunc Getter;
    }

    List<ExportField> _exportField;
    protected Race _race;


    protected ExportBase(Race race)
    {
      _race = race;

      _exportField = new List<ExportField>();
    }


    public void AddField(string name, Type type, GetterFunc getter)
    {
      _exportField.Add(new ExportField { Name = name, Type = type, Getter = getter });
    }


    virtual protected DataTable createTable(DataSet ds)
    {
      DataTable table = ds.Tables.Add();
      addColumns(table);
      return table;
    }

    virtual protected void addColumns(DataTable table)
    {
      foreach( var ef in _exportField)
      {
        table.Columns.Add(ef.Name, ef.Type);
      }
    }

    virtual protected DataRow createDataRow(DataTable table, RaceParticipant rp)
    {
      DataRow row = table.NewRow();

      foreach (var ef in _exportField)
      {
        var val = ef.Getter(_race, rp);
        if (val != null)
          row[ef.Name] = val;
        else
          row[ef.Name] = DBNull.Value;
      }

      return row;
    }


    public DataSet ExportToDataSet()
    {
      DataSet ds = new DataSet();

      DataTable table = createTable(ds);

      foreach (var rp in _race.GetParticipants())
      {
        DataRow row = createDataRow(table, rp);
        table.Rows.Add(row);
      }

      return ds;
    }

  }




  public class Export : ExportBase
  {
    public Export(Race race)
      : base(race)
    {
      addColumns();
    }


    void addColumns()
    {
      AddField("Id", typeof(string), (Race race, RaceParticipant rp) => { return rp.Id; });
      AddField("CodeOrId", typeof(string), (Race race, RaceParticipant rp) => { return rp.Participant.CodeOrSvId; });

      AddField("Name", typeof(string), (Race race, RaceParticipant rp) => { return rp.Name; });
      AddField("Firstname", typeof(string), (Race race, RaceParticipant rp) => { return rp.Firstname; });
      AddField("Fullname", typeof(string), (Race race, RaceParticipant rp) => { return rp.Fullname; });
      AddField("Category", typeof(string), (Race race, RaceParticipant rp) => { return rp.Sex; });
      AddField("Year", typeof(uint), (Race race, RaceParticipant rp) => { return rp.Year; });
      AddField("Club", typeof(string), (Race race, RaceParticipant rp) => { return rp.Club; });
      AddField("Nation", typeof(string), (Race race, RaceParticipant rp) => { return rp.Nation; });

      AddField("Class", typeof(string), (Race race, RaceParticipant rp) => { return rp.Class; });
      AddField("Group", typeof(string), (Race race, RaceParticipant rp) => { return rp.Group; });

      AddField("StartNumber", typeof(uint), (Race race, RaceParticipant rp) => { return rp.StartNumber; });
      AddField("Points", typeof(double), (Race race, RaceParticipant rp) => { return rp.Points; });

      addColumnsPerRun();
    }

    void addColumnsPerRun()
    {
      foreach (RaceRun rr in _race.GetRuns())
      {
        AddField(
          string.Format("Runtime_{0}", rr.Run), 
          typeof(TimeSpan), 
          (Race race, RaceParticipant rp) => 
          {
            if (rr.GetRunResult(rp) is RunResult runRes)
              return runRes.RuntimeWOResultCode;
            return null;
          }
        );

        AddField(
          string.Format("RuntimeSeconds_{0}", rr.Run),
          typeof(double),
          (Race race, RaceParticipant rp) =>
          {
            if (rr.GetRunResult(rp) is RunResult runRes && runRes.RuntimeWOResultCode != null)
              return ((TimeSpan)runRes.RuntimeWOResultCode).TotalSeconds;
            return null;
          }
        );

        AddField(
          string.Format("Resultcode_{0}", rr.Run),
          typeof(string),
          (Race race, RaceParticipant rp) =>
          {
            if (rr.GetRunResult(rp) is RunResult runRes && runRes.ResultCode != RunResult.EResultCode.NotSet)
              return runRes.ResultCode;
            return null;
          }
        );
      }
    }
  }


  public class DSVAlpinExport : ExportBase
  {
    public DSVAlpinExport(Race race)
      : base(race)
    {
      addColumns();
    }

    void addColumns()
    {
      AddField("Idnr", typeof(string), (Race race, RaceParticipant rp) => { return rp.Id; });
      AddField("Stnr", typeof(uint), (Race race, RaceParticipant rp) => { return rp.StartNumber; });
      AddField("DSV-Id", typeof(string), (Race race, RaceParticipant rp) => { return rp.Participant.CodeOrSvId; });

      AddField("Name", typeof(string), (Race race, RaceParticipant rp) => { return rp.Fullname; });
      AddField("Category", typeof(string), (Race race, RaceParticipant rp) => { return rp.Sex; });
      AddField("JG", typeof(uint), (Race race, RaceParticipant rp) => { return rp.Year; });

      AddField("V/G", typeof(string), (Race race, RaceParticipant rp) => { return rp.Nation; });
      AddField("Verein", typeof(string), (Race race, RaceParticipant rp) => { return rp.Club; });
      AddField("LPkte", typeof(double), (Race race, RaceParticipant rp) => { return rp.Points; });

      AddField("Total", typeof(double), (Race race, RaceParticipant rp) => { return 0.0; });

      addColumnsPerRun();

      AddField("Klasse", typeof(string), (Race race, RaceParticipant rp) => { return rp.Class; });
      AddField("Gruppe", typeof(string), (Race race, RaceParticipant rp) => { return rp.Group; });

      AddField("RPkte", typeof(double), (Race race, RaceParticipant rp) => { return 0.0; });
    }


    void addColumnsPerRun()
    {
      foreach (RaceRun rr in _race.GetRuns())
      {
        AddField(
          string.Format("Zeit {0}", rr.Run),
          typeof(double),
          (Race race, RaceParticipant rp) =>
          {
            if (rr.GetRunResult(rp) is RunResult runRes)
              return ((TimeSpan)runRes.RuntimeWOResultCode).TotalSeconds;
            return null;
          }
        );
      }
    }
  }



  public class ExcelExport
  {
    public void Export(string path, DataSet ds)
    {
      var excelWb = new XLWorkbook();
      excelWb.Worksheets.Add(ds);

      // Make the columns with time the right format
      var ws = excelWb.Worksheets.First();
      for(int colIdx=0; colIdx < ds.Tables[0].Columns.Count; colIdx++)
      {
        var col = ds.Tables[0].Columns[colIdx];
        if (col.DataType == typeof(TimeSpan))
          ws.Range(1, colIdx+1, ds.Tables[0].Rows.Count + 1, colIdx+1).Style.DateFormat.Format = "mm:ss.00";
      }

      excelWb.SaveAs(path);
    }

  }


  public class CsvExport
  {
    public void Export(string path, DataSet ds)
    {
      using (var textWriter = File.CreateText(path))
      using (var csv = new CsvWriter(textWriter, System.Globalization.CultureInfo.InvariantCulture))
      {
        var dt = ds.Tables[0];

        // Write columns
        foreach (DataColumn column in dt.Columns)
        {
          csv.WriteField(column.ColumnName);
        }
        csv.NextRecord();

        // Write row values
        foreach (DataRow row in dt.Rows)
        {
          for (var i = 0; i < dt.Columns.Count; i++)
          {
            csv.WriteField(row[i]);
          }
          csv.NextRecord();
        }
      }
    }
  }



  public class TsvExport
  {
    public void Export(string path, DataSet ds)
    {
      using (var textWriter = File.CreateText(path))
      {
        CsvHelper.Configuration.CsvConfiguration csvConfig = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
        csvConfig.Delimiter = "\t";
        csvConfig.Encoding = Encoding.UTF8;

        using (var csv = new CsvWriter(textWriter, csvConfig))
        {

          var dt = ds.Tables[0];

          // Write columns
          foreach (DataColumn column in dt.Columns)
          {
            csv.WriteField(column.ColumnName);
          }
          csv.NextRecord();

          // Write row values
          foreach (DataRow row in dt.Rows)
          {
            for (var i = 0; i < dt.Columns.Count; i++)
            {
              csv.WriteField(row[i]);
            }
            csv.NextRecord();
          }
        }
      }
    }
  }
}
