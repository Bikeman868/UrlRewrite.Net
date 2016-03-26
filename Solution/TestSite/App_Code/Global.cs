using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestSite.App_Code
{
    public class Global : HttpApplication, 
        UrlRewrite.Interfaces.IFactory, 
        UrlRewrite.Interfaces.ILog,
        UrlRewrite.Interfaces.IRequestLog
    {
        void Application_Start(object sender, EventArgs e)
        {
            UrlRewrite.RewriteModule.Initialize(this, null);
        }

        public T Create<T>()
        {
            return (T)Create(typeof (T));
        }

        public object Create(Type type)
        {
            if (type == typeof (UrlRewrite.Interfaces.IFactory))
                return this;
            if (type == typeof(UrlRewrite.Interfaces.ILog))
                return this;

            return type.GetConstructor(Type.EmptyTypes).Invoke(null);
        }

        public void LogException(Exception ex)
        {
        }

        public UrlRewrite.Interfaces.IRequestLog GetRequestLog(HttpApplication application, HttpContext context)
        {
            return this;
        }

        public void LogWarning(string message)
        {
        }

        public void TraceConditionListBegin(UrlRewrite.Interfaces.CombinationLogic logic)
        {
        }

        public void TraceConditionListEnd(bool conditionsMet)
        {
        }

        public void TraceCondition(UrlRewrite.Interfaces.ICondition condition, bool isTrue)
        {
        }

        public void TraceAction(UrlRewrite.Interfaces.IAction action)
        {
        }

        public void TraceRuleBegin(UrlRewrite.Interfaces.IRule rule)
        {
        }

        public void TraceRuleEnd(bool matched, bool stopProcessing)
        {
        }
    }
}