using System.Web;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class TemporaryRedirect : IRuleAction
    {
        public bool PerformAction(IRequestInfo requestInfo)
        {
            requestInfo.Context.Response.Redirect("/SomewhereElse.aspx");
            return StopProcessing;
        }

        public bool EndRequest
        {
            get { return true; }
        }

        public bool StopProcessing
        {
            get { return true; }
        }
    }
}
