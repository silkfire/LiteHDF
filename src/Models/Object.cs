namespace LiteHDF;

/// <summary>
/// Represents an HDF5 object (group or dataset).
/// </summary>
public readonly struct Object
{
    /// <summary>
    /// Name of the object.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type of the object (group or dataset).
    /// </summary>
    public required ObjectType Type { get; init; }

    /// <summary>
    /// Reference to the file containing this object.
    /// </summary>
    public required HdfFile File { get; init; }


    public override string ToString() => $"{Name} · {Type.ToString().ToUpperInvariant()}";
}
