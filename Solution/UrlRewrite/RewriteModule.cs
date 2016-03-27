using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using UrlRewrite.Actions;
using UrlRewrite.Configuration;
using UrlRewrite.Interfaces;
using UrlRewrite.Rules;

namespace UrlRewrite
{
    public class RewriteModule: IHttpModule, IDisposable, ILog
    {
        private static IRule _rootRule;
        private static ILog _log;
        private static SortedList<string, string> _requestUrlsToTrace;
        private static SortedList<string, string> _rewrittenUrlsToTrace;
        private static bool _tracingEnabled;

        /// <summary>
        /// The host application must call this or no rewriting will take place
        /// </summary>
        /// <param name="log">Optional logger or null for no logging</param>
        /// <param name="requestUrlsToTrace">A list of the urls to log traces for. 
        /// If you have a specific URL that is not rewriting as you want then
        /// add the lower case version of that url to this list to log an
        /// execution trace through the rewriting rules</param>
        /// <param name="rewrittenUrlsToTrace">A list of the rewritten urls
        /// to log traces for. If your site it redirecting to an unexpected
        /// page and you want to know why it was redirected there, add the
        /// lower case version of the rewritten/redirected url to this list</param>
        /// <param name="factory">Pass a factory that can construct your
        /// custom actions and custom conditions using Dependency Injection. You
        /// can pass null if all of your custom extensions have a default public
        /// constructor</param>
        /// <param name="rules">Pass the rewriting rules here. This allows you
        /// to store your rules wherever you want (in a database for example).
        /// You can also pass null to read rles from the RewriteRules.config
        /// file just like the Microsoft one does</param>
        public static void Initialize(
            ILog log = null,
            List<string> requestUrlsToTrace = null,
            List<string> rewrittenUrlsToTrace = null,
            IFactory factory = null, 
            XElement rules = null)
        {
            if (_log == null)
            {
                if (log == null && factory != null)
                    log = factory.Create<ILog>();
                _log = log;
            }

            // TODO: when rules are null load from RewriteRules.config
            var parser = new RuleParser(factory);
            _rootRule = parser.Parse(rules);

            if (requestUrlsToTrace != null)
            {
                _requestUrlsToTrace = new SortedList<string, string>();
                foreach (var url in requestUrlsToTrace)
                    _requestUrlsToTrace.Add(url.ToLower(), url);
            }

            if (rewrittenUrlsToTrace != null)
            {
                _rewrittenUrlsToTrace = new SortedList<string, string>();
                foreach (var url in rewrittenUrlsToTrace)
                    _rewrittenUrlsToTrace.Add(url.ToLower(), url);
            }

            _tracingEnabled =
                (requestUrlsToTrace != null && requestUrlsToTrace.Count > 0) ||
                (rewrittenUrlsToTrace != null && rewrittenUrlsToTrace.Count > 0);
        }

        public void Init(HttpApplication application)
        {
            application.BeginRequest += OnBeginRequest;
        }

        public void Dispose()
        {
        }

        private void OnBeginRequest(object source, EventArgs args)
        {
            var rootRule = _rootRule;
            if (rootRule == null) return;

            var log = _log ?? this;

            var application = (HttpApplication) source;
            var context = application.Context;
            var requestInfo = new RequestInfo(application, log);

            try
            {

                if (_tracingEnabled && _requestUrlsToTrace != null && _requestUrlsToTrace.Count > 0)
                    requestInfo.TraceRequest = _requestUrlsToTrace.ContainsKey(context.Request.RawUrl);

                if (requestInfo.TraceRequest)
                    requestInfo.Log.TraceRequestBegin(requestInfo);

                IList<IAction> executedActions = null;

                var ruleResult = _rootRule.Evaluate(requestInfo);
                if (ruleResult == null || ruleResult.Actions == null || ruleResult.Actions.Count == 0) 
                    return;

                var endRequest = false;
                foreach (var action in ruleResult.Actions)
                {
                    if (_tracingEnabled)
                    {
                        if (executedActions == null)
                            executedActions = new List<IAction>();
                        executedActions.Add(action);
                    }

                    var stopProcessing = action.PerformAction(requestInfo);

                    if (requestInfo.TraceRequest && (action as ActionList == null))
                        requestInfo.Log.TraceAction(requestInfo, action, action.EndRequest, stopProcessing);

                    if (action.EndRequest) endRequest = true;
                    if (stopProcessing)
                        break;
                }

                if (_tracingEnabled && !requestInfo.TraceRequest)
                {
                    var newPath = "/";
                    if (requestInfo.NewPath != null && requestInfo.NewPath.Count > 0)
                        newPath = string.Join("/", requestInfo.NewPath).ToLower();

                    if (_rewrittenUrlsToTrace.ContainsKey(newPath))
                    {
                        requestInfo.TraceRequest = true;
                        requestInfo.Log.TraceRequestBegin(requestInfo);
                        _rootRule.Evaluate(requestInfo);
                        if (executedActions != null)
                        {
                            foreach (var action in executedActions)
                            {
                                requestInfo.Log.TraceAction(requestInfo, action, false, false);
                            }
                        }
                    }
                }
                
                if (endRequest)
                    application.CompleteRequest();
            }
            catch (Exception ex)
            {
                log.LogException(requestInfo, ex);
            }
        }
        
