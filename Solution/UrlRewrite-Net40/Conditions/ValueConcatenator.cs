using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Operations;
using UrlRewrite.Interfaces.Rules;

namespace UrlRewrite.Conditions
{
    /// <summary>
    /// Implements IValueGetter by taking a list of IValueGetter and concetenating their output
    /// </summary>
    internal class ValueConcatenator : IValueConcatenator
    {
        private IList<IValueGetter> _values;
        private string _separator;
        private IOperation _operation;

        public IValueGetter Initialize(IList<IValueGetter> values, string separator = null, IOperation operation = null)
        {
            _values = values;
            _separator = separator;
            _operation = operation;
            return this;
        }

        public IValueGetter Initialize(IValueGetter value, IOperation operation)
        {
            _values = new List<IValueGetter> { value };
            _separator = null;
            _operation = operation;
            return this;
        }

        public IValueGetter Initialize(Scope scope, string scopeIndex, IOperation operation)
        {
            _values = new List<IValueGetter> { new ValueGetter().Initialize(scope, scopeIndex, operation) };
            _operation = null;
            return this;
        }

        public IValueGetter Initialize(Scope scope, int scopeIndex, IOperation operation)
        {
            _values = new List<IValueGetter> { new ValueGetter().Initialize(scope, scopeIndex, operation) };
            _operation = null;
            return this;
        }

        public string GetString(IRequestInfo requestInfo, IRuleResult ruleResult)
        {
            var output = new StringBuilder();
            for (var i = 0; i < _values.Count; i++)
            {
                if (i > 0 && !ReferenceEquals(_separator, null))
                    output.Append(_separator);
                output.Append(_values[i].GetString(requestInfo, ruleResult));
            }

            if (ReferenceEquals(_operation, null))
                return output.ToString();

            return _operation.Execute(output.ToString());
        }

        public int GetInt(IRequestInfo requestInfo, IRuleResult ruleResult, int defaultValue)
        {
            var value = GetString(requestInfo, ruleResult);
            int intValue;
            return int.TryParse(value, out intValue) ? intValue : defaultValue;
        }

        public override string ToString()
        {
            var result = string.Join(" + ", _values.Select(v => v.ToString()));
            if (_operation != null) result += "." + _operation;
            return result;
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return ToString();
        }

        public void Describe(System.IO.TextWriter writer, string indent, string indentText)
        {
            if (_values != null && _values.Count > 0)
            {
                writer.WriteLine(indent + "Concatenate these values" +
                                 (_separator == null ? ":" : " separated by '" + _separator + "':"));

                var childIndent = indentText;
                foreach (var value in _values)
                    value.Describe(writer, childIndent, indentText);

                if (_operation != null)
                {
                    writer.WriteLine(indent + "Perform this operation on the result of concatenation");
                    _operation.Describe(writer, childIndent, indentText);
                }
            }
        }
    }
}
