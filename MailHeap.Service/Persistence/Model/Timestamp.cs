namespace MailHeap.Service.Persistence.Model;

public readonly struct Timestamp(DateTime value)
{
    public DateTime Value { get; } = value;

    public override string ToString() => Value.ToString("u");

    public override bool Equals(object? obj) => obj is Timestamp ts && Value.Equals(ts.Value);

    public override int GetHashCode() => Value.GetHashCode();
}