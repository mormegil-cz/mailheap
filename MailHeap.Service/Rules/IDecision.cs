using System.Buffers;
using SmtpServer;

namespace MailHeap.Service.Rules;

public interface IDecision
{
    public Task Execute(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> messageBuffer, CancellationToken cancellationToken);
}