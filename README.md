# UrlRewrite.Net
Drop in replacement for the IIS Url Rewriter that removes all the limitations and perfromance
constraints of the the Microsoft implementation. Drop this into your web site in place of the 
Microsoft Rewrite module and everything will work as before, but now you can extend, enhance
and optimize your rules going forward.

## Why use this?
* Backward compatible with your existing IIS Rewriter module rules.
* Removes all the limitations of the standard Microsoft rewriter module - see features below.
* Order of magnitude faster than the standard Microsoft rewriter module because it is written to be very efficient, 
allows lists of rules to be inside a parent rule, and it supports faster comparison methods than RegEx.

## Features
* All the capabilities of the standard Microsoft IIS rewriter module. See http://www.iis.net/learn/extensions/url-rewrite-module/using-the-url-rewrite-module
* Heirachical rules, ie rules within rules. This allows you to short circuit rule evaluation.
* Regex is supported for backward compatibility but much faster alternatives are provided.
* Can modify query string parameters whereas the Microsoft one only provides an option to append the whole query string or not.
* Ability to replace specific parts of the path or query string without the clunky regex back reference syntax.
* Ability to insert and delete parts of the path and query string as well as delete all except type operations.
* Register your own .Net classes as conditions, actions or operations allowing you to include complex business logic in your rewriter rules.
* Integrates into your application with Dependency Injection.
* Implemented as an IIS managed module.
* All source code is in .Net so you can set break points and trace code if you want.
* Provides a mechanism similar to the Microsoft Failed Request Trace module to trace rule execution.
* Unless rules are set to 'Dynamic' the results of rule evaluation will be cached for subsequent requests.

## Current status
This project is at the pre v1.0 stage. We need to achieve feature parity with the original
Microsoft IIS rewriter module before this can be released so that it can be used as a drop
in replacement.

If you want to get involved and make a contribution please contact the author. If you are 
looking for this kind of functionallity for your website then you will need to wait a few 
months until the tireless men and women of the open source community have worked their magic.

## Roadmap
Estimated completion date for version 1.0 is end of May 2016.

# Getting started
If you already use the Microsoft URL Rewriter module, follow these steps to replace it with this modue.

1. Add a reference to the UrlRewrite.dll assembly. You can compile the source, or install the NuGet package `UrlRewrite.Net`.
2. Add the Rewrite Module to your web.config file.
3. Move your rewriting rules into a separate file (your config could already be set up like this).
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

- Note that your web site needs to be running in Integrated Pipeline mode and not Classic. This is a setting on the App Pool in IIS.

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

# Example rules
Note that these examples use made up sample data. Your rules must be written to match the URLs that
your web site receives.

#### Example of a rules file that uses some of the most common syntax
Permenantly redirects `/company/quote/page3.aspx?date=now` to `/entity/quote/page3.aspx?date=now`

Permenantly redirects `/company/profile/page1.aspx?date=now` to `/entity/profile/page1.aspx?date=now`

Permenantly redirects `/company/financials/page2.aspx?date=now` to `/entity/financials/page2.aspx?date=now`

Does not redirect `/company/history/page1.aspx`

Does not redirect `/company/quote/page1`

For any path like `/company/.../*.aspx` if the session belongs to a customer appends `?customer=true` to the url
```
    <rules name="root">
      <assembly>
        <class name="isCustomer" type="condition" className="MyCompany.Rewrite.Conditions.IsCustomer" />
      </assembly>
    
      <rule name="is a company page">
        <condition scope="pathElement" index="1" test="equals" value="company" />
        <condition scope="pathElement" index="-1" test="endsWith" value=".aspx" />
        <rules name="company page rules>
          <rule name="permenantly redirect urls from v1 site">
            <conditions logicalGrouping="matchAny">
              <condition scope="pathElement" index="2" test="equals" value="quote" />
              <condition scope="pathElement" index="2" test="equals" value="profile" />
              <condition scope="pathElement" index="2" test="equals" value="financials" />
            </conditions>
            <rewrite to="pathElement" toIndex="1" from=="literal" fromIndex="entity" />
            <action type="redirect" redirectType="301" />
    	  </rule>
		  <rule name="flag customers">
			<condition test="isCustomer" />
			<rewrite to="parameter" toIndex="customer" from="literal" fromIndex="true" />
		  </rule>
        </rules>
      </rule
    
    </rules>
```

