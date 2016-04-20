using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Compilation;
using System.Xml;
using System.Xml.Linq;
using UrlRewrite.Interfaces;
using UrlRewrite.Interfaces.Actions;
using UrlRewrite.Interfaces.Conditions;
using UrlRewrite.Interfaces.Operations;
using UrlRewrite.Interfaces.Rules;
using UrlRewrite.Operations;
using UrlRewrite.Utilities;

namespace UrlRewrite.Configuration
{
    internal class StandardRuleParser: IRuleParser
    {
        private readonly IFactory _factory;
        private readonly ICustomTypeRegistrar _customTypeRegistrar;

        public StandardRuleParser(
            IFactory factory,
            ICustomTypeRegistrar customTypeRegistrar)
        {
            _factory = factory;
            _customTypeRegistrar = customTypeRegistrar;
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

            return ParseRulesElement(xmlRoot, "Root");
        }

        #region Constructing instances using factories and custom type registrations

        private IAction ConstructAction(string actionName, XElement configuration)
        {
            Type type;
            switch (actionName.ToLower())
            {
                case "redirect":
                    type = typeof(Actions.Redirect);
                    break;
                case "customresponse":
                    type = typeof(Actions.CustomResponse);
                    break;
                case "abortrequest":
                    type = typeof(Actions.AbortRequest);
                    break;
                case "none":
                    type = typeof(Actions.None);
                    break;
                default:
                    return _customTypeRegistrar.ConstructAction(actionName, configuration);
            }

            var action = _factory.Create(type) as IAction;
            if (action != null) action.Initialize(configuration);
            return action;
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
                    return _customTypeRegistrar.ConstructOperation(operationName, configuration);
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

        private IRuleList ParseRulesElement(XElement element, string defaultName)
        {
            var name = defaultName;
            var stopProcessing = false;

            var rules = element
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
                        case "assembly":
                            return ParseAssemblyElement(e);
                        default:
                            return null;
                    }
                })
                .Where(r => r != null)
                .ToList();

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

