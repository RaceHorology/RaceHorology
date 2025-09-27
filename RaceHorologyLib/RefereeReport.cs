using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace RaceHorologyLib
{

    public class RefereeReport : PDFRaceReport
    {
        //private Dictionary<string, string> d = new Dictionary<string, string>
        //{
        //    ["Organisator"] = "SC Frauenau",
        //    ["Landesverband"] = "BSV",
        //    ["Datum"] = "07.05.2025",
        //    ["Veranstaltung"] = "dddd",
        //    ["Disziplin"] = "RS",
        //    ["RennNr"] = "1",
        //    ["Schiedsrichter"] = "Heider S.",
        //    ["Schiedsrichter_V"] = "d",
        //    ["Rennleiter"] = "Heider T.",
        //    ["Rennleiter_V"] = "d",
        //    ["Trainervertreter"] = "Weghofer P.",
        //    ["Trainervertreter_V"] = "d",
        //    ["EDVKR"] = "Marx Thomas",
        //    ["EDVKR_V"] = "d",
        //    ["Startrichter"] = "d",
        //    ["Startrichter_V"] = "d",
        //    ["Zielrichter"] = "d",
        //    ["Zielrichter_V"] = "dd",
        //    ["Org_Auslosung"] = "per DSValpin",
        //    ["Org_Siegerehrung"] = "nn Min. nach Ende",
        //    ["Org_MedLeiter"] = "name",
        //    ["Org_Torrichter"] = "name (TRC) plus n TR",
        //    ["Punkteberechnung"] = "ja",
        //    ["Zeitmessgeraet"] = "ALGE TdC 8000/8001",
        //    ["Einschaltzeit"] = "18:20",
        //    ["Synchronzeit"] = "18:50",
        //    ["StartersterLaeufer"] = "19:29",
        //    ["Kabelverbindung"] = "ja",
        //    ["Funkverbindung"] = "nein",
        //    ["ProblemeZeitmessung"] = "keine",
        //    ["Anz_Teilnehmer"] = "4",
        //    ["Anz_NichtamStartDG1"] = "0",
        //    ["Anz_Klassifizierte"] = "4",
        //    ["Rennstrecke"] = "Frauenau, Skihang Zell",
        //    ["homologiert"] = "ja",
        //    ["Streckenzustand"] = "Alt",
        //    ["DG1_Kurssetzer"] = "Heider T.",
        //    ["DG2_Kurssetzer"] = "Heider T.",
        //    ["Bem_Kurssetzer"] = "s",
        //    ["DG1_Streckenlaenge"] = "400",
        //    ["DG2_Streckenlaenge"] = "400",
        //    ["Bem_Streckenlaenge"] = "-sss",
        //    ["DG1_Hoehendifferenz"] = "50",
        //    ["DG2_Hoehendifferenz"] = "50",
        //    ["Bem_Hoehendifferenz"] = "-ss",
        //    ["DG1_Tore"] = "21",
        //    ["DG1_Richtaend"] = "20",
        //    ["DG2_Tore"] = "21",
        //    ["DG2_Richtaend"] = "20",
        //    ["Bem_Tore"] = "s",
        //    ["DG1_Bestzeit"] = "06,85",
        //    ["DG2_Bestzeit"] = "04,01",
        //    ["Bem_Bestzeit"] = "-ssss",
        //    ["StreckeGefahren"] = "sda",
        //    ["Stangen"] = "asd",
        //    ["Streckenverbesserung"] = "asd",
        //    ["Rettungsdienst"] = "jaasd",
        //    ["Unfaelle"] = "neinasd",
        //    ["Witterung"] = "Sonnig",
        //    ["Proteste"] = "keine",
        //    ["Sanktionen"] = "keine",
        //    ["Unterstuetzung"] = "ja",
        //    ["Bemerkungen"] = "-sdasd",
        //    ["Aussteller_Name"] = "Heider S.",
        //    ["Aussteller_Telefon"] = "sda",
        //    ["Aussteller_Email"] = "asda",
        //    ["Aussteller_KrNr"] = "ads"
        //};
        private List<string> Keys = new List<string>()
        {
            "Anz_Klassifizierte",
            "Anz_NichtamStartDG1",
            "Anz_Teilnehmer",
            "Aussteller_Email",
            "Aussteller_KrNr",
            "Aussteller_Name",
            "Aussteller_Telefon",
            "Bem_Bestzeit",
            "Bem_Hoehendifferenz",
            "Bem_Kurssetzer",
            "Bem_Streckenlaenge",
            "Bem_Tore",
            "Bemerkungen",
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
            "Trainervertreter",
            "Trainervertreter_V",
            "Unfaelle",
            "Unterstuetzung",
            "Veranstaltung",
            "Witterung",
            "Zeitmessgeraet",
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
                "DSV - Alpiner Schiedsrichterbericht\r\n(Nur Nationale Veranstaltungen) – Saison 2024/25");
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
            return string.Format("DSV - Alpiner Schiedsrichterbericht\r\n(Nur Nationale Veranstaltungen) – Saison 2024/25");
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

            doc.Add(new Paragraph("Jury / Kampfrichter").SetBold().SetFontSize(fontSize));
            AddJuryTable(doc, fontSize, d);


            doc.Add(new Paragraph("Organisation").SetBold().SetFontSize(fontSize));
            AddRowCustomWidths(doc, fontSize, new[] {
            ("Auslosung", d["Org_Auslosung"]),
            ("Siegerehrung", d["Org_Siegerehrung"])
            }, GenerateColumnWidths(2, 0.25f));

            AddRowCustomWidths(doc, fontSize, new[] {
            ("Med. Leiter", d["Org_MedLeiter"]),
            ("Torrichter", d["Org_Torrichter"])
            }, GenerateColumnWidths(2, 0.25f));

            doc.Add(new Paragraph("Zeitmessung und Auswertung").SetBold().SetFontSize(fontSize));

            AddRowCustomWidths(doc, fontSize, new[] {
            ("DSV-Punkteberechnung",d["Punkteberechnung"]),
            ("Start 1. Läufer", d["StartersterLaeufer"]),

            }, GenerateColumnWidths(2, 0.5f));

            AddRowCustomWidths(doc, fontSize, new[] {
            ("Zeitmessgerät", d["Zeitmessgeraet"]),
            ("Einschaltzeit", d["Einschaltzeit"]),
            ("Synchronzeit", d["Synchronzeit"])
            }, GenerateColumnWidths(3, 0.50f));

            AddRowCustomWidths(doc, fontSize, new[] {
            ("Kabel", d["Kabelverbindung"]),
            ("Funk", d["Funkverbindung"]),
            ("Probleme Zeitmessung", d["ProblemeZeitmessung"])
            }, GenerateColumnWidths(3, 0.50f));

            AddRowCustomWidths(doc, fontSize, new[] {
            ("Teilnehmer", d["Anz_Teilnehmer"]),
            ("Nicht am Start", d["Anz_NichtamStartDG1"]),
            ("Klassifizierte", d["Anz_Klassifizierte"]),
            }, GenerateColumnWidths(3, 0.50f));

            doc.Add(new Paragraph("Rennstrecke").SetBold().SetFontSize(fontSize));

            AddRowCustomWidths(doc, fontSize, new[] {
            ("Ort und Name der Rennstrecke", d["Rennstrecke"]),
            ("FIS homologiert", d["homologiert"]),
            }, new float[4]{0.45f,0.55f,0.25f,0.25f});

            AddRowCustomWidths(doc, fontSize, new[] {
            ("Vorbereitung und Schneeverhältnisse", d["Streckenzustand"])         
            }, GenerateColumnWidths(1, 0.30f));



            AddStreckenInfoTable(doc, fontSize, d);


            doc.Add(new Paragraph("Sicherheit").SetBold().SetFontSize(fontSize));
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


            doc.Add(new Paragraph("Rennabwicklung").SetBold().SetFontSize(fontSize));
            AddRowCustomWidths(doc, fontSize, new[] {
            ("Witterungs- und Sichtverhältnisse", d["Witterung"])
            }, GenerateColumnWidths(1, 0.30f));
            AddRowCustomWidths(doc, fontSize, new[] {
            ("Wurden Proteste eingereicht?", d["Proteste"])
            }, GenerateColumnWidths(1, 0.30f));
            AddRowCustomWidths(doc, fontSize, new[] {
            ("Sanktionen gegen Wettkämpfer?", d["Sanktionen"])
            }, GenerateColumnWidths(1, 0.30f));
            AddRowCustomWidths(doc, fontSize, new[] {
            ("Unterstützung der Jury durch Organisator?", d["Unterstuetzung"])
            }, GenerateColumnWidths(1, 0.30f));

            AddRowCustomWidths(doc, fontSize, new[] {
            ("Bemerkungen\r\nSonstiges ", d["Bemerkungen"])
            }, GenerateColumnWidths(1, 0.10f));

          
            doc.Add(new Paragraph("Aussteller").SetBold().SetFontSize(fontSize));

            AddRowCustomWidths(doc, fontSize, new[] {
            ("Name", d["Aussteller_Name"]),
            ("Tel.", d["Aussteller_Telefon"]),
            ("E-Mail", d["Aussteller_Email"])
            }, GenerateColumnWidths(3, 0.30f));


            AddRowCustomWidths(doc, fontSize, new[] {
            ("Ort, Datum", d["Organisator"] + ", " + d["Datum"]),
            ("Unterschrift", "gez. " + d["Aussteller_Name"]),
            ("KR-Nr.", d["Aussteller_KrNr"])
            }, GenerateColumnWidths(3, 0.30f));

            doc.Add(new Paragraph("Bericht ist vom Schiedsrichter/TD zu erstellen als PDF-Datei abzuspeichern und mit Angabe von Renn-Nr. zu senden an den einteilenden Kampfrichter-Referenzenten.\r\n" +
                "Bei DSV-Punkterennen und DSV-Schülerrennen ist der Bericht mit der Renn-Nr.=Dateiname abzuspeichern und innerhalb vom 3 Tagen zusätzlich zu senden an:").SetFontSize(fontSize));
            AddParagraphWithLink(doc, fontSize, "DSV - Kampfrichterreferenten, Hendrik Kuhn, Email ", "hendrik.kuhn@deutscherskiverband.de", "mailto:hendrik.kuhn@deutscherskiverband.de", "");
            AddParagraphWithLink(doc, fontSize, "DSV-Nachwuchsleistungssport alpin, Andreas Kindsmüller, Email ", "andreas.kindsmueller@deutscherskiverband.de", "mailto:andreas.kindsmueller@deutscherskiverband.de", "");
            AddParagraphWithLink(doc, fontSize, "DSV-Leistungssport alpin, Christian Scholz, Email ", "christian.scholz@deutscherskiverband.de", "mailto:christian.scholz@deutscherskiverband.de", "");
         
        }

        public void AddParagraphWithLink(Document document, float fontSize, string preText, string linkText, string url, string postText)
        {
            // Create a Paragraph
            Paragraph paragraph = new Paragraph()
                  .SetMargin(0)
                  .SetPadding(0);
 

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
                    .SetFontSize(fontSize).SetPadding(2));
                table.AddCell(new Cell().Add(new Paragraph(value))
                    .SetFontSize(fontSize).SetPadding(2));
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
            var p = new Paragraph($"{label}: {value}")
                .SetFontSize(fontSize)
                .SetMultipliedLeading(0.6f); // engere Zeilenhöhe
            doc.Add(p);
        }

        static void AddJuryTable(Document doc, float fontSize, Dictionary<string, string> d)
        {
            Table t = new Table(UnitValue.CreatePercentArray(new float[] { 2, 3, 3, 2 })).UseAllAvailableWidth();
            t.AddCell(Header("Funktion", fontSize));
            t.AddCell(Header("Name", fontSize));
            t.AddCell(Header("Verband", fontSize));
            t.AddCell(Header("Rolle", fontSize));

            AddJury(t, fontSize, "Schiedsrichter", d["Schiedsrichter"], d["Schiedsrichter_V"], "Jury");
            AddJury(t, fontSize, "Rennleiter", d["Rennleiter"], d["Rennleiter_V"], "Jury");
            AddJury(t, fontSize, "Trainervertreter", d["Trainervertreter"], d["Trainervertreter_V"], "Jury");
            AddJury(t, fontSize, "EDV-KR", d["EDVKR"], d["EDVKR_V"], "Kampfrichter");
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

        static Cell Cell(string text, float fontSize) =>
            new Cell().Add(new Paragraph(text)).SetFontSize(fontSize).SetPadding(1);

        static Cell Header(string text, float fontSize) =>
            new Cell().Add(new Paragraph(text).SetBold()).SetFontSize(fontSize).SetPadding(1);
        
      
        
    }
}
