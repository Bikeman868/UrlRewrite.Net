using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Utilities;

namespace UrlRewrite.Conditions
{
    internal class StringMatch: IStringMatch
    {
        private string _match;
        private IValueGetter _valueGetter;
        private CompareOperation _compareOperation;
        private bool _inverted;
        private bool _ignoreCase;
        private Func<IRuleResult, string, bool> _testFunc;

        public IStringMatch Initialize(
            IValueGetter valueGetter, 
            CompareOperation compareOperation,
            string match,
            bool inverted = false,
            bool ignoreCase = true,
            string matchGroupsName = "C")
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
                    {
                        _match = _match.ToLower();
                        _testFunc = (ruleResult, text) => text.ToLower().Contains(_match);
                    }
                    else
                        _testFunc = (ruleResult, text) => text.Contains(_match);
                    break;
                case CompareOperation.StartsWith:
                    if (ignoreCase)
                    {
                        _match = _match.ToLower();
                        _testFunc = (ruleResult, text) => text.ToLower().StartsWith(_match);
                    }
                    else
                        _testFunc = (ruleResult, text) => text.StartsWith(_match);
                    break;
                case CompareOperation.EndsWith:
                    if (ignoreCase)
                    {
                        _match = _match.ToLower();
                        _testFunc = (ruleResult, text) => text.ToLower().EndsWith(_match);
                    }
                    else
                        _testFunc = (ruleResult, text) => text.EndsWith(_match);
                    break;
                case CompareOperation.Equals:
                    if (ignoreCase)
                    {
                        _match = _match.ToLower();
                        _testFunc = (ruleResult, text) => text.ToLower().Equals(_match);
                    }
                    else
                        _testFunc = (ruleResult, text) => text.Equals(_match);
                    break;
                case CompareOperation.MatchRegex:
                {
                    var options = RegexOptions.Compiled | RegexOptions.ECMAScript;
                    if (ignoreCase) options |= RegexOptions.IgnoreCase;
                    var regex = new Regex(match, options);
                    _testFunc = GetFunc(regex, matchGroupsName);
                    break;
                }
                case CompareOperation.MatchWildcard:
                {
                    var regularExpression = "^" + Regex.Escape(match).Replace("\\*", "(.*)").Replace("\\?", ".") + "$";
                    var options = RegexOptions.Compiled | RegexOptions.Singleline;
                    if (ignoreCase) options |= RegexOptions.IgnoreCase;
                    var regex = new Regex(regularExpression, options);
                    _testFunc = GetFunc(regex, matchGroupsName);
                    break;
                }
                default:
                    throw new UrlRewriteException("String match does not know how to match using " + compareOperation);
            }
            return this;
        }

        private Func<IRuleResult, string, bool> GetFunc(Regex regex, string name)
        {
            return (ruleResult, text) =>
            {
                var m = regex.Match(text);
                if (m.Success)
                {
                    var matchGroups = new List<string>();
                    foreach (var group in m.Groups)
                        matchGroups.Add(group.ToString());
                    ruleResult.Properties.Set<IList<string>>(matchGroups, name);
                    return true;
                }
                return false;
            };
        }

        public bool Test(IRequestInfo request, IRuleResult ruleResult)
        {
            return _inverted
                ? !_testFunc(ruleResult, _valueGetter.GetString(request, ruleResult))
                : _testFunc(ruleResult, _valueGetter.GetString(request, ruleResult));
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

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            writer.WriteLine(indent + "If " + ToString());
        }
    }
}
