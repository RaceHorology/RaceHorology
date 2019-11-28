using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Layout;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{

  public interface IPDFReport
  {
    void Generate(string filePath);

    string ProposeFilePath();
  }




  public class PDFHelper
  {
    AppDataModel _dm;

    protected List<string> resourcePaths;

    public PDFHelper(AppDataModel dm)
    {
      _dm = dm;

      calcResourcePaths();
    }


    static public Color ColorRHFG1 { get; } = new DeviceRgb(0x3f, 0x43, 0x4b);
    static public Color ColorRHFG2 { get; } = new DeviceRgb(0x66, 0x75, 0x83);
    static public Color ColorRHFG3 { get; } = new DeviceRgb(0x8b, 0x9f, 0xaf);
    static public Color ColorRHFG4 { get; } = new DeviceRgb(0xb8, 0xc9, 0xd6);
    static public Color ColorRHBG1 { get; } = new DeviceRgb(0xf8, 0xfa, 0xf7);
    static public Color ColorRHBG2 { get; } = new DeviceRgb(0xef, 0xf5, 0xf0);
    static public Color ColorRHBG3 { get; } = new DeviceRgb(0xea, 0xf1, 0xea);
    static public Color ColorRHBG4 { get; } = new DeviceRgb(0xe6, 0xee, 0xe5);

    static public float SolidBorderThick { get; } = 0.5F;
    static public float SolidBorderThin { get; } = 0.1F;


    public Image GetImage(string filenameWOExt)
    {
      Image img = null;

      string imgPath = findImage(filenameWOExt);
      if (!string.IsNullOrEmpty(imgPath))
        img = new Image(ImageDataFactory.Create(imgPath));
      //iText.Svg.Converter.SvgConverter.
      return img;
    }


    void calcResourcePaths()
    {
      List<string> paths = new List<string>();
      paths.Add(_dm.GetDB().GetDBPathDirectory());
      paths.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"resources\pdf"));

      resourcePaths = paths;
    }


    string findImage(string filenameWOExt)
    {
      foreach (var resDir in resourcePaths)
      {
        try
        {
          var files = Directory.GetFiles(resDir, string.Format("{0}.*", filenameWOExt));
          if (files.Length > 0)
            return files[0];
        }
        catch (System.IO.DirectoryNotFoundException)
        {
          continue;
        }
      }
      return null;
    }

  }



  public struct Margins
  {
    public float Top;
    public float Left;
    public float Right;
    public float Bottom;
  }



  class ReportHeader : IEventHandler
  {
    PdfDocument _pdfDoc;
    Document _doc;
    PDFHelper _pdfHelper;
    Race _race;
    string _listName;
    Margins _pageMargins;

    string _header1;
    bool _debugAreas = false;
    float _height = 110;
    Image _banner;
    float _bannerHeight = 0F;

    Image _logo1;
    Image _logo2;
    Image _logoRH;


    public ReportHeader(PdfDocument pdfDoc, Document doc, PDFHelper pdfHelper, Race race, string listName, Margins pageMargins)
    {
      _pdfDoc = pdfDoc;
      _doc = doc;
      _pdfHelper = pdfHelper;
      _race = race;
      _listName = listName;
      _pageMargins = pageMargins;

      var pageSize = PageSize.A4; // Assumption

      _banner = _pdfHelper.GetImage("Banner1");
      if (_banner != null)
        _bannerHeight = (pageSize.GetWidth() - _pageMargins.Left - _pageMargins.Right) * _banner.GetImageHeight() / _banner.GetImageWidth();

      _logo1 = _pdfHelper.GetImage("Logo1");
      //if (_logo1 != null)
      //  _bannerHeight = (pageSize.GetWidth() - _pageMargins.Left - _pageMargins.Right) * _logo1.GetImageHeight() / _logo1.GetImageWidth();
      _logo2 = _pdfHelper.GetImage("Logo2");
      //if (_logo2 != null)
      //  _bannerHeight = (pageSize.GetWidth() - _pageMargins.Left - _pageMargins.Right) * _logo2.GetImageHeight() / _logo2.GetImageWidth();
      _logoRH = _pdfHelper.GetImage("LogoRHShort");

      calculateHeader();
      calculateHeight();
    }


    public float Height { get { return _height + 2 + 2; } }


    private void calculateHeight()
    {

      Table tableHeader = createHeaderTable();

      var pageSize = PageSize.A4; // Assumption
      float tableWidth = pageSize.GetWidth() - _pageMargins.Left - _pageMargins.Right;
      var result = tableHeader.CreateRendererSubTree().SetParent(_doc.GetRenderer()).Layout(new LayoutContext(new LayoutArea(1, new Rectangle(0, 0, tableWidth, 10000.0F))));
      float tableHeight = result.GetOccupiedArea().GetBBox().GetHeight();

      _height = _bannerHeight + tableHeight + 7;
    }


    private void calculateHeader()
    {
      _header1 = _race.Description + "\n\n" + _listName;
    }

    public virtual void HandleEvent(Event @event)
    {
      PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
      PdfDocument pdfDoc = docEvent.GetDocument();
      PdfPage page = docEvent.GetPage();

      Rectangle pageSize = page.GetPageSize();
      PdfCanvas pdfCanvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);

      // Header
      if (_banner != null)
      {
        Rectangle areaBanner = new Rectangle(
          pageSize.GetLeft() + _pageMargins.Left, pageSize.GetTop() - _pageMargins.Top - _bannerHeight,
          pageSize.GetWidth() - _pageMargins.Left - _pageMargins.Right, _bannerHeight);
        Canvas canvas = new Canvas(pdfCanvas, pdfDoc, areaBanner).Add(_banner);

        if (_debugAreas)
          pdfCanvas.SetStrokeColor(ColorConstants.RED)
                   .SetLineWidth(0.5f)
                   .Rectangle(areaBanner)
                   .Stroke();
      }


      Table tableHeader = createHeaderTable();

      float tableWidth = pageSize.GetWidth() - _pageMargins.Left - _pageMargins.Right;
      var result = tableHeader.CreateRendererSubTree().SetParent(_doc.GetRenderer()).Layout(new LayoutContext(new LayoutArea(1, new Rectangle(0, 0, tableWidth, 10000.0F))));
      float tableHeight = result.GetOccupiedArea().GetBBox().GetHeight();

      Rectangle rectTable = new Rectangle(
        pageSize.GetLeft() + _pageMargins.Left, pageSize.GetTop() - _pageMargins.Top - _bannerHeight - tableHeight,
        tableWidth, tableHeight);

      new Canvas(pdfCanvas, pdfDoc, rectTable)
              .Add(tableHeader
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA)).SetFontSize(10)
                );

      pdfCanvas.Release();
      return;
    }

    Table createHeaderTable()
    {
      float[] cols = { 15.0F, 70.0F, 15.0F };
      Table tableHeader = new Table(UnitValue.CreatePercentArray(cols));
      tableHeader.SetWidth(UnitValue.CreatePercentValue(100));
        //.SetPaddingBottom(0)
        //.SetMarginBottom(0);

      float padding = 1F;
      float maxHeightCol1 = 56.0F;
      float maxHeightCol2 = 30.0F;
      var fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
      var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
      var fontTitle = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
      int fontSizeTitle = 16;
      int fontSizeNormal = 10;


      if (_logo1 != null)
        tableHeader.AddCell(new Cell()
          .SetTextAlignment(TextAlignment.LEFT)
          .SetVerticalAlignment(VerticalAlignment.MIDDLE)
          .SetMaxHeight(maxHeightCol1)
          .SetBorder(Border.NO_BORDER)
          .SetBorderTop(new SolidBorder(PDFHelper.SolidBorderThick))
          .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin))
          .SetPadding(padding)
          .SetFont(fontBold)
          .Add(_logo1.SetAutoScale(true)));
      else
        tableHeader.AddCell(new Cell()
          .SetMaxHeight(maxHeightCol1)
          .SetBorder(Border.NO_BORDER)
          .SetBorderTop(new SolidBorder(PDFHelper.SolidBorderThick))
          .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin)));


      if (!string.IsNullOrEmpty(_race.Description))
        // Race Titles
        tableHeader.AddCell(new Cell()
          .SetTextAlignment(TextAlignment.CENTER)
          .SetVerticalAlignment(VerticalAlignment.MIDDLE)
          .SetMaxHeight(maxHeightCol1)
          .SetBorder(Border.NO_BORDER)
          .SetBorderTop(new SolidBorder(PDFHelper.SolidBorderThick))
          .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin))
          .SetPadding(padding)
          .SetFont(fontTitle)
          .SetFontSize(fontSizeTitle)
          .Add(new Paragraph(_race.Description)));
      else
        tableHeader.AddCell(new Cell()
          .SetMaxHeight(maxHeightCol1)
          .SetBorder(Border.NO_BORDER)
          .SetBorderTop(new SolidBorder(PDFHelper.SolidBorderThick))
          .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin)));

      if (_logoRH != null)
        tableHeader.AddCell(new Cell()
          .SetTextAlignment(TextAlignment.RIGHT)
          .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
          .SetVerticalAlignment(VerticalAlignment.MIDDLE)
          .SetMaxHeight(maxHeightCol1)
          .SetBorder(Border.NO_BORDER)
          .SetBorderTop(new SolidBorder(PDFHelper.SolidBorderThick))
          .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin))
          .SetPadding(padding)
          .SetFont(fontBold)
          .Add(_logoRH.SetAutoScale(true)));
      else
        tableHeader.AddCell(new Cell()
          .SetMaxHeight(maxHeightCol1)
          .SetBorder(Border.NO_BORDER)
          .SetBorderTop(new SolidBorder(PDFHelper.SolidBorderThick))
          .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin)));


      // Second row
      if (_logo2 != null)
        tableHeader.AddCell(new Cell()
          .SetTextAlignment(TextAlignment.LEFT)
          .SetVerticalAlignment(VerticalAlignment.MIDDLE)
          .SetMaxHeight(maxHeightCol2)
          .SetBorder(Border.NO_BORDER)
          .SetBorderBottom(new SolidBorder(PDFHelper.SolidBorderThick))
          .SetPadding(padding)
          .SetFont(fontBold)
          .Add(_logo2.SetAutoScale(true)));
      else
        tableHeader.AddCell(new Cell()
          .SetMaxHeight(maxHeightCol2)
          .SetBorder(Border.NO_BORDER)
          .SetBorderBottom(new SolidBorder(PDFHelper.SolidBorderThick)));

      // List Name
      tableHeader.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.CENTER)
        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
        .SetMaxHeight(maxHeightCol2)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new SolidBorder(PDFHelper.SolidBorderThick))
        .SetPadding(padding)
        .SetFont(fontTitle)
        .SetFontSize(fontSizeTitle)
        .Add(new Paragraph(_listName)));

      // Race Date & Time
      tableHeader.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.RIGHT)
        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
        .SetMaxHeight(maxHeightCol2)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new SolidBorder(PDFHelper.SolidBorderThick))
        .SetPadding(padding)
        .SetFont(fontNormal)
        .SetFontSize(fontSizeNormal)
        .Add(new Paragraph(_race.DateStart.ToString())));

      return tableHeader;
    }
  }



  class ReportFooter : IEventHandler
  {
    PdfDocument _pdfDoc;
    Document _doc;
    PDFHelper _pdfHelper;
    Race _race;
    string _listName;
    Margins _pageMargins;


    string _footerVersion;
    string _footerWebsite;
    string _footerCopyright;
    bool _debugAreas = false;
    float _height = 110;
    Image _banner;
    float _bannerHeight = 0F;
    Image _logoRH;

    public ReportFooter(PdfDocument pdfDoc, Document doc, PDFHelper pdfHelper, Race race, string listName, Margins pageMargins)
    {
      _pdfDoc = pdfDoc;
      _doc = doc;
      _pdfHelper = pdfHelper;
      _race = race;
      _listName = listName;
      _pageMargins = pageMargins;

      var pageSize = PageSize.A4; // Assumption

      _banner = _pdfHelper.GetImage("Banner2");
      if (_banner!=null)
        _bannerHeight = (pageSize.GetWidth() - _pageMargins.Left - _pageMargins.Right) * _banner.GetImageHeight() / _banner.GetImageWidth();
      _logoRH = _pdfHelper.GetImage("LogoRH");

      calculateFooter();
      calculateHeight();
    }

    public float Height { get { return _height; } }


    private void calculateHeight()
    {

      Table tableFooter = createFooterTable(0);

      var pageSize = PageSize.A4; // Assumption
      float tableWidth = pageSize.GetWidth() - _pageMargins.Left - _pageMargins.Right;
      var result = tableFooter.CreateRendererSubTree().SetParent(_doc.GetRenderer()).Layout(new LayoutContext(new LayoutArea(1, new Rectangle(0, 0, tableWidth, 10000.0F))));
      float tableHeight = result.GetOccupiedArea().GetBBox().GetHeight();

      _height = _bannerHeight + tableHeight + 7;
    }


    private void calculateFooter()
    {
      Assembly assembly = Assembly.GetEntryAssembly();
      if (assembly == null)
        assembly = Assembly.GetExecutingAssembly();

      if (assembly != null)
      {
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        var companyName = fvi.CompanyName;
        var productName = fvi.ProductName;
        var copyrightYear = fvi.LegalCopyright;
        var productVersion = fvi.ProductVersion;

        _footerVersion = productVersion;
        _footerWebsite = "www.race-horology.com";
        _footerCopyright = string.Format("{1} by {2}\nVersion {0}", productVersion, copyrightYear, companyName);
      }
      else
        _footerVersion = _footerWebsite = _footerCopyright = "";

    }


    public virtual void HandleEvent(Event @event)
    {
      PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
      PdfDocument pdfDoc = docEvent.GetDocument();
      PdfPage page = docEvent.GetPage();

      int pageNumber = pdfDoc.GetPageNumber(page);
      Rectangle pageSize = page.GetPageSize();
      PdfCanvas pdfCanvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);

      // Footer
      if (_banner != null)
      {
        Rectangle area3 = new Rectangle(
          pageSize.GetLeft() + _pageMargins.Left, pageSize.GetBottom() + _pageMargins.Bottom, 
          pageSize.GetWidth() - _pageMargins.Left - _pageMargins.Right, _bannerHeight);
        Canvas canvas = new Canvas(pdfCanvas, pdfDoc, area3).Add(_banner);

        if (_debugAreas)
          pdfCanvas.SetStrokeColor(ColorConstants.RED)
                   .SetLineWidth(0.5f)
                   .Rectangle(area3)
                   .Stroke();
      }


      Table tableFooter = createFooterTable(pageNumber);

      float tableWidth = pageSize.GetWidth() - _pageMargins.Left - _pageMargins.Right;
      var result = tableFooter.CreateRendererSubTree().SetParent(_doc.GetRenderer()).Layout(new LayoutContext(new LayoutArea(1, new Rectangle(0, 0, tableWidth, 10000.0F))));
      float tableHeight = result.GetOccupiedArea().GetBBox().GetHeight();

      Rectangle rectTable = new Rectangle(
        pageSize.GetLeft() + _pageMargins.Left, pageSize.GetBottom() + _pageMargins.Bottom + _bannerHeight,
        tableWidth, tableHeight);

      new Canvas(pdfCanvas, pdfDoc, rectTable)
              .Add(tableFooter
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA)).SetFontSize(10)
                );
      
      //pdfCanvas.AddXObject(_pagesPlaceholder)

      

      pdfCanvas.Release();
    }


    Table createFooterTable(int pageNumber)
    {
      Table tableFooter = new Table(UnitValue.CreatePercentArray(new float[]{2.0F, 3.0F, 2.0F}));
      tableFooter.SetWidth(UnitValue.CreatePercentValue(100))
        .SetPaddingBottom(0)
        .SetMarginBottom(0);

      float padding = 1F;
      var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

      Paragraph parPage = new Paragraph(string.Format("Seite {0}", pageNumber));
      tableFooter.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.LEFT)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        .SetPadding(padding)
        .SetFont(fontBold)
        .Add(new Paragraph(DateTime.Now.ToString(@"dd.MM.yyyy"))));
      tableFooter.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.CENTER)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        .SetPadding(padding)
        .SetFont(fontBold)
        .Add(parPage));
      tableFooter.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.RIGHT)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        .SetPadding(padding)
        .SetFont(fontBold)
        .Add(new Paragraph(string.Format("Bewerbsnummer: {0}", "12345"))));


      float middleHeight = 35.0F;
      tableFooter.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.LEFT)
        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
        .SetMaxHeight(middleHeight)
        .SetBackgroundColor(PDFHelper.ColorRHBG1)
        .SetMarginTop(5)
        .SetMarginBottom(5)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        .SetPadding(padding)
        .Add(_logoRH.SetMaxHeight(16.0F)));
        //.Add(new Paragraph(_footerVersion)));
      tableFooter.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.CENTER)
        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
        .SetMaxHeight(middleHeight)
        .SetBackgroundColor(PDFHelper.ColorRHBG1)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        .SetPadding(padding)
        .Add(new Paragraph(_footerWebsite)));
      tableFooter.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.RIGHT)
        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
        .SetMaxHeight(middleHeight)
        .SetBackgroundColor(PDFHelper.ColorRHBG1)
        .SetMarginTop(0)
        .SetMarginBottom(0)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        .SetPadding(padding)
        .Add(new Paragraph(_footerCopyright).SetFontSize(6.0F)));


      tableFooter.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.LEFT)
        .SetBorder(Border.NO_BORDER)
        .SetPadding(padding)
        .SetFont(fontBold)
        .Add(new Paragraph(string.Format("Ausdruck: {0}", DateTime.Now.ToString()))));

      tableFooter.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.CENTER)
        .SetBorder(Border.NO_BORDER)
        .SetPadding(padding)
        .SetFont(fontBold)
        .Add(new Paragraph(string.Format("Auswertung: {0}", _race.AdditionalProperties.Analyzer))));

      tableFooter.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.RIGHT)
        .SetBorder(Border.NO_BORDER)
        .SetPadding(padding)
        .SetFont(fontBold)
        .Add(new Paragraph(string.Format("Timing: {0}", "Alge TdC8001"))));

      return tableFooter;
    }
  }


  class PageXofY : IEventHandler
  {
    protected PdfFormXObject placeholder;
    protected float side = 20;
    protected float x = 300;
    protected float y = 25;
    protected float space = 4.5f;
    protected float descent = 3;

    public PageXofY(PdfDocument pdf)
    {
      placeholder = new PdfFormXObject(new Rectangle(0, 0, side, side));
    }

    public void HandleEvent(Event @event)
    {
      PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
      PdfDocument pdfDoc = docEvent.GetDocument();
      PdfPage page = docEvent.GetPage();

      int pageNumber = pdfDoc.GetPageNumber(page);
      Rectangle pageSize = page.GetPageSize();
      PdfCanvas pdfCanvas = new PdfCanvas(page.GetLastContentStream(), page.GetResources(), pdfDoc);
      Canvas canvas = new Canvas(pdfCanvas, pdfDoc, pageSize);
      Paragraph p = new Paragraph()
          .Add(string.Format("{0}/", pageNumber));

      canvas.ShowTextAligned(p, x, y, TextAlignment.RIGHT);
      pdfCanvas.AddXObject(placeholder, x + space, y - descent);
      pdfCanvas.Release();
    }

    public void WriteTotal(PdfDocument pdfDoc)
    {
      Canvas canvas = new Canvas(placeholder, pdfDoc);
      canvas.ShowTextAligned(pdfDoc.GetNumberOfPages().ToString(), 0, descent, TextAlignment.LEFT);
    }
  }






