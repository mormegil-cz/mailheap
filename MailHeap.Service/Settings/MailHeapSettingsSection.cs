using System.Diagnostics.CodeAnalysis;

namespace MailHeap.Service.Settings;

[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class MailHeapSettingsSection
{
    /**
     * Server hostname
     * <example>mx.mailheap.example.com</example>
     */
    public string? ServerName { get; set; }

    /**
     * Human-readable/display name for the server
     * <example>mailheap.example.com</example>
     */
    public string? ServerDisplayName { get; set; }

    /**
     * List of comma-separated domain names for which this server accepts mail
     * <example>mailheap.example.com,mails.example.com</example>
     */
    public string? RecipientDomains { get; set; }

    /**
     * Name of the file containing the rule definition
     * <example>/etc/mailheap/rules.json</example>
     */
    public string? RuleFile { get; set; }

    /**
     * Address used as From: in the forwarded e-mail messages
     * <example>noreply@mailheap.example.com</example>
     */
    public string? ForwardingFromAddress { get; set; }

    /**
     * Address used as Reply-To in the forwarded e-mail messages
     * <example>noreply@mailheap.example.com</example>
     */
    public string? ReplyToAddress { get; set; }

    /**
     * SMTP server hostname used to forward e-mails
     * <example>smtp.company.example</example>
     */
    public string? SmtpServerHost { get; set; }

    /**
     * SMTP server port number used to forward e-mails
     * <example>587</example>
     */
    public int? SmtpServerPort { get; set; }

    /**
     * Does the outgoing SMTP server use TLS?
     */
    public bool SmtpServerSsl { get; set; }

    /**
     * Port where this program listens for incoming SMTP
     */
    public int Port { get; set; } = 25;

    /**
     * Is the incoming SMTP port secure by default?
     */
    public bool SecurePort { get; set; }

    /**
     * Name of the file containing the TLS certificate for incoming mail; optional. The file can be either a PKCS-12
     * file (.pfx), with the decryption password specified in <see cref="CertificatePassword"/>, or it can be
     * a PEM file, with the private key stored in another file defined in <see cref="CertificateKeyFile"/>.
     */
    public string? CertificateFile { get; set; }

    /**
     * Name of the file containing the private key for the TLS certificate for incoming mail; optional. If used,
     * the corresponding certificate must be configured in <see cref="CertificateFile"/>, and both must be PEM-encoded.
     */
    public string? CertificateKeyFile { get; set; }

    /**
     * Password for the certificate file if stored in PFX format, and configured in <see cref="CertificateFile"/>.
     */
    public string? CertificatePassword { get; set; }

    /**
     * Enable HAProxy support; configure if MailHeap is behind HAProxy to receive the client’s IP address (instead of HAProxy’s).
     */
    public bool EnableProxy { get; set; }

    /**
     * Maximum allowed message size in bytes. (Default is 2 MiB.)
     */
    public int MaxMessageSize { get; set; } = 2 * (1 << 20); // 2 MiB default

    /**
     * Command wait timeout in milliseconds. (Default is 10 s.)
     */
    public int CommandWaitTimeout { get; set; } = 10000; // 10 seconds default

    /**
     * Username for the SMTP server used to forward e-mails
     * <seealso cref="SmtpServerHost"/>
     */
    public string? SmtpServerUsername { get; set; }

    /**
     * Password for the SMTP server used to forward e-mails
     * <seealso cref="SmtpServerHost"/>
     */
    public string? SmtpServerPassword { get; set; }
}