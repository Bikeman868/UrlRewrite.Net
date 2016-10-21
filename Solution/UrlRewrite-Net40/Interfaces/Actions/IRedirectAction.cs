using System.Xml.Linq;
using UrlRewrite.Interfaces.Conditions;

namespace UrlRewrite.Interfaces.Actions
{
    public interface IRedirectAction: IAction
    {
        IRedirectAction Initialize(XElement configuration, bool stopProcessing = true, bool endRequest = true);
    }
}
