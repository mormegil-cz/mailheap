using LinqToDB;
using LinqToDB.Mapping;

// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MailHeap.Service.Persistence.Model;

[Table("MESSAGES")]
public class EmailMessage
{
    [Column("ID"), PrimaryKey, Identity]
    public long Id { get; set; }

    [Column("TIMESTAMP", Configuration = ProviderName.SQLite, DataType = DataType.Double, DbType = "REAL"), NotNull]
    public Timestamp Timestamp { get; set; }

    [Column("STATE"), NotNull]
    public MessageState State { get; set; }

    [Column("ENVELOPE_FROM"), NotNull]
    public required string EnvelopeFrom { get; set; }

    [Column("ENVELOPE_TO"), NotNull]
    public required string EnvelopeTo { get; set; }

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
    public required byte[] Message { get; set; }

    [Column("PARAMS"), Nullable]
    public string? Parameters { get; set; }

    [Column("FORWARD_TO"), Nullable]
    public string? ForwardTo { get; set; }
}

public enum MessageState
{
    Stored = 0,
    ToForwardAndKeep = 1,
    ToForwardAndDelete = 2,
    Kept = 3,
    FailedForwardToDelete = 4,
    FailedForwardToKeep = 5,
}