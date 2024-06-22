using MailHeap.Service.Persistence.Model;

namespace MailHeap.Service.Persistence;

public interface IMailStorage
{
    Task SaveMail(EmailMessage emailMessage, CancellationToken cancellationToken);
}