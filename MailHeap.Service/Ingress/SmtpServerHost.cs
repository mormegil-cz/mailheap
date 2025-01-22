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
    IMailboxFilter mailboxFilterAdapter,
    IOptionalCertificateFactory certificateFactory
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

        if (certificateFactory.IsConfigured)
        {
            builder.Certificate(certificateFactory);
        }
    }

    private void SmtpServerOnSessionCreated(object? sender, SessionEventArgs e)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("New SMTP session from {Endpoint}", SmtpHelpers.GetRemoteEndpoint(e.Context, logger, settings));
        }
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
}