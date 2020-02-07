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

    static string mapClub(Dictionary<string, string> map, string original)
    {
      if (!map.ContainsKey(original))
        map[original] = string.Format("Verein {0}", map.Count + 1);

      return map[original];
    }
  }
}
