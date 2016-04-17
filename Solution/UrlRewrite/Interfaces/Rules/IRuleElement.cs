using UrlRewrite.Interfaces.Utilities;

namespace UrlRewrite.Interfaces.Rules
{
    public interface IRuleElement : IDescribable
    {
        string ToString(IRequestInfo requestInfo);
    }
}
