using System.Collections.Generic;

namespace UrlRewrite.Interfaces
{
    public interface IRuleListResult
    {
        bool StopProcessing { get; }
        bool EndRequest { get; }
        bool IsDynamic { get; }
        List<IRuleResult> RuleResults { get; }
    }
}
