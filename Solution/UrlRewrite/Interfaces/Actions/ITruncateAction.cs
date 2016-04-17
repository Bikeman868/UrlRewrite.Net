using UrlRewrite.Interfaces.Conditions;

namespace UrlRewrite.Interfaces.Actions
{
    public interface ITruncateAction: IAction
    {
        ITruncateAction Initialize(int maximumDepth);
    }
}
