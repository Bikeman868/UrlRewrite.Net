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
            var rawUrlsToTrace = new List<string>
            {
                "/test1.aspx"
            };

            var rewrittenUrlsToTrace = new List<string>
            {
                "/rewriteOne.aspx", 
                "/rewriteTwo.aspx", 
                "/rewriteThree.aspx"
            };

            UrlRewrite.RewriteModule.Initialize(null, rawUrlsToTrace, rewrittenUrlsToTrace);
        }
    }
}