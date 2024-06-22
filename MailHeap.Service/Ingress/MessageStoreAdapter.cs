using System.Buffers;
using System.Net;
using System.Text.Json;
using MailHeap.Service.Helpers;
using MailHeap.Service.Persistence;
using MailHeap.Service.Persistence.Model;
using MailHeap.Service.Rules;
using MailHeap.Service.Settings;
using MimeKit;
using SmtpServer;
using SmtpServer.Net;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace MailHeap.Service.Ingress;

internal class MessageStoreAdapter(
    ILogger<MessageStoreAdapter> logger,
    MailHeapSettings settings,
    IDecisionEngine decisionEngine,
    IMailStorage storage
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
                if (await decisionEngine.ShouldDrop(transaction.From, recipient, parsedMessage?.From, cancellationToken))
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Dropping e-mail from {from} to {to}", transaction.From.MailboxToString(), recipient.MailboxToString());
                    }
                    continue;
                }

                var emailMessage = new EmailMessage
                {
                    Timestamp = DateTime.UtcNow,
                    State = MessageState.Stored,
                    EnvelopeFrom = transaction.From.MailboxToString(),
                    EnvelopeTo = recipient.MailboxToString(),
                    From = parsedMessage?.From.Select(f => f.ToString()).FirstOrDefault(),
                    Subject = parsedMessage?.Subject,
                    SourceIpAddr = remoteEndpoint?.Address.ToString(),
                    SourcePort = remoteEndpoint?.Port,
                    Secured = context.Pipe.IsSecure,
                    HelloName = context.Properties.TryGetValue(SmtpServerHost.HelloDomainOrAddress, out var helloName) ? helloName as string : null,
                    Message = messageBytes,
                    Parameters = SerializeParameters(transaction.Parameters),
                };
                await storage.SaveMail(emailMessage, cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to store message");
            return new SmtpResponse(SmtpReplyCode.Aborted, "Unable to store message");
        }

        // TODO: Wake up processing job

        return SmtpResponse.Ok;
    }

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

    private IPEndPoint? GetRemoteEndpoint(ISessionContext context)
    {
        if (settings.EnableProxy)
        {
            var proxyEndpoint = context.Properties.TryGetValue(ProxyCommand.ProxySourceEndpointKey, out var proxyEp) ? proxyEp as IPEndPoint : null;
            if (proxyEndpoint != null) return proxyEndpoint;
        }

        var endpoint = context.Properties.TryGetValue(EndpointListener.RemoteEndPointKey, out var ep) ? ep as IPEndPoint : null;
        if (endpoint != null) return endpoint;

        logger.LogError("Remote endpoint not available");
        return null;
    }
}