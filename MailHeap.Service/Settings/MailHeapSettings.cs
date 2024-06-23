using Microsoft.Extensions.Options;

namespace MailHeap.Service.Settings;

public class MailHeapSettings(
    IOptions<MailHeapSettingsSection> sectionOptions
)
{
    public string ServerName { get; } = GetRequiredValue(nameof(ServerName), sectionOptions.Value.ServerName);
    public IReadOnlySet<string> RecipientDomains { get; } = new HashSet<string>(GetRequiredValue(nameof(RecipientDomains), sectionOptions.Value.RecipientDomains).Split(',').Select(s => s.ToUpperInvariant()));
    public string RuleFile { get; } = GetRequiredValue(nameof(RuleFile), sectionOptions.Value.RuleFile);
    public string ForwardingFromAddress { get; } = GetRequiredValue(nameof(ForwardingFromAddress), sectionOptions.Value.ForwardingFromAddress);
    public string ReplyToAddress { get; } = GetRequiredValue(nameof(ReplyToAddress), sectionOptions.Value.ReplyToAddress);
    public string SmtpServerHost { get; } = GetRequiredValue(nameof(SmtpServerHost), sectionOptions.Value.SmtpServerHost);
    public int SmtpServerPort { get; } = GetRequiredValue(nameof(SmtpServerPort), sectionOptions.Value.SmtpServerPort);
    public bool SmtpServerSsl { get; } = sectionOptions.Value.SmtpServerSsl;
    public int Port { get; } = sectionOptions.Value.Port;
    public bool SecurePort { get; } = sectionOptions.Value.SecurePort;
    public string? CertificateFile { get; } = sectionOptions.Value.CertificateFile;
    public string? CertificatePassword { get; } = sectionOptions.Value.CertificatePassword;
    public bool EnableProxy { get; } = sectionOptions.Value.EnableProxy;
    public int MaxMessageSize { get; } = sectionOptions.Value.MaxMessageSize;
    public int CommandWaitTimeout { get; } = sectionOptions.Value.CommandWaitTimeout;
    public string? SmtpServerUsername { get; } = sectionOptions.Value.SmtpServerUsername;
    public string? SmtpServerPassword { get; } = sectionOptions.Value.SmtpServerPassword;

    private static T GetRequiredValue<T>(string key, T? value) where T : struct => value ?? throw new InvalidOperationException(key + " configuration is required");
    private static T GetRequiredValue<T>(string key, T? value) where T : class => value ?? throw new InvalidOperationException(key + " configuration is required");
    private static int GetPositiveValue(string key, int value) => value > 0 ? value : throw new InvalidOperationException(key + " configuration is required");
}