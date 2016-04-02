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
        private readonly IValueGetter _valueGetter;
        private readonly CompareOperation _compareOperation;
        private readonly bool _inverted;
        private readonly bool _ignoreCase;
        private readonly Func<IRuleResult, string, bool> _testFunc;

        public StringMatch(
            IValueGetter valueGetter, 
            CompareOperation compareOperation,
            string match,
            bool inverted = false,
            bool ignoreCase = true)
        {
            _match = match;
            _valueGetter = valueGetter;
            _compareOperation = compareOperation;
            _inverted = inverted;
            _ignoreCase = ignoreCase;

            if (ignoreCase) _match = _match.ToLower();

            switch (compareOperation)
            {
                case CompareOperation.Contains:
                    if (ignoreCase)
                        _testFunc = (ruleResult, text) => text.ToLower().Contains(_match);
                    else
                        _testFunc = (ruleResult, text) => text.Contains(_match);
                    break;
                case CompareOperation.StartsWith:
                    if (ignoreCase)
                        _testFunc = (ruleResult, text) => text.ToLower().StartsWith(_match);
                    else
                        _testFunc = (ruleResult, text) => text.StartsWith(_match);
                    break;
                case CompareOperation.EndsWith:
                    if (ignoreCase)
                        _testFunc = (ruleResult, text) => text.ToLower().EndsWith(_match);
                    else
                        _testFunc = (ruleResult, text) => text.EndsWith(_match);
                    break;
                case CompareOperation.Equals:
                    if (ignoreCase)
                        _testFunc = (ruleResult, text) => text.ToLower().Equals(_match);
                    else
                        _testFunc = (ruleResult, text) => text.Equals(_match);
                    break;
                case CompareOperation.MatchRegex:
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
                    throw new UrlRewriteException("String match does not know how to match using " + compareOperation);
            }
        }

        public bool Test(IRequestInfo request, IRuleResult ruleResult)
        {
            return _inverted
                ? !_testFunc(ruleResult, _valueGetter.GetString(request))
                : _testFunc(ruleResult, _valueGetter.GetString(request));
        }

        public override string ToString()
        {
            var description = "request " + _valueGetter;
            description += (_inverted ? " not" : "") + " " + _compareOperation + " '" + _match + "'";
            description += _ignoreCase ? "" : " (case sensitive)";
            return description;
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
