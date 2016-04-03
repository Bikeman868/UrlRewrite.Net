# UrlRewrite.Net
Drop in replacement for the IIS Url Rewriter that fixes all the limitations in the Microsoft implementation

# Why use this?
* Backward compatible with your existing IIS Rewriter module rules.
* Fixes all the limitations of the standard Microsoft rewriter module - see features below.
* Order of magnitude faster than the standard Microsoft rewriter module because it is written to be very efficient, allows lists of rules to be inside a parent rule, and it supports faster comparison methods than RegEx.

# Features
* All the capabilities of the standard Microsoft IIS rewriter module. See http://www.iis.net/learn/extensions/url-rewrite-module/using-the-url-rewrite-module
* Heirachical rules, ie rules within rules. This allows you to short circuit rule evaluation.
* Regex is supported for backward compatibility and much faster alternatives are also supported.
* Can modify query string parameters whereas the Microsoft one only provides an option to append the whole query string or not.
* Ability to replace specific parts of the path or query string without the clunky regex back reference syntax.
* Register your own .Net classes as conditions or actions allowing you to include complex business logic in your rewriter rules.
* Integrates into your application with Dependency Injection.
* Implemented as an IIS managed module.
* All source code is in .Net so you can set break points and trace code if you want.
* Provides a mechanism similar to the Microsoft Failed Request Trace module to trace rule execution.
* Unless rules are set to 'Dynamic' the results of rule evaluation will be cached for subsequent requests.

# Current status
This project is in the design phase at the moment. If you want to get involved and make a contribution please contact the author. If you are 
looking for this kind of functionallity for your website then you will need to wait a few months untill the tireless men and women of the
open source community have worked their magic.

# Roadmap
Estimated completion date for version 1.0 is July 2016.

# Getting started
If you already use the Microsoft URL Rewriter module, follow these steps to replace it with this modue.

1. Add a reference to the UrlRewrite.dll assembly. You can compile the source, or install the NuGet package `UrlRewrite.Net`.
2. Add the Rewrite Module to your web.config file.
3. Move your rewriting rules into a separate file (you may have it like this already).
4. Initialize the Rewrite Module in your application startup code.

These steps are described in more detail below.

## Install the Rewrite Module.
1. In Visual Studio, go to the Tools Menu and choose "NuGet Package Manager" then "Package Manager Console"
2. In the package manager console type `Install-Package UrlRewrite.Net` and press Enter.

## Add the Rewrite Module to your web.config file.
Merge this into your web.config file
```
   <system.webServer>
     <modules runAllManagedModulesForAllRequests="true">
       <remove name="UrlRoutingModule" />
       <add name="UrlRoutingModule" type="UrlRewrite.RewriteModule, UrlRewrite" />
     </modules>
   </system.webServer>
```

- Note that your web site needs to be running in Integrated Pipeline mode and not Classic. This is a setting on the AppPool in IIS.

## Move your rewrite rules to a separate file
In .Net applications config files can store sections of the config file in separate files. If you already have your rewriter
rules in a separate file then you can skip this step. If your existing rules are inline within your web.config file then you
need to do this step. Note that if you are including rewriter rules into your application for the first time, you can simply
create a file in the root of your site called `RewriteRules.config` and skip the rest of this step.

To move the rewriting rules into a separate file, open the `web.config` file and find the `<rewrite>` element. You need
to copy eveything from inside this element and paste it into a new file called `RewriteRules.config`, then you need to replace
the contents of this section with a reference to the file like this:
```
    <rewrite>
      <rules configSource="RewriteRules.config" />
    </rewrite>
```

## Initialize the Rewrite Module
The Rewrite Module must be initialized by your application before it will rewrite any requests. This is usually done in 
the `Application_Start()` method of `Global.asax.cs`.

Note that `Global.asax` runs after the Rewrite Module, so for the very first request to your web site the 
Rewrite Module will not rewrite the request. Until you initialize the Rewrite Module it does not know where the rules are, or
how to construct your custom types.

To initialize the Rewrite Module call the static `Initialize()` method like this:

```
    void Application_Start(object sender, EventArgs e)
    {
	   UrlRewrite.RewriteModule.Initialize();
    }
   
```

The `Initialize()` method has optional parameters you can pass to customize its behaviour, these are described in more detail below.

# Integrating your IoC container

You can optionally integrate the Rewrite Module with an IoC container. You might do this if you want to:
* Extend the Rewrite Module with custom application specific logic and those classes use dependency injection.
* Change the standard behaviour of the Rewrite Module by supplying a different implementation of some of its internal classes.
* You want to register interfaces in your rules and have these resolve to concrete types at runtime.

To integrate your IoC container you need to:
* Implement `UrlRewrite.Interfaces.IFactory` in a way that uses your container to resolve interfaces into concrete classes.
* Pass an instance of your factory class to the `Initialize()` method of the Rewrite Module.
* Register the intefaces that the Rewrite Module uses with your IoC container so that they are resolvable using the factory.

The Rewrite Module needs to be able to resolve the following interfaces via the factory that you supply:
* `UrlRewrite.Interfaces.ILog`
* `UrlRewrite.Interfaces.IFactory`
* `UrlRewrite.Interfaces.IValueGetter`
* `UrlRewrite.Interfaces.IStringMatch`
* `UrlRewrite.Interfaces.INumberMatch`
* `UrlRewrite.Interfaces.IRequestInfo`

# Initializing the Rewrite Module
You must call the Rewrite Module static `Initialize()` method once only when your application starts up. You can do this
in the `Application_Start` method of `Global.asax.cs` if you are using ASP.Net. Similar application startup places exist 
for MVC, OWIN and other web development frameworks.

The Initialize method takes a number of parameters, and they are all optional. The `Initialize()` method parameters are:

