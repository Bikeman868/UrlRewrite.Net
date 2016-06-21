using UrlRewrite.Interfaces.Conditions;

namespace UrlRewrite.Interfaces.Actions
{
    public interface IInsertAction: IAction
    {
        IInsertAction Initialize(Scope scope, string scopeIndex, IValueGetter valueGetter);
    }
}
