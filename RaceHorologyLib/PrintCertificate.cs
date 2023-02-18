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
  }


  public class Certificates : PDFBaseRaceReport
  {
    public Certificates(Race race)
      : base(race) 
    {

    }

    protected override void GenerateImpl(PdfDocument pdf, Document document, DateTime? creationDateTime = null)
    {
      
    }

    protected override Margins getMargins() { return new Margins { Top = 0.0F, Bottom = 0.0F, Left = 0.0F, Right = 0.0F }; }

    protected override string getReportName() { return "Urkunden"; }

  }
}
