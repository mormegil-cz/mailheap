namespace MailHeap.Service.Forwarding;

public class MailProcessingWorker(
    ILogger<MailProcessingWorker> logger,
    IMailProcessor mailProcessor
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogDebug("Mail processing worker running at: {time}", DateTimeOffset.Now);
            await mailProcessor.Wait(20000, stoppingToken);
            if (stoppingToken.IsCancellationRequested) break;

            while (await mailProcessor.TryProcessMessage(stoppingToken))
            {
                logger.LogDebug("Message processed");
            }
        }
    }
}