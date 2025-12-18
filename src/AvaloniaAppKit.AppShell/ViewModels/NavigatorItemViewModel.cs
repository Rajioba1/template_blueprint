using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaAppKit.AppShell.ViewModels;

/// <summary>
/// View model for a navigation item in the sidebar.
/// </summary>
public partial class NavigatorItemViewModel : ObservableObject
{
    /// <summary>
    /// Gets the display title.
    /// </summary>
    [ObservableProperty]
    private string _title = string.Empty;

    /// <summary>
    /// Gets or sets the icon resource key.
    /// </summary>
    [ObservableProperty]
    private string? _iconKey;

    /// <summary>
    /// Gets or sets whether this item is selected.
    /// </summary>
    [ObservableProperty]
    private bool _isSelected;

    /// <summary>
    /// Gets or sets whether this item is expanded (for groups).
    /// </summary>
    [ObservableProperty]
    private bool _isExpanded = true;

    /// <summary>
    /// Gets or sets the view type to display when this item is selected.
    /// </summary>
    public Type? ViewType { get; set; }

    /// <summary>
    /// Gets or sets the view model type associated with this item.
    /// </summary>
    public Type? ViewModelType { get; set; }

    /// <summary>
    /// Gets or sets any data context or tag for this item.
    /// </summary>
    public object? Tag { get; set; }

    /// <summary>
    /// Gets the child items (for hierarchical navigation).
    /// </summary>
    public ObservableCollection<NavigatorItemViewModel> Children { get; } = new();

    /// <summary>
    /// Gets whether this item has children.
    /// </summary>
    public bool HasChildren => Children.Count > 0;

    /// <summary>
    /// Gets whether this is a leaf item (no children).
    /// </summary>
    public bool IsLeaf => Children.Count == 0;

    /// <summary>
    /// Creates a new navigator item.
    /// </summary>
    public NavigatorItemViewModel()
    {
    }

    /// <summary>
    /// Creates a new navigator item with title and optional icon.
    /// </summary>
    /// <param name="title">The display title.</param>
    /// <param name="iconKey">Optional icon resource key.</param>
    /// <param name="viewType">Optional view type to navigate to.</param>
    public NavigatorItemViewModel(string title, string? iconKey = null, Type? viewType = null)
    {
        Title = title;
        IconKey = iconKey;
        ViewType = viewType;
    }

    /// <summary>
    /// Creates a group item with children.
    /// </summary>
    /// <param name="title">The group title.</param>
    /// <param name="iconKey">Optional icon resource key.</param>
    /// <param name="children">Child items.</param>
    public static NavigatorItemViewModel CreateGroup(
        string title,
        string? iconKey = null,
        params NavigatorItemViewModel[] children)
    {
        var group = new NavigatorItemViewModel(title, iconKey);
        foreach (var child in children)
        {
            group.Children.Add(child);
        }
        return group;
    }
}
