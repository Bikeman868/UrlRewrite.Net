using System;
using System.Collections.Generic;
using System.Diagnostics;
using UrlRewrite.Interfaces;
using UrlRewrite.Utilities;

namespace UrlRewrite.Request
{
    internal class RequestLog : IRequestLog
    {
        private readonly PerformanceTimer _timer = new PerformanceTimer();
        private readonly List<string> _output = new List<string>();

        private void Output(params string[] message)
        {
            var line = string.Format("Rewrite: {0,6}mS {1}",
                _timer.ElapsedMilliSeconds.ToString("F1"),
                string.Join(" ", message));
            _output.Add(line);
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
            Output("rewriting URL", request.OriginalUrlString);
            _timer.Start();
        }

        public void TraceRequestEnd(IRequestInfo request)
        {
            _timer.Stop();
            Output("finished URL", request.NewUrlString);

            Trace.WriteLine("--");
            foreach (var line in _output) Trace.WriteLine(line);
        }

        void IRequestLog.TraceRuleListBegin(IRequestInfo request, IRuleList ruleList)
        {
            Output("begin", ruleList.ToString(request));
        }

        void IRequestLog.TraceRuleListEnd(IRequestInfo request, IRuleList ruleList, bool matched, IRuleListResult ruleListResult)
        {
            Output(
                ruleList.ToString(request),
                (matched ? "was executed." : "does not match this request."),
                (ruleListResult.RuleResults != null && ruleListResult.RuleResults.Count > 0 ? ruleListResult.RuleResults.Count + " rules evaluated." : ""),
                (ruleListResult.StopProcessing ? "Stop processing." : ""),
                (ruleListResult.EndRequest ? "End request." : ""));
        }

        void IRequestLog.TraceRuleBegin(IRequestInfo request, IRule rule)
        {
            Output("begin " + rule.ToString(request));
        }

        void IRequestLog.TraceRuleEnd(IRequestInfo request, IRule rule, bool matched, IRuleResult ruleResult)
        {
            Output(
                rule.ToString(request),
                (matched ? "was executed." : "does not match this request."),
                (ruleResult.StopProcessing ? "Stop processing." : ""),
                (ruleResult.EndRequest ? "End request." : ""));
        }

        void IRequestLog.TraceConditionListBegin(IRequestInfo request, CombinationLogic logic)
        {
            Output("list of conditions where " + logic);
        }

        void IRequestLog.TraceConditionListEnd(IRequestInfo request, bool conditionsMet)
        {
            Output("list of conditions evaluated to " + conditionsMet);
        }

        void IRequestLog.TraceCondition(IRequestInfo request, ICondition condition, bool isTrue)
        {
            Output(
                "condition",
                condition.ToString(request),
                (isTrue ? "is true" : "is false"));
        }

        void IRequestLog.TraceAction(IRequestInfo request, IAction action, bool endRequest, bool stopProcessing)
        {
            Output("action", action.ToString(request));
        }

        void IRequestLog.TraceActionListBegin(IRequestInfo request, IAction actionList)
        {
            Output("start", actionList.ToString(request));
        }

        void IRequestLog.TraceActionListEnd(IRequestInfo request, bool stopProcessing)
        {
            Output("finished list of actions.", (stopProcessing ? "Stop processing" : ""));
        }
    }
}
