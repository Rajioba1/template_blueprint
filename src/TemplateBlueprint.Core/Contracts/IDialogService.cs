namespace TemplateBlueprint.Core.Contracts;

/// <summary>
/// Result of a message box dialog.
/// </summary>
public enum MessageBoxResult
{
    None,
    Ok,
    Cancel,
    Yes,
    No
}

/// <summary>
/// Buttons to show in a message box.
/// </summary>
public enum MessageBoxButtons
{
    Ok,
    OkCancel,
    YesNo,
    YesNoCancel
}

/// <summary>
/// File filter for file dialogs.
/// </summary>
public record FileFilter(string Name, IEnumerable<string> Extensions);

/// <summary>
/// Platform-agnostic dialog service.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a simple message dialog.
    /// </summary>
    Task ShowMessageAsync(string message, string title);

    /// <summary>
    /// Shows a message dialog with buttons.
    /// </summary>
    Task<MessageBoxResult> ShowAsync(string message, string title, MessageBoxButtons buttons);

    /// <summary>
    /// Shows an open file dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="filters">File type filters.</param>
    /// <returns>The selected file path, or null if cancelled.</returns>
    Task<string?> ShowOpenFileAsync(string title, IEnumerable<FileFilter> filters);

    /// <summary>
    /// Shows a save file dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="filters">File type filters.</param>
    /// <returns>The selected file path, or null if cancelled.</returns>
    Task<string?> ShowSaveFileAsync(string title, IEnumerable<FileFilter> filters);

    /// <summary>
    /// Shows a confirmation dialog for closing a document with unsaved changes.
    /// </summary>
    /// <param name="documentName">The name of the document.</param>
    /// <returns>True if the user wants to proceed with closing.</returns>
    Task<bool> ShowConfirmCloseAsync(string documentName);
}

