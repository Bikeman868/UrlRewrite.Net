using System.Collections.Generic;
using System.Web;

namespace UrlRewrite.Interfaces
{
    public interface IRequestInfo
    {
        HttpApplication Application { get; }
        HttpContext Context { get; }
        IRequestLog Log { get; }
        bool TraceRequest { get; }
        List<string> OriginalPath { get; }
        List<string> NewPath { get; set; }
        Dictionary<string, List<string>> OriginalQueryString { get; }
        Dictionary<string, List<string>> NewQueryString { get; set; }
    }
}
