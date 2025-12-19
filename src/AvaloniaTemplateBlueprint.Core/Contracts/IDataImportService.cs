namespace AvaloniaTemplateBlueprint.Core.Contracts;

/// <summary>
/// Result of a data import operation.
/// </summary>
public record ImportResult(
    bool Success,
    IReadOnlyList<SpreadsheetColumn> Columns,
    IReadOnlyList<SpreadsheetRow> Rows,
    string? ErrorMessage = null
);

/// <summary>
/// Service for importing data from files.
/// </summary>
public interface IDataImportService
{
    /// <summary>
    /// Imports data from a file.
    /// </summary>
    /// <param name="filePath">The path to the file to import.</param>
    /// <returns>The import result.</returns>
    Task<ImportResult> ImportAsync(string filePath);

    /// <summary>
    /// Gets the supported file extensions (e.g., ".csv", ".xlsx").
    /// </summary>
    IEnumerable<string> SupportedExtensions { get; }
}
