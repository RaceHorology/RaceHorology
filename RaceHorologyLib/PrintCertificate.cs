using iText.Kernel.Pdf;
using iText.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  internal class PrintCertificateModel
  {
    enum TextItemAlignment { Left = 0, Center = 2, Right = 1 };
    struct TextItem
    {
      public string Text;
      public string Font;
      public TextItemAlignment Alignment;
      public int VPos;
      public int HPos;
    }

    List<TextItem> TextItems;

    public PrintCertificateModel() 
    {
      TextItems = new List<TextItem>()
      {
        new TextItem { Text = "SVM-Cup U12 VII", Font = "Haettenschweiler, kursiv, 28", Alignment = (TextItemAlignment) 2, VPos = 1345, HPos = 1050},
        new TextItem { Text = "2022", Font = "Arial Rounded MT Bold, 28", Alignment = (TextItemAlignment) 2, VPos = 1480, HPos = 1050},
        new TextItem { Text = "Riesenslalom", Font = "Bauhaus 93, fett, kursiv, 24", Alignment = (TextItemAlignment) 2, VPos = 1645, HPos = 1050},
        new TextItem { Text = "<Vorname Name>", Font = "Arial Narrow, fett, kursiv, 20", Alignment = (TextItemAlignment) 2, VPos = 1881, HPos = 1050},
        new TextItem { Text = "<Platz>. Platz", Font = "Arial, 16", Alignment = (TextItemAlignment) 2, VPos = 2042, HPos = 1050},
        new TextItem { Text = "<Klasse>", Font = "Arial, 16", Alignment = (TextItemAlignment) 0, VPos = 2269, HPos = 240},
        new TextItem { Text = "Zeit: <Zeit>", Font = "Arial, 16", Alignment = (TextItemAlignment) 1, VPos = 2269, HPos = 1820},
        new TextItem { Text = "Kirchberg in Tirol, <Bewerbsdatum>", Font = "Arial, 12", Alignment = (TextItemAlignment) 0, VPos = 2389, HPos = 240},
        new TextItem { Text = "WSV Glonn", Font = "Arial, 12", Alignment = (TextItemAlignment) 1, VPos = 2389, HPos = 1820}
      };
    }

  }


  public class Certificates : PDFBaseRaceReport
  {
    PrintCertificateModel _certificateModel;

    public Certificates(Race race)
      : base(race) 
    {
      _certificateModel = new PrintCertificateModel();
    }

    protected override void GenerateImpl(PdfDocument pdf, Document document, DateTime? creationDateTime = null)
    {


    }

    protected override Margins getMargins() { return new Margins { Top = 0.0F, Bottom = 0.0F, Left = 0.0F, Right = 0.0F }; }

    protected override string getReportName() { return "Urkunden"; }

  }
}
