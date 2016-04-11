using System;
using System.Collections.Generic;
using System.IO;
using UrlRewrite.Interfaces;
using UrlRewrite.Utilities;

namespace UrlRewrite.Actions
{
    internal class Replace: Action, IAction
    {
        private readonly Scope _scope;
        private readonly string _scopeIndex;
        private readonly int _scopeIndexValue;
        private readonly IValueGetter _valueGetter;

        public Replace(Scope scope, string scopeIndex, IValueGetter valueGetter)
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
        }

        public void PerformAction(
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
                    requestInfo.NewPath[_scopeIndexValue] = value;
                    requestInfo.PathChanged();
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

        public string ToString(IRequestInfo request)
        {
            var text = "replace " + _scope;
            if (!string.IsNullOrEmpty(_scopeIndex))
                text += "[" + _scopeIndex + "]";
            text += " with " + _valueGetter;
            return text;
        }

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            writer.WriteLine(indent + ToString());
        }
    }
}
