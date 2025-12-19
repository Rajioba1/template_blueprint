namespace AvaloniaTemplateBlueprint.Core.Contracts;

/// <summary>
/// Event arguments for data change events.
/// </summary>
public class DataChangedEventArgs : EventArgs
{
    public DataChangeType ChangeType { get; }

    public DataChangedEventArgs(DataChangeType changeType)
    {
        ChangeType = changeType;
    }
}

/// <summary>
/// Type of data change.
/// </summary>
public enum DataChangeType
{
    Loaded,
    Modified,
    Cleared
}

/// <summary>
/// Generic data source for loading and providing data.
/// </summary>
/// <typeparam name="T">The type of data items.</typeparam>
public interface IDataSource<T>
{
    /// <summary>
    /// Gets the current data items.
    /// </summary>
    IEnumerable<T> GetData();

    /// <summary>
    /// Loads data from a file.
    /// </summary>
    /// <param name="path">The file path to load from.</param>
    /// <returns>The loaded data items.</returns>
    Task<IEnumerable<T>> LoadAsync(string path);

    /// <summary>
    /// Raised when data changes.
    /// </summary>
    event EventHandler<DataChangedEventArgs>? DataChanged;
}
