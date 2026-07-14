# CLAUDE.md

Guidance for working in the **LiteHDF** repo. Focuses on non-obvious gotchas; the
code and README cover the rest.

## What this is

A tiny, **read-only**, **Windows-x64-only** .NET wrapper over the native HDF5 C
library, for reading HDF5 files. Public surface is small: `Hdf.Open`,
`HdfFile.GetGroupObjectData`, `HdfFile.GetData<T>`, `HdfFile.GetString`,
`Hdf.GetLibraryVersion`. Everything under `src/PInvoke/` is the hand-written
binding layer; treat it as the trust boundary.

## Layout & build

- Library project: `src/LiteHDF.csproj` (assembly name `LiteHDF.win-x64`),
  solution `LiteHDF.sln` (repo root). Tests: `tests/LiteHDF.Tests/` (xUnit v3).
- Target framework is `net10.0-windows`. The `-windows` and x64-only nature are
  intentional (see LLP64 note below) — don't "helpfully" make it cross-platform.
- Build: `dotnet build src/LiteHDF.csproj -c Debug`. Test:
  `dotnet test tests/LiteHDF.Tests/LiteHDF.Tests.csproj -c Debug`.
- The native `src/native/hdf5.dll` is committed to the repo and copied to output
  via `ContentWithTargetPath` / packed under `runtimes/win-x64/native/`. The
  bundled DLL version is the source of truth for which HDF5 API to bind against.

## Determining the HDF5 version (do this before any P/Invoke work)

The README states the version (currently **HDF5 2.1.1**, tag `2.1.1` in
`HDFGroup/hdf5`), and `src/LiteHDF.csproj` `PackageReleaseNotes` echoes it. When
verifying a binding, always pull the **official public headers at that exact tag**
and check against them — not against memory, not against a different version.
Relevant headers: `H5public.h`, `H5Lpublic.h`, `H5Opublic.h`, `H5Tpublic.h`,
`H5Spublic.h`, `H5Dpublic.h`, `H5Fpublic.h`, `H5Epublic.h`.

Fetch headers via the GitHub MCP server. They are large (90-110 KB) and will be
saved to a tool-results file rather than returned inline — that's expected. Read
them by grepping the saved file for the type name, or decode with a short Python
snippet (the file is JSON with `\n`-escaped content on one line).

## P/Invoke gotchas (the important part)

These are the traps that have actually bitten this project:

1. **HDF5 versioned entry points: the struct must match the function suffix.**
   HDF5 evolves structs by appending a digit to both the struct and the function
   (`H5O_info1_t` ↔ `H5Oget_info_by_name2`; `H5O_info2_t` ↔ `H5Oget_info_by_name3`).
   The numbers do **not** line up (`info1_t` is used by `..._by_name1` *and* `2`;
   `info2_t` by `3`). Pairing the wrong struct with the wrong entry point compiles
   fine, runs, and silently misreads every field after the first divergence —
   here it crashed `GetGroupObjectData` with a `NullReferenceException` because the
   misread `type` value missed the `s_objectTypes` dictionary. **Always confirm the
   struct/function pairing in the header**, don't trust the suffix matching.
   - Current correct pairing: `info2_t` (16-byte `token_t`, no trailing
     `hdr`/`meta_size`) with both `H5Oget_info_by_name3` (by path) and
     `H5Oget_info3` (by open object id — used in `GetData<T>` to read dataset
     metadata off the already-open `datasetId` instead of re-resolving the path).

2. **`unsigned long` is 4 bytes here, not 8.** Windows x64 is LLP64, so C
   `unsigned long` → `uint`, not `ulong`. `H5O_info2_t.fileno` is `unsigned long`
   and is correctly bound as `uint`. On Linux LP64 it would be 8 bytes — which is
   exactly why this library is pinned to Windows x64. Don't "fix" `fileno` to
   `ulong`.

3. **C99 `bool` is 1 byte; default marshalling makes it 4.** Under `LibraryImport`
   with runtime marshalling (this project does **not** set
   `DisableRuntimeMarshalling`), a `bool` struct field marshals as a 4-byte Win32
   `BOOL`. C99/`stdbool.h` `bool` is 1 byte. Mark such fields
   `[MarshalAs(UnmanagedType.U1)]` (see `H5L.info2_t.corder_valid`). It may happen
   to align by luck on x64, but make it explicit.

4. **Unions → `LayoutKind.Explicit` with overlapping `[FieldOffset(0)]`.** See
   `H5L.info2_t.u_t`: the 16-byte `H5O.token_t` and the 8-byte `val_size` both sit
   at offset 0. `token_t` itself is an empty struct sized via
   `[StructLayout(Size = 16)]` (matches `H5O_MAX_TOKEN_SIZE`).

5. **`size_t`/`ssize_t` → `nint`; `hid_t` → `long`; `herr_t` → `int`;
   `hsize_t`/`haddr_t`/`time_t` → `ulong`.** These are declared as `using` aliases
   at the top of each `H5*.cs` file — keep using the aliases for readability.

