namespace TemplateBlueprint.Core.Contracts;

/// <summary>
/// Defines a column role for mapping purposes.
/// </summary>
public record ColumnRole(
    string Key,
    string Label,
    bool Required,
    string? TypeHint = null
);

/// <summary>
/// Information about a data column.
/// </summary>
public record ColumnInfo(
    string Id,
    string Name,
    Type? DataType = null
);

/// <summary>
/// Generic column-to-role mapping for import dialogs.
/// Allows users to specify which data columns map to which roles.
/// </summary>
public interface IColumnRoleMapper
{
    /// <summary>
    /// Gets the available roles to map to.
    /// </summary>
    IReadOnlyList<ColumnRole> Roles { get; }

    /// <summary>
    /// Gets the available columns from the data source.
    /// </summary>
    IReadOnlyList<ColumnInfo> Columns { get; }

    /// <summary>
    /// Gets the current mapping of role keys to column IDs.
    /// </summary>
    /// <returns>Dictionary mapping role key to column ID.</returns>
    Dictionary<string, string> GetMapping();

    /// <summary>
    /// Sets the mapping for a specific role.
    /// </summary>
    /// <param name="roleKey">The role key.</param>
    /// <param name="columnId">The column ID to map to.</param>
    void SetMapping(string roleKey, string columnId);

    /// <summary>
    /// Validates that all required roles have mappings.
    /// </summary>
    /// <returns>True if all required roles are mapped.</returns>
    bool ValidateMapping();
}

