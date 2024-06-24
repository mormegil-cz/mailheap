using MimeKit;
using SmtpServer.Mail;

namespace MailHeap.Service.Rules;

public interface IRule
{
    bool Matches(IMailbox from, IMailbox to);
    bool Matches(IMailbox envelopeFrom, IMailbox to, InternetAddressList? messageFrom);
    string Id { get; }
    RuleEffect Effect { get; }
}

public abstract record RuleEffect(Decision Decision);

public record SimpleEffect(Decision Decision) : RuleEffect(Decision);

public record ForwardingEffect(Decision Decision, string ForwardToAddress) : RuleEffect(Decision);

public enum Decision
{
    Reject,
    Drop,
    Keep,
    ForwardAndDelete,
    ForwardAndKeep,
}