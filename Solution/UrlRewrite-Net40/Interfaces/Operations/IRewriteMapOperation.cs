using System.Xml.Linq;

namespace UrlRewrite.Interfaces.Operations
{
    public interface IRewriteMapOperation: IOperation
    {
        string Name { get; }
        IRewriteMapOperation Initialize(XElement element);
    }
}
