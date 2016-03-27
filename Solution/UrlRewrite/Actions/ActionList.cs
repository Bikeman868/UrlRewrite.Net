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
                if (requestInfo.TraceRequest)
                    requestInfo.Log.TraceActionListBegin(requestInfo, this);

                foreach (var action in _actions)
                {
                    var stop = action.PerformAction(requestInfo);

                    if (requestInfo.TraceRequest)
                        requestInfo.Log.TraceAction(requestInfo, action, action.EndRequest, stop);

                    if (stop)
                    {
                        if (requestInfo.TraceRequest)
                            requestInfo.Log.TraceActionListEnd(requestInfo, true);
                        return true;
                    }
                }

                if (requestInfo.TraceRequest)
                    requestInfo.Log.TraceActionListEnd(requestInfo, StopProcessing);
            }
            return StopProcessing;
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
