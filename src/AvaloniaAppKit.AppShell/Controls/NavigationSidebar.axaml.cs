using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using AvaloniaAppKit.AppShell.ViewModels;

namespace AvaloniaAppKit.AppShell.Controls;

/// <summary>
/// Navigation sidebar control with hierarchical tree view.
/// </summary>
public partial class NavigationSidebar : UserControl
{
    /// <summary>
    /// Defines the Items property.
    /// </summary>
    public static readonly StyledProperty<ObservableCollection<NavigatorItemViewModel>> ItemsProperty =
        AvaloniaProperty.Register<NavigationSidebar, ObservableCollection<NavigatorItemViewModel>>(
            nameof(Items), new ObservableCollection<NavigatorItemViewModel>());

    /// <summary>
    /// Defines the SelectedItem property.
    /// </summary>
    public static readonly StyledProperty<NavigatorItemViewModel?> SelectedItemProperty =
        AvaloniaProperty.Register<NavigationSidebar, NavigatorItemViewModel?>(nameof(SelectedItem));

    /// <summary>
    /// Gets or sets the navigation items.
    /// </summary>
    public ObservableCollection<NavigatorItemViewModel> Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the selected navigation item.
    /// </summary>
    public NavigatorItemViewModel? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Raised when the selection changes.
    /// </summary>
    public event EventHandler<NavigatorItemViewModel?>? SelectionChanged;

    public NavigationSidebar()
    {
        InitializeComponent();
        DataContext = this;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedItemProperty)
        {
            SelectionChanged?.Invoke(this, change.NewValue as NavigatorItemViewModel);
        }
    }
}
