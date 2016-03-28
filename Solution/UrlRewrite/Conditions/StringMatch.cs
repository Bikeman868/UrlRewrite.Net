using System;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Conditions
{
    internal class StringMatch: ICondition
    {
        private readonly string _match;
        private readonly Scope _scope;
        private readonly MatchPattern _matchPattern;
        private readonly Func<IRequestInfo, string> _getValueFunc;
        private readonly Func<IRuleResult, string, bool> _testFunc;

        public StringMatch(
            Scope scope, 
            MatchPattern matchPattern,
            string match)
        {
            _match = match.ToLower();
            _scope = scope;
            _matchPattern = matchPattern;

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
                    throw new NotImplementedException("String match does not know how to get " + scope + " from the request");
            }

            switch (matchPattern)
            {
                case MatchPattern.Contains:
                    _testFunc = (ruleResult, text) => text.Contains(_match);
                    break;
                case MatchPattern.StartsWith:
                    _testFunc = (ruleResult, text) => text.StartsWith(_match);
                    break;
                case MatchPattern.EndsWith:
                    _testFunc = (ruleResult, text) => text.EndsWith(_match);
                    break;
                case MatchPattern.Equals:
                    _testFunc = (ruleResult, text) => text.Equals(_match);
                    break;
                default:
                    throw new NotImplementedException("String match does not know how to match using " + matchPattern);
            }
        }

        public bool Test(IRequestInfo request, IRuleResult ruleResult)
        {
            return _testFunc(ruleResult, _getValueFunc(request));
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
