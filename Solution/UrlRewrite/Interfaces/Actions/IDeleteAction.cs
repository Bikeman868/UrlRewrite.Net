using UrlRewrite.Interfaces.Conditions;

namespace UrlRewrite.Interfaces.Actions
{
    public interface IDeleteAction: IAction
    {
        IDeleteAction Initialize(Scope scope, string scopeIndex = null);
    }
}
