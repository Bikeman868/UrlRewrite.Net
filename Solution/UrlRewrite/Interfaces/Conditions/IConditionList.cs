using UrlRewrite.Interfaces.Rules;

namespace UrlRewrite.Interfaces.Conditions
{
    public interface IConditionList : ICondition
    {
        IConditionList Initialize(CombinationLogic logic, bool trackAllCaptures = false);

        IConditionList Add(ICondition condition);

        /// <summary>
        /// Tests a request to see if it meets this condition
        /// </summary>
        bool Test(IRequestInfo request, IRuleResult ruleResult = null);
    }
}
