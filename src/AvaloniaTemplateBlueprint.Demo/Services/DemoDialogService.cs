using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AvaloniaTemplateBlueprint.Core.Contracts;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace AvaloniaTemplateBlueprint.Demo.Services;

/// <summary>
/// Demo implementation of dialog service using MessageBox.Avalonia.
/// </summary>
public class DemoDialogService : IDialogService
{
    private Window? _mainWindow;

    /// <summary>
    /// Sets the main window for dialog parenting.
    /// </summary>
    public void SetMainWindow(Window window) => _mainWindow = window;

    public async Task ShowMessageAsync(string message, string title)
    {
        var box = MessageBoxManager
            .GetMessageBoxStandard(title, message, ButtonEnum.Ok);

        if (_mainWindow != null)
        {
            await box.ShowWindowDialogAsync(_mainWindow);
        }
        else
        {
            await box.ShowAsync();
        }
    }

    public async Task<MessageBoxResult> ShowAsync(string message, string title, MessageBoxButtons buttons)
    {
        var buttonEnum = buttons switch
        {
            MessageBoxButtons.OkCancel => ButtonEnum.OkCancel,
            MessageBoxButtons.YesNo => ButtonEnum.YesNo,
            MessageBoxButtons.YesNoCancel => ButtonEnum.YesNoCancel,
            _ => ButtonEnum.Ok
        };

        var box = MessageBoxManager
            .GetMessageBoxStandard(title, message, buttonEnum);

        var result = _mainWindow != null
            ? await box.ShowWindowDialogAsync(_mainWindow)
            : await box.ShowAsync();

        return result switch
        {
            MsBox.Avalonia.Enums.ButtonResult.Ok => MessageBoxResult.Ok,
            MsBox.Avalonia.Enums.ButtonResult.Cancel => MessageBoxResult.Cancel,
            MsBox.Avalonia.Enums.ButtonResult.Yes => MessageBoxResult.Yes,
            MsBox.Avalonia.Enums.ButtonResult.No => MessageBoxResult.No,
            _ => MessageBoxResult.None
        };
    }

    public async Task<string?> ShowOpenFileAsync(string title, IEnumerable<FileFilter> filters)
    {
        if (_mainWindow == null)
            return null;

        var storageProvider = _mainWindow.StorageProvider;
        var fileTypes = filters.Select(f => new FilePickerFileType(f.Name)
        {
            Patterns = f.Extensions.Select(e => e.StartsWith(".") ? $"*{e}" : $"*.{e}").ToArray()
        }).ToArray();

        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            FileTypeFilter = fileTypes,
            AllowMultiple = false
        });

        return result.FirstOrDefault()?.Path.LocalPath;
    }

    public async Task<string?> ShowSaveFileAsync(string title, IEnumerable<FileFilter> filters)
    {
        if (_mainWindow == null)
            return null;

        var storageProvider = _mainWindow.StorageProvider;
        var fileTypes = filters.Select(f => new FilePickerFileType(f.Name)
        {
            Patterns = f.Extensions.Select(e => e.StartsWith(".") ? $"*{e}" : $"*.{e}").ToArray()
        }).ToArray();

        var result = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            FileTypeChoices = fileTypes
        });

        return result?.Path.LocalPath;
    }

    public async Task<bool> ShowConfirmCloseAsync(string documentName)
    {
        var result = await ShowAsync(
            $"Do you want to save changes to '{documentName}' before closing?",
            "Unsaved Changes",
            MessageBoxButtons.YesNoCancel);

        // Yes = save and close (true), No = close without saving (true), Cancel = don't close (false)
        return result != MessageBoxResult.Cancel;
    }
}
