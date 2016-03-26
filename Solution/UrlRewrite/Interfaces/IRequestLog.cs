using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace UrlRewrite.Interfaces
{
    public interface IRequestLog
    {
        void LogException(Exception ex);
        void LogWarning(string message);

        void TraceRuleBegin(IRule rule);
        void TraceRuleEnd(bool matched, bool stopProcessing);

        void TraceConditionListBegin(CombinationLogic logic);
        void TraceConditionListEnd(bool conditionsMet);

        void TraceCondition(ICondition condition, bool isTrue);
        void TraceAction(IAction action);
    }
}
