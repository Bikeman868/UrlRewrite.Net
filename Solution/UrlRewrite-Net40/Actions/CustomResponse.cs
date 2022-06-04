using System.IO;
using System.Web;
using System.Xml.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Rules;
using System.Linq;

namespace UrlRewrite.Actions
{
    internal class CustomResponse : Action, ICustomResponse
    {
        private int _statusCode;
        private string _statusDescription;
        private string _responseLine;

        public ICustomResponse Initialize(XElement configuration, bool stopProcessing, bool endRequest)
        {
            _stopProcessing = stopProcessing;
            _endRequest = endRequest;
            _statusCode = 200;
            _statusDescription = "OK";
            _responseLine = "OK";

            if (configuration.HasAttributes)
            {
                foreach (var attribute in configuration.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "statuscode":
                            _statusCode = int.Parse(attribute.Value);
                            break;
                        case "statusdescription":
                            _statusDescription = attribute.Value;
                            break;
                        case "statusline":
                            {
                                var parts = attribute.Value.Split(' ');
                                _statusCode = int.Parse(parts[1]);
                                _statusDescription = string.Join(" ", parts.Skip(2).ToArray());
                            }
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
                requestInfo.Context.Response.StatusCode = _statusCode;
                requestInfo.Context.Response.StatusDescription = _statusDescription;
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