#### Example of a rule that truncates any path deeper than 3 levels
Rewrites `/company/quote/123/march/2/2016` to `/company/quote/123`
```
    <rule name="truncate paths deeper than 3 levels">
      <condition scope="pathElement" index="4" test="equals" value="" negate="true" />
      <keep scope="path" index="3" />
    </rule
```

#### Example of a rule that removes part of the path and turns in into a query string parameter
Rewrites `/company/123` to `/company?id=123`

Rewrites `/company/123/profile.aspx` to `/company/profile.aspx?id=123`

Does not rewrite `/company` or `/company/`
```
    <rule name="move id to querystring">
      <condition scope="pathElement" index="1" test="equals" value="company" />
      <condition scope="pathElement" index="2" test="equals" value="" negate="true"/>
      <rewrite to="parameter" toIndex="id" from="pathElement" fromIndex="2" />
      <delete scope="pathElement" index="2" />
    </rule
```

#### Example of a rule that changes all PUT requests into POST requests
```
    <rule name="treat all PUT as POST">
      <condition scope="serverVariable" index="REQUEST_METHOD" test="equals" value="PUT" />
      <rewrite to="serverVariable" toIndex="REQUEST_METHOD" from="literal" fromIndex="POST" />
    </rule
```

#### Example of a rule that aborts any request for the `/secure/` part of the site unless it was made over HTTPS
```
    <rule name="ensure security">
      <condition scope="serverVariable" index="HTTPS" test="equals" value="false" />
      <condition scope="pathElement" index="1" test="equals" value="secure" />
      <action type="abortRequest" />
    </rule
```

#### Example of a rule that appends the client's IP address to the querystring for all aspx pages
```
    <rule name="ensure security">
      <condition scope="pathElement" index="-1" test="endsWith" value=".aspx" />
      <rewrite to="parameter" toIndex="clientIp" from="serverVariable" fromIndex="REMOTE_ADDR"/>
    </rule
```

#### Example of a rule that adds `/mobile` to the front of all paths where requests are from a mobile device
If the request is a mobile device, rewrites `/company/123?param=value` to `/mobile/company/123?param=value`.

Note that this assumes you have written a custom .Net type that contains the logic for detecting mobile
devices from the USER_AGENT header in the request.

```
    <rules name="root">
      <assembly>
        <class name="isMobile" type="condition" className="MyCompany.Rewrite.Conditions.IsMobile" />
      </assembly>
      <rule name="special mobile only pages">
        <condition scope="header" index="user-agent" test="isMobile" />
        <insert to="pathElement" toIndex="0" from="literal" fromIndex="mobile"/>
      </rule
    </rules>
```

#### Example of a rule that ensures all URL paths start with a leading / and do not end with
a trailing / separator
```
    <rule name="Always">
      <normalize pathLeadingSeparator="add" pathTrailingSeparator="remove"/>  
    </rule>
```

# Integrating your IoC container

You can optionally integrate the Rewrite Module with an IoC container. You might do this if you want to:
* Extend the Rewrite Module with custom application specific logic and those classes use dependency injection.
* Change the standard behaviour of the Rewrite Module by supplying a different implementation of some of its internal classes.
* Register interfaces in your rules and have these resolve to concrete types at runtime so that you can unit test your rewrite rules.

To integrate your IoC container you need to:
* Implement `UrlRewrite.Interfaces.IFactory` in a way that uses your container to resolve interfaces into concrete classes.
* Pass an instance of your factory class to the `Initialize()` method of the Rewrite Module.
* Register the intefaces that the Rewrite Module uses with your IoC container so that they are resolvable using the factory.

To discover which interfaces you need to register with your IoC container add this code to your solution:
```
    var iocClasses = UrlRewrite.Utilities.IocRegistrations.GetAll();
```

