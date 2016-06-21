using System.Collections.Generic;
using UrlRewrite.Interfaces.Actions;

namespace UrlRewrite.Interfaces.Rules
{
    public interface IRuleList : IAction
    {
        IRuleList Initialize(
            string name,
            IList<IRule> rules = null,
            bool stopProcessing = false);

        IRuleList Add(IRule rule);

        string Name { get; }
        IRuleListResult Evaluate(IRequestInfo request);
    }
}
