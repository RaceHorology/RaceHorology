using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public class DSVInterfaceModel
  {
    AppDataModel _dm;

    string _pathLocalDSV;
    DSVImportReader _localReader;


    public DSVInterfaceModel(AppDataModel dm)
    {
      _dm = dm;

      _pathLocalDSV = System.IO.Path.Combine(
        _dm.GetDB().GetDBPathDirectory(),
        _dm.GetDB().GetDBFileName() + ".dsv");

      loadLocal();
    }


    public void UpdateDSVList(IDSVImportReaderFile fileReader)
    {
      // Store File in JSON
      Dictionary<string, string> dic = new Dictionary<string, string>();
      dic["Data"] = (new StreamReader(fileReader.GetStream())).ReadToEnd();
      dic["UsedDSVList"] = fileReader.GetDSVListname();

      using (StreamWriter file = File.CreateText(_pathLocalDSV))
      {
        using (JsonWriter writer = new JsonTextWriter(file))
        {
          JsonSerializer serializer = new JsonSerializer();
          serializer.Formatting = Formatting.Indented;
          serializer.Serialize(writer, dic);
        }
      }

      loadLocal();
    }


    private void loadLocal()
    {
      Dictionary<string, string> dic = new Dictionary<string, string>();
      try
      {
        string configJSON = System.IO.File.ReadAllText(_pathLocalDSV);
        Newtonsoft.Json.JsonConvert.PopulateObject(configJSON, dic);

        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(dic["Data"]);
        writer.Flush();
        stream.Position = 0;

        try
        {
          _localReader = new DSVImportReader(new DSVImportReaderStream(stream, dic["UsedDSVList"]));
        }
        catch (System.IO.IOException)
        {
          _localReader = null;
        }
      }
      catch (Exception e)
      {
        _localReader = null;
      }
    }


    public DataSet Data
    {
      get => _localReader?.Data; 
    }

    public Mapping Mapping
    {
      get => _localReader?.Mapping;
    }

    public string UsedDSVList
    {
      get => _localReader?.UsedDSVList;
    }

    public DateTime? Date
    {
      get => _localReader?.Date;
    }



  }
}