Note that if you use Ninject, then you can more easily register all the Rewrire Module classes with the Ninject IoC container 
by passing an instance of the `UrlRewrite.Utilities.RewriteNinjectModule` class to the Ninject kernel constructor.
```
	IFactory factory = (... your factory construction here ...);
	_iocContainer = new StandardKernel(new RewriteNinjectModule(factory));

```

# Initializing the Rewrite Module
You must call the Rewrite Module static `Initialize()` method once only when your application starts up. You can do this
in the `Application_Start` method of `Global.asax.cs` if you are using ASP.Net. Similar application startup places exist 
for MVC, OWIN and other web development frameworks.

The `Initialize()` method takes a number of optional parameters as follows:

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

If you do not pass a factory there a default factory will be used. The default factory can not map interfaces to
concrete types, it can only construct concrete types by calling the default public constructor. This
only works if all of your custom extensions have a public constructor that takes no parameters.

## `ruleStream`
Pass a stream that can be used to read the rules. This can be a file stream, memory stream, network stream or whatever.
This allows you to store your rewriter rules anywhere you like, including in a database, from an external service etc.

If you do not pass a rule stream then the Rewrite module will attempt to load them from a file called `RewriteRules.config`
in the root folder of the web site.

## `ruleParser`
This is a pretty advanced use case. It allows you to replace the rule syntax with your own syntax. The Rewrite Module parses
your rule file and constructs a set of objects that can execute those rules as efficiently as possible. You dont have 
to use the built-in XML syntax to define your rules, you can define a different syntax here if you want.

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
* `stopProcessing` when true, if this rule matches the incomming request no further rules will be evaluated.

### The `<match>` element
This is mostly for backward compatibility with the Microsoft rewriter V1.0 which did not have the `<conditions>` 
element. If this element matches the incomming request then the `<conditions>` are evaluated. If both `<match>` 
and `<conditions>` match the request, then the rule's `<action>` elements are executed.

Attributes:
* `url` contains the pattern you are looking for in the URL. If the request URL has been modified by a 
prior rule, this rule will try tp match the modified request not the original request.
* `patternSyntax` can be one of `ECMAScript` or `Wildcard `. The default is `ECMAScript` which 
is a flavour of Regular Expression. Wildcard uses the same wildcard scheme as the Windows file system.
* `negate` when true inverts the logic so the rule matches the request when the url is not a match

### The `<conditions>` element
Defines additional conditions that have to be met for the rule to match the incomming request. This element
is optional because it was not present in V1.0 of the Microsoft IIS Rewrite module. This element can contain
`<add>` elements to add conditions. You can set the `<conditions>` element to have `AND` or `OR` logic between
the conditions.

Attributes:
* `logicalGrouping` can be `MatchAll` or `MatchAny`.

### The `<add>` elements inside of `<conditions>`
Adds a condition that must be met for the rule to match the incomming request.

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
* `type` must be one of `Rewrite`, `Redirect`, `CustomResponse`, `AbortRequest` or `None`. Note that `None` is
supported in this Rewrite Module for backward compatibility only, you can achieve the same result by ommitting the `<action>` element. 
Note that this Rewrite Module depreciates the `Rewrite` action type in favor of a more powerful `<rewrite>` element.
* `redirectType` can be 301, 302, 303 or 307. The default is 307 if this attribute is omitted.
* `url` the URL to redirect or rewrite. If this is not provided the last rewrite action will define the url to redirect to.
* `statusLine` only applies when `type` is `CustomResponse`. Sets the status line of the response so that you can return 503 or 204 or whatever.
* `responseLine` only applies when `type` is `CustomResponse`. Sets the body of the response.
* `appendQueryString` adds the original query string to the redirected URL. If false the querystring is stripped off the URL.

### Curly braces
Anything inside curly braces is replaced with information from somewhere else. This provides a way to include 
information from the request and to invoke build-in functions. This curly brace expansion applies to the `url` 
attribute of the `<match>` and `<action>` elements and the `input` attribute of conditions.

