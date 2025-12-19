# AvaloniaTemplateBlueprint Architecture

## Overview

AvaloniaTemplateBlueprint follows a layered architecture with clear separation of concerns:

```
+-------------------------------------------------------------+
|                    Your Application                         |
|               (AvaloniaTemplateBlueprint.Demo example)                 |
+-------------------------------------------------------------+
|                  AvaloniaTemplateBlueprint.AppShell                    |
|    Navigation | Debug Console | Behaviors | Workspaces     |
+-------------------------------------------------------------+
|                 AvaloniaTemplateBlueprint.Controls                     |
|       TreeDataGrid Extensions | Search | Clipboard         |
+-------------------------------------------------------------+
|                   AvaloniaTemplateBlueprint.Core                       |
|              Interfaces | Events | Models                   |
+-------------------------------------------------------------+
|    Avalonia UI    |  TreeDataGrid  |  CommunityToolkit.Mvvm |
+-------------------------------------------------------------+
```

## Core Package (AvaloniaTemplateBlueprint.Core)

The Core package defines interfaces that act as "template contracts". Applications implement these interfaces to customize behavior while maintaining compatibility with AppShell components.

### Key Interfaces

#### IWorkspace / IWorkspaceHost

Defines the workspace pattern for tabbed document management:

```csharp
public interface IWorkspace
{
    string Id { get; }
    string Title { get; }
    bool IsDirty { get; }
    Task<bool> CanCloseAsync();
    Task SaveAsync();
}

public interface IWorkspaceHost
{
    IReadOnlyList<IWorkspace> Workspaces { get; }
    IWorkspace? ActiveWorkspace { get; set; }
    void AddWorkspace(IWorkspace workspace);
    bool RemoveWorkspace(IWorkspace workspace);
    event EventHandler<WorkspaceChangedEventArgs>? WorkspaceChanged;
}
```

#### IDialogService

Platform-agnostic dialog abstraction:

```csharp
public interface IDialogService
{
    Task ShowMessageAsync(string message, string title);
    Task<MessageBoxResult> ShowAsync(string message, string title, MessageBoxButtons buttons);
    Task<string?> ShowOpenFileAsync(string title, IEnumerable<FileFilter> filters);
    Task<string?> ShowSaveFileAsync(string title, IEnumerable<FileFilter> filters);
    Task<bool> ShowConfirmCloseAsync(string documentName);
}
```

#### IAppSettingsService

Settings persistence with first-run detection:

```csharp
public interface IAppSettingsService
{
    bool IsFirstRun { get; }
    void MarkFirstRunComplete();
    T? Get<T>(string key);
    void Set<T>(string key, T value);
    void Save();
    void Load();
}
```

## Controls Package (AvaloniaTemplateBlueprint.Controls)

Provides TreeDataGrid enhancements through behaviors and extension classes.

### GridSearchAdapter<T>

Generic search adapter that implements `IGridSearchAdapter`:

```csharp
public class GridSearchAdapter<T> : IGridSearchAdapter where T : class
{
    public GridSearchAdapter(
        TreeDataGrid grid,
        Func<T, string, bool> matchPredicate,
        Func<IEnumerable<T>> getItems);

    public void Search(string searchText);
    public void NextMatch();
    public void PreviousMatch();
}
```

### Behaviors

- **CellSelectionBehavior**: Adds range selection and shift-click support
- **ClipboardBehavior**: Copy/paste with tab-separated format for Excel compatibility
- **FindReplaceBehavior**: Ctrl+F popup for inline search

## AppShell Package (AvaloniaTemplateBlueprint.AppShell)

Provides the application shell components.

### Navigation

The `NavigatorItemViewModel` supports hierarchical navigation:

```csharp
// Flat item
new NavigatorItemViewModel("Dashboard", "dashboard")

// Group with children
NavigatorItemViewModel.CreateGroup("Settings", "settings",
    new NavigatorItemViewModel("General", "settings"),
    new NavigatorItemViewModel("Appearance", "palette"))
```

### Workspace Management

`WorkspaceManager` implements `IWorkspaceHost`:

