using System.Collections.Generic;

namespace UrlRewrite.Interfaces.Rules
{
    public interface IRuleList : IRuleElement
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
