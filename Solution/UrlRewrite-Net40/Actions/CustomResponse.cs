using System.IO;
using System.Web;
using System.Xml.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Rules;

namespace UrlRewrite.Actions
{
    public class CustomResponse : Action, ICustomResponse
    {
        private string _statusLine;
        private string _responseLine;

        public ICustomResponse Initialize(XElement configuration, bool stopProcessing, bool endRequest)
        {
            _stopProcessing = stopProcessing;
            _endRequest = endRequest;
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
            base.Initialize(configuration);
            return this;
        }

        public override void PerformAction(
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

        public override string ToString(IRequestInfo requestInfo)
        {
            return "return a custom response";
        }
    }
}
