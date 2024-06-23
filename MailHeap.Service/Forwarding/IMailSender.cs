using MimeKit;

namespace MailHeap.Service.Forwarding;

public interface IMailSender
{
    Task SendMail(MimeMessage message, CancellationToken cancellationToken);
}