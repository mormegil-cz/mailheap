namespace MailHeap.Service.Persistence.Model;

public readonly struct Timestamp(DateTime value)
{
    public DateTime Value { get; } = value;
}