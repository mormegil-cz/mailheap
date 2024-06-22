using LinqToDB.Mapping;

namespace MailHeap.Service.Persistence.Model;

[Table("MESSAGES")]
public class EmailMessage
{
    [PrimaryKey, Identity]
    public long Id { get; set; }

    [Column("TIMESTAMP"), NotNull]
    public DateTime Timestamp { get; set; }

    [Column("STATE"), NotNull]
    public MessageState State { get; set; }

    [Column("ENVELOPE_FROM"), NotNull]
    public string EnvelopeFrom { get; set; }

    [Column("ENVELOPE_TO"), NotNull]
    public string EnvelopeTo { get; set; }

    [Column("FROM"), Nullable]
    public string? From { get; set; }

    [Column("SUBJECT"), Nullable]
    public string? Subject { get; set; }

    [Column("SOURCE_IP"), Nullable]
    public string? SourceIpAddr { get; set; }

    [Column("SOURCE_PORT"), Nullable]
    public int? SourcePort { get; set; }

    [Column("SECURED"), NotNull]
    public bool Secured { get; set; }

    [Column("HELLO_NAME"), Nullable]
    public string? HelloName { get; set; }

    [Column("MESSAGE_DATA"), NotNull]
    public byte[] Message { get; set; }

    [Column("PARAMS"), Nullable]
    public string? Parameters { get; set; }
}

public enum MessageState
{
    Stored = 0,
    ToForwardAndKeep = 1,
    ToForwardAndDelete = 2,
    Kept = 3,
}