using System.Xml.Linq;

namespace UrlRewrite.Interfaces
{
    public interface IRuleElement
    {
        void Initialize(XElement configuration);
        string ToString(IRequestInfo requestInfo);
    }
}
