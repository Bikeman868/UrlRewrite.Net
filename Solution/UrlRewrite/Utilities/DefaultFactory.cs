using System;
using Ninject;
using Ninject.Modules;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Rules;
using UrlRewrite.Interfaces.Utilities;

namespace UrlRewrite.Utilities
{
    public class DefaultFactory: IFactory
    {
        private readonly IKernel _ninject;

        public DefaultFactory()
        {
            _ninject = new StandardKernel(new RewriteNinjectModule(this));
        }

        T IFactory.Create<T>()
        {
            return (T)(((IFactory)this).Create(typeof(T)));
        }

        object IFactory.Create(Type type)
        {
            var result = _ninject.TryGet(type);
            if (ReferenceEquals(result, null))
            {
                var constructor = type.GetConstructor(Type.EmptyTypes);
                result = constructor == null ? null : constructor.Invoke(null);
            }
            return result;
        }
    }

    public class RewriteNinjectModule : NinjectModule
    {
        private readonly IFactory _factory;

        public RewriteNinjectModule(IFactory factory)
        {
            _factory = factory;
        }
     
        public override void Load()
        {
            // Factory can resolve itself
            Bind<IFactory>().ToConstant(_factory);

            // Singletons
            Bind<ILog>().To<DefaultLog>().InSingletonScope();
            Bind<ICustomTypeRegistrar>().To<Configuration.CustomTypeRegistrar>().InSingletonScope();

            // Input values and comparison
            Bind<IValueGetter>().To<Conditions.ValueGetter>();
            Bind<IStringMatch>().To<Conditions.StringMatch>();
            Bind<INumberMatch>().To<Conditions.NumberMatch>();
            Bind<IConditionList>().To<Conditions.ConditionList>();
            Bind<IValueConcatenator>().To<Conditions.ValueConcatenator>();

            // Request handling
            Bind<IRequestInfo>().To<Request.RequestInfo>();

            // Actions
            Bind<IActionList>().To<Actions.ActionList>();
            Bind<IKeepAction>().To<Actions.Keep>();
            Bind<IAppendAction>().To<Actions.Append>();
            Bind<IDeleteAction>().To<Actions.Delete>();
            Bind<IReplaceAction>().To<Actions.Replace>();
            Bind<ITruncateAction>().To<Actions.Truncate>();

            // Rules
            Bind<IRuleResult>().To<Rules.RuleResult>();
            Bind<IRuleList>().To<Rules.RuleList>();
            Bind<IRule>().To<Rules.Rule>();

            // Utility classed
            Bind<IPropertyBag>().To<PropertyBag>();
        }
    }
}
