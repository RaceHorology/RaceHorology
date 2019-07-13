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
      writer.WriteValue(value.Class);
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
      writer.WriteValue(value.Class);
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


  public static class JsonConversion
  {
    public static string ConvertStartList(IEnumerable startList)
    {
      JsonSerializer serializer = new JsonSerializer();

      serializer.Converters.Add(new RaceParticipantConverter());

      StringWriter sw = new StringWriter();
      using (JsonWriter writer = new JsonTextWriter(sw))
      {
        serializer.Serialize(writer, startList);
      }

      return sw.ToString();
    }

    public static string ConvertRunResults(IEnumerable resultList)
    {
      JsonSerializer serializer = new JsonSerializer();

      serializer.Converters.Add(new RunResultConverter());

      StringWriter sw = new StringWriter();
      using (JsonWriter writer = new JsonTextWriter(sw))
      {
        serializer.Serialize(writer, resultList);
      }

      return sw.ToString();
    }

  }
}
