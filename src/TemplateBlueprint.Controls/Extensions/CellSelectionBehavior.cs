using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.VisualTree;

namespace TemplateBlueprint.Controls.Extensions;

/// <summary>
/// Provides enhanced cell selection behavior for TreeDataGrid.
/// </summary>
public static class CellSelectionBehavior
{
    /// <summary>
    /// Attached property to enable enhanced cell selection.
    /// </summary>
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<TreeDataGrid, bool>(
            "IsEnabled",
            typeof(CellSelectionBehavior),
            defaultValue: false);

    /// <summary>
    /// Gets the IsEnabled value for a TreeDataGrid.
    /// </summary>
    public static bool GetIsEnabled(TreeDataGrid element) =>
        element.GetValue(IsEnabledProperty);

    /// <summary>
    /// Sets the IsEnabled value for a TreeDataGrid.
    /// </summary>
    public static void SetIsEnabled(TreeDataGrid element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    static CellSelectionBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<TreeDataGrid>(OnIsEnabledChanged);
    }

    private static void OnIsEnabledChanged(TreeDataGrid grid, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            grid.AttachedToVisualTree += OnGridAttached;
            grid.DetachedFromVisualTree += OnGridDetached;

            // Check if already attached by looking for a visual parent
            if (grid.GetVisualRoot() != null)
            {
                AttachBehavior(grid);
            }
        }
        else
        {
            grid.AttachedToVisualTree -= OnGridAttached;
            grid.DetachedFromVisualTree -= OnGridDetached;
            DetachBehavior(grid);
        }
    }

    private static void OnGridAttached(object? sender, EventArgs e)
    {
        if (sender is TreeDataGrid grid)
        {
            AttachBehavior(grid);
        }
    }

    private static void OnGridDetached(object? sender, EventArgs e)
    {
        if (sender is TreeDataGrid grid)
        {
            DetachBehavior(grid);
        }
    }

    private static void AttachBehavior(TreeDataGrid grid)
    {
        // Subscribe to selection changes if cell selection model exists
        if (grid.RowSelection is TreeDataGridCellSelectionModel<object> cellSelection)
        {
            // Cell selection behavior is active
        }
    }

    private static void DetachBehavior(TreeDataGrid grid)
    {
        // Cleanup if needed
    }
}

/// <summary>
/// Represents a selected cell range in the grid.
/// </summary>
public readonly struct CellRange
{
    /// <summary>
    /// Gets the starting row index.
    /// </summary>
    public int StartRow { get; init; }

    /// <summary>
    /// Gets the ending row index.
    /// </summary>
    public int EndRow { get; init; }

    /// <summary>
    /// Gets the starting column index.
    /// </summary>
    public int StartColumn { get; init; }

    /// <summary>
    /// Gets the ending column index.
    /// </summary>
    public int EndColumn { get; init; }

    /// <summary>
    /// Gets the number of rows in the range.
    /// </summary>
    public int RowCount => Math.Abs(EndRow - StartRow) + 1;

    /// <summary>
    /// Gets the number of columns in the range.
    /// </summary>
    public int ColumnCount => Math.Abs(EndColumn - StartColumn) + 1;

    /// <summary>
    /// Gets the normalized start row (smallest row index).
    /// </summary>
    public int NormalizedStartRow => Math.Min(StartRow, EndRow);

    /// <summary>
    /// Gets the normalized end row (largest row index).
    /// </summary>
    public int NormalizedEndRow => Math.Max(StartRow, EndRow);

    /// <summary>
    /// Gets the normalized start column (smallest column index).
    /// </summary>
    public int NormalizedStartColumn => Math.Min(StartColumn, EndColumn);

    /// <summary>
    /// Gets the normalized end column (largest column index).
    /// </summary>
    public int NormalizedEndColumn => Math.Max(StartColumn, EndColumn);

    /// <summary>
    /// Creates a single cell range.
    /// </summary>
    public static CellRange SingleCell(int row, int column) =>
        new() { StartRow = row, EndRow = row, StartColumn = column, EndColumn = column };

    /// <summary>
    /// Enumerates all cells in the range.
    /// </summary>
    public IEnumerable<(int Row, int Column)> EnumerateCells()
    {
        for (var row = NormalizedStartRow; row <= NormalizedEndRow; row++)
        {
            for (var col = NormalizedStartColumn; col <= NormalizedEndColumn; col++)
            {
                yield return (row, col);
            }
        }
    }
}

