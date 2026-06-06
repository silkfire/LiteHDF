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
}
