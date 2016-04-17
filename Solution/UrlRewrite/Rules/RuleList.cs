using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Rules;

namespace UrlRewrite.Rules
{
    internal class RuleList: IRuleList, IAction
    {
        private string _name;
        private bool _stopProcessing;

        private IList<IRule> _rules;

        public IRuleList Initialize(
            string name, 
            IList<IRule> rules,
            bool stopProcessing)
        {
            _name = name;
            _rules = rules;
            _stopProcessing = stopProcessing;
            return this;
        }

        public IRuleList Add(IRule rule)
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

            if (_rules != null && _rules.Count > 0)
            {
                ruleListResult.RuleResults = new List<IRuleResult>();

                foreach (var rule in _rules)
                {
                    var ruleResult = rule.Evaluate(requestInfo);
                    ruleListResult.RuleResults.Add(ruleResult);

                    if (ruleResult.EndRequest) ruleListResult.EndRequest = true;
                    if (ruleResult.IsDynamic) ruleListResult.IsDynamic = true;
                    if (ruleResult.StopProcessing)
                    {
                        ruleListResult.StopProcessing = _stopProcessing;
                        break;
                    }
                }
            }

            if (requestInfo.ExecutionMode != ExecutionMode.ExecuteOnly)
                requestInfo.Log.TraceRuleListEnd(requestInfo, this, ruleListResult);

            return ruleListResult;
        }

        public override string ToString()
        {
            var count = _rules == null ? 0 : _rules.Count;
            return "list of " + count + " rules '" + _name + "'";
        }

        public IAction Initialize(XElement configuration)
        {
            return this;
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

            if (_rules != null)
                foreach (var rule in _rules)
                    rule.Describe(writer, indent, indentText);

            if (_stopProcessing)
                writer.WriteLine(indent + "Stop processing");
        }
    }
}
