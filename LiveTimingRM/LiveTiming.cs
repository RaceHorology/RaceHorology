using DSVAlpin2Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    sendParticipants();

    updateStatus("Test 2");
  }


  public void Test1()
  {
    sendStartList(_dm.GetCurrentRaceRun());
  }
  public void Test2()
  {
    sendTiming(_dm.GetCurrentRaceRun());
  }



  protected void login()
  {
    string licensePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    licensePath = Path.Combine(licensePath, "3rdparty");

    _currentLvStruct = _lv.LoginLiveTiming(_bewerbnr, _login, _password, licensePath);

    _lv.StartLiveTiming(ref _currentLvStruct);
  }


  internal void startLiveTiming(Race race)
  {
    _currentLvStruct.VeranstNr = "1";


    _currentLvStruct.Durchgaenge = string.Format("{0}", race.GetMaxRun());

    // "add", "diff"
    _currentLvStruct.TypZeiten = "add";
    if (!string.IsNullOrEmpty(race.RaceConfiguration.RaceResultView))
      if (race.RaceConfiguration.RaceResultView.Contains("BestOfTwo"))
        _currentLvStruct.TypZeiten = "diff"; 

    // "Klasse", "Gruppe", "Kategorie"
    _currentLvStruct.Gruppierung = "Klasse";
    if (!string.IsNullOrEmpty(race.RaceConfiguration.DefaultGrouping))
      if (race.RaceConfiguration.DefaultGrouping.Contains("Class"))
        _currentLvStruct.Gruppierung = "Klasse";
      else if (race.RaceConfiguration.DefaultGrouping.Contains("Group"))
        _currentLvStruct.Gruppierung = "Gruppe";
      else if (race.RaceConfiguration.DefaultGrouping.Contains("Sex"))
        _currentLvStruct.Gruppierung = "Kategorie";

    _lv.StartLiveTiming(ref _currentLvStruct);
  }


  protected void updateStatus(string status)
  {
    _currentLvStruct.InfoText = status;

    _lv.StartLiveTiming(ref _currentLvStruct);
  }



  protected void sendClassesAndGroups()
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


  internal string getClasses()
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


  internal string getGroups()
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


  internal string getCategories()
  {
    return "Kategorie|M|M|1\nKategorie|W|W|2";
  }


  protected void sendParticipants()
  {
    // Id - Kategorie | Id - Gruppe | Id - Klasse | Id - Teilnehmer | Start - Nr | Code | Name | Jahrgang | Verband | Verein | Punkte

    // Id - Kategorie Id der Kategorie(muss in Datei mit Klasseneinteilungen vorhanden sein)
    // Id - Gruppe Id der Gruppe(muss in Datei mit Klasseneinteilungen vorhanden sein)
    // Id - Klasse Id der Klasse(muss in Datei mit Klasseneinteilungen vorhanden sein)
    // Id - Teilnehmer Id des Teilnehmers(wird in Datei mit Zeiten verwendet)
    // Start - Nr Start - Nr des Teilnehmers
    // Code leer/ Code / DSV - Id des Teilnehmers
    // Name Name des Teilnehmers(NACHANME Vorname)
    // Jahrgang Jahrgang des Teilnehmers(4 - stellig) 
    // Verband leer/ Verband / Nation des Teilnehmers
    // Verein Verein des Teilnehmers
    // Punkte leer/ Punkte des Teilnehmers(mit Komma und 2 Nachkommastellen) 

    string data = "";

    data = getParticipantsData();


    _lv.SendTeilnehmer(ref _currentLvStruct, data);
  }


  internal string getParticipantsData()
  {
    string result = "";

    var participants = _dm.GetCurrentRace().GetParticipants();
    foreach(var participant in participants)
    {
      string item;
      item = getParticpantData(participant);

      if (!string.IsNullOrEmpty(result))
        result += "\n";

      result += item;
    }

    return result;
  }


  internal string getParticpantData(RaceParticipant particpant)
  {
    // Id - Kategorie | Id - Gruppe | Id - Klasse | Id - Teilnehmer | Start - Nr | Code | Name | Jahrgang | Verband | Verein | Punkte

    // Id - Kategorie Id der Kategorie(muss in Datei mit Klasseneinteilungen vorhanden sein)
    // Id - Gruppe Id der Gruppe(muss in Datei mit Klasseneinteilungen vorhanden sein)
    // Id - Klasse Id der Klasse(muss in Datei mit Klasseneinteilungen vorhanden sein)
    // Id - Teilnehmer Id des Teilnehmers(wird in Datei mit Zeiten verwendet)
    // Start - Nr Start - Nr des Teilnehmers
    // Code leer/ Code / DSV - Id des Teilnehmers
    // Name Name des Teilnehmers(NACHANME Vorname)
    // Jahrgang Jahrgang des Teilnehmers(4 - stellig) 
    // Verband leer/ Verband / Nation des Teilnehmers
    // Verein Verein des Teilnehmers
    // Punkte leer/ Punkte des Teilnehmers(mit Komma und 2 Nachkommastellen) 
    string item;

    item = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}"
      , particpant.Sex
      , particpant.Class.Group.Id
      , particpant.Class.Id
      , particpant.Participant.Id
      , particpant.StartNumber
      , particpant.Participant.CodeOrSvId // TODO: set to empty if not used
      , particpant.Participant.Fullname
      , particpant.Year
      , particpant.Nation   // TODO: set to empty if not used
      , particpant.Club
      , particpant.Points); // TODO: set to empty if not used

    return item;
  }


  protected void sendStartList(RaceRun raceRun)
  {
    //iii
    //iii Id des Teilnehmers(muss in Datei mit Teilnehmerdaten vorhanden sein)

    string data = "";

    data = getStartListData(raceRun);

    string dg = string.Format("{0}", raceRun.Run);

    _lv.SendStartliste(ref _currentLvStruct, dg, data);
  }


  internal string getStartListData(RaceRun raceRun)
  {
    string result = "";

    StartListViewProvider slp = raceRun.GetStartListProvider();
    var startList = slp.GetViewList();

    foreach (var sle in startList)
    {
      string item;

      item = string.Format("{0,3}", sle.Participant.Id);
      
      if (!string.IsNullOrEmpty(result))
        result += "\n";

      result += item;
    }

    return result;
  }


  protected void sendTiming(RaceRun raceRun)
  {
    // iiiehhmmss,zh
    // iii Id des Teilnehmers(muss in Datei mit Teilnehmerdaten vorhanden sein)
    // e ErgCode: 0 = Läufer im Ziel Laufzeit = hhmmss,zh
    //            1 = Nicht am Start Laufzeit = 999999,99
    //            2 = Nicht im Ziel Laufzeit = 999999,99
    //            3 = Disqualifiziert Laufzeit = 999999,99
    //            4 = Nicht qualifiziert Laufzeit = 999999,99
    //            9 = Läufer auf der Strecke Laufzeit = 000000,01
    // hhmmss,zh Laufzeit

    string data = "";

    data = getTimingData(raceRun);

    string dg = string.Format("{0}", raceRun.Run);

    _lv.SendZeiten(ref _currentLvStruct, dg, data);
  }


  internal string getTimingData(RaceRun raceRun)
  {
    // iiiehhmmss,zh
    // iii Id des Teilnehmers(muss in Datei mit Teilnehmerdaten vorhanden sein)
    // e ErgCode: 0 = Läufer im Ziel Laufzeit = hhmmss,zh
    //            1 = Nicht am Start Laufzeit = 999999,99
    //            2 = Nicht im Ziel Laufzeit = 999999,99
    //            3 = Disqualifiziert Laufzeit = 999999,99
    //            4 = Nicht qualifiziert Laufzeit = 999999,99
    //            9 = Läufer auf der Strecke Laufzeit = 000000,01
    // hhmmss,zh Laufzeit

    string result = "";

    List<RunResult> results = raceRun.GetResultList().OrderBy(o => o.StartNumber).ToList();
    foreach (var r in results)
    {
      string item, time;
      int eCode;

      if (r.ResultCode == RunResult.EResultCode.Normal)
      {
        if (r.Runtime != null)
        {
          time = r.Runtime?.ToString(@"hhmmss\,ff");
          eCode = 0;
        }
        else if (r.GetStartTime() != null)
        {
          time = "000000,01";
          eCode = 9;
        }
        else
          // No useful data => skip
          continue;
      }
      else
      {
        time = "999999,99";
        switch (r.ResultCode)
        {
          case RunResult.EResultCode.NaS: eCode = 1; break;
          case RunResult.EResultCode.NiZ: eCode = 2; break;
          case RunResult.EResultCode.DIS: eCode = 3; break;
          case RunResult.EResultCode.NQ:  eCode = 4; break;
          default:
            // No useful data => skip
            continue;
        }
      }

      item = string.Format("{0,3}{1,1}{2}", r.Participant.Id, eCode, time);

      if (!string.IsNullOrEmpty(result))
        result += "\n";

      result += item;
    }

    return result;
  }
 
}
