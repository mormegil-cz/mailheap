using SmtpServer;

namespace MailHeap.Service.Ingress;

public interface IOptionalCertificateFactory : ICertificateFactory
{
    bool IsConfigured { get; }
}