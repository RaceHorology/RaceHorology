using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{
  public class RefereeProtocol : PDFRaceReport
  {
    public RefereeProtocol(Race race)
      : base(race) 
    {
    }

    protected override void addContent(PdfDocument pdf, Document document)
    {

      document.Add(new Paragraph("Nicht am Start"));
      //List<uint> stnr = new List<uint>()[1, 2, 3, 4, 5, 6, 7];
      Table table = getStartnumberTable(new uint[] { 1U, 2U, 3U, 4U, 5U, 6U, 7U, 1U, 2U, 3U, 4U, 5U, 6U, 7U, 1U, 2U, 3U, 4U, 5U, 6U, 7U, 1U, 2U, 3U, 4U, 5U, 6U, 100U }, 13, 2);
      document.Add(table);
    }

    protected override string getReportName()
    {
      return "Schiedsrichterprotokoll";
    }

    protected override string getTitle()
    {
      return "Schiedsrichterprotokoll";
    }


    protected Table getStartnumberTable(IEnumerable<uint> stnr, int columns = 13, int minRows = 2)
    {
      var table = new Table(UnitValue.CreatePercentArray(Enumerable.Repeat(1.0F, columns).ToArray()));
      table.SetWidth(UnitValue.CreatePercentValue(100));

      var eStNr = stnr.GetEnumerator();
      bool moreValues = true;
      uint j = 0;
      while (moreValues || j < minRows)
      {
        for (uint i = 0; i < columns; i++)
        {
          moreValues = eStNr.MoveNext();
          Cell cell = null;
          table.AddCell(cell = new Cell()
            .SetBorder(new SolidBorder(PDFHelper.SolidBorderThin))
            .SetMinHeight(UnitValue.CreatePointValue(1.0F / 2.54F * 72))
            .SetTextAlignment(TextAlignment.CENTER)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
          );
          if (moreValues)
            cell.Add(new Paragraph(string.Format("{0}", eStNr.Current)));
        }
        j++;
      }

      table.SetBorder(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick));

      return table;
    }
  }
}