The things you can put inside curly braces are:
* The name of a server variable for example `{URL}`. For a complete list see http://www.w3schools.com/asp/coll_servervariables.asp
* A header from the request prefixed with `HTTP_` and with hyphens replaced with underscore, for example `{HTTP_USER_AGENT}`.
* `{C:n}` inserts a back reference the last condition that matched where `n` is the index of the back reference. 
Index 0 is the whole matched string and 1..9 are the capture groups. You can also back reference capture groups from all
conditions instead of just the last one by adding a `trackAllCaptures="true"` attribute to the `<conditions>` element.
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
rules together and make the path through the rules as short as possible. The `<rules>` element is treated like an
action and will be evaluated in sequence along with the other actions if the rule matches the request.

The recommended best practice is to profile traffic to your site and prioritize requests by frequency, then devise a
rule list heirachy that minimizes the rule processing effort for the most frequently occurring requests.

There is no limit to how deep the nest rule lists within rules.

If you set the `stopProcessing` flag on a `<rule>` to `true` this will stop the procesing of any further rules within 
its enclosing `<rules>` element. This will not propogate to the next level up. For example if you have this structure:
```
    <rules name="List 1">
	  <rule name="Rule 1a" />
	  <rule name="Rule 1b" >
        <rules name="List 2">
		  <rule name="Rule 2a" stopProcessing="true">
		  <rule name="Rule 2b">
		  <rule name="Rule 2c">
		</rules>
	  </rule>
	  <rule name="Rule 1c" />
	  <rule name="Rule 1d" />
	</rules>
```

Then if `Rule 2a` matches the request (and it has the `stopProcessing` flag set) this will cause its enclosing
`<rules name="List 2">` element to stop processing any more rules. This means that Rule 2b and Rule 2c will be 
skipped and processing will continue with the evaluation of Rule 1c.

If you want to propogate the `stopProcessing` flag up to the parent, set the `stopProcessing="true"` on the `<rules>`
element like this:
```
    <rules name="List 1">
	  <rule name="Rule 1a" />
	  <rule name="Rule 1b" >
        <rules name="List 2" stopProcessing="true">
		  <rule name="Rule 2a" stopProcessing="true">
		  <rule name="Rule 2b">
		  <rule name="Rule 2c">
		</rules>
	  </rule>
	  <rule name="Rule 1c" />
	  <rule name="Rule 1d" />
	</rules>
```
In this version if Rule 2a matches the request this will skip the rest of the rules in List 2 and
also pass a 'stopProcessing' indication to its parent Rule 1b, which will skip processing on the
rest of List 1, so rule 1c and rule 1d will not be evaluated.

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
			<rewrite to="PathElement" toIndex="-2" from="literal" fromIndex="v3"/>
		  </rule>
		</rules>
	  </rule>

	  <rule name="Form" stopProcessing="true">
		<condition scope="OriginalPathElement" index="-1" test="EndsWith" value=".aspx"/>
		<rules name="Form rules">
		  <rule name="Upper case" stopProcessing="true">
			<condition scope="OriginalPath" test="MatchRegex" value=".*[A-Z].*" ignoreCase="false" />
			<rewrite to="Path" from="OriginalPath" operation="LowerCase"/>
			<action type="Redirect" redirectType="301"/>
		  </rule>
		</rules>
	  </rule>

    </rules>
