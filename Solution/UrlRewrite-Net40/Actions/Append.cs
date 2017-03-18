using System;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Rules;
using UrlRewrite.Utilities;

namespace UrlRewrite.Actions
{
    internal class Append: Action, IAppendAction
    {
        private Scope _scope;
        private string _scopeIndex;
        private int _scopeIndexValue;
        private IValueGetter _valueGetter;

        public IAppendAction Initialize(Scope scope, string scopeIndex, IValueGetter valueGetter)
        {
            _scope = scope;
            _scopeIndex = scopeIndex;
            _valueGetter = valueGetter;

            if (string.IsNullOrEmpty(scopeIndex))
            {
                switch (scope)
                {
                    case Scope.Header:
                        throw new UrlRewriteException("When appending the request headers you must specify the name of the header to replace");
                    case Scope.ServerVariable:
                        throw new UrlRewriteException("When appending server variables you must specify the name of the server variable to replace");
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
                int.TryParse(scopeIndex, out _scopeIndexValue);
                if (_scopeIndexValue == 0 && scope == Scope.PathElement)
                    _scope = Scope.Path;
            }

            return this;
        }

        public override void PerformAction(
            IRequestInfo requestInfo,
            IRuleResult ruleResult,
            out bool stopProcessing,
            out bool endRequest)
        {
            var value = _valueGetter.GetString(requestInfo, ruleResult);

            switch (_scope)
            {
                case Scope.Url:
                    requestInfo.NewUrlString = requestInfo.NewUrlString + value;
                    break;
                case Scope.Path:
                    requestInfo.NewPathString = requestInfo.NewPathString 
                        + (requestInfo.NewPathString.EndsWith("/") ? "" : "/") + value;
                    break;
                case Scope.QueryString:
                    if (!string.IsNullOrWhiteSpace(value))
                        requestInfo.NewParametersString = requestInfo.NewParametersString
                            + (requestInfo.NewParametersString.Length > 0 ? "&" : "") + value;
                    break;
                case Scope.Header:
                    requestInfo.SetHeader(_scopeIndex, requestInfo.GetHeader(_scopeIndex) + value);
                    break;
                case Scope.Parameter:
                    requestInfo.NewParameters[_scopeIndex].Add(value);
                    requestInfo.ParametersChanged();
                    break;
                case Scope.PathElement:
                    {
                        var count = requestInfo.NewPath.Count;
                        if (string.IsNullOrEmpty(requestInfo.NewPath[count - 1]))
                            count--;
                        if (_scopeIndexValue > 0)
                        {
                            if (_scopeIndexValue < count)
                            {
                                requestInfo.NewPath[_scopeIndexValue] = requestInfo.NewPath[_scopeIndexValue] + value;
                                requestInfo.PathChanged();
                            }
                        }
                        else
                        {
                            var index = count + _scopeIndexValue;
                            if (index > 0)
                            {
                                requestInfo.NewPath[index] = requestInfo.NewPath[index] + value;
                                requestInfo.PathChanged();
                            }
                        }
                        requestInfo.PathChanged();
                        break;
                    }
                case Scope.ServerVariable:
                    requestInfo.SetServerVariable(_scopeIndex, requestInfo.GetServerVariable(_scopeIndex) + value);
                    break;
            }

            stopProcessing = _stopProcessing;
            endRequest = _endRequest;
        }

        public override string ToString()
        {
            var text = "Append " + _valueGetter + " to " + _scope;
            if (!string.IsNullOrEmpty(_scopeIndex))
                text += "[" + _scopeIndex + "]";
            return text;
        }

        public override string ToString(IRequestInfo request)
        {
            var text = "append " + _valueGetter + " to " + _scope;
            if (!string.IsNullOrEmpty(_scopeIndex))
                text += "[" + _scopeIndex + "]";
            return text;
        }
    }
}
