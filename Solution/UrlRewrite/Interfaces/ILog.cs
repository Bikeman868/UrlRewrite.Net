using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace UrlRewrite.Interfaces
{
    public interface ILog
    {
        void LogException(Exception ex);
        IRequestLog GetRequestLog(HttpApplication application, HttpContext context);
    }
}
