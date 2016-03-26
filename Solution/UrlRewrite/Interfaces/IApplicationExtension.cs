using System.Xml.Linq;

namespace UrlRewrite.Interfaces
{
    public interface IApplicationExtension
    {
        void Initialize(XElement configuration);
    }
}
