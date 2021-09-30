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

  public class FISInterfaceModel : IImportListProvider
  {
    AppDataModel _dm;

    string _pathLocal;
    FISImportReader _localReader;
    ParticipantImportUtils _partImportUtils;


    public FISInterfaceModel(AppDataModel dm)
    {
      _dm = dm;

      _pathLocal = System.IO.Path.Combine(
        _dm.GetDB().GetDBPathDirectory(),
        _dm.GetDB().GetDBFileName() + ".fis");

      loadLocal();
    }


    public void UpdateFISList(FISImportReader fileReader)
    {
      System.IO.File.Copy(fileReader.FileName, _pathLocal);

      loadLocal();
    }


    private void loadLocal()
    {
      Dictionary<string, string> dic = new Dictionary<string, string>();
      try
      {
        try
        {
          _localReader = new FISImportReader(_pathLocal);
        }
        catch (System.IO.IOException)
        {
          _localReader = null;
        }
      }
      catch (Exception)
      {
        _localReader = null;
      }

      if (_localReader != null)
        _partImportUtils = new ParticipantImportUtils(_localReader.Mapping, _dm.GetParticipantCategories(), new ClassAssignment(_dm.GetParticipantClasses()));
      else
        _partImportUtils = null;

      var handler = DataChanged;
      handler?.Invoke(this, new EventArgs());
    }


    public bool ContainsParticipant(Participant p)
    {
      if (_localReader?.Data == null || _localReader?.Data.Tables.Count == 0)
        return true;

      foreach (DataRow r in _localReader?.Data.Tables[0].Rows)
      {
        if (r["SvId"]?.ToString() == p.CodeOrSvId)
          return _partImportUtils.EqualsParticipant(p, r);
      }

      return false;
    }


    public event DataChangedHandler DataChanged;


    public DataSet Data
    {
      get => _localReader?.Data; 
    }

    public Mapping Mapping
    {
      get => _localReader?.Mapping;
    }

    public string UsedList
    {
      get => "not implemented";
    }

    public DateTime? Date
    {
      get => _localReader?.Date;
    }



  }
}
