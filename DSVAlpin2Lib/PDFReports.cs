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


    public Image GetImage(string filenameWOExt)
    {
      Image img = null;

      string imgPath = findImage(filenameWOExt);
      if (!string.IsNullOrEmpty(imgPath))
        img = new Image(ImageDataFactory.Create(imgPath));

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
          var files = Directory.GetFiles(resDir, string.Format("{0}*", filenameWOExt));
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
    PDFHelper _pdfHelper;
    Race _race;
    string _listName;
    Margins _pageMargins;

    string _header1;
    bool _debugAreas = false;
    float _height = 110;


    public ReportHeader(PDFHelper pdfHelper, Race race, string listName, Margins pageMargins)
    {
      _pdfHelper = pdfHelper;
      _race = race;
      _listName = listName;
      _pageMargins = pageMargins;

      calculateHeader();
    }


    public float Height { get { return _height + 2 + 2; } }


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

      Image logo1 = _pdfHelper.GetImage("Logo1");
      if (logo1 != null)
      {
        Rectangle area1 = new Rectangle(_pageMargins.Left, pageSize.GetTop() - _height - _pageMargins.Top, _height/*quadratic: width = height*/, _height);
        Canvas canvas = new Canvas(pdfCanvas, pdfDoc, area1)
          .SetHorizontalAlignment(HorizontalAlignment.CENTER)
          .Add(logo1);

        if (_debugAreas)
          pdfCanvas.SetStrokeColor(ColorConstants.RED)
                   .SetLineWidth(0.5f)
                   .Rectangle(area1)
                   .Stroke();
      }

      Image logo2 = _pdfHelper.GetImage("Logo2");
      if (logo2 != null)
      {
        Rectangle area2 = new Rectangle(pageSize.GetRight() - _pageMargins.Right - _height/*quadratic: width = height*/, pageSize.GetTop() - _height - _pageMargins.Top, _height/*quadratic: width = height*/, _height);
        Canvas canvas = new Canvas(pdfCanvas, pdfDoc, area2).Add(logo2);

        if (_debugAreas)
          pdfCanvas.SetStrokeColor(ColorConstants.RED)
                   .SetLineWidth(0.5f)
                   .Rectangle(area2)
                   .Stroke();
      }

      Rectangle rectHead1 = new Rectangle(
        _pageMargins.Left + _height/*quadratic: width = height*/, 
        pageSize.GetTop() - _height - _pageMargins.Top, 
        pageSize.GetRight() - _pageMargins.Right - _height/*quadratic: width = height*/ - (_pageMargins.Left + _height/*quadratic: width = height*/), 
        _height);
      Paragraph pHead1 = new Paragraph(_header1);
      new Canvas(pdfCanvas, pdfDoc, rectHead1)
              .Add(pHead1
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetFontSize(16)
                );

      if (_debugAreas)
        pdfCanvas.SetStrokeColor(ColorConstants.RED)
                 .SetLineWidth(0.5f)
                 .Rectangle(rectHead1)
                 .Stroke();

      // Double Lines
      pdfCanvas.SetStrokeColor(ColorConstants.BLACK)
               .SetLineWidth(0.3F)
               .MoveTo(_pageMargins.Left, pageSize.GetTop() - _height - _pageMargins.Top - 2.0)
               .LineTo(pageSize.GetRight() - _pageMargins.Right, pageSize.GetTop() - _height - _pageMargins.Top - 2.0)
               .MoveTo(_pageMargins.Left, pageSize.GetTop() - _height - _pageMargins.Top - 3.0)
               .LineTo(pageSize.GetRight() - _pageMargins.Right, pageSize.GetTop() - _height - _pageMargins.Top - 3.0)
               .ClosePathStroke();

      pdfCanvas.Release();
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


    string _footer2;
    bool _debugAreas = false;
    float _height = 110;
    Image _logo3;
    float _logoHeight = 0F;

    public ReportFooter(PdfDocument pdfDoc, Document doc, PDFHelper pdfHelper, Race race, string listName, Margins pageMargins)
    {
      _pdfDoc = pdfDoc;
      _doc = doc;
      _pdfHelper = pdfHelper;
      _race = race;
      _listName = listName;
      _pageMargins = pageMargins;

      var pageSize = PageSize.A4; // Assumption

      _logo3 = _pdfHelper.GetImage("Logo3");
      if (_logo3!=null)
        _logoHeight = (pageSize.GetWidth() - _pageMargins.Left - _pageMargins.Right) * _logo3.GetImageHeight() / _logo3.GetImageWidth();

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

      _height = _logoHeight + tableHeight + 7;
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
        var webAddress = "www.race-horology.com";

        _footer2 = string.Format("{0} V{1}, {2} by {3}, {4}", productName, productVersion, copyrightYear, companyName, webAddress);
      }
      else
        _footer2 = "";

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
      if (_logo3 != null)
      {
        Rectangle area3 = new Rectangle(
          pageSize.GetLeft() + _pageMargins.Left, pageSize.GetBottom() + _pageMargins.Bottom, 
          pageSize.GetWidth() - _pageMargins.Left - _pageMargins.Right, _logoHeight);
        Canvas canvas = new Canvas(pdfCanvas, pdfDoc, area3).Add(_logo3);

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
        pageSize.GetLeft() + _pageMargins.Left, pageSize.GetBottom() + _pageMargins.Bottom + _logoHeight,
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
      Table tableFooter = new Table(3);
      tableFooter.SetWidth(UnitValue.CreatePercentValue(100))
        .SetPaddingBottom(0)
        .SetMarginBottom(0);

      float borderWidth = 1F;
      float padding = 1F;
      var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

      Paragraph parPage = new Paragraph(string.Format("Seite {0}", pageNumber));
      tableFooter.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.LEFT)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new DoubleBorder(borderWidth))
        .SetPadding(padding)
        .SetFont(fontBold)
        .Add(new Paragraph(DateTime.Now.ToString(@"dd.MM.yyyy"))));
      tableFooter.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.CENTER)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new DoubleBorder(borderWidth))
        .SetPadding(padding)
        .SetFont(fontBold)
        .Add(parPage));
      tableFooter.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.RIGHT)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new DoubleBorder(borderWidth))
        .SetPadding(padding)
        .SetFont(fontBold)
        .Add(new Paragraph(string.Format("Bewerbsnummer: {0}", "12345"))));


      tableFooter.AddCell(new Cell(1, 3)
        .SetTextAlignment(TextAlignment.CENTER)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new DoubleBorder(borderWidth))
        .SetPadding(padding)
        .Add(new Paragraph(_footer2)));


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
        .Add(new Paragraph(string.Format("Auswertung: {0}", "TODO: Verein"))));

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

      var header = new ReportHeader(_pdfHelper, _race, getTitle(), pageMargins);
      var footer = new ReportFooter(pdf, document, _pdfHelper, _race, getTitle(), pageMargins);

      pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, header);
      pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, footer);
      //var pageXofY = new PageXofY(pdf);
      //pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, pageXofY);

      document.SetMargins(header.Height + pageMargins.Top, pageMargins.Right, pageMargins.Bottom + footer.Height, pageMargins.Left);

      Table table = getResultsTable();
      document.Add(table);

      //pageXofY.WriteTotal(pdf);
      document.Close();
    }


    protected virtual Table getResultsTable()
    {
      var table = new Table(getTableColumnsWidths());

      table.SetWidth(UnitValue.CreatePercentValue(100));
      table.SetBorder(Border.NO_BORDER);

      addHeaderToTable(table);

      var results = getView();
      var lr = results as System.Windows.Data.ListCollectionView;
      foreach (var group in results.Groups)
      {
        System.Windows.Data.CollectionViewGroup cvGroup = group as System.Windows.Data.CollectionViewGroup;
        addLineToTable(table, cvGroup.Name.ToString());

        int i = 0;
        foreach (var result in cvGroup.Items)
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
      return new float[] { 1, 1, 1, 1, 1, 1, 1 };
    }

    protected override void addHeaderToTable(Table table)
    {

      var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
      Paragraph createParagraph(string text)
      {
        return new Paragraph(text).SetFont(font).SetFontSize(10);
      }

      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT).Add(createParagraph("Stnr")));
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

      table.AddCell(new Cell(1, 6)
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
        bgColor = new DeviceRgb(0.98f, 0.98f, 0.98f);


      // Startnumber
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", rrwp.StartNumber))));
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
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", -1.0 /*rrwp.Points*/))));
      // Runtime
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).SetBorderBottom(new SolidBorder(1F)));
    }
  }



  public class RaceRunResultReport : PDFReport
  {
    RaceRun _raceRun;

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
      return new float[] { 1, 1, 1, 1, 1, 1, 1, 1 };
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
    }


    protected override void addLineToTable(Table table, string group)
    {
      table.AddCell(new Cell(1, 2)
        .SetBorder(Border.NO_BORDER));

      table.AddCell(new Cell(1, 6)
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
        bgColor = new DeviceRgb(0.98f, 0.98f, 0.98f);


      // Position
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}.", rrwp.Position))));
      // Startnumber
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", rrwp.StartNumber))));
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
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", -1.0 /*rrwp.Points*/))));
      // Runtime
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", rrwp.Runtime.ToRaceTimeString()))));
    }
  }


  public class RaceResultReport : PDFReport
  {

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
      return new float[] { 1, 1, 1, 1, 1, 1, 1, 1 };
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
    }


    protected override void addLineToTable(Table table, string group)
    {
      table.AddCell(new Cell(1, 2)
        .SetBorder(Border.NO_BORDER));

      table.AddCell(new Cell(1, 6)
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
        bgColor = new DeviceRgb(0.98f, 0.98f, 0.98f);

      // Position
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}.", item.Position))));
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
      // Points
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", -1.0 /*rrwp.Points*/))));
      // Runtime
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", item.TotalTime.ToRaceTimeString()))));
    }

  }
}
