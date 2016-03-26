using System;
using System.Xml.Linq;
using UrlRewrite.Actions;
using UrlRewrite.Conditions;
using UrlRewrite.Interfaces;
using UrlRewrite.Rules;

namespace UrlRewrite.Configuration
{
    internal class RuleParser: IFactory
    {
        private readonly IFactory _factory;

        public RuleParser(IFactory factory)
        {
            _factory = factory ?? this;
        }

        public IRule Parse(XElement rootNode)
        {
            var root = new RuleList("Root", null);

            root.Add(
                new Simple(
                    "Rule 1", 
                    new PathContainsString("1"), 
                    new ActionList(true)
                        .Add(new Replace(Scope.Path, "/rewriteOne.aspx"))
                        .Add(new Rewrite())
                    ));

            root.Add(
                new Simple(
                    "Rule 2",
                    new PathContainsString("2"),
                    new ActionList(true)
                        .Add(new Replace(Scope.Path, "/rewriteTwo.aspx"))
                        .Add(new TemporaryRedirect())
                    ));

            root.Add(
                new Simple(
                    "Rule 3",
                    new PathContainsString("3"),
                    new ActionList(true)
                        .Add(new Replace(Scope.Path, "rewriteThree.aspx"))
                        .Add(new PermenantRedirect())
                    ));

            return root;
        }

        private IAction ConstructAction(Type type, XElement configuration)
        {
            var action = _factory.Create(type) as IAction;
            var extension = action as IApplicationExtension;
            if (extension != null) extension.Initialize(configuration);
            return action;
        }

        private ICondition ConstructCondition(Type type, XElement configuration)
        {
            var condition = _factory.Create(type) as ICondition;
            var extension = condition as IApplicationExtension;
            if (extension != null) extension.Initialize(configuration);
            return condition;
        }

        #region Dummy IFactory implementation

        T IFactory.Create<T>()
        {
            return (T)((IFactory)this).Create(typeof(T));
        }

        object IFactory.Create(Type type)
        {
            if (type == typeof(IFactory))
                return this;

            if (type == typeof(ILog))
                return this;

            var constructor = type.GetConstructor(Type.EmptyTypes);
            return constructor == null ? null : constructor.Invoke(null);
        }

        #endregion
    }
}
