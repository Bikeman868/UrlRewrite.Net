using System;
using System.Collections.Generic;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Rules;
using UrlRewrite.Utilities;

namespace UrlRewrite.Actions
{
    internal class Replace : Action, IReplaceAction
    {
        private Scope _scope;
        private string _scopeIndex;
        private int _scopeIndexValue;
        private IValueGetter _valueGetter;

        public IReplaceAction Initialize(Scope scope, string scopeIndex, IValueGetter valueGetter)
        {
            _scope = scope;
            _scopeIndex = scopeIndex;
            _valueGetter = valueGetter;

            if (string.IsNullOrEmpty(scopeIndex))
            {
                switch (scope)
                {
                    case Scope.Header:
                        throw new UrlRewriteException("When replacing the request headers you must specify the name of the header to replace");
                    case Scope.ServerVariable:
                        throw new UrlRewriteException("When replacing server variables you must specify the name of the server variable to replace");
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
            var value = _valueGetter.GetString(requestInfo, ruleResult);

            switch (_scope)
            {
                case Scope.Url:
                    requestInfo.NewUrlString = value;
                    break;
                case Scope.Path:
                    requestInfo.NewPathString = value;
                    break;
                case Scope.QueryString:
                    requestInfo.NewParametersString = value;
                    break;
                case Scope.Header:
                    requestInfo.SetHeader(_scopeIndex, value);
                    break;
                case Scope.Parameter:
                    requestInfo.NewParameters[_scopeIndex] = new List<string> { value };
                    requestInfo.ParametersChanged();
                    break;
                case Scope.PathElement:
                    if (_scopeIndexValue == 0)
                        requestInfo.NewPathString = value;
                    else if (_scopeIndexValue > 0)
                    {
                        var count = requestInfo.NewPath.Count;
                        if (string.IsNullOrEmpty(requestInfo.NewPath[count - 1]))
                            count--;
                        if (_scopeIndexValue < count)
                        {
                            requestInfo.NewPath[_scopeIndexValue] = value;
                            requestInfo.PathChanged();
                        }
                    }
                    else
                    {
                        var count = requestInfo.NewPath.Count;
                        if (string.IsNullOrEmpty(requestInfo.NewPath[count - 1]))
                            count--;
                        var index = count + _scopeIndexValue;
                        if (index > 0)
                        {
                            requestInfo.NewPath[index] = value;
                            requestInfo.PathChanged();
                        }
                    }
                    break;
                case Scope.ServerVariable:
                    requestInfo.SetServerVariable(_scopeIndex, value);
                    break;
            }

            stopProcessing = _stopProcessing;
            endRequest = _endRequest;
        }

        public override string ToString()
        {
            var text = "Replace " + _scope;
            if (!string.IsNullOrEmpty(_scopeIndex))
                text += "[" + _scopeIndex + "]";
            text += " with " + _valueGetter;
            return text;
        }

        public override string ToString(IRequestInfo request)
        {
            var text = "replace " + _scope;
            if (!string.IsNullOrEmpty(_scopeIndex))
                text += "[" + _scopeIndex + "]";
            text += " with " + _valueGetter;
            return text;
        }
    }
}
