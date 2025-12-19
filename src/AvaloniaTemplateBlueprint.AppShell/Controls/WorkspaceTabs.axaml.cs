using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using AvaloniaTemplateBlueprint.AppShell.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaTemplateBlueprint.AppShell.Controls;

/// <summary>
/// Tab strip control for workspace management.
/// </summary>
public partial class WorkspaceTabs : UserControl
{
    /// <summary>
    /// Defines the Workspaces property.
    /// </summary>
    public static readonly StyledProperty<ObservableCollection<WorkspaceViewModel>> WorkspacesProperty =
        AvaloniaProperty.Register<WorkspaceTabs, ObservableCollection<WorkspaceViewModel>>(
            nameof(Workspaces), new ObservableCollection<WorkspaceViewModel>());

    /// <summary>
    /// Defines the ActiveWorkspace property.
    /// </summary>
    public static readonly StyledProperty<WorkspaceViewModel?> ActiveWorkspaceProperty =
        AvaloniaProperty.Register<WorkspaceTabs, WorkspaceViewModel?>(nameof(ActiveWorkspace));

    /// <summary>
    /// Defines the ShowAddButton property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowAddButtonProperty =
        AvaloniaProperty.Register<WorkspaceTabs, bool>(nameof(ShowAddButton), true);

    /// <summary>
    /// Defines the AddWorkspaceCommand property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> AddWorkspaceCommandProperty =
        AvaloniaProperty.Register<WorkspaceTabs, ICommand?>(nameof(AddWorkspaceCommand));

    /// <summary>
    /// Gets or sets the workspaces collection.
    /// </summary>
    public ObservableCollection<WorkspaceViewModel> Workspaces
    {
        get => GetValue(WorkspacesProperty);
        set => SetValue(WorkspacesProperty, value);
    }

    /// <summary>
    /// Gets or sets the active workspace.
    /// </summary>
    public WorkspaceViewModel? ActiveWorkspace
    {
        get => GetValue(ActiveWorkspaceProperty);
        set => SetValue(ActiveWorkspaceProperty, value);
    }

    /// <summary>
    /// Gets or sets whether to show the add button.
    /// </summary>
    public bool ShowAddButton
    {
        get => GetValue(ShowAddButtonProperty);
        set => SetValue(ShowAddButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to add a new workspace.
    /// </summary>
    public ICommand? AddWorkspaceCommand
    {
        get => GetValue(AddWorkspaceCommandProperty);
        set => SetValue(AddWorkspaceCommandProperty, value);
    }

    /// <summary>
    /// Command to close a tab.
    /// </summary>
    public ICommand CloseTabCommand { get; }

    /// <summary>
    /// Raised when a tab close is requested.
    /// </summary>
    public event EventHandler<WorkspaceViewModel>? CloseTabRequested;

    /// <summary>
    /// Raised when a tab is selected.
    /// </summary>
    public event EventHandler<WorkspaceViewModel?>? TabSelected;

    public WorkspaceTabs()
    {
        CloseTabCommand = new RelayCommand<WorkspaceViewModel>(OnCloseTab);
        InitializeComponent();
    }

    private void OnCloseTab(WorkspaceViewModel? workspace)
    {
        if (workspace != null)
        {
            CloseTabRequested?.Invoke(this, workspace);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ActiveWorkspaceProperty)
        {
            TabSelected?.Invoke(this, change.NewValue as WorkspaceViewModel);
        }
    }
}
