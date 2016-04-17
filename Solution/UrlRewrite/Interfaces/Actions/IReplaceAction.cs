using UrlRewrite.Interfaces.Conditions;

namespace UrlRewrite.Interfaces.Actions
{
    public interface IReplaceAction: IAction
    {
        IReplaceAction Initialize(Scope scope, string scopeIndex, IValueGetter valueGetter);
    }
}
