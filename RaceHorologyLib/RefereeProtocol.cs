using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Layout;
using iText.Layout.Properties;
using iText.Layout.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceHorologyLib
{

  /* This Table render skips the last table rows (skipMaxNum). It is assumed that at least the last skipMaxNum rows are empty rows.
   * This is a workaround for printing the table until the end of the page with empty rows.
   */
  class MyTableRenderer : TableRenderer {
    private int skipMaxNum;

    public MyTableRenderer(Table modelElement, Table.RowRange rowRange, int skipMaxNum)
      : base(modelElement, rowRange)
    {
      this.skipMaxNum = skipMaxNum;
    }
    public MyTableRenderer(Table modelElement, int skipMaxNum)
      : base(modelElement)
    {
      this.skipMaxNum = skipMaxNum;
    }

    public override IRenderer GetNextRenderer()
    {
      return new MyTableRenderer(modelElement as Table, rowRange, skipMaxNum);
    }

    public override LayoutResult Layout(LayoutContext layoutContext)
    {
      var remainingRows = rowRange.GetFinishRow() - rowRange.GetStartRow() + 1;
      if (remainingRows < skipMaxNum)
      {
        LayoutResult result = new LayoutResult(LayoutResult.FULL, new LayoutArea(layoutContext.GetArea().GetPageNumber(), new iText.Kernel.Geom.Rectangle(0, 0)), null, null);
        return result;
      }

      return base.Layout(layoutContext);
    }


    protected override TableRenderer[] Split(int row, bool hasContent, bool cellWithBigRowspanAdded)
    {
      var ret = base.Split(row, hasContent, cellWithBigRowspanAdded);
      return ret;
    }
  }

  /** This cell renderer ensures that the cells are not broken across pages
   */
  class CustomCellRenderer : CellRenderer
  {
    public CustomCellRenderer(Cell modelElement)
      : base(modelElement)
    {
    }

    public override LayoutResult Layout(LayoutContext layoutContext)
    {
      LayoutResult result = base.Layout(layoutContext);
      if (LayoutResult.FULL != result.GetStatus())
      {
        result.SetStatus(LayoutResult.NOTHING);
        result.SetSplitRenderer(null);
        result.SetOverflowRenderer(this);
      }
      return result;
    }

    override public IRenderer GetNextRenderer()
    {
      return new CustomCellRenderer((Cell)GetModelElement());
    }
  }


  // TODO:
  // - Unterschriftenzeile,

  public class RefereeProtocol : PDFRaceReport
  {
    const int ColumnsForStartnumberTable = 13;
    const int MinRowsForNaS = 2;
    const int MinRowsForNiZ = 3;
    const int MinRowsForDIS = 12;
    const float LineHeight = 8.0F;

    RaceRun _raceRun;

    public RefereeProtocol(RaceRun rr)
      : base(rr.GetRace()) 
    {
      _raceRun = rr;
    }

    protected override ReportHeader createHeader()
    {
      return new ReportHeader(_pdfDocument, _document, _pdfHelper, _race, getTitle(), _pageMargins, false);
    }
    protected override ReportFooter createFooter()
    {
      return new ReportFooter(_pdfDocument, _document, _pdfHelper, _race, getTitle(), _pageMargins, creationDateTime(), false);
    }

    protected override void addContent(PdfDocument pdf, Document document)
    {
      addRaceRun(document, _raceRun);
    }

    protected void addRaceRun(Document document, RaceRun rr)
    {
      {
        document.Add(new Paragraph("Nicht am Start")
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
         );
        Table table = getStartnumberTable(
          rr.GetResultList().Where(r => r.ResultCode == RunResult.EResultCode.NaS).Select(r => r.StartNumber), 
          ColumnsForStartnumberTable, MinRowsForNaS);
        table.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA));
        document.Add(table);
      }

      {
        document.Add(new Paragraph("Nicht im Ziel")
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
        );
        Table table = getStartnumberTable(
          rr.GetResultList().Where(r => r.ResultCode == RunResult.EResultCode.NiZ).Select(r => r.StartNumber),
          ColumnsForStartnumberTable, MinRowsForNiZ);
        table.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA));
        document.Add(table);
      }

      {
        document.Add(new Paragraph("Disqualifiziert")
          .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
        );
        Table table = getDisqualifiedTable(document,
          rr.GetResultList().Where(r => r.ResultCode == RunResult.EResultCode.DIS));
        table.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA));
        table.SetNextRenderer(new MyTableRenderer(table, MinRowsForDIS));
        document.Add(table);
      }
    }

    protected override string getReportName()
    {
      return string.Format("Schiedsrichterprotokoll {0}. Durchgang", _raceRun.Run);
    }

    protected override string getTitle()
    {
      return string.Format("Schiedsrichterprotokoll\n{0}. Durchgang", _raceRun.Run);
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
            .SetMinHeight(UnitValue.CreatePointValue(LineHeight / 25.4F * 72))
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


    protected Table getDisqualifiedTable(Document doc, IEnumerable<RunResult> items)
    {
      var table = new Table(Enumerable.Repeat(1.0F, 5).ToArray());
      table.SetWidth(UnitValue.CreatePercentValue(100));

      doc.Add(table);

      addDisqualifiedTableHeader(table);
      addDisqualifiedItems(table, items, 0);
      addDisqualifiedItems(table, new List<RunResult>(), MinRowsForDIS); // Add 15 empty lines

      table.SetBorder(new SolidBorder(PDFHelper.ColorRHFG1, PDFHelper.SolidBorderThick));

      return table;
    }


    private static Cell createCellDis()
    {
      var cell = new Cell()
        .SetBorder(new SolidBorder(PDFHelper.SolidBorderThin))
        .SetMinHeight(UnitValue.CreatePointValue(LineHeight / 25.4F * 72))
        .SetVerticalAlignment(VerticalAlignment.MIDDLE);
      cell.SetNextRenderer(new CustomCellRenderer(cell));
      return cell;
    }

    void addDisqualifiedTableHeader(Table table)
    {
      table.AddCell(createCellDis()
        .SetTextAlignment(TextAlignment.CENTER)
        .Add(new Paragraph("StNr").SetBold())
      );
      table.AddCell(createCellDis()
        .SetTextAlignment(TextAlignment.LEFT)
        .Add(new Paragraph("Name").SetBold())
      );
      table.AddCell(createCellDis()
        .SetTextAlignment(TextAlignment.CENTER)
        .Add(new Paragraph("Tor").SetBold())
      );
      table.AddCell(createCellDis()
        .SetTextAlignment(TextAlignment.LEFT)
        .Add(new Paragraph("Torrichter").SetBold())
      );
      table.AddCell(createCellDis()
        .SetTextAlignment(TextAlignment.LEFT)
        .Add(new Paragraph("Bemerkung").SetBold())
      );
    }

    void addDisqualifiedItems(Table table, IEnumerable<RunResult> items, uint minRows)
    {
      var item = items.GetEnumerator();
      bool moreValues = true;
      uint i = 0;
      while (moreValues || i < minRows)
      {
        moreValues = item.MoveNext();
        RunResult rr = moreValues ? item.Current : null;

        table.AddCell(createCellDis()
          .SetTextAlignment(TextAlignment.CENTER)
          .Add(new Paragraph(rr != null ? string.Format("{0}", rr?.StartNumber) : ""))
        );
        table.AddCell(createCellDis()
          .SetTextAlignment(TextAlignment.LEFT)
          .Add(new Paragraph(rr != null ? rr?.Participant.Fullname : ""))
        );
        table.AddCell(createCellDis()
          .SetTextAlignment(TextAlignment.CENTER)
          .Add(new Paragraph(rr != null ? rr?.GetDisqualifyGoal() : ""))
        );
        table.AddCell(createCellDis()
          .SetTextAlignment(TextAlignment.LEFT)
        );
        table.AddCell(createCellDis()
          .SetTextAlignment(TextAlignment.LEFT)
          .Add(new Paragraph(rr != null ? rr?.GetDisqualifyText() : ""))
        );

        i++;
      }
    }
  }
}
