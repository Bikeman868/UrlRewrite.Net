using System.Xml.Linq;

namespace UrlRewrite.Interfaces
{
    public interface IRuleElement : IDescribable
    {
        string ToString(IRequestInfo requestInfo);
    }
}
