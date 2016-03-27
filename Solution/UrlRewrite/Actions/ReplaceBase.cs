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

        public bool PerformAction(IRequestInfo request)
        {
            List<string> path;
            Dictionary<string, List<string>> queryString;
            GetValues(request, _scope, out path, out queryString);

            switch (_scope)
            {
                case Scope.Url:
                    request.NewPath = path;
                    request.NewQueryString = queryString;
                    break;
                case Scope.Path:
                    request.NewPath = path;
                    break;
                case Scope.QueryString:
                    request.NewQueryString = queryString;
                    break;
            }
            return false;
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
