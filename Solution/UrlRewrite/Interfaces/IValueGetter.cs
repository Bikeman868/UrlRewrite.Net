using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UrlRewrite.Interfaces
{
    public interface IValueGetter
    {
        IValueGetter Initialize(Scope scope, int scopeIndex, bool ignoreCase = true);
        IValueGetter Initialize(Scope scope, string scopeIndex = null, bool ignoreCase = true);

        string GetString(IRequestInfo requestInfo);
        int GetInt(IRequestInfo requestInfo, int defaultValue);
    }
}
