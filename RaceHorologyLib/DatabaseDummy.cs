using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public class DatabaseDummy : IAppDataModelDataBase
  {
    List<Race.RaceProperties> _races;
    string _basePath;

    public DatabaseDummy(string basePath)
    {
      _races = new List<Race.RaceProperties>();
      _races.Add(new Race.RaceProperties
      {
        RaceType = Race.ERaceType.GiantSlalom,
        Runs = 2
      });
      _basePath = basePath;
    }

    public string GetDBPath() { return System.IO.Path.Combine(_basePath, GetDBFileName()); }
    public string GetDBFileName() { return "dummy.mdb"; }
    public string GetDBPathDirectory() { return _basePath; }


    public ItemsChangeObservableCollection<Participant> GetParticipants() { return new ItemsChangeObservableCollection<Participant>(); }

    public List<ParticipantGroup> GetParticipantGroups() { return new List<ParticipantGroup>(); }
    public List<ParticipantClass> GetParticipantClasses() { return new List<ParticipantClass>(); }
    public List<ParticipantCategory> GetParticipantCategories() { return new List<ParticipantCategory>(); }


    public List<Race.RaceProperties> GetRaces() { return _races; }
    public List<RaceParticipant> GetRaceParticipants(Race race) { return new List<RaceParticipant>(); }

    public List<RunResult> GetRaceRun(Race race, uint run) { return new List<RunResult>(); }

    public AdditionalRaceProperties GetRaceProperties(Race race) { return null; }
    public void StoreRaceProperties(Race race, AdditionalRaceProperties props) { }

    public void CreateOrUpdateParticipant(Participant participant) { }
    public void RemoveParticipant(Participant participant) { }

    public void CreateOrUpdateRaceParticipant(RaceParticipant participant) { }
    public void RemoveRaceParticipant(RaceParticipant raceParticipant) { }

    public void CreateOrUpdateRunResult(Race race, RaceRun raceRun, RunResult result) { }
    public void DeleteRunResult(Race race, RaceRun raceRun, RunResult result) { }

    public void UpdateRace(Race race, bool active) { }

    public void CreateOrUpdateClass(ParticipantClass c) { }

    public void RemoveClass(ParticipantClass c) { }

    public void CreateOrUpdateGroup(ParticipantGroup g) { }

    public void RemoveGroup(ParticipantGroup g) { }
    public void CreateOrUpdateCategory(ParticipantCategory c) { }
    public void RemoveCategory(ParticipantCategory c) { }

    public void StoreKeyValue(string key, string value) { }
    public string GetKeyValue(string key) { return null; }
  };
}
