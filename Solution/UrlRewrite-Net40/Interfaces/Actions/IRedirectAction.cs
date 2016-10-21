using System.Xml.Linq;
using UrlRewrite.Interfaces.Conditions;

namespace UrlRewrite.Interfaces.Actions
{
    public interface IRedirectAction: IAction
    {
        IRedirectAction Initialize(bool stopProcessing = true, bool endRequest = true);
    }
}
