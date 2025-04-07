using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Learning.ExcelReader.Api;

public class ExcelService
{
    public FileInfo GetExcelInfo(IFormFile file)
    {
        using var stream = new MemoryStream();
        file.CopyTo(stream);

        using SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(stream, false);
        
        // Access the first worksheet
        var workbookPart = spreadsheet.WorkbookPart;

        var sheets = new List<SheetInfo>();


        foreach(var sheet in workbookPart!.Workbook.Sheets!.OfType<Sheet>())
        {
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);

            var headers = GetHeaders(worksheetPart, workbookPart);

            sheets.Add(new SheetInfo(sheet.Name?.Value ?? "Sheet", headers));
        }

        return new FileInfo(file.Name, sheets.AsReadOnly());
    }

    private static List<string> GetHeaders(WorksheetPart worksheetPart, WorkbookPart workbookPart)
    {
        // Get the first row (header row)
        var headers = new List<string>();
        var rows = worksheetPart.Worksheet.Descendants<Row>();
        var headerRow = rows.FirstOrDefault();

        if (headerRow == null) return headers;

        foreach (var cell in headerRow.Elements<Cell>())
        {
            string headerValue = GetCellValue(workbookPart, cell);
            headers.Add(headerValue);
        }

        return headers;
    }

    private static string GetCellValue(WorkbookPart workbookPart, Cell? cell)
    {
        if (cell?.CellValue == null) return string.Empty;

        string value = cell.CellValue.Text;

        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString
            && workbookPart.SharedStringTablePart is not null)
        {
            var stringTable = workbookPart.SharedStringTablePart.SharedStringTable;
            return stringTable.ElementAt(int.Parse(value)).InnerText;
        }

        return value;
    }
}

public record FileInfo(string Name, IReadOnlyCollection<SheetInfo> Sheets);

public record SheetInfo(string Name, IEnumerable<string> Headers);