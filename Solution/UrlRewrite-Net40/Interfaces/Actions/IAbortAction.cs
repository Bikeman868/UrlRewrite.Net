using UrlRewrite.Interfaces.Conditions;

namespace UrlRewrite.Interfaces.Actions
{
    public interface IAbortAction: IAction
    {
        IAbortAction Initialize();
    }
}
