/*
 *  Copyright (C) 2019 - 2020 by Sven Flossmann
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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RaceHorologyLib
{
  public class DSVExport
  {
    protected XmlWriterSettings _xmlSettings;
    protected XmlWriter _writer;

    public DSVExport()
    {
      _xmlSettings = new XmlWriterSettings();
      _xmlSettings.Indent = true;
      _xmlSettings.IndentChars = "  ";
      _xmlSettings.Encoding = Encoding.UTF8;
      //_xmlSettings.NewLineOnAttributes = true;
    }


    public void Export(string pathZIPFile, Race race)
    {
      // Helper function to add a logo
      void addImageToZip(ZipArchive archive, string imageSrcName, string imageName, Race raceInternal)
      {
        PDFHelper pdfHelper = new PDFHelper(raceInternal.GetDataModel());
        string imgSrcPath = pdfHelper.FindImage(imageSrcName);
        if (imgSrcPath != null)
        {
          // Storing Image as BMP within ZIP
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

      // Create XML file
      MemoryStream xmlData = new MemoryStream();
      ExportXML(xmlData, race);

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

        // Write ZIP to file
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


    protected void writeRace(Race race)
    {
      _writer.WriteStartElement("dsv_alpine_raceresults");
      writeRaceDescription(race);
      writeRaceData(race);
      writeRaceResults(race);
      _writer.WriteEndElement();
    }

    protected void writeRaceDescription(Race race)
    {
      _writer.WriteStartElement("racedescription");

      _writer.WriteStartElement("racedate");
      _writer.WriteValue(race.DateResultList?.ToString("yyyy-MM-dd"));
      _writer.WriteEndElement();

      _writer.WriteStartElement("gender");
      _writer.WriteValue("A"); // TODO: get real gender
      _writer.WriteEndElement();

      _writer.WriteStartElement("season");
      _writer.WriteValue(race.DateResultList?.AddMonths(2).ToString("yyyy"));
      _writer.WriteEndElement();

      _writer.WriteStartElement("raceid");
      _writer.WriteValue(race.RaceNumber);
      _writer.WriteEndElement();

      _writer.WriteStartElement("raceorganizer");
      _writer.WriteValue(race.AdditionalProperties.Organizer);
      _writer.WriteEndElement();

      _writer.WriteStartElement("discipline");
      _writer.WriteValue(getDSVDisciplin(race));
      _writer.WriteEndElement();

      _writer.WriteStartElement("category");
      _writer.WriteValue(getDSVCategory(race));
      _writer.WriteEndElement();

      _writer.WriteStartElement("racename");
      _writer.WriteValue(race.Description);
      _writer.WriteEndElement();

      _writer.WriteStartElement("raceplace");
      _writer.WriteValue(race.AdditionalProperties.Location);
      _writer.WriteEndElement();

      _writer.WriteStartElement("timing");
      _writer.WriteValue("Alge TdC8001"); // TODO: make variable
      _writer.WriteEndElement();

      if (!string.IsNullOrEmpty(race.AdditionalProperties.Analyzer))
      {
        _writer.WriteStartElement("dataprocessing_by");
        _writer.WriteValue(race.AdditionalProperties.Analyzer);
        _writer.WriteEndElement();
      }

      if (getSoftware() != null)
      {
        _writer.WriteStartElement("software");
        _writer.WriteValue(getSoftware());
        _writer.WriteEndElement();
      }

      _writer.WriteEndElement();

    }

    string getDSVDisciplin(Race race)
    {
      if (race.RaceType == Race.ERaceType.DownHill)
        return "DH";
      if (race.RaceType == Race.ERaceType.GiantSlalom)
        return "RS";
      if (race.RaceType == Race.ERaceType.ParallelSlalom)
        return "PSL";
      if (race.RaceType == Race.ERaceType.Slalom)
        return "SL";
      if (race.RaceType == Race.ERaceType.SuperG)
        return "SG";

      if (race.RaceType == Race.ERaceType.KOSlalom)
        return "unknown";

      return "unknown";
    }

    string getDSVCategory(Race race)
    {
      return "unknown";
    }


    string getSoftware()
    {
      Assembly assembly = Assembly.GetEntryAssembly();
      if (assembly == null)
        assembly = Assembly.GetExecutingAssembly();

      if (assembly != null)
      {
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        var productName = fvi.ProductName;
        var productVersion = fvi.ProductVersion;

        return string.Format("{0} {1}", productName, productVersion);
      }

      return null;
    }

    protected void writeRaceData(Race race)
    {
      _writer.WriteStartElement("racedata");

      _writer.WriteStartElement("useddsvlist");
      _writer.WriteValue("unknown"); // TODO: needs to be fixed
      _writer.WriteEndElement();

      _writer.WriteStartElement("fvalue");
      _writer.WriteValue(race.RaceConfiguration.ValueF);
      _writer.WriteEndElement();

      if (false) // TODO: needs to be fixed, btw: optional
      {
        DSVRaceCalculation dsvCalcW = new DSVRaceCalculation(race, race.GetResultViewProvider(), "W");
        // women
        _writer.WriteStartElement("racepenalty");
        _writer.WriteAttributeString("gender", "L");

        _writer.WriteStartElement("applied_penalty");
        _writer.WriteValue(dsvCalcW.AppliedPenalty);
        _writer.WriteEndElement();

        _writer.WriteStartElement("calculated_penalty");
        _writer.WriteValue(dsvCalcW.CalculatedPenalty);
        _writer.WriteEndElement();

        _writer.WriteEndElement();

        // men
        DSVRaceCalculation dsvCalcM = new DSVRaceCalculation(race, race.GetResultViewProvider(), "M");
        _writer.WriteStartElement("racepenalty");
        _writer.WriteAttributeString("gender", "M");

        _writer.WriteStartElement("applied_penalty");
        _writer.WriteValue(dsvCalcM.AppliedPenalty);
        _writer.WriteEndElement();

        _writer.WriteStartElement("calculated_penalty");
        _writer.WriteValue(dsvCalcM.CalculatedPenalty);
        _writer.WriteEndElement();

        _writer.WriteEndElement();
      }

      writeJuryPerson(_writer, "ChiefRace", race.AdditionalProperties.RaceManager);
      writeJuryPerson(_writer, "Referee", race.AdditionalProperties.RaceReferee);
      writeJuryPerson(_writer, "RepresentativeTrainer", race.AdditionalProperties.TrainerRepresentative);

      writeRunData(race.GetRun(0), race.AdditionalProperties.RaceRun1);
      if (race.GetMaxRun() > 1)
        writeRunData(race.GetRun(1), race.AdditionalProperties.RaceRun2);

      _writer.WriteEndElement();
    }


    static void writeJuryPerson(XmlWriter writer, string function, AdditionalRaceProperties.Person person)
    {
      writer.WriteStartElement("racejury");
      writer.WriteAttributeString("function", function);

      writePersonData(writer, person);

      writer.WriteEndElement();
    }

    static void writeCourseSetterPerson(XmlWriter writer, AdditionalRaceProperties.Person person)
    {
      writer.WriteStartElement("coursesetter");
      writePersonData(writer, person);
      writer.WriteEndElement();
    }

    static void writeForeRunnerPerson(XmlWriter writer, int frNo, AdditionalRaceProperties.Person person)
    {
      writer.WriteStartElement("forerunner");
      writer.WriteAttributeString("order", frNo.ToString());

      writePersonData(writer, person);

      writer.WriteEndElement();
    }

    static void writePersonData(XmlWriter writer, AdditionalRaceProperties.Person person)
    {
      writer.WriteStartElement("lastname");
      writer.WriteValue(person.Name);
      writer.WriteEndElement();

      writer.WriteStartElement("club");
      writer.WriteValue(person.Club);
      writer.WriteEndElement();
    }


    void writeRunData(RaceRun raceRun, AdditionalRaceProperties.RaceRunProperties raceRunProperties)
    {
      _writer.WriteStartElement("rundata");
      _writer.WriteAttributeString("runnumber", raceRun.Run.ToString());

      {
        _writer.WriteStartElement("coursedata");

        _writer.WriteStartElement("coursename");
        _writer.WriteValue(raceRun.GetRace().AdditionalProperties.CoarseName);
        _writer.WriteEndElement();

        if (!string.IsNullOrEmpty(raceRun.GetRace().AdditionalProperties.CoarseHomologNo))
        {
          _writer.WriteStartElement("homologationnumber");
          _writer.WriteValue(raceRun.GetRace().AdditionalProperties.CoarseHomologNo);
          _writer.WriteEndElement();
        }

        _writer.WriteStartElement("number_of_gates");
        _writer.WriteValue(raceRunProperties.Gates.ToString());
        _writer.WriteEndElement();

        _writer.WriteStartElement("number_of_turninggates");
        _writer.WriteValue(raceRunProperties.Turns.ToString());
        _writer.WriteEndElement();

        _writer.WriteStartElement("startaltitude");
        _writer.WriteValue(raceRun.GetRace().AdditionalProperties.StartHeight.ToString());
        _writer.WriteEndElement();

        _writer.WriteStartElement("finishaltitude");
        _writer.WriteValue(raceRun.GetRace().AdditionalProperties.StartHeight.ToString());
        _writer.WriteEndElement();

        _writer.WriteStartElement("courselength");
        _writer.WriteValue(raceRun.GetRace().AdditionalProperties.CoarseLength.ToString());
        _writer.WriteEndElement();

        writeCourseSetterPerson(_writer, raceRunProperties.CoarseSetter);

        if (!string.IsNullOrWhiteSpace(raceRunProperties.Forerunner1.Name))
          writeForeRunnerPerson(_writer, 1, raceRunProperties.Forerunner1);
        if (!string.IsNullOrWhiteSpace(raceRunProperties.Forerunner2.Name))
          writeForeRunnerPerson(_writer, 2, raceRunProperties.Forerunner2);
        if (!string.IsNullOrWhiteSpace(raceRunProperties.Forerunner3.Name))
          writeForeRunnerPerson(_writer, 3, raceRunProperties.Forerunner3);

        _writer.WriteEndElement();
      }

      {
        _writer.WriteStartElement("meteodata");

        if (string.IsNullOrEmpty(raceRun.GetRace().AdditionalProperties.Weather))
        {
          _writer.WriteStartElement("weather");
          _writer.WriteValue(raceRun.GetRace().AdditionalProperties.Weather);
          _writer.WriteEndElement();
        }
        if (string.IsNullOrEmpty(raceRun.GetRace().AdditionalProperties.Snow))
        {
          _writer.WriteStartElement("snowtexture");
          _writer.WriteValue(raceRun.GetRace().AdditionalProperties.Snow);
          _writer.WriteEndElement();
        }
        if (string.IsNullOrEmpty(raceRun.GetRace().AdditionalProperties.TempStart))
        {
          _writer.WriteStartElement("temperature_startaltitude");
          _writer.WriteValue(raceRun.GetRace().AdditionalProperties.TempStart);
          _writer.WriteEndElement();
        }
        if (string.IsNullOrEmpty(raceRun.GetRace().AdditionalProperties.TempFinish))
        {
          _writer.WriteStartElement("temperature_finishaltitude");
          _writer.WriteValue(raceRun.GetRace().AdditionalProperties.TempFinish);
          _writer.WriteEndElement();
        }

        _writer.WriteEndElement();
      }

      {
        _writer.WriteStartElement("starttime");
        _writer.WriteValue(raceRunProperties.StartTime); // TODO: Ensure "HH:MM" 24h
        _writer.WriteEndElement();
      }

      _writer.WriteEndElement();
    }


    void writeRaceResults(Race race)
    {
      // Step 1: split according to classified or not classified
      List<RaceResultItem> classified = new List<RaceResultItem>();
      List<RaceResultItem> notClassified = new List<RaceResultItem>();

      var results = race.GetResultViewProvider().GetView();
      var lr = results as System.Windows.Data.ListCollectionView;

      foreach (var result in results.SourceCollection)
      {
        RaceResultItem item = result as RaceResultItem;

        if (item.ResultCode == RunResult.EResultCode.Normal)
          classified.Add(item);
        else
          notClassified.Add(item);
      }

      _writer.WriteStartElement("raceresults");

      writeClassifiedCompetitors(classified);
      writeNotClassifiedCompetitors(notClassified);

      _writer.WriteEndElement();
    }

    void writeClassifiedCompetitors(List<RaceResultItem> classified)
    {
      _writer.WriteStartElement("classified_competitors");

      foreach (RaceResultItem rri in classified)
      {
        _writer.WriteStartElement("ranked");
        _writer.WriteAttributeString("gender", mapSex(rri.Participant));
        _writer.WriteAttributeString("status", "QLF");
        _writer.WriteAttributeString("rank", rri.Position.ToString());
        _writer.WriteAttributeString("bib", rri.Participant.StartNumber.ToString());

        writeCompetitor(_writer, rri.Participant);
        writeRaceResult(_writer, rri);

        _writer.WriteEndElement();
      }

      _writer.WriteEndElement();
    }

    void writeNotClassifiedCompetitors(List<RaceResultItem> notClassified)
    {
      _writer.WriteStartElement("not_classified_competitiors");

      foreach (RaceResultItem rri in notClassified)
      {
        _writer.WriteStartElement("notranked");
        _writer.WriteAttributeString("gender", mapSex(rri.Participant));
        _writer.WriteAttributeString("bib", rri.Participant.StartNumber.ToString());
        _writer.WriteAttributeString("status", mapResultCode(rri.ResultCode));

        if (!string.IsNullOrWhiteSpace(rri.DisqualText))
        {
          _writer.WriteStartElement("reason");
          _writer.WriteValue(rri.DisqualText);
          _writer.WriteEndElement();
        }

        writeCompetitor(_writer, rri.Participant);

        _writer.WriteStartElement("dsvlistpoints");
        _writer.WriteValue(rri.Participant.Points);
        _writer.WriteEndElement();

        _writer.WriteEndElement();
      }

      _writer.WriteEndElement();
    }


    static void writeCompetitor(XmlWriter writer, RaceParticipant participant)
    {
      writer.WriteStartElement("competitor");

      writer.WriteStartElement("dsvcode");
      writer.WriteValue(participant.Participant.CodeOrSvId);
      writer.WriteEndElement();

      writer.WriteStartElement("year_of_birth");
      writer.WriteValue(participant.Participant.Year);
      writer.WriteEndElement();

      writer.WriteStartElement("gender");
      writer.WriteValue(mapSex(participant));
      writer.WriteEndElement();

      writer.WriteStartElement("lastname");
      writer.WriteValue(participant.Name);
      writer.WriteEndElement();

      writer.WriteStartElement("firstname");
      writer.WriteValue(participant.Firstname);
      writer.WriteEndElement();

      writer.WriteStartElement("club");
      writer.WriteValue(participant.Club);
      writer.WriteEndElement();

      writer.WriteStartElement("association"); // TODO: currently Verband = Nation 
      writer.WriteValue(participant.Nation);
      writer.WriteEndElement();

      writer.WriteEndElement();
    }

    static void writeRaceResult(XmlWriter writer, RaceResultItem rri)
    {
      writer.WriteStartElement("raceresult");

      writer.WriteStartElement("dsvlistpoints");
      writer.WriteValue(rri.Participant.Points);
      writer.WriteEndElement();

      writer.WriteStartElement("totaltime");
      writer.WriteValue(formatTime(rri.TotalTime));
      writer.WriteEndElement();

      writer.WriteStartElement("racepoints");
      writer.WriteValue(rri.Points);
      writer.WriteEndElement();

      foreach (var x in rri.SubResults)
      {
        writer.WriteStartElement("runtime");
        writer.WriteAttributeString("runnumber", x.Key.ToString());

        writer.WriteStartElement("time_of_run");
        writer.WriteValue(formatTime(x.Value.Runtime));
        writer.WriteEndElement();

        writer.WriteEndElement();
      }


      writer.WriteEndElement();
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


    static string mapSex(RaceParticipant particpant)
    {
      switch (particpant.Sex)
      {
        case "M":
        case "m":
        case "H":
        case "h":
          return "M";
        case "W":
        case "w":
        case "D":
        case "d":
          return "L";
      }

      return "U";
    }


    static string formatTime(TimeSpan? time)
    {
      return time?.ToString(@"mm\:ss\.ff");
    }
  }
}
