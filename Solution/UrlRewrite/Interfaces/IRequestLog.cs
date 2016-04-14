using System;

namespace UrlRewrite.Interfaces
{
    public interface IRequestLog
    {
        void LogException(IRequestInfo request, Exception ex);
        void LogWarning(IRequestInfo request, string message);

        void TraceRequestBegin(IRequestInfo request);
        void TraceRequestEnd(IRequestInfo request);

        void TraceRuleListBegin(IRequestInfo request, IRuleList ruleList);
        void TraceRuleListEnd(IRequestInfo request, IRuleList ruleList, IRuleListResult ruleResult);

        void TraceRuleBegin(IRequestInfo request, IRule rule);
        void TraceRuleEnd(IRequestInfo request, IRule rule, bool matched, IRuleResult ruleResult);

        void TraceConditionListBegin(IRequestInfo request, ICondition condition);
        void TraceConditionListEnd(IRequestInfo request, ICondition condition, bool conditionsMet);

        void TraceActionListBegin(IRequestInfo request, IAction actionList);
        void TraceActionListEnd(IRequestInfo request, bool stopProcessing);

        void TraceCondition(IRequestInfo request, ICondition condition, bool isTrue);
        void TraceAction(IRequestInfo request, IAction action, bool endrequest, bool stopProcessing);
    }
}
