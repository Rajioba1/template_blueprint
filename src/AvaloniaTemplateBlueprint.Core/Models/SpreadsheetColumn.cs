namespace AvaloniaTemplateBlueprint.Core.Contracts;

/// <summary>
/// Represents a column in spreadsheet data.
/// </summary>
public class SpreadsheetColumn
{
    /// <summary>
    /// Gets or sets the unique identifier for this column.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the column (from header row).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the index of this column (0-based).
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the detected data type of this column.
    /// </summary>
    public ColumnDataType DataType { get; set; } = ColumnDataType.Text;

    /// <summary>
    /// Gets or sets the width of the column for display purposes.
    /// </summary>
    public double Width { get; set; } = 100;
}

/// <summary>
/// Data types that can be detected in columns.
/// </summary>
public enum ColumnDataType
{
    Text,
    Number,
    Integer,
    Date,
    Boolean,
    Mixed
}
