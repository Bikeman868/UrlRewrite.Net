using System.Web;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class Rewrite: Action, IAction
    {
        public Rewrite(bool stopProcessing = false, bool endRequest = false)
        {
            _stopProcessing = stopProcessing;
            _endRequest = endRequest;
        }

        public bool PerformAction(IRequestInfo requestInfo)
        {
            requestInfo.Context.RewritePath(BuildNewUrl(requestInfo));
            return StopProcessing;
        }
    }
}
