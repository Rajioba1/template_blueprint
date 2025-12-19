using AvaloniaTemplateBlueprint.Core.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaTemplateBlueprint.AppShell.ViewModels;

/// <summary>
/// Base class for workspace view models (tabs in the main content area).
/// </summary>
public abstract partial class WorkspaceViewModel : ObservableObject, IWorkspace
{
    /// <summary>
    /// Gets or sets the workspace title (displayed in tab).
    /// </summary>
    [ObservableProperty]
    private string _title = "Untitled";

    /// <summary>
    /// Gets or sets whether this workspace has unsaved changes.
    /// </summary>
    [ObservableProperty]
    private bool _isDirty;

    /// <summary>
    /// Gets the title with dirty indicator.
    /// </summary>
    public string DisplayTitle => IsDirty ? $"{Title} *" : Title;

    /// <summary>
    /// Gets the view type for this workspace.
    /// </summary>
    public abstract Type ViewType { get; }

    /// <summary>
    /// Gets a unique identifier for this workspace instance.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Called when the workspace is activated (tab selected).
    /// </summary>
    public virtual Task OnActivatedAsync() => Task.CompletedTask;

    /// <summary>
    /// Called when the workspace is deactivated (another tab selected).
    /// </summary>
    public virtual Task OnDeactivatedAsync() => Task.CompletedTask;

    /// <summary>
    /// Checks if the workspace can be closed.
    /// Override to prompt for unsaved changes.
    /// </summary>
    /// <returns>True if the workspace can be closed.</returns>
    public virtual Task<bool> CanCloseAsync()
    {
        // Default: always allow close
        // Override to show "Save changes?" dialog when IsDirty
        return Task.FromResult(true);
    }

    /// <summary>
    /// Called when the workspace is being closed.
    /// </summary>
    public virtual Task OnClosingAsync() => Task.CompletedTask;

    /// <summary>
    /// Marks the workspace as having changes.
    /// </summary>
    protected void MarkDirty()
    {
        IsDirty = true;
        OnPropertyChanged(nameof(DisplayTitle));
    }

    /// <summary>
    /// Marks the workspace as saved (no pending changes).
    /// </summary>
    protected void MarkClean()
    {
        IsDirty = false;
        OnPropertyChanged(nameof(DisplayTitle));
    }

    partial void OnIsDirtyChanged(bool value)
    {
        OnPropertyChanged(nameof(DisplayTitle));
    }
}
