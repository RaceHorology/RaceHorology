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

using System.Collections.Generic;

/// <summary>
/// Defines the interface to the actual database engine
/// </summary>
/// <remarks>Assuming the database format changes we can simply create another implementation.</remarks>
namespace RaceHorologyLib
{

  public interface IAppDataModelDataBase
  {

    void Close();

    string GetDBPath();
    string GetDBFileName();
    string GetDBPathDirectory();


    ItemsChangeObservableCollection<Participant> GetParticipants();

    List<ParticipantGroup> GetParticipantGroups();
    List<ParticipantClass> GetParticipantClasses();
    List<ParticipantCategory> GetParticipantCategories();
    List<Team> GetTeams();
    List<TeamGroup> GetTeamGroups();

    List<Race.RaceProperties> GetRaces();
    List<RaceParticipant> GetRaceParticipants(Race race, bool ignoreAktiveFlag = false);

    List<RunResult> GetRaceRun(Race race, uint run);

    AdditionalRaceProperties GetRaceProperties(Race race);
    void StoreRaceProperties(Race race, AdditionalRaceProperties props);

    string GetTimingDevice(Race race);
    void StoreTimingDevice(Race race, string timingDevice);

    void CreateOrUpdateParticipant(Participant participant);
    void RemoveParticipant(Participant participant);

    void CreateOrUpdateClass(ParticipantClass c);
    void RemoveClass(ParticipantClass c);
    void CreateOrUpdateGroup(ParticipantGroup g);
    void RemoveGroup(ParticipantGroup g);
    void CreateOrUpdateCategory(ParticipantCategory c);
    void RemoveCategory(ParticipantCategory c);
    void CreateOrUpdateTeam(Team t);
    void RemoveTeam(Team t);
    void CreateOrUpdateTeamGroup(TeamGroup g);
    void RemoveTeamGroup(TeamGroup g);


    void CreateOrUpdateRaceParticipant(RaceParticipant participant);
    void RemoveRaceParticipant(RaceParticipant raceParticipant);

    void CreateOrUpdateRunResult(Race race, RaceRun raceRun, RunResult result);
    void DeleteRunResult(Race race, RaceRun raceRun, RunResult result);

    void UpdateRace(Race race, bool active);



    void CreateOrUpdateTimestamp(RaceRun raceRun, Timestamp timestamp);
    List<Timestamp> GetTimestamps(Race raceRun, uint run);
    void RemoveTimestamp(RaceRun raceRun, Timestamp timestamp);


    PrintCertificateModel GetCertificateModel(Race race);
    void SaveCertificateModel(Race race, PrintCertificateModel model);


    RefereeReportItems GetRefereeReport(Race race);
    void SaveRefereeReport(Race race, RefereeReportItems report);


    void StoreKeyValue(string key, string value);
    string GetKeyValue(string key);
  };
}
