using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
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
        catch(System.IO.DirectoryNotFoundException)
        {
          continue;
        }
      }
      return null;
    }

  }





  class EndPageHandler : IEventHandler
  {
    PDFHelper _pdfHelper;
    Race _race;
    string _listName;

    string _header1;
    string _footer1;

    public EndPageHandler(PDFHelper pdfHelper, Race race, string listName)
    {
      _pdfHelper = pdfHelper;
      _race = race;
      _listName = listName;

      calculateHeader();
      calculateFooter();
    }



    private void calculateHeader()
    {
      _header1 = _race.Description + "\n\n" + _listName;
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

        _footer1 = string.Format("{0} V{1}, {2} by {3}, {4}", productName, productVersion, copyrightYear, companyName, webAddress);
      }
      else
        _footer1 = "";
    }


    public virtual void HandleEvent(Event @event)
    {
      PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
      PdfDocument pdfDoc = docEvent.GetDocument();
      PdfPage page = docEvent.GetPage();

      int pageNumber = pdfDoc.GetPageNumber(page);
      Rectangle pageSize = page.GetPageSize();
      PdfCanvas pdfCanvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);

      ////Set background
      //Color limeColor = new DeviceCmyk(0.208f, 0, 0.584f, 0);
      //Color blueColor = new DeviceCmyk(0.445f, 0.0546f, 0, 0.0667f);
      //pdfCanvas.SaveState()
      //            .SetFillColor(pageNumber % 2 == 1 ? limeColor : blueColor)
      //            .Rectangle(pageSize.GetLeft(), pageSize.GetBottom(), pageSize.GetWidth(), pageSize.GetHeight())
      //            .Fill()
      //            .RestoreState();

      Image logo1 = _pdfHelper.GetImage("Logo1");
      Image logo2 = _pdfHelper.GetImage("Logo2");

      if (logo1 != null)
      {
        //Rectangle area1 = new Rectangle(0, pageSize.GetTop()- pageSize.GetWidth() * 0.2F * logo1.GetImageHeight() / logo1.GetImageWidth(), pageSize.GetWidth() * 0.2F, pageSize.GetWidth() * 0.2F * logo1.GetImageHeight() / logo1.GetImageWidth());
        Rectangle area1 = new Rectangle(24.0F, pageSize.GetTop() - 110.0F - 24.0F, 110.0F, 110.0F);
        new Canvas(pdfCanvas, pdfDoc, area1)
          .SetHorizontalAlignment(HorizontalAlignment.CENTER)
          .Add(logo1);

        //pdfCanvas
        //              .SetStrokeColor(ColorConstants.RED)
        //              .SetLineWidth(0.5f)
        //              .Rectangle(area1)
        //              .Stroke();
      }

      if (logo2 != null)
      {
        //Rectangle area2 = new Rectangle(pageSize.GetRight() - pageSize.GetWidth()*0.2F, pageSize.GetTop() - pageSize.GetWidth() * 0.2F * logo2.GetImageHeight() / logo2.GetImageWidth(), pageSize.GetWidth() * 0.2F, pageSize.GetWidth() * 0.2F*logo2.GetImageHeight()/logo2.GetImageWidth());
        Rectangle area2 = new Rectangle(pageSize.GetRight() - 24.0F - 110.0F, pageSize.GetTop() - 110.0F - 24.0F, 110.0F, 110.0F);
        new Canvas(pdfCanvas, pdfDoc, area2).Add(logo2);

        //pdfCanvas
        //              .SetStrokeColor(ColorConstants.RED)
        //              .SetLineWidth(0.5f)
        //              .Rectangle(area2)
        //              .Stroke();
      }




      Rectangle rect = new Rectangle(24.0F + 110.0F, pageSize.GetTop() - 110.0F - 24.0F, pageSize.GetRight() - 24.0F - 110.0F - 24.0F - 110.0F, 110.0F);
      Paragraph pHead1 = new Paragraph(_header1);
      new Canvas(pdfCanvas, pdfDoc, rect)
              .Add(pHead1
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetFontSize(16)
                );

      pdfCanvas.SetStrokeColor(ColorConstants.BLACK)
               .SetLineWidth(0.3F)
               .MoveTo(24.0F, pageSize.GetTop() - 110.0F - 24.0F - 2.0)
               .LineTo(pageSize.GetRight() - 24.0F, pageSize.GetTop() - 110.0F - 24.0F - 2.0)
               .MoveTo(24.0F, pageSize.GetTop() - 110.0F - 24.0F - 4.0)
               .LineTo(pageSize.GetRight() - 24.0F, pageSize.GetTop() - 110.0F - 24.0F - 4.0)
               .ClosePathStroke();





      // Footer
      Image logo3 = _pdfHelper.GetImage("Logo3");
      if (logo3 != null)
      {
        Rectangle area3 = new Rectangle(24.0F, 24.0F, pageSize.GetWidth() - 2 * 24.0F, (pageSize.GetWidth() - 2 * 24.0F) * logo3.GetImageHeight() / logo3.GetImageWidth());
        new Canvas(pdfCanvas, pdfDoc, area3).Add(logo3);
      }


      pdfCanvas.SetStrokeColor(ColorConstants.BLACK)
               .SetLineWidth(0.3F)
               .MoveTo(24.0F, pageSize.GetBottom() + (pageSize.GetWidth() - 2 * 24.0F) * logo3.GetImageHeight() / logo3.GetImageWidth() + 2 + 24)
               .LineTo(pageSize.GetRight() - 24.0F, +(pageSize.GetWidth() - 2 * 24.0F) * logo3.GetImageHeight() / logo3.GetImageWidth() + 2 + 24)
               .MoveTo(24.0F, pageSize.GetBottom() + (pageSize.GetWidth() - 2 * 24.0F) * logo3.GetImageHeight() / logo3.GetImageWidth() + 4 + 24)
               .LineTo(pageSize.GetRight() - 24.0F, +(pageSize.GetWidth() - 2 * 24.0F) * logo3.GetImageHeight() / logo3.GetImageWidth() + 4 + 24)
               .ClosePathStroke();

      pdfCanvas.SetStrokeColor(ColorConstants.BLACK)
               .SetLineWidth(0.3F)
               .MoveTo(24.0F, pageSize.GetBottom() + (pageSize.GetWidth() - 2 * 24.0F) * logo3.GetImageHeight() / logo3.GetImageWidth() + 2 + 24 + 24)
               .LineTo(pageSize.GetRight() - 24.0F, +(pageSize.GetWidth() - 2 * 24.0F) * logo3.GetImageHeight() / logo3.GetImageWidth() + 2 + 24 + 24)
               .MoveTo(24.0F, pageSize.GetBottom() + (pageSize.GetWidth() - 2 * 24.0F) * logo3.GetImageHeight() / logo3.GetImageWidth() + 4 + 24 + 24)
               .LineTo(pageSize.GetRight() - 24.0F, +(pageSize.GetWidth() - 2 * 24.0F) * logo3.GetImageHeight() / logo3.GetImageWidth() + 4 + 24 + 24)
               .ClosePathStroke();

      Rectangle rect2 = new Rectangle(24.0F, pageSize.GetBottom() + (pageSize.GetWidth() - 2 * 24.0F) * logo3.GetImageHeight() / logo3.GetImageWidth() + 5 + 24, pageSize.GetRight() - 2 * 24.0F, 24.0F);

      Paragraph pFooter1 = new Paragraph(_footer1);
      new Canvas(pdfCanvas, pdfDoc, rect2)
              .Add(pFooter1
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA)).SetFontSize(10)
                );



      ////Add header and footer
      //pdfCanvas.BeginText()
      //            .SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA), 9)
      //            .MoveText(pageSize.GetWidth() / 2 - 60, pageSize.GetTop() - 20)
      //            .ShowText("THE TRUTH IS OUT THERE")
      //            .MoveText(60, -pageSize.GetTop() + 30)
      //            .ShowText(pageNumber.ToString())
      //            .EndText();


      pdfCanvas.Release();
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
    protected abstract void addLineToTable(Table table, object data);
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

      pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new EndPageHandler(_pdfHelper, _race, getTitle()));

      var document = new Document(pdf, PageSize.A4);

      document.SetMargins(110.0F + 24.0F + 8.0F, 24.0F, 24.0F + 100.0F, 24.0F);

      Table table = getResultsTable();
      document.Add(table);

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

        foreach (var result in cvGroup.Items)
        {
          addLineToTable(table, result);
        }
      }

      return table;
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

      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("Rang")));
      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("Stnr")));
      //table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("Code")));
      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("Teilnehmer")));
      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("JG")));
      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("VB")));
      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("Verein")));
      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("Punkte")));
      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("Laufzeit")));
    }


    protected override void addLineToTable(Table table, string group)
    {
      table.AddCell(new Cell(1, 2)
        .SetBorder(Border.NO_BORDER));

      table.AddCell(new Cell(1, 6)
        .SetBorder(Border.NO_BORDER)
        .Add(new Paragraph(group)
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetFontSize(10)));
    }


    protected override void addLineToTable(Table table, object data)
    {
      RunResultWithPosition rrwp = data as RunResultWithPosition;
      if (rrwp == null)
        return;

      var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
      Paragraph createParagraph(string text)
      {
        return new Paragraph(text).SetFont(font).SetFontSize(9);
      }

      // Position
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).Add(createParagraph(string.Format("{0}.", rrwp.Position))));
      // Startnumber
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).Add(createParagraph(string.Format("{0}", rrwp.StartNumber))));
      //// Code
      //table.AddCell(new Cell().SetBorder(Border.NO_BORDER));
      // Name
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph(rrwp.Participant.Participant.Fullname)));
      // Year
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph(string.Format("{0}", rrwp.Year))));
      // VB
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph(rrwp.Participant.Participant.Nation)));
      // Club
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph(rrwp.Club)));
      // Points
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).Add(createParagraph(string.Format("{0}", -1.0 /*rrwp.Points*/))));
      // Runtime
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).Add(createParagraph(string.Format("{0}", rrwp.Runtime?.ToString(@"mm\:ss\,ff")))));
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

      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("Rang")));
      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("Stnr")));
      //table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("Code")));
      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("Teilnehmer")));
      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("JG")));
      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("VB")));
      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("Verein")));
      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("Punkte")));
      table.AddHeaderCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph("Laufzeit")));
    }


    protected override void addLineToTable(Table table, string group)
    {
      table.AddCell(new Cell(1, 2)
        .SetBorder(Border.NO_BORDER));

      table.AddCell(new Cell(1, 6)
        .SetBorder(Border.NO_BORDER)
        .Add(new Paragraph(group)
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetFontSize(10)));
    }


    protected override void addLineToTable(Table table, object data)
    {
      RaceResultItem item = data as RaceResultItem;
      if (item == null)
        return;

      var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
      Paragraph createParagraph(string text)
      {
        return new Paragraph(text).SetFont(font).SetFontSize(9);
      }

      // Position
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).Add(createParagraph(string.Format("{0}.", item.Position))));
      // Startnumber
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).Add(createParagraph(string.Format("{0}", item.Participant.StartNumber))));
      //// Code
      //table.AddCell(new Cell().SetBorder(Border.NO_BORDER));
      // Name
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph(item.Participant.Participant.Fullname)));
      // Year
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph(string.Format("{0}", item.Participant.Year))));
      // VB
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph(item.Participant.Participant.Nation)));
      // Club
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).Add(createParagraph(item.Participant.Club)));
      // Points
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).Add(createParagraph(string.Format("{0}", -1.0 /*rrwp.Points*/))));
      // Runtime
      table.AddCell(new Cell().SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT).Add(createParagraph(string.Format("{0}", item.TotalTime?.ToString(@"mm\:ss\,ff")))));
    }

  }
}
