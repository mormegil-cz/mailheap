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

            while (await mailProcessor.TryProcessMessage(stoppingToken))
            {
                logger.LogDebug("Message processed");
            }

            await mailProcessor.Wait(20000, stoppingToken);
        }
    }
}