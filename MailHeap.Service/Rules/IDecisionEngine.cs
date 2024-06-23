using MimeKit;
using SmtpServer.Mail;

namespace MailHeap.Service.Rules;

public interface IDecisionEngine
{
    Task<bool> ShouldReject(IMailbox from, IMailbox to, CancellationToken cancellationToken);
    Task<Decision> DetermineDecision(IMailbox envelopeFrom, IMailbox to, InternetAddressList? messageFrom, CancellationToken cancellationToken);
}