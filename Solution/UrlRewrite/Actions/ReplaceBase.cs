using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal abstract class ReplaceBase: Action
    {
        protected readonly Scope _scope;

        protected ReplaceBase(Scope scope)
        {
            _scope = scope;
        }

        public void PerformAction(
            IRequestInfo requestInfo,
            IRuleResult ruleResult,
            out bool stopProcessing,
            out bool endRequest)
        {
            List<string> path;
            Dictionary<string, List<string>> queryString;
            GetValues(requestInfo, _scope, out path, out queryString);

            switch (_scope)
            {
                case Scope.Url:
                    requestInfo.NewPath = path;
                    requestInfo.NewParameters = queryString;
                    break;
                case Scope.Path:
                    requestInfo.NewPath = path;
                    break;
                case Scope.QueryString:
                    requestInfo.NewParameters = queryString;
                    break;
            }

            stopProcessing = _stopProcessing;
            endRequest = _endRequest;
        }

        protected abstract void GetValues(
            IRequestInfo request,
            Scope scope,
            out List<string> path,
            out Dictionary<string, List<string>> queryString);

        protected List<string> ParsePath(string path)
        {
            return string.IsNullOrEmpty(path) ? null : path.Split('/').ToList();
        }

        protected Dictionary<string, List<string>> ParseQueryString(string queryString)
        {
            if (string.IsNullOrEmpty(queryString))
                return null;

            return null;
        }

    }
}
