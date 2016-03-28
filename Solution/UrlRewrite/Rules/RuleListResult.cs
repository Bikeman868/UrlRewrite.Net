using System.Collections.Generic;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Rules
{
    internal class RuleListResult: IRuleListResult
    {
        public bool StopProcessing { get; set; }
        public bool EndRequest { get; set; }
        public List<IRuleResult> RuleResults { get; set; }
    }
}
