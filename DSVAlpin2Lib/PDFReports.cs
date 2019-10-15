using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{


  class EndPageHandler : IEventHandler
  {
    public virtual void HandleEvent(Event @event)
    {
      PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
      PdfDocument pdfDoc = docEvent.GetDocument();
      PdfPage page = docEvent.GetPage();

      int pageNumber = pdfDoc.GetPageNumber(page);
      Rectangle pageSize = page.GetPageSize();
      PdfCanvas pdfCanvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);
      
      //Set background
      Color limeColor = new DeviceCmyk(0.208f, 0, 0.584f, 0);
      Color blueColor = new DeviceCmyk(0.445f, 0.0546f, 0, 0.0667f);
      pdfCanvas.SaveState()
                  .SetFillColor(pageNumber % 2 == 1 ? limeColor : blueColor)
                  .Rectangle(pageSize.GetLeft(), pageSize.GetBottom(), pageSize.GetWidth(), pageSize.GetHeight())
                  .Fill()
                  .RestoreState();
      Image logo1 = getImage("Logo1");
      Image logo2 = getImage("Logo2");
      Image logo3 = getImage("Logo3");

      Rectangle area1 = new Rectangle(0, pageSize.GetTop()- pageSize.GetWidth() * 0.2F * logo1.GetImageHeight() / logo1.GetImageWidth(), pageSize.GetWidth() * 0.2F, pageSize.GetWidth() * 0.2F * logo1.GetImageHeight() / logo1.GetImageWidth());
      new Canvas(pdfCanvas, pdfDoc, area1).Add(logo1);

      Rectangle area2 = new Rectangle(pageSize.GetRight() - pageSize.GetWidth()*0.2F, pageSize.GetTop() - pageSize.GetWidth() * 0.2F * logo2.GetImageHeight() / logo2.GetImageWidth(), pageSize.GetWidth() * 0.2F, pageSize.GetWidth() * 0.2F*logo2.GetImageHeight()/logo2.GetImageWidth());
      new Canvas(pdfCanvas, pdfDoc, area2).Add(logo2);

      Rectangle area3 = new Rectangle(pageSize.GetWidth(), pageSize.GetWidth() * logo3.GetImageHeight() / logo3.GetImageWidth());
      new Canvas(pdfCanvas, pdfDoc, area3).Add(logo3);


      //Add header and footer
      pdfCanvas.BeginText()
                  .SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA), 9)
                  .MoveText(pageSize.GetWidth() / 2 - 60, pageSize.GetTop() - 20)
                  .ShowText("THE TRUTH IS OUT THERE")
                  .MoveText(60, -pageSize.GetTop() + 30)
                  .ShowText(pageNumber.ToString())
                  .EndText();

      
      pdfCanvas.Release();
    }



    string getResourcePath()
    {
      return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"resources\pdf");
    }

    string findImage(string filenameWOExt)
    {
      var files = Directory.GetFiles(getResourcePath(), string.Format("{0}*", filenameWOExt));
      if (files.Length > 0)
        return files[0];

      return null;
    }

    Image getImage(string filenameWOExt)
    {
      Image img = null;

      string imgPath = findImage(filenameWOExt);
      if (!string.IsNullOrEmpty(imgPath))
        img = new Image(ImageDataFactory.Create(imgPath));

      return img;
    }
  }




  public class PDFReport
  {
    public PDFReport()
    {

    }



    public void Generate()
    {

      var writer = new PdfWriter("test.pdf");
      var pdf = new PdfDocument(writer);

      pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new EndPageHandler());

      
      var document = new Document(pdf, PageSize.A4);

      document.SetMargins(50, 10, 50, 10);

      document.Add(new Paragraph("Hello World!"));

      
      var table = new Table(new float[] { 2, 1, 3,1 });
      table.SetWidth(UnitValue.CreatePercentValue(100));

      table.AddHeaderCell(new Cell().Add(new Paragraph("Text")));

      for(int i=1; i<999; i++)
      {
        table.AddCell(new Cell().Add(new Paragraph(string.Format("{0}", i))));

      }

      document.Add(table);

      document.Close();
    }
  }
}
