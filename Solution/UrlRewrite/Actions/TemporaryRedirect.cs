using System.Web;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class TemporaryRedirect : Action, IAction
    {
        public TemporaryRedirect()
        {
            _stopProcessing = true;
            _endRequest = true;
        }
     
        public bool PerformAction(IRequestInfo requestInfo)
        {
            requestInfo.Context.Response.Redirect(BuildNewUrl(requestInfo));
            return StopProcessing;
        }

        public override string ToString()
        {
            return "Temporarily redirect to new URL";
        }

        public void Initialize(XElement configuration)
        {
        }

        public string ToString(IRequestInfo request)
        {
            var url = BuildNewUrl(request);
            return "temporary redirect to '" + url + "'";
        }
    }
}
