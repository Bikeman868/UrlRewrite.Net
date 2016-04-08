using System;
using System.Collections.Generic;
using UrlRewrite.Interfaces;
using UrlRewrite.Utilities;

namespace UrlRewrite.Actions
{
    internal class Delete: Action, IAction
    {
        private readonly Scope _scope;
        private readonly string _scopeIndex;
        private readonly int _scopeIndexValue;

        public Delete(Scope scope, string scopeIndex = null)
        {
            _scope = scope;
            _scopeIndex = scopeIndex;

            if (string.IsNullOrEmpty(scopeIndex))
            {
                switch (scope)
                {
                    case Scope.Header:
                        throw new UrlRewriteException("When deleting a request header you must specify the name of the header to delete");
                    case Scope.Parameter:
                        _scope = Scope.QueryString;
                        break;
                    case Scope.PathElement:
                        _scope = Scope.Path;
                        break;
                }
            }
            else
            {
                if (!int.TryParse(scopeIndex, out _scopeIndexValue))
                {
                    if (scope == Scope.PathElement) _scope = Scope.Path;
                }
            }
        }

        public void PerformAction(
            IRequestInfo requestInfo,
            IRuleResult ruleResult,
            out bool stopProcessing,
            out bool endRequest)
        {
            switch (_scope)
            {
                case Scope.Url:
                    requestInfo.NewUrlString = string.Empty;
                    break;
                case Scope.Path:
                    requestInfo.NewPathString = "/";
                    break;
                case Scope.QueryString:
                    requestInfo.NewParametersString = string.Empty;
                    break;
                case Scope.Header:
                    requestInfo.SetHeader(_scopeIndex, null);
                    break;
                case Scope.Parameter:
                    requestInfo.NewParameters.Remove(_scopeIndex);
                    requestInfo.ParametersChanged();
                    break;
                case Scope.PathElement:
                    requestInfo.NewPath.RemoveAt(_scopeIndexValue);
                    break;
                case Scope.ServerVariable:
                    requestInfo.SetServerVariable(_scopeIndex, null);
                    break;
            }

            stopProcessing = _stopProcessing;
            endRequest = _endRequest;
        }

        public override string ToString()
        {
            var text = "Delete " + _scope;
            if (!string.IsNullOrEmpty(_scopeIndex))
                text += "[" + _scopeIndex + "]";
            return text;
        }

        public string ToString(IRequestInfo request)
        {
            var text = "delete " + _scope;
            if (!string.IsNullOrEmpty(_scopeIndex))
                text += "[" + _scopeIndex + "]";
            return text;
        }
    }
}
