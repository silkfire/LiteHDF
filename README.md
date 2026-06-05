# LiteHDF

![logo](https://raw.githubusercontent.com/silkfire/LiteHDF/refs/heads/main/img/logo.png)

[![NuGet](https://img.shields.io/nuget/v/LiteHDF.win-x64.svg)](https://www.nuget.org/packages/LiteHDF.win-x64)

A tiny high-level library that facilitates the reading of [HDF5](https://en.wikipedia.org/wiki/Hierarchical_Data_Format) files.

Uses [v2.1.1](https://github.com/HDFGroup/hdf5/releases/tag/2.1.1) native library under the hood.

**Note**: Only compatible with *Windows x64*.

## Usage

```csharp
try
{
    using var hdf = Hdf.Open(filePath);

    var energyValues = hdf.GetData<double>("/entries/data/energies");
}
catch (IOException)
{
    // File could not be opened (not found, access denied, or not a valid HDF document)
}
```

### Listing the group structure

Getting the structure of a group (or the root) is done by calling the `GetGroupObjectData` method. It returns an array of `HdfObject` structs containing the name, type and a reference to the file of each object in the group.

```csharp
HdfObject[] objects = hdf.GetGroupObjectData("/entries");

foreach (var obj in objects)
{
    Console.WriteLine($"{obj.Name} ({obj.Type})");
}
```

The `HdfObject` struct has the following properties:

```csharp
struct HdfObject
{
    string Name;        // Name of the object
    ObjectType Type;    // Group or Dataset
    HdfFile File;       // Reference to the containing file
}
```

### Reading data

Getting values from a dataset is done by the `GetData<TValue>` or `GetString` methods. The former returns an array of the specified data type (if it matches the data type of the dataset) and the latter is only used for datasets that contain variable-length string data.

The `GetData` method returns a `HdfData<TValue>` object:

```csharp
class HdfData<TValue>
{
    string DatasetPath;
    ulong? ChangeTime;
    TValue[] Value;
}
```

The `ChangeTime` property contains the modification time of the dataset as the number of seconds since the UNIX epoch. Use `DateTimeOffset.FromUnixTimeSeconds((long)changeTime)` to convert it into a native date time object.

If the dataset doesn't exist, or if `TValue` doesn't match the element size of the dataset's data type, `null` is returned.  
For single values, simply use the LINQ method `SingleOrDefault` on the `Value` property.

### Library version

To get the version of the underlying HDF5 library, use the `GetLibraryVersion` method:

```csharp
Version version = Hdf.GetLibraryVersion();
```
