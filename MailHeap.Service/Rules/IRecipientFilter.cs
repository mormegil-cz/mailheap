using SmtpServer;
using SmtpServer.Mail;

namespace MailHeap.Service.Rules;

public interface IRecipientFilter
{
    Task<bool> Matches(ISessionContext context, IMailbox from, IMailbox to, CancellationToken cancellationToken);
}