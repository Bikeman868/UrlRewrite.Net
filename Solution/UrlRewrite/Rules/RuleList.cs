using System.Collections.Generic;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Rules
{
    internal class RuleList: IRule
    {
        private readonly string _name;
        private readonly ICondition _condition;
        private readonly bool _stopProcessing;

        private IList<IRule> _rules;

        public RuleList(string name, ICondition condition, IList<IRule> rules = null, bool stopProcessing = false)
        {
            _name = name;
            _condition = condition;
            _rules = rules;
            _stopProcessing = stopProcessing;
        }

        public RuleList Add(IRule rule)
        {
            if (_rules == null) _rules = new List<IRule>();
            _rules.Add(rule);
            return this;
        }

        public string Name
        {
            get { return _name; }
        }

        public IRuleResult Evaluate(IRequestInfo request)
        {
            if (request.TraceRequest)
                request.Log.TraceRuleBegin(request, this);

            var conditionIsTrue = true;
            if (_condition != null)
            {
                conditionIsTrue = _condition.Test(request);

                if (request.TraceRequest)
                    request.Log.TraceCondition(request, _condition, conditionIsTrue);
            }

            var result = new RuleResult();

            if (conditionIsTrue)
            {
                result.StopProcessing = _stopProcessing;

                if (_rules != null && _rules.Count > 0)
                {
                    result.Actions = new List<IAction>();
                    foreach (var rule in _rules)
                    {
                        var ruleResult = rule.Evaluate(request);

                        if (ruleResult.Actions != null && ruleResult.Actions.Count > 0)
                            result.Actions.AddRange(ruleResult.Actions);

                        if (ruleResult.StopProcessing)
                        {
                            result.StopProcessing = true;
                            break;
                        }
                    }
                }
            }

            if (request.TraceRequest)
                request.Log.TraceRuleEnd(request, this, conditionIsTrue, result.StopProcessing);

            return result;
        }

        public override string ToString()
        {
            var count = _rules == null ? 0 : _rules.Count;
            return "list of " + count + " rules '" + _name + "'";
        }

        public void Initialize(XElement configuration)
        {
        }

        public string ToString(IRequestInfo request)
        {
            return ToString();
        }
    }
}
