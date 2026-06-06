namespace LiteHDF.Tests;

public class GetGroupObjectDataTests
{
    // ------------------------------------------------------------------
    // Root group listing
    // ------------------------------------------------------------------

    [Fact]
    public void GetGroupObjectData_root_returns_correct_count()
    {
        using var hdf = Hdf.Open(TestFiles.Structure);
        var objects = hdf.GetGroupObjectData("/");

        Assert.Equal(3, objects.Length);
    }

    [Fact]
    public void GetGroupObjectData_root_contains_expected_names()
    {
        using var hdf = Hdf.Open(TestFiles.Structure);
        var names = hdf.GetGroupObjectData("/").Select(o => o.Name).ToHashSet();

        Assert.Contains("ds_root_a", names);
        Assert.Contains("ds_root_b", names);
        Assert.Contains("groupA",    names);
    }

    [Fact]
    public void GetGroupObjectData_root_datasets_have_correct_type()
    {
        using var hdf = Hdf.Open(TestFiles.Structure);
        var byName = hdf.GetGroupObjectData("/").ToDictionary(o => o.Name);

        Assert.Equal(ObjectType.Dataset, byName["ds_root_a"].Type);
        Assert.Equal(ObjectType.Dataset, byName["ds_root_b"].Type);
    }

    [Fact]
    public void GetGroupObjectData_root_group_has_correct_type()
    {
        using var hdf = Hdf.Open(TestFiles.Structure);
        var byName = hdf.GetGroupObjectData("/").ToDictionary(o => o.Name);

        Assert.Equal(ObjectType.Group, byName["groupA"].Type);
    }

    // ------------------------------------------------------------------
    // Nested group — only direct children, not recursive
    // ------------------------------------------------------------------

    [Fact]
    public void GetGroupObjectData_nested_group_returns_direct_children_only()
    {
        using var hdf = Hdf.Open(TestFiles.Structure);
        var objects = hdf.GetGroupObjectData("/groupA");

        Assert.Equal(2, objects.Length);
        var names = objects.Select(o => o.Name).ToHashSet();
        Assert.Contains("ds_a", names);
        Assert.Contains("sub",  names);
    }

    [Fact]
    public void GetGroupObjectData_nested_group_children_have_correct_types()
    {
        using var hdf = Hdf.Open(TestFiles.Structure);
        var byName = hdf.GetGroupObjectData("/groupA").ToDictionary(o => o.Name);

        Assert.Equal(ObjectType.Dataset, byName["ds_a"].Type);
        Assert.Equal(ObjectType.Group,   byName["sub"].Type);
    }

    // ------------------------------------------------------------------
    // HdfObject metadata
    // ------------------------------------------------------------------

    [Fact]
    public void GetGroupObjectData_objects_reference_the_same_file()
    {
        using var hdf = Hdf.Open(TestFiles.Structure);
        var objects = hdf.GetGroupObjectData("/");

        Assert.All(objects, o => Assert.Same(hdf, o.File));
    }

    [Fact]
    public void GetGroupObjectData_object_ToString_contains_name_and_type()
    {
        using var hdf = Hdf.Open(TestFiles.Structure);
        var byName = hdf.GetGroupObjectData("/").ToDictionary(o => o.Name);

        var str = byName["groupA"].ToString();
        Assert.Contains("groupA", str);
        Assert.Contains("GROUP", str, StringComparison.OrdinalIgnoreCase);
    }

    // ------------------------------------------------------------------
    // Unsupported object type (named datatype) — must not throw
    // ------------------------------------------------------------------

    [Fact]
    public void GetGroupObjectData_named_datatype_maps_to_Unsupported_without_throwing()
    {
        using var hdf = Hdf.Open(TestFiles.Unsupported);

        // Must complete without throwing KeyNotFoundException
        var objects = hdf.GetGroupObjectData("/");

        var byName = objects.ToDictionary(o => o.Name);
        Assert.Equal(ObjectType.Unsupported, byName["named_type"].Type);
        Assert.Equal(ObjectType.Dataset,     byName["normal_ds"].Type);
    }
}
