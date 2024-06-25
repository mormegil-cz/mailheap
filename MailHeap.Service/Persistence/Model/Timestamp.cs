namespace MailHeap.Service.Persistence.Model;

/// <summary>
/// Wrapper for <see cref="DateTime"/> because of Sqlite’s representation fighting with Linq2db.
/// Possibly unnecessary but it works (unlike without it, which I could not make to work).
/// </summary>
/// <param name="value">The represented timestamp value</param>
public readonly struct Timestamp(DateTime value) : IFormattable
{
    public DateTime Value { get; } = value.ToUniversalTime();

    public override string ToString() => Value.ToString("u");

    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);

    public override int GetHashCode() => Value.GetHashCode();

    public override bool Equals(object? obj) => obj is Timestamp ts && Value.Equals(ts.Value);

    public static bool operator ==(Timestamp left, Timestamp right) => left.Equals(right);

    public static bool operator !=(Timestamp left, Timestamp right) => !(left == right);
}