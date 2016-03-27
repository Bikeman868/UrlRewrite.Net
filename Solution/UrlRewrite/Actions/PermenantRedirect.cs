using System.Web;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class PermenantRedirect : Action, IAction
    {
        public PermenantRedirect()
        {
            _stopProcessing = true;
            _endRequest = true;
        }
     
        public bool PerformAction(IRequestInfo requestInfo)
        {
            requestInfo.Context.Response.RedirectPermanent(BuildNewUrl(requestInfo));
            return StopProcessing;
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
            return ToString();
        }
    }
}
