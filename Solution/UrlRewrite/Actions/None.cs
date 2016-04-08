using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    /// <summary>
    /// Truncates the path of the URL to specified number of elements
    /// </summary>
    internal class None: Action, IAction
    {
        public void PerformAction(
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

        public string ToString(IRequestInfo request)
        {
            return "do nothing";
        }
    }
}
