using System;
using System.Collections.Generic;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Rules;

namespace UrlRewrite.Actions
{
    internal class Normalize : Action, INormalizeAction
    {
        private NormalizeAction _leadingPathSeparator;
        private NormalizeAction _trailingPathSeparator;
        private List<Action<IRequestInfo>> _actions;

        public INormalizeAction Initialize(
            NormalizeAction leadingPathSeparator,
            NormalizeAction trailingPathSeparator)
        {
            _leadingPathSeparator = leadingPathSeparator;
            _trailingPathSeparator = trailingPathSeparator;

            _actions = new List<Action<IRequestInfo>>();

            switch (leadingPathSeparator)
            {
                case NormalizeAction.Add:
                    _actions.Add(r => 
                    {
                        if (r.NewPath[0].Length > 0 || r.NewPath.Count < 2)
                        {
                            r.NewPath.Insert(0, string.Empty);
                            r.PathChanged();
                        }
                    });
                    break;
                case NormalizeAction.Remove:
                    _actions.Add(r => 
                    {
                        while (r.NewPath.Count > 1 && r.NewPath[0].Length == 0)
                        {
                            r.NewPath.RemoveAt(0);
                            r.PathChanged();
                        }
                    });
                    break;
            }
            switch (trailingPathSeparator)
            {
                case NormalizeAction.Add:
                    _actions.Add(r => 
                    {
                        if (r.NewPath[r.NewPath.Count - 1].Length > 0 || r.NewPath.Count < 2)
                        {
                            r.NewPath.Add(string.Empty);
                            r.PathChanged();
                        }
                    });
                    break;
                case NormalizeAction.Remove:
                    _actions.Add(r => 
                    {
                        while (r.NewPath.Count > 1 && r.NewPath[r.NewPath.Count - 1].Length == 0)
                        {
                            r.NewPath.RemoveAt(r.NewPath.Count - 1);
                            r.PathChanged();
                        }
                    });
                    break;
            }

            return this;
        }

        public override void PerformAction(
            IRequestInfo requestInfo,
            IRuleResult ruleResult,
            out bool stopProcessing,
            out bool endRequest)
        {
            foreach (var action in _actions) action(requestInfo);

            stopProcessing = _stopProcessing;
            endRequest = _endRequest;
        }

        public override string ToString()
        {
            var text = "";
            switch (_leadingPathSeparator)
            {
                case NormalizeAction.Add:
                    text += "Add leading path separator. ";
                    break;
                case NormalizeAction.Remove:
                    text += "Remove leading path separator. ";
                    break;
            }
            switch (_trailingPathSeparator)
            {
                case NormalizeAction.Add:
                    text += "Add trailing path separator. ";
                    break;
                case NormalizeAction.Remove:
                    text += "Remove trailing path separator. ";
                    break;
            }
            return text.Length == 0 ? "No normalization" : text;
        }

        public override string ToString(IRequestInfo request)
        {
            return ToString();
        }
    }
}
