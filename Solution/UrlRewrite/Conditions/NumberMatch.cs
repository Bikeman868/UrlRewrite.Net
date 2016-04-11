using System;
using System.IO;
using System.Xml.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Utilities;

namespace UrlRewrite.Conditions
{
    internal class NumberMatch: INumberMatch
    {
        private int _match;
        private IValueGetter _valueGetter;
        private CompareOperation _compareOperation;
        private bool _inverted;
        private int _defaultValue;
        private Func<IRuleResult, int, bool> _testFunc;

        public INumberMatch Initialize(
            IValueGetter valueGetter, 
            CompareOperation compareOperation,
            int match,
            bool inverted = false,
            int defaultValue = 0)
        {
            _match = match;
            _valueGetter = valueGetter;
            _compareOperation = compareOperation;
            _inverted = inverted;
            _defaultValue = defaultValue;

            switch (compareOperation)
            {
                case CompareOperation.Equals:
                    _testFunc = (ruleResult, number) => number == _match;
                    break;
                case CompareOperation.Less:
                    _testFunc = (ruleResult, number) => number < _match;
                    break;
                case CompareOperation.Greater:
                    _testFunc = (ruleResult, number) => number > _match;
                    break;
                default:
                    throw new UrlRewriteException("Number match does not know how to compare numbers using " + compareOperation);
            }
            return this;
        }

        public bool Test(IRequestInfo request, IRuleResult ruleResult)
        {
            return _inverted
                ? !_testFunc(ruleResult, _valueGetter.GetInt(request, ruleResult, _defaultValue))
                : _testFunc(ruleResult, _valueGetter.GetInt(request, ruleResult, _defaultValue));
        }

        public override string ToString()
        {
            var description = "request " + _valueGetter + " (default " + _defaultValue + ")";
            description += (_inverted ? " not" : "") + " " + _compareOperation + " " + _match;
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
            writer.WriteLine(indent + " If " + ToString());
        }
    }
}
