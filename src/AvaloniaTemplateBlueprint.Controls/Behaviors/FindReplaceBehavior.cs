using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaTemplateBlueprint.Core.Contracts;

namespace AvaloniaTemplateBlueprint.Controls.Behaviors;

/// <summary>
/// Attached behavior that enables Ctrl+F to trigger find/replace UI.
/// </summary>
public static class FindReplaceBehavior
{
    /// <summary>
    /// Attached property to enable find/replace behavior.
    /// </summary>
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "IsEnabled",
            typeof(FindReplaceBehavior),
            defaultValue: false);

    /// <summary>
    /// Attached property for the search adapter.
    /// </summary>
    public static readonly AttachedProperty<IGridSearchAdapter?> SearchAdapterProperty =
        AvaloniaProperty.RegisterAttached<Control, IGridSearchAdapter?>(
            "SearchAdapter",
            typeof(FindReplaceBehavior));

    /// <summary>
    /// Attached property for the command to execute when Ctrl+F is pressed.
    /// </summary>
    public static readonly AttachedProperty<Action?> ShowFindDialogCommandProperty =
        AvaloniaProperty.RegisterAttached<Control, Action?>(
            "ShowFindDialogCommand",
            typeof(FindReplaceBehavior));

    /// <summary>
    /// Gets whether find/replace behavior is enabled.
    /// </summary>
    public static bool GetIsEnabled(Control element) =>
        element.GetValue(IsEnabledProperty);

    /// <summary>
    /// Sets whether find/replace behavior is enabled.
    /// </summary>
    public static void SetIsEnabled(Control element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    /// <summary>
    /// Gets the search adapter.
    /// </summary>
    public static IGridSearchAdapter? GetSearchAdapter(Control element) =>
        element.GetValue(SearchAdapterProperty);

    /// <summary>
    /// Sets the search adapter.
    /// </summary>
    public static void SetSearchAdapter(Control element, IGridSearchAdapter? value) =>
        element.SetValue(SearchAdapterProperty, value);

    /// <summary>
    /// Gets the show find dialog command.
    /// </summary>
    public static Action? GetShowFindDialogCommand(Control element) =>
        element.GetValue(ShowFindDialogCommandProperty);

    /// <summary>
    /// Sets the show find dialog command.
    /// </summary>
    public static void SetShowFindDialogCommand(Control element, Action? value) =>
        element.SetValue(ShowFindDialogCommandProperty, value);

    static FindReplaceBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<Control>(OnIsEnabledChanged);
    }

    private static void OnIsEnabledChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            control.KeyDown += OnKeyDown;
        }
        else
        {
            control.KeyDown -= OnKeyDown;
        }
    }

    private static void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not Control control)
            return;

        var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var adapter = GetSearchAdapter(control);

        switch (e.Key)
        {
            case Key.F when ctrl:
                // Ctrl+F: Show find dialog
                var showCommand = GetShowFindDialogCommand(control);
                showCommand?.Invoke();
                e.Handled = true;
                break;

            case Key.F3 when !shift:
                // F3: Find next
                adapter?.FindNext();
                e.Handled = true;
                break;

            case Key.F3 when shift:
                // Shift+F3: Find previous
                adapter?.FindPrevious();
                e.Handled = true;
                break;

            case Key.Escape:
                // Escape: Clear search
                adapter?.ClearSearch();
                e.Handled = true;
                break;
        }
    }
}

/// <summary>
/// View model for find/replace operations.
/// </summary>
public class FindReplaceViewModel : INotifyPropertyChanged
{
    private string _searchText = string.Empty;
    private string _replaceText = string.Empty;
    private bool _caseSensitive;
    private bool _wholeWord;
    private int _currentMatch;
    private int _totalMatches;
    private IGridSearchAdapter? _adapter;

    /// <summary>
    /// Gets or sets the search text.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }
    }

    /// <summary>
    /// Gets or sets the replace text.
    /// </summary>
    public string ReplaceText
    {
        get => _replaceText;
        set
        {
            if (_replaceText != value)
            {
                _replaceText = value;
                OnPropertyChanged(nameof(ReplaceText));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether search is case sensitive.
    /// </summary>
    public bool CaseSensitive
    {
        get => _caseSensitive;
        set
        {
            if (_caseSensitive != value)
            {
                _caseSensitive = value;
                OnPropertyChanged(nameof(CaseSensitive));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to match whole words only.
    /// </summary>
    public bool WholeWord
    {
        get => _wholeWord;
        set
        {
            if (_wholeWord != value)
            {
                _wholeWord = value;
                OnPropertyChanged(nameof(WholeWord));
            }
        }
    }

    /// <summary>
    /// Gets the current match index (1-based for display).
    /// </summary>
    public int CurrentMatch
    {
        get => _currentMatch;
        private set
        {
            if (_currentMatch != value)
            {
                _currentMatch = value;
                OnPropertyChanged(nameof(CurrentMatch));
                OnPropertyChanged(nameof(MatchStatus));
            }
        }
    }

    /// <summary>
    /// Gets the total number of matches.
    /// </summary>
    public int TotalMatches
    {
        get => _totalMatches;
        private set
        {
            if (_totalMatches != value)
            {
                _totalMatches = value;
                OnPropertyChanged(nameof(TotalMatches));
                OnPropertyChanged(nameof(MatchStatus));
            }
        }
    }

    /// <summary>
    /// Gets the match status string (e.g., "2 of 10").
    /// </summary>
    public string MatchStatus =>
        TotalMatches == 0 ? "No matches" : $"{CurrentMatch} of {TotalMatches}";

    /// <summary>
    /// Sets the search adapter to use.
    /// </summary>
    public void SetAdapter(IGridSearchAdapter adapter)
    {
        if (_adapter != null)
        {
            _adapter.MatchFound -= OnMatchFound;
        }

        _adapter = adapter;

        if (_adapter != null)
        {
            _adapter.MatchFound += OnMatchFound;
        }
    }

    /// <summary>
    /// Executes the find operation.
    /// </summary>
    public void Find()
    {
        if (_adapter == null || string.IsNullOrEmpty(SearchText))
            return;

        TotalMatches = _adapter.FindAll(SearchText, CaseSensitive, WholeWord);
        CurrentMatch = TotalMatches > 0 ? 1 : 0;
    }

    /// <summary>
    /// Finds the next match.
    /// </summary>
    public void FindNext()
    {
        if (_adapter?.FindNext() == true)
        {
            CurrentMatch = _adapter.CurrentMatchIndex + 1;
        }
    }

    /// <summary>
    /// Finds the previous match.
    /// </summary>
    public void FindPrevious()
    {
        if (_adapter?.FindPrevious() == true)
        {
            CurrentMatch = _adapter.CurrentMatchIndex + 1;
        }
    }

    /// <summary>
    /// Replaces all matches.
    /// </summary>
    public void ReplaceAll()
    {
        if (_adapter == null || string.IsNullOrEmpty(SearchText))
            return;

        var count = _adapter.ReplaceAll(SearchText, ReplaceText, CaseSensitive);
        TotalMatches = _adapter.TotalMatches;
        CurrentMatch = TotalMatches > 0 ? _adapter.CurrentMatchIndex + 1 : 0;
    }

    /// <summary>
    /// Clears the search.
    /// </summary>
    public void Clear()
    {
        _adapter?.ClearSearch();
        TotalMatches = 0;
        CurrentMatch = 0;
    }

    private void OnMatchFound(object? sender, EventArgs e)
    {
        if (_adapter != null)
        {
            CurrentMatch = _adapter.CurrentMatchIndex + 1;
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
