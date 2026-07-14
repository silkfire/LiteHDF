namespace LiteHDF.Tests;

public class UnicodeNameTests
{
    [Fact]
    public void GetGroupObjectData_decodes_non_ascii_dataset_names()
    {
        using var hdf = Hdf.Open(TestFiles.UnicodeNames);
        var names = hdf.GetGroupObjectData("/").Select(o => o.Name).ToHashSet();

        Assert.Contains("mätvärden", names);
        Assert.Contains("温度",       names);
        Assert.Contains("gruppe",    names);
    }

    [Fact]
    public void GetData_reads_dataset_with_non_ascii_name()
    {
        using var hdf = Hdf.Open(TestFiles.UnicodeNames);
        var data = hdf.GetData<int>("/mätvärden");

        Assert.NotNull(data);
        Assert.Equal([1, 2, 3], data.Value);
    }

    [Fact]
    public void GetData_reads_nested_dataset_with_non_ascii_name()
    {
        using var hdf = Hdf.Open(TestFiles.UnicodeNames);
        var data = hdf.GetData<int>("/gruppe/café");

        Assert.NotNull(data);
        Assert.Equal([7], data.Value);
    }
}
