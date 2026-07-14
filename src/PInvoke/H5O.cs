namespace LiteHDF.PInvoke;

using herr_t = int;
using hsize_t = ulong;
using time_t = ulong;
using hid_t = long;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security;

/// <summary>
/// Manage HDF5 objects (groups, datasets, datatype objects).
/// </summary>
internal static partial class H5O
{
    /// <summary>
    /// Fill in the fileno, token, type, and rc fields.
    /// </summary>
    public const uint H5O_INFO_BASIC = 0x0001U;

    /// <summary>
    /// Fill in the atime, mtime, ctime, and btime fields.
    /// </summary>
    public const uint H5O_INFO_TIME = 0x0002U;

    /// <summary>
    /// Types of objects in file.
    /// </summary>
    public enum type_t
    {
        /// <summary>
        /// Unknown object type.
        /// </summary>
        UNKNOWN = -1,

        /// <summary>
        /// Object is a group.
        /// </summary>
        GROUP,

        /// <summary>
        /// Object is a dataset.
        /// </summary>
        DATASET,

        /// <summary>
        /// Object is a named data type.
        /// </summary>
        NAMED_DATATYPE,

        /// <summary>
        /// Object is a map.
        /// </summary>
        MAP,

        /// <summary>
        /// Number of different object types (must be last!).
        /// </summary>
        NTYPES
    }

    /// <summary>
    /// Type for object tokens.
    /// </summary>
    /// <remarks>An object token is an opaque, fixed-size (16-byte) identifier of an object within an HDF5 container.</remarks>
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct token_t { }

    /// <summary>
    /// Data model information struct for objects (for <see cref="get_info_by_name"/>).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct info2_t
    {
        /// <summary>
        /// File number that object is located in. Constant across multiple opens of the same file.
        /// </summary>
        public uint fileno;

        /// <summary>
        /// Token representing the object.
        /// </summary>
        public token_t token;

        /// <summary>
        /// Basic object type (group, dataset, etc.).
        /// </summary>
        public type_t type;

        /// <summary>
        /// Reference count of object.
        /// </summary>
        public uint rc;

        /// <summary>
        /// Access time.
        /// </summary>
        public time_t atime;

        /// <summary>
        /// Modification time.
        /// </summary>
        public time_t mtime;

        /// <summary>
        /// Change time.
        /// </summary>
        public time_t ctime;

        /// <summary>
        /// Birth time.
        /// </summary>
        public time_t btime;

        /// <summary>
        /// Number of attributes attached to object.
        /// </summary>
        public hsize_t num_attrs;
    }

    /// <summary>
    /// Retrieves the metadata for an object, identifying the object by location and relative name.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5O.html#Object-GetInfoByName" /> for further reference.</para>
    /// </summary>
    /// <param name="loc_id">File or group identifier specifying location of group in which object is located.</param>
    /// <param name="name">Name of object, relative to <paramref name="loc_id"/>.</param>
    /// <param name="oinfo">Buffer in which to return object information.</param>
    /// <param name="fields">Flags specifying the fields to include in <paramref name="oinfo"/>.</param>
    /// <param name="lapl_id">Link access property list.</param>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HDF5LibraryName, EntryPoint = "H5Oget_info_by_name3"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial herr_t get_info_by_name(hid_t loc_id, [MarshalUsing(typeof(Utf8StringMarshaller))] string name, out info2_t oinfo, uint fields, hid_t lapl_id);

    /// <summary>
    /// Retrieves the metadata for an object specified by an identifier.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5O.html#Object-GetInfo" /> for further reference.</para>
    /// </summary>
    /// <param name="loc_id">Identifier of the object.</param>
    /// <param name="oinfo">Buffer in which to return object information.</param>
    /// <param name="fields">Flags specifying the fields to include in <paramref name="oinfo"/>.</param>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    /// <remarks>Uses the same <see cref="info2_t"/> struct as <see cref="get_info_by_name"/>; both pair with the
    /// version-3 entry points. Prefer this over <see cref="get_info_by_name"/> when an object identifier is already
    /// open, to avoid re-resolving the object's path.</remarks>
    [LibraryImport(Constants.HDF5LibraryName, EntryPoint = "H5Oget_info3"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial herr_t get_info(hid_t loc_id, out info2_t oinfo, uint fields);
}
