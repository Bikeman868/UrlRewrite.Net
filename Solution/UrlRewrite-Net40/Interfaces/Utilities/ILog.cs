using System;
using System.Web;

namespace UrlRewrite.Interfaces.Utilities
{
    public interface ILog
    {
        void LogException(IRequestInfo request, Exception ex);
        IRequestLog GetRequestLog(HttpApplication application, HttpContext context);
    }
}
