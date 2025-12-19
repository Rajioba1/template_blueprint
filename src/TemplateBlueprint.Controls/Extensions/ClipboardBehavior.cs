using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;

namespace TemplateBlueprint.Controls.Extensions;

/// <summary>
/// Provides clipboard copy/paste behavior for TreeDataGrid.
/// </summary>
public static class ClipboardBehavior
{
    /// <summary>
    /// Attached property to enable clipboard behavior.
    /// </summary>
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<TreeDataGrid, bool>(
            "IsEnabled",
            typeof(ClipboardBehavior),
            defaultValue: false);

    /// <summary>
    /// Attached property for the data provider function.
    /// </summary>
    public static readonly AttachedProperty<Func<CellRange, string[,]>?> DataProviderProperty =
        AvaloniaProperty.RegisterAttached<TreeDataGrid, Func<CellRange, string[,]>?>(
            "DataProvider",
            typeof(ClipboardBehavior));

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

    /// <summary>
    /// Gets the data provider function.
    /// </summary>
    public static Func<CellRange, string[,]>? GetDataProvider(TreeDataGrid element) =>
        element.GetValue(DataProviderProperty);

    /// <summary>
    /// Sets the data provider function that returns cell values for a range.
    /// </summary>
    public static void SetDataProvider(TreeDataGrid element, Func<CellRange, string[,]>? value) =>
        element.SetValue(DataProviderProperty, value);

    static ClipboardBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<TreeDataGrid>(OnIsEnabledChanged);
    }

    private static void OnIsEnabledChanged(TreeDataGrid grid, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            grid.KeyDown += OnKeyDown;
        }
        else
        {
            grid.KeyDown -= OnKeyDown;
        }
    }

    private static async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TreeDataGrid grid)
            return;

        var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);

        if (ctrl && e.Key == Key.C)
        {
            await CopyToClipboardAsync(grid);
            e.Handled = true;
        }
    }

    /// <summary>
    /// Copies the selected cells to clipboard in tab-separated format.
    /// </summary>
    public static async Task CopyToClipboardAsync(TreeDataGrid grid)
    {
        var clipboard = TopLevel.GetTopLevel(grid)?.Clipboard;
        if (clipboard == null)
            return;

        var dataProvider = GetDataProvider(grid);
        if (dataProvider == null)
            return;

        // For now, just copy what's selected (implementation depends on selection model)
        // This is a basic implementation - extend based on your selection handling
        var text = GetSelectedText(grid, dataProvider);
        if (!string.IsNullOrEmpty(text))
        {
            await clipboard.SetTextAsync(text);
        }
    }

    /// <summary>
    /// Gets the text representation of selected cells.
    /// </summary>
    private static string GetSelectedText(TreeDataGrid grid, Func<CellRange, string[,]> dataProvider)
    {
        // This is a placeholder - actual implementation depends on selection model
        // Override this by providing a custom data provider that reads from your selection
        return string.Empty;
    }

    /// <summary>
    /// Converts a 2D array of values to tab-separated text.
    /// </summary>
    public static string ToTabSeparatedText(string[,] data)
    {
        var rows = data.GetLength(0);
        var cols = data.GetLength(1);

        var sb = new StringBuilder();
        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < cols; col++)
            {
                if (col > 0)
                    sb.Append('\t');

                var value = data[row, col] ?? string.Empty;
                // Escape tabs and newlines in values
                if (value.Contains('\t') || value.Contains('\n') || value.Contains('\r'))
                {
                    value = "\"" + value.Replace("\"", "\"\"") + "\"";
                }
                sb.Append(value);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Parses tab-separated text into a 2D array.
    /// </summary>
    public static string[,] FromTabSeparatedText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new string[0, 0];

        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();

        if (lines.Count == 0)
            return new string[0, 0];

        var maxCols = lines.Max(l => l.Split('\t').Length);
        var result = new string[lines.Count, maxCols];

        for (var row = 0; row < lines.Count; row++)
        {
            var cells = lines[row].Split('\t');
            for (var col = 0; col < cells.Length; col++)
            {
                result[row, col] = cells[col];
            }
            for (var col = cells.Length; col < maxCols; col++)
            {
                result[row, col] = string.Empty;
            }
        }

        return result;
    }
}

