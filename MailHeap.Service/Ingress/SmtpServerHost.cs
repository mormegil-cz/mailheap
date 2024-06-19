using System.Security.Cryptography.X509Certificates;
using SmtpServer;
using ServiceProvider = SmtpServer.ComponentModel.ServiceProvider;

namespace MailHeap.Service.Ingress;

internal static class SmtpServerHost
{
    internal static Task RunServer(CancellationToken cancellationToken)
    {
        var options = new SmtpServerOptionsBuilder()
            .ServerName("localhost")
            .Endpoint(builder =>
                builder
                    .Port(9025, true)
                    .AllowUnsecureAuthentication(false)
                    .Certificate(CreateCertificate()))
            .Build();

        var serviceProvider = new ServiceProvider();
        serviceProvider.Add(new SampleMessageStore());
        serviceProvider.Add(new SampleMailboxFilter());
        serviceProvider.Add(new SampleUserAuthenticator());

        var smtpServer = new SmtpServer.SmtpServer(options, serviceProvider);
        return smtpServer.StartAsync(cancellationToken);
    }

    private static X509Certificate2 CreateCertificate()
    {
        var certificate = File.ReadAllBytes(@"Certificate.pfx");

        return new X509Certificate2(certificate, "P@ssw0rd");
    }
}