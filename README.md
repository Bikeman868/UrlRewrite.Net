# UrlRewrite.Net
Drop in replacement for the IIS Url Rewriter that fixes all the limitations in the Microsoft implementation

## Why use this?
* Backward compatible with your existing IIS Rewriter module rules.
* Fixes all the limitations of the standard Microsoft rewriter module - see features below.
* Order of magnitude faster than the standard Microsoft rewriter module because it is written to be very efficient, allows lists of rules to be inside a parent rule, and it supports faster comparison methods than RegEx.

## Features
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

## Current status
This project is in the design phase at the moment. If you want to get involved and make a contribution please contact the author. If you are 
looking for this kind of functionallity for your website then you will need to wait a few months untill the tireless men and women of the
open source community have worked their magic.

## Roadmap
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
* The elements inside each rule can appear in any order. The `<action>` elements execute in the order 
that they appear inside the rule but only after all `<match>` and `<conditions>` elements have been 
evaluated even if the `<action>` is above the `<conditions>`.
* You can only have one `<match>` and it is usually the first element inside the `<rule>`.

### The `<rule>` element
Defines a rule. Rules are executed in order until a rule matches and has `stopProcessing` set to true, or
the end of the rule list is reached. If the `<match>` and `<conditions>` match the incomming request 
then the `<action>` elements are executed otherwise they are skipped.

Attributes:
* `name` is useful in trace output to identify the rule that was being executed.
* 'stopProcessing' when true, if this rule matches the incomming request no further rules will be evaluated.

### The `<match>` element
This is mostly for backward compatibility with the Microsoft rewriter V1.0 which did not have the `<conditions>` 
element. If this element matches the incomming request then the `<conditions>` are evaluated. If both `<match>` 
and `<conditions>` match the request, then the rule's `<action>` elements are executed.

Attributes:
* 'url' contains the pattern you are looking for in the URL. If the request URL has been modified by a 
prior rule, this rule will try tp match the modified request not the original request.
* 'patternSyntax' can be one of `ECMAScript` or `Wildcard `. The default is `ECMAScript` which 
is a flavour of Regular Expression. Wildcard uses the same wildcard scheme as the Windows file system.
* 'negate' when true inverts the logic so the rule matches the request when the url is not a match

### The `<conditions>` element
Defines additional conditions that have to be met for the rule to match the incomming request. This element
is optional because it was not present in V1.0 of the Microsoft IIS Rewrite module. This element can contain
`<add>` elements to add conditions. You can set the `<conditions>` element to have `AND` or `OR` logic between
the conditions.

Attributes:
* `logicalGrouping` can be `MatchAll` or `MatchAny`.

### The `<add>` elements inside of `<conditions>`
Adds a contition that must be met for the rule to match the incomming request.

Attributes:
* `input` specifies what should be compared. Note that this supports curly brace replacements.
* `matchType` can be one of `isFile`, `isDirectory` or `pattern`. The default value is `pattern`.
* `pattern` only applies when the `matchType` is `pattern`. Contains a Regular Expression.
* `ignoreCase` only applies when the `matchType` is `pattern`.
* `negate` when `true` inverts the result.

### The `<action>` element
Defines an action to take if the rule matches the request. The rule can contain multiple actions, so for example
you can modify the path and add a query string parameter in two separate actions and they will both be acted upon.

Note that `Redirect`, `CustomResponse` and `AbortRequest` actions all stop any further rule processing and
return a response back to the client even if the `stopProcessing` attribute of the rule is `false`.

Attributes:
* `type`can be one of `Rewrite`, `Redirect`, `CustomResponse`, `AbortRequest` or `None`.
* `url` the URL to redirect or rewrite.
* `statusLine` only applies when `type` is `CustomResponse`. Sets the status line of the response so that you can return 503 or 204 or whatever.
* `responseLine` only applies when `type` is `CustomResponse`. Sets the body of the response.

### Curly braces
Anything inside curly braces is replaced with information from elsewhere. This provides a way to include 
information from the request and to invoke build-in functions. This applies to the `url` attribute of the 
`<match>` and `<action>` elements and the `input` attribute of conditions.

The things you can put inside curly braces are:
* The name of a server variable for example `{URL}`. For a complete list see http://www.w3schools.com/asp/coll_servervariables.asp
* A header from the request prefixed with `HTTP_`, for example `{HTTP_USER_AGENT}`.
* `{C:n}` inserts a back reference the last condition that matched where `n` is the index of the back reference. 
Index 0 is the whole matched string and 1..9 are the capture groups.
* `{R:n}` inserts a back reference to the match pattern where `n` is the index of the back reference. 
Index 0 is the whole matched string and 1..9 are the capture groups.
* `{ToLower:}` converts the text after the colon to lower case. You can nest curly braces after the color, eg `{ToLower:{URL}}`
* `{UrlEncode:}` converts the text after the colon to its URL encoded form.
* `{UrlDecode:}` converts the text after the colon to its URL decoded form.

## New functionallity
This section defines how the standard Microsoft rewriter rule syntax was extended to include the new features
available in this Rewrite Module.

### Rules within rules
This Rewrite Module allows you to put another `<rules>` element inside of a `<rule>` element. If the rule does not
match the request then all of the rules inside the `<rules>` element are skipped. Use this feature to group similar 
rules together and make the path through the rules as short as possible.

The recommended best practice is to profile traffic to your site and prioritize requests by frequency, then devise a
rule list heirachy that minimizes the rule processing for the most frequently occurring requests.

There is no limit to how deep the nest rule lists within rules.

Example:
```
    <rules name="Root">

	  <rule name="Image" stopProcessing="true">
		<conditions logicalGrouping="MatchAny">
		  <condition scope="OriginalPathElement" index="-1" test="EndsWith" value=".bmp"/>
		  <condition scope="OriginalPathElement" index="-1" test="EndsWith" value=".jpg"/>
		  <condition scope="OriginalPathElement" index="-1" test="EndsWith" value=".png"/>
		  <condition scope="OriginalPathElement" index="-1" test="EndsWith" value=".svg"/>
		</conditions>
		<rules name="Image rules">
			<rule />
			<rule />
		</rules>
	  </rule>

	  <rule name="Style" stopProcessing="true">
		<condition scope="OriginalPathElement" index="-1" test="EndsWith" value=".css"/>
		<rules name="Style rules">
		  <rule name="Old version">
  		    <conditions logicalGrouping="MatchAny">
			  <condition scope="PathElement" index="-2" test="Equals" value="v1" />
			  <condition scope="PathElement" index="-2" test="Equals" value="v2" />
			</conditions>
			<action type="Rewrite" scope="PathElement" index="-2" value="v3"/>
		  </rule>
		</rules>
	  </rule>

	  <rule name="Form" stopProcessing="true">
		<condition scope="OriginalPathElement" index="-1" test="EndsWith" value=".aspx"/>
		<rules name="Form rules">
		  <rule name="Upper case" stopProcessing="true">
			<condition scope="OriginalPath" test="MatchRegex" value=".*[A-Z].*" ignoreCase="false" />
			<action type="RedirectPermenant" scope="Url">
				<value scope="OriginalUrl" operation="LowerCase" />
			</action>
		  </rule>
		</rules>
	  </rule>

    </rules>
```
In this example the expensive regular expression to detect upper case in the path is only executed for aspx pages.

### Efficiently accessing different parts of the request

### More complex and/or condition support

### Selectively modifying the request

### Matching the original incomming request rather than the rewritten one

### Register your own custom conditions and actions
