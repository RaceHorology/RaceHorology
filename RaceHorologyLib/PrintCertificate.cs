using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Layout;
using iText.Layout.Properties;
using iText;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceHorologyLib.PrintCertificateModel;
using iText.Kernel.Geom;
using iText.IO.Font;
using iText.Kernel.Font;
using System.ArrayExtensions;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace RaceHorologyLib
{
  internal class PrintCertificateModel
  {
    public enum TextItemAlignment { Left = 0, Center = 2, Right = 1 };
    public struct TextItem
    {
      public string Text;
      public string Font;
      public TextItemAlignment Alignment;
      public int VPos;
      public int HPos;
    }

    public List<TextItem> TextItems;

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


  internal static class CertificatesUtils
  {
    // Taken from: https://stackoverflow.com/questions/21525377/retrieve-filename-of-a-font
    public static string GetSystemFontFileName(System.Drawing.Font font)
    {
      RegistryKey fonts = null;
      try
      {
        fonts = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Fonts", false);
        if (fonts == null)
        {
          fonts = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Fonts", false);
          if (fonts == null)
          {
            throw new Exception("Can't find font registry database.");
          }
        }

        string suffix = "";
        if (font.Bold)
          suffix += "(?: Bold)?";
        if (font.Italic)
          suffix += "(?: Italic)?";

        var regex = new Regex(@"^(?:.+ & )?" + Regex.Escape(font.Name) + @"(?: & .+)?(?<suffix>" + suffix + @") \(TrueType\)$");

        string[] names = fonts.GetValueNames();

        string name = names.Select(n => regex.Match(n)).Where(m => m.Success).OrderByDescending(m => m.Groups["suffix"].Length).Select(m => m.Value).FirstOrDefault();

        if (name != null)
        {
          var f = fonts.GetValue(name);
          return f.ToString();
        }
        else
        {
          return null;
        }
      }
      finally
      {
        if (fonts != null)
        {
          fonts.Dispose();
        }
      }
    }

    public static TextAlignment mapAlignment(TextItemAlignment al)
    {
      switch (al)
      {
        case TextItemAlignment.Left: return TextAlignment.LEFT;
        case TextItemAlignment.Right: return TextAlignment.RIGHT;
        case TextItemAlignment.Center: return TextAlignment.CENTER;
      }
      return TextAlignment.LEFT;
    }

    public static int mapFontSize(string font)
    {
      var fontParts = font.Split(',');
      fontParts = Array.ConvertAll(fontParts, (f) => f.Trim());
      try { return int.Parse(fontParts.Last()); } catch (Exception) { return 10; }
    }

    public static bool mapIsFontItalic(string font)
    {
      var fontParts = font.Split(',');
      fontParts = Array.ConvertAll(fontParts, (f) => f.Trim());
      return fontParts.Contains("kursiv");
    }
    public static bool mapIsFontBold(string font)
    {
      var fontParts = font.Split(',');
      fontParts = Array.ConvertAll(fontParts, (f) => f.Trim());
      return fontParts.Contains("fett");
    }


    public static PdfFont mapFont(string font)
    {
      var fontParts = font.Split(',');
      fontParts = Array.ConvertAll(fontParts, (f) => f.Trim());

      var fontName = fontParts[0];
      var fontSize = 1;
      try { fontSize = int.Parse(fontParts.Last()); } catch (Exception) { }

      FontStyle fontStyle = FontStyle.Regular;
      // Omit the following, since the font style does not seem to be considered correctly, using Paragraph.SetBold() / .SetItalic() instead
      //if (fontParts.Contains("kursiv"))
      //  fontStyle = fontStyle | FontStyle.Italic;
      //if (fontParts.Contains("fett"))
      //  fontStyle = fontStyle | FontStyle.Bold;
      try
      {
        var fXC = new System.Drawing.Font(fontParts[0], fontSize, fontStyle);
        var fXCFile = GetSystemFontFileName(fXC);
        if (fXCFile != null)
          return PdfFontFactory.CreateFont(@"c:\windows\fonts\" + fXCFile, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
        else
          return null;
      }
      catch (iText.IO.Exceptions.IOException)
      {
        return null;
      }
    }

    public static float mmToPDFPoints(float micrometer)
    {
      float mm = micrometer / 10.0F;
      //PdfNumber userUnit = null;// pdf.GetFirstPage().GetPdfObject().GetAsNumber(PdfName.UserUnit);
      //float userUnitValue = userUnit == null ? 72f : userUnit.FloatValue();
      return mm * 2.83F;// Manually calculated out of page size and compared with DIN A4
    }

  }



  public class Certificates : PDFBaseRaceReport
  {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private bool _debugPdf = false;
    PrintCertificateModel _certificateModel;
    int _maxCertificatesPerGroup;
    bool _generateTemplate;

    internal delegate string GetterFunc(Race race, RaceResultItem rri);

    Dictionary<string, GetterFunc> _variableReplacements = new Dictionary<string, GetterFunc>();

    bool _firstPage;


    public Certificates(Race race, int maxCertificatesPerGroup, bool generateTemplate = false)
      : base(race) 
    {
      _certificateModel = new PrintCertificateModel();
      _maxCertificatesPerGroup = maxCertificatesPerGroup;
      _generateTemplate = generateTemplate;

      _variableReplacements.Add("<Vorname Name>", (rc, result) => { return result.Participant.Name; });
      _variableReplacements.Add("<Vorname>", (rc, result) => { return result.Participant.Firstname; } );
      _variableReplacements.Add("<Nachname>", (rc, result) => { return result.Participant.Name; } );
      _variableReplacements.Add("<Verein>", (rc, result) => { return result.Participant.Club; } );
      _variableReplacements.Add("<Klasse>", (rc, result) => { return result.Participant.Class.ToString(); });
      _variableReplacements.Add("<Gruppe>", (rc, result) => { return result.Participant.Group.ToString(); });
      _variableReplacements.Add("<Kategorie>", (rc, result) => { return result.Participant.Sex.PrettyName.ToString(); });
      _variableReplacements.Add("<Disziplin>", (rc, result) => { return rc.RaceType.ToString();} );
      _variableReplacements.Add("<Platz>", (rc, result) => { return result.Position.ToString();} );
      _variableReplacements.Add("<Zeit>", (rc, result) => { return result.TotalTime.ToRaceTimeString(); } );
      _variableReplacements.Add("<Bewerbsdatum>", (rc, result) => { return rc.DateResultList?.ToShortDateString(); } );
      _variableReplacements.Add("<Datum>", (rc, result) => { return DateTime.Now.ToShortDateString(); });
    }

    protected override void GenerateImpl(PdfDocument pdf, Document document, DateTime? creationDateTime = null)
    {
      _firstPage = true;

      _document.SetMargins(0, 0, 0, 0);
      var pageSize = pdf.GetDefaultPageSize();

      if (_generateTemplate)
      {
        addCertificate(document, null);
        return;
      }

      var results = _race.GetResultViewProvider().GetView();
      var lr = results as System.Windows.Data.ListCollectionView;
      if (results.Groups != null)
      {
        foreach (var group in results.Groups)
        {
          System.Windows.Data.CollectionViewGroup cvGroup = group as System.Windows.Data.CollectionViewGroup;
          for(int i = Math.Min(_maxCertificatesPerGroup-1, cvGroup.Items.Count); cvGroup.Items.Count > 0 && i >= 0; i--)
          {
            var result = cvGroup.Items[i] as RaceResultItem;
            if (result.Position > 0)
              addCertificate(document, result);
          }
        }
      }
      else
      {
        var resultItems = results.SourceCollection.Cast<RaceResultItem>().ToList();
        for (int i = Math.Min(_maxCertificatesPerGroup-1, resultItems.Count); resultItems.Count > 0 && i >= 0; i--)
        {
          var result = resultItems[i] as RaceResultItem;
          if (result.Position > 0)
            addCertificate(document, result);
        }
      }
    }

    protected void addDebugMarkers(Document document)
    {
      var pageSize = document.GetPdfDocument().GetDefaultPageSize();

      document.ShowTextAligned(new Paragraph("top left"), 0, pageSize.GetHeight(), TextAlignment.LEFT, VerticalAlignment.TOP);
      document.ShowTextAligned(new Paragraph("top right"), pageSize.GetWidth(), pageSize.GetHeight(), TextAlignment.RIGHT, VerticalAlignment.TOP);
      document.ShowTextAligned(new Paragraph("bottom left"), 0, 0, TextAlignment.LEFT, VerticalAlignment.BOTTOM);
      document.ShowTextAligned(new Paragraph("bottom right"), pageSize.GetWidth(), 0, TextAlignment.RIGHT, VerticalAlignment.BOTTOM);
    }


    protected void addCertificate(Document document, RaceResultItem result)
    {
      var pageSize = document.GetPdfDocument().GetDefaultPageSize();

      if (!_firstPage)
        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

      if (_debugPdf)
        addDebugMarkers(document);

      foreach (var ti in _certificateModel.TextItems)
      {
        var par = new Paragraph(replaceVariables(ti.Text, result));
        if (CertificatesUtils.mapIsFontBold(ti.Font))
          par.SetBold();
        if (CertificatesUtils.mapIsFontItalic(ti.Font))
          par.SetItalic();

        par.SetFontSize(CertificatesUtils.mapFontSize(ti.Font));
        var font = CertificatesUtils.mapFont(ti.Font);
        if (font != null)
          par.SetFont(font);

        document.ShowTextAligned(par, CertificatesUtils.mmToPDFPoints(ti.HPos), pageSize.GetHeight() - CertificatesUtils.mmToPDFPoints(ti.VPos), CertificatesUtils.mapAlignment(ti.Alignment), VerticalAlignment.TOP);
      }

      _firstPage = false;
    }

    protected string replaceVariables(string template, RaceResultItem rri)
    {
      if (_generateTemplate)
        return template;

      string result = template;
      foreach(var replacement in _variableReplacements)
      {
        if (result.IndexOf(replacement.Key) >= 0)
        {
          try
          {
            result = result.Replace(replacement.Key, replacement.Value(_race, rri));
          }
          catch (Exception e)
          {
            Logger.Error(e);
          }
        }
      }
      return result;
    }

    protected override Margins getMargins() { return new Margins { Top = 0.0F, Bottom = 0.0F, Left = 0.0F, Right = 0.0F }; }

    protected override string getReportName() { return "Urkunden"; }

  }
}
