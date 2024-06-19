using SmtpServer;
using SmtpServer.Mail;

namespace MailHeap.Service.Rules;

public interface ISenderFilter
{
    Task<bool> Matches(ISessionContext context, IMailbox from, int size, CancellationToken cancellationToken);
}