namespace LiteHDF;

/// <summary>
/// Holds the data read from an HDF5 dataset.
/// </summary>
/// <typeparam name="TValue">Unmanaged element type of the dataset.</typeparam>
public class HdfData<TValue>
{
    /// <summary>
    /// Absolute path to the dataset within the HDF5 file.
    /// </summary>
    public required string DatasetPath { get; init; }

    /// <summary>
    /// Change time of the dataset as a Unix timestamp, or <see langword="null"/> if not recorded.
    /// </summary>
    public required ulong? ChangeTime { get; init; }

    /// <summary>
    /// The data read from the dataset.
    /// </summary>
    public required TValue[] Value { get; init; }

    /// <inheritdoc/>
    public override string ToString() => DatasetPath;
}
