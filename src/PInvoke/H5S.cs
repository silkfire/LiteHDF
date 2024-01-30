namespace LiteHDF.PInvoke;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

using herr_t = int;
using hsize_t = ulong;
using hid_t = long;

/// <summary>
/// Manage the lifecycle of HDF5 library instances.
/// </summary>
internal sealed partial class H5S
{
    // Define atomic datatypes
    public const int ALL = 0;

    /// <summary>
    /// Releases and terminates access to a dataspace.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5S.html#Dataspace-Close" /> for further reference.</para>
    /// </summary>
    /// <param name="space_id">Identifier of dataspace to release.</param>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HD5LibraryName, EntryPoint = "H5Sclose"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial herr_t close(hid_t space_id);

    /// <summary>
    /// Retrieves dataspace dimension size and maximum size.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5S.html#Dataspace-ExtentDims" /> for further reference.</para>
    /// </summary>
    /// <param name="space_id">Identifier of the dataspace object to query.</param>
    /// <param name="dims">Pointer to array to store the size of each dimension.</param>
    /// <param name="maxdims">Pointer to array to store the maximum size of each dimension.</param>
    /// <returns>Returns the number of dimensions in the dataspace if successful; otherwise returns a negative value.</returns>
    /// <remarks>Either or both of <paramref name="dims"/> and <paramref name="maxdims"/> may be <c>NULL</c>.</remarks>
    [LibraryImport(Constants.HD5LibraryName, EntryPoint = "H5Sget_simple_extent_dims"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int get_simple_extent_dims(hid_t space_id, [Out] hsize_t[] dims, [Out] hsize_t[] maxdims);

    /// <summary>
    /// Determines the dimensionality of a dataspace.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5S.html#Dataspace-ExtentNdims" /> for further reference.</para>
    /// </summary>
    /// <param name="space_id">Identifier of the dataspace.</param>
    /// <returns>Returns the number of dimensions in the dataspace if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HD5LibraryName, EntryPoint = "H5Sget_simple_extent_ndims"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int get_simple_extent_ndims(hid_t space_id);
}
