using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    /// <summary>
    /// Truncates the path of the URL to specified number of elements
    /// </summary>
    internal class Truncate: Action
    {
        private readonly int _maximumDepth;

        public Truncate(int maximumDepth)
        {
            _maximumDepth = maximumDepth;
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
