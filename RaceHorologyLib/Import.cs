﻿/*
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

using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public interface IImportReader
  {

    DataSet Data { get; }
    
    List<string> Columns { get; }
  }


  public static class ImportUtils
  {
    internal static List<string> extractFields(DataSet ds)
    {
      List<string> ret = new List<string>();
      foreach (DataColumn col in ds.Tables[0].Columns)
      {
        ret.Add(col.ColumnName);
      }
      return ret;
    }
  }




  public class ImportReader : IImportReader
  {
    DataSet _dataSet;

    static private string[] txtExtensions = { ".csv", ".tsv", ".txt" };

    public ImportReader(string path)
    {
      IExcelDataReader reader;

      if (txtExtensions.Contains(System.IO.Path.GetExtension(path).ToLower()))
      {
        // CSV File
        var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        reader = ExcelReaderFactory.CreateCsvReader(stream);
        _dataSet = reader.AsDataSet(new ExcelDataSetConfiguration() { ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration() { UseHeaderRow = true } });
      }
      else
      {
        // Excel
        var stream = File.Open(path, FileMode.Open, FileAccess.Read);
        reader = ExcelReaderFactory.CreateReader(stream);
        _dataSet = reader.AsDataSet(new ExcelDataSetConfiguration() { ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration() { UseHeaderRow = true } });
      }

      Columns = ImportUtils.extractFields(_dataSet);
    }

    public DataSet Data { get { return _dataSet; } }

    public List<string> Columns { get; protected set; }

  }



  public class ImportZipReader : IImportReader
  {
    DataSet _dataSet;

    static private string[] txtExtensions = { ".csv", ".tsv", ".txt" };

    public ImportZipReader(string path)
    {
      IExcelDataReader reader;

      Stream dataStream = getDataStream(path);
      MemoryStream stream = new MemoryStream();
      dataStream.CopyTo(stream);

      reader = ExcelReaderFactory.CreateCsvReader(stream);
      _dataSet = reader.AsDataSet(new ExcelDataSetConfiguration() { ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration() { UseHeaderRow = true } });

      Columns = ImportUtils.extractFields(_dataSet);
    }

    Stream getDataStream(string zipPath)
    {
      ZipArchive archive = new ZipArchive(new FileStream(zipPath, FileMode.Open, FileAccess.Read, FileShare.Read), ZipArchiveMode.Read);

      {
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
          if (entry.FullName.StartsWith("T_") && entry.FullName.EndsWith(".txt"))
          {
            return entry.Open();
          }
        }
      }

      return null;
    }

    public DataSet Data { get { return _dataSet; } }

    public List<string> Columns { get; protected set; }
  }





  public class Mapping
  {
    public class MappingEntry
    {
      public string Key { get; set; }
      public string Value { get; set; }
    }


    ObservableCollection<MappingEntry> _mapping;
    List<string> _availableFields;
    List<string> _requiredFields;

    public Mapping(IEnumerable<string> requiredFields, IEnumerable<string> availableFields)
    {
      _availableFields = new List<string>();
      _availableFields.Add("---");
      _availableFields.AddRange(availableFields);
      
      _requiredFields = requiredFields.ToList();

      _mapping = new ObservableCollection<MappingEntry>();

      initMapping();
    }


    public void Assign(string requiredField, string providedField)
    {
      var mapEntry = _mapping.FirstOrDefault(m => m.Key == requiredField);
      if (mapEntry != null)
      {
        mapEntry.Value = providedField;
      }
      else
        _mapping.Add(new MappingEntry { Key = requiredField, Value = providedField });
    }


    public string MappedField(string requiredField)
    {
      var mapEntry = _mapping.FirstOrDefault(m => m.Key == requiredField);
      return mapEntry?.Value;
    }

    /// <summary>
    /// The mapping from required field => configured field
    /// </summary>
    public ObservableCollection<MappingEntry> MappingList { get { return _mapping; } private set { _mapping = value; } }

    /// <summary>
    /// Returns the available fields (used by the UI to populate the ComboBox)
    /// </summary>
    public List<string> AvailableFields { get { return _availableFields; } }

    /// <summary>
    /// Initially populates the mapping
    /// </summary>
    void initMapping()
    {
      foreach (var v in _requiredFields)
      {
        Assign(v, guessMappedField(v));
      }
    }

    /// <summary>
    /// Tries to guess which field form the _availableFields maps best to the required field
    /// </summary>
    virtual protected string guessMappedField(string reqField, double threshold = 0.7)
    {
      double maxV = 0;
      int selI = 0;
      for (int i = 0; i < _availableFields.Count; i++)
      {
        foreach (var s in synonyms(reqField))
        {
          double val = StringComparison.ComparisonMetrics.Similarity(s, _availableFields[i],
            StringComparison.Enums.StringComparisonOption.UseHammingDistance | StringComparison.Enums.StringComparisonOption.UseRatcliffObershelpSimilarity
            );
          if (val > maxV)
          {
            selI = i;
            maxV = val;
          }
        }
      }

      if (maxV > threshold)
        return _availableFields[selI];

      return null;
    }

    /// <summary>
    /// Returns potential synonyms for the given field
    /// </summary>
    virtual protected List<string> synonyms(string field)
    {
      return new List<string> { field };
    }
  }

  /// <summary>
  /// Pre-configured mapping for participant mapping (participant import)
  /// </summary>
  public class ParticipantMapping : Mapping
  {
    /// <summary>
    /// Map defining the required fields and potential available fields
    /// </summary>
    static Dictionary<string, List<string>> _requiredField = new Dictionary<string, List<string>>
    {
      { "Name", new List<string>{ "Name", "Nachname" } },
      { "Firstname", new List<string>{"Vorname"} },
      { "Sex", new List<string>{"Geschlecht", "Kategorie", "Sex", "m/w" } },
      { "Year", new List<string>{"Geburtsjahr", "Jahr", "Jahrgang", "JG" } },
      { "Club", new List<string>{"Club", "Verein"} },
      { "Nation", new List<string>{"Nation", "Verband", "Verbandskürzel" } },
      { "Code", new List<string>{"DSV-Id", "Code"} },
      { "SvId", new List<string>{"SvId", "SkiverbandId", "id" } }
    };

    public ParticipantMapping(List<string> availableFields) : base(_requiredField.Keys, availableFields)
    { 
    }

    protected override List<string> synonyms(string field)
    {
      return _requiredField[field];
    }

  }


  /// <summary>
  /// Pre-configured mapping for race mapping (race import)
  /// </summary>
  public class RaceMapping : Mapping
  {
    /// <summary>
    /// Map defining the required fields and potential available fields
    /// </summary>
    static Dictionary<string, List<string>> _requiredField = new Dictionary<string, List<string>>
    {
      { "Name", new List<string>{ "Name", "Nachname" } },
      { "Firstname", new List<string>{"Vorname", "Firstname"} },
      { "Sex", new List<string>{"Geschlecht", "Kategorie", "Sex", "m/w"} },
      { "Year", new List<string>{"Geburtsjahr", "Jahr", "Jahrgang", "JG", "Year" } },
      { "Club", new List<string>{"Club", "Verein"} },
      { "Nation", new List<string>{"Nation", "Verband", "Verbandskürzel" } },
      { "Code", new List<string>{"DSV-Id", "Code" } },
      { "SvId", new List<string>{"SvId", "SkiverbandId", "id" } },
      { "Points", new List<string>{"Points", "Punkte"} },
      { "StartNumber", new List<string>{"start number", "Startnummer", "SN"} },
    };

    public RaceMapping(List<string> availableFields) : base(_requiredField.Keys, availableFields)
    {
    }

    protected override List<string> synonyms(string field)
    {
      return _requiredField[field];
    }

  }


  public class ImportResults
  {
    int _success = 0;
    int _error = 0;
    List<string> _errors;

    public int SuccessCount { get { return _success; } }
    public int ErrorCount { get { return _error; } }
    public List<string> Errors { get { return _errors; } }

    public ImportResults()
    {
      _errors = new List<string>();
    }


    public void AddSuccess()
    {
      _success++;
    }

    public void AddError()
    {
      _error++;
    }

    public void AddError(string message)
    {
      _error++;
      _errors.Add(message);
    }

  }

  public interface IImport
  {
    ImportResults DoImport(DataSet ds);
  }

  public class BaseImport
  {
    protected Mapping _mapping;
    protected ClassAssignment _classAssignment;


    protected BaseImport(Mapping mapping, ClassAssignment classAssignment)
    {
      _mapping = mapping;
      _classAssignment = classAssignment;
    }

    protected object getValueAsObject(DataRow row, string field)
    {
      var columnName = _mapping.MappedField(field);

      if (columnName != null)
        if (row.Table.Columns.Contains(columnName))
          if (!row.IsNull(columnName))
            return row[columnName];

      return null;
    }

    protected string getValueAsString(DataRow row, string field)
    {
      return Convert.ToString(getValueAsObject(row, field));
    }

    protected double getValueAsDouble(DataRow row, string field, double @default = 0.0)
    {
      object v = getValueAsObject(row, field);
      if (v == null)
        return @default;

      try
      {
        return Convert.ToDouble(v, System.Globalization.CultureInfo.InvariantCulture);
      }
      catch (Exception)
      {
        return @default;
      }
    }

    protected uint getValueAsUint(DataRow row, string field, uint @default = 0)
    {
      object v = getValueAsObject(row, field);
      if (v == null)
        return @default;

      try
      {
        return Convert.ToUInt32(v);
      }
      catch(Exception)
      {
        return @default;
      }
    }

    protected void assignClass(Participant participant)
    {
      if (_classAssignment != null)
        participant.Class = _classAssignment.DetermineClass(participant);
    }

  }



  public class ParticipantImport : BaseImport, IImport
  {
    IList<Participant> _particpants;
    IList<ParticipantCategory> _categories;

    public ParticipantImport(IList<Participant> particpants, Mapping mapping, IList<ParticipantCategory> categories, ClassAssignment classAssignment = null) : base(mapping, classAssignment)
    {
      _particpants = particpants;
      _categories = categories;
    }


    public  ImportResults DoImport(DataSet ds)
    {
      ImportResults impRes = new ImportResults();

      var rows = ds.Tables[0].Rows;

      foreach(DataRow row in rows)
      {
        try
        {
          ImportRow(row);
          impRes.AddSuccess();
        }
        catch (Exception)
        {
          impRes.AddError();
        }
      }

      return impRes;
    }


    public Participant ImportRow(DataRow row) 
    {
      Participant partImported = null;

      Participant partCreated = createParticipant(row);

      Participant partExisting = findExistingParticpant(partCreated);

      if (partExisting != null)
        partImported = updateParticipant(partExisting, partCreated);
      else
        partImported = insertParticpant(partCreated);

      assignClass(partImported);

      return partImported;
    }


    Participant createParticipant(DataRow row)
    {
      Participant p = new Participant
      {
        Name = getNameComaSeparated(getValueAsString(row, "Name")),
        Firstname = getFirstNameComaSeparated(getValueAsString(row, "Firstname")),
        Sex = importSex(getValueAsString(row, "Sex")),
        Club = getValueAsString(row, "Club"),
        Nation = getValueAsString(row, "Nation"),
        SvId = getValueAsString(row, "SvId"),
        Code = getValueAsString(row, "Code"),
        Year = getValueAsUint(row, "Year")
      };

      return p;
    }

    string getNameComaSeparated(string name)
    {
      string res;
      var nameParts = name.Split(',');
      if (nameParts.Length > 1)
        res = nameParts[0];
      else
        res = name;

      return res.Trim();
    }

    string getFirstNameComaSeparated(string name)
    {
      string res;
      var nameParts = name.Split(',');
      if (nameParts.Length > 1)
        res = nameParts[nameParts.Length - 1];
      else
        res = name;

      return res.Trim();
    }

    bool sameParticpant(Participant p1, Participant p2)
    {
      if (!string.IsNullOrEmpty(p1.SvId) && !string.IsNullOrEmpty(p2.SvId))
        return p1.SvId == p2.SvId;

      if (!string.IsNullOrEmpty(p1.Code) && !string.IsNullOrEmpty(p2.Code))
        return p1.Code == p2.Code;

      return p1.Fullname == p2.Fullname;

    }

    Participant findExistingParticpant(Participant partImp)
    {
      var pFound = _particpants.FirstOrDefault(p => sameParticpant(p, partImp));
      return pFound;
    }

    Participant updateParticipant(Participant partExisting, Participant partImp)
    {
      partExisting.Assign(partImp);
      return partExisting;
    }

    Participant insertParticpant(Participant partImp)
    {
      _particpants.Add(partImp);
      
      return partImp;
    }

    ParticipantCategory importSex(string sex)
    {
      // Looks in category name first, afterwards in synonyms

      if (string.IsNullOrEmpty(sex))
        return null;

      char sexInvariant = char.ToLowerInvariant(sex[0]);

      ParticipantCategory category = null;
      foreach (var c in _categories)
      {
        if (char.ToLowerInvariant(c.Name) == sexInvariant)
        {
          category = c;
          break;
        }
      }

      if (category == null)
        foreach (var c in _categories)
        {
          if (!string.IsNullOrEmpty(c.Synonyms) && c.Synonyms.ToLowerInvariant().Contains(sexInvariant))
          {
            category = c;
            break;
          }
        }

      return category;
    }
  }


  public class RaceImport : BaseImport, IImport
  {
    Race _race;

    ParticipantImport _particpantImport;

    public RaceImport(Race race, Mapping mapping, ClassAssignment classAssignment = null) : base(mapping, classAssignment)
    {
      _race = race;

      _particpantImport = new ParticipantImport(_race.GetDataModel().GetParticipants(), _mapping, _race.GetDataModel().GetParticipantCategories(), _classAssignment);
    }


    public ImportResults DoImport(DataSet ds)
    {
      ImportResults impRes = new ImportResults();

      var rows = ds.Tables[0].Rows;
      foreach (DataRow row in rows)
      {
        try
        {
          RaceParticipant rp = ImportRow(row);
          impRes.AddSuccess();
        }
        catch (Exception)
        {
          impRes.AddError();
        }
      }

      return impRes;
    }


    public RaceParticipant ImportRow(DataRow row)
    {
      Participant importedParticipant = _particpantImport.ImportRow(row);

      double points = getPoints(row);
      uint sn = getStartNumber(row);

      RaceParticipant rp = _race.AddParticipant(importedParticipant, sn, points);

      return rp;
    }


    double getPoints(DataRow row) 
    {
      return getValueAsDouble(row, "Points", -1);
    }


    uint getStartNumber(DataRow row)
    {
      return getValueAsUint(row, "StartNumber", 0);
    }
  }


  public class UpdatePointsImport : BaseImport, IImport
  {
    Race _race;

    Dictionary<string, DataRow> _id2row;


    public UpdatePointsImport(Race race, Mapping mapping) : base(mapping, null)
    {
      _race = race;

      _id2row = new Dictionary<string, DataRow>();
    }


    public ImportResults DoImport(DataSet ds)
    {
      buildDictionary(ds);

      ImportResults impRes = new ImportResults();

      // Update the points for all participants in the race
      foreach(var rp in _race.GetParticipants() )
      {
        string key = rp.SvId;

        try
        {
          DataRow row = _id2row[key];
          double points = getPoints(row);
          rp.Points = points;
          impRes.AddSuccess();
        }
        catch (KeyNotFoundException)
        {
          rp.Points = 99999.99;
          impRes.AddError(string.Format("{0} (SvId: {1}) ist nicht der der Punktedatei. Punkte wurden auf {2:0.00} gesetzt", rp.Fullname, rp.SvId, rp.Points));
        }
        catch (Exception)
        {
          impRes.AddError();
        }
      }

      return impRes;
    }


    double getPoints(DataRow row)
    {
      return getValueAsDouble(row, "Points", -1);
    }


    protected void buildDictionary(DataSet ds)
    {
      // Build map for fast and easy access to row
      var rows = ds.Tables[0].Rows;
      foreach (DataRow row in rows)
      {
        string key = getValueAsString(row, "SvId");
        _id2row.Add(key, row);
      }
    }


  }


}
