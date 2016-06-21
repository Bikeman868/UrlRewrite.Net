using UrlRewrite.Interfaces.Conditions;

namespace UrlRewrite.Interfaces.Actions
{
    public interface IKeepAction: IAction
    {
        IKeepAction Initialize(Scope scope, string scopeIndex);
    }
}
