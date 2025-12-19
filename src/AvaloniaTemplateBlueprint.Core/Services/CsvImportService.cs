using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using AvaloniaTemplateBlueprint.Core.Contracts;

namespace AvaloniaTemplateBlueprint.Core.Services;

/// <summary>
/// Default CSV import service using CsvHelper.
/// </summary>
public class CsvImportService : IDataImportService
{
    /// <inheritdoc />
    public IEnumerable<string> SupportedExtensions => new[] { ".csv", ".txt", ".tsv" };

    /// <inheritdoc />
    public async Task<ImportResult> ImportAsync(string filePath)
    {
        try
        {
            var columns = new List<SpreadsheetColumn>();
            var rows = new List<SpreadsheetRow>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null,
                DetectDelimiter = true
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            // Read header
            await csv.ReadAsync();
            csv.ReadHeader();

            var headers = csv.HeaderRecord ?? Array.Empty<string>();
            for (var i = 0; i < headers.Length; i++)
            {
                columns.Add(new SpreadsheetColumn
                {
                    Id = $"col_{i}",
                    Name = headers[i],
                    Index = i,
                    DataType = ColumnDataType.Text
                });
            }

            // Read rows
            var rowIndex = 0;
            while (await csv.ReadAsync())
            {
                var row = new SpreadsheetRow { Index = rowIndex++ };

                for (var i = 0; i < columns.Count; i++)
                {
                    var value = csv.GetField(i);
                    row.Values[columns[i].Id] = value;
                }

                rows.Add(row);
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
            var value = rows[i].GetString(columnId);
            if (string.IsNullOrWhiteSpace(value)) continue;

            totalNonEmpty++;

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
            {
                numericCount++;
                if (d == Math.Floor(d) && int.TryParse(value, out _))
                    integerCount++;
            }
            else if (DateTime.TryParse(value, out _))
            {
                dateCount++;
            }
            else if (bool.TryParse(value, out _) || value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                     value.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
                boolCount++;
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
