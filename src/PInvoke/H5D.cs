namespace LiteHDF.PInvoke;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security;

using herr_t = int;
using size_t = nint;
using hid_t = long;

/// <summary>
/// Manage HDF5 datasets, including the transfer of data between memory and disk and the description of dataset properties.
/// </summary>
internal sealed partial class H5D
{
    /// <summary>
    /// Closes the specified dataset.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5D.html#Dataset-Close" /> for further reference.</para>
    /// </summary>
    /// <param name="dset_id">Identifier of the dataset to close access to.</param>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HD5LibraryName, EntryPoint = "H5Dclose"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial herr_t close(hid_t dset_id);

    /// <summary>
    /// Returns an identifier for a copy of the dataspace for a dataset.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5D.html#Dataset-GetSpace" /> for further reference.</para>
    /// </summary>
    /// <param name="dset_id">Identifier of the dataset to query.</param>
    /// <returns>Returns a dataspace identifier if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HD5LibraryName, EntryPoint = "H5Dget_space"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial hid_t get_space(hid_t dset_id);

    /// <summary>
    /// Returns an identifier for a copy of the datatype for a dataset.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5D.html#Dataset-GetType" /> for further reference.</para>
    /// </summary>
    /// <param name="dset_id">Identifier of the dataset to query.</param>
    /// <returns>Returns a datatype identifier if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HD5LibraryName, EntryPoint = "H5Dget_type"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial hid_t get_type(hid_t dset_id);

    /// <summary>
    /// Opens an existing dataset.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5D.html#Dataset-Open2" /> for further reference.</para>
    /// </summary>
    /// <param name="file_id">Location identifier.</param>
    /// <param name="name">Dataset name.</param>
    /// <param name="dapl_id">Dataset access property list.</param>
    /// <returns>Returns a dataset identifier if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HD5LibraryName, EntryPoint = "H5Dopen2"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial hid_t open(hid_t file_id, [MarshalUsing(typeof(AnsiStringMarshaller))] string name, hid_t dapl_id);

    /// <summary>
    /// Reads raw data from a dataset into a buffer.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5D.html#Dataset-Read" /> for further reference.</para>
    /// </summary>
    /// <param name="dset_id">Identifier of the dataset read from.</param>
    /// <param name="mem_type_id">Identifier of the memory datatype.</param>
    /// <param name="mem_space_id">Identifier of the memory dataspace.</param>
    /// <param name="file_space_id">Identifier of the dataset's dataspace in the file.</param>
    /// <param name="plist_id">Identifier of a transfer property list for this I/O operation.</param>
    /// <param name="buf">Buffer to receive data read from file.</param>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HD5LibraryName, EntryPoint = "H5Dread"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial herr_t read(hid_t dset_id, hid_t mem_type_id, hid_t mem_space_id, hid_t file_space_id, hid_t plist_id, size_t buf);
}
