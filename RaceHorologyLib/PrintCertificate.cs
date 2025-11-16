using DocumentFormat.OpenXml.Spreadsheet;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.XMP.Impl;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using static RaceHorologyLib.PrintCertificateModel;



namespace RaceHorologyLib
{

    public class PrintCertificateModel : INotifyPropertyChanged
    {
        public enum TextItemAlignment { Left = 0, Center = 2, Right = 1 };

        [DataContract]
        public class TextItem : INotifyPropertyChanged
        {

            public TextItemAlignment Alignment;


            protected bool SetField<T>(ref T storage, T value, [CallerMemberName] string prop = null)
            {
                if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
                storage = value;
                OnPropertyChanged(prop);
                return true;
            }

            private string _text = string.Empty;
            [DataMember(Order = 0)] public string Text { get { return _text; } set { _text = value; OnPropertyChanged(); } }

            private int _hPos;
            [DataMember(Order = 1)] public int HPos { get => _hPos; set => SetField(ref _hPos, value); }

            private int _vPos;
            [DataMember(Order = 2)] public int VPos { get => _vPos; set => SetField(ref _vPos, value); }

            private TextAlignment _textAlignment = TextAlignment.LEFT; // "Left", "Center", "Right"
            [DataMember(Order = 3)] public TextAlignment TextAlignment { get { return _textAlignment; } set { _textAlignment = value; OnPropertyChanged(); } }

   
            // ===== Composite font string from DB =====
            private string _font = "Segoe UI, 12";
            [DataMember(Order = 4)]
            public string Font
            {
                get => _font;
                set
                {
                    if (value == _font) return;
                    _font = value ?? "";
                    OnPropertyChanged();      // notify Font changed
                                              // Parse into parts without causing loops:
                    ParseFontString(_font, out var fam, out var bold, out var italic, out var size);
                    _updatingFromComposite = true;
                    try
                    {
                        // Only set if changed to avoid redundant events
                        FontFamilyName = fam;
                        IsBold = bold;
                        IsItalic = italic;
                        FontSize = size;
                    }
                    finally { _updatingFromComposite = false; }
                }
            }

   
            private bool _isBold;
            [DataMember(Order = 5)]
            public bool IsBold
            {
                get => _isBold;
                set
                {
                    if (!SetField(ref _isBold, value)) return;
                    RebuildFontIfNeeded();
                }
            }

            private bool _isItalic;
            [DataMember(Order = 6)]
            public bool IsItalic
            {
                get => _isItalic;
                set
                {
                    if (!SetField(ref _isItalic, value)) return;
                    RebuildFontIfNeeded();
                }
            }

            private double _fontSize = 24.0;
            [DataMember(Order = 7)]
            public double FontSize
            {
                get
                {
                    return _fontSize;
                }
                set
                {
                    if (!SetField(ref _fontSize, value)) return;
                    RebuildFontIfNeeded();
                }
            }

            // In FieldVM:
            [DataMember(Order = 8)]
            public string FontFamilyName
            {
                 get { return _fontFamilyName; }
                 set
                {
                    if (!SetField(ref _fontFamilyName, value)) return;
                    RebuildFontIfNeeded();
                }
            }
            
            private string _fontFamilyName = "Segoe UI"; // Default

            // ===== Loop prevention =====
            private bool _updatingFromComposite;   // set when parsing Font->parts
            private bool _updatingComposite;       // set when composing parts->Font

            private void RebuildFontIfNeeded()
            {
                if (_updatingFromComposite) return; // don't echo while parsing

                var composed = ComposeFontString(FontFamilyName, IsBold, IsItalic, FontSize);
                if (string.Equals(composed, _font, System.StringComparison.Ordinal)) return;

                _updatingComposite = true;
                try
                {
                    _font = composed;
                    OnPropertyChanged(nameof(Font)); // raise once after rebuild
                }
                finally { _updatingComposite = false; }
            }


            // ===== Parsing/composing helpers =====
            private static void ParseFontString(string s, out string family, out bool bold, out bool italic, out double sizePt)
            {
                family = "Segoe UI"; bold = false; italic = false; sizePt = 12.0;
                if (string.IsNullOrWhiteSpace(s)) return;

                var tokens = s.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0) return;

                family = tokens[0].Trim();
                for (int i = 1; i < tokens.Length; i++)
                {
                    var t = tokens[i].Trim();
                    if (t.Length == 0) continue;

                    // styles
                    if (t.Equals("fett", System.StringComparison.OrdinalIgnoreCase)) { bold = true; continue; }
                    if (t.Equals("kursiv", System.StringComparison.OrdinalIgnoreCase)) { italic = true; continue; }

                    // size: accept "12", "12pt"
                    if (double.TryParse(t.TrimEnd().TrimEnd('p', 't'),
                            NumberStyles.Float, CultureInfo.InvariantCulture, out var n))
                    {
                        sizePt = n;
                    }
                }
            }

            private static string ComposeFontString(string family, bool bold, bool italic, double sizePt)
            {
                var sb = new StringBuilder();
                sb.Append(string.IsNullOrWhiteSpace(family) ? "Segoe UI" : family.Trim());
                if (bold) sb.Append(", fett");
                if (italic) sb.Append(", kursiv");
                sb.Append(", ").Append(sizePt.ToString("0.###", CultureInfo.InvariantCulture));

                return sb.ToString();
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged([CallerMemberName] string n = null)
            { var h = PropertyChanged; if (h != null) h(this, new PropertyChangedEventArgs(n)); }

        }

         private ObservableCollection<TextItem> _textItems;
         public ObservableCollection<TextItem> TextItems { get { return _textItems; } set { _textItems = value; OnPropertyChanged(); } }


        public PrintCertificateModel()
        {
            TextItems = new ObservableCollection<TextItem>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string n = null)
        { var h = PropertyChanged; if (h != null) h(this, new PropertyChangedEventArgs(n)); }

    }


  public class CertificatesUtils
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

        /// <summary>
        /// For converting later to System.Windows.TextAllignment
        /// </summary>
        /// <param name="al"></param>
        /// <returns></returns>
        public static int mapAlignmentInt(TextItemAlignment al)
        {
            switch (al)
            {
                case TextItemAlignment.Left: return (int)TextAlignment.LEFT;
                case TextItemAlignment.Right: return (int)TextAlignment.RIGHT;
                case TextItemAlignment.Center: return (int)TextAlignment.CENTER;
            }
            return (int)TextAlignment.LEFT;
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


    public static string MapFontFamily(string font)
    {
        var fontParts = font.Split(',');
        fontParts = Array.ConvertAll(fontParts, (f) => f.Trim());

        try
        {
            return fontParts[0];
        }


        catch (Exception)
        {
            return "Arial";
        }
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

    public int MaxCertificatesPerGroup
    {
      get { return _maxCertificatesPerGroup; }
      set { _maxCertificatesPerGroup = value; }
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
