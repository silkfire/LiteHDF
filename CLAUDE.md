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
  solution `src/LiteHDF.sln`. Tests: `tests/LiteHDF.Tests/` (xUnit v3).
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
     `hdr`/`meta_size`) with `H5Oget_info_by_name3`.

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

The five `.h5` files in `tests/LiteHDF.Tests/TestData/` are committed directly.
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

When adding a new fixture, also add a `Content` item in `LiteHDF.Tests.csproj` —
the `Content Include="TestData\*.h5"` glob already covers any new `.h5` in that folder.

## Test parallelism

`AssemblyInfo.cs` (or the equivalent `<AssemblyAttribute>` in the csproj) sets
`[assembly: CollectionBehavior(DisableTestParallelization = true)]`. **Do not
remove this.** The HDF5 native library has global process-level state; running
xUnit's default parallel test-class execution causes an access-violation crash
(`0xC0000005`) that kills the test host. All tests must run sequentially within a
single process.

## GetString: variable-length only

`GetString` only supports variable-length (`H5T_VARIABLE`) string datasets. The
doc comment on `HdfFile.GetString` says fixed-length strings "will cause undefined
behaviour" — this means the result is unpredictable and may crash the test host.
The test suite deliberately does not include a fixed-length string fixture or test.
