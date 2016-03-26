using System;
using System.Collections.Generic;
using System.Web;
using UrlRewrite.Interfaces;

namespace UrlRewrite
{
    public class RewriteModule: IHttpModule, IDisposable
    {
        private static IRuleEngine _ruleEngine;
        private static ILog _log;

        private bool _firstRequest = true;
        private bool _tracingEnabled;
        private IRequestLog _dummyLog;
        private SortedList<string, string> _requestUrlsToTrace;
        private SortedList<string, string> _rewrittenUrlsToTrace;

        /// <summary>
        /// The host application must call this to integrate the DI framework
        /// </summary>
        public static void Initialize(IFactory factory)
        {
            if (_log == null)
                _log = factory.Create<ILog>();

            if (_ruleEngine == null)
            {
                _ruleEngine = factory.Create<RuleEngine>();

                //TODO: Load rules and initialize rule engine
                //TODO: Polulate _requestUrlsToTrace and _rewrittenUrlsToTrace and set _tracingEnabled flag
            }
        }

        public void Init(HttpApplication application)
        {
            _dummyLog = new DummyLog();

            _requestUrlsToTrace = new SortedList<string, string>();
            _rewrittenUrlsToTrace = new SortedList<string, string>();

            application.BeginRequest += OnBeginRequest;
        }

        public void Dispose()
        {
            var ruleEngine = _ruleEngine;
            _ruleEngine = null;

            if (ruleEngine != null)
                ruleEngine.Dispose();
        }

        private void OnBeginRequest(object source, EventArgs args)
        {
            var ruleEngine = _ruleEngine;
            if (ruleEngine == null) return;

            var application = (HttpApplication) source;
            var context = application.Context;

            try
            {
                var requestInfo = new RequestInfo
                {
                    Application = application,
                    Context = context
                };

                requestInfo.Log = _log == null ? _dummyLog : _log.GetRequestLog(application, context);

                if (_tracingEnabled)
                {
                    // Check if request matches _requestUrlsToTrace
                    requestInfo.TraceRequest = true;
                }

                var actions = ruleEngine.EvaluateRules(requestInfo);
                if (actions == null) return;

                var endRequest = false;
                foreach (var action in actions)
                {
                    action.PerformAction(requestInfo);
                    if (action.EndRequest) endRequest = true;
                }

                if (_tracingEnabled && !requestInfo.TraceRequest)
                {
                    // Check if request matches _rewrittenUrlsToTrace
                    requestInfo.TraceRequest = true;
                    ruleEngine.EvaluateRules(requestInfo);
                }
                
                if (endRequest)
                    application.CompleteRequest();
            }
            catch (Exception ex)
            {
                if (_log == null) throw;
                _log.LogException(ex);
            }
        }

        private class RequestInfo: IRequestInfo
        {
            public HttpApplication Application { get; set; }
            public HttpContext Context { get; set; }
            public IRequestLog Log { get; set; }
            public bool TraceRequest { get; set; }
        }

        private class DummyLog : IRequestLog
        {
            public void LogException(Exception ex) { }
            public void LogWarning(string message) { }
            public void TraceRuleBegin(string rulePath) { }
            public void TraceRuleEnd(string rulePath) { }
            public void TraceConditionListBegin(ConditionLogic logic) { }
            public void TraceConditionListEnd(bool conditionsMet) { }
            public void TraceAction(IRuleAction action) { }
            public void TraceCondition(IRuleCondition condition, bool isTrue) { }
        }
    }
}
