using UrlRewrite.Interfaces.Conditions;

namespace UrlRewrite.Interfaces.Actions
{
    public interface IAppendAction: IAction
    {
        IAppendAction Initialize(Scope scope, string scopeIndex, IValueGetter valueGetter);
    }
}
