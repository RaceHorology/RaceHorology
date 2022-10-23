/*
 *  Copyright (C) 2019 - 2022 by Sven Flossmann
 *  
 *  This file is part of Race Horology.
 *
 *  Race Horology is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  any later version.
 * 
 *  Race Horology is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with Race Horology.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  Diese Datei ist Teil von Race Horology.
 *
 *  Race Horology ist Freie Software: Sie können es unter den Bedingungen
 *  der GNU Affero General Public License, wie von der Free Software Foundation,
 *  Version 3 der Lizenz oder (nach Ihrer Wahl) jeder neueren
 *  veröffentlichten Version, weiter verteilen und/oder modifizieren.
 *
 *  Race Horology wird in der Hoffnung, dass es nützlich sein wird, aber
 *  OHNE JEDE GEWÄHRLEISTUNG, bereitgestellt; sogar ohne die implizite
 *  Gewährleistung der MARKTFÄHIGKEIT oder EIGNUNG FÜR EINEN BESTIMMTEN ZWECK.
 *  Siehe die GNU Affero General Public License für weitere Details.
 *
 *  Sie sollten eine Kopie der GNU Affero General Public License zusammen mit diesem
 *  Programm erhalten haben. Wenn nicht, siehe <https://www.gnu.org/licenses/>.
 * 
 */

using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Wmf;
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

