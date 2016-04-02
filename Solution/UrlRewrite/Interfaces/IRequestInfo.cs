using System;
using System.Collections.Generic;
using System.Web;

namespace UrlRewrite.Interfaces
{
    public interface IRequestInfo
    {
        // Contextual properties
        HttpApplication Application { get; }
        HttpContext Context { get; }
        IRequestLog Log { get; }
        ExecutionMode ExecutionMode { get; }
        IList<Action<IRequestInfo>> DeferredActions { get; }

        // Information parsed from the incomming request
        string OriginalPathString { get; }
        List<string> OriginalPath { get; }
        string OriginalParametersString { get; }
        Dictionary<string, List<string>> OriginalParameters { get; }

        // Control over the rewritten/redirected URL
        string NewUrlString { get; }
        string NewPathString { get; }
        string NewParametersString { get; }
        List<string> NewPath { get; set; }
        Dictionary<string, List<string>> NewParameters { get; set; }
    }
}
