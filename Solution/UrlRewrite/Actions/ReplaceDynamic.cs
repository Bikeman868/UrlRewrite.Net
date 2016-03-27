using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    /// <summary>
    /// This action replaces part of the URL from a dynamic value obtained from
    /// some other part of the request.
    /// </summary>
    internal class ReplaceDynamic : ReplaceBase, IAction
    {
        private readonly Func<IRequestInfo, string> _getValue;

        /// <summary>
        /// Replaces part of the URL using a value dynamically obtained from the request
        /// </summary>
        /// <param name="scope">The part of the URL to replace</param>
        /// <param name="getValue">A lambda expression that gets the value from the request</param>
        public ReplaceDynamic(Scope scope, Func<IRequestInfo, string> getValue)
            : base(scope)
        {
            _getValue = getValue;
        }

        protected override void GetValues(
            IRequestInfo request, 
            Scope scope, 
            out List<string> path, 
            out Dictionary<string, List<string>> queryString)
        {
            var value = _getValue(request);

            switch (scope)
            {
                case Scope.Url:
                    {
                        var query = value == null ? -1 : value.IndexOf('?');
                        if (query < 0)
                        {
                            path = ParsePath(value);
                            queryString = ParseQueryString(null);
                        }
                        else
                        {
                            path = ParsePath(value.Substring(0, query));
                            queryString = ParseQueryString(value.Substring(query + 1));
                        }
                    }
                    break;
                case Scope.Path:
                    path = ParsePath(value);
                    queryString = null;
                    break;
                case Scope.QueryString:
                    path = null;
                    queryString = ParseQueryString(value);
                    break;
                default:
                    path = null;
                    queryString = null;
                    break;
            }
        }

        public override string ToString()
        {
            return "Replace " + _scope + " by evaluating a function";
        }

        public void Initialize(XElement configuration)
        {
        }

        public string ToString(IRequestInfo request)
        {
            var value = _getValue(request);
            return "replace " + _scope + " with '" + value + "'";
        }
    }
}
