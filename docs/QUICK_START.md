# Quick Start Guide

Get up and running with AvaloniaAppKit in minutes.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- IDE: Visual Studio 2022, JetBrains Rider, or VS Code with C# Dev Kit

## Step 1: Clone the Template

```bash
git clone https://github.com/yourusername/AvaloniaAppKit.git MyApp
cd MyApp
```

## Step 2: Rename the Solution

1. Rename `AvaloniaAppKit.sln` to `MyApp.sln`
2. Rename project folders under `src/`:
   - `AvaloniaAppKit.Core` → `MyApp.Core`
   - `AvaloniaAppKit.Controls` → `MyApp.Controls`
   - `AvaloniaAppKit.AppShell` → `MyApp.AppShell`
   - `AvaloniaAppKit.Demo` → `MyApp`
3. Update namespace references (find and replace `AvaloniaAppKit` → `MyApp`)

## Step 3: Build and Run

```bash
dotnet build
dotnet run --project src/MyApp
```

## Step 4: Customize Your App

### Change App Name

Edit `src/MyApp/ViewModels/MainWindowViewModel.cs`:

```csharp
[ObservableProperty]
private string _title = "My Application";
```

### Add Navigation Items

```csharp
private void InitializeNavigation()
{
    NavigatorItems.Add(new NavigatorItemViewModel("Home", "home"));
    NavigatorItems.Add(new NavigatorItemViewModel("Analysis", "chart"));
    NavigatorItems.Add(new NavigatorItemViewModel("Reports", "document"));
}
```

### Add App Icon

1. Place your icon at `src/MyApp/Assets/app-icon.ico`
2. Uncomment the line in `MyApp.csproj`:
   ```xml
   <ApplicationIcon>Assets\app-icon.ico</ApplicationIcon>
   ```

## Step 5: Enable Excel Import (Optional)

1. Uncomment in `MyApp.csproj`:
   ```xml
   <ProjectReference Include="..\MyApp.Import.Excel\MyApp.Import.Excel.csproj" />
   ```

2. Add detection in `App.axaml.cs`:
   ```csharp
   FeatureFlags.DetectFeatures();
   ```

## Step 6: Add Your First Workspace

Create a new view model:

```csharp
public partial class DataWorkspaceViewModel : WorkspaceViewModel
{
    public DataWorkspaceViewModel() : base("Data View", "grid") { }
}
```

Add it from navigation:

```csharp
partial void OnSelectedNavigatorItemChanged(NavigatorItemViewModel? value)
{
    if (value?.Id == "analysis")
    {
        WorkspaceManager.AddWorkspace(new DataWorkspaceViewModel());
    }
}
```

## Common Tasks

### Show a Dialog

```csharp
await _dialogService.ShowMessageAsync("Hello!", "Message");

var result = await _dialogService.ShowAsync(
    "Save changes?",
    "Confirm",
    MessageBoxButtons.YesNoCancel);
```

### Open a File

```csharp
var path = await _dialogService.ShowOpenFileAsync("Open", new[]
{
    new FileFilter("CSV Files", new[] { ".csv" }),
    new FileFilter("All Files", new[] { ".*" })
});

if (path != null)
{
    var result = await _importService.ImportAsync(path);
}
```

### Track Dirty State

```csharp
_dirtyTracker.MarkDirty("document1");

if (_dirtyTracker.IsDirty)
{
    // Prompt for save
}

_dirtyTracker.MarkClean("document1");
```

### Log Messages

```csharp
_logger.LogInformation("Operation completed");
_logger.LogWarning("Unexpected value: {Value}", value);
_logger.LogError(ex, "Operation failed");
```

## Next Steps

- Read [ARCHITECTURE.md](ARCHITECTURE.md) for detailed architecture documentation
- Check out the `samples/` folder for more examples
- See the main [README.md](../README.md) for API reference
