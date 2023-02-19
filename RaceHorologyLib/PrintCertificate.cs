using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaceHorologyLib.PrintCertificateModel;
using iText.IO.Font;
using iText.Kernel.Font;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using iText.Kernel.XMP.Impl;

namespace RaceHorologyLib
{
  public class PrintCertificateModel
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
      TextItems = new List<TextItem>();
    }

  }


  internal class CertificatesUtils
  {
    protected Dictionary<string, PdfFont> _fontCache;

    public CertificatesUtils()
    {
      _fontCache = new Dictionary<string, PdfFont>();
    }

    // Taken from: https://stackoverflow.com/questions/21525377/retrieve-filename-of-a-font
    static string getSystemFontFileName(System.Drawing.Font font)
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

    public static int MapFontSize(string font)
    {
      var fontParts = font.Split(',');
      fontParts = Array.ConvertAll(fontParts, (f) => f.Trim());
      try { return int.Parse(fontParts.Last()); } catch (Exception) { return 10; }
    }

    public static bool MapIsFontItalic(string font)
    {
      var fontParts = font.Split(',');
      fontParts = Array.ConvertAll(fontParts, (f) => f.Trim());
      return fontParts.Contains("kursiv");
    }
    public static bool MapIsFontBold(string font)
    {
      var fontParts = font.Split(',');
      fontParts = Array.ConvertAll(fontParts, (f) => f.Trim());
      return fontParts.Contains("fett");
    }


    public PdfFont MapFont(string font)
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
      
      var fontCacheKey = string.Format("{0}-{1}-{2}", fontName, fontSize, fontStyle);
      if (_fontCache.ContainsKey(fontCacheKey))
        return _fontCache[fontCacheKey];

      try
      {
        var fXC = new System.Drawing.Font(fontParts[0], fontSize, fontStyle);
        var fXCFile = getSystemFontFileName(fXC);
        if (fXCFile != null)
        {
          var pdfFont = PdfFontFactory.CreateFont(@"c:\windows\fonts\" + fXCFile, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED, true);
          _fontCache.Add(fontCacheKey, pdfFont);
          return pdfFont;
        }
        else
          return null;
      }
      catch (iText.IO.Exceptions.IOException)
      {
        return null;
      }
    }

    public static float mmToPDFPoints(float tenthMilliMeter)
    {
      float mm = tenthMilliMeter / 10.0F;
      //PdfNumber userUnit = null;// pdf.GetFirstPage().GetPdfObject().GetAsNumber(PdfName.UserUnit);
      //float userUnitValue = userUnit == null ? 72f : userUnit.FloatValue();
      return mm * 2.83F;// Manually calculated out of page size and compared with DIN A4
    }

  }



  public class Certificates : PDFBaseRaceReport
  {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private bool _debugPdf = false;

    CertificatesUtils _utils;
    PrintCertificateModel _certificateModel;
    int _maxCertificatesPerGroup;
    bool _generateTemplate;

    internal delegate string GetterFunc(Race race, RaceResultItem rri);

    Dictionary<string, GetterFunc> _variableReplacements = new Dictionary<string, GetterFunc>();

    bool _firstPage;


    public Certificates(Race race, int maxCertificatesPerGroup, bool generateTemplate = false)
      : base(race) 
    {
      _certificateModel = race.GetDataModel().GetDB().GetCertificateModel(race);
      _maxCertificatesPerGroup = maxCertificatesPerGroup;
      _generateTemplate = generateTemplate;

      _variableReplacements.Add("<Vorname Name>", (rc, result) => { return result.Participant.Fullname; });
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
      _utils = new CertificatesUtils();

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
          for(int i = Math.Min(_maxCertificatesPerGroup, cvGroup.Items.Count)-1; i >= 0; i--)
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
        for (int i = Math.Min(_maxCertificatesPerGroup-1, resultItems.Count)-1; i >= 0; i--)
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
        if (CertificatesUtils.MapIsFontBold(ti.Font))
          par.SetBold();
        if (CertificatesUtils.MapIsFontItalic(ti.Font))
          par.SetItalic();

        par.SetFontSize(CertificatesUtils.MapFontSize(ti.Font));
        var font = _utils.MapFont(ti.Font);
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
