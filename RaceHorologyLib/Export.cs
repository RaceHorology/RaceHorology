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
  public class Export
  {
    Race _race;

    public Export(Race race)
    {
      _race = race;
    }


    public DataSet ExportToDataSet()
    {
      DataSet ds = new DataSet();

      DataTable table = createTable(ds);

      foreach(var rp in _race.GetParticipants())
      {
        DataRow row = createDataRow(table, rp);
        table.Rows.Add(row);
      }

      return ds;
    }


    protected DataTable createTable(DataSet ds)
    {
      DataTable table = ds.Tables.Add();
      
      addColumns(table);

      return table;
    }

    protected DataTable addColumns(DataTable table)
    {
      table.Columns.Add("Id");
      table.Columns.Add("CodeOrId");

      table.Columns.Add("Name");
      table.Columns.Add("Firstname");
      table.Columns.Add("Fullname");
      table.Columns.Add("Category");
      table.Columns.Add("Year", typeof(uint));
      table.Columns.Add("Club");
      table.Columns.Add("Nation");

      table.Columns.Add("Class");
      table.Columns.Add("Group");

      table.Columns.Add("StartNumber", typeof(uint));
      table.Columns.Add("Points", typeof(double));

      addColumnsPerRun(table);

      return table;
    }

    protected DataTable addColumnsPerRun(DataTable table)
    {
      foreach(RaceRun rr in _race.GetRuns())
      {
        table.Columns.Add(string.Format("Runtime_{0}", rr.Run), typeof(TimeSpan));
        table.Columns.Add(string.Format("RuntimeSeconds_{0}", rr.Run), typeof(double));
        
        table.Columns.Add(string.Format("Resultcode_{0}", rr.Run));
      }

      return table;
    }

    protected DataRow createDataRow(DataTable table, RaceParticipant rp)
    {
      DataRow row = table.NewRow();

      row["Id"] = rp.Id;
      row["CodeOrId"] = rp.Participant.CodeOrSvId;

      row["Name"] = rp.Name;
      row["Firstname"] = rp.Firstname;
      row["Fullname"] = rp.Fullname;
      row["Category"] = rp.Sex;
      row["Year"] = rp.Year;
      row["Club"] = rp.Club;
      row["Nation"] = rp.Nation;

      row["Class"] = rp.Class;
      row["Group"] = rp.Group;

      row["StartNumber"] = rp.StartNumber;
      row["Points"] = rp.Points;

      addDataPerRun(row, rp);

      return row;
    }

    protected DataRow addDataPerRun(DataRow row, RaceParticipant rp)
    {
      foreach (RaceRun rr in _race.GetRuns())
      {
        RunResult runRes = rr.GetRunResult(rp);
        if (runRes != null)
        {
          if (runRes.RuntimeWOResultCode != null)
          {
            row[string.Format("Runtime_{0}", rr.Run)] = runRes.RuntimeWOResultCode;
            row[string.Format("RuntimeSeconds_{0}", rr.Run)] = ((TimeSpan)runRes.RuntimeWOResultCode).TotalSeconds;
          }
          if (runRes.ResultCode != RunResult.EResultCode.NotSet)
            row[string.Format("Resultcode_{0}", rr.Run)] = runRes.ResultCode;
        }
      }

      return row;
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
}
