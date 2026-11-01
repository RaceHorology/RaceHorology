/*
 *  Copyright (C) 2019 - 2026 by Sven Flossmann & Co-Authors (CREDITS.TXT)
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace RaceHorologyLib
{

  public class FISExportException : Exception
  {
    public FISExportException(string message) : base(message)
    { }

    public string GetHumanReadableError()
    {
      switch (Message)
      {
        case "missing racedate":
          return "Renndatum fehlt";
        case "missing raceid":
          return "Rennnummer fehlt";
        case "missing raceorganizer":
          return "Rennorganisator fehlt";
        case "missing racename":
          return "Rennname und/oder Beschreibung fehlt";
        case "missing raceplace":
          return "Rennort fehlt";
        case "not supported racetype":
          return "Renntyp nicht unterstützt";
        case "missing racejury Chiefrace":
          return "Rennleiter fehlt";
        case "missing racejury Referee":
          return "Schiedsrichter fehlt";
        case "missing racejury TechnicalDelegate":
          return "Trainervertreter/TD fehlt";
        case "missing racejury Assistantreferee":
          return "Schiedsrichterassistent fehlt";
        case "missing coursesetter":
          return "Kurssetzer fehlt";
        case "missing coarsename":
          return "Kursname fehlt";
        case "missing number_of_gates":
          return "Anzahl der Tore fehlt";
        case "missing number_of_turninggates":
          return "Anzahl der Richtungsänderungen fehlt";
        case "missing startaltitude":
          return "Starthöhe fehlt";
        case "missing finishaltitude":
          return "Zielhöhe fehlt";
        case "missing courselength":
          return "Kurslänge fehlt";
        case "missing forerunner":
          return "Vorläufer fehlt";
        case "missing f-value":
          return "F-Wert nicht korrekt";
        default:
          return Message;
      }
    }
  }


  public class FISExport
  {
    protected XmlWriterSettings _xmlSettings;
    protected XmlWriter _writer;

    public FISExport()
    {
      _xmlSettings = new XmlWriterSettings();
      _xmlSettings.Indent = true;
      _xmlSettings.IndentChars = "  ";
      _xmlSettings.Encoding = Encoding.UTF8;
    }


    public void Export(string pathZIPFile, Race race)
    {
      void addImageToZip(ZipArchive archive, string imageSrcName, string imageName, Race raceInternal)
      {
        PDFHelper pdfHelper = new PDFHelper(raceInternal.GetDataModel());
        string imgSrcPath = pdfHelper.FindImage(imageSrcName);
        if (imgSrcPath != null)
        {
          var imgZipFile = archive.CreateEntry(imageName + ".jpg");
          using (var imgZipStream = imgZipFile.Open())
          {
            Image img = Image.FromFile(imgSrcPath);
            img.Save(imgZipStream, ImageFormat.Jpeg);
            imgZipStream.Close();
          }
        }
      }

      string baseFileName = Path.GetFileNameWithoutExtension(pathZIPFile);

      using (var zipStream = new MemoryStream())
      {
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
          var xmlFile = archive.CreateEntry(baseFileName + ".xml");
          using (var xmlStream = xmlFile.Open())
          {
            ExportXML(xmlStream, race);
          }

          addImageToZip(archive, "Logo1", "LogoClub", race);
        }

        using (var fileStream = new FileStream(pathZIPFile, FileMode.Create))
        {
          zipStream.Seek(0, SeekOrigin.Begin);
          zipStream.CopyTo(fileStream);
        }
      }
    }

    public void ExportXML(string pathXMLFile, Race race)
    {
      FileStream output = new FileStream(pathXMLFile, FileMode.Create);
      ExportXML(output, race);
    }


    public void ExportXML(Stream output, Race race)
    {
      _writer = XmlWriter.Create(output, _xmlSettings);

      _writer.WriteStartDocument();
      writeRace(race);
      _writer.WriteEndDocument();

      _writer.Close();
    }


    void writeRace(Race race)
    {
      _writer.WriteStartElement("Fisresults");
      writeRaceheader(race);
      _writer.WriteStartElement("AL_race");
      writeALRaceinfo(race);
      writeRaceResults(race);
      _writer.WriteEndElement(); // AL_race
      _writer.WriteEndElement(); // Fisresults
    }


    void writeRaceheader(Race race)
    {
      _writer.WriteStartElement("Raceheader");
      _writer.WriteAttributeString("Sector", "AL");
      _writer.WriteAttributeString("Gender", determineGender(race));

      writeElement("Season", race.DateResultList?.AddMonths(3).ToString("yyyy"));

      if (string.IsNullOrWhiteSpace(race.RaceNumber))
        throw new FISExportException("missing raceid");
      writeElement("Codex", race.RaceNumber);

      if (!string.IsNullOrWhiteSpace(race.AdditionalProperties?.RaceNation))
        writeElement("Nation", race.AdditionalProperties.RaceNation);

      writeElement("Discipline", getFISDiscipline(race));

      if (!string.IsNullOrWhiteSpace(race.AdditionalProperties?.FISCategory))
        writeElement("Category", race.AdditionalProperties.FISCategory);

      if (race.DateResultList == null)
        throw new FISExportException("missing racedate");
      writeRacedate(race.DateResultList.Value);

      if (string.IsNullOrWhiteSpace(race.AdditionalProperties?.Organizer))
        throw new FISExportException("missing raceorganizer");

      if (string.IsNullOrWhiteSpace(race.AdditionalProperties?.Location))
        throw new FISExportException("missing raceplace");
      writeElement("Place", race.AdditionalProperties?.Location);

      if (string.IsNullOrWhiteSpace(race.Description))
        throw new FISExportException("missing racename");
      writeElement("Eventname", race.Description);

      _writer.WriteEndElement();
    }


    void writeRacedate(DateTime date)
    {
      _writer.WriteStartElement("Racedate");
      writeElement("Day", date.Day.ToString());
      writeElement("Month", date.Month.ToString());
      writeElement("Year", date.Year.ToString());
      _writer.WriteEndElement();
    }


    void writeALRaceinfo(Race race)
    {
      _writer.WriteStartElement("AL_raceinfo");

      FISRaceResultViewProvider fisRaceVP = race.GetResultViewProvider() as FISRaceResultViewProvider;
      if (fisRaceVP != null)
      {
        if (race.RaceConfiguration.ValueF > 0.0)
          writeElement("Fvalue", race.RaceConfiguration.ValueF.ToString("F0", CultureInfo.InvariantCulture));

        FISRaceCalculation fisCalc = fisRaceVP.GetFISRaceCalculation();
        if (fisCalc != null && fisCalc.CalculationValid)
        {
          writeElement("Appliedpenalty", fisCalc.AppliedPenalty.ToString("F2", CultureInfo.InvariantCulture));
          writeElement("Calculatedpenalty", fisCalc.CalculatedPenalty.ToString("F2", CultureInfo.InvariantCulture));
        }
      }

      if (race.AdditionalProperties?.TrainerRepresentative == null || ((AdditionalRaceProperties.Person)race.AdditionalProperties?.TrainerRepresentative).IsEmpty())
        throw new FISExportException("missing racejury TechnicalDelegate");
      writeJuryTD(race.AdditionalProperties?.TrainerRepresentative, race.AdditionalProperties?.TDNumber);

      if (race.AdditionalProperties?.RaceManager == null || ((AdditionalRaceProperties.Person)race.AdditionalProperties?.RaceManager).IsEmpty())
        throw new FISExportException("missing racejury Chiefrace");
      writeJury("Chiefrace", race.AdditionalProperties?.RaceManager);

      if (race.AdditionalProperties?.RaceReferee == null || ((AdditionalRaceProperties.Person)race.AdditionalProperties?.RaceReferee).IsEmpty())
        throw new FISExportException("missing racejury Referee");
      writeJury("Referee", race.AdditionalProperties?.RaceReferee);

      if (race.RaceType == Race.ERaceType.SuperG || race.RaceType == Race.ERaceType.DownHill)
      {
        if (race.AdditionalProperties?.AssistantReferee == null || ((AdditionalRaceProperties.Person)race.AdditionalProperties?.AssistantReferee).IsEmpty())
          throw new FISExportException("missing racejury Assistantreferee");
      }
      if (race.AdditionalProperties?.AssistantReferee != null && !((AdditionalRaceProperties.Person)race.AdditionalProperties?.AssistantReferee).IsEmpty())
        writeJury("Assistantreferee", race.AdditionalProperties?.AssistantReferee);

      writeRuninfo(race.GetRun(0), race.AdditionalProperties?.RaceRun1);
      if (race.GetMaxRun() > 1)
        writeRuninfo(race.GetRun(1), race.AdditionalProperties?.RaceRun2);

      writeElement("Timingby", string.IsNullOrWhiteSpace(race.TimingDevice) ? "Race Horology" : race.TimingDevice);

      if (!string.IsNullOrEmpty(race.AdditionalProperties?.Analyzer.Club) && !string.IsNullOrEmpty(race.AdditionalProperties?.Analyzer.Name))
        writeElement("Dataprocessingby", string.Format("{0}, {1}", race.AdditionalProperties?.Analyzer.Name, race.AdditionalProperties?.Analyzer.Club));
      else if (!string.IsNullOrEmpty(race.AdditionalProperties?.Analyzer.Club))
        writeElement("Dataprocessingby", race.AdditionalProperties?.Analyzer.Club);
      else if (!string.IsNullOrEmpty(race.AdditionalProperties?.Analyzer.Name))
        writeElement("Dataprocessingby", race.AdditionalProperties?.Analyzer.Name);

      writeElement("Softwarename", "RaceHorology");

      _writer.WriteEndElement();
    }


    void writeJury(string function, AdditionalRaceProperties.Person person)
    {
      string lastname, firstname;
      DSVExport.guessLastAndFirstname(person?.Name, out lastname, out firstname);

      _writer.WriteStartElement("Jury");
      _writer.WriteAttributeString("Function", function);
      writeElement("Lastname", lastname);
      writeElement("Firstname", firstname);
      if (!string.IsNullOrEmpty(person?.Club))
        writeElement("Nation", person.Club);
      _writer.WriteEndElement();
    }


    void writeJuryTD(AdditionalRaceProperties.Person person, string tdNumber)
    {
      string lastname, firstname;
      DSVExport.guessLastAndFirstname(person?.Name, out lastname, out firstname);

      _writer.WriteStartElement("Jury");
      _writer.WriteAttributeString("Function", "TechnicalDelegate");
      if (!string.IsNullOrEmpty(tdNumber))
        writeElement("Number", tdNumber);
      writeElement("Lastname", lastname);
      writeElement("Firstname", firstname);
      if (!string.IsNullOrEmpty(person?.Club))
        writeElement("Nation", person.Club);
      _writer.WriteEndElement();
    }


    void writeRuninfo(RaceRun raceRun, AdditionalRaceProperties.RaceRunProperties raceRunProperties)
    {
      _writer.WriteStartElement("Runinfo");
      _writer.WriteAttributeString("no", raceRun.Run.ToString());

      _writer.WriteStartElement("Course");

      if (string.IsNullOrEmpty(raceRun.GetRace().AdditionalProperties?.CoarseName))
        throw new FISExportException("missing coarsename");
      writeElement("Name", raceRun.GetRace().AdditionalProperties?.CoarseName);

      if (!string.IsNullOrEmpty(raceRun.GetRace().AdditionalProperties?.CoarseHomologNo))
        writeElement("Homologation", raceRun.GetRace().AdditionalProperties.CoarseHomologNo);

      if (raceRunProperties.Gates <= 0)
        throw new FISExportException("missing number_of_gates");
      writeElement("Gates", raceRunProperties.Gates.ToString());

      if (raceRunProperties.Turns <= 0)
        throw new FISExportException("missing number_of_turninggates");
      writeElement("Turninggates", raceRunProperties.Turns.ToString());

      if (raceRun.GetRace().AdditionalProperties?.CoarseLength <= 0)
        throw new FISExportException("missing courselength");
      writeElement("Length", raceRun.GetRace().AdditionalProperties.CoarseLength.ToString());

      if (raceRun.GetRace().AdditionalProperties?.StartHeight <= 0)
        throw new FISExportException("missing startaltitude");
      writeElement("Startelev", raceRun.GetRace().AdditionalProperties?.StartHeight.ToString());

      if (raceRun.GetRace().AdditionalProperties?.FinishHeight <= 0)
        throw new FISExportException("missing finishaltitude");
      writeElement("Finishelev", raceRun.GetRace().AdditionalProperties?.FinishHeight.ToString());

      writeCoursesetter(raceRunProperties.CoarseSetter);

      if (!string.IsNullOrWhiteSpace(raceRunProperties.Forerunner1.Name))
        writeForerunner(1, raceRunProperties.Forerunner1);
      else
        throw new FISExportException("missing forerunner");
      if (!string.IsNullOrWhiteSpace(raceRunProperties.Forerunner2.Name))
        writeForerunner(2, raceRunProperties.Forerunner2);
      if (!string.IsNullOrWhiteSpace(raceRunProperties.Forerunner3.Name))
        writeForerunner(3, raceRunProperties.Forerunner3);

      _writer.WriteEndElement(); // Course

      writeElement("Starttime", raceRunProperties.StartTime);

      _writer.WriteEndElement(); // Runinfo
    }


    void writeCoursesetter(AdditionalRaceProperties.Person person)
    {
      if (person == null || person.IsEmpty())
        throw new FISExportException("missing coursesetter");

      string lastname, firstname;
      DSVExport.guessLastAndFirstname(person?.Name, out lastname, out firstname);

      _writer.WriteStartElement("Coursesetter");
      writeElement("Lastname", lastname);
      if (!string.IsNullOrEmpty(firstname))
        writeElement("Firstname", firstname);
      if (!string.IsNullOrEmpty(person?.Club))
        writeElement("Nation", person.Club);
      _writer.WriteEndElement();
    }


    void writeForerunner(int order, AdditionalRaceProperties.Person person)
    {
      string lastname, firstname;
      DSVExport.guessLastAndFirstname(person?.Name, out lastname, out firstname);

      _writer.WriteStartElement("Forerunner");
      _writer.WriteAttributeString("Order", order.ToString());
      writeElement("Lastname", lastname);
      if (!string.IsNullOrEmpty(firstname))
        writeElement("Firstname", firstname);
      if (!string.IsNullOrEmpty(person?.Club))
        writeElement("Nation", person.Club);
      _writer.WriteEndElement();
    }


    void writeRaceResults(Race race)
    {
      List<RaceResultItem> classified = new List<RaceResultItem>();
      List<RaceResultItem> notClassified = new List<RaceResultItem>();

      var results = race.GetResultViewProvider().GetView();

      foreach (var result in results.SourceCollection)
      {
        RaceResultItem item = result as RaceResultItem;

        if (item.ResultCode == RunResult.EResultCode.Normal && item.TotalTime != null)
          classified.Add(item);
        else
          notClassified.Add(item);
      }

      FISRaceCalculation fisCalc = (race.GetResultViewProvider() as FISRaceResultViewProvider)?.GetFISRaceCalculation();
      writeALClassified(classified, fisCalc);
      writeALNotClassified(notClassified);
    }


    void writeALClassified(List<RaceResultItem> classified, FISRaceCalculation fisCalc)
    {
      _writer.WriteStartElement("AL_classified");

      foreach (RaceResultItem rri in classified)
      {
        _writer.WriteStartElement("AL_ranked");
        _writer.WriteAttributeString("Status", "QLF");

        writeElement("Rank", rri.Position.ToString());
        writeElement("Bib", rri.Participant.StartNumber.ToString());
        writeCompetitor(rri.Participant);

        _writer.WriteStartElement("AL_result");
        foreach (var x in rri.SubResults.OrderBy(k => k.Key))
        {
          writeElement("Timerun" + x.Key, formatTime(x.Value.Runtime));
        }
        writeElement("Totaltime", formatTime(rri.TotalTime));

        double racePoints = fisCalc != null ? fisCalc.CalculatePoints(rri, false) : -1.0;
        if (racePoints >= 0)
          writeElement("Racepoints", racePoints.ToString("F2", CultureInfo.InvariantCulture));

        _writer.WriteEndElement(); // AL_result

        _writer.WriteEndElement(); // AL_ranked
      }

      _writer.WriteEndElement();
    }


    void writeALNotClassified(List<RaceResultItem> notClassified)
    {
      _writer.WriteStartElement("AL_notclassified");

      foreach (RaceResultItem rri in notClassified)
      {
        string status;
        uint failedRun;
        mapNotClassifiedStatus(rri, out status, out failedRun);

        _writer.WriteStartElement("AL_notranked");
        _writer.WriteAttributeString("Status", status);

        if (failedRun > 0)
          writeElement("Run", failedRun.ToString());

        writeElement("Bib", rri.Participant.StartNumber.ToString());
        writeCompetitor(rri.Participant);

        if (!string.IsNullOrWhiteSpace(rri.DisqualText))
          writeElement("Reason", rri.DisqualText);

        _writer.WriteEndElement();
      }

      _writer.WriteEndElement();
    }


    void writeCompetitor(RaceParticipant participant)
    {
      _writer.WriteStartElement("Competitor");

      writeElement("Fiscode", participant.Participant.CodeOrSvId);

      writeElement("Lastname", participant.Name);
      writeElement("Firstname", participant.Firstname);

      writeElement("Nation", participant.Nation);

      writeElement("Yearofbirth", participant.Participant.Year.ToString());

      _writer.WriteEndElement();
    }


    void writeElement(string name, string value)
    {
      _writer.WriteStartElement(name);
      _writer.WriteValue(value ?? "");
      _writer.WriteEndElement();
    }


    static string formatTime(TimeSpan? time)
    {
      if (time == null)
        return "";

      if (time.Value.Hours > 0 || time.Value.Minutes >= 1)
        return time.Value.ToString(@"m\:ss\.ff");

      return time.Value.ToString(@"ss\.ff");
    }


    static string getFISDiscipline(Race race)
    {
      if (race.RaceType == Race.ERaceType.DownHill)
        return "DH";
      if (race.RaceType == Race.ERaceType.GiantSlalom)
        return "GS";
      if (race.RaceType == Race.ERaceType.ParallelSlalom)
        return "PSL";
      if (race.RaceType == Race.ERaceType.Slalom)
        return "SL";
      if (race.RaceType == Race.ERaceType.SuperG)
        return "SG";

      throw new FISExportException("not supported racetype");
    }


    static string determineGender(Race race)
    {
      bool hasMale = false;
      bool hasFemale = false;

      foreach (var participant in race.GetParticipants())
      {
        if (participant.Sex == null)
          continue;

        char cat = participant.Sex.Name;
        if (cat == 'M' || cat == 'm')
          hasMale = true;
        else if (cat == 'W' || cat == 'w' || cat == 'L' || cat == 'l')
          hasFemale = true;
      }

      if (hasMale && !hasFemale)
        return "M";
      if (hasFemale && !hasMale)
        return "W";
      return "A";
    }


    static void mapNotClassifiedStatus(RaceResultItem rri, out string status, out uint failedRun)
    {
      status = "UNKNOWN";
      failedRun = 0;

      foreach (KeyValuePair<uint, RaceResultItem.SubResult> kvp in rri.SubResults.OrderBy(k => k.Key))
      {
        if (kvp.Value.RunResultCode != RunResult.EResultCode.Normal)
        {
          status = mapResultCode(kvp.Value.RunResultCode);
          failedRun = kvp.Key;
          break;
        }
      }
    }

    static string mapResultCode(RunResult.EResultCode resultCode)
    {
      switch (resultCode)
      {
        case RunResult.EResultCode.DIS:
          return "DSQ";
        case RunResult.EResultCode.NaS:
          return "DNS";
        case RunResult.EResultCode.NiZ:
          return "DNF";
        case RunResult.EResultCode.NQ:
          return "DNQ";
      }

      return "UNKNOWN";
    }


  }
}
