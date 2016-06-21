using System.Xml.Linq;
using UrlRewrite.Interfaces.Rules;

namespace UrlRewrite.Interfaces.Conditions
{
    public interface ICondition : IRuleElement
    {
        ICondition Initialize(XElement configuration, IValueGetter valueGetter);

        /// <summary>
        /// Tests a request to see if it meets this condition
        /// </summary>
        bool Test(IRequestInfo request, IRuleResult ruleResult = null);
    }
}
