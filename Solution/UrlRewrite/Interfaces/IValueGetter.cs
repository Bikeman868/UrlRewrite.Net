using System.Collections.Generic;

namespace UrlRewrite.Interfaces
{
    public interface IValueGetter : IDescribable
    {
        IValueGetter Initialize(Scope scope, string scopeIndex = null, IOperation operation = null);
        IValueGetter Initialize(Scope scope, int scopeIndex, IOperation operation = null);

        string GetString(IRequestInfo requestInfo, IRuleResult ruleResult);
        int GetInt(IRequestInfo requestInfo, IRuleResult ruleResult, int defaultValue);
    }
}