```
In this example the expensive regular expression to detect upper case in the path is only executed for aspx pages.

### Efficiently accessing different parts of the request
The Microsoft IIS Rewrite module uses regular expressions to test if the request matches the rule. This uses the
entire path by default, and can be configured to compare the query string or the entire url, but either way the
regular expressions to pick out specific parts of the url are quite complex, difficult to read, and expensive
at runtime.

This Rewrite Module takes a different approach, it parses the path into a list of strings, and parses the query 
string into a dictionary. It does this lazily on demand so that the lists and discioranries only get created if
they are required to evaluate the rules.

Because the original incomming request and the rewritten version are structures that can be accessed very quickly,
it allows the rules to be written in a way that is much more readable, and execute much faster. It is much faster
to compare the third element in a list than it is to write and evaluate a regular expression that will do this.
The regular expression will also be cryptic and hard to decipher.

Note that all of the original syntax from the Microsoft implementation is fully supported, this is an extension to
that syntax.

This functionallity can be accessed by adding the new `<condition>` element to your rules. These elements can be
placed inside of `<rule>` elements and inside of `<conditions>` element. When placed inside `<conditions>`
elements the logic used to combine the conditions can be specified with the `logicalGrouping` attribute. When
`<condition>` elements are placed directly inside the `<rule>`, all conditions must be true for the rule to be
matched to the request.

The `<condition>` element can have the following attributes:
* `scope` specifies what part of the request to compare. See below.
* `index` for path element scope this is the numeric index of the path element. Index 0 returns the entire path.
Index 1 is the first element of the path, index 2 the second element etc. If you pass an idex thats larger than the number
of path elements then an empty string is returned for comparison. For example if the request is 
`http://mydomain.com/path1/path2` then path1 is index position 1 amd path2 ia index position 2. You can also set 
the `index` attribute to a negative number to count path elements from right to left. In the previous example index 
-1 returns path2 and -2 returns path1. Larger negative values will return a blank string for comparison.
* `index` for parameter scope this is the name of the parameter. For example if the url is 
`http://mydomain.com/path1/path2?p1=one&p2=two` you can test the value of query string parameter p2 by setting the 
`index="p2"`.
* 'index' for header scope is the name of the header, for example 'user-agent'.
* 'index' for server variable scope is the name of the server variable, for example 'REQUEST_METHOD'.
* `test` specifies how to compare the `scope` to the `value`. The possible values are: `StartsWith`, `EndsWith`, `Contains`,
`Equals`,  `MatchWildcard`, `MatchRegex`, `Greater`, `Less`.
* `value` specifies the literal value to compare with the scope.
* `ignoreCase` defaults to `true` but can be set to `false` for case sensitivity.
* 'negate' defaults to `false` but can be set to `true` to invert the result of the test.

The values of the `scope` attribute can be:
* `OriginalUrl` the full url of the original request regrdless of any rewrite actions that have executed.
* `OriginalPath` just the path part of the original request.
* `OriginalQueryString` just the query string part of the original request.
* `OriginalPathElement` one element from the path part of the original request. Pass a number in the `index` 
attribute to specify the position within the path, or pass 0 as the index to compare the entire path.
* `OriginalParameter` the value of one parameter from the query string part of the original request. Pass a paremater 
name in the `index` attribute to specify the name of the parameter to compare.
* `OriginalHeader` one of the headers from the original request. Pass the name of the header in the `index` attribute.
* `Url` the full url as modified by any rewrite actions that have executed.
* `Path` just the path part of the rewritten url.
* `QueryString` just the query string part of the rewrtten url.
* `PathElement` one element from the path part of the rewritten url. Pass a number in the `index` 
attribute to specify the position within the path, or pass 0 as the index to compare the entire path.
* `Parameter` the value of one parameter from the query string part of the rewritten url. Pass a paremater 
name in the `index` attribute to specify the name of the parameter to compare.
* `Header` one of the headers from the rewritten response. Pass the name of the header in the `index` attribute.
* `OriginalServerVariable` original value one of the IIS server variables. Pass the name of the variable in the `index` parameter.
* `ServerVariable` rewritten value of one of the IIS server variables. Pass the name of the variable in the `index` parameter.
* `Literal` compares a hard coded value contained in the `index` attribute. This is mostly useful in the `<rewrite>` element.
* `MatchGroup` one of the groups from the last `<match>` element that matched the request. Index 0 is the whole match, index 1 is group 1 etc. This is equivalent to the `{R:n}` syntax.
* `ConditionGroup` one of the groups from the last `<condition>` element that matched the request. Index 0 is the whole match, index 1 is group 1 etc. This is equivalent to the `{C:n}` syntax.

### More complex and/or condition support and simplified conditions too
The original version of the IIS Rewrite Module only had the `<match>` element. Version 2 introduced the optional
`<conditions><add /></conditions>` syntax but the `<match>` element is still required. In this Rewrite Module the
`<match>` element is optional and I recommend that you only use it to port existing rewrite rules. For any new rules
the new syntax is more readable and executes much faster.

