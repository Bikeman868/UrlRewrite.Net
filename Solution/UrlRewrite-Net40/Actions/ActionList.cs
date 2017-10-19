using System.Collections.Generic;
using System.IO;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Rules;

namespace UrlRewrite.Actions
{
    /// <summary>
    /// This action executes a list of other actions. This exists to simplify rules which
    /// now only have to have a single action to execute where that action can be an ActionList
    /// </summary>
    internal class ActionList : Action, IActionList
    {
        private List<IAction> _actions;

        public IActionList Initialize(bool stopProcessing, bool endRequest)
        {
            _stopProcessing = stopProcessing;
            _endRequest = endRequest;
            _actions = null;

            return this;
        }

        public IActionList Add(IAction action)
        {
            if (action == null)
                return this;

            if (_actions == null)
                _actions = new List<IAction>();

            var actionList = action as ActionList;
            if (actionList == null)
                _actions.Add(action);
            else
            {
                if (actionList._actions != null && actionList._actions.Count > 0)
                    _actions.AddRange(actionList._actions);
            }

            return this;
        }

        public override void PerformAction(
            IRequestInfo requestInfo,
            IRuleResult ruleResult,
            out bool stopProcessing,
            out bool endRequest)
        {
            stopProcessing = _stopProcessing;
            endRequest = _endRequest;

            if (_actions != null && _actions.Count > 0)
            {
                if (requestInfo.ExecutionMode != ExecutionMode.ExecuteOnly)
                    requestInfo.Log.TraceActionListBegin(requestInfo, this);

                foreach (var action in _actions)
                {
                    bool actionStopProcessing;
                    bool actionEndRequest;
                    action.PerformAction(requestInfo, ruleResult, out actionStopProcessing, out actionEndRequest);

                    if (requestInfo.ExecutionMode != ExecutionMode.ExecuteOnly)
                        requestInfo.Log.TraceAction(requestInfo, action, actionEndRequest, actionStopProcessing);

                    if (actionEndRequest)
                        endRequest = true;

                    if (actionStopProcessing)
                    {
                        stopProcessing = true;
                        break;
                    }
                }

                if (requestInfo.ExecutionMode != ExecutionMode.ExecuteOnly)
                    requestInfo.Log.TraceActionListEnd(requestInfo, stopProcessing);
            }
        }

        public override string ToString()
        {
            var count = _actions == null ? 0 : _actions.Count;
            return "list of " + count + " actions";
        }

        public override string ToString(IRequestInfo requestInfo)
        {
            return ToString();
        }

        public override void Describe(TextWriter writer, string indent, string indentText)
        {
            if (_actions != null && _actions.Count > 0)
            {
                writer.WriteLine(indent + "Perform these actions:");
                indent += indentText;
                foreach (var action in _actions)
                    action.Describe(writer, indent, indentText);
            }
        }
    }
}
