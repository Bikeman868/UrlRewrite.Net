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

            ICondition condition = null;
            IAction action = null;

            foreach (var child in element.Elements())
            {
                switch (child.Name.LocalName.ToLower())
                {
                    case "match":
                        condition = CombineConditions(condition, ParseRuleMatch(child));
                        break;
                    case "condition":
                        condition = CombineConditions(condition, ParseRuleCondition(child));
                        break;
                    case "conditions":
                        condition = CombineConditions(condition, ParseRuleConditions(child));
                        break;
                    case "action":
                        action = CombineActions(action, ParseRuleAction(child));
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
                        case "patternsyntax":
                            if (attribute.Value == "ECMAScript")
                                compareOperation = CompareOperation.MatchRegex;
                            else if (attribute.Value == "Wildcard")
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
            ICondition result = null;

            var logicalGroupingAttribute = element.Attributes().FirstOrDefault(a => a.Name.LocalName.ToLower() == "logicalgrouping");
            if (logicalGroupingAttribute != null)
            {
                CombinationLogic logic;
                if (Enum.TryParse(logicalGroupingAttribute.Value, out logic))
                    result = new ConditionList().Initialize(logic);
            }

            foreach (var child in element.Elements())
            {
                switch (child.Name.LocalName.ToLower())
                {
                    case "add":
                        result = CombineConditions(result, ParseRuleConditionsAdd(child));
                        break;
                    case "condition":
                        result = CombineConditions(result, ParseRuleCondition(child));
                        break;
                    case "conditions":
                        result = CombineConditions(result, ParseRuleConditions(child));
                        break;
                }
            }
            return result;
        }

        private ICondition ParseRuleCondition(XElement element)
        {
            var scope = Scope.Url;

            var isNumericIndex = false;
            string scopeIndexString = null;
            var scopeIndexInt = 0;
            var indexIsANumber = false;

            var isNumericValue = false;
            string text = null;
            var value = 0;
            var defaultValue = 0;
            var valueIsANumber = false;

            var compareOperation = CompareOperation.MatchRegex;
            var inverted = false;
            var ignoreCase = true;

            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "scope":
                            Enum.TryParse(attribute.Value, true, out scope);
                            break;
                        case "index":
                            scopeIndexString = attribute.Value;
                            indexIsANumber = int.TryParse(attribute.Value, out scopeIndexInt);
                            break;
                        case "test":
                            Enum.TryParse(attribute.Value, true, out compareOperation);
                            break;
                        case "value":
                            text = attribute.Value;
                            valueIsANumber = int.TryParse(attribute.Value, out value);
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

            switch (scope)
            {
                case Scope.PathElement:
                case Scope.OriginalPathElement:
                    isNumericIndex = true;
                    isNumericValue = false;
                    break;
                case Scope.Header:
                    isNumericIndex = false;
                    isNumericValue = valueIsANumber;
                    break;
                case Scope.Literal:
                    isNumericIndex = indexIsANumber;
                    isNumericValue = indexIsANumber;
                    break;
                default:
                    isNumericIndex = false;
                    isNumericValue = false;
                    break;
            }

            var valueGetter = _factory.Create<IValueGetter>();
            if (isNumericIndex)
                 valueGetter.Initialize(scope, scopeIndexInt);
            else
                valueGetter.Initialize(scope, scopeIndexString, ignoreCase);

            if (isNumericValue)
                return _factory.Create<INumberMatch>().Initialize(valueGetter, compareOperation, value, inverted, defaultValue);
            return _factory.Create<IStringMatch>().Initialize(valueGetter, compareOperation, text, inverted, ignoreCase);
        }

        private ICondition ParseRuleConditionsAdd(XElement element)
        {
            var scope = Scope.Url;
            string scopeIndex = null;
            var compareOperation = CompareOperation.MatchRegex;
            var inverted = false;
            var ignoreCase = true;
            var text = ".*";
            string operationName = null;

            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "input":
                            ParseCurlyBraces(attribute.Value, out scope, out scopeIndex, out operationName);
                            break;
                        case "matchType":
                            if (attribute.Value.ToLower() == "isfile")
                            {
                                scope = Scope.Path;
                                scopeIndex = null;
                                compareOperation = CompareOperation.EndsWith;
                                text = "/";
                                inverted = true;
                            }
                            else if (attribute.Value.ToLower() == "isdirectory")
                            {
                                scope = Scope.Path;
                                scopeIndex = null;
                                compareOperation = CompareOperation.EndsWith;
                                text = "/";
                                inverted = false;
                            }
                            break;
                        case "pattern":
                            text = attribute.Value;
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

            var valueGetter = _factory.Create<IValueGetter>().Initialize(scope, scopeIndex, ignoreCase);
            var stringMatch = _factory.Create<IStringMatch>().Initialize(valueGetter, compareOperation, text, inverted, ignoreCase);
            return stringMatch;
        }

        private IRule ParseRewriteMaps(XElement element)
        {
            return null;
        }

        private IAction ParseRuleAction(XElement element)
        {
            return null;
        }

        private ICondition CombineConditions(ICondition c1, ICondition c2)
        {
            if (c2 == null) return c1;
            if (c1 == null) return c2;

            var conditionList = c1 as ConditionList;
            if (conditionList == null)
            {
                conditionList = new ConditionList();
                conditionList.Initialize(CombinationLogic.MatchAll);
                conditionList.Add(c1);
            }
            conditionList.Add(c2);
            return conditionList;
        }

        private void ParseCurlyBraces(string input, out Scope scope, out string scopeIndex, out string operationName)
        {
            input = input.Trim();
            if (input.StartsWith("{") && input.EndsWith("}"))
            {
                var colonIndex = input.IndexOf(':');
                if (colonIndex > 0)
                {
                    operationName = input.Substring(1, colonIndex - 1);
                    input = input.Substring(colonIndex + 1, input.Length - colonIndex - 4);
                }
                else
                {
                    operationName = null;
                }

                if (input.StartsWith("{") && input.EndsWith("}"))
                {
                    input = input.ToUpper();
                    if (input.StartsWith("{HTTP_"))
                    {
                        scope = Scope.OriginalHeader;
                        scopeIndex = input.Substring(6, input.Length - 7);
                    }
                    else
                    {
                        scope = Scope.ServerVariable;
                        scopeIndex = input.Substring(1, input.Length - 2);
                    }
                }
                else
                {
                    scope = Scope.Literal;
                    scopeIndex = input;
                }
            }
            else
            {
                scope = Scope.Literal;
                scopeIndex = input;
                operationName = null;
            }
        }

        private IAction CombineActions(IAction a1, IAction a2)
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
        }

    }
}