There is a new `<condition>` element. Element of this type can be placed directly inside the `<rule>` element or grouped
inside a `<conditions>` element. Furthermore `<conditions>` elements can be nested inside of other `<conditions>` elements
to implement more complex and/or logic.

For example you can define two groups of conditions where either group must be true, but within each group all the
conditions must be true. This example would be organized as follows:
```
    <conditions logicalGrouping="MatchAny">
      <conditions logicalGrouping="MatchAll">
        <condition />
        <condition />
	  </conditions>
      <conditions logicalGrouping="MatchAll">
        <condition />
        <condition />
	  </conditions>
    </conditions>
```

The `<conditions>` element has the following attributes:
* `logicalGrouping` can be any of `MatchAll`, `MatchAny` or `MatchNone`.
 
### Selectively modifying the request
In the Microsoft Rewrite module when you define a rewrite action you have to pass the entire URL which means
that you have to use Regex back references to pick up the parts of the request you don't want to modify, and
this complicates the original Regex as well as making the whole rule syntex much less readable.

In this Rewrite Module you can specify exactly which part of the url you want to modify, you can add multiple
`<rewrite>` elements to the `<rule>` element to make multiple modifications. The url in the `<action>` element 
is optional for this Rewrite Module and I strongly recommend that you do not use it. You can still specify the 
url in the `<action>` if you like for backwards compatibility but if you do this then the `<rewrite>` elements 
become ineffective.

The `<rewrite>` element can have the following attributes:
* `to` specifies the scope of where to update the request. See the description of the `scope` attribute of the `<condition>`
element above for a full definition. For this attribute you can not specify any of the Original.. scopes (you can't change
the request that was received).
* `toIndex` specified the index where this is applicable. For example you can overwrite a specific element in the request 
path without having to be concerned with the rest of the path, or modify a specific parameter in the query string leaving
all other query string parameters untouched.
* `from` specifies the scope to get the new value from.
* `fromIndex` specifies the the index within this scope to get the value from if applicable for the scope.
* `operation` specifes an operation to perform on the value after reading it from the `from` scope and before writing it
to the `to` scope. Possible values are `LowerCase`, `UpperCase`, `UrlEncode`, `UrlDecode`. This is optional. You can also 
register your own custom operations - see below.

This Rewrite module also provides a `<delete>` element for removing parts of the request a `<keep>` element to delete
all except certain parts of the request an `<insert>` element and an `<append>` element. These elements allow you to 
perform any modifications to the url that you need. It also has a <normalize> element that allows you to make your
URLs consistent (for example always starting the path with / and never ending the path with /).

The `<delete>` element has the following attributes:
* `scope` identifies which part of the url to delete as `Url`, `Path`, `QueryString`, `PathElement`, `Parameter`, 'Header'.
* `index` specifies the scope index if appropriate for the scope.

The `<keep>` element has the following attributes:
* `scope` identifies which part of the url to modify as `Path`, `QueryString`, `Header`.
* `index` specifies the depth to trim the path to if scope is `Path` for example passing `index="2"` will remove path elements 3 onwards.
* `index` is a comma separated list of parameter names if the scope is `QueryString`. All other parameters will be deleted.
* `index` is a comma separated list of header names if the scope is `Header`. All other headers will be deleted.

The `<insert>` element has the following attributes:
* `scope` identifies which part of the url to insert into `Url`, `Path`, `QueryString`, `PathElement`, `Parameter`, 'Header'.
* `index` specifies the scope index of the element to insert in front of.
* `from` specifies the scope to get the new value from.
* `fromIndex` specifies the the index within this scope to get the value from.
* `operation` specifes an operation to perform on the value after reading it from the `from` scope and before writing it
to the `to` scope. Possible values are `LowerCase`, `UpperCase`, `UrlEncode`, `UrlDecode`. You can also register your own
custom operations - see below.

