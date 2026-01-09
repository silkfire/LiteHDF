namespace LiteHDF;

public class HdfData<TValue>
{
    public required string DatasetPath { get; init; }

    public required ulong? ChangeTime { get; init; }

    public required TValue[] Value { get; init; }

    public override string ToString() => DatasetPath;
}
