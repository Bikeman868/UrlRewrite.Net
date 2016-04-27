using System.Xml.Linq;
using UrlRewrite.Interfaces.Rules;

namespace UrlRewrite.Interfaces.Operations
{
    public interface IOperation : IRuleElement
    {
        string Execute(string value);
    }
}
