namespace MailHeap.Service.Forwarding;

public interface IMailProcessor
{
    public void WakeUp();
    public Task<bool> Wait(int millisTimeout, CancellationToken cancellationToken);
    public Task<bool> TryProcessMessage(CancellationToken cancellationToken);
}