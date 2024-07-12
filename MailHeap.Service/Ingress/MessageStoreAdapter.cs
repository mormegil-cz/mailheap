using System.Buffers;
using System.Net;
using System.Text.Json;
using MailHeap.Service.Forwarding;
using MailHeap.Service.Helpers;
using MailHeap.Service.Persistence;
using MailHeap.Service.Persistence.Model;
using MailHeap.Service.Rules;
using MailHeap.Service.Settings;
using MimeKit;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace MailHeap.Service.Ingress;

internal class MessageStoreAdapter(
    ILogger<MessageStoreAdapter> logger,
    MailHeapSettings settings,
    IDecisionEngine decisionEngine,
    IMailStorage storage,
    IMailProcessor mailProcessor
) : IMessageStore
{
    public async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        logger.LogDebug("Processing message");
        try
        {
            var remoteEndpoint = GetRemoteEndpoint(context);
            var messageBytes = buffer.ToArray();
            var parsedMessage = await ParseMessageBytes(messageBytes, cancellationToken);

            foreach (var recipient in transaction.To)
            {
                var decision = await decisionEngine.DetermineDecision(transaction.From, recipient, parsedMessage?.From, cancellationToken);
                if (decision.Decision is Decision.Drop or Decision.Reject)
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Dropping e-mail from {from} to {to}", transaction.From.MailboxToString(), recipient.MailboxToString());
                    }
                    continue;
                }

                var emailMessage = new EmailMessage
                {
                    Timestamp = new Timestamp(DateTime.UtcNow),
                    State = InitialStateForDecision(decision.Decision),
                    EnvelopeFrom = transaction.From.MailboxToString(),
                    EnvelopeTo = recipient.MailboxToString(),
                    From = parsedMessage?.From.OfType<MailboxAddress>().Select(f => f.Address).FirstOrDefault(),
                    Subject = parsedMessage?.Subject,
                    SourceIpAddr = remoteEndpoint?.Address.ToString(),
                    SourcePort = remoteEndpoint?.Port,
                    Secured = context.Pipe.IsSecure,
                    HelloName = context.Properties.TryGetValue(SmtpServerHost.HelloDomainOrAddress, out var helloName) ? helloName as string : null,
                    Message = messageBytes,
                    Parameters = SerializeParameters(transaction.Parameters),
                    ForwardTo = decision is ForwardingEffect fe ? fe.ForwardToAddress : null
                };
                await storage.SaveMail(emailMessage, cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to store message");
            return new SmtpResponse(SmtpReplyCode.Aborted, "Unable to store message");
        }

        mailProcessor.WakeUp();

        return SmtpResponse.Ok;
    }

    private static MessageState InitialStateForDecision(Decision decision) => decision switch
    {
        Decision.ForwardAndDelete => MessageState.ToForwardAndDelete,
        Decision.ForwardAndKeep => MessageState.ToForwardAndKeep,
        Decision.Keep => MessageState.Kept,
        _ => throw new ArgumentOutOfRangeException(nameof(decision), decision, "Unexpected decision value")
    };

    private static string? SerializeParameters(IReadOnlyDictionary<string, string> transactionParameters) => transactionParameters.Count == 0 ? null : JsonSerializer.Serialize(transactionParameters);

    private async Task<MimeMessage?> ParseMessageBytes(byte[] messageBytes, CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream(messageBytes);
        try
        {
            return await MimeMessage.LoadAsync(stream, cancellationToken);
        }
        catch (FormatException e)
        {
            logger.LogError(e, "Error parsing received message");
            return null;
        }
    }

    private IPEndPoint? GetRemoteEndpoint(ISessionContext context) => SmtpHelpers.GetRemoteEndpoint(context, logger, settings);
}