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
            IList<string> path;
            IDictionary<string, IList<string>> queryString;
            GetValues(requestInfo, _scope, out path, out queryString);

            switch (_scope)
            {
                case Scope.NewUrl:
                    requestInfo.NewPath = path;
                    requestInfo.NewParameters = queryString;
                    break;
                case Scope.NewPath:
                    requestInfo.NewPath = path;
                    break;
                case Scope.NewQueryString:
                    requestInfo.NewParameters = queryString;
                    break;
            }

            stopProcessing = _stopProcessing;
            endRequest = _endRequest;
        }

        protected abstract void GetValues(
            IRequestInfo request,
            Scope scope,
            out IList<string> path,
            out IDictionary<string, IList<string>> queryString);

        protected IList<string> ParsePath(string path)
        {
            return string.IsNullOrEmpty(path) ? null : path.Split('/').ToList();
        }

        protected IDictionary<string, IList<string>> ParseQueryString(string queryString)
        {
            if (string.IsNullOrEmpty(queryString))
                return null;

            return null;
        }

    }
}
