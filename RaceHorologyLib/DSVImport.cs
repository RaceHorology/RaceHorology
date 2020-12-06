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

  static public class DSVUpdatePoints
  {
    static public List<ImportResults> UpdatePoints(AppDataModel dm, DataSet data, Mapping mapping, string usedDSVList)
    {
      List<ImportResults> impRes = new List<ImportResults>();
      
      foreach (Race race in dm.GetRaces())
      {
        UpdatePointsImport import = new UpdatePointsImport(race, mapping);
        var res = import.DoImport(data);
        impRes.Add(res);
      }
      dm.GetDB().StoreKeyValue("DSV_UsedDSVList", usedDSVList);

      return impRes;
    }
  }

  /// <summary>
  /// Pre-configured mapping for race mapping (race import)
  /// </summary>
  public class DSVMapping : Mapping
  {
    /// <summary>
    /// Map defining the required fields and potential available fields
    /// </summary>
    static Dictionary<string, List<string>> _requiredField = new Dictionary<string, List<string>>
    {
      { "SvId", new List<string>{"SvId"} },
      { "Name", new List<string>{ "Name" } },
      { "Firstname", new List<string>{ "Firstname"} },
      { "Year", new List<string>{ "Year" } },
      { "Club", new List<string>{ "Club" } },
      { "Nation", new List<string>{ "Verband" } },
      { "Points", new List<string>{"Points"} },
      { "Sex", new List<string>{"Sex"} }
    };

    static List<string> _availableFields = new List<string>
    {
      "SvId",
      "Name",
      "Firstname",
      "Year",
      "Club",
      "Nation",
      "Points",
      "Sex"
    };

    public DSVMapping() : base(_requiredField.Keys, _availableFields)
    {
    }

    protected override List<string> synonyms(string field)
    {
      return _requiredField[field];
    }

  }


  public class DSVImportReader : IImportReader
  {
    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    protected List<string> _columns;
    protected DataSet _dataSet;
    protected string _usedDSVList;
    protected DateTime? _listDate;

    public Mapping Mapping { get; protected set; }



    public DSVImportReader(IDSVImportReaderFile dsvFile )
    {
      readData(dsvFile.GetStream(), dsvFile.GetDSVListname());
    }

    public DataSet Data { get => _dataSet; }

    public List<string> Columns { get => _columns; }

    public string UsedDSVList { get => _usedDSVList; }

    public DateTime? Date { get => _listDate; }

    protected void readData(Stream dsvData, string usedDsvList)
    {
      Mapping = new DSVMapping();

      _usedDSVList = usedDsvList;

      StreamReader sr = new StreamReader(dsvData, true);

      _dataSet = new DataSet();
      DataTable table = _dataSet.Tables.Add();

      table.Columns.Add("SvId");
      table.Columns.Add("Name");
      table.Columns.Add("Firstname");
      table.Columns.Add("Year", typeof(uint));
      table.Columns.Add("Club");
      table.Columns.Add("Verband");
      table.Columns.Add("Points", typeof(double));
      table.Columns.Add("Sex");

      string line;
      int lineNumber = 0;
      while ((line = sr.ReadLine()) != null)
      {
        try
        {
          string id = line.Substring(0, 10).Trim();

          if (id == "1000") // Last line
          {
            string tmpName;
            parseLastLine(line, out tmpName, out _listDate);
            break;
          }

          string name = line.Substring(10, 20).Trim();
          string firstname = line.Substring(30, 14).Trim();
          string year = line.Substring(44, 10).Trim();
          string club = line.Substring(54, 30).Trim();
          string region = line.Substring(84, 10).Trim();
          string points = line.Substring(94, 10).Trim();
          string sex = line.Substring(104).Trim();
          DataRow row = table.NewRow();
          row["SvId"] = id;
          row["Name"] = name;
          row["Firstname"] = firstname;
          row["Year"] = uint.Parse(year);
          row["Club"] = club;
          row["Verband"] = region;
          row["Points"] = double.Parse(points, System.Globalization.CultureInfo.InvariantCulture);
          row["Sex"] = sex;

          table.Rows.Add(row);
        }
        catch(Exception e)
        {
          logger.Warn(e, string.Format("Could not import line {0}", lineNumber));
        }

        lineNumber++;
      }

      _columns = ImportUtils.extractFields(_dataSet);
    }

    void parseLastLine(string line, out string name, out DateTime? date)
    {
      string tmpName = line.Substring(10, 20).Trim();
      string tmpDate = line.Substring(30, 14).Trim();

      try
      {
        date = DateTime.ParseExact(tmpDate, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
        name = tmpName;
      }
      catch (FormatException)
      {
        name = null;
        date = null;
      }
    }
  }


  public interface IDSVImportReaderFile
  {
    Stream GetStream();
    string GetDSVListname();
  }



  public class DSVImportReaderFile : IDSVImportReaderFile
  {
    string _dsvDataFilename;
    public DSVImportReaderFile(string dsvDataFilename)
    {
      _dsvDataFilename = dsvDataFilename;
    }

    public Stream GetStream()
    {
      return File.Open(_dsvDataFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public string GetDSVListname()
    {
      return System.IO.Path.GetFileNameWithoutExtension(_dsvDataFilename);
    }
  }


  public class DSVImportReaderZipBase : IDSVImportReaderFile
  {
    string _dsvList;
    Stream _stream;

    protected DSVImportReaderZipBase()
    {
    }


    public Stream GetStream()
    {
      return _stream;
    }


    public string GetDSVListname()
    {
      return _dsvList;
    }


    protected void parseZip(Stream zipStream)
    {
      ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
      {
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
          if (entry.FullName.StartsWith("DSVSA") && entry.FullName.EndsWith(".txt"))
          {
            _dsvList = System.IO.Path.GetFileNameWithoutExtension(entry.FullName);
            _stream = entry.Open();
            return;
          }
        }
      }
    }
  }

  public class DSVImportReaderZip : DSVImportReaderZipBase
  {
    string _zipPath;

    public DSVImportReaderZip(string path)
    {
      _zipPath = path;

      parseZip(new FileStream(_zipPath, FileMode.Open, FileAccess.Read, FileShare.Read));
    }
  }



  public class DSVImportReaderOnline : DSVImportReaderZipBase
  {

    public DSVImportReaderOnline()
    {
      parseZip(getZipOnlineStream());
    }


    protected static Stream getZipOnlineStream()
    {
      WebClient client = new WebClient();

      Stream stream = client.OpenRead("https://alpin.rennverwaltung.de/punktelisten/zip");

      return stream;
    }
  }


}
