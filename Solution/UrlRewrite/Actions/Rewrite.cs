using System.Web;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class Rewrite: IRuleAction
    {
        public bool PerformAction(IRequestInfo requestInfo)
        {
            requestInfo.Context.RewritePath("/surprise.aspx");
            return StopProcessing;
        }

        public bool EndRequest
        {
            get { return false; }
        }

        public bool StopProcessing
        {
            get { return false; }
        }
    }
}
