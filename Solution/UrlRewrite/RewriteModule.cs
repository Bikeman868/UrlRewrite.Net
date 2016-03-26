using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using UrlRewrite.Configuration;
using UrlRewrite.Interfaces;
using UrlRewrite.Rules;

namespace UrlRewrite
{
    public class RewriteModule: IHttpModule, IDisposable
    {
        private static IRule _rootRule;
        private static ILog _log;
        private static SortedList<string, string> _requestUrlsToTrace;
        private static SortedList<string, string> _rewrittenUrlsToTrace;
        private static bool _tracingEnabled;

        private bool _firstRequest = true;

        /// <summary>
        /// The host application must call this to integrate the DI framework
        /// </summary>
        public static void Initialize(
            IFactory factory, 
            XElement rules,
            SortedList<string, string> requestUrlsToTrace = null,
            SortedList<string, string> rewrittenUrlsToTrace = null)
        {
            if (_log == null)
                _log = factory.Create<ILog>();

            var parser = new RuleParser();
            _rootRule = parser.Parse(rules);

            _requestUrlsToTrace = requestUrlsToTrace;
            _rewrittenUrlsToTrace = rewrittenUrlsToTrace;

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

            var application = (HttpApplication) source;
            var context = application.Context;

            try
            {
                var requestInfo = new RequestInfo(application, _log);

                if (_tracingEnabled && _requestUrlsToTrace != null && _requestUrlsToTrace.Count > 0)
                    requestInfo.TraceRequest = _requestUrlsToTrace.ContainsKey(context.Request.RawUrl);

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
                    if (action.EndRequest) endRequest = true;
                    if (stopProcessing)
                        break;
                }

                if (_tracingEnabled && !requestInfo.TraceRequest)
                {
                    var newPath = "/";
                    if (requestInfo.NewPath != null && requestInfo.NewPath.Count > 0)
                        newPath += string.Join("/", requestInfo.NewPath);

                    if (_rewrittenUrlsToTrace.ContainsKey(newPath))
                    {
                        requestInfo.TraceRequest = true;
                        _rootRule.Evaluate(requestInfo);
                        if (executedActions != null)
                        {
                            foreach (var action in executedActions)
                            {
                                requestInfo.Log.TraceAction(action);
                            }
                        }
                    }
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
                Log = log.GetRequestLog(application, application.Context);

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
        }
    }
}
