using System;
using System.Diagnostics;
using System.Web;
using UrlRewrite.Conditions;
using UrlRewrite.Interfaces;
using UrlRewrite.Request;
using UrlRewrite.Rules;

namespace UrlRewrite.Utilities
{
    public class DefaultFactory: IFactory, ILog
    {
        T IFactory.Create<T>()
        {
            return (T)((IFactory)this).Create(typeof(T));
        }

        object IFactory.Create(Type type)
        {
            if (type == typeof(IFactory))
                return this;

            if (type == typeof(ILog))
                return this;

            if (type == typeof (IValueGetter))
                return new ValueGetter();

            if (type == typeof(IStringMatch))
                return new StringMatch();

            if (type == typeof(INumberMatch))
                return new NumberMatch();

            if (type == typeof(IRequestInfo))
                return new RequestInfo();

            if (type == typeof(IRuleResult))
                return new RuleResult();

            var constructor = type.GetConstructor(Type.EmptyTypes);
            return constructor == null ? null : constructor.Invoke(null);
        }

        void ILog.LogException(IRequestInfo request, Exception ex)
        {
            Trace.WriteLine("Rewrite module exception: ", ex.Message);
        }

        IRequestLog ILog.GetRequestLog(HttpApplication application, HttpContext context)
        {
            return new RequestLog();
        }
    }
}
