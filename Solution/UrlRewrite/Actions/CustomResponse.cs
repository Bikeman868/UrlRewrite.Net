using System.IO;
using System.Web;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class CustomResponse: Action, IAction
    {
        private string _statusLine;
        private string _responseLine;

        public CustomResponse() : this(true, true) { }

        public CustomResponse(bool stopProcessing, bool endRequest)
        {
            _stopProcessing = stopProcessing;
            _endRequest = endRequest;
        }

        public override void Initialize(XElement configuration)
        {
            _statusLine = "HTTP/1.1 200 OK";
            _responseLine = "OK";

            if (configuration.HasAttributes)
            {
                foreach (var attribute in configuration.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "statusline":
                            _statusLine = attribute.Value;
                            break;
                        case "responseline":
                            _responseLine = attribute.Value;
                            break;
                    }
                }
            }
        }

        public void PerformAction(
            IRequestInfo requestInfo,
            IRuleResult ruleResult,
            out bool stopProcessing,
            out bool endRequest)
        {
            if (requestInfo.ExecutionMode != ExecutionMode.TraceOnly)
            {
                requestInfo.Context.Response.StatusDescription = _statusLine;
                requestInfo.Context.Response.Write(_responseLine);
            }

            stopProcessing = _stopProcessing;
            endRequest = _endRequest;
        }

        public override string ToString()
        {
            return "Return a custom response";
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return "return a custom response";
        }

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            writer.WriteLine(indent + ToString());
        }
    }
}
