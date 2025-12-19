using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace TemplateBlueprint.AppShell.Behaviors;

/// <summary>
/// Attached behavior that guards window close with a confirmation prompt.
/// </summary>
public static class WindowCloseGuardBehavior
{
    /// <summary>
    /// Attached property to enable close guard.
    /// </summary>
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Window, bool>(
            "IsEnabled",
            typeof(WindowCloseGuardBehavior),
            defaultValue: false);

    /// <summary>
    /// Attached property for the close check function.
    /// Returns true if close should proceed, false to cancel.
    /// </summary>
    public static readonly AttachedProperty<Func<Task<bool>>?> CanCloseCheckProperty =
        AvaloniaProperty.RegisterAttached<Window, Func<Task<bool>>?>(
            "CanCloseCheck",
            typeof(WindowCloseGuardBehavior));

    /// <summary>
    /// Gets whether close guard is enabled.
    /// </summary>
    public static bool GetIsEnabled(Window window) =>
        window.GetValue(IsEnabledProperty);

    /// <summary>
    /// Sets whether close guard is enabled.
    /// </summary>
    public static void SetIsEnabled(Window window, bool value) =>
        window.SetValue(IsEnabledProperty, value);

    /// <summary>
    /// Gets the close check function.
    /// </summary>
    public static Func<Task<bool>>? GetCanCloseCheck(Window window) =>
        window.GetValue(CanCloseCheckProperty);

    /// <summary>
    /// Sets the close check function.
    /// </summary>
    public static void SetCanCloseCheck(Window window, Func<Task<bool>>? value) =>
        window.SetValue(CanCloseCheckProperty, value);

    static WindowCloseGuardBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<Window>(OnIsEnabledChanged);
    }

    private static void OnIsEnabledChanged(Window window, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            window.Closing += OnWindowClosing;
        }
        else
        {
            window.Closing -= OnWindowClosing;
        }
    }

    private static async void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (sender is not Window window)
            return;

        var canCloseCheck = GetCanCloseCheck(window);
        if (canCloseCheck == null)
            return;

        // Prevent immediate close
        e.Cancel = true;

        // Check if we can close
        var canClose = await canCloseCheck();

        if (canClose)
        {
            // Temporarily disable the guard and close
            SetIsEnabled(window, false);
            window.Close();
        }
    }
}

