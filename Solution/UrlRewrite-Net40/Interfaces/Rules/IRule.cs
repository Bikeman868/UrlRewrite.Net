using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Conditions;

namespace UrlRewrite.Interfaces.Rules
{
    public interface IRule : IRuleElement
    {
        IRule Initialize(
            string name,
            ICondition condition,
            IAction action,
            bool stopProcessing = false,
            bool isDynamic = false);

        string Name { get; }
        IRuleResult Evaluate(IRequestInfo request);
    }
}
