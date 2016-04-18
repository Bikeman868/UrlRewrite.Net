using System;
using System.Collections.Generic;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Rules;
using UrlRewrite.Utilities;

namespace UrlRewrite.Actions
{
    internal class Insert : Action, IInsertAction
    {
        private Scope _scope;
        private string _scopeIndex;
        private int _scopeIndexValue;
        private IValueGetter _valueGetter;
        private Action<IRequestInfo, string> _action;

        public IInsertAction Initialize(Scope scope, string scopeIndex, IValueGetter valueGetter)
        {
            _scope = scope;
            _scopeIndex = scopeIndex;
            _valueGetter = valueGetter;

            if (string.IsNullOrEmpty(scopeIndex))
                throw new UrlRewriteException("When inserting into the path the index of the element to insert before must be provided");
            
            if (scope != Scope.Path)
                throw new UrlRewriteException("You can only insert into the path scope");

            if (!int.TryParse(scopeIndex, out _scopeIndexValue))
                throw new UrlRewriteException("The index of the path element to insert must be a number");

            if (_scopeIndexValue == 0)
                _action = (requestInfo, value) => requestInfo.NewPathString = value + "/" + requestInfo.NewPathString;

            else if (_scopeIndexValue > 0)
            {
                _action = (requestInfo, value) =>
                {
                    if (_scopeIndexValue < requestInfo.NewPath.Count)
                    {
                        var newPath = new List<string>();
                        for (var i = 0; i < _scopeIndexValue; i++)
                            newPath.Add(requestInfo.NewPath[i]);
                        newPath.Add(value);
                        for (var i = _scopeIndexValue; i < requestInfo.NewPath.Count; i++)
                            newPath.Add(requestInfo.NewPath[i]);
                        requestInfo.NewPath = newPath;
                    }
                };
            }

            else
            {
                _action = (requestInfo, value) =>
                {
                    var index = requestInfo.NewPath.Count + _scopeIndexValue;
                    if (index >= 0)
                    {
                        var newPath = new List<string>();
                        for (var i = 0; i < index; i++)
                            newPath.Add(requestInfo.NewPath[i]);
                        newPath.Add(value);
                        for (var i = index; i < requestInfo.NewPath.Count; i++)
                            newPath.Add(requestInfo.NewPath[i]);
                        requestInfo.NewPath = newPath;
                    }
                };
            }

            return this;
        }

        public override void PerformAction(
            IRequestInfo requestInfo,
            IRuleResult ruleResult,
            out bool stopProcessing,
            out bool endRequest)
        {
            var value = _valueGetter.GetString(requestInfo, ruleResult);
            _action(requestInfo, value);

            stopProcessing = _stopProcessing;
            endRequest = _endRequest;
        }

        public override string ToString()
        {
            var text = "Insert " + _valueGetter + " into " + _scope;
            if (!string.IsNullOrEmpty(_scopeIndex))
                text += " before " + _scopeIndex;
            return text;
        }

        public override string ToString(IRequestInfo request)
        {
            var text = "insert " + _valueGetter + " into " + _scope;
            if (!string.IsNullOrEmpty(_scopeIndex))
                text += " before " + _scopeIndex;
            return text;
        }
    }
}
