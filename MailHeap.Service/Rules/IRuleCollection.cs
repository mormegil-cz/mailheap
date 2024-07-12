using System.Collections.ObjectModel;
using System.Text.Json.Nodes;

namespace MailHeap.Service.Rules;

public interface IRuleCollection
{
    IEnumerable<IRule> GetRules();

    protected static ReadOnlyCollection<IRule> LoadSettings(string settingsRuleFile) =>
        (JsonNode.Parse(File.ReadAllBytes(settingsRuleFile)) ?? throw new FormatException("Invalid rule JSON format")).AsArray()
        .Select(entry => entry?.AsObject() ?? throw new FormatException("Invalid rule JSON format"))
        .Select((ruleJson, index) => SimpleRule.ParseFromJson(ruleJson, $"Rule #{index + 1}"))
        .Cast<IRule>()
        .ToList().AsReadOnly();
}