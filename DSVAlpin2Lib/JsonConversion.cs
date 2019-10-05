using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
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

  public class RunResultConverter : JsonConverter<RunResultWithPosition>
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
      writer.WriteValue(value.Runtime?.ToString(@"mm\:ss\,ff"));
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
    public static string ConvertStartList(IEnumerable startList)
    {
      var wrappedData = new Dictionary<string, object>
      {
        {"type", "startlist" },
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

    public static string ConvertRunResults(IEnumerable resultList)
    {
      var wrappedData = new Dictionary<string, object>
      {
        {"type", "racerunresult" },
        {"data",  resultList}
      };

      JsonSerializer serializer = new JsonSerializer();

      serializer.Converters.Add(new RunResultConverter());

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


  }
}
