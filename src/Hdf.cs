namespace LiteHDF;

using PInvoke;

using System;

/// <summary>
/// Entry point for opening HDF5 files and querying library information.
/// </summary>
public static class Hdf
{
    /// <summary>
    /// Opens an existing HDF5 file for reading.
    /// </summary>
    /// <param name="filename">Path to the HDF5 file.</param>
    /// <returns>An <see cref="HdfFile"/> representing the open file. Dispose it when done.</returns>
    /// <exception cref="System.IO.IOException">Thrown when the file cannot be opened (file not found, access denied, or not a valid HDF5 file).</exception>
    public static HdfFile Open(string filename)
    {
        return new HdfFile(filename);
    }

    /// <summary>
    /// Gets the version of the underlying HDF5 library.
    /// </summary>
    public static Version GetLibraryVersion()
    {
        uint majorVersion = 0U, minorVersion = 0U, releaseVersion = 0U;
        H5.get_libversion(ref majorVersion, ref minorVersion, ref releaseVersion);

        var libraryVersion = new Version((int)majorVersion, (int)minorVersion, (int)releaseVersion);

        return libraryVersion;
    }
}
