using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;

namespace RaceHorologyLib
{

  public class RefereeReport : PDFRaceReport
  {
    private List<string> Keys = new List<string>()
        {
            "Anz_Klassifizierte",
            "Anz_NichtamStartDG1",
            "Anz_Teilnehmer",
            "Aussteller_Email",
            //"Aussteller_KrNr",
            "Aussteller_Name",
            "Aussteller_Telefon",
            "Bem_Bestzeit",
            "Bem_Hoehendifferenz",
            "Bem_Kurssetzer",
            "Bem_Streckenlaenge",
            "Bem_Tore",
            //"Bemerkungen",
            "Datum",
            "DG1_Bestzeit",
            "DG1_Hoehendifferenz",
            "DG1_Kurssetzer",
            "DG1_Richtaend",
            "DG1_Streckenlaenge",
            "DG1_Tore",
            "DG2_Bestzeit",
            "DG2_Hoehendifferenz",
            "DG2_Kurssetzer",
            "DG2_Richtaend",
            "DG2_Streckenlaenge",
            "DG2_Tore",
            "Disziplin",
            "EDVKR",
            "EDVKR_V",
            "EDVKR_Email",
            "EDVKR_Telefon",
            "Einschaltzeit",
            "Funkverbindung",
            "homologiert",
            "Kabelverbindung",
            "Landesverband",
            "Org_Auslosung",
            "Org_MedLeiter",
            "Org_Siegerehrung",
            "Org_Torrichter",
            "Organisator",
            "ProblemeZeitmessung",
            "Proteste",
            "Punkteberechnung",
            "Rennleiter",
            "Rennleiter_V",
            "RennNr",
            "Rennstrecke",
            "Rettungsdienst",
            "Sanktionen",
            "Schiedsrichter",
            "Schiedsrichter_V",
            "Stangen",
            "StartersterLaeufer",
            "Startrichter",
            "Startrichter_V",
            "StreckeGefahren",
            "Streckenverbesserung",
            "Streckenzustand",
            "Synchronzeit",
            "SyncZeit1Min",
            "Trainervertreter",
            "Trainervertreter_V",
            "Unfaelle",
            //"Unterstuetzung",
            "Veranstaltung",
            "Witterung",
            "Zeitmessgeraet",
            "Zeitmessstreifen",
            "Zielrichter",
            "Zielrichter_V"
        };

    private Dictionary<string, string> d;

    RaceRun _raceRun;
    public RefereeReport(RaceRun rr)
      : base(rr.GetRace())
    {
      _raceRun = rr;

      Race r = rr.GetRace();
      d = r.GetDataModel().GetDB().GetRefereeReportData(r);

    }

    protected override ReportHeader createHeader()
    {
      return new ReportHeader(_pdfDocument, _document, _pdfHelper, _race, getTitle(), _pageMargins, false, true,
          "Deutscher Skiverband e.V. - TD/Schiedsrichter-Bericht\r\nSaison 2025-26 (Nur Nationale Veranstaltungen)");
    }

    protected override ReportFooter createFooter(DateTime creationDateTime)
    {
      return new ReportFooter(_pdfDocument, _document, _pdfHelper, _race, getTitle(), _pageMargins, creationDateTime, false, true);
    }

    protected override string getReportName()
    {
      return string.Format("Schiedsrichterbericht");
    }

    protected override string getTitle()
    {
      return string.Format("Deutscher Skiverband e.V. - TD/Schiedsrichter-Bericht\r\nSaison 2025-26 (Nur Nationale Veranstaltungen)");
    }

    protected override void addContent(PdfDocument pdf, Document doc)
    {
      float fontSize = 7f;

      doc.Add(new Paragraph("").SetBold().SetFontSize(fontSize));


      AddRowCustomWidths(doc, fontSize, new[] {
            ("Organisator", d["Organisator"]),
            ("Landesverband", d["Landesverband"]),
            ("Datum", d["Datum"])
            }, GenerateColumnWidths(3, 0.4f));

      AddRowCustomWidths(doc, fontSize, new[] {
            ("Veranstaltung", d["Veranstaltung"]),
            ("Disziplin", d["Disziplin"]),
            ("Renn-Nr.", d["RennNr"])
            }, GenerateColumnWidths(3, 0.4f));

      doc.Add(new Paragraph("Jury / Wettkampfkomitee").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));
      AddJuryTable(doc, fontSize, d);


