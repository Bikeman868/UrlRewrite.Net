using System;
using System.Diagnostics;
using System.Web;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Utilities;
using UrlRewrite.Request;

namespace UrlRewrite.Utilities
{
    public class TraceLog: ILog
    {
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
