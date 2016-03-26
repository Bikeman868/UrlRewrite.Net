using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Rules
{
    internal class RuleResult: IRuleResult
    {
        public List<IAction> Actions { get; set; }
        public bool StopProcessing { get; set; }
    }
}
