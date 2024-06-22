using LinqToDB;
using MailHeap.Service.Persistence.Model;

namespace MailHeap.Service.Persistence;

public class SqliteStorage(
    ILogger<SqliteStorage> logger
) : IMailStorage
{
    public async Task SaveMail(EmailMessage emailMessage, CancellationToken cancellationToken)
    {
        await using var db = new DbConnection();
        var insertedId = await db.InsertWithInt64IdentityAsync(emailMessage, token: cancellationToken);
        logger.LogInformation("Inserted message #{id}", insertedId);
    }
}