using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace UrlRewrite.Interfaces
{
    public interface IRequestInfo
    {
        HttpApplication Application { get; }
        HttpContext Context { get; }
        IRequestLog Log { get; }
        bool TraceRequest { get; }
    }
}
