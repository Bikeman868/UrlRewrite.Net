using System.IO;
using System.Web;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class AbortRequest: Action
    {
        public override void PerformAction(
            IRequestInfo requestInfo,
            IRuleResult ruleResult,
            out bool stopProcessing,
            out bool endRequest)
        {
            stopProcessing = true;
            endRequest = true;
        }

        public override string ToString()
        {
            return "Abort the request";
        }

        public override string ToString(IRequestInfo requestInfo)
        {
            return "abort the request";
        }
    }
}
