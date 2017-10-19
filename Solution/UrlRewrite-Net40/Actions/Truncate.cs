using System;
using System.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Rules;

namespace UrlRewrite.Actions
{
    /// <summary>
    /// Truncates the path of the URL to specified number of elements
    /// </summary>
    internal class Truncate : Action, ITruncateAction
    {
        private int _maximumDepth;

        public ITruncateAction Initialize(int maximumDepth)
        {
            _maximumDepth = maximumDepth;
            return this;
        }

        public override void PerformAction(
            IRequestInfo requestInfo,
            IRuleResult ruleResult,
            out bool stopProcessing,
            out bool endRequest)
        {
            if (requestInfo.NewPath != null && requestInfo.NewPath.Count > _maximumDepth)
                requestInfo.NewPath = requestInfo.NewPath.Take(_maximumDepth).ToList();

            stopProcessing = _stopProcessing;
            endRequest = _endRequest;
        }

        public override string ToString()
        {
            return "Truncate the URL path to a maximum depth of " + _maximumDepth;
        }

        public override string ToString(IRequestInfo request)
        {
            return "truncate the URL path to a maximum depth of " + _maximumDepth;
        }
    }
}
