using System.Collections.Generic;
using System.Web;
using System.Xml.Linq;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    /// <summary>
    /// This action executes a list of other actions. This exists to simplify rules which
    /// now only have to have a single action to execute where that action can be an ActionList
    /// </summary>
    internal class ActionList: Action, IAction
    {
        private List<IAction> _actions;

        public ActionList(bool stopProcessing = false, bool endRequest = false)
        {
            _stopProcessing = stopProcessing;
            _endRequest = endRequest;
        }

        public ActionList Add(IAction action)
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

        public void PerformAction(
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

        public void Initialize(XElement configuration)
        {
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return ToString();
        }
    }
}
