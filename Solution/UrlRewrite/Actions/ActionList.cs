using System.Collections.Generic;
using System.Web;
using UrlRewrite.Interfaces;

namespace UrlRewrite.Actions
{
    internal class ActionList: Action, IAction
    {
        private List<IAction> _actions;

        public ActionList(bool stopProcessing = false, bool endRequest = false)
        {
            _stopProcessing = stopProcessing;
            _endRequest = endRequest;
            _actions = new List<IAction>();
        }

        public ActionList Add(IAction action)
        {
            if (_actions == null)
                _actions = new List<IAction>();

            _actions.Add(action);

            return this;
        }

        public bool PerformAction(IRequestInfo requestInfo)
        {
            if (_actions != null && _actions.Count > 0)
            {
                foreach (var action in _actions)
                {
                    var stop = action.PerformAction(requestInfo);
                    if (stop) return true;
                }
            }
            return StopProcessing;
        }
    }
}
