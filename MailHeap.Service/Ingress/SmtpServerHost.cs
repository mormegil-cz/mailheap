using System.Security.Cryptography.X509Certificates;
using MailHeap.Service.Settings;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using ServiceProvider = SmtpServer.ComponentModel.ServiceProvider;

namespace MailHeap.Service.Ingress;

internal class SmtpServerHost(
    ILogger<SmtpServerHost> logger,
    MailHeapSettings settings,
    IMessageStore messageStoreAdapter,
    IMailboxFilter mailboxFilterAdapter
)
{
    internal const string HelloDomainOrAddress = "SmtpServerHost:HelloDomainOrAddress";

    internal Task RunServer(CancellationToken cancellationToken)
    {
        var optionsBuilder = new SmtpServerOptionsBuilder()
            .ServerName(settings.ServerName)
            .Endpoint(ConfigureEndpoint)
            .MaxMessageSize(settings.MaxMessageSize)
            .CommandWaitTimeout(TimeSpan.FromMilliseconds(settings.CommandWaitTimeout));

        var serviceProvider = new ServiceProvider();
        serviceProvider.Add(messageStoreAdapter);
        serviceProvider.Add(mailboxFilterAdapter);

        var smtpServer = new SmtpServer.SmtpServer(optionsBuilder.Build(), serviceProvider);
        smtpServer.SessionCreated += SmtpServerOnSessionCreated;
        logger.LogInformation("Starting SMTP server {serverName}", settings.ServerName);
        return smtpServer.StartAsync(cancellationToken);
    }

    private void ConfigureEndpoint(EndpointDefinitionBuilder builder)
    {
        builder.Port(settings.Port, settings.SecurePort);
        var certificateFilename = settings.CertificateFile;
        if (certificateFilename != null)
        {
            builder.Certificate(LoadCertificate(certificateFilename, settings.CertificatePassword ?? throw new InvalidOperationException("Certificate password is required when certificate is configured")));
        }
    }

    private void SmtpServerOnSessionCreated(object? sender, SessionEventArgs e)
    {
        e.Context.CommandExecuting += ContextOnCommandExecuting;
    }

    private void ContextOnCommandExecuting(object? sender, SmtpCommandEventArgs e)
    {
        logger.LogDebug("Executing command {command}", e.Command.Name);
        switch (e.Command)
        {
            case HeloCommand heloCommand:
                e.Context.Properties[HelloDomainOrAddress] = heloCommand.DomainOrAddress;
                break;

            case EhloCommand ehloCommand:
                e.Context.Properties[HelloDomainOrAddress] = ehloCommand.DomainOrAddress;
                break;
        }
    }

    private static X509Certificate2 LoadCertificate(string filename, string password) => new(File.ReadAllBytes(filename), password);
}