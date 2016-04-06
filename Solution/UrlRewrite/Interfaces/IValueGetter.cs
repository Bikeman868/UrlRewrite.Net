namespace UrlRewrite.Interfaces
{
    public interface IValueGetter
    {
        IValueGetter Initialize(Scope scope, string scopeIndex = null, bool ignoreCase = true);
        IValueGetter Initialize(Scope scope, int scopeIndex);

        string GetString(IRequestInfo requestInfo);
        int GetInt(IRequestInfo requestInfo, int defaultValue);
    }
}
