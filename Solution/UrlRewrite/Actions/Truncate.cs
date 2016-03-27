using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    /// <summary>
    /// Truncates the path of the URL to specified number of elements
    /// </summary>
    internal class Truncate: Action, IAction
    {
        private readonly int _maximumDepth;

        public Truncate(int maximumDepth)
        {
            _maximumDepth = maximumDepth;
        }

        public bool PerformAction(IRequestInfo request)
        {
            if (request.NewPath != null && request.NewPath.Count > _maximumDepth)
                request.NewPath = request.NewPath.Take(_maximumDepth).ToList();

            return _stopProcessing;
        }

        public override string ToString()
        {
            return "Truncate the URL path to a maximum depth of " + _maximumDepth;
        }

        public void Initialize(XElement configuration)
        {
        }

        public string ToString(IRequestInfo request)
        {
            return "truncate the URL path to a maximum depth of " + _maximumDepth;
        }
    }
}