        #region Dummy ILog implementation

        void ILog.LogException(IRequestInfo request, Exception ex)
        {
            Trace.WriteLine("Exception in rewriter module: " + ex.Message);
        }

        IRequestLog ILog.GetRequestLog(HttpApplication application, HttpContext context)
        {
            return null;
        }

        #endregion

        private class RequestInfo: IRequestInfo, IRequestLog
        {
            public HttpApplication Application { get; private set; }
            public HttpContext Context { get; private set; }
            public IRequestLog Log { get; private set; }
            public List<string> OriginalPath { get; private set; }
            public Dictionary<string, List<string>> OriginalQueryString { get; private set; }

            public bool TraceRequest { get; set; }
            public List<string> NewPath { get; set; }
            public Dictionary<string, List<string>> NewQueryString { get; set; }

            public RequestInfo(
                HttpApplication application,
                ILog log)
            {
                Application = application;
                Context = application.Context;
                Log = log.GetRequestLog(application, application.Context) ?? this;

                // TODO: improve performance by making the rest of this lazy

                var request = application.Context.Request;

                OriginalPath = request.Path
                    .Split('/')
                    .Where(e => !string.IsNullOrEmpty(e))
                    .ToList();
                NewPath = OriginalPath.ToList();

                OriginalQueryString = new Dictionary<string, List<string>>();
                NewQueryString = new Dictionary<string, List<string>>();

                foreach (var key in request.QueryString.AllKeys)
                {
                    var value = request.QueryString[key];
                    List<string> originalValues = null;
                    List<string> newValues = null;

                    if (value != null)
                    {
                        var values = value.Split(',');
                        originalValues = values.ToList();
                        newValues = values.ToList();
                    }
                    OriginalQueryString.Add(key, originalValues);
                    NewQueryString.Add(key, newValues);
                }
            }

            #region IRequestLog

            void IRequestLog.LogException(IRequestInfo request, Exception ex)
            {
                Trace.WriteLine("Exception processing request in rewrite module. " + ex.Message);
            }

            void IRequestLog.LogWarning(IRequestInfo request, string message)
            {
                Trace.WriteLine("Warning from rewrite module. " + message);
            }

            void IRequestLog.TraceRequestBegin(IRequestInfo request)
            {
                Trace.WriteLine("Rewrite: executing rules for " + request.Context.Request.RawUrl);
            }
         
            void IRequestLog.TraceRuleBegin(IRequestInfo request, IRule rule)
            {
                Trace.WriteLine("Rewrite: begin " + rule.ToString(request));
            }

            void IRequestLog.TraceRuleEnd(IRequestInfo request, IRule rule, bool matched, bool stopProcessing)
            {
                Trace.WriteLine(
                    "Rewrite: rule " + rule.ToString(request) + " was" 
                    + (matched ? " matched" : " not matched") 
                    + (stopProcessing ? ", stop processing" : ""));
            }

            void IRequestLog.TraceConditionListBegin(IRequestInfo request, CombinationLogic logic)
            {
                Trace.WriteLine("Rewrite: list of conditions where " + logic);
            }

            void IRequestLog.TraceConditionListEnd(IRequestInfo request, bool conditionsMet)
            {
                Trace.WriteLine(
                    "Rewrite: list of conditions was" 
                    + (conditionsMet ? " met" : " not met"));
            }

            void IRequestLog.TraceCondition(IRequestInfo request, ICondition condition, bool isTrue)
            {
                Trace.WriteLine(
                    "Rewrite: contition " 
                    + condition.ToString(request)
                    + (isTrue ? " is true" : " is false"));
            }

            void IRequestLog.TraceAction(IRequestInfo request, IAction action, bool endRequest, bool stopProcessing)
            {
                Trace.WriteLine("Rewrite: action " + action.ToString(request));
            }

            void IRequestLog.TraceActionListBegin(IRequestInfo request, IAction actionList)
            {
                Trace.WriteLine("Rewrite: start " + actionList.ToString(request));
            }

            void IRequestLog.TraceActionListEnd(IRequestInfo request, bool stopProcessing)
            {
                Trace.WriteLine("Rewrite: finished list of actions" + (stopProcessing ? ", stop processing" : ""));
            }

            #endregion
        }

    }
}