      doc.Add(new Paragraph("Organisation").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Auslosung", d["Org_Auslosung"]),
            ("Siegerehrung", d["Org_Siegerehrung"])
            }, GenerateColumnWidths(2, 0.25f));

      AddRowCustomWidths(doc, fontSize, new[] {
            ("Med. Leiter", d["Org_MedLeiter"]),
            ("Torrichter", d["Org_Torrichter"])
            }, GenerateColumnWidths(2, 0.25f));

      doc.Add(new Paragraph("Zeitmessung und Auswertung").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));

      AddRowCustomWidths(doc, fontSize, new[] {
            ("DSV-Punkteberechnung?",d["Punkteberechnung"]),
            ("", ""),
            ("Einschaltzeit", d["Einschaltzeit"]),

            }, GenerateColumnWidths(3, 0.5f));

      AddRowCustomWidths(doc, fontSize, new[] {
            ("Zeitmessgerät", d["Zeitmessgeraet"]),
            ("Teilnehmer", d["Anz_Teilnehmer"]),
            ("Synchronzeit", d["Synchronzeit"])
            }, GenerateColumnWidths(3, 0.50f));

      AddRowCustomWidths(doc, fontSize, new[] {
            ("Kabel", d["Kabelverbindung"]),
            ("Nicht am Start", d["Anz_NichtamStartDG1"]),
            ("Klassifizierte", d["Anz_Klassifizierte"]),

            }, GenerateColumnWidths(3, 0.50f));

      AddRowCustomWidths(doc, fontSize, new[] {
            ("Kabellose Verbindung", d["Funkverbindung"]),
            ("Sync-Zeit nach 1. Min.", d["SyncZeit1Min"]),
            ("Zeitmessstreifen liegt vor?", d["Zeitmessstreifen"]),
            }, GenerateColumnWidths(3, 0.50f));

      //AddRowCustomWidths(doc, fontSize, new[] {
      //("DSV-Punkteberechnung?",d["Punkteberechnung"]),
      //("Start 1. Läufer", d["StartersterLaeufer"]),

      //}, GenerateColumnWidths(2, 0.5f));

      //AddRowCustomWidths(doc, fontSize, new[] {
      //("Zeitmessgerät", d["Zeitmessgeraet"]),
      //("Einschaltzeit", d["Einschaltzeit"]),
      //("Synchronzeit", d["Synchronzeit"])
      //}, GenerateColumnWidths(3, 0.50f));

      //AddRowCustomWidths(doc, fontSize, new[] {
      //("Kabel", d["Kabelverbindung"]),
      //("Funk", d["Funkverbindung"]),
      //("Probleme Zeitmessung", d["ProblemeZeitmessung"])
      //}, GenerateColumnWidths(3, 0.50f));

      //AddRowCustomWidths(doc, fontSize, new[] {
      //("Teilnehmer", d["Anz_Teilnehmer"]),
      //("Nicht am Start", d["Anz_NichtamStartDG1"]),
      //("Klassifizierte", d["Anz_Klassifizierte"]),
      //}, GenerateColumnWidths(3, 0.50f));

      doc.Add(new Paragraph("Rennstrecke").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));

      AddRowCustomWidths(doc, fontSize, new[] {
            ("Ort und Name der Rennstrecke", d["Rennstrecke"]),
            ("FIS homologiert?", d["homologiert"]),
            }, new float[4] { 0.45f, 0.55f, 0.25f, 0.25f });

      AddRowCustomWidths(doc, fontSize, new[] {
            ("Vorbereitung und Schneeverhältnisse", d["Streckenzustand"])
            }, GenerateColumnWidths(1, 0.30f));



      AddStreckenInfoTable(doc, fontSize, d);


      doc.Add(new Paragraph("Sicherheit").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Spezielle Gefahren der Strecke", d["StreckeGefahren"])
            }, GenerateColumnWidths(1, 0.30f));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Verwendete Stangen und Torflaggen", d["Stangen"])
            }, GenerateColumnWidths(1, 0.30f));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Streckenverbesserung durch die Jury?", d["Streckenverbesserung"])
            }, GenerateColumnWidths(1, 0.30f));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("War der Rettungsdienst ausreichend?", d["Rettungsdienst"])
            }, GenerateColumnWidths(1, 0.30f));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Gab es Unfälle während des Rennens?\r\n(Zusatzbericht erforderlich)", d["Unfaelle"])
            }, GenerateColumnWidths(1, 0.30f));


      doc.Add(new Paragraph("Rennabwicklung").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Witterungs- und Sichtverhältnisse", d["Witterung"])
            }, GenerateColumnWidths(1, 0.30f));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Wurden Proteste eingereicht?", d["Proteste"])
            }, GenerateColumnWidths(1, 0.30f));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Sanktionen gegen Athleten?", d["Sanktionen"])
            }, GenerateColumnWidths(1, 0.30f));
      //Entfernt Saison 25-26
      //AddRowCustomWidths(doc, fontSize, new[] {
      //("Unterstützung der Jury durch Organisator?", d["Unterstuetzung"])
      //}, GenerateColumnWidths(1, 0.30f));

      //AddRowCustomWidths(doc, fontSize, new[] {
      //("Bemerkungen\r\nSonstiges ", d["Bemerkungen"])
      //}, GenerateColumnWidths(1, 0.10f));

      doc.Add(new Paragraph("Angaben / Unterschrift Zeitnehemer").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));
      AddParagraphRow(doc, "Mit seiner Unterschrift bestätigt der Zeitnehmer alle Zeiten nach Vorgabe des DSV gemessen und dokumentiert zu haben", "", fontSize);

      AddSignTable(doc, fontSize, d, true);


      doc.Add(new Paragraph("Angaben / Unterschrift Schiedsrichter / TD").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));
      AddParagraphRow(doc, "Mit seiner Unterschrift bestätigt der Schiedsrichter / TD, das alle Angaben im Schiedsrichterbericht kontrolliert wurden", "", fontSize);
      AddSignTable(doc, fontSize, d, false);


      doc.Add(new Paragraph("Der Bericht ist vom Schiedsrichter/TD oder Zeitnehmer zu erstellen als PDF-Datei abzuspeichern und mit Angabe von Renn-Nr. an den einteilenden Kampfrichter-Referenzenten zu senden.").SetMargin(0).SetPadding(0).SetFontSize(fontSize));


      Paragraph p = new Paragraph()
          .SetMargin(0)
          .SetPadding(0)
          .SetFontSize(fontSize)
          .SetFontColor(ColorConstants.RED);
      p.Add(new Text("Bei DSV-Punkterennen und DSV-Schülerrennen ist der Bericht mit der Renn-Nr.=Dateiname abzuspeichern und "));
      p.Add(new Text("spätestens am Folgetag ").SetBold());
      p.Add(new Text("zusätzlich zu senden an:"));
      doc.Add(p);

      AddParagraphWithLink(doc, fontSize, "DSV - Kampfrichterreferenten, Hendrik Kuhn, Email ", "hendrik.kuhn@deutscherskiverband.de", "mailto:hendrik.kuhn@deutscherskiverband.de", "");
      AddParagraphWithLink(doc, fontSize, "DSV-Nachwuchsleistungssport alpin, Andreas Kindsmüller, Email ", "andreas.kindsmueller@deutscherskiverband.de", "mailto:andreas.kindsmueller@deutscherskiverband.de", "");
      AddParagraphWithLink(doc, fontSize, "DSV-Leistungssport alpin, Christian Scholz, Email ", "christian.scholz@deutscherskiverband.de", "mailto:christian.scholz@deutscherskiverband.de", "");

    }

    public void AddParagraphWithLink(Document document, float fontSize, string preText, string linkText, string url, string postText)
    {
      // Create a Paragraph
      Paragraph paragraph = new Paragraph()
            .SetMargin(0)
            .SetPadding(0)
            .SetFontColor(iText.Kernel.Colors.ColorConstants.RED);


      // Add the text before the link
      paragraph.Add(new Text(preText));

      // Create the hyperlink text
      var link = new Text(linkText)
          .SetFontColor(ColorConstants.BLUE)
          .SetUnderline()
          .SetAction(PdfAction.CreateURI(url));

      // Add the link
      paragraph.Add(link);

      // Add the text after the link
      paragraph.Add(new Text(postText));

      paragraph.SetFontSize(fontSize);

      // Add the paragraph to the document
      document.Add(paragraph);
    }

    static void AddRow(Document doc, float fontSize, int cols, (string, string)[] values)
    {
      var widths = new float[cols];
      for (int i = 0; i < cols; i++) widths[i] = 1;
      Table t = new Table(UnitValue.CreatePercentArray(widths)).UseAllAvailableWidth();
      foreach (var (label, value) in values)
      {
        t.AddCell(new Cell().Add(new Paragraph($"{label}: {value}"))
            .SetFontSize(fontSize).SetPadding(2));
      }
      doc.Add(t);
    }

    static void AddRowCustomWidths(Document doc, float fontSize, (string label, string value)[] values, float[] columnWidths)
    {
      if (columnWidths.Length != values.Length * 2)
        throw new ArgumentException("columnWidths must have twice the length of values");

      Table table = new Table(UnitValue.CreatePercentArray(columnWidths)).UseAllAvailableWidth();

      foreach (var (label, value) in values)
      {
        table.AddCell(new Cell().Add(new Paragraph(label).SetBold())
            .SetFontSize(fontSize).SetPadding(1.5F));
        table.AddCell(new Cell().Add(new Paragraph(value))
            .SetFontSize(fontSize).SetPadding(1.5F));
      }

      doc.Add(table);
    }

    static float[] GenerateColumnWidths(int pairCount, float labelWidth = 0.3f)
    {
      float valueWidth = 1f - labelWidth;
      float[] widths = new float[pairCount * 2];
      for (int i = 0; i < pairCount; i++)
      {
        widths[i * 2] = labelWidth;
        widths[i * 2 + 1] = valueWidth;
      }
      return widths;
    }

    static void AddParagraphRow(Document doc, string label, string value, float fontSize)
    {
      if (value == string.Empty)
      {
        var p = new Paragraph($"{label}")
          .SetFontSize(fontSize)
          .SetMargin(0.5F)
          .SetPadding(0.5F)
          .SetFontColor(iText.Kernel.Colors.ColorConstants.RED);  // engere Zeilenhöhe

        doc.Add(p);
      }
      else
      {
        var p = new Paragraph($"{label}: {value}")
            .SetFontSize(fontSize)
            .SetMargin(0.5F)
            .SetPadding(0.5F)
            .SetFontColor(iText.Kernel.Colors.ColorConstants.RED);  // engere Zeilenhöhe

        doc.Add(p);
      }

    }

    static void AddJuryTable(Document doc, float fontSize, Dictionary<string, string> d)
    {
      Table t = new Table(UnitValue.CreatePercentArray(new float[] { 2, 3, 3, 2 })).UseAllAvailableWidth();
      t.AddCell(Header("Funktion", fontSize));
      t.AddCell(Header("Name", fontSize));
      t.AddCell(Header("Verband/Verein", fontSize));
      t.AddCell(Header("Rolle", fontSize));

      AddJury(t, fontSize, "TD/Schiedsrichter", d["Schiedsrichter"], d["Schiedsrichter_V"], "Jury");
      AddJury(t, fontSize, "Rennleiter", d["Rennleiter"], d["Rennleiter_V"], "Jury");
      AddJury(t, fontSize, "Trainer-Vertreter", d["Trainervertreter"], d["Trainervertreter_V"], "Jury");
      AddJury(t, fontSize, "Zeitnehmer", d["EDVKR"], d["EDVKR_V"], "Kampfrichter");
      AddJury(t, fontSize, "Startrichter", d["Startrichter"], d["Startrichter_V"], "Kampfrichter");
      AddJury(t, fontSize, "Zielrichter", d["Zielrichter"], d["Zielrichter_V"], "Kampfrichter");

      doc.Add(t);
    }
    static void AddJury(Table t, float fontSize, string funktion, string name, string verband, string rolle)
    {
      t.AddCell(Cell(funktion, fontSize));
      t.AddCell(Cell(name, fontSize));
      t.AddCell(Cell(verband, fontSize));
      t.AddCell(Cell(rolle, fontSize));
    }


    static void AddStreckenInfoTable(Document doc, float fontSize, Dictionary<string, string> d)
    {
      Table t = new Table(UnitValue.CreatePercentArray(new float[] { 1.6f, 2.8f, 2.8f, 2.8f })).UseAllAvailableWidth();
      t.AddCell(Header("Streckeninfo", fontSize));
      t.AddCell(Header("1. Lauf/DG", fontSize));
      t.AddCell(Header("2. Lauf/DG", fontSize));
      t.AddCell(Header("Bemerkungen", fontSize));

      AddJury(t, fontSize, "Kurssetzer", d["DG1_Kurssetzer"], d["DG2_Kurssetzer"], d["Bem_Kurssetzer"]);
      AddJury(t, fontSize, "Streckenlänge", d["DG1_Streckenlaenge"], d["DG2_Streckenlaenge"], d["Bem_Streckenlaenge"]);
      AddJury(t, fontSize, "Höhendifferenz", d["DG1_Hoehendifferenz"], d["DG2_Hoehendifferenz"], d["Bem_Hoehendifferenz"]);
      AddJury(t, fontSize, "# Tore / Richtg. Änderung", d["DG1_Tore"] + "/" + d["DG1_Richtaend"], d["DG2_Tore"] + "/" + d["DG2_Richtaend"], d["Bem_Tore"]);
      AddJury(t, fontSize, "Bestzeit", d["DG1_Bestzeit"], d["DG2_Bestzeit"], d["Bem_Bestzeit"]);

      doc.Add(t);
    }


    static void AddSignTable(Document doc, float fontSize, Dictionary<string, string> d, bool isEDV)
    {
      Table t = new Table(UnitValue.CreatePercentArray(new float[] { 2, 2, 3, 3 })).UseAllAvailableWidth();
      t.AddCell(Header("Name, Vorname", fontSize));
      t.AddCell(Header("Telefonnummer", fontSize));
      t.AddCell(Header("E-Mail", fontSize));

      if (isEDV)
      {
        t.AddCell(Header("Unterschrift Zeitnehmer", fontSize));

        AddSign(t, fontSize, d["EDVKR"], d["EDVKR_Telefon"], d["EDVKR_Email"], "");
      }
      else
      {
        t.AddCell(Header("Unterschrift Schiedsrichter / TD", fontSize));

        AddSign(t, fontSize, d["Aussteller_Name"], d["Aussteller_Telefon"], d["Aussteller_Email"], "");
      }



      doc.Add(t);
    }

    static void AddSign(Table t, float fontSize, string name, string telefonnummer, string email, string unterschrift)
    {
      t.AddCell(Cell(name, fontSize));
      t.AddCell(Cell(telefonnummer, fontSize));
      t.AddCell(Cell(email, fontSize));
      t.AddCell(Cell(unterschrift, fontSize));
    }

    static Cell Cell(string text, float fontSize) =>
        new Cell().Add(new Paragraph(text)).SetFontSize(fontSize).SetPadding(1);

    static Cell Header(string text, float fontSize) =>
        new Cell().Add(new Paragraph(text).SetBold()).SetFontSize(fontSize).SetPadding(1);



  }
}
