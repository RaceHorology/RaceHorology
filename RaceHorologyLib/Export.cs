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

using ClosedXML.Excel;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RaceHorologyLib
{

  interface IDataSetExporter
  {
    DataSet ExportToDataSet();
  }


  public abstract class ExportBase<GetterBaseType> : IDataSetExporter
  {
    public delegate object GetterFunc(GetterBaseType input);

    protected class ExportField
    {
      public string Name;
      public Type @Type;
      public GetterFunc Getter;
    }

    protected List<ExportField> _exportField;

    protected ExportBase()
    {
      _exportField = new List<ExportField>();
    }

    public void AddField(string name, Type type, GetterFunc getter)
    {
      _exportField.Add(new ExportField { Name = name, Type = type, Getter = getter });
    }


    protected void addColumns(DataTable table)
    {
      foreach (var ef in _exportField)
      {
        table.Columns.Add(ef.Name, ef.Type);
      }
    }

    abstract protected void exportData(DataTable table);

    virtual protected DataTable createTable(DataSet ds)
    {
      DataTable table = ds.Tables.Add();
      addColumns(table);
      return table;
    }

    public DataSet ExportToDataSet()
    {
      DataSet ds = new DataSet();
      DataTable table = createTable(ds);
      exportData(table);
      return ds;
    }
  }

  public class RaceExportBase : ExportBase<RaceExportBase.BaseType>
  {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public struct BaseType {
      public Race race; 
      public RaceParticipant rp;
    }

    protected Race _race;

    protected RaceExportBase(Race race)
    {
      _race = race;
    }


    virtual protected DataRow createDataRow(DataTable table, RaceParticipant rp)
    {
      DataRow row = table.NewRow();

      foreach (var ef in _exportField)
      {
        var toGet = new BaseType { race = _race, rp = rp };
        try
        {
          var val = ef.Getter( toGet );
          if (val != null)
            row[ef.Name] = val;
          else
            row[ef.Name] = DBNull.Value;
        }catch(Exception e)
        {
          Logger.Warn("Error while exporting data field {0}: {1}", ef.Name, e);
          row[ef.Name] = DBNull.Value;
        }
      }

      return row;
    }

    override protected void exportData(DataTable table)
    {
      foreach (var rp in _race.GetParticipants())
      {
        DataRow row = createDataRow(table, rp);
        table.Rows.Add(row);
      }
    }
  }

  public class ViewExportBase<LineType> : ExportBase<ViewExportBase<LineType>.BaseType> where LineType : class
  {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public struct BaseType
    {
      public LineType item;
      public string group;
    }



    ICollectionView _view;

    public ViewExportBase(ICollectionView view)
    {
      _view = view;
    }


    virtual protected DataRow createDataRow(DataTable table, BaseType item)
    {
      DataRow row = table.NewRow();

      foreach (var ef in _exportField)
      {
        try
        {
          var val = ef.Getter(item);
          if (val != null)
            row[ef.Name] = val;
          else
            row[ef.Name] = DBNull.Value;
        }
        catch (Exception e)
        {
          Logger.Warn("Error while exporting data field {0}: {1}", ef.Name, e);
          row[ef.Name] = DBNull.Value;
        }
      }

      return row;
    }

    ICollectionView getView()
    {
      return _view;
    }

    override protected void exportData(DataTable table)
    {
      void addRow(DataRow row)
      {
        table.Rows.Add(row);
      }

      var results = getView();
      var lr = results as ListCollectionView;
      if (results.Groups != null)
      {
        foreach (var group in results.Groups)
        {
          var cvGroup = group as CollectionViewGroup;
          foreach (var item in cvGroup.Items)
            addRow(createDataRow(table, new BaseType { item = item as LineType, group = cvGroup.GetName() }));
        }
      }
      else
      {
        foreach (var item in results.SourceCollection)
          addRow(createDataRow(table, new BaseType { item = item as LineType }));
      }
    }
  }




  public class RaceExport : RaceExportBase
  {
    public RaceExport(Race race)
      : base(race)
    {
      addColumns();
    }


    void addColumns()
    {
      AddField("Id", typeof(string), (item) => { return item.rp.Id; });
      AddField("CodeOrId", typeof(string), (item) => { return item.rp.Participant.CodeOrSvId; });

      AddField("Name", typeof(string), (item) => { return item.rp.Name; });
      AddField("Firstname", typeof(string), (item) => { return item.rp.Firstname; });
      AddField("Fullname", typeof(string), (item) => { return item.rp.Fullname; });
      AddField("Category", typeof(string), (item) => { return item.rp.Sex; });
      AddField("CategoryShort", typeof(string), (item) => { return item.rp.Sex?.Name; });
      AddField("Year", typeof(uint), (item) => { return item.rp.Year; });
      AddField("Club", typeof(string), (item) => { return item.rp.Club; });
      AddField("Nation", typeof(string), (item) => { return item.rp.Nation; });

      AddField("Class", typeof(string), (item) => { return item.rp.Class; });
      AddField("Group", typeof(string), (item) => { return item.rp.Group; });

      AddField("StartNumber", typeof(uint), (item) => { return item.rp.StartNumber; });
      AddField("Points", typeof(double), (item) => { return item.rp.Points; });

      addColumnsPerRun();

      AddField(
        "Totaltime",
        typeof(TimeSpan),
        (item) =>
        {
          var vp = item.race.GetResultViewProvider();
          var raceResult = vp.GetViewList().FirstOrDefault(r => r.Participant == item.rp);
          if (raceResult != null)
            if (raceResult.TotalTime != null)
              return raceResult.TotalTime;

          return null;
        }
      );

      AddField(
        "Totaltime_Seconds",
        typeof(double),
        (item) =>
        {
          var vp = item.race.GetResultViewProvider();
          var raceResult = vp.GetViewList().FirstOrDefault(r => r.Participant == item.rp);
          if (raceResult != null)
            if (raceResult.TotalTime != null)
              return ((TimeSpan)raceResult.TotalTime).TotalSeconds;

          return null;
        }
      );

      AddField(
        "Total_Position",
        typeof(int),
        (item) =>
        {
          var vp = item.race.GetResultViewProvider();
          var raceResult = vp.GetViewList().FirstOrDefault(r => r.Participant == item.rp);
          if (raceResult != null)
            if (raceResult.TotalTime != null)
              return raceResult.Position;

          return null;
        }
      );

      AddField(
        "Total_RacePoints",
        typeof(double),
        (item) =>
        {
          var vp = item.race.GetResultViewProvider();
          var raceResult = vp.GetViewList().FirstOrDefault(r => r.Participant == item.rp);
          if (raceResult != null)
            if (raceResult.Points >= 0.0)
              return string.Format(new System.Globalization.CultureInfo("de-DE"), "{0:0.00}", raceResult.Points);

          return null;
        }
      );

    }

    void addColumnsPerRun()
    {
      foreach (RaceRun rr in _race.GetRuns())
      {
        AddField(
          string.Format("Runtime_{0}", rr.Run), 
          typeof(TimeSpan), 
          (item) => 
          {
            if (rr.GetRunResult(item.rp) is RunResult runRes)
              return runRes.RuntimeWOResultCode;
            return null;
          }
        );

        AddField(
          string.Format("RuntimeSeconds_{0}", rr.Run),
          typeof(double),
          (item) =>
          {
            if (rr.GetRunResult(item.rp) is RunResult runRes && runRes.RuntimeWOResultCode != null)
              return ((TimeSpan)runRes.RuntimeWOResultCode).TotalSeconds;
            return null;
          }
        );

        AddField(
          string.Format("Resultcode_{0}", rr.Run),
          typeof(string),
          (item) =>
          {
            if (rr.GetRunResult(item.rp) is RunResult runRes && runRes.ResultCode != RunResult.EResultCode.NotSet)
              return runRes.ResultCode;
            return null;
          }
        );
      }
    }

  }


  public class DSVAlpinExport : RaceExportBase
  {
    public DSVAlpinExport(Race race)
      : base(race)
    {
      addColumns();
    }

    void addColumns()
    {
      AddField("Idnr", typeof(string), (item) => { return item.rp.Id; });
      AddField("Stnr", typeof(uint), (item) => { return item.rp.StartNumber; });
      AddField("DSV-ID", typeof(string), (item) => { return item.rp.Participant.CodeOrSvId; });

      AddField("Name", typeof(string), (item) => { return item.rp.Fullname; });
      AddField("Kateg", typeof(string), (item) => { return item.rp.Sex?.Name.ToString(); });
      AddField("JG", typeof(uint), (item) => { return item.rp.Year; });

      AddField("V/G", typeof(string), (item) => { return item.rp.Nation; });
      AddField("Verein", typeof(string), (item) => { return item.rp.Club; });
      AddField("LPkte", typeof(string), (item) => { return string.Format(new System.Globalization.CultureInfo("de-DE"), "{0:0.00}", item.rp.Points); });

      AddField(
        "Total", 
        typeof(string), 
        (item) => 
        {
          var vp = item.race.GetResultViewProvider();
          var raceResult = vp.GetViewList().FirstOrDefault(r => r.Participant == item.rp);
          if (raceResult != null)
          {
            if (raceResult.TotalTime != null)
              return raceResult.TotalTime.ToRaceTimeString();
            else
            {
              string resultCode = string.Empty;
              foreach (KeyValuePair<uint, RaceResultItem.SubResult> kvp in raceResult.SubResults.OrderBy(k => k.Key))
              {
                if (kvp.Value.RunResultCode != RunResult.EResultCode.Normal)
                {
                  resultCode = getResultString(kvp.Value.RunResultCode, kvp.Key);
                  break;
                }
              }
              return resultCode;
            }
          }
          return null; 
        }
      );

      addColumnsPerRun();

      AddField("Klasse", typeof(string), (item) => { return item.rp.Class; });
      AddField("Gruppe", typeof(string), (item) => { return item.rp.Group; });

      AddField(
        "RPkte",
        typeof(string),
        (item) =>
        {
          var vp = item.race.GetResultViewProvider();
          var raceResult = vp.GetViewList().FirstOrDefault(r => r.Participant == item.rp);
          if (raceResult != null)
          {
            if (raceResult.Points >= 0.0)
              return string.Format(new System.Globalization.CultureInfo("de-DE"), "{0:0.00}", raceResult.Points);
          }
          return "---";
        }
      );
    }


    void addColumnsPerRun()
    {
      foreach (RaceRun rr in _race.GetRuns())
      {
        AddField(
          string.Format("Zeit {0}", rr.Run),
          typeof(string),
          (item) =>
          {
            if (rr.GetRunResult(item.rp) is RunResult runRes)
              if (runRes.ResultCode == RunResult.EResultCode.Normal)
              {
                if (runRes.RuntimeWOResultCode != null)
                  return runRes.RuntimeWOResultCode.ToRaceTimeString();
              }
              else
              {
                return getResultString(runRes.ResultCode, 0);
              }
            return null;
          }
        );
      }
    }

    protected static string getResultString(RunResult.EResultCode code, uint run)
    {
      string str = string.Empty;
      switch (code)
      {
        case RunResult.EResultCode.DIS: str = "DIS"; break;
        case RunResult.EResultCode.NaS: str = "NAS"; break;
        case RunResult.EResultCode.NiZ: str = "NIZ"; break;
        case RunResult.EResultCode.NQ: str = "NQ"; break;
      }

      if (run > 0)
        str += run.ToString();

      return str;
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
    public void Export(string path, DataSet ds, bool utf8, string delimiter = ",")
    {
      Encoding encoding;
      if (utf8)
        encoding = Encoding.UTF8;
      else
        encoding = Encoding.GetEncoding("windows-1252");

      var csvConfig = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
      csvConfig.Delimiter = delimiter;

      using (var textWriter = new StreamWriter(File.Open(path, FileMode.Create), encoding))
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



  public class TsvExport
  {
    public void Export(string path, DataSet ds, bool utf8)
    {
      Encoding encoding;
      if (utf8)
        encoding = Encoding.UTF8;
      else
        encoding = Encoding.GetEncoding("windows-1252");

      using (var textWriter = new StreamWriter(File.Open(path, FileMode.Create), encoding))
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


  public class AlpenhundeStartlistExport : ViewExportBase<StartListEntry>
  {
    public AlpenhundeStartlistExport(ICollectionView view)
      : base(view)
    {
      AddField("Short Name", typeof(uint), (item) => { return item.item.Participant.StartNumber; });
      AddField("Full Name", typeof(string), (item) => { return item.item.Participant.Fullname; });
      AddField("Team", typeof(string), (item) => { return item.group; });
    }
  }

  public class GenericStartlistExport : ViewExportBase<StartListEntry>
  {
    public GenericStartlistExport(ICollectionView view)
      : base(view)
    {
      AddField("StartNumber", typeof(uint), (item) => { return item.item.Participant.StartNumber; });

      AddField("StartGroup", typeof(string), (item) => { return item.group; });

      AddField("Id", typeof(string), (item) => { return item.item.Participant.Id; });
      AddField("CodeOrId", typeof(string), (item) => { return item.item.Participant.Participant.CodeOrSvId; });

      AddField("Name", typeof(string), (item) => { return item.item.Participant.Name; });
      AddField("Firstname", typeof(string), (item) => { return item.item.Participant.Firstname; });
      AddField("Fullname", typeof(string), (item) => { return item.item.Participant.Fullname; });
      AddField("Category", typeof(string), (item) => { return item.item.Participant.Sex; });
      AddField("Year", typeof(uint), (item) => { return item.item.Participant.Year; });
      AddField("Club", typeof(string), (item) => { return item.item.Participant.Club; });
      AddField("Nation", typeof(string), (item) => { return item.item.Participant.Nation; });

      AddField("Class", typeof(string), (item) => { return item.item.Participant.Class; });
      AddField("Group", typeof(string), (item) => { return item.item.Participant.Group; });
    }
  }


}
