using MailHeap.Service.Settings;
using MailKit.Net.Smtp;
using MimeKit;

namespace MailHeap.Service.Forwarding;

public class SmtpEmailSender(
    ILogger<SmtpEmailSender> logger,
    MailHeapSettings settings
) : IMailSender
{
    public async Task SendMail(MimeMessage message, CancellationToken cancellationToken)
    {
        using var client = new SmtpClient();
        client.LocalDomain = settings.ServerName;

        logger.LogDebug("Connecting to SMTP server {server}:{port}", settings.SmtpServerHost, settings.SmtpServerPort);
        await client.ConnectAsync(settings.SmtpServerHost, settings.SmtpServerPort, settings.SmtpServerSsl, cancellationToken);
        var userName = settings.SmtpServerUsername;
        if (userName != null)
        {
            await client.AuthenticateAsync(userName, settings.SmtpServerPassword ?? throw new InvalidOperationException("SMTP server password required when using username"), cancellationToken);
        }

        logger.LogDebug("Sending message to SMTP server");
        await client.SendAsync(message, cancellationToken);

        logger.LogDebug("Disconnecting from SMTP server");
        await client.DisconnectAsync(true, cancellationToken);
    }
}