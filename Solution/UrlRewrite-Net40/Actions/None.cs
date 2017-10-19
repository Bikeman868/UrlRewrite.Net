using System.IO;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Rules;

namespace UrlRewrite.Actions
{
    /// <summary>
    /// Truncates the path of the URL to specified number of elements
    /// </summary>
    internal class None : Action, IDoNothingAction
    {
        public IDoNothingAction Initialize()
        {
            return this;
        }

        public override void PerformAction(
            IRequestInfo requestInfo,
            IRuleResult ruleResult,
            out bool stopProcessing,
            out bool endRequest)
        {
            stopProcessing = _stopProcessing;
            endRequest = _endRequest;
        }

        public override string ToString()
        {
            return "Do nothing";
        }

        public override string ToString(IRequestInfo request)
        {
            return "do nothing";
        }
    }
}
