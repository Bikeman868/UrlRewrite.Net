using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Rules
{
    internal class Rule: IRule
    {
        private readonly string _name;
        private readonly ICondition _condition;
        private readonly bool _stopProcessing;
        private readonly List<IAction> _actions;
        private readonly IRuleList _childRules;

        public Rule(
            string name, 
            ICondition condition, 
            IAction action, 
            bool stopProcessing = false)
        {
            _name = name;
            _condition = condition;
            _actions = action == null ? null : new List<IAction> { action };
            _stopProcessing = stopProcessing;
        }

        public string Name { get { return _name; } }

        public IRuleResult Evaluate(IRequestInfo request)
        {
            if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                request.Log.TraceRuleBegin(request, this);

            var result = new RuleResult();

            var conditionIsTrue = true;
            if (_condition != null)
            {
                conditionIsTrue = _condition.Test(request, result);

                if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                    request.Log.TraceCondition(request, _condition, conditionIsTrue);
            }

            if (conditionIsTrue)
            {
                result.StopProcessing = _stopProcessing;
                if (_actions != null)
                {
                    result.Actions = new List<IAction>();

                    foreach (var action in _actions)
                    {
                        result.Actions.Add(action);

                        bool stopProcessing;
                        bool endRequest;
                        action.PerformAction(request, result, out stopProcessing, out endRequest);

                        if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                            request.Log.TraceAction(request, action, endRequest, stopProcessing);

                        if (endRequest)
                            result.EndRequest = true;

                        if (stopProcessing)
                        {
                            result.StopProcessing = true;
                            break;
                        }
                    }
                }
            }

            if (request.ExecutionMode != ExecutionMode.ExecuteOnly)
                request.Log.TraceRuleEnd(request, this, conditionIsTrue, result);

            return result;
        }

        public override string ToString()
        {
            return "rule '" + _name + "'";
        }

        public void Initialize(XElement configuration)
        {
        }

        public string ToString(IRequestInfo request)
        {
            return ToString();
        }

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            writer.WriteLine(indent + _name + " rule");
            indent += indentText;

            if (_condition != null) 
                _condition.Describe(writer, indent, indentText);

            if (_actions != null) 
                foreach (var action in _actions)
                    action.Describe(writer, indent, indentText);

            if (_childRules != null)
                _childRules.Describe(writer, indent, indentText);

            if (_stopProcessing)
                writer.WriteLine(indent + "Stop processing");
        }
    }
}
