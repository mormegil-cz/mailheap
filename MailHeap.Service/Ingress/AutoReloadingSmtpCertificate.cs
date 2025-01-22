using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using MailHeap.Service.Settings;
using SmtpServer;

namespace MailHeap.Service.Ingress;

public class AutoReloadingSmtpCertificate : IOptionalCertificateFactory, IDisposable
{
    private readonly Lock reloadSync = new();
    private readonly ILogger<AutoReloadingSmtpCertificate> logger;
    private readonly FileSystemWatcher? certWatcher;
    private readonly FileSystemWatcher? keyWatcher;
    private readonly string? certFilename;
    private readonly string? keyFilename;
    private readonly string? certPassword;

    private volatile X509Certificate2? certificate;
    private bool reloadPending;

    public AutoReloadingSmtpCertificate(ILogger<AutoReloadingSmtpCertificate> logger, MailHeapSettings settings)
    {
        this.logger = logger;

        certFilename = settings.CertificateFile;
        keyFilename = settings.CertificateKeyFile;
        certPassword = settings.CertificatePassword;

        if (certFilename == null)
        {
            certificate = null;
            certWatcher = null;
            keyWatcher = null;
            logger.LogInformation("No certificate configured");
            return;
        }

        certWatcher = CreateWatcher(certFilename);
        if (keyFilename != null)
        {
            keyWatcher = CreateWatcher(keyFilename);
        }
        else if (certPassword == null)
        {
            throw new ArgumentException("Certificate password is required when single certificate file is specified");
        }

        certificate = LoadCertificate(certFilename, keyFilename, certPassword);

        logger.LogInformation("Certificate loaded from {certFilename}", certFilename);
    }

    private FileSystemWatcher CreateWatcher(string filename)
    {
        var result = new FileSystemWatcher(Path.GetDirectoryName(Path.GetFullPath(filename)) ?? throw new ArgumentException("Invalid rule filename"), Path.GetFileName(filename));
        result.Changed += WatcherOnChanged;
        result.Error += WatcherOnError;
        result.NotifyFilter = NotifyFilters.LastWrite;
        result.EnableRaisingEvents = true;
        return result;
    }

    public X509Certificate GetServerCertificate(ISessionContext sessionContext) => certificate ?? throw new InvalidOperationException("No certificate configured");

    public bool IsConfigured => certFilename != null;

    private void WatcherOnChanged(object _, FileSystemEventArgs e)
    {
        logger.LogDebug("Certificate file changed");
        lock (reloadSync)
        {
            if (reloadPending)
            {
                logger.LogDebug("Certificate reload already pending");
            }
            else
            {
                reloadPending = true;
                ThreadPool.QueueUserWorkItem(ReloadCertificateAfterDelay, 500, false);
                logger.LogDebug("Queued certificate reload");
            }
        }
    }

    private void WatcherOnError(object _, ErrorEventArgs e) => logger.LogError(e.GetException(), "Error watching certificate file");

    private void ReloadCertificateAfterDelay(int delayMs)
    {
        Debug.Assert(certFilename != null);

        Thread.Sleep(delayMs);
        lock (reloadSync)
        {
            if (!reloadPending)
            {
                // wat
                logger.LogError("Reloading certificate without reload pending!");
            }
            reloadPending = false;
            logger.LogDebug("Reloading certificate");
            try
            {
                certificate = LoadCertificate(certFilename, keyFilename, certPassword);
                logger.LogInformation("Certificate reloaded");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error reloading certificate");
            }
        }
    }

    private static X509Certificate2 LoadCertificate(string certFilename, string? keyFilename, string? certPassword) =>
        keyFilename == null
            ? X509CertificateLoader.LoadPkcs12(File.ReadAllBytes(certFilename), certPassword)
            : X509Certificate2.CreateFromPemFile(certFilename, keyFilename);

    public void Dispose()
    {
        certWatcher?.Dispose();
        keyWatcher?.Dispose();
        GC.SuppressFinalize(this);
    }
}