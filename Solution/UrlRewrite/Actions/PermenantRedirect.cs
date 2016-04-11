using System.IO;
using System.Web;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class PermenantRedirect : Action, IAction
    {
        public PermenantRedirect() : this(true, true) { }

        public PermenantRedirect(bool stopProcessing, bool endRequest)
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
                var url = requestInfo.NewUrlString;
                requestInfo.DeferredActions.Add(ri => ri.Context.Response.RedirectPermanent(url));
            }

            stopProcessing = _stopProcessing;
            endRequest = _endRequest;
        }

        public override string ToString()
        {
            return "Permenently redirect to new URL";
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return "permenant redirect to '" + requestInfo.NewUrlString + "'";
        }

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            writer.WriteLine(indent + "Permenant redirect to new URL");
        }
    }
}
