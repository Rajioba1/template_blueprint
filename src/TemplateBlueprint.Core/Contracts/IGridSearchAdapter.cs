namespace TemplateBlueprint.Core.Contracts;

/// <summary>
/// Event arguments for search match events.
/// </summary>
public class SearchMatchEventArgs : EventArgs
{
    public int RowIndex { get; }
    public int ColumnIndex { get; }
    public string MatchedText { get; }

    public SearchMatchEventArgs(int rowIndex, int columnIndex, string matchedText)
    {
        RowIndex = rowIndex;
        ColumnIndex = columnIndex;
        MatchedText = matchedText;
    }
}

/// <summary>
/// Grid-agnostic find and replace functionality.
/// Implement this interface to add search capabilities to any TreeDataGrid.
/// </summary>
public interface IGridSearchAdapter
{
    /// <summary>
    /// Finds all occurrences of the search text.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <param name="caseSensitive">Whether to match case.</param>
    /// <param name="wholeWord">Whether to match whole words only.</param>
    /// <returns>The number of matches found.</returns>
    int FindAll(string searchText, bool caseSensitive, bool wholeWord);

    /// <summary>
    /// Moves to the next match.
    /// </summary>
    /// <returns>True if there is a next match.</returns>
    bool FindNext();

    /// <summary>
    /// Moves to the previous match.
    /// </summary>
    /// <returns>True if there is a previous match.</returns>
    bool FindPrevious();

    /// <summary>
    /// Replaces all occurrences of search text with replacement text.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <param name="replaceText">The replacement text.</param>
    /// <param name="caseSensitive">Whether to match case.</param>
    /// <returns>The number of replacements made.</returns>
    int ReplaceAll(string searchText, string replaceText, bool caseSensitive);

    /// <summary>
    /// Clears the current search.
    /// </summary>
    void ClearSearch();

    /// <summary>
    /// Raised when a match is found and selected.
    /// </summary>
    event EventHandler<SearchMatchEventArgs>? MatchFound;

    /// <summary>
    /// Gets the index of the current match (0-based).
    /// </summary>
    int CurrentMatchIndex { get; }

    /// <summary>
    /// Gets the total number of matches.
    /// </summary>
    int TotalMatches { get; }
}

