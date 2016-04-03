using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    /// <summary>
    /// This action replaces part of the URL from a static value defined in the rules
    /// </summary>
    internal class Replace : ReplaceBase, IAction
    {
        private readonly string _value;
        private readonly IList<string> _path;
        private readonly IDictionary<string, IList<string>> _queryString;

        /// <summary>
        /// Constructs a new URL replacement action
        /// </summary>
        /// <param name="scope">The part of the URL to replace</param>
        /// <param name="value">The static value to put into the part of the URL</param>
        public Replace(Scope scope, string value) :base(scope)
        {
            _value = value;
            switch (scope)
            {
                case Scope.NewUrl:
                    {
                        var query = value == null ? -1 : value.IndexOf('?');
                        if (query < 0)
                        {
                            _path = ParsePath(value);
                            _queryString = ParseQueryString(null);
                        }
                        else
                        {
                            _path = ParsePath(value.Substring(0, query));
                            _queryString = ParseQueryString(value.Substring(query + 1));
                        }
                    }
                    break;
                case Scope.NewPath:
                    _path = ParsePath(value);
                    break;
                case Scope.NewQueryString:
                    _queryString = ParseQueryString(value);
                    break;
            }
        }

        protected override void GetValues(
            IRequestInfo request, 
            Scope scope, 
            out IList<string> path, 
            out IDictionary<string, IList<string>> queryString)
        {
            path = _path;
            queryString = _queryString;
        }

        public override string ToString()
        {
            return "replace " + _scope + " with '" + _value + "'";
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return ToString();
        }
    }
}
