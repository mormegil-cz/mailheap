using MailHeap.Service.Ingress;

namespace MailHeap.Service;

internal class Worker(
    ILogger<Worker> logger,
    SmtpServerHost smtpServerHost
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var smtpServerTask = smtpServerHost.RunServer(stoppingToken);

        logger.LogInformation("Worker starting");
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogDebug("Worker running");
            if (smtpServerTask.IsCompleted && !stoppingToken.IsCancellationRequested)
            {
                logger.Log(LogLevel.Critical, "SMTP server stopped unexpectedly");
                break;
            }
            await Task.Delay(60000, stoppingToken);
        }
        logger.LogInformation("Worker finishing");

        await smtpServerTask;
    }
}