namespace LiteHDF.Tests;

public class LibraryTests(ITestOutputHelper output)
{
    [Fact]
    public void GetLibraryVersion_returns_non_null_version()
    {
        var version = Hdf.GetLibraryVersion();

        Assert.NotNull(version);
        Assert.Equal(2, version.Major);

        output.WriteLine(version.ToString());
    }
}