The `<append>` element has the following attributes:
* `scope` identifies which part of the url to append to `Url`, `Path`, `QueryString`, `PathElement`, `Parameter`, 'Header'.
* `index` specifies the scope index of the element to append to.
* `from` specifies the scope to get the new value from.
* `fromIndex` specifies the the index within this scope to get the value from.
* `operation` specifes an operation to perform on the value after reading it from the `from` scope and before writing it
to the `to` scope. Possible values are `LowerCase`, `UpperCase`, `UrlEncode`, `UrlDecode`. You can also register your own
custom operations - see below.

The `<normalize>` element has the following optional attributes:
* `pathLeadingSeparator` can be `add` or `remove`.
* `pathTrailingSeparator` can be `add` or `remove`.

An example of a rule that makes changes to the request follows:
```
    <rule name="Flatten forms permenantly" stopProcessing="true">
      <condition scope="OriginalPathElement" index="-1" test="EndsWith" value=".aspx" />
      <condition scope="OriginalPathElement" index="3" test="Equals" value="" negate="true" />
      <rewrite to="Path" from="OriginalPath" operation="LowerCase" />
      <rewrite to="PathElement" toIndex="2" from="PathElement" fromIndex="-1" />
      <keep scope="Path" index="2" />
      <keep scope="QueryString" index="page" />
      <action type="Redirect" redirectType="301" />
    </rule>
```
If this rule was run against http://mydomain.com/Companies/Quote/MyCompany.aspx?order=date&Page=3&id=99 
the request would be permenantly redirected to http://mydomain.com/companies/mycompany.aspx?Page=3 because it
matches any request where the path is 3 or more deep and where the last element of the path ends in .aspx, then
it makes the path part of the url all lower case, and copies the last element of the path to the second element
and deletes all subsequent elements in the path. It also removes all query string parameters except for `page`.

### Register your own custom conditions, operations and actions
The features provided by this Rewrite Module can be extended by writing your own conditions, operations and actions. In each 
case you need to register your class first with a unique name and you class needs to implement a specific interface, then
you reference the registered name of your class in the rewrite rules.

Note that this allows you to write rules that hit databases and call external services, but this should be done with
extreme caution. Remember that every request to your site goes through the Rewrite Module, even requests for images, Javascript
files, css files etc. You can alleviate some of these performance concerns by nesting your rules inside other rules that
pre-qualifies the request.

You can register your custom class by adding an <assembly> element inside a `<rules>` element. Assemblies must be 
registered before they are referenced, I recommend that you put them at the very top of your rule file.

The `<assembly>` element has the following attributes:
* `fileName` is the name of the assembly file without the DLL extension. Make sure this DLL is present in the `bin` folder of your web site.

The `<assembly>` element can contain any number of `<class>` elements. These define your extensions. The `<class>` element 
has the following attributes:
* `className` contains the full namespace qualified name of the class. For example `MyCompany.Rewrite.Conditions.IsCustomer`.
* `type` is one of `condition`, `action` or `operation`. Your class must implement the corresponsing interface 
`UrlRewrite.Interfaces.IConditon`, `UrlRewrite.Interfaces.IAction` or `UrlRewrite.Interfaces.IOperation`.
* `name` is the name of your extension. This is the name you will refer to in your Rewrite Module rules.

This example aborts all requests that do not come from a customer:
```
    <rules name="root">
	  <assembly fileName="MyCompany.Rewrite">
	    <class name="isCustomer" type="condition" className="MyCompany.Rewrite.Conditions.IsCustomer" />
	  </assembly>

	  <rule>
		<condition scope="parameter" index="userId" test="isCustomer" negate="true" />
	    <action type="AbortRequest" />
	  </rule>
	<rules>
```

For custom conditions pass the name of your condition in the `test` attribute of the `<condition>` element. In this case the
`scope` and `index` attributes will be used to extract part of the original or rewritten request, and this will be passed
into your custom condition which should then return a boolean result.

For custom actions pass the name of your action to the `type` attribute of the `<action>` element. Your action will be passed
an interface containing details of the request being processed.

For custom operations pass the name of your operation to the `operation` attribute of the `<rewrite>` element. It will be
passed a string and should return a modified version of that string.
