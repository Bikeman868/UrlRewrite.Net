using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Rules
{
    internal class RuleList: IRuleList, IAction
    {
        private readonly string _name;
        private readonly ICondition _condition;
        private readonly bool _stopProcessing;

        private IList<IRule> _rules;

        public RuleList(
            string name, 
            ICondition condition, 
            IList<IRule> rules = null,
            bool stopProcessing = false)
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

        public IRuleListResult Evaluate(IRequestInfo requestInfo)
        {
            if (requestInfo.ExecutionMode != ExecutionMode.ExecuteOnly)
                requestInfo.Log.TraceRuleListBegin(requestInfo, this);

            var ruleListResult = new RuleListResult();

            var conditionIsTrue = true;
            if (_condition != null)
            {
                conditionIsTrue = _condition.Test(requestInfo);

                if (requestInfo.ExecutionMode != ExecutionMode.ExecuteOnly)
                    requestInfo.Log.TraceCondition(requestInfo, _condition, conditionIsTrue);
            }

            if (conditionIsTrue)
            {
                ruleListResult.StopProcessing = _stopProcessing;

                if (_rules != null && _rules.Count > 0)
                {
                    ruleListResult.RuleResults = new List<IRuleResult>();

                    foreach (var rule in _rules)
                    {
                        var ruleResult = rule.Evaluate(requestInfo);
                        ruleListResult.RuleResults.Add(ruleResult);

                        if (ruleResult.EndRequest) ruleListResult.EndRequest = true;
                        if (ruleResult.StopProcessing)
                        {
                            ruleListResult.StopProcessing = true;
                            break;
                        }
                    }
                }
            }

            if (requestInfo.ExecutionMode != ExecutionMode.ExecuteOnly)
                requestInfo.Log.TraceRuleListEnd(requestInfo, this, conditionIsTrue, ruleListResult);

            return ruleListResult;
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

        void IAction.PerformAction(
            IRequestInfo requestInfo, 
            IRuleResult ruleResult, 
            out bool stopProcessing, 
            out bool endRequest)
        {
            var result = Evaluate(requestInfo);
            stopProcessing = result.StopProcessing;
            endRequest = result.EndRequest;
        }

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            writer.WriteLine(indent + _name + " list of rules");
            indent += indentText;

            if (_condition != null) 
                _condition.Describe(writer, indent, indentText);

            if (_rules != null)
                foreach (var rule in _rules)
                    rule.Describe(writer, indent, indentText);

            if (_stopProcessing)
                writer.WriteLine(indent + "Stop processing");
        }
    }
}
