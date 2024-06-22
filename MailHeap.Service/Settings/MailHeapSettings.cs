using Microsoft.Extensions.Options;

namespace MailHeap.Service.Settings;

public class MailHeapSettings(
    IOptions<MailHeapSettingsSection> sectionOptions
)
{
    public string ServerName { get; } = GetRequiredValue(nameof(ServerName), sectionOptions.Value.ServerName);
    public IReadOnlySet<string> RecipientDomains { get; } = new HashSet<string>(GetRequiredValue(nameof(RecipientDomains), sectionOptions.Value.RecipientDomains).Split(',').Select(s => s.ToUpperInvariant()));
    public string RuleFile { get; } = GetRequiredValue(nameof(RuleFile), sectionOptions.Value.RuleFile);
    public int Port { get; } = sectionOptions.Value.Port;
    public bool SecurePort { get; } = sectionOptions.Value.SecurePort;
    public string? CertificateFile { get; } = sectionOptions.Value.CertificateFile;
    public string? CertificatePassword { get; } = sectionOptions.Value.CertificatePassword;
    public bool EnableProxy { get; } = sectionOptions.Value.EnableProxy;
    public int MaxMessageSize { get; } = sectionOptions.Value.MaxMessageSize;
    public int CommandWaitTimeout { get; } = sectionOptions.Value.CommandWaitTimeout;

    private static string GetRequiredValue(string key, string? value) => value ?? throw new InvalidOperationException(key + " configuration is required");
}