namespace LiteHDF.Tests;

public class HdfTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Should_return_libversion()
    {
        var libVersion = Hdf.GetLibraryVersion();

        testOutputHelper.WriteLine(libVersion.ToString());
    }
}