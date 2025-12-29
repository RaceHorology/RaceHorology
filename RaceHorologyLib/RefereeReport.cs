using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;

namespace RaceHorologyLib
{

  public class RefereeReport : PDFRaceReport
  {
    private RefereeReportItems report;

    RaceRun _raceRun;
    public RefereeReport(RaceRun rr)
      : base(rr.GetRace())
    {
      _raceRun = rr;

      Race r = rr.GetRace();
      report = r.GetDataModel().GetDB().GetRefereeReport(r);

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
            ("Organisator", report.Data("Organisator")),
            ("Landesverband", report.Data("Landesverband")),
            ("Datum", report.Data("Datum"))
            }, GenerateColumnWidths(3, 0.4f));

      AddRowCustomWidths(doc, fontSize, new[] {
            ("Disziplin",     report.Data("Disziplin")),
            ("Renn-Nr.",      report.Data("RennNr")),
            (" ", " " )
            }, GenerateColumnWidths(3, 0.4f));


      AddRowCustomWidths(doc, fontSize, new[] {
            ("Veranstaltung", report.Data("Veranstaltung")),
            }, GenerateColumnWidths(1, 0.1335f));


      doc.Add(new Paragraph("Jury / Wettkampfkomitee").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));
      AddJuryTable(doc, fontSize, report);


      doc.Add(new Paragraph("Organisation").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Auslosung", report.Data("Org_Auslosung")),
            ("Siegerehrung", report.Data("Org_Siegerehrung"))
            }, GenerateColumnWidths(2, 0.25f));

      AddRowCustomWidths(doc, fontSize, new[] {
            ("Med. Leiter", report.Data("Org_MedLeiter")),
            ("Torrichter", report.Data("Org_Torrichter"))
            }, GenerateColumnWidths(2, 0.25f));

      doc.Add(new Paragraph("Zeitmessung und Auswertung").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));

      AddRowCustomWidths(doc, fontSize, new[] {
            ("DSV-Punkteberechnung?",report.Data("Punkteberechnung")),
            ("", ""),
            ("Einschaltzeit", report.Data("Einschaltzeit")),

            }, GenerateColumnWidths(3, 0.5f));

      AddRowCustomWidths(doc, fontSize, new[] {
            ("Zeitmessgerät", report.Data("Zeitmessgeraet")),
            ("Teilnehmer", report.Data("Anz_Teilnehmer")),
            ("Synchronzeit", report.Data("Synchronzeit"))
            }, GenerateColumnWidths(3, 0.50f));

      AddRowCustomWidths(doc, fontSize, new[] {
            ("Kabel", report.Data("Kabelverbindung")),
            ("Nicht am Start", report.Data("Anz_NichtamStartDG1")),
            ("Klassifizierte", report.Data("Anz_Klassifizierte")),

            }, GenerateColumnWidths(3, 0.50f));

      AddRowCustomWidths(doc, fontSize, new[] {
            ("Kabellose Verbindung", report.Data("Funkverbindung")),
            ("Sync-Zeit nach 1. Min.", report.Data("SyncZeit1Min")),
            ("Zeitmessstreifen liegt vor?", report.Data("Zeitmessstreifen")),
            }, GenerateColumnWidths(3, 0.50f));

      //AddRowCustomWidths(doc, fontSize, new[] {
      //("DSV-Punkteberechnung?",report.Data("Punkteberechnung")),
      //("Start 1. Läufer", report.Data("StartersterLaeufer")),

      //}, GenerateColumnWidths(2, 0.5f));

      //AddRowCustomWidths(doc, fontSize, new[] {
      //("Zeitmessgerät", report.Data("Zeitmessgeraet")),
      //("Einschaltzeit", report.Data("Einschaltzeit")),
      //("Synchronzeit", report.Data("Synchronzeit"))
      //}, GenerateColumnWidths(3, 0.50f));

      //AddRowCustomWidths(doc, fontSize, new[] {
      //("Kabel", report.Data("Kabelverbindung")),
      //("Funk", report.Data("Funkverbindung")),
      //("Probleme Zeitmessung", report.Data("ProblemeZeitmessung"))
      //}, GenerateColumnWidths(3, 0.50f));

      //AddRowCustomWidths(doc, fontSize, new[] {
      //("Teilnehmer", report.Data("Anz_Teilnehmer")),
      //("Nicht am Start", report.Data("Anz_NichtamStartDG1")),
      //("Klassifizierte", report.Data("Anz_Klassifizierte")),
      //}, GenerateColumnWidths(3, 0.50f));

      doc.Add(new Paragraph("Rennstrecke").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));

      AddRowCustomWidths(doc, fontSize, new[] {
            ("Ort und Name der Rennstrecke", report.Data("Rennstrecke")),
            ("FIS homologiert?", report.Data("homologiert")),
            }, new float[4] { 0.45f, 0.55f, 0.25f, 0.25f });

      AddRowCustomWidths(doc, fontSize, new[] {
            ("Vorbereitung und Schneeverhältnisse", report.Data("Streckenzustand"))
            }, GenerateColumnWidths(1, 0.30f));



      AddStreckenInfoTable(doc, fontSize, report);


      doc.Add(new Paragraph("Sicherheit").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Spezielle Gefahren der Strecke", report.Data("StreckeGefahren"))
            }, GenerateColumnWidths(1, 0.30f));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Verwendete Stangen und Torflaggen", report.Data("Stangen"))
            }, GenerateColumnWidths(1, 0.30f));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Streckenverbesserung durch die Jury?", report.Data("Streckenverbesserung"))
            }, GenerateColumnWidths(1, 0.30f));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("War der Rettungsdienst ausreichend?", report.Data("Rettungsdienst"))
            }, GenerateColumnWidths(1, 0.30f));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Gab es Unfälle während des Rennens?\r\n(Zusatzbericht erforderlich)", report.Data("Unfaelle"))
            }, GenerateColumnWidths(1, 0.30f));


      doc.Add(new Paragraph("Rennabwicklung").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Witterungs- und Sichtverhältnisse", report.Data("Witterung"))
            }, GenerateColumnWidths(1, 0.30f));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Wurden Proteste eingereicht?", report.Data("Proteste"))
            }, GenerateColumnWidths(1, 0.30f));
      AddRowCustomWidths(doc, fontSize, new[] {
            ("Sanktionen gegen Athleten?", report.Data("Sanktionen"))
            }, GenerateColumnWidths(1, 0.30f));
      //Entfernt Saison 25-26
      //AddRowCustomWidths(doc, fontSize, new[] {
      //("Unterstützung der Jury durch Organisator?", report.Data("Unterstuetzung"))
      //}, GenerateColumnWidths(1, 0.30f));

      //AddRowCustomWidths(doc, fontSize, new[] {
      //("Bemerkungen\r\nSonstiges ", report.Data("Bemerkungen"))
      //}, GenerateColumnWidths(1, 0.10f));
      Paragraph p = new Paragraph()
        .SetMargin(0)
        .SetPadding(0)
        .SetFontSize(fontSize)
        .SetFontColor(ColorConstants.RED);
      p.Add(new Text("Angaben / Unterschrift Zeitnehemer").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));
      p.Add(new Text("   Mit seiner Unterschrift bestätigt der Zeitnehmer alle Zeiten nach Vorgabe des DSV gemessen und dokumentiert zu haben"));

      doc.Add(p);


     AddSignTable(doc, fontSize, report, true);

      p = new Paragraph()
        .SetMargin(0)
        .SetPadding(0)
        .SetFontSize(fontSize)
        .SetFontColor(ColorConstants.RED);
      p.Add(new Text("Angaben / Unterschrift Schiedsrichter / TD").SetBold().SetFontSize(fontSize).SetFontColor(iText.Kernel.Colors.ColorConstants.RED));
      p.Add(new Text("   Mit seiner Unterschrift bestätigt der Schiedsrichter / TD, das alle Angaben im Schiedsrichterbericht kontrolliert wurden"));

      doc.Add(p);

      AddSignTable(doc, fontSize, report, false);


      doc.Add(new Paragraph("Der Bericht ist vom Schiedsrichter/TD oder Zeitnehmer zu erstellen als PDF-Datei abzuspeichern und mit Angabe von Renn-Nr. an den einteilenden Kampfrichter-Referenzenten zu senden.").SetMargin(0).SetPadding(0).SetFontSize(fontSize));


      p = new Paragraph()
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

    static void AddJuryTable(Document doc, float fontSize, RefereeReportItems report)
    {
      Table t = new Table(UnitValue.CreatePercentArray(new float[] { 2, 3, 3, 2 })).UseAllAvailableWidth();
      t.AddCell(Header("Funktion", fontSize));
      t.AddCell(Header("Name", fontSize));
      t.AddCell(Header("Verband/Verein", fontSize));
      t.AddCell(Header("Rolle", fontSize));

      AddJury(t, fontSize, "TD/Schiedsrichter", report.Data("Schiedsrichter"), report.Data("Schiedsrichter_V"), "Jury");
      AddJury(t, fontSize, "Rennleiter", report.Data("Rennleiter"), report.Data("Rennleiter_V"), "Jury");
      AddJury(t, fontSize, "Trainer-Vertreter", report.Data("Trainervertreter"), report.Data("Trainervertreter_V"), "Jury");
      AddJury(t, fontSize, "Zeitnehmer", report.Data("EDVKR"), report.Data("EDVKR_V"), "Kampfrichter");
      AddJury(t, fontSize, "Startrichter", report.Data("Startrichter"), report.Data("Startrichter_V"), "Kampfrichter");
      AddJury(t, fontSize, "Zielrichter", report.Data("Zielrichter"), report.Data("Zielrichter_V"), "Kampfrichter");

      doc.Add(t);
    }
    static void AddJury(Table t, float fontSize, string funktion, string name, string verband, string rolle)
    {
      t.AddCell(Cell(funktion, fontSize));
      t.AddCell(Cell(name, fontSize));
      t.AddCell(Cell(verband, fontSize));
      t.AddCell(Cell(rolle, fontSize));
    }


    static void AddStreckenInfoTable(Document doc, float fontSize, RefereeReportItems report)
    {
      Table t = new Table(UnitValue.CreatePercentArray(new float[] { 1.6f, 2.8f, 2.8f, 2.8f })).UseAllAvailableWidth();
      t.AddCell(Header("Streckeninfo", fontSize));
      t.AddCell(Header("1. Lauf/DG", fontSize));
      t.AddCell(Header("2. Lauf/DG", fontSize));
      t.AddCell(Header("Bemerkungen", fontSize));

      AddJury(t, fontSize, "Kurssetzer", report.Data("DG1_Kurssetzer"), report.Data("DG2_Kurssetzer"), report.Data("Bem_Kurssetzer"));
      AddJury(t, fontSize, "Streckenlänge", report.Data("DG1_Streckenlaenge"), report.Data("DG2_Streckenlaenge"), report.Data("Bem_Streckenlaenge"));
      AddJury(t, fontSize, "Höhendifferenz", report.Data("DG1_Hoehendifferenz"), report.Data("DG2_Hoehendifferenz"), report.Data("Bem_Hoehendifferenz"));
      AddJury(t, fontSize, "# Tore / Richtg. Änderung", report.Data("DG1_Tore") + "/" + report.Data("DG1_Richtaend"), report.Data("DG2_Tore") + "/" + report.Data("DG2_Richtaend"), report.Data("Bem_Tore"));
      AddJury(t, fontSize, "Bestzeit", report.Data("DG1_Bestzeit"), report.Data("DG2_Bestzeit"), report.Data("Bem_Bestzeit"));

      doc.Add(t);
    }


    static void AddSignTable(Document doc, float fontSize, RefereeReportItems report, bool isEDV)
    {
      Table t = new Table(UnitValue.CreatePercentArray(new float[] { 2, 2, 3, 3 })).UseAllAvailableWidth();
      t.AddCell(Header("Name, Vorname", fontSize));
      t.AddCell(Header("Telefonnummer", fontSize));
      t.AddCell(Header("E-Mail", fontSize));

      if (isEDV)
      {
        t.AddCell(Header("Unterschrift Zeitnehmer", fontSize));

        AddSign(t, fontSize, report.Data("EDVKR"), report.Data("EDVKR_Telefon"), report.Data("EDVKR_Email"), "");
      }
      else
      {
        t.AddCell(Header("Unterschrift Schiedsrichter / TD", fontSize));

        AddSign(t, fontSize, report.Data("Aussteller_Name"), report.Data("Aussteller_Telefon"), report.Data("Aussteller_Email"), "");
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
