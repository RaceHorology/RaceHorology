/*
 *  Copyright (C) 2019 - 2026 by Sven Flossmann & Co-Authors (CREDITS.TXT)
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

using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RHDevTools
{
  public static class DSVAlpinDBTools
  {
    public static void AnonymizeDB(string path)
    {

      Database db = new Database();
      db.Connect(path);

      AppDataModel model = new AppDataModel(db);

      var particpants = db.GetParticipants();

      var races = db.GetRaces();
      var raceParticipants = db.GetRaceParticipants(new Race(db, model, races[0]));

      Dictionary<string, string> clubMap = new Dictionary<string, string>();
      foreach (var rp in raceParticipants)
      {
        rp.Participant.Firstname = string.Format("Vorname {0}", rp.StartNumber);
        rp.Participant.Name = string.Format("Name {0}", rp.StartNumber);
        rp.Participant.Club = mapClub(clubMap, rp.Participant.Club);

        db.CreateOrUpdateParticipant(rp.Participant);
      }

      db.Close();
    }


    public static void UpgradeSchema(string path)
    {

      Database db = new Database();
      db.Connect(path);
      db.Close();
    }



    static string mapClub(Dictionary<string, string> map, string original)
    {
      if (!map.ContainsKey(original))
        map[original] = string.Format("Verein {0}", map.Count + 1);

      return map[original];
    }
  }
}
