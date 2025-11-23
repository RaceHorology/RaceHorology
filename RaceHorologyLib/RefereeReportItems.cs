using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RaceHorologyLib
{
  public class RefereeReportItem
  {
    public string Key { get; set; }
    public string Value { get; set; }
    public string Text { get; set; }
    public bool IsAdditional { get; set; }

    public RefereeReportItem(string key, bool isAdditional, string text, string defaultValue = "")
    {
      Key = key;
      Text = text;
      IsAdditional = isAdditional;

      if (defaultValue != string.Empty)
        Value = defaultValue;
      else
        Value = string.Empty;
    }
  }



  public class RefereeReportItems
  {
    public ObservableCollection<RefereeReportItem> RefReportItemList { get; set; }


    public string SavedText
    {
      get;
      set;
    }

    /// <summary>
    /// Default items with key from DSVAlpinX, a label text and the indicator if this value can be found in 
    /// existing properties
    /// </summary>
    /// 
    public List<RefereeReportItem> defaultItemList = new List<RefereeReportItem>()
    {
      //Info
      new RefereeReportItem("Organisator",           false,     "Organisator"),
      new RefereeReportItem("Landesverband",         false,     "Landesverband"),
      new RefereeReportItem("Datum",                 false,     "Datum"),
      new RefereeReportItem("Veranstaltung",         false,     "Veranstaltung"),
      new RefereeReportItem("Disziplin",             false,     "Disziplin"),
      new RefereeReportItem("RennNr",                false,     "Renn-Nr."),

      //Jury/Kampftrichter
      new RefereeReportItem("Schiedsrichter",        false,     "Schiedsrichter ") ,
      new RefereeReportItem("Schiedsrichter_V",      false,     "Schiedsrichter Verein"),
      new RefereeReportItem("Rennleiter",            false,     "Rennleiter"),
      new RefereeReportItem("Rennleiter_V",          false,     "Rennleiter Verein"),
      new RefereeReportItem("Trainervertreter",      false,     "Trainervertreter"),
      new RefereeReportItem("Trainervertreter_V",    false,     "Trainervertreter Verein"),
      new RefereeReportItem("EDVKR",                 false,     "EDV-KR")  ,
      new RefereeReportItem("EDVKR_V",               true,      "EDV-KR Verein"),
      new RefereeReportItem("EDVKR_Email",           true,      "EDV-KR Email")  ,
      new RefereeReportItem("EDVKR_Telefon",         true,      "EDV-KR Telefon"),
      new RefereeReportItem("Startrichter",          false,     "Startrichter"),
      new RefereeReportItem("Startrichter_V",        false,     "Startrichter Verein"),
      new RefereeReportItem("Zielrichter",           true,      "Zielrichter"),
      new RefereeReportItem("Zielrichter_V",         true,      "Zielrichter Verein"),

      //Organisation
      new RefereeReportItem("Org_Auslosung",         true,      "Auslosung", "Per Race Horology"),
      new RefereeReportItem("Org_Siegerehrung",      true,      "Siegerehrung", "xxx Min. nach Rennende"),
      new RefereeReportItem("Org_MedLeiter",         true,      "Med. Leiter"),
      new RefereeReportItem("Org_Torrichter",        false,     "Torrichter"),

      //Zeitmessung Auswertung
      new RefereeReportItem("Punkteberechnung",      true,      "DSV-Punkteberechnung", "Nein") ,
      new RefereeReportItem("StartersterLaeufer",    false,     "Start 1. Läufer"),
      new RefereeReportItem("Zeitmessgeraet",        false,     "Zeitmessgeraet"),
      new RefereeReportItem("Einschaltzeit",         false,     "Einschaltzeit"),
      new RefereeReportItem("Synchronzeit",          true,      "Synchronzeit"),
      new RefereeReportItem("Kabelverbindung",       true,      "Kabel"),
      new RefereeReportItem("Funkverbindung",        true,      "Funk"),
      //new RefereeReportItem("ProblemeZeitmessung",   true,      "Probleme Zeitmessung", "Nein"),
      new RefereeReportItem("Anz_Teilnehmer",        false,     "Teilnehmer"),
      new RefereeReportItem("Anz_NichtamStartDG1",   false,     "Nicht am Start"),
      new RefereeReportItem("Anz_Klassifizierte",    false,     "Klassifizierte"),
      new RefereeReportItem("SyncZeit1Min",          true,      "Sync-Zeit nach 1. Min."),
      new RefereeReportItem("Zeitmessstreifen",      true,      "Zeitmessstreifen", "Ja"),

      //Rennstrecke
      new RefereeReportItem("Rennstrecke",           false,     "Ort und Name der Rennstrecke")    ,
      new RefereeReportItem("homologiert",           false,     "FIS homologiert"),
      new RefereeReportItem("Streckenzustand",       true,      "Vorbereitung und Schneeverhältnisse"),
      new RefereeReportItem("DG1_Kurssetzer",        false,     "Kurssetzer"),
      new RefereeReportItem("DG2_Kurssetzer",        false,     "Kurssetzer"),
      new RefereeReportItem("Bem_Kurssetzer",        true,      "Bem. Kurssetzer ", "Keine"),
      new RefereeReportItem("DG1_Streckenlaenge",    false,     "Streckenlänge "),
      new RefereeReportItem("DG2_Streckenlaenge",    false,     "Streckenlänge "),
      new RefereeReportItem("Bem_Streckenlaenge",    true,      "Bem. Streckenlänge ", "Keine"),
      new RefereeReportItem("DG1_Hoehendifferenz",   false,     "Höhendifferenz "),
      new RefereeReportItem("DG2_Hoehendifferenz",   false,     "Höhendifferenz "),
      new RefereeReportItem("Bem_Hoehendifferenz",   true,      "Bem. Höhendifferenz ", "Keine"),
      new RefereeReportItem("DG1_Tore",              false,     "# Tore / Richtg. Änderung"),
      new RefereeReportItem("DG1_Richtaend",         false,     "# Tore / Richtg. Änderung "),
      new RefereeReportItem("DG2_Tore",              false,     "# Tore / Richtg. Änderung "),
      new RefereeReportItem("DG2_Richtaend",         false,     "# Tore / Richtg. Änderung"),
      new RefereeReportItem("Bem_Tore",              true,      "Bem. Tore ", "Keine"),
      new RefereeReportItem("DG1_Bestzeit",          true,      "Bestzeit DG1"),
      new RefereeReportItem("DG2_Bestzeit",          true,      "Bestzeit DG2"),
      new RefereeReportItem("Bem_Bestzeit",          true,      "Bem. Bestzeit", "Keine"),

      //Sicherheit
      new RefereeReportItem("StreckeGefahren",       true,      "Spezielle Gefahren der Strecke", "Keine"),
      new RefereeReportItem("Stangen",               true,      "Verwendete Stangen und Torflaggen"),
      new RefereeReportItem("Streckenverbesserung",  true,      "Streckenverbesserung durch die Jury?", "Keine"),
      new RefereeReportItem("Rettungsdienst",        true,      "War der Rettungsdienst ausreichend?", "Ja"),
      new RefereeReportItem("Unfaelle",              true,      "Gab es Unfälle während des Rennens?\r\n(Zusatzbericht erforderlich)"),

      //Rennabwicklung           
      new RefereeReportItem("Witterung",             false,     "Witterungs- und Sichtverhältnisse"),
      new RefereeReportItem("Proteste",              true,      "Wurden Proteste eingereicht?", "Nein"),
      new RefereeReportItem("Sanktionen",            true,      "Sanktionen gegen Athleten?", "Nein") ,
      //new RefereeReportItem("Unterstuetzung",        true,      "Unterstützung der Jury durch Organisator?", "Gut"), 
      //new RefereeReportItem("Bemerkungen",           true,     "Bemerkungen\r\nSonstiges", "Keine"),

      //Aussteller
      new RefereeReportItem("Aussteller_Name",       false,     "Name"),
      new RefereeReportItem("Aussteller_Telefon",    true,      "Tel."),
      new RefereeReportItem("Aussteller_Email",      true,      "E-Mail"),
      //new RefereeReportItem("Aussteller_KrNr",       true,      "KR-Nr."),

        };

    public RefereeReportItems(Race race)
    {
      RefReportItemList = new ObservableCollection<RefereeReportItem>(defaultItemList);

      Dictionary<string, string> d = race.GetDataModel().GetDB().GetRefereeReportData(race);

      foreach (RefereeReportItem rritem in RefReportItemList)
      {
        if (d.ContainsKey(rritem.Key))
        {
          rritem.Value = d[rritem.Key];
        }
        else
        {
          race.GetDataModel().GetDB().CreateOrUpdateReferreReportItem(rritem, race, false);
        }
      }

    }

    /// <summary>
    /// Returns string for racetype 
    /// Optional move in Raca class?
    /// </summary>
    /// <param name="race"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private string getDisciplin(Race race)
    {
      switch (race.RaceType)
      {
        case Race.ERaceType.DownHill: return "DH";
        case Race.ERaceType.GiantSlalom: return "GS";
        case Race.ERaceType.Slalom: return "SL";
        case Race.ERaceType.SuperG: return "SG";
        case Race.ERaceType.ParallelSlalom: return "PS";
        case Race.ERaceType.KOSlalom: return "KO";

        default:
          throw new Exception(string.Format("???", race.RaceType));
      }
    }

    public void updateList(Race r)
    {


      UpdateItemValue("Organisator", r.AdditionalProperties.Organizer);
      UpdateItemValue("Veranstaltung", r.AdditionalProperties.Description);
      UpdateItemValue("RennNr", r.AdditionalProperties.RaceNumber);
      UpdateItemValue("Schiedsrichter", r.AdditionalProperties.RaceReferee.Name);
      UpdateItemValue("Schiedsrichter_V", r.AdditionalProperties.RaceReferee.Club);
      UpdateItemValue("Aussteller_Name", r.AdditionalProperties.RaceReferee.Name);

      UpdateItemValue("Rennleiter", r.AdditionalProperties.RaceManager.Name);
      UpdateItemValue("Rennleiter_V", r.AdditionalProperties.RaceManager.Club);
      UpdateItemValue("Trainervertreter", r.AdditionalProperties.TrainerRepresentative.Name);
      UpdateItemValue("Trainervertreter_V", r.AdditionalProperties.TrainerRepresentative.Club);

      UpdateItemValue("EDVKR", r.AdditionalProperties.Analyzer);

      UpdateItemValue("Disziplin", getDisciplin(r));

      UpdateItemValue("Zeitmessgeraet", r.TimingDevice);

      if (r.AdditionalProperties.StartHeight > 0 && r.AdditionalProperties.FinishHeight > 0)
      {
        UpdateItemValue("DG1_Hoehendifferenz", (r.AdditionalProperties.StartHeight - r.AdditionalProperties.FinishHeight).ToString());
        UpdateItemValue("DG2_Hoehendifferenz", (r.AdditionalProperties.StartHeight - r.AdditionalProperties.FinishHeight).ToString());
      }

      UpdateItemValue("DG1_Kurssetzer", r.AdditionalProperties.RaceRun1.CoarseSetter.Name);
      UpdateItemValue("DG1_Richtaend", r.AdditionalProperties.RaceRun1.Turns.ToString());
      UpdateItemValue("DG1_Streckenlaenge", r.AdditionalProperties.CoarseLength.ToString());
      UpdateItemValue("DG1_Tore", r.AdditionalProperties.RaceRun1.Gates.ToString());

      UpdateItemValue("DG2_Kurssetzer", r.AdditionalProperties.RaceRun2.CoarseSetter.Name);
      UpdateItemValue("DG2_Richtaend", r.AdditionalProperties.RaceRun2.Turns.ToString());
      UpdateItemValue("DG2_Streckenlaenge", r.AdditionalProperties.CoarseLength.ToString());
      UpdateItemValue("DG2_Tore", r.AdditionalProperties.RaceRun2.Gates.ToString());


      UpdateItemValue("Rennstrecke", r.AdditionalProperties.CoarseName);

      if (r.AdditionalProperties.CoarseHomologNo != string.Empty)
      {
        UpdateItemValue("homologiert", r.AdditionalProperties.CoarseHomologNo);
      }
      else
      {
        UpdateItemValue("homologiert", "Nein");
      }

      UpdateItemValue("Witterung", r.AdditionalProperties.Weather);

      //Or from timing list?
      UpdateItemValue("StartersterLaeufer", r.AdditionalProperties.RaceRun1.StartTime);


      int participants = r.GetParticipants().Count();
      int particpantsClassified = 0;

      var endresult = r.GetResultViewProvider().GetView();
      foreach (var o in endresult)
      {
        if (o is RaceResultItem res)
        {
          if (res.Position > 0)
            particpantsClassified++;
        }
      }

      UpdateItemValue("Anz_Teilnehmer", participants.ToString());
      UpdateItemValue("Anz_Klassifizierte", particpantsClassified.ToString());

      RaceRun rr = r.GetRun(0);
      var runRes = rr.GetResultList().Where(res => res.ResultCode == RunResult.EResultCode.NaS);

      UpdateItemValue("Anz_NichtamStartDG1", runRes.Count().ToString());

      foreach (RefereeReportItem item in RefReportItemList)
      {
        r.GetDataModel().GetDB().CreateOrUpdateReferreReportItem(item, r, true);
      }

      SavedText = "Angaben gespeichert";
    }

    public void UpdateItemValue(string key, string newValue)
    {
      var item = RefReportItemList.FirstOrDefault(i => i.Key == key);
      if (item != null)
      {
        item.Value = newValue;
      }
    }
  }
}
