namespace LiteHDF.PInvoke;

using hid_t = long;

/// <summary>
/// HDF5 property lists are the main vehicle to configure the behavior of HDF5 API functions.
/// </summary>
internal sealed class H5P
{
    /// <summary>
    /// Default value for all property list classes
    /// </summary>
    public const hid_t DEFAULT = 0;
}
