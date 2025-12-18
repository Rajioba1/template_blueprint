# Sample Projects

This folder contains sample projects demonstrating AvaloniaAppKit features.

## Available Samples

### Demo Application (`../src/AvaloniaAppKit.Demo`)

The main demo application showcases all core features:

- Navigation sidebar with hierarchical items
- Debug console with log capture
- File import with type detection
- First-run detection and settings persistence
- Standard keyboard shortcuts

To run:
```bash
dotnet run --project ../src/AvaloniaAppKit.Demo
```

## Planned Samples

The following samples are planned for future releases:

### BasicApp
Minimal application showing core setup:
- Single window with navigation
- Simple settings management
- No data grid features

### DataGridApp
Focused on TreeDataGrid features:
- Large dataset handling
- Search and filtering
- Cell selection and clipboard
- Excel-style copy/paste

### WorkspaceApp
Document-based application:
- Multiple workspace types
- Tabbed interface
- Save/load workflows
- Unsaved changes prompts

### ImportExportApp
Data import/export focus:
- CSV import with preview
- Excel import (optional package)
- Export to various formats
- Column mapping workflow

## Creating Your Own Sample

1. Create a new folder: `samples/MySample/`
2. Create project file:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <OutputType>WinExe</OutputType>
     </PropertyGroup>
     <ItemGroup>
       <ProjectReference Include="../../src/AvaloniaAppKit.Core/AvaloniaAppKit.Core.csproj" />
       <ProjectReference Include="../../src/AvaloniaAppKit.AppShell/AvaloniaAppKit.AppShell.csproj" />
     </ItemGroup>
   </Project>
   ```

3. Add to solution:
   ```bash
   dotnet sln add samples/MySample/MySample.csproj
   ```

## Contributing Samples

We welcome sample contributions! Please ensure:

- Clear, commented code
- README explaining the sample's purpose
- Minimal dependencies
- Works with latest template version
