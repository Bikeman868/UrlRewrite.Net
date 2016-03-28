#define TRACE_ALL

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using UrlRewrite.Configuration;
using UrlRewrite.Interfaces;
using UrlRewrite.Utilities;

namespace UrlRewrite
{
    public class RewriteModule: IHttpModule, IDisposable, ILog
    {
        private static IRuleList _rules;
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
            _rules = parser.Parse(rules);

#if TRACE_ALL
            _tracingEnabled = true;
#else
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
#endif
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
            var rules = _rules;
            if (rules == null) return;

            var log = _log ?? this;

            var application = (HttpApplication) source;
            var context = application.Context;
            var requestInfo = new RequestInfo(application, log);

            try
            {

#if !TRACE_ALL
                if (_tracingEnabled
                    && _requestUrlsToTrace != null
                    && _requestUrlsToTrace.Count > 0
                    && _requestUrlsToTrace.ContainsKey(context.Request.RawUrl))
                {
#endif
                    requestInfo.ExecutionMode = ExecutionMode.ExecuteAndTrace;
                    requestInfo.Log.TraceRequestBegin(requestInfo);
#if !TRACE_ALL
                }
#endif
                
                var ruleListResult = _rules.Evaluate(requestInfo);

#if !TRACE_ALL
                if (_tracingEnabled
                    && requestInfo.ExecutionMode == ExecutionMode.ExecuteOnly
                    && _rewrittenUrlsToTrace != null
                    && _rewrittenUrlsToTrace.Count > 0)
                {
                    var newPath = "/";
                    if (requestInfo.NewPath != null && requestInfo.NewPath.Count > 0)
                        newPath = string.Join("/", requestInfo.NewPath).ToLower();

                    if (_rewrittenUrlsToTrace.ContainsKey(newPath))
                    {
                        requestInfo.ExecutionMode = ExecutionMode.TraceOnly;
                        requestInfo.Log.TraceRequestBegin(requestInfo);
                        _rules.Evaluate(requestInfo);
                    }
                }
#endif

                if (requestInfo.ExecutionMode != ExecutionMode.ExecuteOnly)
                    requestInfo.Log.TraceRequestEnd(requestInfo);

                requestInfo.ExecuteDeferredActions();

                if (ruleListResult.EndRequest)
                    application.CompleteRequest();
            }
            catch (ThreadAbortException)
            {
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

        private class RequestInfo : IRequestInfo
        {
            public HttpApplication Application { get; private set; }
            public HttpContext Context { get; private set; }
            public ExecutionMode ExecutionMode { get; set; }

            private readonly ILog _log;
            private IRequestLog _requestLog;

            public IRequestLog Log
            {
                get
                {
                    if (ReferenceEquals(_requestLog, null))
                        _requestLog = _log.GetRequestLog(Application, Context) ?? new RequestLog();
                    return _requestLog;
                }
            }

            private IList<System.Action<IRequestInfo>> _deferredActions;

            public IList<System.Action<IRequestInfo>> DeferredActions
            {
                get
                {
                    if (ReferenceEquals(_deferredActions, null))
                        _deferredActions = new List<System.Action<IRequestInfo>>();
                    return _deferredActions;
                }
            }

            private int? _queryPos;

            public int QueryPos
            {
                get
                {
                    if (!_queryPos.HasValue)
                    {
                        var request = Context.Request;
                        _queryPos = request.RawUrl.IndexOf('?');
                    }
                    return _queryPos.Value;
                }
            }

            private string _originalPathString;

            public string OriginalPathString
            {
                get
                {
                    if (ReferenceEquals(_originalPathString, null))
                    {
                        _originalPathString = QueryPos < 0
                            ? Context.Request.RawUrl
                            : Context.Request.RawUrl.Substring(0, QueryPos);
                    }
                    return _originalPathString;
                }
            }

            private List<string> _originalPath;

            public List<string> OriginalPath
            {
                get
                {
                    if (ReferenceEquals(_originalPath, null))
                    {
                        _originalPath = OriginalPathString
                            .Split('/')
                            .Where(e => !string.IsNullOrEmpty(e))
                            .ToList();
                        if (OriginalPathString.StartsWith("/"))
                            _originalPath.Insert(0, "");
                    }
                    return _originalPath;
                }
            }

            private List<string> _newPath;

            public List<string> NewPath
            {
                get
                {
                    if (ReferenceEquals(_newPath, null))
                        _newPath = OriginalPath.ToList();
                    return _newPath;
                }
                set { _newPath = value; }
            }

            private string _originalParametersString;

            public string OriginalParametersString
            {
                get
                {
                    if (ReferenceEquals(_originalParametersString, null))
                    {
                        _originalParametersString = QueryPos < 0
                            ? ""
                            : Context.Request.RawUrl.Substring(QueryPos + 1);
                    }
                    return _originalParametersString;
                }
            }

            private Dictionary<string, List<string>> _originalParameters;
            private Dictionary<string, List<string>> _newParameters;

            private void ParseParameters()
            {
                var parameters = OriginalParametersString
                    .Split('&')
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList();

                var originalParameters = new Dictionary<string, List<string>>();
                var newParameters = new Dictionary<string, List<string>>();

                foreach (var parameter in parameters)
                {
                    string key;
                    string value = null;
                    var equalsPos = parameter.IndexOf('=');
                    if (equalsPos < 0)
                    {
                        key = parameter.ToLower();
                    }
                    else
                    {
                        key = parameter.Substring(0, equalsPos).ToLower();
                        value = parameter.Substring(equalsPos + 1);
                    }

                    List<string> values;
                    if (originalParameters.TryGetValue(key, out values))
                    {
                        values.Add(value);
                        newParameters[key].Add(value);
                    }
                    else
                    {
                        originalParameters.Add(key, new List<string> {value});
                        newParameters.Add(key, new List<string> {value});
                    }
                }

                _originalParameters = originalParameters;
                _newParameters = newParameters;
            }

            public Dictionary<string, List<string>> OriginalParameters
            {
                get
                {
                    if (ReferenceEquals(_originalParameters, null))
                        ParseParameters();
                    return _originalParameters;
                }
            }

            public Dictionary<string, List<string>> NewParameters
            {
                get
                {
                    if (ReferenceEquals(_newParameters, null))
                        ParseParameters();
                    return _newParameters;
                }
                set { _newParameters = value; }
            }

            public RequestInfo(
                HttpApplication application,
                ILog log)
            {
                Application = application;
                Context = application.Context;
                _log = log;
                ExecutionMode = ExecutionMode.ExecuteOnly;
            }

            public void ExecuteDeferredActions()
            {
                if (ReferenceEquals(_deferredActions, null)) return;
                foreach (var action in _deferredActions)
                    action(this);
            }
        }

        private class RequestLog: IRequestLog
        {
            private readonly PerformanceTimer _timer = new PerformanceTimer();
            private readonly List<string> _output = new List<string>();

            private void Output(params string[] message)
            {
                var line = string.Format("Rewrite: {0,6}mS {1}",
                    _timer.ElapsedMilliSeconds.ToString("F1"),
                    string.Join(" ", message));
                _output.Add(line);
            }
         
            void IRequestLog.LogException(IRequestInfo request, Exception ex)
            {
                Output("Exception", ex.Message);
            }

            void IRequestLog.LogWarning(IRequestInfo request, string message)
            {
                Output("Warning", message);
            }

            void IRequestLog.TraceRequestBegin(IRequestInfo request)
            {
                Output("rewriting URL", request.Context.Request.RawUrl);
                _timer.Start();
            }

            public void TraceRequestEnd(IRequestInfo request)
            {
                _timer.Stop();
                Output("finished URL", request.Context.Request.RawUrl);

                Trace.WriteLine("--");
                foreach (var line in _output) Trace.WriteLine(line);
            }

            void IRequestLog.TraceRuleListBegin(IRequestInfo request, IRuleList ruleList)
            {
                Output("begin", ruleList.ToString(request));
            }

            void IRequestLog.TraceRuleListEnd(IRequestInfo request, IRuleList ruleList, bool matched, IRuleListResult ruleListResult)
            {
                Output(
                    ruleList.ToString(request),
                    (matched ? "was executed." : "does not match this request."),
                    (ruleListResult.RuleResults != null && ruleListResult.RuleResults.Count > 0 ? ruleListResult.RuleResults.Count + " rules evaluated." : ""),
                    (ruleListResult.StopProcessing ? "Stop processing." : ""),
                    (ruleListResult.EndRequest ? "End request." : ""));
            }

            void IRequestLog.TraceRuleBegin(IRequestInfo request, IRule rule)
            {
                Output("begin " + rule.ToString(request));
            }

            void IRequestLog.TraceRuleEnd(IRequestInfo request, IRule rule, bool matched, IRuleResult ruleResult)
            {
                Output(
                    rule.ToString(request),
                    (matched ? "was executed." : "does not match this request."),
                    (ruleResult.StopProcessing ? "Stop processing." : ""),
                    (ruleResult.EndRequest ? "End request." : ""));
            }

            void IRequestLog.TraceConditionListBegin(IRequestInfo request, CombinationLogic logic)
            {
                Output("list of conditions where " + logic);
            }

            void IRequestLog.TraceConditionListEnd(IRequestInfo request, bool conditionsMet)
            {
                Output("list of conditions evaluated to " + conditionsMet);
            }

            void IRequestLog.TraceCondition(IRequestInfo request, ICondition condition, bool isTrue)
            {
                Output(
                    "contition",
                    condition.ToString(request),
                    (isTrue ? "is true" : "is false"));
            }

            void IRequestLog.TraceAction(IRequestInfo request, IAction action, bool endRequest, bool stopProcessing)
            {
                Output("action", action.ToString(request));
            }

            void IRequestLog.TraceActionListBegin(IRequestInfo request, IAction actionList)
            {
                Output("start", actionList.ToString(request));
            }

            void IRequestLog.TraceActionListEnd(IRequestInfo request, bool stopProcessing)
            {
                Output("finished list of actions.", (stopProcessing ? "Stop processing" : ""));
            }
        }

    }
}
