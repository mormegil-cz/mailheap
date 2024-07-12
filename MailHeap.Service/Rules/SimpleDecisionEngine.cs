using MailHeap.Service.Helpers;
using MimeKit;
using SmtpServer.Mail;

namespace MailHeap.Service.Rules;

public class SimpleDecisionEngine(
    ILogger<SimpleDecisionEngine> logger,
    IRuleCollection ruleCollection
)
    : IDecisionEngine
{
    private static readonly RuleEffect keepDecision = new SimpleEffect(Decision.Keep);

    public Task<bool> ShouldReject(IMailbox from, IMailbox to, CancellationToken cancellationToken)
    {
        var matchingRule = ruleCollection.GetRules().FirstOrDefault(rule => rule.Matches(from, to));
        if (matchingRule == null)
        {
            if (logger.IsEnabled(LogLevel.Debug)) logger.LogDebug("No matching rule for rejection test for e-mail from {from} to {to}", from.MailboxToString(), to.MailboxToString());
            return Task.FromResult(false);
        }

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Found rule {rule} for rejection of e-mail from {from} to {to}", matchingRule.Id, from.MailboxToString(), to.MailboxToString());
        }
        return Task.FromResult(matchingRule.Effect.Decision == Decision.Reject);
    }

    public Task<RuleEffect> DetermineDecision(IMailbox envelopeFrom, IMailbox to, InternetAddressList? messageFrom, CancellationToken cancellationToken)
    {
        var matchingRule = ruleCollection.GetRules().FirstOrDefault(rule => rule.Matches(envelopeFrom, to, messageFrom));
        if (matchingRule == null)
        {
            if (logger.IsEnabled(LogLevel.Warning)) logger.LogWarning("No matching rule for e-mail from {from} to {to}", envelopeFrom.MailboxToString(), to.MailboxToString());
            return Task.FromResult(keepDecision);
        }

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace("Found rule {rule} for dropping e-mail from {envelopeFrom} ({fullFrom}) to {to}", matchingRule.Id, envelopeFrom.MailboxToString(), String.Join(", ", messageFrom?.Select(m => m.ToString()) ?? Array.Empty<string>()), to.MailboxToString());
        }
        return Task.FromResult(matchingRule.Effect);
    }
}