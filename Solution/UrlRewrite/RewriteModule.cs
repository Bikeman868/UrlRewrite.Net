using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using UrlRewrite.Configuration;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Rules;
using UrlRewrite.Interfaces.Utilities;
using UrlRewrite.Utilities;

namespace UrlRewrite
{
    public class RewriteModule: IHttpModule, IDisposable
    {
        private static IRuleList _rules;
        private static ILog _log;
        private static IFactory _factory;

#if !TRACE_ALL
        private static SortedList<string, string> _requestUrlsToTrace;
        private static SortedList<string, string> _rewrittenUrlsToTrace;
        private static bool _tracingEnabled;
#endif
        /// <summary>
        /// The host application must call this or no rewriting will take place
        /// </summary>
        /// <param name="log">Optional logger or null for logging to Trace output</param>
        /// <param name="requestUrlsToTrace">A list of the urls to log traces for. 
        /// If you have a specific URL that is not rewriting as you want then
        /// add that url to this list to log an execution trace through the 
        /// rewriting rules</param>
        /// <param name="rewrittenUrlsToTrace">A list of the rewritten urls
        /// to log traces for. If your site it redirecting to an unexpected
        /// page and you want to know why it was redirected there, add the
        /// rewritten/redirected url to this list</param>
        /// <param name="factory">Pass a factory that can construct your
        /// custom actions and custom conditions using Dependency Injection. You
        /// can pass null if all of your custom extensions have a default public
        /// constructor</param>
        /// <param name="ruleStream">Pass the rewriting rules here. This allows you
        /// to store your rules wherever you want (in a database for example).
        /// You can also pass null to read rles from the RewriteRules.config
        /// file just like the Microsoft one does</param>
        /// <param name="ruleParser">Provide a custom parser to support your
        /// own rule syntax, or pass null for backward compatibility with
        /// the Microsoft IIS Rewriter module but with many more features</param>
        public static void Initialize(
            ILog log = null,
            List<string> requestUrlsToTrace = null,
            List<string> rewrittenUrlsToTrace = null,
            IFactory factory = null, 
            Stream ruleStream = null,
            IRuleParser ruleParser = null)
        {
            if (_factory == null)
            {
                if (factory == null)
                    factory = new NinjectFactory();
                _factory = factory;
            }

            if (_log == null)
            {
                if (log == null)
                    log = _factory.Create<ILog>();
                _log = log;
            }

            if (ruleStream == null)
            {
                var filePath = HttpContext.Current.Server.MapPath("~/RewriteRules.config");
                ruleStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            var parser = ruleParser ?? _factory.Create<StandardRuleParser>();
            _rules = parser.Parse(ruleStream);
            ruleStream.Close();

            var rulesDescriptionStream = new MemoryStream();
            using (var rulesDescriptionWriter = new StreamWriter(rulesDescriptionStream))
            {
                _rules.Describe(rulesDescriptionWriter, "", "  ");
                rulesDescriptionWriter.Flush();
                var description = Encoding.ASCII.GetString(
                    rulesDescriptionStream.GetBuffer(), 
                    0,
                    (int) rulesDescriptionStream.Length);
                Trace.WriteLine(description);
            }

#if !TRACE_ALL
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

            var application = (HttpApplication) source;
            var context = application.Context;
            var requestInfo = _factory.Create<IRequestInfo>().Initialize(application, _log);

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
                else if (requestInfo.UrlIsModified)
                    requestInfo.Context.RewritePath(requestInfo.NewPathString, "", requestInfo.NewParametersString);
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                _log.LogException(requestInfo, ex);
            }
        }
        
    }
}
