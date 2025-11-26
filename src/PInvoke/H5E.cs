namespace LiteHDF.PInvoke;

using herr_t = int;
using ssize_t = nint;
using hid_t = long;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

/// <summary>
/// HDF5 library error reporting.
/// </summary>
internal sealed partial class H5E
{
    /// <summary>
    /// Value for the default error stack.
    /// </summary>
    public const hid_t DEFAULT = 0;

    /// <summary>
    /// Callback for error handling.
    /// </summary>
    /// <param name="estack">Error stack identifier.</param>
    /// <param name="client_data">Pointer to client data in the format expected by the user-defined function.</param>
    /// <returns>Returns a non-negative value if successful; otherwise returns a negative value.</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate herr_t auto_t(hid_t estack, ssize_t client_data);

    /// <summary>
    /// Turns automatic error printing on or off.
    /// <para>See <see href="https://support.hdfgroup.org/HDF5/doc/RM/RM_H5E.html#Error-SetAuto2" /> for further reference.</para>
    /// </summary>
    /// <param name="estack_id">Error stack identifier.</param>
    /// <param name="func">Function to be called upon an error condition.</param>
    /// <param name="client_data">Data passed to the error function.</param>
    /// <returns>Returns a non-negative value on success; otherwise returns a negative value.</returns>
    [LibraryImport(Constants.HDF5LibraryName, EntryPoint = "H5Eset_auto2"), SuppressUnmanagedCodeSecurity, SecuritySafeCritical]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial herr_t set_auto(hid_t estack_id, auto_t? func, ssize_t client_data);
}
