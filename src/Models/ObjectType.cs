namespace LiteHDF;

/// <summary>
/// Defines an object type.
/// </summary>
public enum ObjectType
{
    /// <summary>
    /// A group, which can contain other groups and datasets.
    /// </summary>
    Group,

    /// <summary>
    /// A dataset containing raw data.
    /// </summary>
    Dataset,

    /// <summary>
    /// An object of a type LiteHDF does not model, such as a named datatype or map.
    /// </summary>
    Unsupported
}
