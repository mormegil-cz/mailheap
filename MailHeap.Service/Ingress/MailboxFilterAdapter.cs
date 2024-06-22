using MailHeap.Service.Helpers;
using MailHeap.Service.Rules;
using MailHeap.Service.Settings;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace MailHeap.Service.Ingress;

internal class MailboxFilterAdapter(
    ILogger<MailboxFilterAdapter> logger,
    MailHeapSettings settings,
    IDecisionEngine decisionEngine
) : IMailboxFilter
{
    public Task<bool> CanAcceptFromAsync(ISessionContext context, IMailbox from, int size, CancellationToken cancellationToken) => Task.FromResult(true);

    public async Task<bool> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox from, CancellationToken cancellationToken)
    {
        if (!settings.RecipientDomains.Contains(to.Host.ToUpperInvariant()))
        {
            if (logger.IsEnabled(LogLevel.Warning)) logger.LogWarning("Rejected forwarding e-mail from {from} to {to}", from.MailboxToString(), to.MailboxToString());
            return false;
        }

        var reject = await decisionEngine.ShouldReject(from, to, cancellationToken);
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("{decision} e-mail from {from} to {to}", reject ? "Rejected" : "Accepted", from.MailboxToString(), to.MailboxToString());
        }
        return !reject;
    }
}