namespace RaceHorologyLib
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
      try
      {
        Image img = null;

        string foundResource = null;
        foundResource = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(x => x.StartsWith("RaceHorologyLib.resources.pdf." + filenameWOExt));
        if (foundResource != null)
        {
          byte[] ReadFully(Stream input)
          {
            using (MemoryStream ms = new MemoryStream())
            {
              input.CopyTo(ms);
              return ms.ToArray();
            }
          }

          var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(foundResource);
          img = new Image(ImageDataFactory.Create(ReadFully(stream)));
        }

        if (img == null)
        {
          string imgPath = FindImage(filenameWOExt);
          if (!string.IsNullOrEmpty(imgPath))
            img = new Image(ImageDataFactory.Create(imgPath));
        }

        return img;
      }catch(Exception /*e*/)
      {
        throw new Exception(string.Format("cannot load image {0}", filenameWOExt));
      }
    }


    void calcResourcePaths()
    {
      List<string> paths = new List<string>();
      paths.Add(_dm.GetDB().GetDBPathDirectory());
      paths.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"resources\pdf"));

      resourcePaths = paths;
    }


    public string FindImage(string filenameWOExt)
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

  public static class PDFHelperExt
  {
    public static Cell ConfigureHeaderCell(this Cell cell)
    {
      cell
        .SetBackgroundColor(PDFHelper.ColorRHFG1)
        .SetFontColor(ColorConstants.WHITE)
        .SetBorderTop(new SolidBorder(PDFHelper.SolidBorderThick))
        .SetBorderBottom(new SolidBorder(PDFHelper.SolidBorderThick))
        ;
      return cell;
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
      _logoRH = _pdfHelper.GetImage("LogoRHShortM");

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
        Canvas canvas = new Canvas(pdfCanvas, areaBanner).Add(_banner);

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
      tableHeight = tableHeight * 1.02F;

      Rectangle rectTable = new Rectangle(
        pageSize.GetLeft() + _pageMargins.Left, pageSize.GetTop() - _pageMargins.Top - _bannerHeight - tableHeight,
        tableWidth, tableHeight);

      new Canvas(pdfCanvas, rectTable)
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
          //.SetMaxHeight(maxHeightCol1)
          .SetBorder(Border.NO_BORDER)
          .SetBorderTop(new SolidBorder(PDFHelper.SolidBorderThick))
          .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin))
          .SetPadding(padding)
          .SetFont(fontBold)
          .Add(_logo1.SetMaxHeight(maxHeightCol1)));
      else
        tableHeader.AddCell(new Cell()
          .SetBorder(Border.NO_BORDER)
          .SetBorderTop(new SolidBorder(PDFHelper.SolidBorderThick))
          .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin)));


      if (!string.IsNullOrEmpty(_race.Description))
        // Race Titles
        tableHeader.AddCell(new Cell()
          .SetTextAlignment(TextAlignment.CENTER)
          .SetVerticalAlignment(VerticalAlignment.MIDDLE)
          .SetBorder(Border.NO_BORDER)
          .SetBorderTop(new SolidBorder(PDFHelper.SolidBorderThick))
          .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin))
          .SetPadding(padding)
          .SetFont(fontTitle)
          .SetFontSize(fontSizeTitle)
          .Add(new Paragraph(_race.Description)));
      else
        tableHeader.AddCell(new Cell()
          .SetBorder(Border.NO_BORDER)
          .SetBorderTop(new SolidBorder(PDFHelper.SolidBorderThick))
          .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin)));

      if (_logoRH != null)
        tableHeader.AddCell(new Cell()
          .SetTextAlignment(TextAlignment.RIGHT)
          .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
          .SetVerticalAlignment(VerticalAlignment.MIDDLE)
          .SetBorder(Border.NO_BORDER)
          .SetBorderTop(new SolidBorder(PDFHelper.SolidBorderThick))
          .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin))
          .SetPadding(padding)
          .SetFont(fontBold)
          .Add(_logoRH.SetMaxHeight(maxHeightCol1*0.8F)));
      else
        tableHeader.AddCell(new Cell()
          .SetBorder(Border.NO_BORDER)
          .SetBorderTop(new SolidBorder(PDFHelper.SolidBorderThick))
          .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin)));


      // Second row
      if (_logo2 != null)
      {
        tableHeader.AddCell(new Cell()
          .SetTextAlignment(TextAlignment.LEFT)
          .SetVerticalAlignment(VerticalAlignment.MIDDLE)
          
          .SetBorder(Border.NO_BORDER)
          .SetBorderBottom(new SolidBorder(PDFHelper.SolidBorderThick))
          .SetPadding(padding)
          .SetFont(fontBold)
          .Add(_logo2.SetMaxHeight(maxHeightCol2)));
      }
      else
        tableHeader.AddCell(new Cell()
          .SetBorder(Border.NO_BORDER)
          .SetBorderBottom(new SolidBorder(PDFHelper.SolidBorderThick)));

      // List Name
      tableHeader.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.CENTER)
        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new SolidBorder(PDFHelper.SolidBorderThick))
        .SetPadding(padding)
        .SetFont(fontTitle)
        .SetFontSize(fontSizeTitle)
        .Add(new Paragraph(_listName)));

      // Race Date & Time
      tableHeader.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.RIGHT)
        .SetVerticalAlignment(VerticalAlignment.TOP)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new SolidBorder(PDFHelper.SolidBorderThick))
        .SetPadding(padding)
        .SetFont(fontNormal)
        .SetFontSize(fontSizeNormal)
        .Add(new Paragraph(_race.DateResultList?.ToShortDateString() + "\n" + (_race.AdditionalProperties?.Location ?? ""))));

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
        Canvas canvas = new Canvas(pdfCanvas, area3).Add(_banner);

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

      new Canvas(pdfCanvas, rectTable)
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
      if (string.IsNullOrEmpty(_race.RaceNumber))
        tableFooter.AddCell(new Cell()
          .SetTextAlignment(TextAlignment.LEFT)
          .SetBorder(Border.NO_BORDER)
          .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
          .SetPadding(padding)
          .SetFont(fontBold));
      else
        tableFooter.AddCell(new Cell()
          .SetTextAlignment(TextAlignment.LEFT)
          .SetBorder(Border.NO_BORDER)
          .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
          .SetPadding(padding)
          .SetFont(fontBold)
          .Add(new Paragraph(string.Format("Bewerbsnummer: {0}", _race.RaceNumber))));

      tableFooter.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.CENTER)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        .SetPadding(padding)
        .SetFont(fontBold)
        .Add(new Paragraph("")));

      tableFooter.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.RIGHT)
        .SetBorder(Border.NO_BORDER)
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        .SetPadding(padding)
        .SetFont(fontBold)
        .Add(parPage));
     


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
        .Add(new Paragraph("")));

      tableFooter.AddCell(new Cell()
        .SetTextAlignment(TextAlignment.RIGHT)
        .SetBorder(Border.NO_BORDER)
        .SetPadding(padding)
        .SetFont(fontBold)
        .Add(new Paragraph(string.Format("Timing: {0}", "Alge TdC8000/8001"))));

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
      Canvas canvas = new Canvas(pdfCanvas, pageSize);
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





  public abstract class PDFRaceReport : IPDFReport
  {
    protected Race _race;
    protected AppDataModel _dm;
    protected Document _document;

    protected PDFHelper _pdfHelper;
    PointsConverter _pointsConverter;

    public PDFRaceReport(Race race)
    {
      _race = race;
      _dm = race.GetDataModel();
      _document = null;

      _pdfHelper = new PDFHelper(_dm);
      _pointsConverter = new PointsConverter();
    }

    protected abstract string getTitle();
    protected abstract string getReportName();
    protected abstract void addContent(PdfDocument pdf, Document document);


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
      determineTableFontAndSize();

      var writer = new PdfWriter(filePath);
      var pdf = new PdfDocument(writer);

      Margins pageMargins = new Margins { Top = 24.0F, Bottom = 24.0F, Left = 24.0F, Right = 24.0F };

      var document = new Document(pdf, PageSize.A4);
      _document = document;

      var header = new ReportHeader(pdf, document, _pdfHelper, _race, getTitle(), pageMargins);
      var footer = new ReportFooter(pdf, document, _pdfHelper, _race, getTitle(), pageMargins);

      pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, header);
      pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, footer);
      //var pageXofY = new PageXofY(pdf);
      //pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, pageXofY);

      document.SetMargins(header.Height + pageMargins.Top, pageMargins.Right, pageMargins.Bottom + footer.Height, pageMargins.Left);

      addContent(pdf, document);
      _document = null;

      //pageXofY.WriteTotal(pdf);
      document.Close();
    }



    protected PdfFont _tableFont;
    protected PdfFont _tableFontHeader;
    protected float _tableFontSize;
    protected float _tableFontSizeHeader;

    protected virtual void determineTableFontAndSize()
    {
      _tableFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
      _tableFontHeader = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

      _tableFontSize = 9;
      _tableFontSizeHeader = _tableFontSize + 1;
    }

    protected Paragraph createCellParagraphForTable(string text)
    {
      if (text == null)
        text = string.Empty;

      return new Paragraph(text)
        .SetFont(_tableFont)
        .SetFontSize(_tableFontSize)
        .SetPaddingTop(0)
        .SetPaddingBottom(0)
        .SetPaddingLeft(0)
        .SetPaddingRight(0);
    }

    protected Paragraph createHeaderCellParagraphForTable(string text)
    {
      if (text == null)
        text = string.Empty;

      return new Paragraph(text)
        .SetFont(_tableFontHeader)
        .SetFontSize(_tableFontSizeHeader);
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

    protected Cell createCellForTable(int colspan, TextAlignment? textAlignment = TextAlignment.LEFT)
    {
      return new Cell(1, colspan)
        .SetBorder(Border.NO_BORDER)
        .SetPaddingTop(0)
        .SetPaddingBottom(0)
        .SetPaddingLeft(4)
        .SetPaddingRight(4)
        .SetTextAlignment(textAlignment);
    }

    protected string formatPoints(double points)
    {
      return (string) _pointsConverter.Convert(points, typeof(string), null, null);
    }

    protected string formatStartNumber(uint startNumber)
    {
      if (startNumber < 1)
        return "---";

      return startNumber.ToString();
    }
  }




  public abstract class PDFReport : PDFRaceReport
  {
    protected PositionConverter _positionConverter = new PositionConverter();

    protected int _nOptFields;

    public bool WithDiagram { get; set; } = true;
    public bool WithRaceHeader { get; set; } = true;

    public PDFReport(Race race) : base(race)
    {

      _nOptFields = 0;
    }

    protected abstract ICollectionView getView();
    protected abstract float[] getTableColumnsWidths();
    protected abstract void addHeaderToTable(Table table);
    protected abstract void addLineToTable(Table table, string group);
    protected abstract void addCommentLineToTable(Table table, string comment);
    protected abstract bool addLineToTable(Table table, object data, int i = 0);

    protected override void addContent(PdfDocument pdf, Document document)
    {
      if (WithRaceHeader)
      {
        Table raceProperties = getRacePropertyTable();
        if (raceProperties != null)
          document.Add(raceProperties);
      }

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
        .Add(new Paragraph("KAMPFGERICHT / JURY")
          .SetPaddingTop(6)
          .SetFont(fontBold)));

      table.AddCell(createCell(1,2)
        //.SetBorder(Border.NO_BORDER)
        .Add(new Paragraph("TECHNISCHE DATEN")
          .SetPaddingTop(6)
          .SetFont(fontBold)));

      table.AddCell(createCell()
        .Add(new Paragraph("Schiedsrichter:")
          .SetPaddingTop(6)
          .SetFont(fontBold)));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceReferee.Name))
          .SetPaddingTop(6)));
      table.AddCell(createCell()
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.RaceReferee.Club))
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

      table.AddCell(createCell()
        .Add(new Paragraph("Auswertung / Zeitnahme:")
          .SetFont(fontBold)));
      table.AddCell(createCell(1, 2)
        .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.Analyzer))));
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
      if (string.IsNullOrEmpty(_race.AdditionalProperties.CoarseHomologNo))
      {
        table.AddCell(createCell(1, 2));
      }
      else
      {
        table.AddCell(createCell()
          .Add(new Paragraph("Homolog Nr.:")
            .SetFont(fontBold)));
        table.AddCell(createCell()
          .SetTextAlignment(TextAlignment.RIGHT)
          .Add(new Paragraph(stringOrEmpty(_race.AdditionalProperties.CoarseHomologNo))));
      }

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


      string formatWeatherHeading()
      {
        if (string.IsNullOrEmpty(_race.AdditionalProperties.Snow))
          return "Wetter:";

        return "Wetter / Schnee:";
      }

      string formatWeather()
      {
        if (string.IsNullOrEmpty(_race.AdditionalProperties.Weather) && string.IsNullOrEmpty(_race.AdditionalProperties.Snow))
          return "";
        if (string.IsNullOrEmpty(_race.AdditionalProperties.Weather))
          return _race.AdditionalProperties.Snow;
        if (string.IsNullOrEmpty(_race.AdditionalProperties.Snow))
          return _race.AdditionalProperties.Weather;

        return string.Format("{0} / {1}", _race.AdditionalProperties.Weather, _race.AdditionalProperties.Snow);
      }

      table.AddCell(createCell()
        .Add(new Paragraph(formatWeatherHeading())
          .SetPaddingTop(6)
          .SetFont(fontBold)));
      table.AddCell(createCell(1, 2)
        .Add(new Paragraph(formatWeather())));
      table.AddCell(createCell()
        .Add(new Paragraph("Temperatur (Start/Ziel):")
          .SetPaddingTop(6)
          .SetFont(fontBold)));
      table.AddCell(createCell()
        .Add(new Paragraph(string.Format("{0} °C / {1} °C", _race.AdditionalProperties.TempStart, _race.AdditionalProperties.TempFinish))));

      table.AddCell(createCell(1, 5)
        .SetPaddingTop(12)
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick)));
      table.AddCell(createCell(1, 5)
        .SetPaddingTop(12));

      return table;
    }


    protected virtual void calcNumOptFields()
    {
      _nOptFields = 0;

      if (_race.IsFieldActive("Code"))
        _nOptFields++;
      if (_race.IsFieldActive("Year"))
        _nOptFields++;
      if (_race.IsFieldActive("Nation"))
        _nOptFields++;
      if (_race.IsFieldActive("Club"))
        _nOptFields++;
      if (_race.IsFieldActive("Points"))
        _nOptFields++;
      if (_race.IsFieldActive("Percentage"))
        _nOptFields++;
    }

    protected override void determineTableFontAndSize()
    {
      _tableFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
      _tableFontHeader = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

      _tableFontSize = 9;
      if (_nOptFields > 3)
        _tableFontSize = 8;
      if (_nOptFields > 4)
        _tableFontSize = 7;

      _tableFontSizeHeader = _tableFontSize + 1;
    }

    protected virtual Table getResultsTable()
    {
      calcNumOptFields(); // Ensures the member _nOptFields is correct (used by getTableColumnsWidths())
      determineTableFontAndSize();

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
          addLineToTable(table, cvGroup.GetName());

          int i = 0;
          foreach (var result in cvGroup.Items)
            if (addLineToTable(table, result, i))
              i++;
          
          if (i == 0)
            addCommentLineToTable(table, "keine Teilnehmer");
        }
      }
      else
      {
        int i = 0;
        foreach (var result in results.SourceCollection)
          if (addLineToTable(table, result, i))
            i++;
      }

      return table;
    }




  }



  public class StartListReport : PDFReport
  {
    RaceRun _raceRun;

    public StartListReport(RaceRun rr) : base(rr.GetRace())
    {
      _raceRun = rr;
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
      float[] columns = new float[3 + _nOptFields];
      for (int i = 0; i < columns.Length; i++)
        columns[i] = 1;

      return columns;
    }

    protected override void addHeaderToTable(Table table)
    {
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Stnr")));
      if (_race.IsFieldActive("Code"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("Code")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Teilnehmer")));
      if (_race.IsFieldActive("Year"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("JG")));
      if (_race.IsFieldActive("Nation"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("VB")));
      if (_race.IsFieldActive("Club"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("Verein")));
      if (_race.IsFieldActive("Points"))
        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("Punkte")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Laufzeit")));
    }


    protected override void addLineToTable(Table table, string group)
    {
      table.AddCell(new Cell(1, 1)
        .SetBorder(Border.NO_BORDER)
        .SetBorderTop(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        //.SetBackgroundColor(PDFHelper.ColorRHBG2)
        );

      table.AddCell(new Cell(1, 2 + _nOptFields)
        .SetBorder(Border.NO_BORDER)
        .SetBorderTop(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        //.SetBackgroundColor(PDFHelper.ColorRHBG2)
        .Add(new Paragraph(group)
          .SetPaddingTop(6)
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetFontSize(10)));
    }

    protected override void addCommentLineToTable(Table table, string comment)
    {
      table.AddCell(new Cell(1, 1)
        .SetBorder(Border.NO_BORDER)
        );

      table.AddCell(new Cell(1, 2 + _nOptFields)
        .SetBorder(Border.NO_BORDER)
        .Add(new Paragraph(comment)
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE)).SetFontSize(10)));
    }



    protected override bool addLineToTable(Table table, object data, int i = 0)
    {
      StartListEntry rrwp = data as StartListEntry;
      if (rrwp == null)
        return false;

      Color bgColor = ColorConstants.WHITE;// new DeviceRgb(0.97f, 0.97f, 0.97f);
      if (i % 2 == 1)
        bgColor = PDFHelper.ColorRHBG1;

      // Startnumber
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(formatStartNumber(rrwp.StartNumber))));
      // Code
      if (_race.IsFieldActive("Code"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.CodeOrSvId)));
      // Name
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.Fullname)));
      // Year
      if (_race.IsFieldActive("Year"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", rrwp.Year))));
      // VB
      if (_race.IsFieldActive("Nation"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.Nation)));
      // Club
      if (_race.IsFieldActive("Club"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Club)));
      // Points
      if (_race.IsFieldActive("Points"))
        table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(formatPoints(rrwp.Points))));
      // Runtime
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).SetBorderBottom(new DottedBorder(1F)));

      return true;
    }
  }


  public class StartListReport2ndRun : PDFReport
  {
    RaceRun _raceRun;

    ResultTimeAndCodeConverter _timeConverter = new ResultTimeAndCodeConverter();

    public StartListReport2ndRun(RaceRun rr) : base(rr.GetRace())
    {
      _raceRun = rr;
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
      float[] columns = new float[5 + _nOptFields];
      for (int i = 0; i < columns.Length; i++)
        columns[i] = 1;

      return columns;
    }

    protected override void addHeaderToTable(Table table)
    {
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Lfnr")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Stnr")));
      if (_race.IsFieldActive("Code"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("Code")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Teilnehmer")));
      if (_race.IsFieldActive("Year"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("JG")));
      if (_race.IsFieldActive("Nation"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("VB")));
      if (_race.IsFieldActive("Club"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("Verein")));
      if (_race.IsFieldActive("Points"))
        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("Punkte")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Zeit-1")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Laufzeit")));
    }


    protected override void addLineToTable(Table table, string group)
    {
      table.AddCell(new Cell(1, 2)
        .SetBorder(Border.NO_BORDER)
        .SetBorderTop(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        //.SetBackgroundColor(PDFHelper.ColorRHBG2)
        );

      table.AddCell(new Cell(1, 3 + _nOptFields)
        .SetBorder(Border.NO_BORDER)
        .SetBorderTop(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        //.SetBackgroundColor(PDFHelper.ColorRHBG2)
        .Add(new Paragraph(group)
          .SetPaddingTop(6)
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetFontSize(10)));
    }

    protected override void addCommentLineToTable(Table table, string comment)
    {
      table.AddCell(new Cell(1, 2)
        .SetBorder(Border.NO_BORDER)
        );

      table.AddCell(new Cell(1, 3 + _nOptFields)
        .SetBorder(Border.NO_BORDER)
        .Add(new Paragraph(comment)
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE)).SetFontSize(10)));
    }


    protected override bool addLineToTable(Table table, object data, int i = 0)
    {
      StartListEntryAdditionalRun rrwp = data as StartListEntryAdditionalRun;
      if (rrwp == null)
        return false;

      Color bgColor = ColorConstants.WHITE;// new DeviceRgb(0.97f, 0.97f, 0.97f);
      if (i % 2 == 1)
        bgColor = PDFHelper.ColorRHBG1;

      // Running Number
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", (i+1)))));
      // Startnumber
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(formatStartNumber(rrwp.StartNumber))));
      // Code
      if (_race.IsFieldActive("Code"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.CodeOrSvId)));
      // Name
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.Fullname)));
      // Year
      if (_race.IsFieldActive("Year"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", rrwp.Year))));
      // VB
      if (_race.IsFieldActive("Nation"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.Nation)));
      // Club
      if (_race.IsFieldActive("Club"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Club)));
      // Points
      if (_race.IsFieldActive("Points"))
        table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(formatPoints(rrwp.Points))));

      // Runtime 1st run
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(
        (string)_timeConverter.Convert(new object[] { rrwp.Runtime, rrwp.ResultCode }, typeof(string), null, null))));

      // Empty runtime slot
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).SetBorderBottom(new DottedBorder(1F)));

      return true;
    }
  }



  public abstract class ResultReport : PDFReport
  {

    protected ResultReport(Race race) : base(race)
    { }


    protected abstract void addLineToTable(Table table, RunResultWithPosition data, string notes, int i = 0);
    protected abstract void addSubHeaderToTable(Table table, string group);



    protected virtual Table addNotFinishedPart(Table table, RaceRun rr)
    {
      List<RunResultWithPosition> itemsToPrint = new List<RunResultWithPosition>();
      foreach (var obj in rr.GetResultViewProvider().GetView())
      {
        if (!(obj is RunResultWithPosition item))
          continue;

        if (item.ResultCode != RunResult.EResultCode.NiZ)
          continue;

        itemsToPrint.Add(item);
      }

      if (itemsToPrint.Count == 0)
        return table;


      addSubHeaderToTable(table, string.Format("Nicht im Ziel {0}. Durchgang", rr.Run));

      int i = 0;
      foreach (var item in itemsToPrint)
      {
        addLineToTable(table, item, "", i);
        //i++;
      }

      return table;
    }


    protected virtual Table addDisqualifiedTable(Table table, RaceRun rr)
    {
      List<RunResultWithPosition> itemsToPrint = new List<RunResultWithPosition>();
      foreach (var obj in rr.GetResultViewProvider().GetView())
      {
        if (!(obj is RunResultWithPosition item))
          continue;

        if (item.ResultCode != RunResult.EResultCode.DIS)
          continue;

        itemsToPrint.Add(item);
      }

      if (itemsToPrint.Count == 0)
        return table;

      addSubHeaderToTable(table, string.Format("Disqualifiziert im {0}. Durchgang", rr.Run));

      int i = 0;
      foreach (var item in itemsToPrint)
      {
        addLineToTable(table, item, item.DisqualText, i);
        //i++;
      }

      return table;
    }


    protected virtual Table addNotStartedTable(Table table, RaceRun rr)
    {
      List<RunResultWithPosition> itemsToPrint = new List<RunResultWithPosition>();
      foreach (var obj in rr.GetResultViewProvider().GetView())
      {
        if (!(obj is RunResultWithPosition item))
          continue;

        if (item.ResultCode != RunResult.EResultCode.NaS)
          continue;

        itemsToPrint.Add(item);
      }

      if (itemsToPrint.Count == 0)
        return table;

      addSubHeaderToTable(table, string.Format("Nicht am Start {0}. Durchgang", rr.Run));

      int i = 0;
      foreach (var item in itemsToPrint)
      {
        addLineToTable(table, item, "", i);
        //i++;
      }

      return table;
    }
  }




  public class RaceRunResultReport : ResultReport
  {
    RaceRun _raceRun;

    ResultTimeAndCodeConverter _timeConverter = new ResultTimeAndCodeConverter();
    PercentageConverter _percentageConverter = new PercentageConverter(false);


    public RaceRunResultReport(RaceRun rr) : base(rr.GetRace())
    {
      _raceRun = rr;
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
      float[] columns = new float[5 + _nOptFields];
      for (int i = 0; i < columns.Length; i++)
        columns[i] = 1;

      return columns;
    }

    protected override void addHeaderToTable(Table table)
    {
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Rang")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Stnr")));
      if (_race.IsFieldActive("Code"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("Code")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Teilnehmer")));
      if (_race.IsFieldActive("Year"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("JG")));
      if (_race.IsFieldActive("Nation"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("VB")));
      if (_race.IsFieldActive("Club"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("Verein")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Laufzeit")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Diff [s]")));
      if (_race.IsFieldActive("Percentage"))
        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("Diff [%]")));
      if (_race.IsFieldActive("Points"))
        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("Punkte")));
    }


    protected override void addLineToTable(Table table, string group)
    {
      table.AddCell(new Cell(1, 2)
        .SetBorder(Border.NO_BORDER)
        .SetBorderTop(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        );

      table.AddCell(new Cell(1, 3 + _nOptFields)
        .SetBorder(Border.NO_BORDER)
        .SetBorderTop(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick))
        .Add(new Paragraph(group)
          .SetPaddingTop(6)
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetFontSize(10)));
    }

    protected override void addCommentLineToTable(Table table, string comment)
    {
      table.AddCell(new Cell(1, 2)
        .SetBorder(Border.NO_BORDER)
        );

      table.AddCell(new Cell(1, 3 + _nOptFields)
        .SetBorder(Border.NO_BORDER)
        .Add(new Paragraph(comment)
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE)).SetFontSize(10)));
    }

    protected override void addSubHeaderToTable(Table table, string group)
    {
      table.AddCell(new Cell(1, 2)
        .SetBorder(Border.NO_BORDER)
        );

      table.AddCell(new Cell(1, 3 + _nOptFields)
        .SetBorder(Border.NO_BORDER)
        .Add(new Paragraph(group)
          .SetPaddingTop(12)
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetFontSize(10)));
    }


    protected override bool addLineToTable(Table table, object data, int i=0)
    {
      RunResultWithPosition rrwp = data as RunResultWithPosition;
      if (rrwp == null)
        return false;

      if (rrwp.ResultCode == RunResult.EResultCode.NotSet)
        return false;

      Color bgColor = ColorConstants.WHITE;// new DeviceRgb(0.97f, 0.97f, 0.97f);
      if (i % 2 == 1)
        bgColor = PDFHelper.ColorRHBG1;

      // Position
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable((string)_positionConverter.Convert(rrwp.Position, typeof(string), null, null))));
      // Startnumber
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(formatStartNumber(rrwp.StartNumber))));
      // Code
      if (_race.IsFieldActive("Code"))
        table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.CodeOrSvId)));
      // Name
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.Fullname)));
      // Year
      if (_race.IsFieldActive("Year"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", rrwp.Year))));
      // VB
      if (_race.IsFieldActive("Nation"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.Nation)));
      // Club
      if (_race.IsFieldActive("Club"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Club)));

      // Runtime
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor)
        .Add(createCellParagraphForTable((string)_timeConverter.Convert(new object[] { rrwp.Runtime, rrwp.ResultCode }, typeof(string), null, null))));
      // Diff
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", rrwp.DiffToFirst.ToRaceTimeString()))));
      if (_race.IsFieldActive("Percentage"))
        table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(
        (string)_percentageConverter.Convert(rrwp.DiffToFirstPercentage, typeof(string), null, null))));

      // Points
      if (_race.IsFieldActive("Points"))
        table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(formatPoints(-1.0/*TODO: rrwp.Points*/))));

      return true;
    }


    protected override void addLineToTable(Table table, RunResultWithPosition rrwp, string notes, int i = 0)
    {
      Color bgColor = ColorConstants.WHITE;// new DeviceRgb(0.97f, 0.97f, 0.97f);
      if (i % 2 == 1)
        bgColor = PDFHelper.ColorRHBG1;

      // Position
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable("")));
      // Startnumber
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(formatStartNumber(rrwp.StartNumber))));
      // Code
      if (_race.IsFieldActive("Code"))
        table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.CodeOrSvId)));
      // Name
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.Fullname)));
      // Year
      if (_race.IsFieldActive("Year"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", rrwp.Year))));
      // VB
      if (_race.IsFieldActive("Nation"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.Nation)));
      // Club
      if (_race.IsFieldActive("Club"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Club)));

      int colSpan = 2;
      if (_race.IsFieldActive("Percentage"))
        colSpan++;
      if (_race.IsFieldActive("Points"))
        colSpan++;

      // Notes
      table.AddCell(createCellForTable(colSpan, TextAlignment.LEFT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(notes)));
    }


    protected override Table getResultsTable()
    {
      Table table = base.getResultsTable();

      addNotStartedTable(table, _raceRun);
      addNotFinishedPart(table, _raceRun);
      addDisqualifiedTable(table, _raceRun);

      return table;
    }

  }


  public class RaceResultReport : ResultReport
  {

    ResultTimeAndCodeConverter _timeConverter = new ResultTimeAndCodeConverter();
    PercentageConverter _percentageConverter = new PercentageConverter(false);


    public RaceResultReport(Race race) : base(race)
    {
    
    }


    protected override string getReportName()
    {
      return string.Format("Ergebnis Gesamt");
    }

    protected override string getTitle()
    {
      return string.Format("ERGEBNISLISTE");
    }


    protected override ICollectionView getView()
    {
      return _race.GetResultViewProvider().GetView();
    }


    protected override float[] getTableColumnsWidths()
    {
      float[] columns = new float[5 + _nOptFields + _race.GetMaxRun()];
      for (int i = 0; i < columns.Length; i++)
        columns[i] = 1F;

      return columns;
    }

    protected override void addHeaderToTable(Table table)
    {
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Rang")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Stnr")));
      if (_race.IsFieldActive("Code"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("Code")));
      table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Teilnehmer")));
      if (_race.IsFieldActive("Year"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("JG")));
      if (_race.IsFieldActive("Nation"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("VB")));
      if (_race.IsFieldActive("Club"))
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("Verein")));

      for (int i = 1; i <= _race.GetMaxRun(); i++)
        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable(string.Format("Zeit-{0}", i))));

      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Laufzeit")));
      table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
        .ConfigureHeaderCell()
        .Add(createHeaderCellParagraphForTable("Diff [s]")));
      if (_race.IsFieldActive("Percentage"))
        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("Diff [%]")));
      if (_race.IsFieldActive("Points"))
        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderCellParagraphForTable("Punkte")));
    }


    protected override void addLineToTable(Table table, string group)
    {
      table.AddCell(new Cell(1, 2)
        .SetBorder(Border.NO_BORDER)
        .SetBorderTop(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin))
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin))
        //.SetBackgroundColor(PDFHelper.ColorRHBG2)
        );

      table.AddCell(new Cell(1, 3 + _race.GetMaxRun() + _nOptFields)
        .SetBorder(Border.NO_BORDER)
        .SetBorderTop(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin))
        .SetBorderBottom(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin))
        //.SetBackgroundColor(PDFHelper.ColorRHBG2)
        .Add(new Paragraph(group)
          .SetPaddingTop(6)
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetFontSize(10)));
    }

    protected override void addCommentLineToTable(Table table, string comment)
    {
      table.AddCell(new Cell(1, 2)
        .SetBorder(Border.NO_BORDER)
        );

      table.AddCell(new Cell(1, 3 + _race.GetMaxRun() + _nOptFields)
        .SetBorder(Border.NO_BORDER)
        .Add(new Paragraph(comment)
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE)).SetFontSize(10)));
    }

    protected override void addSubHeaderToTable(Table table, string group)
    {
      table.AddCell(new Cell(1, 2)
        .SetBorder(Border.NO_BORDER)
        );

      table.AddCell(new Cell(1, 3 + _race.GetMaxRun() + _nOptFields)
        .SetBorder(Border.NO_BORDER)
        .Add(new Paragraph(group)
          .SetPaddingTop(12)
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)).SetFontSize(10)));
    }


    protected override bool addLineToTable(Table table, object data, int i=0)
    {
      RaceResultItem item = data as RaceResultItem;
      if (item == null)
        return false;

      if (item.ResultCode == RunResult.EResultCode.NotSet)
        return false;

      Color bgColor = ColorConstants.WHITE;
      if (i % 2 == 1)
        bgColor = PDFHelper.ColorRHBG1;

      // Position
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable((string)_positionConverter.Convert(item.Position, typeof(string), null, null))));
      // Startnumber
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", item.Participant.StartNumber))));
      // Code
      if (_race.IsFieldActive("Code"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(item.Participant.Participant.CodeOrSvId)));
      // Name
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(item.Participant.Participant.Fullname)));
      // Year
      if (_race.IsFieldActive("Year"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", item.Participant.Year))));
      // VB
      if (_race.IsFieldActive("Nation"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(item.Participant.Participant.Nation)));
      // Club
      if (_race.IsFieldActive("Club"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(item.Participant.Club)));

      for (uint j = 1; j <= _race.GetMaxRun(); j++)
      {
        if (item.SubResults.ContainsKey(j))
        {
          string str = (string)_timeConverter.Convert(new object[] { item.SubResults[j].Runtime, item.SubResults[j].RunResultCode }, typeof(string), null, null);
          table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(str)));
        }
        else
          table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor));
      }

      // Runtime
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", item.TotalTime.ToRaceTimeString()))));
      // Diff
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", item.DiffToFirst.ToRaceTimeString()))));
      if (_race.IsFieldActive("Percentage"))
        table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(
          (string)_percentageConverter.Convert(item.DiffToFirstPercentage, typeof(string), null, null))));

      // Points
      if (_race.IsFieldActive("Points"))
        table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(formatPoints(item.Points))));

      return true;
    }


    protected override void addLineToTable(Table table, RunResultWithPosition rrwp, string notes, int i = 0)
    {
      Color bgColor = ColorConstants.WHITE;
      if (i % 2 == 1)
        bgColor = PDFHelper.ColorRHBG1;

      // Position
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable("")));
      // Startnumber
      table.AddCell(createCellForTable(TextAlignment.RIGHT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", rrwp.Participant.StartNumber))));
      // Code
      if (_race.IsFieldActive("Code"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.CodeOrSvId)));
      // Name
      table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.Fullname)));
      // Year
      if (_race.IsFieldActive("Year"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(string.Format("{0}", rrwp.Participant.Year))));
      // VB
      if (_race.IsFieldActive("Nation"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Participant.Nation)));
      // Club
      if (_race.IsFieldActive("Club"))
        table.AddCell(createCellForTable().SetBackgroundColor(bgColor).Add(createCellParagraphForTable(rrwp.Participant.Club)));

      int colSpan = 2 + _race.GetMaxRun();
      if (_race.IsFieldActive("Percentage"))
        colSpan++;
      if (_race.IsFieldActive("Points"))
        colSpan++;

      // Runtime
      table.AddCell(createCellForTable(colSpan, TextAlignment.LEFT).SetBackgroundColor(bgColor).Add(createCellParagraphForTable(notes)));
    }


    protected override Table getResultsTable()
    {
      Table table = base.getResultsTable();

      for (int i = 0; i < _race.GetMaxRun(); i++)
      {
        addNotStartedTable(table, _race.GetRun(i));
        addNotFinishedPart(table, _race.GetRun(i));
        addDisqualifiedTable(table, _race.GetRun(i));
      }

      return table;
    }


    protected override void addContent(PdfDocument pdf, Document document)
    {
      base.addContent(pdf, document);

      addStatistic(pdf, document);

      if (WithDiagram)
        addResultsChart(pdf, document);
    }


    protected void addStatistic(PdfDocument pdf, Document document)
    {
      int participants = _race.GetParticipants().Count();
      int particpantsClassified = 0;

      var endresult = _race.GetResultViewProvider().GetView();
      foreach(var o in endresult)
      {
        if (o is RaceResultItem res)
        {
          if (res.Position > 0)
            particpantsClassified++;
        }
      }

      var fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
      var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
      var fontTitle = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
      //int fontSizeTitle = 16;
      int fontSizeNormal = 10;

      document.Add(
        new Paragraph("Bewerbsstatistik")
        .SetFont(fontBold)
        .SetFontSize(fontSizeNormal)
        .SetPaddingTop(12)
      );

      string statistic = string.Format(
        "Gemeldete Teilnehmer: \t{0}\n" +
        "Gewertete Teilnehmer: \t{1}\n" +
        "Ausgeschiedene Teilnehmer: \t{2}\n"
        , participants, particpantsClassified, participants - particpantsClassified
      );

      document.Add(
        new Paragraph(statistic)
        .SetFont(fontNormal)
        .SetFontSize(fontSizeNormal)
      );


    }

    protected void addResultsChart(PdfDocument pdf, Document document)
    {
      var page = pdf.AddNewPage();

      // Determine the position of the chart
      Rectangle areaChart = new Rectangle(document.GetLeftMargin(), document.GetBottomMargin(),
        page.GetPageSize().GetWidth() - document.GetLeftMargin() - document.GetRightMargin(), 
        page.GetPageSize().GetHeight() - document.GetBottomMargin() - document.GetTopMargin());

      // Render the chart (vector format WMF)
      OfflineChart fileHelper = new OfflineChart((int)areaChart.GetWidth(), (int)areaChart.GetHeight());

      //if (false)
      //{
      //  MemoryStream chartStreamWMF = new MemoryStream();
      //  fileHelper.RenderToWmf(chartStreamWMF, _race.GetResultViewProvider());

      //  // Create an iText Image 
      //  //WmfImageData imgData = new WmfImageData(chartStreamWMF.ToArray());
      //  WmfImageData imgData = new WmfImageData(@"c:\trash\test.wmf");
      //  var pdfFormxObj = new PdfFormXObject(imgData, pdf);
      //  Image imgChart = new Image(pdfFormxObj);
      //  // Render the image
      //  PdfCanvas pdfCanvas = new PdfCanvas(page);
      //  Canvas canvas = new Canvas(pdfCanvas, areaChart)
      //    .SetHorizontalAlignment(HorizontalAlignment.CENTER)
      //    .Add(imgChart.SetAutoScale(true));
      //}
      //else
      {
        MemoryStream imgStream = new MemoryStream();
        fileHelper.RenderToImage(imgStream, _race.GetResultViewProvider());

        // Create an iText Image 
        var imgData = new Image(ImageDataFactory.Create(imgStream.ToArray()));
        // Render the image
        PdfCanvas pdfCanvas = new PdfCanvas(page);
        Canvas canvas = new Canvas(pdfCanvas, areaChart)
          .SetHorizontalAlignment(HorizontalAlignment.CENTER)
          .Add(imgData.SetAutoScale(true));
      }


      // WORKAROUND: to let the other pages appear correctly
      document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
    }
  }

  public class DSVSchoolRaceResultReport : RaceResultReport
  {
    public DSVSchoolRaceResultReport(Race race) : base(race)
    {
    }

    protected override void addContent(PdfDocument pdf, Document document)
    {
      base.addContent(pdf, document);

      DSVSchoolRaceResultViewProvider resultVP = _race.GetResultViewProvider() as DSVSchoolRaceResultViewProvider;

      if (resultVP.GetDSVRaceCalculationWomen().CalculationValid)
        addPenaltyCalculation(pdf, document, resultVP.GetDSVRaceCalculationWomen(), "Damen/Mädchen");
      if (resultVP.GetDSVRaceCalculationMen().CalculationValid)
        addPenaltyCalculation(pdf, document, resultVP.GetDSVRaceCalculationMen(), "Herren/Buben");
    }


    protected void addPenaltyCalculation(PdfDocument pdf, Document document, DSVRaceCalculation dsvCalc, string subTitle)
    {
      var fontNormal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
      var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
      int fontSizeTitle = 16;
      int fontSizeNormal = 9;

      Paragraph createHeaderParagraph(string text)
      {
        return new Paragraph(text).SetFont(fontBold).SetFontSize(fontSizeNormal);
      }
      Paragraph createParagraph(string text)
      {
        return new Paragraph(text).SetFont(fontNormal).SetFontSize(fontSizeNormal);
      }

      string formatRang(RaceResultItem rri)
      {
        if (rri.Position > 0)
          return string.Format("{0}", rri.Position);
        else
          return rri.ResultCode.ToString();
      }


      document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

      document.Add(
        new Paragraph(string.Format("Zuschlagsberechnung {0}", subTitle))
        .SetFont(fontBold)
        .SetFontSize(fontSizeTitle)
        .SetHorizontalAlignment(HorizontalAlignment.CENTER)
        .SetTextAlignment(TextAlignment.CENTER)
      );

      document.Add(
        new Paragraph(string.Format("F-Wert: {0:0.00}", dsvCalc.ValueF))
        .SetFont(fontNormal)
        .SetFontSize(fontSizeNormal)
      );

      {
        document.Add(
          new Paragraph(string.Format("Die besten 10 klassierten Teilnehmer des Bewerbs:"))
          .SetFont(fontNormal)
          .SetFontSize(fontSizeNormal)
          .SetHorizontalAlignment(HorizontalAlignment.CENTER)
        );

        var table = new Table(new float[] { 1, 1, 1, 1, 1, 1, 1 })
          .SetFontSize(fontSizeNormal)
          .SetFont(fontNormal)
          .SetWidth(UnitValue.CreatePercentValue(100))
          .SetBorder(Border.NO_BORDER);

        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderParagraph("Rang")));
        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderParagraph("Laufzeit")));
        table.AddHeaderCell(createCellForTable(TextAlignment.CENTER)
          .ConfigureHeaderCell()
          .Add(createHeaderParagraph("Code")));
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderParagraph("Name")));
        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderParagraph("Listenpunkte")));
        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderParagraph("Besten Fünf")));
        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderParagraph("Rennpunkte")));

        for (int i = 0; i < dsvCalc.TopTen.Count; i++)
        {
          var item = dsvCalc.TopTen[i];

          Color bgColor = ColorConstants.WHITE;
          if (i % 2 == 1)
            bgColor = PDFHelper.ColorRHBG1;

          table.AddCell(createCellForTable(TextAlignment.RIGHT)
            .SetBackgroundColor(bgColor)
            .Add(createParagraph((string)_positionConverter.Convert(item.RRI.Position, typeof(string), null, null))));

          table.AddCell(createCellForTable(TextAlignment.RIGHT)
            .SetBackgroundColor(bgColor)
            .Add(createParagraph(string.Format("{0}", item.RRI.TotalTime.ToRaceTimeString()))));

          table.AddCell(createCellForTable(TextAlignment.CENTER)
            .SetBackgroundColor(bgColor)
            .Add(createParagraph(item.RRI.Participant.Participant.CodeOrSvId)));
          table.AddCell(createCellForTable()
            .SetBackgroundColor(bgColor)
            .Add(createParagraph(item.RRI.Participant.Participant.Fullname)));

          table.AddCell(createCellForTable(TextAlignment.RIGHT)
            .SetBackgroundColor(bgColor)
            .Add(createParagraph(formatPoints(item.RRI.Participant.Points))));
          if (item.TopFive)
          {
            table.AddCell(createCellForTable(TextAlignment.RIGHT)
              .SetBackgroundColor(bgColor)
              .Add(createParagraph(formatPoints(item.DSVPoints))));
            table.AddCell(createCellForTable(TextAlignment.RIGHT)
              .SetBackgroundColor(bgColor)
              .Add(createParagraph(formatPoints(item.RacePoints))));
          }
          else
          {
            table.AddCell(createCellForTable(TextAlignment.RIGHT)
              .SetBackgroundColor(bgColor)
              );
            table.AddCell(createCellForTable(TextAlignment.RIGHT)
              .SetBackgroundColor(bgColor)
              );
          }
        }

        table.AddCell(createCellForTable(5)
          .SetBorderTop(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin))
          );
        table.AddCell(createCellForTable(TextAlignment.RIGHT)
          .SetBorderTop(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin))
          .Add(createParagraph(formatPoints(dsvCalc.PenaltyA))));
        table.AddCell(createCellForTable(TextAlignment.RIGHT)
          .SetBorderTop(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin))
          .Add(createParagraph(formatPoints(dsvCalc.PenaltyC))));

        table.AddCell(createCellForTable(5));
        table.AddCell(createCellForTable(TextAlignment.RIGHT)
          .Add(createParagraph(">>A<<")));
        table.AddCell(createCellForTable(TextAlignment.RIGHT)
          .Add(createParagraph(">>C<<")));

        document.Add(table);
      }


      {
        document.Add(
          new Paragraph(string.Format("Die besten 5 gestarten Teilnehmer des Bewerbs (laut Punkteliste):"))
          .SetFont(fontNormal)
          .SetFontSize(fontSizeNormal)
          .SetHorizontalAlignment(HorizontalAlignment.CENTER)
        );

        var table = new Table(new float[] { 1, 1, 1, 1, 1, 1 })
          .SetFontSize(fontSizeNormal)
          .SetFont(fontNormal)
          .SetWidth(UnitValue.CreatePercentValue(100))
          .SetBorder(Border.NO_BORDER);

        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderParagraph("Rang")));
        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderParagraph("Laufzeit")));
        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderParagraph("Code")));
        table.AddHeaderCell(createCellForTable(TextAlignment.CENTER)
          .ConfigureHeaderCell()
          .Add(createHeaderParagraph("Name")));
        table.AddHeaderCell(createCellForTable(TextAlignment.LEFT)
          .ConfigureHeaderCell()
          .Add(createHeaderParagraph("Listenpunkte")));
        table.AddHeaderCell(createCellForTable(TextAlignment.RIGHT)
          .ConfigureHeaderCell()
          .Add(createHeaderParagraph("Bewerbsrang")));

        for (int i = 0; i < dsvCalc.TopFiveDSV.Count; i++)
        {
          var item = dsvCalc.TopFiveDSV[i];

          Color bgColor = ColorConstants.WHITE;
          if (i % 2 == 1)
            bgColor = PDFHelper.ColorRHBG1;

          table.AddCell(createCellForTable(TextAlignment.RIGHT)
            .SetBackgroundColor(bgColor)
            .Add(createParagraph(string.Format("{0}", i+1))));

          table.AddCell(createCellForTable(TextAlignment.RIGHT)
            .SetBackgroundColor(bgColor)
            .Add(createParagraph(string.Format("{0}", item.TotalTime.ToRaceTimeString()))));

          table.AddCell(createCellForTable(TextAlignment.CENTER)
            .SetBackgroundColor(bgColor)
            .Add(createParagraph(item.Participant.Participant.CodeOrSvId)));
          table.AddCell(createCellForTable()
            .SetBackgroundColor(bgColor)
            .Add(createParagraph(item.Participant.Participant.Fullname)));

          table.AddCell(createCellForTable(TextAlignment.RIGHT)
            .SetBackgroundColor(bgColor)
            .Add(createParagraph(formatPoints(item.Participant.Points))));
          
          table.AddCell(createCellForTable(TextAlignment.RIGHT)
            .SetBackgroundColor(bgColor)
            .Add(createParagraph(formatRang(item))));
        }

        table.AddCell(createCellForTable(4)
          .SetBorderTop(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin))
          );
        table.AddCell(createCellForTable(TextAlignment.RIGHT)
          .SetBorderTop(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin))
          .Add(createParagraph(formatPoints(dsvCalc.PenaltyB))));
        table.AddCell(createCellForTable(TextAlignment.RIGHT)
          .SetBorderTop(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThin)));

        table.AddCell(createCellForTable(4));
        table.AddCell(createCellForTable(TextAlignment.RIGHT)
          .Add(createParagraph(">>B<<")));
        table.AddCell(createCellForTable(TextAlignment.RIGHT));

        document.Add(table);
      }

      {
        var table = new Table(new float[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 })
          .SetFontSize(fontSizeNormal)
          .SetFont(fontNormal)
          .SetBorder(Border.NO_BORDER);

        table.AddCell(createCellForTable(TextAlignment.LEFT)
          .Add(createParagraph("Berechneter Zuschlag:")));
        table.AddCell(createCellForTable(TextAlignment.RIGHT)
          .Add(createParagraph("(")));
        table.AddCell(createCellForTable(TextAlignment.CENTER)
          .Add(createParagraph(string.Format("{0}", dsvCalc.PenaltyA))));
        table.AddCell(createCellForTable(TextAlignment.CENTER)
          .Add(createParagraph("+")));
        table.AddCell(createCellForTable(TextAlignment.CENTER)
          .Add(createParagraph(string.Format("{0}", dsvCalc.PenaltyB))));
        table.AddCell(createCellForTable(TextAlignment.CENTER)
          .Add(createParagraph("-")));
        table.AddCell(createCellForTable(TextAlignment.CENTER)
          .Add(createParagraph(string.Format("{0}", dsvCalc.PenaltyC))));
        table.AddCell(createCellForTable(TextAlignment.CENTER)
          .Add(createParagraph(") : 10 = ")));
        table.AddCell(createCellForTable(TextAlignment.LEFT)
          .Add(createParagraph(string.Format("{0}", dsvCalc.ExactCalculatedPenalty))));

        table.AddCell(createCellForTable(TextAlignment.LEFT)
          .Add(createParagraph("")));
        table.AddCell(createCellForTable(TextAlignment.RIGHT)
          .Add(createParagraph("")));
        table.AddCell(createCellForTable(TextAlignment.CENTER)
          .Add(createParagraph(">>A<<")));
        table.AddCell(createCellForTable(TextAlignment.CENTER)
          .Add(createParagraph("")));
        table.AddCell(createCellForTable(TextAlignment.CENTER)
          .Add(createParagraph(">>B<<")));
        table.AddCell(createCellForTable(TextAlignment.CENTER)
          .Add(createParagraph("")));
        table.AddCell(createCellForTable(TextAlignment.CENTER)
          .Add(createParagraph(">>C<<")));
        table.AddCell(createCellForTable(TextAlignment.CENTER)
          .Add(createParagraph("")));
        table.AddCell(createCellForTable(TextAlignment.LEFT)
          .Add(createParagraph("")));

        table.AddCell(createCellForTable(TextAlignment.LEFT)
          .Add(createParagraph("Gerundet:")));
        table.AddCell(createCellForTable(7, TextAlignment.RIGHT)
          .Add(createParagraph("")));
        table.AddCell(createCellForTable(TextAlignment.RIGHT)
          .Add(createParagraph(string.Format("{0:0.00}", dsvCalc.CalculatedPenalty))));

        if (dsvCalc.ValueA > 0.0)
        {
          table.AddCell(createCellForTable(TextAlignment.LEFT)
          .Add(createParagraph("Kategorie-Adder:")));
          table.AddCell(createCellForTable(7, TextAlignment.RIGHT)
            .Add(createParagraph("")));
          table.AddCell(createCellForTable(TextAlignment.RIGHT)
            .Add(createParagraph(string.Format("{0:0.00}", dsvCalc.ValueA))));
        }

        if (dsvCalc.ValueZ > 0.0)
        {
          table.AddCell(createCellForTable(TextAlignment.LEFT)
            .Add(createParagraph("Korrekturwert (Z-Wert):")));
          table.AddCell(createCellForTable(7, TextAlignment.RIGHT)
            .Add(createParagraph("")));
          table.AddCell(createCellForTable(TextAlignment.RIGHT)
            .Add(createParagraph(string.Format("{0:0.00}", dsvCalc.ValueZ))));
        }

        table.AddCell(createCellForTable(TextAlignment.LEFT)
          .Add(createParagraph("Punktezuschlag:").SetFont(fontBold)));
        table.AddCell(createCellForTable(7, TextAlignment.RIGHT)
          .Add(createParagraph("")));
        table.AddCell(createCellForTable(TextAlignment.RIGHT)
          .Add(createParagraph(string.Format("{0:0.00}", dsvCalc.CalculatedPenaltyWithAdded)).SetFont(fontBold)));

        table.AddCell(createCellForTable(TextAlignment.LEFT)
          .Add(createParagraph("Minimumzuschlag:")));
        table.AddCell(createCellForTable(7, TextAlignment.RIGHT)
          .Add(createParagraph("")));
        table.AddCell(createCellForTable(TextAlignment.RIGHT)
          .Add(createParagraph(string.Format("{0:0.00}", dsvCalc.MinPenalty))));

        table.AddCell(createCellForTable(9, TextAlignment.LEFT).Add(createParagraph(" ")));

        table.AddCell(createCellForTable(TextAlignment.LEFT)
          .Add(createParagraph("Angewandter Zuschlag:").SetFont(fontBold)));
        table.AddCell(createCellForTable(7, TextAlignment.RIGHT)
          .Add(createParagraph("")));
        table.AddCell(createCellForTable(TextAlignment.RIGHT)
          .Add(createParagraph(string.Format("{0:0.00}", dsvCalc.AppliedPenalty)).SetFont(fontBold)));

        document.Add(table);
      }
    }
  }

}
