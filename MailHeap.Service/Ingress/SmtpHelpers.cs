using System.Net;
using MailHeap.Service.Settings;
using SmtpServer;
using SmtpServer.Net;
using SmtpServer.Protocol;

namespace MailHeap.Service.Ingress;

public static class SmtpHelpers
{
    public static IPEndPoint? GetRemoteEndpoint(
        ISessionContext context,
        ILogger logger,
        MailHeapSettings settings
    )
    {
        if (settings.EnableProxy)
        {
            var proxyEndpoint = context.Properties.TryGetValue(ProxyCommand.ProxySourceEndpointKey, out var proxyEp) ? proxyEp as IPEndPoint : null;
            if (proxyEndpoint != null) return proxyEndpoint;
        }

        var endpoint = context.Properties.TryGetValue(EndpointListener.RemoteEndPointKey, out var ep) ? ep as IPEndPoint : null;
        if (endpoint != null) return endpoint;

        logger.LogError("Remote endpoint not available");
        return null;
    }
}