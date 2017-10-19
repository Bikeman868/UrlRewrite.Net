using System.IO;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Rules;

namespace UrlRewrite.Actions
{
    internal class AbortRequest: Action, IAbortAction
    {
        public IAbortAction Initialize()
        {
            return this;
        }

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
