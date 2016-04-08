using System.Collections.Generic;

namespace UrlRewrite.Interfaces
{
    public interface IValueGetter
    {
        IValueGetter Initialize(Scope scope, string scopeIndex = null, IList<IOperation> operations = null);
        IValueGetter Initialize(Scope scope, int scopeIndex, IList<IOperation> operations = null);

        string GetString(IRequestInfo requestInfo, IRuleResult ruleResult);
        int GetInt(IRequestInfo requestInfo, IRuleResult ruleResult, int defaultValue);
    }
}
