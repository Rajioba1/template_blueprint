using System.Text.RegularExpressions;
using AvaloniaTemplateBlueprint.Core.Contracts;

namespace AvaloniaTemplateBlueprint.Controls.Extensions;

/// <summary>
/// Represents a search match location in the grid.
/// </summary>
public record SearchMatch(int RowIndex, int ColumnIndex, string Value);

/// <summary>
/// Generic grid search adapter that works with any IEnumerable data source.
/// Implement this interface for your specific data model.
/// </summary>
/// <typeparam name="TRow">The row type in the data source.</typeparam>
public class GridSearchAdapter<TRow> : IGridSearchAdapter where TRow : class
{
    private readonly Func<IEnumerable<TRow>> _getRows;
    private readonly Func<TRow, IEnumerable<(int ColumnIndex, string Value)>> _getCellValues;
    private readonly Action<TRow, int, string>? _setCellValue;

    private List<SearchMatch> _matches = new();
    private int _currentIndex = -1;
    private string _lastSearchText = string.Empty;
    private bool _lastCaseSensitive;
    private bool _lastWholeWord;

    /// <inheritdoc />
    public int CurrentMatchIndex => _currentIndex;

    /// <inheritdoc />
    public int TotalMatches => _matches.Count;

    /// <inheritdoc />
    public event EventHandler<SearchMatchEventArgs>? MatchFound;

    /// <summary>
    /// Creates a new grid search adapter.
    /// </summary>
    /// <param name="getRows">Function to get all rows from the data source.</param>
    /// <param name="getCellValues">Function to get all cell values for a row (column index and string value).</param>
    /// <param name="setCellValue">Optional function to set a cell value (for replace operations).</param>
    public GridSearchAdapter(
        Func<IEnumerable<TRow>> getRows,
        Func<TRow, IEnumerable<(int ColumnIndex, string Value)>> getCellValues,
        Action<TRow, int, string>? setCellValue = null)
    {
        _getRows = getRows ?? throw new ArgumentNullException(nameof(getRows));
        _getCellValues = getCellValues ?? throw new ArgumentNullException(nameof(getCellValues));
        _setCellValue = setCellValue;
    }

    /// <inheritdoc />
    public int FindAll(string searchText, bool caseSensitive, bool wholeWord)
    {
        _matches.Clear();
        _currentIndex = -1;
        _lastSearchText = searchText;
        _lastCaseSensitive = caseSensitive;
        _lastWholeWord = wholeWord;

        if (string.IsNullOrEmpty(searchText))
            return 0;

        var comparison = caseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        var rowIndex = 0;
        foreach (var row in _getRows())
        {
            foreach (var (columnIndex, value) in _getCellValues(row))
            {
                if (string.IsNullOrEmpty(value))
                    continue;

                var isMatch = wholeWord
                    ? IsWholeWordMatch(value, searchText, caseSensitive)
                    : value.Contains(searchText, comparison);

                if (isMatch)
                {
                    _matches.Add(new SearchMatch(rowIndex, columnIndex, value));
                }
            }
            rowIndex++;
        }

        if (_matches.Count > 0)
        {
            _currentIndex = 0;
            RaiseMatchFound();
        }

        return _matches.Count;
    }

    /// <inheritdoc />
    public bool FindNext()
    {
        if (_matches.Count == 0)
            return false;

        _currentIndex = (_currentIndex + 1) % _matches.Count;
        RaiseMatchFound();
        return true;
    }

    /// <inheritdoc />
    public bool FindPrevious()
    {
        if (_matches.Count == 0)
            return false;

        _currentIndex = _currentIndex <= 0 ? _matches.Count - 1 : _currentIndex - 1;
        RaiseMatchFound();
        return true;
    }

    /// <inheritdoc />
    public int ReplaceAll(string searchText, string replaceText, bool caseSensitive)
    {
        if (_setCellValue == null)
            throw new NotSupportedException("Replace is not supported. Provide a setCellValue function in constructor.");

        if (string.IsNullOrEmpty(searchText))
            return 0;

        var comparison = caseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        var replacedCount = 0;
        var rowIndex = 0;

        foreach (var row in _getRows())
        {
            foreach (var (columnIndex, value) in _getCellValues(row))
            {
                if (string.IsNullOrEmpty(value))
                    continue;

                if (value.Contains(searchText, comparison))
                {
                    var newValue = caseSensitive
                        ? value.Replace(searchText, replaceText)
                        : ReplaceIgnoreCase(value, searchText, replaceText);

                    _setCellValue(row, columnIndex, newValue);
                    replacedCount++;
                }
            }
            rowIndex++;
        }

        // Refresh search results
        if (replacedCount > 0)
        {
            FindAll(_lastSearchText, _lastCaseSensitive, _lastWholeWord);
        }

        return replacedCount;
    }

    /// <inheritdoc />
    public void ClearSearch()
    {
        _matches.Clear();
        _currentIndex = -1;
        _lastSearchText = string.Empty;
    }

    /// <summary>
    /// Gets the current match, if any.
    /// </summary>
    public SearchMatch? CurrentMatch => _currentIndex >= 0 && _currentIndex < _matches.Count
        ? _matches[_currentIndex]
        : null;

    private void RaiseMatchFound()
    {
        if (_currentIndex >= 0 && _currentIndex < _matches.Count)
        {
            var match = _matches[_currentIndex];
            MatchFound?.Invoke(this, new SearchMatchEventArgs(
                match.RowIndex,
                match.ColumnIndex,
                match.Value));
        }
    }

    private static bool IsWholeWordMatch(string text, string word, bool caseSensitive)
    {
        var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
        var pattern = $@"\b{Regex.Escape(word)}\b";
        return Regex.IsMatch(text, pattern, options);
    }

    private static string ReplaceIgnoreCase(string input, string search, string replacement)
    {
        return Regex.Replace(input, Regex.Escape(search), replacement, RegexOptions.IgnoreCase);
    }
}
