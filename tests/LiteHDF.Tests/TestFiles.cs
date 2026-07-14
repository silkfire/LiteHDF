namespace LiteHDF.Tests;

internal static class TestFiles
{
    private static string Path(string filename) => System.IO.Path.Combine(AppContext.BaseDirectory, "TestData", filename);

    public static string Numeric     => Path("numeric.h5");
    public static string Endian      => Path("endian.h5");
    public static string Shapes      => Path("shapes.h5");
    public static string Strings     => Path("strings.h5");
    public static string StringsEdge => Path("strings_edge.h5");
    public static string Structure   => Path("structure.h5");
    public static string Unsupported => Path("unsupported.h5");
}
