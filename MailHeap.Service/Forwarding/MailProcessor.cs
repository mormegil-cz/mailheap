using System.Data;
using System.Globalization;
using LinqToDB;
using MailHeap.Service.Persistence;
using MailHeap.Service.Persistence.Model;
using MailHeap.Service.Settings;
using MimeKit;
using MimeKit.Text;
using MimeKit.Utils;

namespace MailHeap.Service.Forwarding;

public class MailProcessor(
    ILogger<MailProcessor> logger,
    MailHeapSettings settings,
    DbConnectionFactory dbConnectionFactory,
    IMailSender mailSender
) : IMailProcessor
{
    private static readonly SemaphoreSlim semaphore = new(0);

    public void WakeUp() => semaphore.Release();

    public Task<bool> Wait(int millisTimeout, CancellationToken cancellationToken) => semaphore.WaitAsync(millisTimeout, cancellationToken);

    public async Task<bool> TryProcessMessage(CancellationToken cancellationToken)
    {
        await using var db = dbConnectionFactory.Create();
        await using var tx = await db.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var pendingMessage = await db.EmailMessages
            .Where(m => m.State == MessageState.ToForwardAndDelete || m.State == MessageState.ToForwardAndKeep)
            .OrderBy(m => m.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);
        if (pendingMessage == null)
        {
            await tx.CommitAsync(cancellationToken);
            logger.LogDebug("No pending messages");
            return false;
        }

        logger.LogDebug("Found message #{id}", pendingMessage.Id);
        await ProcessMessage(db, pendingMessage, cancellationToken);

        await tx.CommitAsync(cancellationToken);

        return true;
    }

    private async Task ProcessMessage(DbConnection db, EmailMessage emailMessage, CancellationToken cancellationToken)
    {
        var messageState = emailMessage.State;
        if (messageState != MessageState.ToForwardAndDelete && messageState != MessageState.ToForwardAndKeep)
        {
            // we should not be here, in that case!
            logger.LogWarning("Unexpected message state {state}", messageState);
            return;
        }

        try
        {
            await ForwardMessage(emailMessage, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to forward message #{id}", emailMessage.Id);

            // TODO: Schedule redelivery
            await SwitchMessageState(db, emailMessage, messageState switch
            {
                MessageState.ToForwardAndDelete => MessageState.FailedForwardToDelete,
                MessageState.ToForwardAndKeep => MessageState.FailedForwardToKeep,
                _ => throw new InvalidOperationException("Unexpected state")
            }, cancellationToken);

            return;
        }

        switch (messageState)
        {
            case MessageState.ToForwardAndDelete:
                await DeleteMessage(db, emailMessage, cancellationToken);
                break;

            case MessageState.ToForwardAndKeep:
                await SwitchMessageState(db, emailMessage, MessageState.Kept, cancellationToken);
                break;

            default:
                throw new InvalidOperationException("Unexpected state");
        }
    }

    private async Task SwitchMessageState(DbConnection db, EmailMessage emailMessage, MessageState newState, CancellationToken cancellationToken)
    {
        var oldState = emailMessage.State;
        emailMessage.State = newState;
        var updateCount = await db.EmailMessages
            .Where(m => m.Id == emailMessage.Id && m.State == oldState)
            .Set(m => m.State, newState)
            .UpdateAsync(cancellationToken);
        if (updateCount == 1)
        {
            logger.LogInformation("Switched message #{id} from {oldState} to {newState}", emailMessage.Id, oldState, newState);
        }
        else
        {
            // D’oh! what now? Maybe throw an exception instead??
            logger.LogError("Unexpected row count when switching message #{id} from {oldState} to {newState}: {count}", emailMessage.Id, oldState, newState, updateCount);
        }
    }

    private Task ForwardMessage(EmailMessage emailMessage, CancellationToken cancellationToken)
    {
        var forwardedMessage = new MimeMessage();
        var fromAddress = emailMessage.From ?? emailMessage.EnvelopeFrom;
        var forwardTo = emailMessage.ForwardTo;
        if (forwardTo == null) throw new InvalidOperationException("Missing forwarding address");

        //forwardedMessage.From.Add(new MailboxAddress(null, fromAddress));
        forwardedMessage.From.Add(new MailboxAddress(String.Format(UiTexts.Culture, UiTexts.ForwardingFromNameFormat, fromAddress), settings.ForwardingFromAddress));
        // forwardedMessage.ResentFrom.Add(new MailboxAddress(String.Format(UiTexts.Culture, UiTexts.ForwardingFromNameFormat, fromAddress), settings.ForwardingFromAddress));
        forwardedMessage.ResentTo.Add(new MailboxAddress(null, forwardTo));
        forwardedMessage.To.Add(new MailboxAddress(null, emailMessage.EnvelopeTo));
        forwardedMessage.Subject = emailMessage.Subject == null ? UiTexts.DefaultForwardedSubject : UiTexts.ForwardedSubjectPrefix + emailMessage.Subject;
        forwardedMessage.ResentReplyTo.Add(new MailboxAddress(UiTexts.ReplyToName, settings.ReplyToAddress));
        // OK, so this leaks the number of processed messages; but only to the recipients, so probably OK-ish
        forwardedMessage.MessageId = emailMessage.Id + "." + MimeUtils.GenerateMessageId(settings.ServerName);
        // forwardedMessage.ResentMessageId = emailMessage.Id + "." + MimeUtils.GenerateMessageId(settings.ServerName);
        var body = new TextPart(TextFormat.Plain)
        {
            Text = String.Format(UiTexts.Culture, UiTexts.ForwardedMessageBodyFormat, settings.ServerName, emailMessage.Timestamp, emailMessage.EnvelopeTo, emailMessage.EnvelopeFrom, emailMessage.From, emailMessage.SourceIpAddr)
        };
        var attachedMessage = new MimePart("message", "rfc822")
        {
            Content = new MimeContent(new MemoryStream(emailMessage.Message)),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment) { CreationDate = emailMessage.Timestamp.Value },
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName = UiTexts.ForwardedMessageFileName
        };
        var multipart = new Multipart("mixed") { body, attachedMessage };
        forwardedMessage.Body = multipart;

        logger.LogInformation("Forwarding message #{id}", emailMessage.Id);
        return mailSender.SendMail(forwardedMessage, cancellationToken);
    }

    private async Task DeleteMessage(DbConnection db, EmailMessage emailMessage, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting message #{id}", emailMessage.Id);
        await db.EmailMessages
            .Where(m => m.Id == emailMessage.Id)
            .DeleteAsync(cancellationToken);
    }
}

internal static class UiTexts
{
    internal static readonly IFormatProvider Culture = CultureInfo.GetCultureInfo("en-US");

    internal const string ForwardingFromNameFormat = "{0} [via MailHeap]";

    internal const string ForwardedSubjectPrefix = "Fwd: ";
    internal const string DefaultForwardedSubject = "Forwarded message from MailHeap";

    internal const string ReplyToName = "Do Not Reply";

    internal const string ForwardedMessageBodyFormat = """
                                                       This is a forwarded message from MailHeap at {0}.

                                                       At {1:u}, MailHeap has received a message for {2}
                                                       from {3} (signed as {4}, received from {5}).
                                                       The received message is attached.
                                                       """;

    internal const string ForwardedMessageFileName = "message.msg";
}