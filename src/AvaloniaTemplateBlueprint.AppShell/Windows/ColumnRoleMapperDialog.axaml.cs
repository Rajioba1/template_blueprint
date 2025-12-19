using System.Collections.ObjectModel;
using Avalonia.Controls;
using AvaloniaTemplateBlueprint.Core.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaTemplateBlueprint.AppShell.Windows;

/// <summary>
/// View model for a single role mapping.
/// </summary>
public partial class RoleMappingViewModel : ObservableObject
{
    /// <summary>
    /// Gets the role key.
    /// </summary>
    public string RoleKey { get; }

    /// <summary>
    /// Gets the role display label.
    /// </summary>
    public string RoleLabel { get; }

    /// <summary>
    /// Gets whether this mapping is required.
    /// </summary>
    public bool IsRequired { get; }

    /// <summary>
    /// Gets the available columns to choose from.
    /// </summary>
    public ObservableCollection<ColumnInfo> AvailableColumns { get; }

    /// <summary>
    /// Gets or sets the selected column.
    /// </summary>
    [ObservableProperty]
    private ColumnInfo? _selectedColumn;

    public RoleMappingViewModel(ColumnRole role, IEnumerable<ColumnInfo> columns)
    {
        RoleKey = role.Key;
        RoleLabel = role.Label;
        IsRequired = role.Required;
        AvailableColumns = new ObservableCollection<ColumnInfo>(columns);
    }
}

/// <summary>
/// Generic column-to-role mapping dialog.
/// Use this for import dialogs, analysis configuration, etc.
/// </summary>
public partial class ColumnRoleMapperDialog : Window
{
    private readonly ColumnRoleMapperViewModel _viewModel;
    private TaskCompletionSource<Dictionary<string, string>?>? _resultTcs;

    public ColumnRoleMapperDialog()
    {
        InitializeComponent();
        _viewModel = new ColumnRoleMapperViewModel(this);
        DataContext = _viewModel;
    }

    /// <summary>
    /// Shows the dialog and returns the mapping result.
    /// </summary>
    /// <param name="owner">The owner window.</param>
    /// <param name="roles">The roles to map.</param>
    /// <param name="columns">The available columns.</param>
    /// <param name="description">Optional description text.</param>
    /// <returns>The mapping (role key -> column id) or null if cancelled.</returns>
    public static async Task<Dictionary<string, string>?> ShowAsync(
        Window owner,
        IEnumerable<ColumnRole> roles,
        IEnumerable<ColumnInfo> columns,
        string? description = null)
    {
        var dialog = new ColumnRoleMapperDialog();
        dialog._viewModel.Initialize(roles, columns, description);
        dialog._resultTcs = new TaskCompletionSource<Dictionary<string, string>?>();

        await dialog.ShowDialog(owner);
        return await dialog._resultTcs.Task;
    }

    internal void Complete(Dictionary<string, string>? result)
    {
        _resultTcs?.TrySetResult(result);
        Close();
    }
}

/// <summary>
/// View model for the column role mapper dialog.
/// </summary>
public partial class ColumnRoleMapperViewModel : ObservableObject, IColumnRoleMapper
{
    private readonly ColumnRoleMapperDialog _dialog;
    private readonly List<ColumnRole> _roles = new();
    private readonly List<ColumnInfo> _columns = new();
    private readonly Dictionary<string, string> _mappings = new();

    /// <summary>
    /// Gets or sets the description text.
    /// </summary>
    [ObservableProperty]
    private string _description = "Map your data columns to the required roles.";

    /// <summary>
    /// Gets the role mappings.
    /// </summary>
    public ObservableCollection<RoleMappingViewModel> RoleMappings { get; } = new();

    /// <summary>
    /// Gets whether all required mappings are complete.
    /// </summary>
    public bool IsValid => ValidateMapping();

    /// <inheritdoc />
    public IReadOnlyList<ColumnRole> Roles => _roles;

    /// <inheritdoc />
    public IReadOnlyList<ColumnInfo> Columns => _columns;

    public ColumnRoleMapperViewModel(ColumnRoleMapperDialog dialog)
    {
        _dialog = dialog;
    }

    /// <summary>
    /// Initializes the dialog with roles and columns.
    /// </summary>
    public void Initialize(IEnumerable<ColumnRole> roles, IEnumerable<ColumnInfo> columns, string? description)
    {
        _roles.Clear();
        _roles.AddRange(roles);

        _columns.Clear();
        _columns.AddRange(columns);

        _mappings.Clear();

        if (!string.IsNullOrEmpty(description))
        {
            Description = description;
        }

        RoleMappings.Clear();
        foreach (var role in _roles)
        {
            var mapping = new RoleMappingViewModel(role, _columns);
            mapping.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(RoleMappingViewModel.SelectedColumn))
                {
                    var vm = s as RoleMappingViewModel;
                    if (vm?.SelectedColumn != null)
                    {
                        _mappings[vm.RoleKey] = vm.SelectedColumn.Id;
                    }
                    else if (vm != null)
                    {
                        _mappings.Remove(vm.RoleKey);
                    }
                    OnPropertyChanged(nameof(IsValid));
                }
            };
            RoleMappings.Add(mapping);
        }
    }

    /// <inheritdoc />
    public Dictionary<string, string> GetMapping()
    {
        return new Dictionary<string, string>(_mappings);
    }

    /// <inheritdoc />
    public void SetMapping(string roleKey, string columnId)
    {
        _mappings[roleKey] = columnId;

        // Update UI
        var roleVm = RoleMappings.FirstOrDefault(r => r.RoleKey == roleKey);
        if (roleVm != null)
        {
            roleVm.SelectedColumn = _columns.FirstOrDefault(c => c.Id == columnId);
        }

        OnPropertyChanged(nameof(IsValid));
    }

    /// <inheritdoc />
    public bool ValidateMapping()
    {
        return _roles
            .Where(r => r.Required)
            .All(r => _mappings.ContainsKey(r.Key));
    }

    /// <summary>
    /// Applies the mapping and closes the dialog.
    /// </summary>
    [RelayCommand]
    private void Apply()
    {
        if (IsValid)
        {
            _dialog.Complete(GetMapping());
        }
    }

    /// <summary>
    /// Cancels and closes the dialog.
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        _dialog.Complete(null);
    }
}
