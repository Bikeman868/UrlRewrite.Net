using System.Xml.Linq;
using UrlRewrite.Interfaces.Rules;

namespace UrlRewrite.Interfaces.Operations
{
    public interface IOperation : IRuleElement
    {
        IOperation Initialize(XElement configuration);
        string Execute(string value);
    }
}
