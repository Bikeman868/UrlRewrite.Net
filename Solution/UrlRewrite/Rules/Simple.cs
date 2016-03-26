using System.Collections.Generic;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Rules
{
    internal class Simple: IRule
    {
        private readonly string _name;
        private readonly ICondition _condition;
        private readonly IRuleResult _noMatchResult;
        private readonly IRuleResult _matchResult;

        public Simple(string name, ICondition condition, IAction action, bool stopProcessing = false)
        {
            _name = name;
            _condition = condition;

            _noMatchResult = new RuleResult();

            _matchResult = action == null
                ? new RuleResult
                    {
                        StopProcessing = stopProcessing
                    }
                : new RuleResult 
                    {
                        Actions = new List<IAction> { action }, 
                        StopProcessing = stopProcessing 
                    };
        }

        public string Name
        {
            get { return _name; }
        }

        public IRuleResult Evaluate(IRequestInfo request)
        {
            if (request.TraceRequest)
                request.Log.TraceRuleBegin(this);

            var conditionIsTrue = true;

            if (_condition != null)
            {
                conditionIsTrue = _condition.Test(request);

                if (request.TraceRequest)
                    request.Log.TraceCondition(_condition, conditionIsTrue);
            }

            var result = conditionIsTrue ? _matchResult : _noMatchResult;

            if (request.TraceRequest)
                request.Log.TraceRuleEnd(conditionIsTrue, result.StopProcessing);

            return result;
        }
    }
}
