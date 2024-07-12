using System.Collections.ObjectModel;
using MailHeap.Service.Settings;

namespace MailHeap.Service.Rules;

public class StaticRuleCollection(
    MailHeapSettings settings
) : IRuleCollection
{
    private readonly ReadOnlyCollection<IRule> rules = IRuleCollection.LoadSettings(settings.RuleFile);

    public IEnumerable<IRule> GetRules() => rules;
}