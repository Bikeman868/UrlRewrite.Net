using System.Collections.Generic;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Rules;

namespace UrlRewrite.Rules
{
    public class RuleListResult: IRuleListResult
    {
        public bool StopProcessing { get; set; }
        public bool EndRequest { get; set; }
        public bool IsDynamic { get; set; }
        public List<IRuleResult> RuleResults { get; set; }
    }
}
