/*
 *  Copyright (C) 2019 - 2022 by Sven Flossmann
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

using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  static public class FISUpdatePoints
  {
    static public List<ImportResults> UpdatePoints(AppDataModel dm, FISInterfaceModel fisData)
    {
      List<ImportResults> impRes = new List<ImportResults>();
      
      foreach (Race race in dm.GetRaces())
      {
        var mapping = fisData.GetMapping(race);

        UpdatePointsImport import = new UpdatePointsImport(race, mapping);
        var res = import.DoImport(fisData.Data);
        impRes.Add(res);
      }
      dm.GetDB().StoreKeyValue("FIS_UsedFISList", fisData.UsedList);

      return impRes;
    }
  }

  /// <summary>
  /// Pre-configured mapping for race mapping (race import)
  /// </summary>
  public class FISMapping : Mapping
  {

    /// <summary>
    /// Map defining the required fields and potential available fields
    /// </summary>
    private Dictionary<string, List<string>> _requiredField;

    static string getFISPointField(Race race)
    {
      if (race != null)
      {
        switch (race.RaceType)
        {
          case Race.ERaceType.DownHill:
            return "DHpoints";
          case Race.ERaceType.GiantSlalom:
            return "GSpoints";
          case Race.ERaceType.Slalom:
            return "SLpoints";
          case Race.ERaceType.SuperG:
            return "SGpoints";
        }
      }
      return null;
    }

    static Dictionary<string, List<string>> createRequiredFields(Race race)
    {
      var fields = new Dictionary<string, List<string>>
      {
        { "Code", new List<string>{"Fiscode"} },
        { "Name", new List<string>{ "Lastname" } },
        { "Firstname", new List<string>{ "Firstname"} },
        { "Year", new List<string>{ "Birthyear" } },
        { "Club", new List<string>{ "Skiclub" } },
        { "Nation", new List<string>{ "Nationcode" } },
        { "Sex", new List<string>{"Gender"} }
      };

      var fieldPoints = getFISPointField(race);
      if (!string.IsNullOrEmpty(fieldPoints))
        fields.Add("Points", new List<string> { fieldPoints });

      return fields;
    }

    static List<string> createAvailableFields(Race race)
    {
      var fields = new List<string>
      {
        "Fiscode",
        "Lastname",
        "Firstname",
        "Birthyear",
        "Skiclub",
        "Nationcode",
        "Gender"
      };

      var fieldPoints = getFISPointField(race);
      if (!string.IsNullOrEmpty(fieldPoints))
        fields.Add(fieldPoints);

      return fields;
    }

    public FISMapping(Race race) 
      : base()
    {
      _requiredField = createRequiredFields(race);
      initMapping(_requiredField.Keys, createAvailableFields(race));
    }

    protected override List<string> synonyms(string field)
    {
      return _requiredField[field];
    }

  }


  public class FISImportReader : IImportReader
  {
    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    protected string _fisExcelFile;
    protected List<string> _columns;
    protected DataSet _dataSet;
    protected string _usedFISList;
    protected DateTime? _listDate;


    public Mapping GetMapping(Race race)
    {
      return new FISMapping(race);
    }


    public FISImportReader(string fisExcelFile)
    {
      _fisExcelFile = fisExcelFile;

      readData(fisExcelFile);
    }

    public string FileName { get => _fisExcelFile; }

    public DataSet Data { get => _dataSet; }

    public List<string> Columns { get => _columns; }

    public string UsedFISList { get => _usedFISList; }

    public DateTime? Date { get => _listDate; }

    protected void readData(string fisExcelFile)
    {
      var stream = File.Open(fisExcelFile, FileMode.Open, FileAccess.Read, FileShare.Read);
      using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
      {
        _dataSet = reader.AsDataSet(new ExcelDataSetConfiguration() { ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration() { UseHeaderRow = true } });
      }

      _usedFISList = derriveListName(_dataSet);
      _listDate = derriveListDate(_dataSet);

      replaceEmptyPointsWith(_dataSet.Tables[0], "DHpoints", 9999.99);
      replaceEmptyPointsWith(_dataSet.Tables[0], "SLpoints", 9999.99);
      replaceEmptyPointsWith(_dataSet.Tables[0], "GSpoints", 9999.99);
      replaceEmptyPointsWith(_dataSet.Tables[0], "SGpoints", 9999.99);
      replaceEmptyPointsWith(_dataSet.Tables[0], "ACpoints", 9999.99);

      deleteUnusedColumns(_dataSet);
      checkForNeededColumns(_dataSet);

      _columns = ImportUtils.extractFields(_dataSet);
    }

    protected string derriveListName(DataSet dataSet)
    {
      return _dataSet.Tables[0].Rows[0]["Listname"].ToString();
    }
    protected DateTime? derriveListDate(DataSet dataSet)
    {
      string sDate = _dataSet.Tables[0].Rows[0]["Calculationdate"].ToString();
      DateTime? date = null;

      try
      {
        date = DateTime.ParseExact(sDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
      }
      catch (Exception) { }

      return date;
    }


    protected void replaceEmptyPointsWith(DataTable table, string column, double valueReplace)
    {
      foreach(DataRow row in table.Rows)
      {
        if (row[column] == System.DBNull.Value)
          row[column] = valueReplace;
      }
    }


    static protected void deleteUnusedColumns(DataSet dataSet)
    {
      dataSet.Tables[0].Columns.Remove("Listid");
      dataSet.Tables[0].Columns.Remove("Listname");
      dataSet.Tables[0].Columns.Remove("Published");
      dataSet.Tables[0].Columns.Remove("Sectorcode");
      dataSet.Tables[0].Columns.Remove("Status");
      dataSet.Tables[0].Columns.Remove("Competitorid");
      dataSet.Tables[0].Columns.Remove("Nationalcode");
      dataSet.Tables[0].Columns.Remove("Competitorname");
      dataSet.Tables[0].Columns.Remove("Calculationdate");
      dataSet.Tables[0].Columns.Remove("DHpos");
      dataSet.Tables[0].Columns.Remove("DHSta");
      dataSet.Tables[0].Columns.Remove("SLpos");
      dataSet.Tables[0].Columns.Remove("SLSta");
      dataSet.Tables[0].Columns.Remove("GSpos");
      dataSet.Tables[0].Columns.Remove("GSSta");
      dataSet.Tables[0].Columns.Remove("SGpos");
      dataSet.Tables[0].Columns.Remove("SGSta");
      dataSet.Tables[0].Columns.Remove("ACpoints");
      dataSet.Tables[0].Columns.Remove("ACpos");
      dataSet.Tables[0].Columns.Remove("ACSta");
    }


    static protected void checkForNeededColumns(DataSet dataSet)
    {
      string[] neededColumns = new string[]
      {
        "Fiscode",
        "Lastname",
        "Firstname",
        "Birthyear",
        "Skiclub",
        "Nationcode",
        "Gender",
        "DHpoints",
        "GSpoints",
        "SLpoints",
        "SGpoints"
      };

      foreach(var neededCol in neededColumns)
      {
        if (!dataSet.Tables[0].Columns.Contains(neededCol))
          throw new Exception("missing column in FIS import file");
      }
    }
  }
}
