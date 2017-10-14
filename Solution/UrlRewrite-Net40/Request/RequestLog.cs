using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UrlRewrite.Actions;
using UrlRewrite.Conditions;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Rules;
using UrlRewrite.Interfaces.Utilities;
using UrlRewrite.Utilities;

namespace UrlRewrite.Request
{
    public class RequestLog : IRequestLog
    {
        private readonly PerformanceTimer _timer = new PerformanceTimer();
        private readonly List<string> _output = new List<string>();

        private string indent = string.Empty;

        private void Output(params string[] message)
        {
            var line = string.Format("Rewrite: {0,6}mS {1}{2}",
                _timer.ElapsedMilliSeconds.ToString("F1"),
                indent,
                string.Join(" ", message));
            _output.Add(line);
        }

        private void IncreaseIndent()
        {
            indent += "  ";
        }

        private void ReduceIndent()
        {
            indent = indent.Substring(2);
        }

        void IRequestLog.LogException(IRequestInfo request, Exception ex)
        {
            Output("Exception", ex.Message);
        }

        void IRequestLog.LogWarning(IRequestInfo request, string message)
        {
            Output("Warning", message);
        }

        void IRequestLog.TraceRequestBegin(IRequestInfo request)
        {
            Output("rewriting ", request.OriginalUrlString);
            IncreaseIndent();
            _timer.Start();
        }

        public void TraceRequestEnd(IRequestInfo request)
        {
            _timer.Stop();
            ReduceIndent();
            Output("finished ", request.NewUrlString);

            Trace.WriteLine("");
            foreach (var line in _output) Trace.WriteLine(line);
        }

        void IRequestLog.TraceRuleListBegin(IRequestInfo request, IRuleList ruleList)
        {
            Output(ruleList.ToString(request));
            IncreaseIndent();
        }

        void IRequestLog.TraceRuleListEnd(IRequestInfo request, IRuleList ruleList, IRuleListResult ruleListResult)
        {
            ReduceIndent();
            Output(
                ruleList.ToString(request),
                (ruleListResult.RuleResults != null && ruleListResult.RuleResults.Count > 0 ? ruleListResult.RuleResults.Count + " rules evaluated." : ""),
                (ruleListResult.StopProcessing ? "Stop processing." : ""),
                (ruleListResult.EndRequest ? "End request." : ""));
        }

        void IRequestLog.TraceRuleBegin(IRequestInfo request, IRule rule)
        {
            Output(rule.ToString(request));
            IncreaseIndent();
        }

        void IRequestLog.TraceRuleEnd(IRequestInfo request, IRule rule, bool matched, IRuleResult ruleResult)
        {
            ReduceIndent();
            Output(
                rule.ToString(request),
                (matched ? "was executed." : "does not match this request."),
                (ruleResult.IsDynamic ? "Results are dynamic and can not be cached." : ""),
                (ruleResult.StopProcessing ? "Stop processing." : ""),
                (ruleResult.EndRequest ? "End request." : ""));
        }

        void IRequestLog.TraceConditionListBegin(IRequestInfo request, ICondition condition)
        {
            Output(condition.ToString(request));
            IncreaseIndent();
        }

        void IRequestLog.TraceConditionListEnd(IRequestInfo request, ICondition condition, bool conditionsMet)
        {
            ReduceIndent();
            Output(condition.ToString(request), "evaluated to", conditionsMet ? "true" : "false");
        }

        void IRequestLog.TraceCondition(IRequestInfo request, ICondition condition, bool isTrue)
        {
            if (!(condition is ConditionList))
                Output(condition.ToString(request), (isTrue ? "is true" : "is false"));
        }

        void IRequestLog.TraceAction(IRequestInfo request, IAction action, bool endRequest, bool stopProcessing)
        {
            if (!(action is ActionList))
                Output(action.ToString(request));
        }

        void IRequestLog.TraceActionListBegin(IRequestInfo request, IAction actionList)
        {
            Output("execute", actionList.ToString(request));
            IncreaseIndent();
        }

        void IRequestLog.TraceActionListEnd(IRequestInfo request, bool stopProcessing)
        {
            ReduceIndent();
            Output("finished executing actions.", (stopProcessing ? "Stop processing" : ""));
        }
    }
}
