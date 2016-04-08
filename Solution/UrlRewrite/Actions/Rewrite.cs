using System.Web;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class Rewrite: Action, IAction
    {
        public Rewrite() : this(true, false) { }

        public Rewrite(bool stopProcessing, bool endRequest)
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
                requestInfo.Context.RewritePath(requestInfo.NewPathString, "", requestInfo.NewParametersString);

            stopProcessing = _stopProcessing;
            endRequest = _endRequest;
        }

        public override string ToString()
        {
            return "Rewrite the URL, resulting in a different page being returned than the one requested";
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return "rewrite the request URL to '" + requestInfo.NewUrlString + "'";
        }
    }
}
