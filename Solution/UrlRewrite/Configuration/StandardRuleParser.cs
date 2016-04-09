using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UrlRewrite.Actions;
using UrlRewrite.Conditions;
using UrlRewrite.Interfaces;
using UrlRewrite.Operations;
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

        private IOperation ConstructOperation(Type type, XElement configuration)
        {
            var operation = _factory.Create(type) as IOperation;
            if (operation != null) operation.Initialize(configuration);
            return operation;
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
                    case "rewrite":
                        action = CombineActions(action, ParseRewrite(child));
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

            var valueGetter = _factory.Create<IValueGetter>().Initialize(scope);
            var stringMatch = _factory.Create<IStringMatch>().Initialize(valueGetter, compareOperation, text, inverted, ignoreCase, "R");
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
                valueGetter.Initialize(scope, scopeIndexString);

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
            IOperation operation = null;

            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "input":
                            ParseCurlyBraces(attribute.Value, out scope, out scopeIndex, out operation);
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

            IList<IOperation> operations = operation == null ? null : new List<IOperation> { operation };

            var valueGetter = _factory.Create<IValueGetter>().Initialize(scope, scopeIndex, operations);
            var stringMatch = _factory.Create<IStringMatch>().Initialize(valueGetter, compareOperation, text, inverted, ignoreCase);
            return stringMatch;
        }

        private IAction ParseRewrite(XElement element)
        {
            return null;
        }

        private IRule ParseRewriteMaps(XElement element)
        {
            return null;
        }

        private IAction ParseRuleAction(XElement element)
        {
            var type = typeof(TemporaryRedirect);
            IValueGetter valueGetter = null;
            var appendQueryString = false;

            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "url":
                        {
                            Scope scope;
                            string scopeIndex;
                            IOperation operation;
                            ParseCurlyBraces(attribute.Value, out scope, out scopeIndex, out operation);
                            IList<IOperation> operations = operation == null ? null : new List<IOperation> { operation };
                            valueGetter = _factory.Create<IValueGetter>().Initialize(scope, scopeIndex, operations);
                            break;
                        }
                        case "type":
                            if (attribute.Value.ToLower() == "rewrite") type = typeof (Rewrite);
                            else if (attribute.Value.ToLower() == "redirect") type = typeof (TemporaryRedirect);
                            else if (attribute.Value.ToLower() == "redirectpermenant") type = typeof (PermenantRedirect);
                            else if (attribute.Value.ToLower() == "customresponse") type = typeof(CustomResponse);
                            else if (attribute.Value.ToLower() == "abortrequest") type = typeof(AbortRequest);
                            break;
                        case "appendquerystring":
                            appendQueryString = attribute.Value.ToLower() == "true";
                            break;
                    }
                }
            }

            var action = ConstructAction(type, element);

            if (valueGetter != null)
            {
                var actionList = new ActionList();
                if (!appendQueryString) actionList.Add(new Delete(Scope.QueryString));
                actionList.Add(new Replace(Scope.Path, null, valueGetter));
                actionList.Add(action);
                return actionList;
            }
            return action;
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

        private void ParseCurlyBraces(string input, out Scope scope, out string scopeIndex, out IOperation operation)
        {
            input = input.Trim();
            string operationName;
            if (input.StartsWith("{") && input.EndsWith("}"))
            {
                var colonIndex = input.IndexOf(':');
                if (colonIndex > 0)
                {
                    operationName = input.Substring(1, colonIndex - 1).ToLower();
                    input = input.Substring(colonIndex + 1, input.Length - colonIndex - 2);
                }
                else
                {
                    operationName = null;
                }

                if (operationName == "c") 
                {
                    scope = Scope.ConditionGroup;
                    scopeIndex = input;
                }
                else if (operationName == "r")
                {
                    scope = Scope.MatchGroup;
                    scopeIndex = input;
                }
                else if (input.StartsWith("{") && input.EndsWith("}"))
                {
                    input = input.ToUpper();
                    if (input.StartsWith("{HTTP_"))
                    {
                        scope = Scope.Header;
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

            if (operationName == "tolower") operation = new LowerCaseOperation();
            else if (operationName == "toupper") operation = new UpperCaseOperation();
            else if (operationName == "urlencode") operation = new UrlEncodeOperation();
            else if (operationName == "urldecode") operation = new UrlDecodeOperation();
            else operation = null;
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
