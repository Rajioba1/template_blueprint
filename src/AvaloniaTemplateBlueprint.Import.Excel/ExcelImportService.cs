using System.Globalization;
using AvaloniaTemplateBlueprint.Core.Contracts;
using ClosedXML.Excel;

namespace AvaloniaTemplateBlueprint.Import.Excel;

/// <summary>
/// Excel import service using ClosedXML.
/// This is an optional package - only add if you need .xlsx import.
/// </summary>
public class ExcelImportService : IDataImportService
{
    /// <inheritdoc />
    public IEnumerable<string> SupportedExtensions => new[] { ".xlsx", ".xls", ".xlsm" };

    /// <inheritdoc />
    public async Task<ImportResult> ImportAsync(string filePath)
    {
        return await Task.Run(() => ImportInternal(filePath));
    }

    private static ImportResult ImportInternal(string filePath)
    {
        try
        {
            var columns = new List<SpreadsheetColumn>();
            var rows = new List<SpreadsheetRow>();

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);

            // Find used range
            var usedRange = worksheet.RangeUsed();
            if (usedRange == null)
            {
                return new ImportResult(true, columns, rows);
            }

            var firstRow = usedRange.FirstRow();
            var lastColumn = usedRange.LastColumn().ColumnNumber();
            var lastRow = usedRange.LastRow().RowNumber();

            // Read headers
            for (var col = 1; col <= lastColumn; col++)
            {
                var cell = firstRow.Cell(col);
                var headerText = cell.GetString();
                if (string.IsNullOrWhiteSpace(headerText))
                {
                    headerText = GetColumnName(col);
                }

                columns.Add(new SpreadsheetColumn
                {
                    Id = $"col_{col - 1}",
                    Name = headerText,
                    Index = col - 1,
                    DataType = ColumnDataType.Text
                });
            }

            // Read data rows
            for (var row = 2; row <= lastRow; row++)
            {
                var dataRow = new SpreadsheetRow { Index = row - 2 };

                for (var col = 1; col <= lastColumn; col++)
                {
                    var cell = worksheet.Cell(row, col);
                    var value = GetCellValue(cell);
                    dataRow.Values[columns[col - 1].Id] = value;
                }

                rows.Add(dataRow);
            }

            // Detect column data types
            foreach (var column in columns)
            {
                column.DataType = DetectColumnType(rows, column.Id);
            }

            return new ImportResult(true, columns, rows);
        }
        catch (Exception ex)
        {
            return new ImportResult(false, Array.Empty<SpreadsheetColumn>(), Array.Empty<SpreadsheetRow>(), ex.Message);
        }
    }

    private static object? GetCellValue(IXLCell cell)
    {
        if (cell.IsEmpty())
            return null;

        return cell.DataType switch
        {
            XLDataType.Number => cell.GetDouble(),
            XLDataType.Boolean => cell.GetBoolean(),
            XLDataType.DateTime => cell.GetDateTime(),
            XLDataType.TimeSpan => cell.GetTimeSpan(),
            _ => cell.GetString()
        };
    }

    private static string GetColumnName(int columnNumber)
    {
        var columnName = string.Empty;
        while (columnNumber > 0)
        {
            var modulo = (columnNumber - 1) % 26;
            columnName = Convert.ToChar('A' + modulo) + columnName;
            columnNumber = (columnNumber - modulo) / 26;
        }
        return columnName;
    }

    private static ColumnDataType DetectColumnType(IReadOnlyList<SpreadsheetRow> rows, string columnId)
    {
        if (rows.Count == 0) return ColumnDataType.Text;

        var sampleSize = Math.Min(100, rows.Count);
        var numericCount = 0;
        var integerCount = 0;
        var dateCount = 0;
        var boolCount = 0;
        var totalNonEmpty = 0;

        for (var i = 0; i < sampleSize; i++)
        {
            var value = rows[i][columnId];
            if (value == null) continue;

            totalNonEmpty++;

            if (value is double or int or float or decimal)
            {
                numericCount++;
                if (value is int || (value is double d && d == Math.Floor(d)))
                    integerCount++;
            }
            else if (value is DateTime)
            {
                dateCount++;
            }
            else if (value is bool)
            {
                boolCount++;
            }
            else if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var dVal))
                {
                    numericCount++;
                    if (dVal == Math.Floor(dVal) && int.TryParse(str, out _))
                        integerCount++;
                }
                else if (DateTime.TryParse(str, out _))
                {
                    dateCount++;
                }
                else if (bool.TryParse(str, out _) ||
                         str.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                         str.Equals("no", StringComparison.OrdinalIgnoreCase))
                {
                    boolCount++;
                }
            }
        }

        if (totalNonEmpty == 0) return ColumnDataType.Text;

        var threshold = totalNonEmpty * 0.8;

        if (integerCount >= threshold) return ColumnDataType.Integer;
        if (numericCount >= threshold) return ColumnDataType.Number;
        if (dateCount >= threshold) return ColumnDataType.Date;
        if (boolCount >= threshold) return ColumnDataType.Boolean;

        return ColumnDataType.Text;
    }
}
