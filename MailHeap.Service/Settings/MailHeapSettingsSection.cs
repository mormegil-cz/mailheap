namespace MailHeap.Service.Settings;

public class MailHeapSettingsSection
{
    public string? ServerName { get; set; }
    public string? RecipientDomains { get; set; }
    public string? RuleFile { get; set; }
    public string? ForwardingFromAddress { get; set; }
    public string? ReplyToAddress { get; set; }
    public string? SmtpServerHost { get; set; }
    public int? SmtpServerPort { get; set; }
    public bool SmtpServerSsl { get; set; }
    public int Port { get; set; } = 25;
    public bool SecurePort { get; set; }
    public string? CertificateFile { get; set; }
    public string? CertificatePassword { get; set; }
    public bool EnableProxy { get; set; }
    public int MaxMessageSize { get; set; } = 2 * (1 << 20); // 2 MiB default
    public int CommandWaitTimeout { get; set; } = 10000; // 10 seconds default
    public string? SmtpServerUsername { get; set; }
    public string? SmtpServerPassword { get; set; }
}