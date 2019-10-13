using DSVAlpin2Lib;
using System;
using System.IO;

public class LiveTimingRM
{
  private AppDataModel _dm;
  private string _bewerbnr;
  private string _login;
  private string _password;

  rmlt.LiveTiming _lv;
  rmlt.LiveTiming.rmltStruct _currentLvStruct;

  public LiveTimingRM(AppDataModel dm, string bewerbnr, string login, string password)
  {
    _dm = dm;
    _bewerbnr = bewerbnr;
    _login = login;
    _password = password;
  }


  public void Init()
  {
    _lv = new rmlt.LiveTiming();

    login();
    sendClassesAndGroups();

    updateStatus("Test 2");
  }


  private void login()
  {
    string licensePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    licensePath = Path.Combine(licensePath, "3rdparty");

    _currentLvStruct = _lv.LoginLiveTiming(_bewerbnr, _login, _password, licensePath);

    _lv.StartLiveTiming(ref _currentLvStruct);
  }

  private void updateStatus(string status)
  {
    _currentLvStruct.InfoText = status;

    _lv.StartLiveTiming(ref _currentLvStruct);
  }



  private void sendClassesAndGroups()
  {
    //Typ|Id|Bezeichung|sortpos
    // \n
    //Typ Klasse / Gruppe / Kategorie
    //Id Id der Klasse / Gruppe / Kategorie
    //Bezeichnung Bezeichnung der Klasse / Gruppe / Kategorie
    //sortpos Reihenfolge innerhalb der Klasse / Gruppe / Kategorie

    string data = "";

    data += getClasses() + "\n" + getGroups() + "\n" + getCategories();
    
    _lv.SendKlassen(ref _currentLvStruct, data);
  }

  private string getClasses()
  {
    string result = "";

    foreach (var c in _dm.GetParticipantClasses())
    {
      string item;
      item = string.Format("Klasse|{0}|{1}|{2}", c.Id, c.Name, c.SortPos);

      if (!string.IsNullOrEmpty(result))
        result += "\n";

      result += item;
    }

    return result;
  }


  private string getGroups()
  {
    string result = "";

    foreach (var c in _dm.GetParticipantGroups())
    {
      string item;
      item = string.Format("Gruppe|{0}|{1}|{2}", c.Id, c.Name, c.SortPos);

      if (!string.IsNullOrEmpty(result))
        result += "\n";

      result += item;
    }

    return result;
  }


  private string getCategories()
  {
    return "Kategorie|M|M|1\nKategorie|W|W|2";
  }


}
