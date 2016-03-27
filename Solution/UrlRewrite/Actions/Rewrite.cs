using System.Web;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class Rewrite: Action, IAction
    {
        public Rewrite(bool stopProcessing = false, bool endRequest = false)
        {
            _stopProcessing = stopProcessing;
            _endRequest = endRequest;
        }

        public bool PerformAction(IRequestInfo requestInfo)
        {
            requestInfo.Context.RewritePath(BuildNewUrl(requestInfo));
            return StopProcessing;
        }

        public override string ToString()
        {
            return "Rewrite the URL, resulting in a different page being returned than the one requested";
        }

        public void Initialize(XElement configuration)
        {
        }

        public string ToString(IRequestInfo request)
        {
            var url = BuildNewUrl(request);
            return "rewrite the request URL to '" + url + "'";
        }
    }
}
