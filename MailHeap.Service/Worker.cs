using MailHeap.Service.Ingress;
using MailHeap.Service.Persistence;

namespace MailHeap.Service;

internal class Worker(
    ILogger<Worker> logger,
    SmtpServerHost smtpServerHost
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var smtpServerTask = smtpServerHost.RunServer(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            if (smtpServerTask.IsCompleted && !stoppingToken.IsCancellationRequested)
            {
                logger.LogError("SMTP server stopped unexpectedly");
                break;
            }
            await Task.Delay(60000, stoppingToken);
        }

        await smtpServerTask;
    }
}