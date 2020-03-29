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
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLibTest
{
  static class TestUtilities
  {
    public static string CreateWorkingFileFrom(string srcDirectory, string srcFilename)
    {
      string srcPath = Path.Combine(srcDirectory, srcFilename);

      string dstDirectory = Path.Combine(srcDirectory, Path.GetRandomFileName());
      Directory.CreateDirectory(dstDirectory);

      string dstPath = Path.Combine(dstDirectory, srcFilename);
      File.Copy(srcPath, dstPath);

      var additionalFiles = Directory.GetFiles(srcDirectory, Path.GetFileNameWithoutExtension(srcFilename) + "*");
      foreach(var f in additionalFiles)
      {
        if (f == srcPath)
          continue;

        string dstF = Path.Combine(dstDirectory, Path.GetFileName(f));
        File.Copy(f, dstF);
      }

      return dstPath;
    }

    public static string Copy(string srcFilepath, string dstFilename)
    {
      string dstFilepath = Path.Combine(Path.GetDirectoryName(srcFilepath), dstFilename);
      File.Copy(srcFilepath, dstFilepath);

      return dstFilepath;
    }

  }

  public class DBTestUtilities
  {
    string _filename;
    OleDbConnection _conn;
    public DBTestUtilities(string filename)
    {
      _filename = filename;
      _conn = new OleDbConnection
      {
        ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data source= " + filename
      };
      _conn.Open();
    }

    public void Close()
    {
      _conn.Close();
    }

    public void ClearTimeMeasurements()
    {
      string sql = @"DELETE FROM tblZeit";
      var cmd = new OleDbCommand(sql, _conn);
      cmd.CommandType = System.Data.CommandType.Text;
      int temp = cmd.ExecuteNonQuery();
    }
  }

}