## `log`
The Rewrite Module will log exceptions always, and can optionally log a trace of the rewrite rule execution for
specific URLs. If you dont pass this parameter then the log will be output to `Trace`. You can see this in the Output
window of Visual Studio whilst debugging.

## `requestUrlsToTrace`
This is a list of URLs to output trace information for. This is the incomming URL as received by the server from the
browser or other client application. The URLs in this list are case insensitive.

## `rewrittenUrlsToTrace`
This is a list of URLs to output trace information for. This is the rewritten URL that is the result of executing
the rewriter rules. The URLs in this list are case insensitive. Note that adding URLs to this list makes the 
Rewrite Module execute the rules for these URLs twice.

## `factory`
Allows you to integrate an IoC container for resolving interfaces. This is covered in detail above.

If you do not pass a factory here a default factory will be used. The default factory can not map interfaces to
concrete type, it can only construct concrete types by calling the default public constructor. This
works perfectly if all of your custom extensions have a public constructor that takes no parameters.

## `ruleStream`
Pass a stream that can be used to read the rules. This can be a file stream, memory stream, network stream or whatever.
This allows you to store your rewriter rules anywhere you like, including in a database, from an external service etc.

If you do not pass a rele stream then the Rewrite module will attempt to load them from a file called RewriteRules.config
in the root folder of the web site.

## `ruleParser`
This is a pretty advanced use case. It allows you to replace the rule syntax with your own syntax. The Rewrite Module parses
your rule file and constructs a set of objects that can execute those rules as efficiently as possible. You dont have 
to use this XML syntax to define your rules, you can define a different syntax, including loading your rules from
a structured database.

When the Rewrite Module initializes, it passes the rule stream to the parser which produces an IRuleList. Since you can
pass in both the stream and the parser, you can take complete control over the generation of the rule list.

# Rule Syntax

## Backward Compatibility
For backward compatibility the Rewrite Module supports all of the syntax that is defined for the Microsoft IIS Rewriter module.
This symtax is summarized below for convenience and also documented by Microsoft here 
http://www.iis.net/learn/extensions/url-rewrite-module/url-rewrite-module-configuration-reference.

### Example file
This shows the overall structure of the rules file:
```
    <rules>
      <clear />
      <rule name="LowerCaseRule" stopProcessing="true">
        <match url=".*[A-Z].*" ignoreCase="false" />
        <action type="Redirect" url="{ToLower:{URL}}" />
        <conditions>
          <add input="{URL}" pattern="^b/" negate="true" />
        </conditions>
      </rule>
    </rules>
```

Notes:
* The root element of your XML must be `<rules>`. The file must be well formed, valid XML.
* The `<clear />` element is ignored by this Rewrite Module and can safely be deleted.
* The elements inside each rule can appear in any order. The `<action>` elements execute in the order that they appear inside the rule but only after all `<match>` and `<conditions>` elements have been evaluated even if the `<action>` is above the `<conditions>`.
* You can only have one `<match>` and it is usually the first element inside the `<rule>`.

### The `<rule>` element
Attributes:
* `name` is useful in trace output to identify the rule that was being executed.
* 'stopProcessing' when true, if this rule matches the incomming request no further rules will be evaluated.

### The `<match>` element
Attributes:
* 'url' contains the pattern you are looking for in the URL. If the request URL has been modified by a prior rule, this rule will try tp match the modified request not the original request.
* 'patternSyntax' can be one of `ECMAScript` or `Wildcard `. The default is `ECMAScript` which is a flavour of Regular Expression.
* 'negate' when true inverts the logic so the rule matches the request when the url is not a match

### The `<conditions>` element
Attributes:
* `logicalGrouping` can be `MatchAll` or `MatchAny`.

### The `<add>` elements inside of `<conditions>`
Attributes:
* `input` specifies what should be compared. Note that this support curly brace replacements.
* `matchType` can be one of `isFile`, `isDirectory`, `pattern`. The default value is `pattern`.
* `pattern` only applies when the `matchType` is `Pattern`.
* 'ignoreCase' only applies when the `matchType` is `Pattern`.
* `negate` when `true` invets the result.

### The `<action>` element
Attributes:
* `type`can be one of `Rewrite`, `Redirect`, `CustomResponse`, `AbortRequest` or `None`.
* `url` the URL to redirect, rewrite etc as defined by the `type` attribute.
* `statusLine` only applies when `type` is `CustomResponse`.
* `responseLine` only applies when `type` is `CustomResponse`.

### Curly braces
Anything inside curly braces is replaced. This provides a way to includeh information from the request and to invoke build-in functions.
This applies to the `url` attribute of the `<match>` and `<action>` elements and the `input` attribute of conditions.

The things you can put inside curly braces are:
* `{URL}` the request path and query string as modified by the rewriter rules.
* `{REQUEST_FILENAME}`.
* `{QUERY_STRING}`.
* `{HTTP_xxx}` the value of an http header in the request, for example `{HTTP_USER_AGENT}`.
* '{C:n}' inserts a back reference the condition that matched where `n` is the index of the back reference 0-9. Index 0 is the whole matched string and 1..9 are the capture groups.
* '{R:n}' inserts a back reference to the match pattern where `n` is the index of the back reference 0-9. Index 0 is the whole matched string and 1..9 are the capture groups.
* `{ToLower:}` converts the text after the colon to lower case.
* `{UrlEncode:}` converts the text after the colon to its URL encoded form.
* `{UrlDecode:}` converts the text after the colon to its URL decoded form.

## New functionallity
This section defines how the standard rewriter rule syntax was extended to include new features.

### Rules within rules

### Efficiently accessing different parts of the request

### Selectively modifying the request

### Matching the original incomming request rather than the rewritten one

### Register your own custom conditions and actions
