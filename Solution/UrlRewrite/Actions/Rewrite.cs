using System.Web;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class Rewrite: Action, IAction
    {
        public Rewrite(bool stopProcessing = true, bool endRequest = false)
        {
            _stopProcessing = stopProcessing;
            _endRequest = endRequest;
        }

        public void PerformAction(
            IRequestInfo requestInfo,
            IRuleResult ruleResult,
            out bool stopProcessing,
            out bool endRequest)
        {
            if (requestInfo.ExecutionMode != ExecutionMode.TraceOnly)
                requestInfo.Context.RewritePath(requestInfo.NewUrlString);

            stopProcessing = _stopProcessing;
            endRequest = _endRequest;
        }

        public override string ToString()
        {
            return "Rewrite the URL, resulting in a different page being returned than the one requested";
        }

        public void Initialize(XElement configuration)
        {
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return "rewrite the request URL to '" + requestInfo.NewUrlString + "'";
        }
    }
}
