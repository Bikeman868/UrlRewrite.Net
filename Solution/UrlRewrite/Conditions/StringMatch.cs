using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Utilities;

namespace UrlRewrite.Conditions
{
    internal class StringMatch: ICondition
    {
        private readonly string _match;
        private readonly Scope _scope;
        private readonly MatchPattern _matchPattern;
        private readonly bool _inverted;
        private readonly Func<IRequestInfo, string> _getValueFunc;
        private readonly Func<IRuleResult, string, bool> _testFunc;

        public StringMatch(
            Scope scope, 
            MatchPattern matchPattern,
            string match,
            bool inverted = false,
            bool ignoreCase = true)
        {
            _match = match;
            _scope = scope;
            _matchPattern = matchPattern;
            _inverted = inverted;

            if (ignoreCase) _match = _match.ToLower();

            switch (scope)
            {
                case Scope.Url:
                    _getValueFunc = request => request.Context.Request.RawUrl.ToLower();
                    break;
                case Scope.Path:
                    _getValueFunc = request => request.OriginalPathString.ToLower();
                    break;
                case Scope.QueryString:
                    _getValueFunc = request => request.OriginalParametersString.ToLower();
                    break;
                default:
                    throw new UrlRewriteException("String match does not know how to get " + scope + " from the request");
            }

            switch (matchPattern)
            {
                case MatchPattern.Contains:
                    if (ignoreCase)
                        _testFunc = (ruleResult, text) => text.ToLower().Contains(_match);
                    else
                        _testFunc = (ruleResult, text) => text.Contains(_match);
                    break;
                case MatchPattern.StartsWith:
                    if (ignoreCase)
                        _testFunc = (ruleResult, text) => text.ToLower().StartsWith(_match);
                    else
                        _testFunc = (ruleResult, text) => text.StartsWith(_match);
                    break;
                case MatchPattern.EndsWith:
                    if (ignoreCase)
                        _testFunc = (ruleResult, text) => text.ToLower().EndsWith(_match);
                    else
                        _testFunc = (ruleResult, text) => text.EndsWith(_match);
                    break;
                case MatchPattern.Equals:
                    if (ignoreCase)
                        _testFunc = (ruleResult, text) => text.ToLower().Equals(_match);
                    else
                        _testFunc = (ruleResult, text) => text.Equals(_match);
                    break;
                case MatchPattern.MatchRegex:
                {
                    var options = RegexOptions.Compiled | RegexOptions.ECMAScript;
                    if (ignoreCase) options |= RegexOptions.IgnoreCase;
                    var regex = new Regex(match, options);
                    _testFunc = (ruleResult, text) =>
                    {
                        // TODO: store match groups in ruleResult.Properties
                        return regex.IsMatch(text);
                    };
                    break;
                }
                default:
                    throw new UrlRewriteException("String match does not know how to match using " + matchPattern);
            }
        }

        public bool Test(IRequestInfo request, IRuleResult ruleResult)
        {
            return _inverted 
                ? !_testFunc(ruleResult, _getValueFunc(request))
                : _testFunc(ruleResult, _getValueFunc(request));
        }

        public override string ToString()
        {
            return "request " + _scope + " " + _matchPattern + " '" + _match + "'";
        }

        public void Initialize(XElement configuration)
        {
        }

        public string ToString(IRequestInfo request)
        {
            return ToString();
        }
    }
}
