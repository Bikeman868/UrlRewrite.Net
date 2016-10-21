using System.Xml.Linq;
using UrlRewrite.Interfaces.Conditions;

namespace UrlRewrite.Interfaces.Actions
{
    public interface ICustomResponse: IAction
    {
        ICustomResponse Initialize(XElement configuration, bool stopProcessing = true, bool endRequest = true);
    }
}
