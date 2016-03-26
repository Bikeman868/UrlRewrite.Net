using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class Replace: Action, IAction
    {
        private Scope _scope;
        private List<string> _path;
        private Dictionary<string, List<string>> _queryString;

        public Replace(Scope scope, string value)
        {
            _scope = scope;
            switch (scope)
            {
                case Scope.Url:
                    break;
                case Scope.Path:
                    _path = value
                        .Split('/')
                        .ToList();
                    break;
                case Scope.QueryString:
                    break;
            }
        }

        public bool PerformAction(IRequestInfo request)
        {
            switch (_scope)
            {
                case Scope.Url:
                    request.NewPath = _path;
                    request.NewQueryString = _queryString;
                    break;
                case Scope.Path:
                    request.NewPath = _path;
                    break;
                case Scope.QueryString:
                    request.NewQueryString = _queryString;
                    break;
            }
            return false;
        }
    }
}
