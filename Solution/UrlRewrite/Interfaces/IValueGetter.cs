namespace UrlRewrite.Interfaces
{
    public interface IValueGetter
    {
        IValueGetter Initialize(Scope scope, string scopeIndex = null, bool ignoreCase = true);
        IValueGetter Initialize(Scope scope, int scopeIndex);

        string GetString(IRequestInfo requestInfo, IRuleResult ruleResult);
        int GetInt(IRequestInfo requestInfo, IRuleResult ruleResult, int defaultValue);
    }
}
