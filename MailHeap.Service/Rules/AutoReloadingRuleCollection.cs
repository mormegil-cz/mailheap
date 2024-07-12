using System.Collections.ObjectModel;
using MailHeap.Service.Settings;

namespace MailHeap.Service.Rules;

public class AutoReloadingRuleCollection : IRuleCollection, IDisposable
{
    private readonly object reloadSync = new();
    private readonly ILogger<AutoReloadingRuleCollection> logger;
    private readonly FileSystemWatcher watcher;
    private readonly string ruleFilename;

    private volatile ReadOnlyCollection<IRule> rules;
    private bool reloadPending;

    public AutoReloadingRuleCollection(ILogger<AutoReloadingRuleCollection> logger, MailHeapSettings settings)
    {
        this.logger = logger;
        ruleFilename = settings.RuleFile;
        watcher = new FileSystemWatcher(Path.GetDirectoryName(Path.GetFullPath(ruleFilename)) ?? throw new ArgumentException("Invalid rule filename"), Path.GetFileName(ruleFilename));
        watcher.Changed += WatcherOnChanged;
        watcher.Error += WatcherOnError;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.EnableRaisingEvents = true;
        rules = IRuleCollection.LoadSettings(ruleFilename);
        logger.LogInformation("Rules loaded from {RuleFile}", ruleFilename);
    }

    private void WatcherOnChanged(object _, FileSystemEventArgs e)
    {
        logger.LogDebug("Rule file changed");
        lock (reloadSync)
        {
            if (reloadPending)
            {
                logger.LogDebug("Reload already pending");
            }
            else
            {
                reloadPending = true;
                ThreadPool.QueueUserWorkItem(ReloadRulesAfterDelay, 500, false);
                logger.LogDebug("Queued rule reload");
            }
        }
    }

    private void WatcherOnError(object _, ErrorEventArgs e) => logger.LogError(e.GetException(), "Error watching rule file");

    private void ReloadRulesAfterDelay(int delayMs)
    {
        Thread.Sleep(delayMs);
        lock (reloadSync)
        {
            if (!reloadPending)
            {
                // wat
                logger.LogError("Reloading without reload pending!");
            }
            reloadPending = false;
            logger.LogDebug("Reloading rule file");
            try
            {
                rules = IRuleCollection.LoadSettings(ruleFilename);
                logger.LogInformation("Rule file reloaded");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error reloading rule file");
            }
        }
    }

    public IEnumerable<IRule> GetRules() => rules;

    public void Dispose()
    {
        watcher.Dispose();
        GC.SuppressFinalize(this);
    }
}