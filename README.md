# LiteHDF

![logo](https://github.com/silkfire/LiteHDF/blob/master/img/logo.png)

[![NuGet](https://img.shields.io/nuget/v/LiteHDF.svg)](https://www.nuget.org/packages/LiteHDF)

A tiny high-level library that facilitates the reading of [HDF5](https://en.wikipedia.org/wiki/Hierarchical_Data_Format) files.

## Usage

```csharp
using var hdf = Hdf.Open(filepath);

if (file.FileIdentifier < 0L)
{
    // File not a valid HDF document
}

double energyValues = file.GetData<double>($"/entries/data/energies");
```

### Listing the group structure

Getting the structure of a group (or the root) is done by calling the `IterateGroup` method. The first argument is a path to the group to list and the second argument is a `GroupIterationCallback` delegate:

`void GroupIterationCallback(string objectName, ObjectType type, ulong creationTimeUnixSeconds)`

On every iteration, it will return the name, type (group or dataset) and the creation time (if existent) of the object of the current iteration.

```csharp
hdf.IterateGroup("/entries", (name, type, creationTime) => {
    // Action to perform on each iteration
}
```

The creation time is represented by the number of seconds since the UNIX epoch. Use `DateTimeOffset.FromUnixTimeSeconds(creationTime)` to convert it into a native date time object.

### Reading data

Getting values from a dataset is done by the `GetData<TValue>` or `GetString` methods. The former returns an array of the specified data type (if it matches the data type of the dataset) and the latter is only used for datasets that contain string data.

The `GetData` method returns a `HdfData<TValue>` object:

```csharp
class HdfObject<TValue> {
    string DatasetPath;
    ulong? ChangeTime;
    TValue[] Value;
}
```

If the dataset doesn't exist, a `null` object is returned.  
For single values, simply use the LINQ method `SingleOrDefault` on the `Value` property.
