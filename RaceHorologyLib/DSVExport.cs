using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
      _xmlSettings.NewLineOnAttributes = true;
    }

    public void Export(string pathXMLFile, Race race)
    {
      FileStream output = new FileStream(pathXMLFile, FileMode.Create);
      Export(output, race);
    }


    public void Export(Stream output, Race race)
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
      _writer.WriteEndElement();
    }

    protected void writeRaceDescription(Race race)
    {
      _writer.WriteStartElement("racedescription");

      _writer.WriteStartElement("racedate");
      _writer.WriteValue(race.DateResult.ToString("yyyy-MM-dd"));
      _writer.WriteEndElement();

      _writer.WriteStartElement("gender");
      _writer.WriteValue("A"); // TODO: get real gender
      _writer.WriteEndElement();

      _writer.WriteStartElement("season");
      _writer.WriteValue(race.DateResult.AddMonths(2).ToString("yyyy"));
      _writer.WriteEndElement();

      _writer.WriteEndElement();

    }

  }
}
