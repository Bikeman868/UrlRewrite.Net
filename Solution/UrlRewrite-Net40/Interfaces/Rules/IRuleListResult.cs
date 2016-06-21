using System.Collections.Generic;

namespace UrlRewrite.Interfaces.Rules
{
    public interface IRuleListResult
    {
        bool StopProcessing { get; }
        bool EndRequest { get; }
        bool IsDynamic { get; }
        List<IRuleResult> RuleResults { get; }
    }
}
