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
        private Func<IRequestInfo, IRuleResult, string> _getValueFunc;

        public IValueGetter Initialize(
            Scope scope,
            int scopeIndex)
        {
            _scope = scope;
            _scopeIndex = scopeIndex.ToString();

            SetFunction(scopeIndex);

            return this;
        }

        public IValueGetter Initialize(
            Scope scope, 
            string scopeIndex,
            bool ignoreCase)
        {
            if (scopeIndex != null && string.IsNullOrWhiteSpace(scopeIndex))
                scopeIndex = null;

            var scopeIndexValue = 0;
            var scopeIndexIsNumber = false;

            var hasNumericIndex =
                scope == Scope.OriginalPathElement ||
                scope == Scope.PathElement ||
                scope == Scope.ConditionGroup ||
                scope == Scope.MatchGroup;

            if (scopeIndex == null)
            {
                if (scope == Scope.OriginalPathElement) scope = Scope.OriginalPath;
                if (scope == Scope.PathElement) scope = Scope.Path;
                if (scope == Scope.OriginalParameter) scope = Scope.OriginalQueryString;
                if (scope == Scope.Parameter) scope = Scope.QueryString;
            }
            else
            {
                if (hasNumericIndex)
                {
                    scopeIndexIsNumber = int.TryParse(scopeIndex, out scopeIndexValue);
                    if (!scopeIndexIsNumber)
                    {
                        if (scope == Scope.OriginalPathElement) scope = Scope.OriginalPath;
                        if (scope == Scope.PathElement) scope = Scope.Path;
                        if (scope == Scope.ConditionGroup)
                        {
                            scopeIndexValue = 0;
                            scopeIndexIsNumber = true;
                        }
                        if (scope == Scope.MatchGroup)
                        {
                            scopeIndexValue = 0;
                            scopeIndexIsNumber = true;
                        }
                    }
                }
            }

            _scope = scope;
            _scopeIndex = scopeIndex;
            _ignoreCase = ignoreCase;

            if (scopeIndexIsNumber)
                SetFunction(scopeIndexValue);
            else if (scopeIndex != null)
                SetFunction(scopeIndex);
            else
                SetFunction();

            return this;
        }

        /// <summary>
        /// Sets the function that will retrieve the value when the index is a number
        /// </summary>
        private void SetFunction(int scopeIndexValue)
        {
            switch (_scope)
            {
                case Scope.OriginalPathElement:
                    if (scopeIndexValue >= 0)
                    {
                        _getValueFunc = (request, ruleResult) => 
                            scopeIndexValue < request.OriginalPath.Count
                            ? request.OriginalPath[scopeIndexValue]
                            : string.Empty;
                    }
                    else
                    {
                        _getValueFunc = (request, ruleResult) =>
                        {
                            var i = request.OriginalPath.Count + scopeIndexValue;
                            if (string.IsNullOrEmpty(request.OriginalPath[request.OriginalPath.Count - 1])) i--;
                            return i > 0
                                ? request.OriginalPath[i]
                                : string.Empty;
                        };
                    }
                    break;

                case Scope.PathElement:
                    if (scopeIndexValue >= 0)
                    {
                        _getValueFunc = (request, ruleResult) => 
                            scopeIndexValue < request.NewPath.Count
                            ? request.NewPath[scopeIndexValue]
                            : string.Empty;
                    }
                    else
                    {
                        _getValueFunc = (request, ruleResult) =>
                        {
                            var i = request.NewPath.Count + scopeIndexValue;
                            if (string.IsNullOrEmpty(request.NewPath[request.NewPath.Count - 1])) i--;
                            return i > 0
                                ? request.NewPath[i]
                                : string.Empty;
                        };
                    }
                    break;

                case Scope.MatchGroup:
                    _getValueFunc = (request, ruleResult) =>
                    {
                        var matchGroups = ruleResult.Properties.Get<IList<string>>("R");
                        if (matchGroups == null || matchGroups.Count < scopeIndexValue + 1)
                            return string.Empty;
                        return matchGroups[scopeIndexValue];
                    };
                    break;

                case Scope.ConditionGroup:
                    _getValueFunc = (request, ruleResult) =>
                    {
                        var matchGroups = ruleResult.Properties.Get<IList<string>>("C");
                        if (matchGroups == null || matchGroups.Count < scopeIndexValue + 1)
                            return string.Empty;
                        return matchGroups[scopeIndexValue];
                    };
                    break;

                default:
                    throw new UrlRewriteException("ValueGetter does not know how to get " + _scope + "[" + scopeIndexValue + "] from the request");
            }
        }

        /// <summary>
        /// Sets the function that will retrieve the value when the index is a string
        /// </summary>
        private void SetFunction(string scopeIndex)
        {
            switch (_scope)
            {
                case Scope.OriginalParameter:
                    _getValueFunc = (request, ruleResult) =>
                    {
                        IList<string> values;
                        if (request.OriginalParameters.TryGetValue(scopeIndex, out values))
                            return values.Count > 0 ? values[0] : string.Empty;
                        return string.Empty;
                    };
                    break;

                case Scope.Parameter:
                    _getValueFunc = (request, ruleResult) =>
                    {
                        IList<string> values;
                        if (request.NewParameters.TryGetValue(scopeIndex, out values))
                            return values.Count > 0 ? values[0] : string.Empty;
                        return string.Empty;
                    };
                    break;

                case Scope.OriginalServerVariable:
                    _getValueFunc = (request, ruleResult) => request.GetOriginalServerVariable(scopeIndex);
                    break;

                case Scope.ServerVariable:
                    _getValueFunc = (request, ruleResult) => request.GetServerVariable(scopeIndex);
                    break;

                case Scope.OriginalHeader:
                    _getValueFunc = (request, ruleResult) => request.GetOriginalHeader(scopeIndex);
                    break;

                case Scope.Header:
                    _getValueFunc = (request, ruleResult) => request.GetHeader(scopeIndex);
                    break;

                case Scope.Literal:
                    _getValueFunc = (request, ruleResult) => scopeIndex;
                    break;

                default:
                    throw new UrlRewriteException("ValueGetter does not know how to get " + _scope + "[\"" + scopeIndex + "\"] from the request");
            }
        }

        /// <summary>
        /// Sets the function that will retrieve the value when there is no index
        /// </summary>
        private void SetFunction()
        {
            switch (_scope)
            {
                case Scope.OriginalUrl:
                    _getValueFunc = (request, ruleResult) => request.OriginalUrlString;
                    break;

                case Scope.OriginalPath:
                    _getValueFunc = (request, ruleResult) => request.OriginalPathString;
                    break;

                case Scope.OriginalQueryString:
                    _getValueFunc = (request, ruleResult) => request.OriginalParametersString;
                    break;

                case Scope.Url:
                    _getValueFunc = (request, ruleResult) => request.NewUrlString;
                    break;

                case Scope.Path:
                    _getValueFunc = (request, ruleResult) => request.NewPathString;
                    break;

                case Scope.QueryString:
                    _getValueFunc = (request, ruleResult) => request.NewParametersString;
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

        public string GetString(IRequestInfo requestInfo, IRuleResult ruleResult)
        {
            var value = _getValueFunc(requestInfo, ruleResult);
            return _ignoreCase ? value.ToLower() : value;
        }

        public int GetInt(IRequestInfo requestInfo, IRuleResult ruleResult, int defaultValue)
        {
            int value;
            if (int.TryParse(_getValueFunc(requestInfo, ruleResult), out value))
                return value;
            return defaultValue;
        }
    }
}
