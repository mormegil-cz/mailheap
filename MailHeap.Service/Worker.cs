using MailHeap.Service.Ingress;

namespace MailHeap.Service;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var smtpServerTask = SmtpServerHost.RunServer(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            if (smtpServerTask.IsCompleted)
            {
                logger.LogError("SMTP server stopped");
                break;
            }
            await Task.Delay(1000, stoppingToken);
        }

        await smtpServerTask;
    }
}