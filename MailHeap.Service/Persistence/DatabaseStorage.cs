using LinqToDB;
using MailHeap.Service.Persistence.Model;

namespace MailHeap.Service.Persistence;

public class DatabaseStorage(
    ILogger<DatabaseStorage> logger,
    DbConnectionFactory dbConnectionFactory
) : IMailStorage
{
    public async Task SaveMail(EmailMessage emailMessage, CancellationToken cancellationToken)
    {
        await using var db = dbConnectionFactory.Create();
        var insertedId = await db.InsertWithInt64IdentityAsync(emailMessage, token: cancellationToken);
        logger.LogInformation("Inserted message #{id}", insertedId);
    }
}