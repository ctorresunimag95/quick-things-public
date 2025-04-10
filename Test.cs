using MiniExcelLibs;
using System.IO;

namespace Learning.Excel.Mini.Api;

public class ExcelService
{
    public dynamic GetExcelInfo(IFormFile file)
    {
        var result = new Dictionary<string, dynamic>();

        using var stream = file.OpenReadStream();

        var sheetNames = stream.GetSheetNames().ToList();

        //stream.Position = 0;

        foreach (var sheet in sheetNames)
        {
            // Rewind again before each sheet
            //stream.Position = 0;

            var rows = stream.Query(sheetName: sheet).Cast<IDictionary<string, object>>();

            var headers = FindHeuristicHeaders(rows);
            if (headers.Any())
            {
                return headers;
            }
        }

        return result;
    }

    private static List<string> FindHeuristicHeaders(IEnumerable<IDictionary<string, object>> rows)
    {
        var rowsList = rows.ToArray();

        for (int i = 0; i < rowsList.Length - 1; i++)
        {
            var currentRow = rowsList[i];
            var nextRow = rowsList[i + 1];

            if (currentRow.Count <= 1) continue;

            bool hasOnlyText = currentRow.All(cell => !double.TryParse(Convert.ToString(cell.Value), out _));

            bool nextRowHasData = nextRow.Count == currentRow.Count &&
                                  nextRow.Any(c => c.Value is not null && !string.IsNullOrWhiteSpace(c.Value.ToString()));

            if (hasOnlyText && nextRowHasData)
            {
                return currentRow.Select(c => c.Value.ToString() ?? string.Empty).ToList();
            }
        }

        return new List<string>();
    }
}
