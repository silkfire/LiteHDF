namespace LiteHDF.Tests;

public class GetStringTests
{
    [Fact]
    public void GetString_vlen_ascii_returns_correct_value()
    {
        using var hdf = Hdf.Open(TestFiles.Strings);

        Assert.Equal("hello world", hdf.GetString("/vlen_ascii"));
    }

    [Fact]
    public void GetString_vlen_utf8_round_trips_non_ascii_characters()
    {
        using var hdf = Hdf.Open(TestFiles.Strings);

        Assert.Equal("héllo ☃", hdf.GetString("/vlen_utf8"));
    }

    [Fact]
    public void GetString_missing_dataset_returns_null()
    {
        using var hdf = Hdf.Open(TestFiles.Strings);

        Assert.Null(hdf.GetString("/does_not_exist"));
    }

    [Fact]
    public void GetString_multi_element_dataset_returns_null_without_corrupting_memory()
    {
        using var hdf = Hdf.Open(TestFiles.StringsEdge);

        // A dataspace with more than one variable-length string element would overrun
        // the single-pointer read buffer; it must be rejected rather than read.
        Assert.Null(hdf.GetString("/vlen_array"));
    }

    [Fact]
    public void GetString_fixed_length_dataset_returns_null()
    {
        using var hdf = Hdf.Open(TestFiles.StringsEdge);

        // Fixed-length strings are not pointers and are not supported.
        Assert.Null(hdf.GetString("/fixed_ascii"));
    }

    [Fact]
    public void GetString_single_element_vlen_dataset_returns_value()
    {
        using var hdf = Hdf.Open(TestFiles.StringsEdge);

        Assert.Equal("solo", hdf.GetString("/vlen_single"));
    }
}
