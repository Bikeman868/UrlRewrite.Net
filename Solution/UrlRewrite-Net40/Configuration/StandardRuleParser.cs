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

        public IRuleList Parse(Stream stream, Encoding encoding)
        {
            XDocument document;
            try
            {
                using (var reader = new StreamReader(stream, encoding))
                    document = XDocument.Load(reader);
            }
            catch(Exception ex)
            {
                throw new UrlRewriteException("Failed to load rewriter rules as XDocument", ex);
            }

            var xmlRoot = document.Root;
            if (xmlRoot == null)
                throw new UrlRewriteException("No root element in rules");

            if (xmlRoot.Name != "rewrite")
                throw new UrlRewriteException("The rewriter rules must be an XML document with a <rewrite> root element");

            var context = new ParserContext();

            foreach (var element in xmlRoot.Elements())
            {
                if (element.Name.LocalName.ToLower() == "rewritemaps")
                    ParseRewriteMaps(element, context);

                else if (element.Name.LocalName.ToLower() == "rules")
                    return ParseRulesElement(element, context, "Root");
            }
            throw new UrlRewriteException("The rewriter rules must have a <rules> element inside the root <rewrite> element");
        }

        #region Constructing instances using factories and custom type registrations

        private IAction ConstructAction(string actionName, XElement configuration)
        {
            switch (actionName.ToLower())
            {
                case "redirect":
                    return _factory.Create<IRedirectAction>().Initialize(configuration);
                case "customresponse":
                    return _factory.Create<ICustomResponse>().Initialize(configuration);
                case "abortrequest":
                    return _factory.Create<IAbortAction>().Initialize();
                case "none":
                    return _factory.Create<IDoNothingAction>().Initialize();
                default:
                    return _customTypeRegistrar.ConstructAction(actionName, configuration);
            }
        }

        private IOperation ConstructOperation(string operationName, ParserContext context)
        {
            operationName = operationName.ToLower();

            switch (operationName)
            {
                case "tolower":
                    return _factory.Create(typeof(LowerCaseOperation)) as IOperation;
                case "toupper":
                    return _factory.Create(typeof(UpperCaseOperation)) as IOperation;
                case "urlencode":
                    return _factory.Create(typeof(UrlEncodeOperation)) as IOperation;
                case "urldecode":
                    return _factory.Create(typeof(UrlDecodeOperation)) as IOperation;
            }
            
            if (context.RewriteMaps.ContainsKey(operationName))
                return context.RewriteMaps[operationName];

            return _customTypeRegistrar.ConstructOperation(operationName);
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

        private void ParseRewriteMaps(XElement element, ParserContext context)
        {
            foreach (var child in element.Elements())
            {
                if (child.Name.LocalName.ToLower() == "rewritemap")
                {
                    var rewriteMap = new RewriteMapOperation();
                    rewriteMap.Initialize(child);
                    context.RewriteMaps[rewriteMap.Name.ToLower()] = rewriteMap;
                }
            }
        }

        private IRuleList ParseRulesElement(XElement element, ParserContext context, string defaultName)
        {
            var name = defaultName;
            var stopProcessing = true;

            var rules = element
                .Nodes()
                .Where(n => n.NodeType == XmlNodeType.Element)
                .Cast<XElement>()
                .Select<XElement, IRule>(e =>
                {
                    switch (e.Name.LocalName.ToLower())
                    {
                        case "rule":
                            return ParseRuleElement(e, context);
                        case "assembly":
                            return ParseAssemblyElement(e, context);
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

        private IRule ParseRuleElement(XElement element, ParserContext context)
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
                        case "enabled":
                            if (attribute.Value.ToLower() != "true")
                                return null;
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
                        condition = CombineConditions(condition, ParseMatchElement(child, context));
                        break;
                    case "condition":
                        condition = CombineConditions(condition, ParseConditionElement(child, context));
                        break;
                    case "conditions":
                        condition = CombineConditions(condition, ParseConditionsElement(child, context));
                        break;
                    case "action":
                        action = CombineActions(action, ParseActionElement(child, context));
                        break;
                    case "rewrite":
                        action = CombineActions(action, ParseRewriteElement(child, context));
                        break;
                    case "rules":
                        action = CombineActions(action, ParseRulesElement(child, context, Guid.NewGuid().ToString()));
                        break;
                    case "delete":
                        action = CombineActions(action, ParseDeleteElement(child, context));
                        break;
                    case "keep":
                        action = CombineActions(action, ParseKeepElement(child, context));
                        break;
                    case "insert":
                        action = CombineActions(action, ParseInsertElement(child, context));
                        break;
                    case "append":
                        action = CombineActions(action, ParseAppendElement(child, context));
                        break;
                    case "normalize":
                        action = CombineActions(action, ParseNormalizeElement(child, context));
                        break;
                }
            }

            return _factory.Create<IRule>().Initialize(name, condition, action, stopProcessing, isDynamic);
        }

        private ICondition ParseMatchElement(XElement element, ParserContext context)
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
                            scope = Scope.MatchPath;
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

        private ICondition ParseConditionsElement(XElement element, ParserContext context)
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
                        result = CombineConditions(result, ParseConditionsAddElement(child, context));
                        break;
                    case "condition":
                        result = CombineConditions(result, ParseConditionElement(child, context));
                        break;
                    case "conditions":
                        result = CombineConditions(result, ParseConditionsElement(child, context));
                        break;
                }
            }
            return result;
        }

        private ICondition ParseConditionElement(XElement element, ParserContext context)
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

        private ICondition ParseConditionsAddElement(XElement element, ParserContext context)
        {
            IValueGetter valueGetter = null;
            var defaultScope = Scope.Url; // only applies when there is no "input" attribute
            var compareOperation = CompareOperation.MatchRegex;
            var inverted = false;
            var ignoreCase = true;
            var text = ".*";
            var isFileMatch = false;
            var isDirectoryMatch = false;

            if (element.HasAttributes)
            {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName.ToLower())
                    {
                        case "input":
                            valueGetter = ParseTextWithMacros(attribute.Value, context);
                            break;
                        case "matchtype":
                            defaultScope = Scope.Path;
                            if (attribute.Value.ToLower() == "isfile")
                                isFileMatch = true;
                            else if (attribute.Value.ToLower() == "isdirectory")
                                isDirectoryMatch = true;
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
                valueGetter = ConstructValueGetter(defaultScope);

            if (isFileMatch || isDirectoryMatch)
                return _factory.Create<IStaticFileMatch>().Initialize(valueGetter, isDirectoryMatch, inverted);
            return _factory.Create<IStringMatch>().Initialize(valueGetter, compareOperation, text, inverted, ignoreCase);
        }

        private IAction ParseRewriteElement(XElement element, ParserContext context)
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
                            operation = ConstructOperation(attribute.Value, context);
                            break;
                        case "value":
                            fromScope = Scope.Literal;
                            fromIndex = attribute.Value;
                            break;
                    }
                }
            }

            var value = toScope == Scope.Literal 
                ? ParseTextWithMacros(fromIndex, context, operation)
                : _factory.Create<IValueGetter>().Initialize(fromScope, fromIndex, operation);

            return _factory.Create<IReplaceAction>().Initialize(toScope, toIndex, value);
        }


        private IAction ParseDeleteElement(XElement element, ParserContext context)
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

        private IAction ParseInsertElement(XElement element, ParserContext context)
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
                            operation = ConstructOperation(attribute.Value, context);
                            break;
                        case "value":
                            fromScope = Scope.Literal;
                            fromIndex = attribute.Value;
                            break;
                    }
                }
            }

            var value = toScope == Scope.Literal
                ? ParseTextWithMacros(fromIndex, context, operation)
                : _factory.Create<IValueGetter>().Initialize(fromScope, fromIndex, operation);

            return _factory.Create<IInsertAction>().Initialize(toScope, toIndex, value);
        }

        private IAction ParseAppendElement(XElement element, ParserContext context)
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
                            operation = ConstructOperation(attribute.Value, context);
                            break;
                        case "value":
                            fromScope = Scope.Literal;
                            fromIndex = attribute.Value;
                            break;
                    }
                }
            }

            var value = toScope == Scope.Literal
                ? ParseTextWithMacros(fromIndex, context, operation)
                : _factory.Create<IValueGetter>().Initialize(fromScope, fromIndex, operation);

            return _factory.Create<IAppendAction>().Initialize(toScope, toIndex, value);
        }

        private IAction ParseKeepElement(XElement element, ParserContext context)
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

        private IAction ParseNormalizeElement(XElement element, ParserContext context)
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

        private IRule ParseAssemblyElement(XElement element, ParserContext context)
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

        private IAction ParseActionElement(XElement element, ParserContext context)
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
                            valueGetter = ParseTextWithMacros(attribute.Value, context, new AbsoluteUrlOperation());
                            break;
                        case "redirectType":
                            action = ConstructAction("Redirect", element);
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
            {
                actionList.Add(_factory.Create<IReplaceAction>().Initialize(Scope.Url, null, valueGetter));

                if (appendQueryString)
                    actionList.Add(_factory.Create<IAppendAction>().Initialize(Scope.QueryString, null, ConstructValueGetter(Scope.OriginalQueryString)));
            }

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

        private IValueGetter ParseMacro(string input, ParserContext context)
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

                var operationInputGetter = ParseTextWithMacros(input, context);
                var operation = ConstructOperation(operationName, context);

                return operation == null ? operationInputGetter : _factory.Create<IValueConcatenator>().Initialize(operationInputGetter, operation);
            }

            if (input.StartsWith("HTTP_"))
                return ConstructValueGetter(Scope.Header, input.Substring(5).Replace("_", "-"));
            return ConstructValueGetter(Scope.ServerVariable, input);
        }

        private IValueGetter ParseTextWithMacros(string input, ParserContext context, IOperation operation = null)
        {
            var textAreas = SeparateMarcoText(input.Trim());
            if (textAreas == null || textAreas.Count == 0) return null;

            var areaGetters = textAreas
                .Select(a => a.IsQuoted
                    ? ParseMacro(a.Text, context) 
                    : ConstructValueGetter(Scope.Literal, a.Text))
                .ToList();

            return areaGetters.Count == 1  && operation == null
                ? areaGetters[0] 
                : _factory.Create<IValueConcatenator>().Initialize(areaGetters, null, operation);
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

        #region Parser context

        private class ParserContext
        {
            public IDictionary<string, IOperation> RewriteMaps;

            public ParserContext()
            {
                RewriteMaps = new Dictionary<string, IOperation>();
            }
        }

        #endregion
    }
}
