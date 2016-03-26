using System.Web;
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
    }
}
