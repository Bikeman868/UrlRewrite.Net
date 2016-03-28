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

        public IRuleList Parse(XElement rootNode)
        {
            var root = new RuleList("Root", null);

            root.Add(
                new Rule(
                    "Rule 1", 
                    new ConditionList(CombinationLogic.AllTrue)
                        .Add(new StringMatch(Scope.Path, MatchPattern.Contains, "1"))
                        .Add(new StringMatch(Scope.Path, MatchPattern.EndsWith, ".aspx")), 
                    new ActionList(true)
                        .Add(new Replace(Scope.Path, "/rewriteOne.aspx"))
                        .Add(new Rewrite()),
                        true
                    ));

            root.Add(
                new Rule(
                    "Rule 2",
                    new StringMatch(Scope.Path, MatchPattern.Contains, "2"),
                    new ActionList(true)
                        .Add(new Replace(Scope.Path, "/rewriteTwo.aspx"))
                        .Add(new TemporaryRedirect()),
                        true
                    ));

            root.Add(
                new Rule(
                    "Rule 3",
                    new StringMatch(Scope.Path, MatchPattern.Contains, "3"),
                    new ActionList(true)
                        .Add(new Replace(Scope.Path, "rewriteThree.aspx"))
                        .Add(new PermenantRedirect()),
                        true
                    ));

            return root;
        }

        private IAction ConstructAction(Type type, XElement configuration)
        {
            var action = _factory.Create(type) as IAction;
            var extension = action as IRuleElement;
            if (extension != null) extension.Initialize(configuration);
            return action;
        }

        private ICondition ConstructCondition(Type type, XElement configuration)
        {
            var condition = _factory.Create(type) as ICondition;
            var extension = condition as IRuleElement;
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