public abstract class PDFReport : IPDFReport
  {
    protected Race _race;

    protected AppDataModel _dm;
    protected PDFHelper _pdfHelper;

    protected PositionConverter _positionConverter = new PositionConverter();

    public PDFReport(Race race)
    {
      _race = race;

      _dm = race.GetDataModel();
      _pdfHelper = new PDFHelper(_dm);
    }

    protected abstract string getTitle();

    protected abstract ICollectionView getView();
    protected abstract float[] getTableColumnsWidths();
    protected abstract void addHeaderToTable(Table table);
    protected abstract void addLineToTable(Table table, string group);
    protected abstract void addLineToTable(Table table, object data, int i=0);
    protected abstract string getReportName();

    public virtual string ProposeFilePath()
    {
      string path;

      path = System.IO.Path.Combine(
        _dm.GetDB().GetDBPathDirectory(),
        System.IO.Path.GetFileNameWithoutExtension(_dm.GetDB().GetDBFileName()) + " - " + getReportName() + " - " + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf"
      );

      return path;
    }

    public void Generate(string filePath)
    {
      var writer = new PdfWriter(filePath);
      var pdf = new PdfDocument(writer);

      Margins pageMargins = new Margins { Top = 24.0F, Bottom = 24.0F, Left = 24.0F, Right = 24.0F };

      var document = new Document(pdf, PageSize.A4);

      var header = new ReportHeader(pdf, document, _pdfHelper, _race, getTitle(), pageMargins);
      var footer = new ReportFooter(pdf, document, _pdfHelper, _race, getTitle(), pageMargins);

      pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, header);
      pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, footer);
      //var pageXofY = new PageXofY(pdf);
      //pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, pageXofY);

      document.SetMargins(header.Height + pageMargins.Top, pageMargins.Right, pageMargins.Bottom + footer.Height, pageMargins.Left);

      addContent(pdf, document);

      //pageXofY.WriteTotal(pdf);
      document.Close();
    }

    protected virtual void addContent(PdfDocument pdf, Document document)
    {
      Table raceProperties = getRacePropertyTable();
      if (raceProperties != null)
        document.Add(raceProperties);

      Table table = getResultsTable();
      document.Add(table);
    }


    protected Table getRacePropertyTable()
    {
      var fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
      var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

      Cell createCell(int rs=1, int cs=1)
      {
        return new Cell(rs, cs)
          .SetPaddingTop(0)
          .SetPaddingBottom(0)
          .SetPaddingLeft(4)
          .SetPaddingRight(4)
          .SetVerticalAlignment(VerticalAlignment.BOTTOM)
          .SetBorder(Border.NO_BORDER);
      }

      string stringOrEmpty(string s)
      {
        if (string.IsNullOrEmpty(s))
          return "";
        return s;
      }

      var table = new Table(new float[] { 1, 1, 1, 1, 1 })
        .SetFontSize(10)
        .SetFont(fontNormal);

      table.SetWidth(UnitValue.CreatePercentValue(100));
      table.SetBorder(Border.NO_BORDER);

      table.AddCell(createCell()
        .Add(new Paragraph("Organisator:")
          .SetPaddingTop(6)
          .SetFont(fontBold)));

      table.AddCell(createCell(1,4)
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.Organizer))
          .SetPaddingTop(6)
          .SetFont(fontBold)));

      table.AddCell(createCell(1,3)
        .Add(new Paragraph("KAMPGERICHT")
          .SetPaddingTop(6)
          .SetFont(fontBold)));

      table.AddCell(createCell(1,2)
        //.SetBorder(Border.NO_BORDER)
        .Add(new Paragraph("TECHNISCHE DATEN")
          .SetPaddingTop(6)
          .SetFont(fontBold)));

      table.AddCell(createCell()
        .Add(new Paragraph("Schiedrichter:")
          .SetPaddingTop(6)
          .SetFont(fontBold)));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceDirector.Name))
          .SetPaddingTop(6)));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceDirector.Club))
          .SetPaddingTop(6)));
      table.AddCell(createCell()
        .Add(new Paragraph("Streckenname:")
          .SetPaddingTop(6)
          .SetFont(fontBold)));
      table.AddCell(createCell()
        .SetTextAlignment(TextAlignment.RIGHT)
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.CoarseName))
          .SetPaddingTop(6)));

      table.AddCell(createCell()
        .Add(new Paragraph("Rennleiter:")
          .SetFont(fontBold)));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceManager.Name))));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceManager.Club))));
      table.AddCell(createCell()
        .Add(new Paragraph("Start:")
          .SetFont(fontBold)));
      table.AddCell(createCell()
        .SetTextAlignment(TextAlignment.RIGHT)
        .Add(new Paragraph(_race.AdditionalProperties.StartHeight > 0 ? string.Format("{0} m", _race.AdditionalProperties.StartHeight):"")));

      table.AddCell(createCell()
        .Add(new Paragraph("Trainervertreter:")
          .SetFont(fontBold)));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.TrainerRepresentative.Name))));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.TrainerRepresentative.Club))));
      table.AddCell(createCell()
        .Add(new Paragraph("Ziel:")
          .SetFont(fontBold)));
      table.AddCell(createCell()
        .SetTextAlignment(TextAlignment.RIGHT)
        .Add(new Paragraph(_race.AdditionalProperties.FinishHeight > 0 ? string.Format("{0} m", _race.AdditionalProperties.FinishHeight):"")));

      table.AddCell(createCell(1,3));
      table.AddCell(createCell()
        .Add(new Paragraph("Höhendifferenz:")
          .SetFont(fontBold)));
      table.AddCell(createCell()
        .SetTextAlignment(TextAlignment.RIGHT)
        .Add(new Paragraph((_race.AdditionalProperties.StartHeight - _race.AdditionalProperties.FinishHeight) > 0 ? string.Format("{0} m", _race.AdditionalProperties.StartHeight - _race.AdditionalProperties.FinishHeight) : "")));

      table.AddCell(createCell(1, 3));
      table.AddCell(createCell()
        .Add(new Paragraph("Streckenlänge:")
          .SetFont(fontBold)));
      table.AddCell(createCell()
        .SetTextAlignment(TextAlignment.RIGHT)
        .Add(new Paragraph(_race.AdditionalProperties.CoarseLength > 0 ? string.Format("{0} m", _race.AdditionalProperties.CoarseLength) : "")));

      table.AddCell(createCell(1, 3));
      table.AddCell(createCell()
        .Add(new Paragraph("Homolog Nr.:")
          .SetFont(fontBold)));
      table.AddCell(createCell()
        .SetTextAlignment(TextAlignment.RIGHT)
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.CoarseHomologNo))));

      table.AddCell(createCell(1, 1));
      table.AddCell(createCell(1, 2)
        .Add(new Paragraph("1. Durchgang")
          .SetPaddingTop(12)
          .SetFont(fontBold)));
      table.AddCell(createCell(1, 2)
        .Add(new Paragraph("2. Durchgang")
          .SetPaddingTop(12)
          .SetFont(fontBold)));

      table.AddCell(createCell()
        .Add(new Paragraph("Kurssetzer:")
          .SetPaddingTop(6)
          .SetFont(fontBold)));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun1.CoarseSetter.Name))));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun1.CoarseSetter.Club))));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun2.CoarseSetter.Name))));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun2.CoarseSetter.Club))));

      table.AddCell(createCell()
        .Add(new Paragraph("Tore / R.-Änder.:")
          .SetPaddingTop(6)
          .SetFont(fontBold)));
      table.AddCell(createCell(1, 2)
        .Add(new Paragraph(
          _race.AdditionalProperties.RaceRun1.Gates > 0 && _race.AdditionalProperties.RaceRun1.Turns > 0
          ? string.Format("{0} / {1}", _race.AdditionalProperties.RaceRun1.Gates, _race.AdditionalProperties.RaceRun1.Turns)
          : "")));
      table.AddCell(createCell(1, 2)
        .Add(new Paragraph(
          _race.AdditionalProperties.RaceRun2.Gates > 0 && _race.AdditionalProperties.RaceRun2.Turns > 0
          ? string.Format("{0} / {1}", _race.AdditionalProperties.RaceRun2.Gates, _race.AdditionalProperties.RaceRun2.Turns)
          : "")));

      table.AddCell(createCell()
        .Add(new Paragraph("Vorläufer:")
          .SetPaddingTop(6)
          .SetFont(fontBold)));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun1.Forerunner1.Name))));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun1.Forerunner1.Club))));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun2.Forerunner1.Name))));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun2.Forerunner1.Club))));

      table.AddCell(createCell());
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun1.Forerunner2.Name))));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun1.Forerunner2.Club))));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun2.Forerunner2.Name))));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun2.Forerunner2.Club))));

      table.AddCell(createCell());
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun1.Forerunner3.Name))));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun1.Forerunner3.Club))));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun2.Forerunner3.Name))));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun2.Forerunner3.Club))));

      table.AddCell(createCell()
        .Add(new Paragraph("Startzeit:")
          .SetPaddingTop(6)
          .SetFont(fontBold)));
      table.AddCell(createCell(1, 2)
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun1.StartTime))));
      table.AddCell(createCell(1, 2)
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceRun2.StartTime))));

      table.AddCell(createCell(1, 5)
        .SetPaddingTop(12)
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick)));
      table.AddCell(createCell(1, 5)
        .SetPaddingTop(12));

      return table;
    }


    protected virtual Table getResultsTable()
    {
      var table = new Table(getTableColumnsWidths());

      table.SetWidth(UnitValue.CreatePercentValue(100));
      table.SetBorder(Border.NO_BORDER);

      addHeaderToTable(table);

      var results = getView();
      var lr = results as System.Windows.Data.ListCollectionView;
      if (results.Groups != null)
      {
        foreach (var group in results.Groups)
        {
          System.Windows.Data.CollectionViewGroup cvGroup = group as System.Windows.Data.CollectionViewGroup;
          addLineToTable(table, cvGroup.Name.ToString());

          int i = 0;
          foreach (var result in cvGroup.Items)
            addLineToTable(table, result, i++);
        }
      }
      else
      {
        int i = 0;
        foreach (var result in results.SourceCollection)
          addLineToTable(table, result, i++);
      }

      return table;
    }


    protected Paragraph createCellParagraphForTable(string text)
    {
      var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

      return new Paragraph(text)
        .SetFont(font)
        .SetFontSize(9)
        .SetPaddingTop(0)
        .SetPaddingBottom(0)
        .SetPaddingLeft(0)
        .SetPaddingRight(0);
    }


    protected Cell createCellForTable(TextAlignment? textAlignment = TextAlignment.LEFT)
    {
      return new Cell()
        .SetBorder(Border.NO_BORDER)
        .SetPaddingTop(0)
        .SetPaddingBottom(0)
        .SetPaddingLeft(4)
        .SetPaddingRight(4)
        .SetTextAlignment(textAlignment);
    }


    protected string formatPoints(double points)
    {
      if (points < 0.0)
        return "---";

      return string.Format("{0:0.00}", points);
    }

    protected string formatStartNumber(uint startNumber)
    {
      if (startNumber < 1)
        return "---";

      return startNumber.ToString();
    }



  }



  public class StartListReport : PDFReport
  {
    RaceRun _raceRun;

    public StartListReport(RaceRun rr) : base(rr.GetRace())
    {
      _raceRun = rr;

      //dm.Location;

      //race.Description;
      //race.DateResult;


    }


    protected override string getReportName()
    {
      return string.Format("Startliste {0}. Durchgang", _raceRun.Run);
    }


    protected override string getTitle()
    {
      return string.Format("STARTLISTE {0}. Durchgang", _raceRun.Run);
    }


    protected override ICollectionView getView()
    {
      return _raceRun.GetStartListProvider().GetView();
    }

    protected override float[] getTableColumnsWidths()
    {
      return new float[] { 1, 1, 1, 1, 1, 1, 1, 1 };
    }

    protected override void addHeaderToTable(Table table)
    {

      var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
      Paragraph createParagraph(string text)
      {
        return new Paragraph(text).SetFont(font).SetFontSize(10);
      }

      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT).Add(createParagraph("Stnr")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT).Add(createParagraph("Code")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT).Add(createParagraph("Teilnehmer")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT).Add(createParagraph("JG")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT).Add(createParagraph("VB")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT).Add(createParagraph("Verein")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT).Add(createParagraph("Punkte")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT).Add(createParagraph("Laufzeit")));
    }


    protected override void addLineToTable(Table table, string group)
    {
      table.AddCell(new Cell(1, 1)
        .SetBorder(Border.NO_BORDER));

      table.AddCell(new Cell(1, 7)
        .SetBorder(Border.NO_BORDER)
        .Add(new Paragraph(group)
          .SetPaddingTop(6)
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetFontSize(10)));
    }


    protected override void addLineToTable(Table table, object data, int i = 0)
    {
      StartListEntry rrwp = data as StartListEntry;
      if (rrwp == null)
        return;

      Color bgColor = ColorConstants.WHITE;// new DeviceRgb(0.97f, 0.97f, 0.97f);
      if (i % 2 == 1)
        bgColor = PDFHelper.ColorRHBG1;


      // Startnumber
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(formatStartNumber(rrwp.StartNumber))));
      // Code
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.CodeOrSvId)));
      // Name
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.Fullname)));
      // Year
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", rrwp.Year))));
      // VB
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.Nation)));
      // Club
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Club)));
      // Points
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(formatPoints(rrwp.Points))));
      // Runtime
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).SetBorderBottom(new DottedBorder(1F)));
    }
  }



  public class RaceRunResultReport : PDFReport
  {
    RaceRun _raceRun;

    ResultTimeAndCodeConverter _timeConverter = new ResultTimeAndCodeConverter();

    public RaceRunResultReport(RaceRun rr) : base(rr.GetRace())
    {
      _raceRun = rr;

      //dm.Location;

      //race.Description;
      //race.DateResult;

      
    }


    protected override string getReportName()
    {
      return string.Format("Ergebnis {0}. Durchgang", _raceRun.Run);
    }


    protected override string getTitle()
    {
      return string.Format("ERGEBNISLISTE {0}. Durchgang", _raceRun.Run);
    }


    protected override ICollectionView getView()
    {
      return _raceRun.GetResultViewProvider().GetView();
    }

    protected override float[] getTableColumnsWidths()
    {
      return new float[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
    }

    protected override void addHeaderToTable(Table table)
    {

      var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
      Paragraph createParagraph(string text)
      {
        return new Paragraph(text).SetFont(font).SetFontSize(10);
      }

      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT).Add(createParagraph("Rang")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT).Add(createParagraph("Stnr")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT).Add(createParagraph("Teilnehmer")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT).Add(createParagraph("JG")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT).Add(createParagraph("VB")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT).Add(createParagraph("Verein")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT).Add(createParagraph("Punkte")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT).Add(createParagraph("Laufzeit")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT).Add(createParagraph("Diff")));
    }


    protected override void addLineToTable(Table table, string group)
    {
      table.AddCell(new Cell(1, 2)
        .SetBorder(Border.NO_BORDER));

      table.AddCell(new Cell(1, 7)
        .SetBorder(Border.NO_BORDER)
        .Add(new Paragraph(group)
          .SetPaddingTop(6)
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetFontSize(10)));
    }


    protected override void addLineToTable(Table table, object data, int i=0)
    {
      RunResultWithPosition rrwp = data as RunResultWithPosition;
      if (rrwp == null)
        return;

      Color bgColor = ColorConstants.WHITE;// new DeviceRgb(0.97f, 0.97f, 0.97f);
      if (i % 2 == 1)
        bgColor = PDFHelper.ColorRHBG1;


      // Position
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable((string)_positionConverter.Convert(rrwp.Position, typeof(string), null, null))));
      // Startnumber
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(formatStartNumber(rrwp.StartNumber))));
      //// Code
      //table.AddCell(new Cell().SetBorder(Border.NO_BORDER));
      // Name
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.Fullname)));
      // Year
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", rrwp.Year))));
      // VB
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.Nation)));
      // Club
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Club)));
      // Points
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(formatPoints(-1.0/*TODO: rrwp.Points*/))));

      // Runtime
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor)
        .Add(createCellParagraphForTable((string)_timeConverter.Convert(new object[] { rrwp.Runtime, rrwp.ResultCode }, typeof(string), null, null))));
      // Diff
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", rrwp.DiffToFirst.ToRaceTimeString()))));
    }
  }


  public class RaceResultReport : PDFReport
  {

    ResultTimeAndCodeConverter _timeConverter = new ResultTimeAndCodeConverter();


    public RaceResultReport(Race race) : base(race)
    {
    
    }


    protected override string getReportName()
    {
      return string.Format("Ergebnis Gesamt");
    }

    protected override string getTitle()
    {
      return string.Format("OFFIZIELLE ERGEBNISLISTE");
    }


    protected override ICollectionView getView()
    {
      return _race.GetResultViewProvider().GetView();
    }


    protected override float[] getTableColumnsWidths()
    {

      float[] columns = new float[9 + _race.GetMaxRun()];

      for (int i = 0; i < columns.Length; i++)
        columns[i] = 1F;

      return columns;
    }

    protected override void addHeaderToTable(Table table)
    {

      var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
      Paragraph createParagraph(string text)
      {
        return new Paragraph(text).SetFont(font).SetFontSize(10);
      }

      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT).Add(createParagraph("Rang")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT).Add(createParagraph("Stnr")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT).Add(createParagraph("Teilnehmer")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT).Add(createParagraph("JG")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT).Add(createParagraph("VB")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT).Add(createParagraph("Verein")));

      for (int i = 1; i <= _race.GetMaxRun(); i++)
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT).Add(createParagraph(string.Format("Zeit-{0}", i))));

      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT).Add(createParagraph("Punkte")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT).Add(createParagraph("Laufzeit")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT).Add(createParagraph("Diff")));
    }


    protected override void addLineToTable(Table table, string group)
    {
      table.AddCell(new Cell(1, 2)
        .SetBorder(Border.NO_BORDER));

      table.AddCell(new Cell(1, 7+ _race.GetMaxRun())
        .SetBorder(Border.NO_BORDER)
        .Add(new Paragraph(group)
          .SetPaddingTop(6)
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetFontSize(10)));
    }


    protected override void addLineToTable(Table table, object data, int i=0)
    {
      RaceResultItem item = data as RaceResultItem;
      if (item == null)
        return;

      Color bgColor = ColorConstants.WHITE;// new DeviceRgb(0.97f, 0.97f, 0.97f);
      if (i % 2 == 1)
        bgColor = PDFHelper.ColorRHBG1;

      // Position
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable((string)_positionConverter.Convert(item.Position, typeof(string), null, null))));
      // Startnumber
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", item.Participant.StartNumber))));
      //// Code
      //table.AddCell(new Cell().SetBorder(Border.NO_BORDER));
      // Name
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(item.Participant.Participant.Fullname)));
      // Year
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", item.Participant.Year))));
      // VB
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(item.Participant.Participant.Nation)));
      // Club
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(item.Participant.Club)));

      for (uint j = 1; j <= _race.GetMaxRun(); j++)
      {
        if (item.RunTimes.ContainsKey(j))
        {
          string str = (string)_timeConverter.Convert(new object[] { item.RunTimes[j], item.RunResultCodes[j] }, typeof(string), null, null);
          table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(str)));
        }
        else
          table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor));
      }

      // Points
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(formatPoints(-1.0/*TODO: item.Points*/))));
      // Runtime
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", item.TotalTime.ToRaceTimeString()))));
      // Diff
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", item.DiffToFirst.ToRaceTimeString()))));
    }


    protected override void addContent(PdfDocument pdf, Document document)
    {
      base.addContent(pdf, document);

      addResultsChart(pdf, document);
    }


    protected void addResultsChart(PdfDocument pdf, Document document)
    {
      var page = pdf.AddNewPage();
      
      PdfCanvas canvas = new PdfCanvas(page);

      for (float x = 0; x < page.GetPageSize().GetWidth();)
      {
        for (float y = 0; y < page.GetPageSize().GetHeight();)
        {
          canvas.Circle(x, y, 1f);
          y += 72f;
        }
        x += 72f;
      }
      canvas.Fill();
    }
  }
}