6. Calling convention is **cdecl** everywhere
   (`[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]`), and every import
   uses `SuppressUnmanagedCodeSecurity, SecuritySafeCritical`. Match the existing
   pattern when adding imports.

7. **HDF5 link/object names are UTF-8.** For `string` parameters (`H5Fopen`,
   `H5Dopen2`, `H5Oget_info_by_name3`, `H5Literate_by_name2` group name) use
   `[MarshalUsing(typeof(Utf8StringMarshaller))]`. Do **not** use
   `AnsiStringMarshaller` — on a non-UTF-8 ANSI code page it mis-encodes non-ASCII
   names. (Beware: a dev box with the "Use Unicode UTF-8" beta option has ACP 65001,
   so ANSI and UTF-8 behave identically there and a mis-marshalling won't reproduce.)
   The `H5Literate_by_name2` **callback** link name is no longer a marshalled `string`
   (see #8) — it arrives as a raw `byte*`; recover it with `Marshal.PtrToStringUTF8`
   (still UTF-8, still not ANSI — the rule stands, only the mechanism changed).

8. **The `H5Literate_by_name2` operator is an unmanaged function pointer, not a
   delegate.** `H5L.iterate_by_name`'s `op` parameter is
   `delegate* unmanaged[Cdecl]<hid_t, byte*, info2_t*, nint, herr_t>`, matching the C
   `herr_t (*)(hid_t group, const char *name, const H5L_info2_t *info, void *op_data)`
   directly (link name as `byte*`, info as `info2_t*` — mirroring the `const
   H5L_info2_t *` pointer; never declare `info` by value). The managed operator is a
   `static [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]` method
   (`HdfFile.OnLink`); because it can't capture, the accumulation state is threaded
   through `op_data` as a pinned `GCHandle` and recovered in the callback. It must not
   let a managed exception escape into the native frame — on failure it records the
   error in the state and returns a negative `herr_t` to abort iteration, and the
   managed caller re-throws after `iterate_by_name` returns.

## `[SuppressGCTransition]` depends on the non-threadsafe DLL

Four pure in-memory getters carry `[SuppressGCTransition]` to skip the GC transition
on the `GetData<T>` hot path: `H5T.get_size` and `H5S.get_simple_extent_{ndims,type,
dims}`. They are safe because they do no I/O, no allocation, no ID-table mutation, and
no callbacks — **and** because the bundled `src/native/hdf5.dll` is a non-threadsafe
build, so there is no global library mutex to acquire (the attribute's contract
forbids taking locks).

This is a hard precondition. If the bundled DLL is ever swapped for a **threadsafe**
build, the library's recursive mutex would be acquired under these calls and the "no
locks" contract would be violated — **remove the attribute from all four** in that
case. To re-check the bundled build's thread safety, scan the DLL for its embedded
config summary (a Python byte-scan for `SUMMARY OF THE HDF5 CONFIGURATION`; look for
the `Threadsafety:` line — currently `OFF`).

Do **not** extend `[SuppressGCTransition]` to `open`/`read`/`close`/`get_info*` (I/O
or path-walking), `get_space`/`get_type` (allocate + register a new ID), or
`iterate_by_name` (runs a managed callback).

## Enum fidelity

Enums that are only consumed by managed code (e.g. the `NTYPES`/`N` sentinels,
reserved `cset_t` members) don't affect marshalling, but should still mirror the
upstream enum exactly for correctness and future-proofing — e.g. `H5O.type_t` must
include `MAP`, `H5T.cset_t` goes up to `RESERVED_15`. When in doubt, copy the full
enum from the header.

## Verifying native-interop changes (don't trust "it compiles")

A successful build proves nothing about struct layout. The test suite under
`tests/LiteHDF.Tests/` does exercise the real marshalling paths, so run it first:
`dotnet test tests/LiteHDF.Tests/LiteHDF.Tests.csproj -c Debug`. It drives
committed h5py-authored files (`TestData/*.h5`) through the public surface —
`GetGroupObjectData` (reads `oinfo.type`, which is what the `info2_t`/entry-point
pairing regression crashed on), `GetData<T>` across every numeric width and
several dataspace shapes, `GetString`, and `Open` — so a struct-layout regression
will generally make a test fail rather than pass silently.

That said, the suite can't anticipate a new field or struct you add. When you
touch `info2_t`, an entry point, or any struct that isn't already covered:

1. Extend the suite (or, for a quick spike, drive a throwaway console project that
   references `src/LiteHDF.csproj`) with a file that actually contains the thing
   you changed. Author HDF5 files with `python` + `h5py` (`pip install h5py`; it
   bundles its own HDF5 and produces format-compatible files); see `TestData/` for
   the existing fixtures.
2. To prove a fix matters, temporarily revert it and re-run — confirm it breaks.
3. Clean up any scratch project/file afterward (don't leave it next to the tests).

Note: `ctime` is often `0`/`null` for h5py-authored files (HDF5 doesn't always
record object change times) — `GetData_change_time_is_null_for_h5py_file` asserts
exactly this. That's correct behavior, not a marshalling bug — distinguish "zero
from the right offset" from "garbage from the wrong offset".

## Test fixtures

The `.h5` files in `tests/LiteHDF.Tests/TestData/` are committed directly.
There is no generator script in the repo — they were produced once with h5py and
committed. If you need to recreate or extend them:

```python
import numpy as np, h5py
# see the commit that introduced the TestData/ folder for the full script
```

Each file targets a specific concern:
- `numeric.h5` — one dataset per numeric type (`/i8`…`/f64`), boundary values
- `shapes.h5` — dataspace classes: `/scalar`, `/vector`, `/matrix`, `/cube`, `/empty` (NULL)
- `strings.h5` — variable-length strings: `/vlen_ascii`, `/vlen_utf8`
- `structure.h5` — group nesting: root has two datasets + `groupA`; `groupA` has `sub` + `ds_a`
- `unsupported.h5` — a committed named datatype (`/named_type`) + a normal dataset,
  used to test `ObjectType.Unsupported`
- `endian.h5` — big-endian `/be_i32`, `/be_f64` (+ little-endian `/le_i32`) to prove
  `GetData<T>` converts byte order
- `strings_edge.h5` — `/vlen_array` (N>1 vlen), `/fixed_ascii` (fixed-length),
  `/vlen_single`; exercises the `GetString` guards
- `unicode_names.h5` — datasets with non-ASCII names (`mätvärden`, `温度`,
  `gruppe/café`) to prove UTF-8 name marshalling

Note: a test source file containing non-ASCII string literals (e.g.
`UnicodeNameTests.cs`) must be saved as **UTF-8 with BOM** so Roslyn reads the
literals correctly regardless of the build host's locale.

When adding a new fixture, also add a `Content` item in `LiteHDF.Tests.csproj` —
the `Content Include="TestData\*.h5"` glob already covers any new `.h5` in that folder.

## Test parallelism

`AssemblyInfo.cs` (or the equivalent `<AssemblyAttribute>` in the csproj) sets
`[assembly: CollectionBehavior(DisableTestParallelization = true)]`. **Do not
remove this.** The HDF5 native library has global process-level state; running
xUnit's default parallel test-class execution causes an access-violation crash
(`0xC0000005`) that kills the test host. All tests must run sequentially within a
single process.

## Reads convert to native type and fail loudly

Two invariants in `HdfFile` that are easy to accidentally undo:

- **`GetData<T>` reads into the *native* in-memory type**, obtained via
  `H5Tget_native_type(fileType, DEFAULT)` — not the file type. Passing the file type
  as `H5Dread`'s `mem_type_id` suppresses conversion, so a big-endian (or otherwise
  non-native) dataset reads as byte-swapped garbage. The size check compares
  `H5Tget_size(nativeType)` to `sizeof(T)`. `endian.h5` covers this; don't revert to
  passing the file type.

- **Failed native reads throw `IOException`, they don't return zeroed data.**
  `H5Dread`, `H5Literate_by_name2`, `H5Oget_info_by_name3`, and `H5Oget_info3` return
  values are checked; a negative `herr_t` throws. A silently-zeroed buffer that looks
  like valid data is the worst failure mode for a data library, so keep these checked.
  `GetGroupObjectData` on a nonexistent group therefore **throws** (a negative iterate
  return) rather than returning an empty collection — don't "helpfully" swallow it back
  to an empty result. Note the iteration operator is now an unmanaged function pointer
  (see P/Invoke gotcha #8): a per-child metadata-read failure is recorded in the
  callback state and the callback returns negative to abort; `GetGroupObjectData`
  re-throws the `IOException` after `iterate_by_name` returns rather than throwing
  across the native frame. Observable behavior is unchanged.

## File handle is a SafeHandle

The native file handle is wrapped in `Hdf5FileHandle : SafeHandle`
(`src/PInvoke/Hdf5FileHandle.cs`); `ReleaseHandle` calls `H5Fclose`. `HdfFile` has
no hand-rolled finalizer — the SafeHandle finalizes the handle. `FileIdentifier`
projects `_handle.FileId`, and `ToString` reports `NULL` once the handle is closed
(`IsClosed`/`IsInvalid`). `hid_t` is a 64-bit `long` stored in the handle's `nint`,
which is safe because this library is x64-only.

## GetString: variable-length, single-element only

`GetString` only supports a **single** variable-length (`H5T_VARIABLE`) string
element. It guards the dataspace element count (`H5Sget_simple_extent_npoints == 1`)
and the datatype (`H5Tis_variable_str`), returning `null` otherwise — a multi-element
vlen dataset would overrun the single-pointer read buffer (memory corruption), and a
fixed-length string isn't a pointer at all. `strings_edge.h5` covers both rejected
cases plus the single-element happy path. Don't remove the guard; the doc comment's
"undefined behaviour" wording predates it, but the guard is what keeps it safe.
