using System;

namespace UrlRewrite.Interfaces
{
    public interface IRequestLog
    {
        void LogException(IRequestInfo request, Exception ex);
        void LogWarning(IRequestInfo request, string message);

        void TraceRequestBegin(IRequestInfo request);

        void TraceRuleBegin(IRequestInfo request, IRule rule);
        void TraceRuleEnd(IRequestInfo request, IRule rule, bool matched, bool stopProcessing);

        void TraceConditionListBegin(IRequestInfo request, CombinationLogic logic);
        void TraceConditionListEnd(IRequestInfo request, bool conditionsMet);

        void TraceActionListBegin(IRequestInfo request, IAction actionList);
        void TraceActionListEnd(IRequestInfo request, bool stopProcessing);

        void TraceCondition(IRequestInfo request, ICondition condition, bool isTrue);
        void TraceAction(IRequestInfo request, IAction action, bool endrequest, bool stopProcessing);
    }
}