```csharp
public class WorkspaceManager : IWorkspaceHost
{
    public void AddWorkspace(IWorkspace workspace);
    public bool RemoveWorkspace(IWorkspace workspace);
    public async Task CloseAllAsync();
}
```

### Debug Console

The debug console captures logs via `ConsoleWindowLoggerProvider`:

```
+---------------------------------------------+
| Debug Console                          _ [] X|
+---------------------------------------------+
| [DBG] 10:23:45 App started                  |
| [INF] 10:23:46 Loading settings...          |
| [WRN] 10:23:47 Config file not found        |
| [ERR] 10:23:48 Failed to connect            |
+---------------------------------------------+
| [x] Debug  [x] Info  [x] Warning  [x] Error |
+---------------------------------------------+
```

Features:
- Real-time log capture
- Log level filtering
- Automatic password/token redaction
- Copy to clipboard
- Clear button

### Window Close Guard

`WindowCloseGuardBehavior` prompts for unsaved changes:

```xml
<Window>
    <i:Interaction.Behaviors>
        <behaviors:WindowCloseGuardBehavior DirtyTracker="{Binding DirtyTracker}" />
    </i:Interaction.Behaviors>
</Window>
```

## Import.Excel Package (Optional)

Provides Excel import using ClosedXML:

```csharp
public class ExcelImportService : IDataImportService
{
    public IEnumerable<string> SupportedExtensions =>
        new[] { ".xlsx", ".xls", ".xlsm" };

    public async Task<ImportResult> ImportAsync(string filePath);
}
```

Features:
- Automatic worksheet detection
- Smart type inference (80% consistency threshold)
- Header row detection
- Boolean parsing ("yes"/"no", "true"/"false", 1/0)

## Dependency Injection

The template uses Microsoft.Extensions.DependencyInjection:

```csharp
services.AddSingleton<IAppSettingsService, MyAppSettingsService>();
services.AddSingleton<IDialogService, MyDialogService>();
services.AddSingleton<IProjectDirtyTracker, ProjectDirtyTracker>();
services.AddTransient<MainWindowViewModel>();
```

## Event Flow

### Workspace Lifecycle

```
AddWorkspace() -> WorkspaceChanged(Added) -> PropertyChanged(ActiveWorkspace)
                                                     |
                                                     v
                                              UI Updates Tab
                                                     |
                                                     v
User closes tab -> CanCloseAsync() ---> true ---> RemoveWorkspace()
                          |
                          v
                   false -> Cancel close
```

### Search Flow

```
User types in search box
        |
        v
GridSearchAdapter.Search(text)
        |
        v
Filter items using matchPredicate
        |
        v
Update matches collection
        |
        v
Raise SearchMatchChanged event
        |
        v
Highlight matching cells
```

## Best Practices

1. **Interface Segregation**: Implement only the interfaces you need
2. **Dependency Injection**: Register services in App.axaml.cs
3. **Async Operations**: Use async for I/O operations (dialogs, file operations)
4. **Log Redaction**: Sensitive data is automatically redacted in debug console
5. **Feature Flags**: Check for optional features at runtime (Excel import)

## Extending the Template

### Adding a New Workspace Type

```csharp
public partial class DataWorkspaceViewModel : WorkspaceViewModel
{
    public DataWorkspaceViewModel() : base("Data", "grid")
    {
    }

    public override async Task<bool> CanCloseAsync()
    {
        if (!IsDirty) return true;
        // Show save prompt
        return await _dialogService.ShowConfirmCloseAsync(Title);
    }

    public override async Task SaveAsync()
    {
        // Save logic
        IsDirty = false;
    }
}
```

### Adding Custom Grid Search

```csharp
var adapter = new GridSearchAdapter<MyRow>(
    _grid,
    (row, text) => row.Name.Contains(text, StringComparison.OrdinalIgnoreCase),
    () => _dataSource.Items);

adapter.SearchMatchChanged += (s, e) =>
{
    StatusText = $"Match {e.CurrentIndex + 1} of {e.TotalMatches}";
};
```
