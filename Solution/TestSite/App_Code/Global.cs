using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestSite.App_Code
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            var rawUrlsToTrace = 
                new[]
                {
                    "/test1.aspx"
                }
                .Select(u => u.ToLower())
                .ToList();

            var rewrittenUrlsToTrace = 
                new[]
                {
                    "/rewriteOne.aspx", 
                    "/rewriteTwo.aspx", 
                    "/rewriteThree.aspx"
                }
                .Select(u => u.ToLower())
                .ToList();

            UrlRewrite.RewriteModule.Initialize(
                null, 
                url => rawUrlsToTrace.Contains(url.ToLower()),
                url => rewrittenUrlsToTrace.Contains(url.ToLower()));
        }
    }
}