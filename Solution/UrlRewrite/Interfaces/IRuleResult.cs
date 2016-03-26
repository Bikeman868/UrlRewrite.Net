using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UrlRewrite.Interfaces
{
    public interface IRuleResult
    {
        List<IAction> Actions { get; }
        bool StopProcessing { get; }
    }
}
