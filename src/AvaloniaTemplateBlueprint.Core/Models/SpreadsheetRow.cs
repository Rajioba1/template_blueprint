namespace AvaloniaTemplateBlueprint.Core.Contracts;

/// <summary>
/// Represents a row in spreadsheet data.
/// </summary>
public class SpreadsheetRow
{
    /// <summary>
    /// Gets or sets the row index (0-based, excluding header).
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the cell values keyed by column ID.
    /// </summary>
    public Dictionary<string, object?> Values { get; set; } = new();

    /// <summary>
    /// Gets a cell value by column ID.
    /// </summary>
    /// <param name="columnId">The column ID.</param>
    /// <returns>The cell value, or null if not found.</returns>
    public object? this[string columnId]
    {
        get => Values.TryGetValue(columnId, out var value) ? value : null;
        set => Values[columnId] = value;
    }

    /// <summary>
    /// Gets a cell value as a string.
    /// </summary>
    /// <param name="columnId">The column ID.</param>
    /// <returns>The string representation of the value.</returns>
    public string GetString(string columnId)
    {
        return this[columnId]?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Gets a cell value as a double.
    /// </summary>
    /// <param name="columnId">The column ID.</param>
    /// <returns>The double value, or null if not a valid number.</returns>
    public double? GetDouble(string columnId)
    {
        var value = this[columnId];
        if (value is double d) return d;
        if (value is int i) return i;
        if (value is float f) return f;
        if (double.TryParse(value?.ToString(), out var parsed)) return parsed;
        return null;
    }
}
