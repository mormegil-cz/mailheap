using SmtpServer.Mail;

namespace MailHeap.Service.Helpers;

public static class MailboxExtensions
{
    public static string MailboxToString(this IMailbox mailbox) => string.Concat(mailbox.User, "@", mailbox.Host);
}