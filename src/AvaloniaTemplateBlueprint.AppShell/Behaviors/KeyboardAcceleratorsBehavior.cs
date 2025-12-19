using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Windows.Input;

namespace AvaloniaTemplateBlueprint.AppShell.Behaviors;

/// <summary>
/// Standard keyboard accelerator actions.
/// </summary>
public class KeyboardAcceleratorActions
{
    /// <summary>
    /// Gets or sets the Save command (Ctrl+S).
    /// </summary>
    public ICommand? SaveCommand { get; set; }

    /// <summary>
    /// Gets or sets the Save As command (Ctrl+Shift+S).
    /// </summary>
    public ICommand? SaveAsCommand { get; set; }

    /// <summary>
    /// Gets or sets the Open command (Ctrl+O).
    /// </summary>
    public ICommand? OpenCommand { get; set; }

    /// <summary>
    /// Gets or sets the New command (Ctrl+N).
    /// </summary>
    public ICommand? NewCommand { get; set; }

    /// <summary>
    /// Gets or sets the Find command (Ctrl+F).
    /// </summary>
    public ICommand? FindCommand { get; set; }

    /// <summary>
    /// Gets or sets the Toggle Debug Console command (F12).
    /// </summary>
    public ICommand? ToggleConsoleCommand { get; set; }

    /// <summary>
    /// Gets or sets the Undo command (Ctrl+Z).
    /// </summary>
    public ICommand? UndoCommand { get; set; }

    /// <summary>
    /// Gets or sets the Redo command (Ctrl+Y or Ctrl+Shift+Z).
    /// </summary>
    public ICommand? RedoCommand { get; set; }

    /// <summary>
    /// Gets or sets the Close Tab command (Ctrl+W).
    /// </summary>
    public ICommand? CloseTabCommand { get; set; }

    /// <summary>
    /// Gets or sets the Preferences/Settings command (Ctrl+,).
    /// </summary>
    public ICommand? PreferencesCommand { get; set; }
}

/// <summary>
/// Attached behavior that handles standard keyboard accelerators.
/// </summary>
public static class KeyboardAcceleratorsBehavior
{
    /// <summary>
    /// Attached property to enable keyboard accelerators.
    /// </summary>
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "IsEnabled",
            typeof(KeyboardAcceleratorsBehavior),
            defaultValue: false);

    /// <summary>
    /// Attached property for the accelerator actions.
    /// </summary>
    public static readonly AttachedProperty<KeyboardAcceleratorActions?> ActionsProperty =
        AvaloniaProperty.RegisterAttached<Control, KeyboardAcceleratorActions?>(
            "Actions",
            typeof(KeyboardAcceleratorsBehavior));

    /// <summary>
    /// Gets whether keyboard accelerators are enabled.
    /// </summary>
    public static bool GetIsEnabled(Control control) =>
        control.GetValue(IsEnabledProperty);

    /// <summary>
    /// Sets whether keyboard accelerators are enabled.
    /// </summary>
    public static void SetIsEnabled(Control control, bool value) =>
        control.SetValue(IsEnabledProperty, value);

    /// <summary>
    /// Gets the accelerator actions.
    /// </summary>
    public static KeyboardAcceleratorActions? GetActions(Control control) =>
        control.GetValue(ActionsProperty);

    /// <summary>
    /// Sets the accelerator actions.
    /// </summary>
    public static void SetActions(Control control, KeyboardAcceleratorActions? value) =>
        control.SetValue(ActionsProperty, value);

    static KeyboardAcceleratorsBehavior()
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

        var actions = GetActions(control);
        if (actions == null)
            return;

        var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var noModifiers = e.KeyModifiers == KeyModifiers.None;

        ICommand? command = null;

        switch (e.Key)
        {
            case Key.S when ctrl && shift:
                command = actions.SaveAsCommand;
                break;

            case Key.S when ctrl:
                command = actions.SaveCommand;
                break;

            case Key.O when ctrl:
                command = actions.OpenCommand;
                break;

            case Key.N when ctrl:
                command = actions.NewCommand;
                break;

            case Key.F when ctrl:
                command = actions.FindCommand;
                break;

            case Key.F12 when noModifiers:
                command = actions.ToggleConsoleCommand;
                break;

            case Key.Z when ctrl && shift:
            case Key.Y when ctrl:
                command = actions.RedoCommand;
                break;

            case Key.Z when ctrl:
                command = actions.UndoCommand;
                break;

            case Key.W when ctrl:
                command = actions.CloseTabCommand;
                break;

            case Key.OemComma when ctrl:
                command = actions.PreferencesCommand;
                break;
        }

        if (command?.CanExecute(null) == true)
        {
            command.Execute(null);
            e.Handled = true;
        }
    }
}
