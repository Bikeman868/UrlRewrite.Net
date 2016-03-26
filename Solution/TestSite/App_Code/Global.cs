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
            UrlRewrite.RewriteModule.Initialize();
        }
    }
}