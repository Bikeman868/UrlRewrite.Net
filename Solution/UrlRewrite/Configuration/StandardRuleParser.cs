using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UrlRewrite.Actions;
using UrlRewrite.Conditions;
using UrlRewrite.Interfaces;
using UrlRewrite.Rules;
using UrlRewrite.Utilities;

namespace UrlRewrite.Configuration
{
    internal class StandardRuleParser: IRuleParser
    {
        private readonly IFactory _factory;

        public StandardRuleParser(IFactory factory)
        {
            _factory = factory;
        }

        public IRuleList Parse(Stream stream)
        {
            XDocument document;
            try
            {
                document = XDocument.Load(stream);
            }
            catch(Exception ex)
            {
                throw new UrlRewriteException("Failed to load rewriter rules as XDocument", ex);
            }

            var xmlRoot = document.Root;
            if (xmlRoot == null)
                throw new UrlRewriteException("No root element in rules");

            if (xmlRoot.Name != "rules")
                throw new UrlRewriteException("The rewriter rules must be an XML document with a <rules> root element");

            var rootName = "Root";
            if (xmlRoot.HasAttributes)
            {
                var nameAttribute = xmlRoot.Attributes().FirstOrDefault(a => a.Name == "name");
                if (nameAttribute != null)
                    rootName = nameAttribute.Value;
            }
            var rules = ParseRules(xmlRoot);

            return new RuleList(rootName, null, rules);
        }

        private IAction ConstructAction(Type type, XElement configuration)
        {
            var action = _factory.Create(type) as IAction;
            if (action != null) action.Initialize(configuration);
            return action;
        }

        private ICondition ConstructCondition(Type type, XElement configuration)
        {
            var condition = _factory.Create(type) as ICondition;
            if (condition != null) condition.Initialize(configuration);
            return condition;
        }

        private IList<IRule> ParseRules(XElement element)
        {
            return element
                .Nodes()
                .Where(n => n.NodeType == XmlNodeType.Element)
                .Cast<XElement>()
                .Select<XElement, IRule>(e =>
                {
                    switch (e.Name.LocalName.ToLower())
                    {
                        case "rule":
                            return ParseRule(e);
                        case "rewritemaps":
                            return ParseRewriteMaps(e);
                        default:
                            return null;
                    }
                })
                .Where(r => r != null)
                .ToList();
        }

        private IRule ParseRule(XElement element)
        {
            var name = "Rule " + Guid.NewGuid();
            var stopProcessing = true;

            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "name":
                            name = attribute.Value;
                            break;
                        case "stopprocessing":
                            stopProcessing = attribute.Value.ToLower() == "true";
                            break;
                    }
                }
            }

            Func<ICondition, ICondition, ICondition> combineConditions = (c1, c2) =>
            {
                if (c2 == null) return c1;
                if (c1 == null) return c2;

                var conditionList = c1 as ConditionList;
                if (conditionList == null)
                {
                    conditionList = new ConditionList(CombinationLogic.AllTrue);
                    conditionList.Add(c1);
                }
                conditionList.Add(c2);
                return conditionList;
            };

            Func<IAction, IAction, IAction> combineActions = (a1, a2) =>
            {
                if (a2 == null) return a1;
                if (a1 == null) return a2;

                var actionList = a1 as ActionList;
                if (actionList == null)
                {
                    actionList = new ActionList();
                    actionList.Add(a1);
                }
                actionList.Add(a2);
                return actionList;
            };

            ICondition condition = null;
            IAction action = null;

            foreach (var child in element.Elements())
            {
                switch (child.Name.LocalName.ToLower())
                {
                    case "match":
                        condition = combineConditions(condition, ParseRuleMatch(child));
                        break;
                    case "conditions":
                        condition = combineConditions(condition, ParseRuleConditions(child));
                        break;
                    case "action":
                        action = combineActions(action, ParseRuleAction(child));
                        break;
                }
            }

            return new Rule(name, condition, action, stopProcessing);
        }

        private ICondition ParseRuleMatch(XElement element)
        {
            var scope = Scope.Url;
            var compareOperation = CompareOperation.MatchRegex;
            var inverted = false;
            var ignoreCase = true;
            var text = ".*";

            if (element.HasAttributes)
            {
                foreach(var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "url":
                            text = attribute.Value;
                            scope = Scope.Path;
                            break;
                        case "patternSyntax":
                            if (attribute.Value == "ECMAScript")
                                compareOperation = CompareOperation.MatchRegex;
                            else if (attribute.Value == "Wildcard ")
                                compareOperation = CompareOperation.MatchWildcard;
                            break;
                        case "negate":
                            inverted = attribute.Value.ToLower() == "true";
                            break;
                        case "ignorecase":
                            ignoreCase = attribute.Value.ToLower() == "true";
                            break;
                    }
                }
            }

            var valueGetter = _factory.Create<IValueGetter>().Initialize(scope, null, ignoreCase);
            var stringMatch = _factory.Create<IStringMatch>().Initialize(valueGetter, compareOperation, text, inverted, ignoreCase);
            return stringMatch;
        }

        private ICondition ParseRuleConditions(XElement element)
        {
            return null;
        }

        private IRule ParseRewriteMaps(XElement element)
        {
            return null;
        }

        private IAction ParseRuleAction(XElement element)
        {
            return null;
        }

    }
}
