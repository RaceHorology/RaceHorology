/*
 *  Copyright (C) 2019 - 2021 by Sven Flossmann
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

using RaceHorologyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LiveTimingFIS
{
  public class LiveTimingFIS : ILiveTiming
  {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    string _fisRaceCode;
    string _fisCategory;
    string _fisPassword;
    int _fisPort;
    int _sequence;

    public LiveTimingFIS()
    {
    }

    private Race _race;
    public Race Race
    {
      set { _race = value; }
      get { return _race; }
    }

    public void Login(string fisRaceCode, string fisCategory, string fisPassword, int fisPort)
    {
      _fisRaceCode = fisRaceCode;
      _fisCategory = fisCategory;
      _fisPassword = fisPassword;
      _fisPort = fisPort;

      _sequence = 0;

      scheduleTransfer(new LTTransfer(getXmlClearRace(), _fisPort));
    }

    public void Start()
    {

      throw new NotImplementedException();
    }

    public void Stop()
    {
      throw new NotImplementedException();
    }


    #region FIS specific XML serializer

    protected XmlWriterSettings _xmlSettings;
    protected XmlWriter _writer;

    private void setUpXmlFormat()
    {
      _xmlSettings = new XmlWriterSettings();
      _xmlSettings.Indent = true;
      _xmlSettings.IndentChars = "  ";
      _xmlSettings.Encoding = Encoding.UTF8;
    }

    internal string getXmlClearRace()
    {
      using (var sw = new StringWriter())
      {
        using (var xw = XmlWriter.Create(sw, _xmlSettings))
        {
          xw.WriteStartDocument();
          xmlWriteStartElementLivetiming(xw);
          xw.WriteStartElement("command");
          
          xw.WriteStartElement("clear");
          xw.WriteEndElement(); // clear

          xw.WriteEndElement(); // command
          xw.WriteEndElement(); // Livetiming
          xw.WriteEndDocument();
        }
        return sw.ToString();
      }
    }


    internal string getXmlRaceRun(RaceRun raceRun)
    {
      using (var sw = new StringWriter())
      {
        using (var xw = XmlWriter.Create(sw, _xmlSettings))
        {
          xw.WriteStartDocument();
          xmlWriteStartElementLivetiming(xw);
          xw.WriteStartElement("raceinfo");

          xw.WriteElementString("event", raceRun.GetRace().Description);
          xw.WriteElementString("slope", raceRun.GetRace().AdditionalProperties.CoarseName);
          xw.WriteElementString("disciplin", getDisciplin(raceRun.GetRace()));
          xw.WriteElementString("gender", getGender(raceRun.GetRace()));
          xw.WriteElementString("category", _fisCategory);
          xw.WriteElementString("place", raceRun.GetRace().AdditionalProperties.Location);
          xw.WriteElementString("tempunit", "c");
          xw.WriteElementString("longunit", "m");
          xw.WriteElementString("speedunit", "Kmh");

          xw.WriteStartElement("run");
          xw.WriteAttributeString("no", raceRun.Run.ToString());

          xw.WriteElementString("disciplin", getDisciplin(raceRun.GetRace()));

          if (raceRun.GetRace().AdditionalProperties?.StartHeight > 0)
            xw.WriteElementString("start", raceRun.GetRace().AdditionalProperties?.StartHeight.ToString()));
          if (raceRun.GetRace().AdditionalProperties?.FinishHeight > 0)
            xw.WriteElementString("finish", raceRun.GetRace().AdditionalProperties?.FinishHeight.ToString()));
          if (raceRun.GetRace().AdditionalProperties?.StartHeight > 0 && raceRun.GetRace().AdditionalProperties?.FinishHeight > 0)
            xw.WriteElementString("height", (raceRun.GetRace().AdditionalProperties?.StartHeight - raceRun.GetRace().AdditionalProperties?.StartHeight).ToString()));

          AdditionalRaceProperties.RaceRunProperties raceRunProperties = null;
          if (raceRun.Run == 1)
            raceRunProperties = raceRun.GetRace().AdditionalProperties?.RaceRun1;
          else if (raceRun.Run == 2)
            raceRunProperties = raceRun.GetRace().AdditionalProperties?.RaceRun2;

          if (raceRunProperties != null)
          {
            if (raceRunProperties.Gates > 0)
              xw.WriteElementString("gates", raceRunProperties.Gates.ToString());

            if (raceRunProperties.Turns > 0)
              xw.WriteElementString("turninggates", raceRunProperties.Turns.ToString());

            if (raceRunProperties.StartTime.Contains(":") && raceRunProperties.StartTime.Length==5)
            {
              xw.WriteElementString("hour", raceRunProperties.StartTime.Substring(0, 2));
              xw.WriteElementString("minute", raceRunProperties.StartTime.Substring(3, 2));
            }
          }

          if (raceRun.GetRace().AdditionalProperties?.DateResultList != null)
          {
            xw.WriteElementString("day", ((DateTime)raceRun.GetRace().AdditionalProperties?.DateResultList).Day.ToString());
            xw.WriteElementString("month", ((DateTime)raceRun.GetRace().AdditionalProperties?.DateResultList).Month.ToString());
            xw.WriteElementString("year", ((DateTime)raceRun.GetRace().AdditionalProperties?.DateResultList).Year.ToString());
          }

          xw.WriteStartElement("racedef");
          xw.WriteEndElement();

          xw.WriteEndElement(); // run

          xw.WriteEndElement(); // raceinfo
          xw.WriteEndElement(); // Livetiming
          xw.WriteEndDocument();
        }
        return sw.ToString();
      }
    }


    private string getXmlStartList(RaceRun raceRun)
    {
      using (var sw = new StringWriter())
      {
        using (var xw = XmlWriter.Create(sw, _xmlSettings))
        {
          xw.WriteStartDocument();
          xmlWriteStartElementLivetiming(xw);

          xw.WriteStartElement("command");
          xw.WriteStartElement("activerun");
          xw.WriteAttributeString("no", raceRun.Run.ToString());
          xw.WriteEndElement(); // activerun
          xw.WriteEndElement(); // command

          xw.WriteStartElement("startlist");
          xw.WriteAttributeString("runno", raceRun.Run.ToString());

          StartListViewProvider slp = raceRun.GetStartListProvider();
          var startList = slp.GetViewList();

          int i = 1;
          foreach (var sle in startList)
          {
            xw.WriteElementString("bib", sle.StartNumber.ToString());
            xw.WriteElementString("lastname", sle.Name);
            xw.WriteElementString("firstname", sle.Firstname);
            xw.WriteElementString("nat", sle.Nation);
            xw.WriteElementString("fiscode", sle.Code);
            i++;
          }

          xw.WriteEndElement(); // startlist

          xw.WriteEndElement(); // Livetiming
          xw.WriteEndDocument();
        }
        return sw.ToString();
      }
    }


    private void xmlWriteStartElementLivetiming(XmlWriter xw)
    {
      xw.WriteStartElement("livetiming");
      xw.WriteAttributeString("codex", _fisRaceCode);
      xw.WriteAttributeString("passwd", _fisPassword);
      xw.WriteAttributeString("sequence", _sequence.ToString("D5"));
      xw.WriteAttributeString("timestamp", System.DateTime.Now.ToString("hh:mm:ss"));
    }

    #endregion


    #region FIS specific getter

    private string getDisciplin(Race race)
    {
      switch(race.RaceType)
      {
        case Race.ERaceType.DownHill: return "DH";
        case Race.ERaceType.GiantSlalom: return "GS";
        case Race.ERaceType.Slalom: return "SL";
        case Race.ERaceType.SuperG: return "SG";

        case Race.ERaceType.ParallelSlalom:
        case Race.ERaceType.KOSlalom:
        default:
          throw new Exception(string.Format("{0} not supported for FIS livetiming", race.RaceType));
      }
    }

    /// <summary>
    /// Determines the FIS gender based on the participants of a race
    /// </summary>
    /// <returns>M: men, L: ladies, A: mixed</returns>
    private string getGender(Race race)
    {
      string raceGender = string.Empty;
      foreach(var rp in race.GetParticipants())
      {
        char sex = char.ToUpper(rp.Sex.Name);
        string gender = string.Empty;

        if (sex == 'M')
          gender = "M";
        if (sex == 'W' || sex == 'L')
          gender = "L";

        if (raceGender == string.Empty)
          raceGender = gender;
        else if (raceGender != gender)
          raceGender = "A";

      }

      if (raceGender == string.Empty) // Fallback, if no participants
        raceGender = "A";

      return raceGender;
    }

    private string getDate(Race race)
    {
      DateTime? date = race.AdditionalProperties?.DateResultList;
      if (date == null)
        date = DateTime.Today;

      return ((DateTime)date).ToString("dd.MM.yy", System.Globalization.DateTimeFormatInfo.InvariantInfo);
    }


    #endregion


    #region Transfer Implementation

    List<LTTransfer> _transfers = new List<LTTransfer>();
    object _transferLock = new object();
    bool _transferInProgress = false;

    private void scheduleTransfer(LTTransfer transfer)
    {
      lock (_transferLock)
      {
        // Remove all outdated transfers
        //_transfers.RemoveAll(x => x.IsEqual(transfer));
        _transfers.Add(transfer);
      }

      if (!_transferInProgress)
      {
        _transferInProgress = true;
        processNextTransfer();
      }
    }


    private void processNextTransfer()
    {
      LTTransfer nextItem = null;
      lock (_transferLock)
      {
        if (_transfers.Count() > 0)
        {
          nextItem = _transfers[0];
          _transfers.RemoveAt(0);
        }
      }

      if (nextItem != null)
      {
        // Trigger execution of transfers
        Task.Run(() =>
        {
          Logger.Debug("process transfer: " + nextItem.ToString());
          nextItem.performTransfer();
        })
          .ContinueWith(delegate { processNextTransfer(); });
      }
      else
        _transferInProgress = false;
    }

    #endregion
  }


  public class LTTransfer
  {
    protected string _type;

    protected string _xmlMessage;
    protected int _port;

    public LTTransfer(string xmlMessage, int port)
    {
      _xmlMessage = xmlMessage;
      _port = port;
    }

    public override string ToString()
    {
      return "LTTransfer(" + _xmlMessage + ")";
    }


    public void performTransfer()
    {
      System.Net.Sockets.TcpClient tcpClient = new System.Net.Sockets.TcpClient();

      try
      {
        tcpClient.Connect("live.fisski.com", _port);

        byte[] utf8Message = System.Text.Encoding.UTF8.GetBytes(_xmlMessage);
        var stream = tcpClient.GetStream();
        stream.Write(utf8Message, 0, utf8Message.Length);

        tcpClient.Close();
      }
      catch(Exception )
      {
      }

      tcpClient.Dispose();
    }

  }


}
