using System;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Rules;
using UrlRewrite.Utilities;

namespace UrlRewrite.Actions
{
    public class Delete: Action, IDeleteAction
    {
        private Scope _scope;
        private string _scopeIndex;
        private int _scopeIndexValue;

        public IDeleteAction Initialize(Scope scope, string scopeIndex)
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

            return this;
        }

        public override void PerformAction(
            IRequestInfo requestInfo,
            IRuleResult ruleResult,
            out bool stopProcessing,
            out bool endRequest)
        {
            switch (_scope)
            {
                case Scope.Url:
                    requestInfo.NewUrlString = "/";
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
                {
                    if (_scopeIndexValue == 0 || requestInfo.NewPathString == "/")
                    {
                        requestInfo.NewPathString = "/";
                    }
                    else
                    {
                        var count = requestInfo.NewPath.Count;
                        if (string.IsNullOrEmpty(requestInfo.NewPath[count - 1])) count--;
                        var indexToRemove = _scopeIndexValue < 0
                            ? count + _scopeIndexValue
                            : _scopeIndexValue;
                        if (indexToRemove > 0 && indexToRemove < count)
                        {
                            requestInfo.NewPath.RemoveAt(indexToRemove);
                            requestInfo.PathChanged();
                        }
                    }
                    break;
                }
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

        public override string ToString(IRequestInfo request)
        {
            var text = "delete " + _scope;
            if (!string.IsNullOrEmpty(_scopeIndex))
                text += "[" + _scopeIndex + "]";
            return text;
        }
    }
}
