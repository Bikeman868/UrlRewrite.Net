using System.Xml.Linq;

namespace UrlRewrite.Interfaces
{
    public interface IOperation : IRuleElement
    {
        IOperation Initialize(XElement configuration);
        string Execute(string value);
    }
}
