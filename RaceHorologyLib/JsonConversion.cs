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
      writer.WriteValue(value.Runtime.ToRaceTimeString());
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

      writer.WritePropertyName("DisqualText");
      writer.WriteValue(value.DisqualText);

      writer.WritePropertyName("Runtimes");
      writer.WriteStartArray();
      for(uint i=1; i<=_runs; i++)
      {
        if (value.RunTimes.ContainsKey(i) && value.RunResultCodes.ContainsKey(i))
        {
          string str = (string)_timeConverter.Convert(new object[] { value.RunTimes[i], value.RunResultCodes[i] }, typeof(string), null, null);
          writer.WriteValue(str);
        }
        else
          writer.WriteValue(string.Empty);
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
        groupedData.Add(null, dstItems);

        foreach (var item in cv.SourceCollection)
          dstItems.Add(item);
      }

      return groupedData;
    }
    

    public static string ConvertStartList(ICollectionView startList)
    {
      var wrappedData = new Dictionary<string, object>
      {
        {"type", "startlist" },
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
      var wrappedData = new Dictionary<string, object>
      {
        {"type", "racerunresult" },
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
        { "Runtimes", new Dictionary<string,string> { { "Runtime1", "Zeit 1" }, { "Runtime2", "Zeit 2" } } }
      };

      var wrappedData = new Dictionary<string, object>
      {
        {"type", "raceresult" },
        {"fields", fields },
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

    public static string ConvertMetaData(ParticipantClass[] classes, ParticipantGroup[] groups, string[] sex, string[] grouping)
    {
      var wrappedData = new Dictionary<string, object>
      {
        {"type", "metadata" },
        {"data",  new Dictionary<string, object>
          {
            {"classes", classes },
            {"groups", groups},
            {"sex", sex},
            {"groupings", grouping}
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
