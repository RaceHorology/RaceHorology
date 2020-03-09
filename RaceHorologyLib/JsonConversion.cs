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

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  public class RaceParticipantConverter : JsonConverter<RaceParticipant>
  {
    public override void WriteJson(JsonWriter writer, RaceParticipant value, JsonSerializer serializer)
    {
      writer.WriteStartObject();
      writer.WritePropertyName("Id");
      writer.WriteValue(value.Id);
      writer.WritePropertyName("StartNumber");
      writer.WriteValue(value.StartNumber);
      writer.WritePropertyName("Name");
      writer.WriteValue(value.Name);
      writer.WritePropertyName("Firstname");
      writer.WriteValue(value.Firstname);
      writer.WritePropertyName("Sex");
      writer.WriteValue(value.Sex);
      writer.WritePropertyName("Year");
      writer.WriteValue(value.Year);
      writer.WritePropertyName("Club");
      writer.WriteValue(value.Club);
      writer.WritePropertyName("Nation");
      writer.WriteValue(value.Nation);
      writer.WritePropertyName("Class");
      writer.WriteValue(value.Class.ToString());
      writer.WritePropertyName("Group");
      writer.WriteValue(value.Class.Group.ToString());
      writer.WriteEndObject();
    }

    public override RaceParticipant ReadJson(JsonReader reader, Type objectType, RaceParticipant existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }
  }

  public class RunResultWPConverter : JsonConverter<RunResultWithPosition>
  {
    ResultTimeAndCodeConverter _timeConverter = new ResultTimeAndCodeConverter();

    public override void WriteJson(JsonWriter writer, RunResultWithPosition value, JsonSerializer serializer)
    {
      writer.WriteStartObject();
      writer.WritePropertyName("Id");
      writer.WriteValue(value.Id);
      writer.WritePropertyName("Position");
      writer.WriteValue(value.Position);
      writer.WritePropertyName("StartNumber");
      writer.WriteValue(value.StartNumber);
      writer.WritePropertyName("Name");
      writer.WriteValue(value.Name);
      writer.WritePropertyName("Firstname");
      writer.WriteValue(value.Firstname);
      writer.WritePropertyName("Sex");
      writer.WriteValue(value.Sex);
      writer.WritePropertyName("Year");
      writer.WriteValue(value.Year);
      writer.WritePropertyName("Club");
      writer.WriteValue(value.Club);
      writer.WritePropertyName("Nation");
      writer.WriteValue(value.Nation);
      writer.WritePropertyName("Class");
      writer.WriteValue(value.Class.ToString());
      writer.WritePropertyName("Group");
      writer.WriteValue(value.Class.Group.ToString());

      writer.WritePropertyName("Runtime");

      string str = (string)_timeConverter.Convert(new object[] { value.Runtime, value.ResultCode }, typeof(string), null, null);
      writer.WriteValue(str);


      writer.WritePropertyName("DiffToFirst");
      writer.WriteValue(value.DiffToFirst.ToRaceTimeString());
      writer.WritePropertyName("DisqualText");
      writer.WriteValue(value.DisqualText);
      writer.WritePropertyName("JustModified");
      writer.WriteValue(value.JustModified);
      writer.WriteEndObject();
    }

    public override RunResultWithPosition ReadJson(JsonReader reader, Type objectType, RunResultWithPosition existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }
  }


  public class RunResultConverter : JsonConverter<RunResult>
  {
    bool _precisionIn100seconds;
    bool _onlyTimeData;

    public RunResultConverter(bool precisionIn100seconds, bool onlyTimeData)
    {
      _precisionIn100seconds = precisionIn100seconds;
      _onlyTimeData = onlyTimeData;
    }
    public override void WriteJson(JsonWriter writer, RunResult value, JsonSerializer serializer)
    {
      writer.WriteStartObject();

      if (!_onlyTimeData)
      {
        writer.WritePropertyName("Id");
        writer.WriteValue(value.Id);
        writer.WritePropertyName("StartNumber");
        writer.WriteValue(value.StartNumber);
        writer.WritePropertyName("Name");
        writer.WriteValue(value.Name);
        writer.WritePropertyName("Firstname");
        writer.WriteValue(value.Firstname);
        writer.WritePropertyName("Sex");
        writer.WriteValue(value.Sex);
        writer.WritePropertyName("Year");
        writer.WriteValue(value.Year);
        writer.WritePropertyName("Club");
        writer.WriteValue(value.Club);
        writer.WritePropertyName("Nation");
        writer.WriteValue(value.Nation);
        writer.WritePropertyName("Class");
        writer.WriteValue(value.Class.ToString());
        writer.WritePropertyName("Group");
        writer.WriteValue(value.Class.Group.ToString());
      }
      writer.WritePropertyName("Runtime");
      if (_precisionIn100seconds)
        writer.WriteValue(value.Runtime.ToRaceTimeString());
      else
        writer.WriteValue(value.Runtime?.ToString(@"mm\:ss"));

      writer.WritePropertyName("DisqualText");
      writer.WriteValue(value.DisqualText);

      writer.WriteEndObject();
    }

    public override RunResult ReadJson(JsonReader reader, Type objectType, RunResult existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }
  }


  public class RaceResultConverter : JsonConverter<RaceResultItem>
  {
    ResultTimeAndCodeConverter _timeConverter = new ResultTimeAndCodeConverter();

    private uint _runs;
    public RaceResultConverter(uint runs)
    {
      _runs = runs;
    }

    public override void WriteJson(JsonWriter writer, RaceResultItem value, JsonSerializer serializer)
    {
      writer.WriteStartObject();
      writer.WritePropertyName("Id");
      writer.WriteValue(value.Participant.Id);
      writer.WritePropertyName("Position");
      writer.WriteValue(value.Position);
      writer.WritePropertyName("StartNumber");
      writer.WriteValue(value.Participant.StartNumber);
      writer.WritePropertyName("Name");
      writer.WriteValue(value.Participant.Name);
      writer.WritePropertyName("Firstname");
      writer.WriteValue(value.Participant.Firstname);
      writer.WritePropertyName("Sex");
      writer.WriteValue(value.Participant.Sex);
      writer.WritePropertyName("Year");
      writer.WriteValue(value.Participant.Year);
      writer.WritePropertyName("Club");
      writer.WriteValue(value.Participant.Club);
      writer.WritePropertyName("Nation");
      writer.WriteValue(value.Participant.Nation);
      writer.WritePropertyName("Class");
      writer.WriteValue(value.Participant.Class.ToString());
      writer.WritePropertyName("Group");
      writer.WriteValue(value.Participant.Class.Group.ToString());
      writer.WritePropertyName("Totaltime");
      writer.WriteValue(value.TotalTime.ToRaceTimeString());
      writer.WritePropertyName("DiffToFirst");
      writer.WriteValue(value.DiffToFirst.ToRaceTimeString());
      writer.WritePropertyName("DisqualText");
      writer.WriteValue(value.DisqualText);

      writer.WritePropertyName("Runtimes");
      writer.WriteStartArray();
      for(uint i=1; i<=_runs; i++)
      {
        writer.WriteStartObject();
        if (value.SubResults.ContainsKey(i))
        {
          string str = (string)_timeConverter.Convert(new object[] { value.SubResults[i].Runtime, value.SubResults[i].RunResultCode }, typeof(string), null, null);
          writer.WritePropertyName("Runtime");
          writer.WriteValue(str);

          writer.WritePropertyName("Position");
          writer.WriteValue(value.SubResults[i].Position);
        }
        writer.WriteEndObject();
      }
      writer.WriteEndArray();
      
      writer.WritePropertyName("JustModified");
      writer.WriteValue(value.JustModified);
      writer.WriteEndObject();
    }

    public override RaceResultItem ReadJson(JsonReader reader, Type objectType, RaceResultItem existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }
  }



  public class StartListEntryConverter : JsonConverter<StartListEntry>
  {
    public override void WriteJson(JsonWriter writer, StartListEntry value, JsonSerializer serializer)
    {
      writer.WriteStartObject();
      writer.WritePropertyName("Id");
      writer.WriteValue(value.Id);
      writer.WritePropertyName("StartNumber");
      writer.WriteValue(value.StartNumber);
      writer.WritePropertyName("Name");
      writer.WriteValue(value.Name);
      writer.WritePropertyName("Firstname");
      writer.WriteValue(value.Firstname);
      writer.WritePropertyName("Sex");
      writer.WriteValue(value.Sex);
      writer.WritePropertyName("Year");
      writer.WriteValue(value.Year);
      writer.WritePropertyName("Club");
      writer.WriteValue(value.Club);
      writer.WritePropertyName("Nation");
      writer.WriteValue(value.Nation);
      writer.WritePropertyName("Class");
      writer.WriteValue(value.Class.ToString());
      writer.WritePropertyName("Group");
      writer.WriteValue(value.Class.Group.ToString());
      writer.WriteEndObject();
    }

    public override StartListEntry ReadJson(JsonReader reader, Type objectType, StartListEntry existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }
  }


  public static class JsonConversion
  {

    public static Dictionary<object, object> GroupData(ICollectionView cv)
    {
      Dictionary<object, object> groupedData = new Dictionary<object, object>();

      var lr = cv as System.Windows.Data.ListCollectionView;
      if (lr.Groups != null)
      {
        foreach (var group in lr.Groups)
        {
          System.Windows.Data.CollectionViewGroup cvGroup = group as System.Windows.Data.CollectionViewGroup;

          List<object> dstItems = new List<object>();
          groupedData.Add(cvGroup.Name, dstItems);

          foreach (var item in cvGroup.Items)
            dstItems.Add(item);
        }
      }
      else
      {
        List<object> dstItems = new List<object>();
        groupedData.Add("", dstItems);

        foreach (var item in cv.SourceCollection)
          dstItems.Add(item);
      }

      return groupedData;
    }

    public static string GetGroupBy(ICollectionView cv)
    { 
      string groupby = "";
      if (cv.GroupDescriptions.Count > 0)
      {
        if (cv.GroupDescriptions[0] is System.Windows.Data.PropertyGroupDescription pgd)
        {
          groupby = pgd.PropertyName.Split('.')[1];
        }
      }

      return groupby;
    }


    public static string ConvertStartList(ICollectionView startList)
    {
      var wrappedData = new Dictionary<string, object>
      {
        {"type", "startlist" },
        {"groupby", GetGroupBy(startList)},
        {"data",  GroupData(startList)}
      };

      JsonSerializer serializer = new JsonSerializer();

      serializer.Converters.Add(new RaceParticipantConverter());
      serializer.Converters.Add(new StartListEntryConverter());

      StringWriter sw = new StringWriter();
      using (JsonWriter writer = new JsonTextWriter(sw))
      {
        serializer.Serialize(writer, wrappedData);
      }

      return sw.ToString();
    }

    public static string ConvertOnStartList(IEnumerable startList)
    {
      var wrappedData = new Dictionary<string, object>
      {
        {"type", "onstart" },
        {"data",  startList}
      };

      JsonSerializer serializer = new JsonSerializer();

      serializer.Converters.Add(new RaceParticipantConverter());
      serializer.Converters.Add(new StartListEntryConverter());

      StringWriter sw = new StringWriter();
      using (JsonWriter writer = new JsonTextWriter(sw))
      {
        serializer.Serialize(writer, wrappedData);
      }

      return sw.ToString();
    }


    public static string ConvertOnTrack(IEnumerable resultList)
    {
      var wrappedData = new Dictionary<string, object>
      {
        {"type", "ontrack" },
        {"data",  resultList}
      };

      JsonSerializer serializer = new JsonSerializer();

      serializer.Converters.Add(new RunResultConverter(false, false));

      StringWriter sw = new StringWriter();
      using (JsonWriter writer = new JsonTextWriter(sw))
      {
        serializer.Serialize(writer, wrappedData);
      }

      return sw.ToString();
    }



    public static string ConvertRunResults(ICollectionView results)
    {
      var fields = new Dictionary<string, object>
      { { "Id", "Id" },
        { "Position", "Platz" },
        { "StartNumber", "Startnummer" },
        { "Name", "Nachname" },
        { "Firstname", "Vorname" },
        { "Sex", "Geschlecht" },
        { "Year", "Jahr" },
        { "Club", "Verein" },
        { "Nation", "Nation" },
        { "Class", "Klasse" },
        { "Group", "Gruppe" },
        { "Runtime", "Zeit" },
        { "DiffToFirst", "Diff" },
        { "DisqualText", "Bemerkung" }
      };


      var wrappedData = new Dictionary<string, object>
      {
        {"type", "racerunresult" },
        {"fields", fields },
        {"groupby", GetGroupBy(results)},
        {"data",  GroupData(results)}
      };

      JsonSerializer serializer = new JsonSerializer();

      serializer.Converters.Add(new RunResultWPConverter());

      StringWriter sw = new StringWriter();
      using (JsonWriter writer = new JsonTextWriter(sw))
      {
        serializer.Serialize(writer, wrappedData);
      }

      return sw.ToString();
    }

    public static string ConvertRaceResults(ICollectionView results, uint runs)
    {

      var fields = new Dictionary<string, object> 
      { { "Id", "Id" },
        { "Position", "Platz" },
        { "StartNumber", "Startnummer" },
        { "Name", "Nachname" },
        { "Firstname", "Vorname" },
        { "Sex", "Geschlecht" },
        { "Year", "Jahr" },
        { "Club", "Verein" },
        { "Nation", "Nation" },
        { "Class", "Klasse" },
        { "Group", "Gruppe" },
        { "Totaltime", "Zeit" },
        { "Runtimes", new Dictionary<string,string> { { "Runtime1", "Zeit 1" }, { "Runtime2", "Zeit 2" } } },
        { "DiffToFirst", "Diff" },
        { "DisqualText", "Bemerkung" }
      };


      var wrappedData = new Dictionary<string, object>
      {
        {"type", "raceresult" },
        {"fields", fields },
        {"groupby", GetGroupBy(results)},
        {"data",  GroupData(results)}
      };

      JsonSerializer serializer = new JsonSerializer();

      serializer.Converters.Add(new RaceResultConverter(runs));

      StringWriter sw = new StringWriter();
      using (JsonWriter writer = new JsonTextWriter(sw))
      {
        serializer.Serialize(writer, wrappedData);
      }

      return sw.ToString();
    }


    public static string ConvertCurrrentRaceRun(IEnumerable data)
    {
      var wrappedData = new Dictionary<string, object>
      {
        {"type", "currentracerun" },
        {"data",  data}
      };

      JsonSerializer serializer = new JsonSerializer();

      StringWriter sw = new StringWriter();
      using (JsonWriter writer = new JsonTextWriter(sw))
      {
        serializer.Serialize(writer, wrappedData);
      }

      return sw.ToString();
    }


    public static string ConvertEvent(RaceParticipant particpant, string eventType, RunResult runResult)
    {

      var data = new Dictionary<string, object>
      {
        { "EventType", eventType },
        { "Participant", particpant },
        { "RunResult", runResult }
      };

      var wrappedData = new Dictionary<string, object>
      {
        {"type", "event_participant" },
        {"data",  data}
      };

      JsonSerializer serializer = new JsonSerializer();

      serializer.Converters.Add(new RaceParticipantConverter());
      serializer.Converters.Add(new RunResultConverter(true, true));


      StringWriter sw = new StringWriter();
      using (JsonWriter writer = new JsonTextWriter(sw))
      {
        serializer.Serialize(writer, wrappedData);
      }

      return sw.ToString();
    }



    public static string ConvertWrappedData(IEnumerable wrappedData)
    {
      JsonSerializer serializer = new JsonSerializer();

      StringWriter sw = new StringWriter();
      using (JsonWriter writer = new JsonTextWriter(sw))
      {
        serializer.Serialize(writer, wrappedData);
      }

      return sw.ToString();
    }

    public static string ConvertMetaData(ParticipantClass[] classes, ParticipantGroup[] groups, string[] sex, string[] grouping, int runs)
    {
      var wrappedData = new Dictionary<string, object>
      {
        {"type", "metadata" },
        {"data",  new Dictionary<string, object>
          {
            {"classes", classes },
            {"groups", groups},
            {"sex", sex},
            {"groupings", grouping},
            {"runs", runs}
          }
        }
      };

      JsonSerializer serializer = new JsonSerializer();

      StringWriter sw = new StringWriter();
      using (JsonWriter writer = new JsonTextWriter(sw))
      {
        serializer.Serialize(writer, wrappedData);
      }

      return sw.ToString();
    }



  }
}
