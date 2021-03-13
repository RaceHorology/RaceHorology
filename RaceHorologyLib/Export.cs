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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public class Export
  {
    public DataSet ExportToDataSet(Race race)
    {
      DataSet ds = new DataSet();

      DataTable table = createTable(ds);

      foreach(var rp in race.GetParticipants())
      {
        DataRow row = createDataRow(table, rp);
        table.Rows.Add(row);
      }

      return ds;
    }


    protected DataTable createTable(DataSet ds)
    {
      DataTable table = ds.Tables.Add();

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

      table.Columns.Add("StartNumber");
      table.Columns.Add("Points", typeof(double));

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

      return row;
    }


  }
}
