using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UrlRewrite.Interfaces
{
    public interface IRule
    {
        string Name { get; }
        IRuleResult Evaluate(IRequestInfo request);
    }
}
