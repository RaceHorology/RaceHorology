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
    const int ColumnsForStartnumberTable = 13;
    const int MinRowsForNaS = 2;
    const int MinRowsForNiZ = 3;

    public RefereeProtocol(Race race)
      : base(race) 
    {
    }

    protected override void addContent(PdfDocument pdf, Document document)
    {
      foreach(var rr in _race.GetRuns())
      {
        addRaceRun(document, rr);
      }
    }

    protected void addRaceRun(Document document, RaceRun rr)
    {
      {
        document.Add(new Paragraph("Nicht am Start"));
        Table table = getStartnumberTable(
          rr.GetResultList().Where(r => r.ResultCode == RunResult.EResultCode.NaS).Select(r => r.StartNumber), 
          ColumnsForStartnumberTable, MinRowsForNaS);
        document.Add(table);
      }

      {
        document.Add(new Paragraph("Nicht im Ziel"));
        Table table = getStartnumberTable(
          rr.GetResultList().Where(r => r.ResultCode == RunResult.EResultCode.NiZ).Select(r => r.StartNumber),
          ColumnsForStartnumberTable, MinRowsForNiZ);
        document.Add(table);
      }

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
