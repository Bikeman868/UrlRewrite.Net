using System.Web;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class TemporaryRedirect : Action, IAction
    {
        public TemporaryRedirect()
        {
            _stopProcessing = true;
            _endRequest = true;
        }
     
        public bool PerformAction(IRequestInfo requestInfo)
        {
            requestInfo.Context.Response.Redirect(BuildNewUrl(requestInfo));
            return StopProcessing;
        }
    }
}
