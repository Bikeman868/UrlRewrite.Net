using System;
using System.Collections.Generic;
using System.Web;

namespace UrlRewrite.Interfaces
{
    public interface IRuleEngine: IDisposable
    {
        /// <summary>
        /// Evaluates rules for a request and returns the actions to perform
        /// </summary>
        IEnumerable<IRuleAction> EvaluateRules(IRequestInfo request);
    }
}
