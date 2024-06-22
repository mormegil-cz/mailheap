using MimeKit;
using SmtpServer.Mail;

namespace MailHeap.Service.Rules;

public interface IRule
{
    bool Matches(IMailbox from, IMailbox to);
    bool Matches(IMailbox envelopeFrom, IMailbox to, InternetAddressList? messageFrom);
    string Id { get; }
    bool ShouldReject { get; }
    bool ShouldDrop { get; }
}