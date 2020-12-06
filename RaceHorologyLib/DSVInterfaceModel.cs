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
      // Copy data from stream locally
      using (FileStream file = new FileStream(_pathLocalDSV, FileMode.Create, System.IO.FileAccess.Write))
      {
        fileReader.GetStream().CopyTo(file);
        file.Close();
      }

      loadLocal();
    }


    private void loadLocal()
    {
      try
      {
        _localReader = new DSVImportReader(new DSVImportReaderFile(_pathLocalDSV));
      }
      catch(System.IO.IOException)
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
