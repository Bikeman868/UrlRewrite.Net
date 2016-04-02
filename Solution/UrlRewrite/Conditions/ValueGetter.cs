using System;
using System.Collections.Generic;
using UrlRewrite.Interfaces;
using UrlRewrite.Utilities;

namespace UrlRewrite.Conditions
{
    internal class ValueGetter: IValueGetter
    {
        private Scope _scope;
        private string _scopeIndex;
        private bool _ignoreCase;
        private Func<IRequestInfo, string> _getValueFunc;

        public IValueGetter Initialize(
            Scope scope,
            int scopeIndex,
            bool ignoreCase = true)
        {
            _scope = scope;
            _scopeIndex = scopeIndex.ToString();
            _ignoreCase = ignoreCase;

            SetFunction(scopeIndex);

            return this;
        }

        public IValueGetter Initialize(
            Scope scope, 
            string scopeIndex = null,
            bool ignoreCase = true)
        {
            if (scopeIndex != null && string.IsNullOrWhiteSpace(_scopeIndex))
                scopeIndex = null;

            var scopeIndexValue = 0;
            var scopeIndexIsNumber = false;

            if (scopeIndex == null)
            {
                if (scope == Scope.OriginalPathElement) scope = Scope.OriginalPath;
                if (scope == Scope.NewPathElement) scope = Scope.NewPath;
                if (scope == Scope.OriginalParameter) scope = Scope.OriginalQueryString;
                if (scope == Scope.NewParameter) scope = Scope.NewQueryString;
            }
            else
            {
                scopeIndexIsNumber = int.TryParse(scopeIndex, out scopeIndexValue);
            }

            if (!scopeIndexIsNumber)
            {
                if (scope == Scope.OriginalPathElement) scope = Scope.OriginalPath;
                if (scope == Scope.NewPathElement) scope = Scope.NewPath;
            }
         
            _scope = scope;
            _scopeIndex = scopeIndex;
            _ignoreCase = ignoreCase;

            SetFunction(scopeIndexValue);

            return this;
        }

        private void SetFunction(int scopeIndexValue)
        {
            switch (_scope)
            {
                case Scope.OriginalUrl:
                    _getValueFunc = request => request.Context.Request.RawUrl;
                    break;

                case Scope.OriginalPath:
                    _getValueFunc = request => request.OriginalPathString;
                    break;

                case Scope.OriginalQueryString:
                    _getValueFunc = request => request.OriginalParametersString;
                    break;

                case Scope.OriginalPathElement:
                    if (scopeIndexValue >= 0)
                    {
                        _getValueFunc = request => 
                            scopeIndexValue < request.OriginalPath.Count
                            ? request.OriginalPath[scopeIndexValue]
                            : string.Empty;
                    }
                    else
                    {
                        _getValueFunc = request =>
                        {
                            var i = request.OriginalPath.Count + scopeIndexValue;
                            return i < request.OriginalPath.Count
                                ? request.OriginalPath[i]
                                : string.Empty;
                        };
                    }
                    break;

                case Scope.OriginalParameter:
                    _getValueFunc = request =>
                    {
                        List<string> values;
                        if (request.OriginalParameters.TryGetValue(_scopeIndex, out values))
                            return values.Count > 0 ? values[0] : string.Empty;
                        return string.Empty;
                    };
                    break;

                case Scope.NewUrl:
                    _getValueFunc = request => request.NewUrlString;
                    break;

                case Scope.NewPath:
                    _getValueFunc = request => request.NewPathString;
                    break;

                case Scope.NewQueryString:
                    _getValueFunc = request => request.NewParametersString;
                    break;

                case Scope.NewPathElement:
                    if (scopeIndexValue >= 0)
                    {
                        _getValueFunc = request => 
                            scopeIndexValue < request.NewPath.Count
                            ? request.NewPath[scopeIndexValue]
                            : string.Empty;
                    }
                    else
                    {
                        _getValueFunc = request =>
                        {
                            var i = request.NewPath.Count + scopeIndexValue;
                            return i < request.NewPath.Count
                                ? request.NewPath[i]
                                : string.Empty;
                        };
                    }
                    break;

                case Scope.NewParameter:
                    _getValueFunc = request =>
                    {
                        List<string> values;
                        if (request.NewParameters.TryGetValue(_scopeIndex, out values))
                            return values.Count > 0 ? values[0] : string.Empty;
                        return string.Empty;
                    };
                    break;

                default:
                    throw new UrlRewriteException("ValueGetter does not know how to get " + _scope + " from the request");
            }
        }

        public override string ToString()
        {
            var description = _ignoreCase ? "lower case " : "";
            description += _scope.ToString();
            if (_scopeIndex != null)
                description += "[" + _scopeIndex + "]";
            return description;
        }

        public string GetString(IRequestInfo requestInfo)
        {
            var value = _getValueFunc(requestInfo);
            return _ignoreCase ? value.ToLower() : value;
        }

        public int GetInt(IRequestInfo requestInfo, int defaultValue)
        {
            int value;
            if (int.TryParse(_getValueFunc(requestInfo), out value))
                return value;
            return defaultValue;
        }
    }
}
