using System.Web;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class PermenantRedirect : Action, IAction
    {
        public PermenantRedirect(bool stopProcessing = true, bool endRequest = true)
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
            {
                var url = BuildNewUrl(requestInfo);
                requestInfo.DeferredActions.Add(ri => ri.Context.Response.RedirectPermanent(url));
            }

            stopProcessing = _stopProcessing;
            endRequest = _endRequest;
        }

        public override string ToString()
        {
            return "Permenently redirect to new URL";
        }

        public void Initialize(XElement configuration)
        {
        }

        public string ToString(IRequestInfo requestInfo)
        {
            var url = BuildNewUrl(requestInfo);
            return "permenant redirect to '" + url + "'";
        }
    }
}
