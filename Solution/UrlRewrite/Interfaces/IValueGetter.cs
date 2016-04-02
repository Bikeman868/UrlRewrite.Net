using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UrlRewrite.Interfaces
{
    public interface IValueGetter
    {
        string GetString(IRequestInfo requestInfo);
        int GetInt(IRequestInfo requestInfo, int defaultValue);
    }
}
