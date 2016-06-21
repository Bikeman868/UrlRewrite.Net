using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using UrlRewrite.Configuration;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Rules;
using UrlRewrite.Interfaces.Utilities;
using UrlRewrite.Utilities;

namespace UrlRewrite
{
    public class RewriteModule: IHttpModule, IDisposable
    {
        private static ILog _log;
        private static IFactory _factory;
        private static IRuleList _rules;

#if !TRACE_ALL
        private static bool _tracingEnabled;
        private static Func<string, bool> _forwardTracePredicate;
        private static Func<string, bool> _reverseTracePredicate;
#endif
        /// <summary>
        /// The host application must call this or no rewriting will take place
        /// </summary>
        /// <param name="log">Optional logger or null for logging to Trace output</param>
        /// <param name="forwardTracePredicate">A function that determines which urls to 
        /// log traces for. If you have a specific URL that is not rewriting as you want then
        /// add that url to this list to log an execution trace through the rewriting rules</param>
        /// <param name="reverseTracePredicate">A list of the rewritten urls
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
            Func<string, bool> forwardTracePredicate = null,
            Func<string, bool> reverseTracePredicate = null,
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
            try
            {
                SetRules(LoadRules(ruleStream));
            }
            finally
            {
                ruleStream.Close();
            }
            DescribeRulesToTrace();

#if !TRACE_ALL
            _forwardTracePredicate = forwardTracePredicate;
            _reverseTracePredicate = reverseTracePredicate;
            _tracingEnabled = forwardTracePredicate != null || reverseTracePredicate != null;
#endif
        }

        /// <summary>
        /// Takes a set of rules and writes a description of them into a stream
        /// </summary>
        /// <param name="rules">The rules to describe</param>
        /// <param name="stream">The stream to write the description into</param>
        /// <param name="encoding">The encoding to use when writing text to the stream</param>
        public static void DescribeRules(
            IRuleList rules, 
            Stream stream, 
            Encoding encoding)
        {
            var writer = new StreamWriter(stream, encoding);
            if (rules == null)
                writer.Write("There is no spoon");
            else
                rules.Describe(writer, "", "  ");
            writer.Flush();

            // Note, do not dispose or close the writer unless it owns the stream.
        }

        public static void DescribeRulesToTrace()
        {
            var encoding = Encoding.UTF8;
            using (var stream = new MemoryStream())
            {
                DescribeRules(_rules, stream, encoding);
                var description = encoding.GetString(stream.GetBuffer(), 0, (int)stream.Length);
                Trace.WriteLine(description);
            }
        }

        /// <summary>
        /// Parses rules from a stream
        /// </summary>
        /// <param name="stream">The stream to read rules from</param>
        /// <param name="encoding">The text encoding used in this stream, or null for UTF8</param>
        /// <param name="ruleParser">A custom parser, or null to use the default parser</param>
        /// <returns></returns>
        public static IRuleList LoadRules(
            Stream stream, 
            Encoding encoding = null, 
            IRuleParser ruleParser = null)
        {
            if (stream == null)
                return null;

            encoding = encoding ?? Encoding.UTF8;
            var parser = ruleParser ?? _factory.Create<StandardRuleParser>();
            return parser.Parse(stream, encoding);
        }

        public static void SetRules(IRuleList rules)
        {
            _rules = rules;
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
                    && _forwardTracePredicate != null
                    && _forwardTracePredicate(context.Request.RawUrl))
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
                    && _reverseTracePredicate != null)
                {
                    var newPath = "/";
                    if (requestInfo.NewPath != null && requestInfo.NewPath.Count > 0)
                        newPath = string.Join("/", requestInfo.NewPath).ToLower();

                    if (_reverseTracePredicate(newPath))
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
