using UrlRewrite.Interfaces.Conditions;

namespace UrlRewrite.Interfaces.Actions
{
    public interface IDoNothingAction: IAction
    {
        IDoNothingAction Initialize();
    }
}
