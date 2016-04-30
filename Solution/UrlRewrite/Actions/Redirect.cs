using System;
using System.Linq;
using System.Xml.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Rules;
using UrlRewrite.Utilities;

namespace UrlRewrite.Actions
{
    internal class Redirect : Action
    {
        private Action<IRequestInfo, string> _redirectAction;
        private string _code;

        public Redirect() : this(true, true) { }

        public Redirect(bool stopProcessing , bool endRequest)
        {
            _stopProcessing = stopProcessing;
            _endRequest = endRequest;
        }

        public override void PerformAction(
            IRequestInfo requestInfo,
            IRuleResult ruleResult,
            out bool stopProcessing,
            out bool endRequest)
        {
            if (requestInfo.ExecutionMode != ExecutionMode.TraceOnly)
            {
                var url = requestInfo.NewUrlString;
                requestInfo.DeferredActions.Add(ri => _redirectAction(ri, url));
            }

            stopProcessing = _stopProcessing;
            endRequest = _endRequest;
        }

        public override IAction Initialize(XElement configuration)
        {
            var redirectTypeAttribute = configuration.Attributes().FirstOrDefault(a => a.Name.LocalName.ToLower() == "redirecttype");
            _code = redirectTypeAttribute == null ? "307" : redirectTypeAttribute.Value;

            switch (_code.ToLower())
            {
                case "permenant":
                case "301": 
                    _redirectAction = (ri, url) => ri.Context.Response.RedirectPermanent(url);
                    break;
                case "found":
                case "302":
                    _redirectAction = (ri, url) =>
                    {
                        ri.Context.Response.Redirect(url);
                        ri.Context.Response.StatusCode = 302;
                    };
                    break;
                case "seeother":
                case "see other":
                case "303":
                    _redirectAction = (ri, url) =>
                    {
                        ri.Context.Response.Redirect(url);
                        ri.Context.Response.StatusCode = 303;
                    };
                    break;
                case "temporary":
                case "307":
                    _redirectAction = (ri, url) => ri.Context.Response.Redirect(url);
                    break;
                default:
                    throw new UrlRewriteException("Unknown redirection type code. Supported values are permenant, found, seeOther, temporary, 301, 302, 303 and 307");
            }
            return base.Initialize(configuration);
        }

        public override string ToString()
        {
            return "Redirect to new URL with " + _code + " code";
        }

        public override string ToString(IRequestInfo requestInfo)
        {
            return "redirect to '" + requestInfo.NewUrlString + "' with " + _code + " code";
        }
    }
}
