using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            var rules = ParseRulesElement(xmlRoot);

            return new RuleList(rootName, null, rules);
        }

        #region Constructing instances using factories and custom type registrations

        private IAction ConstructAction(string actionName, XElement configuration)
        {
            Type type;
            switch (actionName.ToLower())
            {
                case "redirect":
                    type = typeof(TemporaryRedirect);
                    break;
                case "redirectpermenant":
                    type = typeof(PermenantRedirect);
                    break;
                case "customresponse":
                    type = typeof(CustomResponse);
                    break;
                case "abortrequest":
                    type = typeof(AbortRequest);
                    break;
                case "none":
                    type = typeof(None);
                    break;
                default:
                    // TODO: Look up list of registered custom actions
                    return null;
            }

            var action = _factory.Create(type) as IAction;
            if (action != null) action.Initialize(configuration);
            return action;
        }

        private ICondition ConstructCondition(string conditionName, XElement configuration)
        {
            Type type = null;
            var condition = _factory.Create(type) as ICondition;
            if (condition != null) condition.Initialize(configuration);
            return condition;
        }

        private IOperation ConstructOperation(string operationName, XElement configuration)
        {
            Type type;
            switch (operationName.ToLower())
            {
                case "tolower":
                    type = typeof(LowerCaseOperation);
                    break;
                case "toupper":
                    type = typeof(UpperCaseOperation);
                    break;
                case "urlencode":
                    type = typeof(UrlEncodeOperation);
                    break;
                case "urldecode":
                    type = typeof(UrlDecodeOperation);
                    break;
                default:
                    // TODO: Look up list of registered custom operations
                    return null;
            }

            var operation = _factory.Create(type) as IOperation;
            if (operation != null) operation.Initialize(configuration);
            return operation;
        }

        IValueGetter ConstructValueGetter(Scope scope = Scope.Url, string scopeIndex = null, IOperation operation = null)
        {
            return _factory.Create<IValueGetter>().Initialize(scope, scopeIndex, operation);
        }

        IValueGetter ConstructValueGetter(Scope scope, int scopeIndex, IOperation operation = null)
        {
            return _factory.Create<IValueGetter>().Initialize(scope, scopeIndex, operation);
        }

        #endregion

        #region Parsing XML elements

        private IList<IRule> ParseRulesElement(XElement element)
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
                            return ParseRuleElement(e);
                        case "rewritemaps":
                            return ParseRewriteMapsElement(e);
                        default:
                            return null;
                    }
                })
                .Where(r => r != null)
                .ToList();
        }

        private IRule ParseRuleElement(XElement element)
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
                        condition = CombineConditions(condition, ParseMatchElement(child));
                        break;
                    case "condition":
                        condition = CombineConditions(condition, ParseConditionElement(child));
                        break;
                    case "conditions":
                        condition = CombineConditions(condition, ParseConditionsElement(child));
                        break;
                    case "action":
                        action = CombineActions(action, ParseActionElement(child));
                        break;
                    case "rewrite":
                        action = CombineActions(action, ParseRewriteElement(child));
                        break;
                }
            }

            return new Rule(name, condition, action, stopProcessing);
        }

        private ICondition ParseMatchElement(XElement element)
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

            var valueGetter = ConstructValueGetter(scope);
            var stringMatch = _factory.Create<IStringMatch>().Initialize(valueGetter, compareOperation, text, inverted, ignoreCase, "R");
            return stringMatch;
        }

        private ICondition ParseConditionsElement(XElement element)
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
                        result = CombineConditions(result, ParseConditionsAddElement(child));
                        break;
                    case "condition":
                        result = CombineConditions(result, ParseConditionElement(child));
                        break;
                    case "conditions":
                        result = CombineConditions(result, ParseConditionsElement(child));
                        break;
                }
            }
            return result;
        }

        private ICondition ParseConditionElement(XElement element)
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

            IValueGetter valueGetter;
            valueGetter = isNumericIndex ? ConstructValueGetter(scope, scopeIndexInt) : ConstructValueGetter(scope, scopeIndexString);

            if (isNumericValue)
                return _factory.Create<INumberMatch>().Initialize(valueGetter, compareOperation, value, inverted, defaultValue);
            return _factory.Create<IStringMatch>().Initialize(valueGetter, compareOperation, text, inverted, ignoreCase);
        }

        private ICondition ParseConditionsAddElement(XElement element)
        {
            IValueGetter valueGetter = null;
            var scope = Scope.Url;
            var compareOperation = CompareOperation.MatchRegex;
            var inverted = false;
            var ignoreCase = true;
            var text = ".*";

            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "input":
                            valueGetter = ParseTextWithMacros(attribute.Value);
                            break;
                        case "matchType":
                            if (attribute.Value.ToLower() == "isfile")
                            {
                                scope = Scope.Path;
                                compareOperation = CompareOperation.EndsWith;
                                text = "/";
                                inverted = true;
                            }
                            else if (attribute.Value.ToLower() == "isdirectory")
                            {
                                scope = Scope.Path;
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

            if (valueGetter == null)
                valueGetter = ConstructValueGetter(scope);

            var stringMatch = _factory.Create<IStringMatch>().Initialize(valueGetter, compareOperation, text, inverted, ignoreCase);
            return stringMatch;
        }

        private IAction ParseRewriteElement(XElement element)
        {
            var fromScope = Scope.Path;
            string fromIndex = null;
            var toScope = Scope.Path;
            string toIndex = null;
            IOperation operation = null;
            
            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "to":
                            Enum.TryParse(attribute.Value, true, out toScope);
                            break;
                        case "toindex":
                            toIndex = attribute.Value;
                            break;
                        case "from":
                            Enum.TryParse(attribute.Value, true, out fromScope);
                            break;
                        case "fromindex":
                            fromIndex = attribute.Value;
                            break;
                        case "operation":
                            operation = ConstructOperation(attribute.Value, element);
                            break;
                    }
                }
            }

            var value = _factory.Create<IValueGetter>().Initialize(fromScope, fromIndex, operation);
            return new Replace(toScope, toIndex, value);
        }

        private IRule ParseRewriteMapsElement(XElement element)
        {
            return null;
        }

        private IAction ParseActionElement(XElement element)
        {
            IValueGetter valueGetter = null;
            IAction action = null;
            var appendQueryString = false;

            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "url":
                        {
                            valueGetter = ParseTextWithMacros(attribute.Value);
                            break;
                        }
                        case "type":
                            action = ConstructAction(attribute.Value, element);
                            break;
                        case "appendquerystring":
                            appendQueryString = attribute.Value.ToLower() == "true";
                            break;
                    }
                }
            }

            var actionList = new ActionList();

            if (valueGetter != null)
                actionList.Add(new Replace(Scope.Url, null, valueGetter));

            if (appendQueryString)
                actionList.Add(new Append(Scope.QueryString, null, ConstructValueGetter(Scope.OriginalQueryString)));

            if (action != null)
                actionList.Add(action);

            return actionList;
        }
        
        #endregion

        #region Parsing macros in curly braces

        private class TextArea
        {
            public string Text;
            public bool IsQuoted;
        }

        private IList<TextArea> SeparateMarcoText(string input)
        {
            var depth = 0;
            var literalText = new StringBuilder();
            var macroText = new StringBuilder();
            var result = new List<TextArea>();

            Action<char> appendChar = c =>
            {
                if (depth > 0) macroText.Append(c);
                else literalText.Append(c);
            };

            for (var i = 0; i < input.Length; i++)
            {
                var ch = input[i];
                switch (ch)
                {
                    case '{':
                        if (depth == 0)
                        {
                            if (literalText.Length > 0)
                            {
                                result.Add(new TextArea
                                {
                                    Text = literalText.ToString(),
                                    IsQuoted = false
                                });
                                literalText.Clear();
                            }
                        }
                        else
                        {
                            appendChar(ch);
                        }
                        depth++;
                        break;
                    case '}':
                        depth--;
                        if (depth == 0)
                        {
                            if (macroText.Length > 0)
                            {
                                result.Add(new TextArea
                                {
                                    Text = macroText.ToString(),
                                    IsQuoted = true
                                });
                                macroText.Clear();
                            }
                        }
                        else
                        {
                            appendChar(ch);
                        }
                        break;
                    default:
                        appendChar(ch);
                        break;
                }
            }
            if (literalText.Length > 0) result.Add(new TextArea{Text = literalText.ToString()});
            return result;
        }

        private IValueGetter ParseMacro(string input)
        {
            var colonIndex = input.IndexOf(':');
            if (colonIndex > 0)
            {
                var operationName = input.Substring(0, colonIndex).ToLower();
                input = input.Substring(colonIndex + 1, input.Length - colonIndex - 1);

                if (operationName == "c")
                    return ConstructValueGetter(Scope.ConditionGroup, input);

                if (operationName == "r")
                    return ConstructValueGetter(Scope.MatchGroup, input);

                var operationInputGetter = ParseTextWithMacros(input);
                IOperation operation = null;

                     if (operationName == "tolower") operation = new LowerCaseOperation();
                else if (operationName == "toupper") operation = new UpperCaseOperation();
                else if (operationName == "urlencode") operation = new UrlEncodeOperation();
                else if (operationName == "urldecode") operation = new UrlDecodeOperation();

                // TODO: custom operations

                return operation == null ? operationInputGetter : new ValueConcatenator().Initialize(operationInputGetter, operation);
            }

            if (input.StartsWith("HTTP_"))
                return ConstructValueGetter(Scope.Header, input.Substring(5));
            return ConstructValueGetter(Scope.ServerVariable, input);
        }
     
        private IValueGetter ParseTextWithMacros(string input)
        {
            var textAreas = SeparateMarcoText(input.Trim());
            if (textAreas == null || textAreas.Count == 0) return null;

            var areaGetters = textAreas
                .Select(a => a.IsQuoted
                    ? ParseMacro(a.Text) 
                    : ConstructValueGetter(Scope.Literal, a.Text))
                .ToList();

            return areaGetters.Count == 1 ? areaGetters[0] : new ValueConcatenator().Initialize(areaGetters);
        }

        #endregion

        #region Private helper functions

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

        private IAction CombineActions(IAction a1, IAction a2)
        {
            if (a2 == null) return a1;
            if (a1 == null) return a2;

            var actionList1 = a1 as ActionList;
            var actionList2 = a2 as ActionList;

            if (actionList1 == null)
            {
                if (actionList2 == null)
                {
                    var newActionList = new ActionList();
                    newActionList.Add(a1);
                    newActionList.Add(a2);
                    return newActionList;
                }
                actionList2.Add(a1);
                return actionList2;
            }

            actionList1.Add(a2);
            return actionList1;
        }

        #endregion
    }
}
