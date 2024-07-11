namespace MailHeap.Service.Forwarding;

public class MailProcessingWorker(
    ILogger<MailProcessingWorker> logger,
    IMailProcessor mailProcessor
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Mail processing worker starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogDebug("Mail processing worker running");

            while (await mailProcessor.TryProcessMessage(stoppingToken))
            {
                logger.LogDebug("Message processed");
            }

            await mailProcessor.Wait(20000, stoppingToken);
        }

        logger.LogInformation("Mail processing worker finishing");
    }
}