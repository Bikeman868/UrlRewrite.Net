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

        void TraceRuleBegin(string rulePath);
        void TraceRuleEnd(string rulePath);

        void TraceConditionListBegin(ConditionLogic logic);
        void TraceConditionListEnd(bool conditionsMet);

        void TraceCondition(IRuleCondition condition, bool isTrue);
        void TraceAction(IRuleAction action);
    }
}
