using System.Collections.Generic;
using Ioc.Modules;
using UrlRewrite.Configuration;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Rules;
using UrlRewrite.Interfaces.Utilities;
using UrlRewrite.Utilities;

namespace UrlRewrite
{
    [Package]
    public class Package: IPackage
    {
        public string Name { get { return "Url rewrite"; } }

        public IList<IocRegistration> IocRegistrations
        {
            get
            {
                return new List<IocRegistration>
                {
                    new IocRegistration().Init<ILog, TraceLog>(),
                    new IocRegistration().Init<ICustomTypeRegistrar, CustomTypeRegistrar>(),

                    // Input values and comparison
                    new IocRegistration().Init<IValueGetter, Conditions.ValueGetter>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IStringMatch, Conditions.StringMatch>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<INumberMatch, Conditions.NumberMatch>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IConditionList, Conditions.ConditionList>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IValueConcatenator, Conditions.ValueConcatenator>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IStaticFileMatch, Conditions.StaticFileMatch>(IocLifetime.MultiInstance),

                    // Request handling
                    new IocRegistration().Init<IRequestInfo, Request.RequestInfo>(IocLifetime.MultiInstance),

                    // Actions
                    new IocRegistration().Init<IActionList, Actions.ActionList>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IAppendAction, Actions.Append>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IDeleteAction, Actions.Delete>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IInsertAction, Actions.Insert>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IKeepAction, Actions.Keep>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<INormalizeAction, Actions.Normalize>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IReplaceAction, Actions.Replace>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<ITruncateAction, Actions.Truncate>(IocLifetime.MultiInstance),

                    // Rules
                    new IocRegistration().Init<IRuleResult, Rules.RuleResult>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IRuleList, Rules.RuleList>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IRule, Rules.Rule>(IocLifetime.MultiInstance),

                    // Utility
                    new IocRegistration().Init<IPropertyBag, PropertyBag>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IFactory, NinjectFactory>(),
                };
            }
        }

    }
}
