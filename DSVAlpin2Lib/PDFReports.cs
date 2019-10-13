using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSVAlpin2Lib
{
  public class PDFReport1
  {
    public static void Test()
    {

      var writer = new PdfWriter("test.pdf");
      var pdf = new PdfDocument(writer);
      var document = new Document(pdf);
      document.Add(new Paragraph("Hello World!"));
      document.Close();
    }
  }
}