            return _factory.Create<IRuleList>().Initialize(name, rules, stopProcessing);
        }

        private IRule ParseRuleElement(XElement element)
        {
            var name = "Rule " + Guid.NewGuid();
            var stopProcessing = false;
            var isDynamic = false;

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
                        case "dynamic":
                            isDynamic = attribute.Value.ToLower() == "true";
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
                    case "rules":
                        action = CombineActions(action, ParseRulesElement(child, Guid.NewGuid().ToString()));
                        break;
                    case "delete":
                        action = CombineActions(action, ParseDeleteElement(child));
                        break;
                    case "keep":
                        action = CombineActions(action, ParseKeepElement(child));
                        break;
                    case "insert":
                        action = CombineActions(action, ParseInsertElement(child));
                        break;
                    case "append":
                        action = CombineActions(action, ParseAppendElement(child));
                        break;
                    case "normalize":
                        action = CombineActions(action, ParseNormalizeElement(child));
                        break;
                }
            }

            return _factory.Create<IRule>().Initialize(name, condition, action, stopProcessing, isDynamic);
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
                            scope = Scope.Path;
                            text = attribute.Value;
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
            var logic = CombinationLogic.MatchAll;
            var trackAllCaptures = false;

            if (element.HasAttributes)
            {
                foreach(var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "logicalgrouping":
                            CombinationLogic logicalgrouping;
                            if (Enum.TryParse(attribute.Value, out logicalgrouping))
                                logic = logicalgrouping;
                            break;
                        case "trackallcaptures":
                            trackAllCaptures = attribute.Value.ToLower() == "true";
                            break;
                    }
                }
            }

            ICondition result = _factory.Create<IConditionList>().Initialize(logic, trackAllCaptures);

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
            string customConditionName = null;
            var inverted = false;
            var ignoreCase = true;

            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "scope":
                            if (!Enum.TryParse(attribute.Value, true, out scope))
                                throw new UrlRewriteException(attribute.Value + " is not a valid scope");
                            break;
                        case "index":
                            scopeIndexString = attribute.Value;
                            indexIsANumber = int.TryParse(attribute.Value, out scopeIndexInt);
                            break;
                        case "test":
                            if (!Enum.TryParse(attribute.Value, true, out compareOperation))
                                customConditionName = attribute.Value;
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
                case Scope.ConditionGroup:
                case Scope.MatchGroup:
                    isNumericIndex = true;
                    isNumericValue = false;
                    break;
                case Scope.ServerVariable:
                case Scope.Header:
                case Scope.OriginalHeader:
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

            var valueGetter = isNumericIndex 
                ? ConstructValueGetter(scope, scopeIndexInt) 
                : ConstructValueGetter(scope, scopeIndexString);

            if (customConditionName != null)
                return _customTypeRegistrar.ConstructCondition(customConditionName, element, valueGetter);
            
            if (isNumericValue)
                return _factory.Create<INumberMatch>()
                    .Initialize(valueGetter, compareOperation, value, inverted, defaultValue);

            return _factory.Create<IStringMatch>()
                .Initialize(valueGetter, compareOperation, text, inverted, ignoreCase);
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
                            if (!Enum.TryParse(attribute.Value, true, out toScope))
                                throw new UrlRewriteException(attribute.Value + " is not a valid scope");
                            break;
                        case "toindex":
                            toIndex = attribute.Value;
                            break;
                        case "from":
                            if (!Enum.TryParse(attribute.Value, true, out fromScope))
                                throw new UrlRewriteException(attribute.Value + " is not a valid scope");
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
            return _factory.Create<IReplaceAction>().Initialize(toScope, toIndex, value);
        }


        private IAction ParseDeleteElement(XElement element)
        {
            var scope = Scope.PathElement;
            string index = null;

            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "scope":
                            if (!Enum.TryParse(attribute.Value, true, out scope))
                                throw new UrlRewriteException(attribute.Value + " is not a valid scope");
                            break;
                        case "index":
                            index = attribute.Value;
                            break;
                    }
                }
            }

            return _factory.Create<IDeleteAction>().Initialize(scope, index);
        }

        private IAction ParseInsertElement(XElement element)
        {
            var fromScope = Scope.Literal;
            string fromIndex = null;
            var toScope = Scope.PathElement;
            var toIndex = "0";
            IOperation operation = null;

            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "scope":
                            if (!Enum.TryParse(attribute.Value, true, out toScope))
                                throw new UrlRewriteException(attribute.Value + " is not a valid scope");
                            break;
                        case "index":
                            toIndex = attribute.Value;
                            break;
                        case "from":
                            if (!Enum.TryParse(attribute.Value, true, out fromScope))
                                throw new UrlRewriteException(attribute.Value + " is not a valid scope");
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
            return _factory.Create<IInsertAction>().Initialize(toScope, toIndex, value);
        }

        private IAction ParseAppendElement(XElement element)
        {
            var fromScope = Scope.Literal;
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
                        case "scope":
                            if (!Enum.TryParse(attribute.Value, true, out toScope))
                                throw new UrlRewriteException(attribute.Value + " is not a valid scope");
                            break;
                        case "index":
                            toIndex = attribute.Value;
                            break;
                        case "from":
                            if (!Enum.TryParse(attribute.Value, true, out fromScope))
                                throw new UrlRewriteException(attribute.Value + " is not a valid scope");
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
            return _factory.Create<IAppendAction>().Initialize(toScope, toIndex, value);
        }

        private IAction ParseKeepElement(XElement element)
        {
            var scope = Scope.Parameter;
            string index = null;

            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "scope":
                            if (!Enum.TryParse(attribute.Value, true, out scope))
                                throw new UrlRewriteException(attribute.Value + " is not a valid scope");
                            break;
                        case "index":
                            index = attribute.Value;
                            break;
                    }
                }
            }

            return _factory.Create<IKeepAction>().Initialize(scope, index);
        }

        private IAction ParseNormalizeElement(XElement element)
        {
            var leadingSeparator = NormalizeAction.None;
            var trailingSeparator = NormalizeAction.None;

            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "pathleadingseparator":
                            if (!Enum.TryParse(attribute.Value, true, out leadingSeparator))
                                throw new UrlRewriteException(attribute.Value + " is not a valid normalization action");
                            break;
                        case "pathtrailingseparator":
                            if (!Enum.TryParse(attribute.Value, true, out trailingSeparator))
                                throw new UrlRewriteException(attribute.Value + " is not a valid normalization action");
                            break;
                    }
                }
            }

            return _factory.Create<INormalizeAction>().Initialize(leadingSeparator, trailingSeparator);
        }

        private IRule ParseRewriteMapsElement(XElement element)
        {
            throw new NotImplementedException();
        }

        private IRule ParseAssemblyElement(XElement element)
        {
            var filenameAttribute = element.Attributes().FirstOrDefault(a => a.Name.LocalName.ToLower() == "filename");
            if (filenameAttribute == null) return null;

            var filename = filenameAttribute.Value;
            var assembly = Assembly.Load(filename);

            foreach (var classElement in element.Elements())
            {
                var nameAttribute = classElement.Attributes().FirstOrDefault(a => a.Name.LocalName.ToLower() == "name");
                var typeAttribute = classElement.Attributes().FirstOrDefault(a => a.Name.LocalName.ToLower() == "type");
                var classNameAttribute = classElement.Attributes().FirstOrDefault(a => a.Name.LocalName.ToLower() == "classname");

                if (nameAttribute == null || typeAttribute == null || classNameAttribute == null)
                    continue;

                var type = assembly.GetType(classNameAttribute.Value, false, true);
                if (type == null) type = BuildManager.GetType(classNameAttribute.Value, false);
                if (type == null)
                    throw new UrlRewriteException("Unable to load type " + classNameAttribute.Value + " from " + assembly.FullName);

                switch (typeAttribute.Value.ToLower())
                {
                    case "operation":
                        if (!typeof (IOperation).IsAssignableFrom(type))
                            throw new UrlRewriteException("Type " + type.FullName + " from " + filename + " does not implement IOperation");
                        _customTypeRegistrar.RegisterOperation(type, nameAttribute.Value);
                        break;
                    case "action":
                        if (!typeof(IAction).IsAssignableFrom(type))
                            throw new UrlRewriteException("Type " + type.FullName + " from " + filename + " does not implement IAction");
                        _customTypeRegistrar.RegisterAction(type, nameAttribute.Value);
                        break;
                    case "condition":
                        if (!typeof(ICondition).IsAssignableFrom(type))
                            throw new UrlRewriteException("Type " + type.FullName + " from " + filename + " does not implement ICondition");
                        _customTypeRegistrar.RegisterCondition(type, nameAttribute.Value);
                        break;
                }
            }
            return null;
        }

        private IAction ParseActionElement(XElement element)
        {
            IValueGetter valueGetter = null;
            IAction action = null;
            var appendQueryString = true;

            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "url":
                            valueGetter = ParseTextWithMacros(attribute.Value);
                            break;
                        case "redirectType":
                            switch(attribute.Value)
                            {
                                case "301":
                                    action = ConstructAction("redirectPermenant", element);
                                    break;
                                case "302":
                                    action = ConstructAction("Found", element);
                                    break;
                                case "303":
                                    action = ConstructAction("PermenantRedirect", element);
                                    break;
                                case "307":
                                    action = ConstructAction("Redirect", element);
                                    break;
                            }
                            break;
                        case "type":
                            action = ConstructAction(attribute.Value, element);
                            break;
                        case "appendquerystring":
                            appendQueryString = attribute.Value.ToLower() == "true";
                            break;
                    }
                }
            }

            var actionList = _factory.Create<IActionList>().Initialize();

            if (valueGetter != null)
                actionList.Add(_factory.Create<IReplaceAction>().Initialize(Scope.Url, null, valueGetter));

            if (appendQueryString)
                actionList.Add(_factory.Create<IAppendAction>().Initialize(Scope.QueryString, null, ConstructValueGetter(Scope.OriginalQueryString)));

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

                return operation == null ? operationInputGetter : _factory.Create<IValueConcatenator>().Initialize(operationInputGetter, operation);
            }

            if (input.StartsWith("HTTP_"))
                return ConstructValueGetter(Scope.Header, input.Substring(5).Replace("_", "-"));
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

            return areaGetters.Count == 1 ? areaGetters[0] : _factory.Create<IValueConcatenator>().Initialize(areaGetters);
        }

        #endregion

        #region Private helper functions

        private ICondition CombineConditions(ICondition c1, ICondition c2)
        {
            if (c2 == null) return c1;
            if (c1 == null) return c2;

            var conditionList = c1 as IConditionList;
            if (conditionList == null)
            {
                conditionList = _factory.Create<IConditionList>().Initialize(CombinationLogic.MatchAll);
                conditionList.Add(c1);
            }
            conditionList.Add(c2);
            return conditionList;
        }

        private IAction CombineActions(IAction a1, IAction a2)
        {
            if (a2 == null) return a1;
            if (a1 == null) return a2;

            var actionList = a1 as IActionList;

            if (actionList == null)
            {
                actionList = _factory.Create<IActionList>().Initialize();
                actionList.Add(a1);
            }

            actionList.Add(a2);
            return actionList;
        }

        #endregion
    }
}
