using System.Collections.Generic;
using UrlRewrite.Configuration;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Rules;
using UrlRewrite.Interfaces.Utilities;

namespace UrlRewrite.Utilities
{
    public static class IocRegistrations
    {
        public static IEnumerable<IocRegistration> GetAll()
        {
            return new List<IocRegistration>
            {
                // Singletons
                new IocRegistration().Init<ILog, TraceLog>(IocLifetime.Singleton),
                new IocRegistration().Init<ICustomTypeRegistrar, CustomTypeRegistrar>(IocLifetime.Singleton),

                // Input values and comparison
                new IocRegistration().Init<IValueGetter, Conditions.ValueGetter>(IocLifetime.CreateEachTime),
                new IocRegistration().Init<IStringMatch, Conditions.StringMatch>(IocLifetime.CreateEachTime),
                new IocRegistration().Init<INumberMatch, Conditions.NumberMatch>(IocLifetime.CreateEachTime),
                new IocRegistration().Init<IConditionList, Conditions.ConditionList>(IocLifetime.CreateEachTime),
                new IocRegistration().Init<IValueConcatenator, Conditions.ValueConcatenator>(IocLifetime.CreateEachTime),

                // Request handling
                new IocRegistration().Init<IRequestInfo, Request.RequestInfo>(IocLifetime.CreateEachTime),

                // Actions
                new IocRegistration().Init<IActionList, Actions.ActionList>(IocLifetime.CreateEachTime),
                new IocRegistration().Init<IKeepAction, Actions.Keep>(IocLifetime.CreateEachTime),
                new IocRegistration().Init<IAppendAction, Actions.Append>(IocLifetime.CreateEachTime),
                new IocRegistration().Init<IDeleteAction, Actions.Delete>(IocLifetime.CreateEachTime),
                new IocRegistration().Init<IReplaceAction, Actions.Replace>(IocLifetime.CreateEachTime),
                new IocRegistration().Init<ITruncateAction, Actions.Truncate>(IocLifetime.CreateEachTime),

                // Rules
                new IocRegistration().Init<IRuleResult, Rules.RuleResult>(IocLifetime.CreateEachTime),
                new IocRegistration().Init<IRuleList, Rules.RuleList>(IocLifetime.CreateEachTime),
                new IocRegistration().Init<IRule, Rules.Rule>(IocLifetime.CreateEachTime),

                // Utility classed
                new IocRegistration().Init<IPropertyBag, PropertyBag>(IocLifetime.CreateEachTime),
            };
        }
    }
}
