using System;
using System.Collections.Generic;
using System.Web;
using UrlRewrite.Conditions;
using UrlRewrite.Interfaces;

namespace UrlRewrite
{
    internal class RuleEngine: IRuleEngine
    {
        private readonly IRuleCondition[] _conditions;
        private readonly IRuleAction[] _actions;

        public RuleEngine()
        {
            _conditions = new IRuleCondition[]
            {
                new PathContainsString("1"),
                new PathContainsString("2"),
                new PathContainsString("3")
            };

            _actions = new IRuleAction[]
            {
                new Actions.Rewrite(),
                new Actions.PermenantRedirect(),
                new Actions.TemporaryRedirect()
            };
        }

        public void Dispose()
        {
        }

        public IEnumerable<IRuleAction> EvaluateRules(IRequestInfo request)
        {
            // Hard coded rules just to get things going for now

            for (var i = 0; i < _conditions.Length; i++)
            {
                if (request.TraceRequest) request.Log.TraceRuleBegin("Rule " + (i + 1));
                try
                {
                    if (request.TraceRequest) request.Log.TraceConditionListBegin(ConditionLogic.AllTrue);
                    var conditionsMet = _conditions[i].Test(request);
                    if (request.TraceRequest) request.Log.TraceCondition(_conditions[i], conditionsMet);
                    if (request.TraceRequest) request.Log.TraceConditionListEnd(conditionsMet);
                    if (conditionsMet)
                    {
                        var action = _actions[i];
                        if (request.TraceRequest) request.Log.TraceAction(action);
                        return new[] {action};
                    }
                }
                catch (Exception ex)
                {
                    request.Log.LogException(ex);
                }
                finally
                {
                    if (request.TraceRequest) request.Log.TraceRuleEnd("Rule " + (i + 1));
                }
            }

            return null;
        }
    }
}
