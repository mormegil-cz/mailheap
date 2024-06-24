using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using MailHeap.Service.Helpers;
using MimeKit;
using SmtpServer.Mail;

namespace MailHeap.Service.Rules;

public class SimpleRule(
    string id,
    RuleEffect effect,
    string? fromFilter,
    Regex? fromRegexFilter,
    string? toFilter,
    Regex? toRegexFilter,
    string? messageFromFilter,
    Regex? messageFromRegexFilter
) : IRule
{
    public bool Matches(IMailbox from, IMailbox to)
    {
        var fromStr = from.MailboxToString();
        var toStr = to.MailboxToString();

        if (fromFilter != null && !string.Equals(fromFilter, fromStr, StringComparison.InvariantCultureIgnoreCase)) return false;
        if (fromRegexFilter != null && !fromRegexFilter.IsMatch(fromStr)) return false;
        if (toFilter != null && !string.Equals(toFilter, toStr, StringComparison.InvariantCultureIgnoreCase)) return false;
        if (toRegexFilter != null && !toRegexFilter.IsMatch(toStr)) return false;

        return true;
    }

    public bool Matches(IMailbox envelopeFrom, IMailbox to, InternetAddressList? messageFrom)
    {
        if (!Matches(envelopeFrom, to)) return false;

        if (messageFrom == null) return messageFromFilter == null && messageFromRegexFilter == null;

        if (messageFromFilter != null && !messageFrom.OfType<MailboxAddress>().Any(f => string.Equals(messageFromFilter, f.Address, StringComparison.InvariantCultureIgnoreCase))) return false;
        if (messageFromRegexFilter != null && !messageFrom.OfType<MailboxAddress>().Any(f => messageFromRegexFilter.IsMatch(f.Address))) return false;

        return true;
    }

    public string Id => id;
    public RuleEffect Effect => effect;

    public static SimpleRule ParseFromJson(JsonObject json, string defaultId)
    {
        var id = json["id"]?.GetValue<string>() ?? defaultId;
        var effect = ParseEffect(json["effect"]?.AsObject() ?? throw new FormatException("Invalid rule JSON"));

        var filter = json["filter"]?.AsObject();
        string? fromFilter;
        Regex? fromRegexFilter;
        string? toFilter;
        Regex? toRegexFilter;
        string? messageFromFilter;
        Regex? messageFromRegexFilter;

        if (filter == null)
        {
            fromFilter = null;
            fromRegexFilter = null;
            toFilter = null;
            toRegexFilter = null;
            messageFromFilter = null;
            messageFromRegexFilter = null;
        }
        else
        {
            fromFilter = filter["from"]?.GetValue<string>();
            fromRegexFilter = ParseRegex(filter["fromRegex"]?.GetValue<string>());
            toFilter = filter["to"]?.GetValue<string>();
            toRegexFilter = ParseRegex(filter["toRegex"]?.GetValue<string>());
            messageFromFilter = filter["messageFrom"]?.GetValue<string>();
            messageFromRegexFilter = ParseRegex(filter["messageFromRegex"]?.GetValue<string>());
        }

        return new SimpleRule(id, effect, fromFilter, fromRegexFilter, toFilter, toRegexFilter, messageFromFilter, messageFromRegexFilter);

        Regex? ParseRegex(string? str) => str == null ? null : new Regex(str, RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.NonBacktracking | RegexOptions.IgnoreCase);

        RuleEffect ParseEffect(JsonObject effectJson)
        {
            var decision = ParseDecision(effectJson["decision"]?.GetValue<string>() ?? throw new FormatException("Invalid rule JSON"));
            return decision switch
            {
                Decision.Drop or Decision.Keep or Decision.Reject => new SimpleEffect(decision),
                Decision.ForwardAndDelete or Decision.ForwardAndKeep => new ForwardingEffect(decision, effectJson["address"]?.GetValue<string>() ?? throw new FormatException("Forwarding address required")),
                _ => throw new FormatException("Unsupported decision value " + decision)
            };
        }

        Decision ParseDecision(string str) => str switch
        {
            "reject" => Decision.Reject,
            "drop" => Decision.Drop,
            "keep" => Decision.Keep,
            "forwardAndDelete" => Decision.ForwardAndDelete,
            "forwardAndKeep" => Decision.ForwardAndKeep,
            _ => throw new FormatException("Invalid decision value " + str)
        };
    }
}