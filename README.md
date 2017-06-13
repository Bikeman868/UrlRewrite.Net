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
Version 1.0.0 was functionally complete except for the caching of non-dynamic requests. This
version was published to NuGet so that I can get feedback on backwards compatibility.

The standard IIS rewrite module is not very precicely documented so I need some people out there
to try is with their rewrite rules and let me know where the behaviour is different between this
Rewrite Module and the Microsoft one. I am aiming for 100% backwards compatibility.

If you are starting from scratch and don't need compatibility with an existing set of rewriter rules
then go for it, this code is stable and production ready. I already have it on all of my own web sites
and have started optimizing the performance of my rules.

## Roadmap
1. Estimated completion date for version 1.0 is end of May 2016.[ Actually completed end of April 2016]
2. Caching of non-dynamic requests. This involves hashing the url and doing a lookup on the redirection
results rather then re-evaluating the rules each time. If any of your rules produce different results
each time they are run (because they lookup in a database, or use the current time of day for example)
then be sure to mark them as `Dynamic` so that the results will not be cached.
3. 100% unit test coverage.
4. Outbound rules. This is a feature of the IIS rewriter that I only just discovered.
5. Ability to rewrite the 'host' part of the request.

## NuGet Package Versions
These are the versions that had major feature improvements or breaking changes you need to be aware of.

|Version|Comments|
|---|---|
|1.0.0|First production ready release April 2016|
|1.0.1|Adds the handler to the web.config file when it is installed|
|1.0.6|Added support for {} in literal values|
|1.0.7|Rules have an enabled property - useful during testing and debugging|
|1.1.0|BREAKING CHANGE - method sugnature of the `Initialize()` method changed to provide more flexibility in selecting which requests to trace|
|1.1.1|BREAKING CHANGE - the default value for the `stopProcessing` attribute on the `<rules>` element changed from `false` to `true`|
|     |Added a `value="my value"` attribute to `<rewrite>`, `<append>` and `<insert>` as a shorthand for `from="literal" fromIndex="my value"`|
|1.1.2|Added support for version 4.0 and 4.5 of the .Net Framework|
|1.2.0|The IsFile and IsDirectory matchType now check for the existence of a physical file on disk|
|1.2.1|Uodated version numebr of dependant packages|
|1.2.2|Trace log includes both values in comparison operations|
|1.2.3|Made it easier to write your own custom parser by using IoC to construct actions and conditions|
|1.2.4|Fixed bugs found by raerae1616 in https://github.com/Bikeman868/UrlRewrite.Net/issues/10 |
|1.2.5|Fixed bug with macro expansion in literals |
|1.2.6|Added support for negative indexes when appending to a path element|
|1.2.7|Fixed a bug in the `<insert>` action which was expecting an `index` attribute rather than `toIndex` as documented. `index` still works for backward compatibility.|

# Getting started
If you already use the Microsoft URL Rewriter module, follow these steps to replace it with this modue.

1. Add a reference to the UrlRewrite.dll assembly. You can compile the source, or install the NuGet package `UrlRewrite.Net`.
2. Add the Rewrite Module to your web.config file.
3. Move your rewriting rules into a separate file (unless your config file is already set up like this).
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

> Note that your web site needs to be running in Integrated Pipeline mode and not Classic. This is a setting on the App Pool in IIS.

> Note that the NuGet package installer will attempt to make this change for you but I recommend 
> that you check that the changes were made correctly.

## Move your rewrite rules to a separate file
In .Net applications config files can store sections of the config file in separate files. If you have 
already configured your application like this, then then you should rename the file containig your
rules to `RewriteRules.config`.

If you are starting from scratch then create a file called `RewriteRules.config` in the root folder of your
web site and type your rules into it as described in the rest of this document.

If you use the Microsoft Url rewriter already and have the rules directly in your `web.config file`, then
you will need to cut the `<rewrite>` section from your `web.config` file and paste them into a new 
file called `RewriteRules.config`. In this case if you want to be able to switch back to the Microsoft
Url rewriter, then replace the `<rewrite>` section in your `web.config` which this:
```
    <rewrite configSource="RewriteRules.config" />
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
    <rewrite>
      <rules name="root">

        <assembly fileName="MyCompany.Rewrite">
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
              <rewrite to="pathElement" toIndex="1" from="literal" fromIndex="entity" />
              <action type="redirect" redirectType="301" />
      	    </rule>

  		    <rule name="flag customers">
  			  <condition test="isCustomer" />
  			  <rewrite to="parameter" toIndex="customer" value="true" />
  		    </rule>
          </rules>
        </rule
      
      </rules>
	</rewrite>
```

#### Example of a rule that truncates any path deeper than 3 levels
Rewrites `/company/quote/123/march/2/2016` to `/company/quote/123`
```
    <rule name="truncate paths deeper than 3 levels">
      <condition scope="pathElement" index="4" test="equals" value="" negate="true" />
      <keep scope="path" index="3" />
    </rule
```

#### Example of a rule that appends a default page to an empty path
Rewrites `/` to `/home.aspx`
```
    <rule name="default home page">
      <condition scope="pathElement" index="1" test="equals" value="" />
      <append scope="path" value="home.aspx" />
    </rule
```

#### Example of a rule that moves part of the path to a query string parameter
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
      <rewrite to="serverVariable" toIndex="REQUEST_METHOD" value="POST" />
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
      <assembly fileName="MyCompany.Rewrite">
        <class name="isMobile" type="condition" className="MyCompany.Rewrite.Conditions.IsMobile" />
      </assembly>
      <rule name="special mobile only pages">
        <condition scope="header" index="user-agent" test="isMobile" />
        <insert to="pathElement" toIndex="0" from="literal" fromIndex="mobile"/>
      </rule
    </rules>
```

#### Example of a rule that ensures all URL paths start with a leading / and do not end with a trailing / separator
```
    <rule name="Always">
      <normalize pathLeadingSeparator="add" pathTrailingSeparator="remove"/>  
    </rule>
```

#### Example of a rule that maps all virtual URLs (no physical file) to index.html.
```
    <rule name="AngularJS">
      <conditions logicalGrouping="MatchAll">
        <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
        <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
      </conditions>
      <action type="Rewrite" url="/index.html" />    
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

The Rewrite Module uses the `Ioc.Modules` NuGet package to configure IoC. If you also use this method within your application
then IoC configuration will happen automatically because IoC Modules will discover the `Package` class in the Rewrite Module
and register its IoC needs with your application container.

If you don't pass a `IFactory` implementation when you initialize the Rewrite Module, then it will use Ninject internally.

Note that if you use Ninject, then you can more easily register all the Rewrire Module classes with the Ninject IoC container 
by passing an instance of the `UrlRewrite.Utilities.RewriteNinjectModule` class to the Ninject kernel constructor like this:
```
	IFactory factory = (... your factory construction here ...);
	_iocContainer = new StandardKernel(new RewriteNinjectModule(factory));

```

# Initializing the Rewrite Module
You must call the Rewrite Module static `Initialize()` method once only when your application starts up. You can do this
in the `Application_Start` method of `Global.asax.cs` if you are using ASP.Net. Similar application startup places exist 
for MVC, OWIN and other web development frameworks.

The `Initialize()` method takes a number of optional parameters as follows:

### `log`
The Rewrite Module will log exceptions always, and can optionally log a trace of the rewrite rule execution for
specific URLs. If you dont pass this parameter then the log will be output to `Trace`. You can see this in the Output
window of Visual Studio whilst debugging.

### `forwardTracePredicate`
This function is called for each request to determine whether to output trace information for the incomming URL. 
This function is passed the URL as received by the server from the browser and should return true to trace the
execution of rewrite rules for this request.

Passing null for this parameter will speed up processing of the requests.

### `reverseTracePredicate`
This function is called for each request to determine whether to output trace information for the rewritten reqeust.
This function is passed the URL that results from executing the rewrite rules and should return true to re-run the 
rules with logging enabled.

Passing null for this parameter will speed up processing of the requests.

### `factory`
Allows you to integrate an IoC container for resolving interfaces. This is covered in detail above.

If you do not pass a factory there a default factory will be used. The default factory can not map interfaces to
concrete types, it can only construct concrete types by calling the default public constructor. This
only works if all of your custom extensions have a public constructor that takes no parameters.

### `ruleStream`
Pass a stream that can be used to read the rules. This can be a file stream, memory stream, network stream or whatever.
This allows you to store your rewriter rules anywhere you like, including in a database, from an external service etc.

If you do not pass a rule stream then the Rewrite module will attempt to load them from a file called `RewriteRules.config`
in the root folder of the web site.

### `ruleParser`
This is a pretty advanced use case. It allows you to replace the rule syntax with your own syntax. The Rewrite Module parses
your rule file and constructs a set of objects that can execute those rules as efficiently as possible. You dont have 
to use the built-in XML syntax to define your rules, you can define a different syntax here if you want.

When the Rewrite Module initializes, it passes the rule stream to the parser which produces an IRuleList. Since you can
pass in both the stream and the parser, you can take complete control over the generation of the rule list.

# Rule Syntax

## Backward Compatibility
For backward compatibility the Rewrite Module supports all of the syntax that is defined for the Microsoft IIS Rewriter module.
This symtax is summarized below for convenience and also documented by Microsoft here 
http://www.iis.net/learn/extensions/url-rewrite-module/url-rewrite-module-configuration-reference. Note that the Microsoft documentation is very thin and full of holes, so you might find that there are behaviours of the Microsoft implementation that you rely on but I did not realize existed because they arn't documented. In these cases please let me know so I can get as close as possible to 100% backward compatibility.

### Example file
This shows the overall structure of the rules file:
```
    <rewrite>
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
    </rewrite>
```

Notes:
* The root element of your XML must be `<rewrite>`. The file must be well formed, valid XML.
* Inside of the `<rewrite>` element you can have a `<rewriteMaps>` element and a `<rules>` element.
* The `<clear />` element is ignored by this Rewrite Module and can safely be deleted.
* The elements inside each rule can appear in any order. The `<action>` elements execute in the order 
that they appear inside the rule but only after all `<match>` and `<conditions>` elements have been 
evaluated even if the `<action>` is above the `<conditions>`.
* You can only have one `<match>` and it is usually the first element inside the `<rule>`.
* For backwards compatibility the `<match url=""/>` element strips the leading `/` from the URL before 
trying to match it to the regular expression. Also the `<action type="Redirect" url="" />`
element adds a `/` to the front of the URL. This creates behaviour that matches the standard IIS rewrite
module for backwards compatibility. I recommend that you don't use this syntax in any new rules
that you write.

### The `<rule>` element
Defines a rule. Rules are executed in order until a rule matches and has `stopProcessing` set to true, or
the end of the rule list is reached. If the `<match>` and `<conditions>` match the incomming request 
then the `<action>` elements are executed otherwise they are skipped.

Attributes:
* `name` is useful in trace output to identify the rule that was being executed. If you dont specify a name it will be a GUID.
* `stopProcessing` when true, if this rule matches the incomming request no further rules will be evaluated.

### The `<match>` element
This is mostly for backward compatibility with the Microsoft rewriter V1.0 which did not have the `<conditions>` 
element. If this element matches the incomming request then the `<conditions>` are evaluated. If both `<match>` 
and `<conditions>` match the request, then the rule's `<action>` elements are executed.

Attributes:
* `url` contains the pattern you are looking for in the URL. If the request URL has been modified by a 
prior rule, this rule will try to match the modified request not the original request. The request URL 
will have the leading `/` removed from it prior to matching.
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
* `redirectType` can be 301, 302, 303 or 307. The default is 307 if this attribute is omitted. See https://en.wikipedia.org/wiki/List_of_HTTP_status_codes for more information.
* `url` the URL to redirect or rewrite. If this is not provided the last rewrite action will define the url to redirect to.
* `statusLine` only applies when `type` is `CustomResponse`. Sets the status line of the response so that you can return 503 or 204 or whatever.
* `responseLine` only applies when `type` is `CustomResponse`. Sets the body of the response.
* `appendQueryString` adds the original query string to the redirected URL. If false the querystring is stripped off the URL.

### Rewrite maps
See http://www.iis.net/learn/extensions/url-rewrite-module/using-rewrite-maps-in-url-rewrite-module for
more details on what rewrite maps are and how to configure them.

Example rewrite map usage:
```
    <rewrite>
      <rewriteMaps>
        <rewriteMap name="StaticRewrites" defaultValue="">
          <add key="/article1" value="/article.aspx?id=1&amp;title=some-title" />
          <add key="/some-title" value="/article.aspx?id=1&amp;title=some-title" />
          <add key="/post/some-title.html" value="/article.aspx?id=1&amp;title=some-title" />
        </rewriteMap>
      </rewriteMaps>
      <rules>
        <rule name="Rewrite Rule">
          <match url=".*" />
          <conditions>
            <add input="{StaticRewrites:{REQUEST_URI}}" pattern="(.+)" />
          </conditions>
          <action type="Rewrite" url="{C:1}" />
        </rule>
      </rules>
    </rewrite>
```
Effectively the rewrite map becomes a custom function that does a dictionary lookup. You can
reference this function using the curly braces syntax anywhere where this is supported as
described below.

### Curly braces
Anything inside curly braces is replaced with information from somewhere else. This provides a way to include 
information from the request and to invoke build-in functions. This curly brace expansion applies to the `url` 
attribute of the `<match>` and `<action>` elements. the `input` attribute of conditions and literal values.

The things you can put inside curly braces are:
* The name of a server variable for example `{URL}`. For a complete list see http://www.w3schools.com/asp/coll_servervariables.asp
* A header from the request prefixed with `HTTP_` and with hyphens replaced with underscore, for example `{HTTP_USER_AGENT}`.
* The name of a rewrite map followed by colon. This takes the value after the colon and uses it as a key into the rewrite map dictionary.
* `{c:n}` inserts a back reference the last condition that matched where `n` is the index of the back reference. 
Index 0 is the whole matched string and 1..9 are the capture groups. You can also back reference capture groups from all
conditions instead of just the last one by adding a `trackAllCaptures="true"` attribute to the `<conditions>` element.
* `{r:n}` inserts a back reference to the `<match>` pattern where `n` is the index of the back reference. 
Index 0 is the whole matched string and 1..9 are the capture groups.
* `{toLower:}` converts the text after the colon to lower case. You can nest curly braces after the colon, eg `{toLower:{URL}}`
* `{urlEncode:}` converts the text after the colon to its URL encoded form. You can nest curly braces after the colon.
* `{urlDecode:}` converts the text after the colon to its URL decoded form. You can nest curly braces after the colon.

## New functionallity
This section defines how the standard Microsoft rewriter rule syntax was extended to include the new features
available in this Rewrite Module.

### Curly brace extensions
In this Rewrite Module implementation the curly brace syntax has been extended to include
* The name of a custom operation followed by a colon. Custom operations can be registered with the `<class>` element. The value 
after the colon will be the input to your custom function, this can be another curly brace expension, for example 
`{myCustomOperation:{toLower:{HTTP_USER_AGENT}}}`
* `{toUpper:}` converts the text after the colon to upper case. You can nest curly braces after the colon.

### Rules within rules
This Rewrite Module allows you to put another `<rules>` element inside of a `<rule>` element. If the rule does not
match the request then all of the rules inside the `<rules>` element are skipped. Use this feature to group similar 
rules together and make the path through the rules as short as possible. The `<rules>` element is treated like an
action and will be evaluated in sequence along with the other actions if the rule matches the request.

The recommended best practice is to profile traffic to your site and prioritize requests by frequency, then devise a
rule list heirachy that minimizes the rule processing effort for the most frequently occurring requests.

There is no limit to how deep the nest rule lists within rules.

If you set the `stopProcessing` flag on a `<rule>` to `true` this will stop the procesing of any further rules within 
its enclosing `<rules>` element. This will only propogate to the next level up if the parent `<rules>` element also
has  `stopProcessing` flag set to `true`. For example if you have this structure:
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
`<rules name="List 2">` element to stop processing any more rules. By default the `<rules>` element will also
propagate this flag up to its parent, so no further rules will be evaluated in this case.

If you do not want to propogate the `stopProcessing` flag up to the parent, set `stopProcessing="false"` 
on the `<rules>`element like this:
```
    <rules name="List 1">
	  <rule name="Rule 1a" />
	  <rule name="Rule 1b" >
        <rules name="List 2" stopProcessing="false">
		  <rule name="Rule 2a" stopProcessing="true">
		  <rule name="Rule 2b">
		  <rule name="Rule 2c">
		</rules>
	  </rule>
	  <rule name="Rule 1c" />
	  <rule name="Rule 1d" />
	</rules>
```
In this version if `Rule 2a` matches the request this will skip the rest of the rules in `List 2` but this
will not propagate up to `<rules name="List 1">` so rule processing will continue with `<rule name="Rule 1c" />`.

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
		  <rule name="Upper case">
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
string into a dictionary. It does this lazily on demand so that the lists and dictionaries only get created if
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

### More complex and/or condition support and simplified conditions too
The first version of the Microsoft IIS Rewrite Module only had the `<match>` element. In version Version 2 
Microsoft introduced the optional `<conditions><add /></conditions>` syntax but the `<match>` element is still
required. In this Rewrite Module the `<match>` element is optional and I recommend that you only use it to port 
existing rewrite rules. For any new rules the new syntax is more readable and executes much faster.

There is a new `<condition>` element. Elements of this type can be placed directly inside the `<rule>` element or grouped
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
 
### Selectively modifying the request
In the Microsoft Rewrite module when you define a rewrite action you have to pass the entire URL which means
that you have to use Regex back references to pick up the parts of the request you don't want to modify, and
this complicates the original Regex as well as making the whole rule syntex much less readable.

In this Rewrite Module you can specify exactly which part of the url you want to modify, and you can add multiple
`<rewrite>` elements to the `<rule>` element to make multiple modifications. The url in the `<action>` element 
is optional for this Rewrite Module and I strongly recommend that you do not use it. You can still specify the 
url in the `<action>` if you like for backwards compatibility but if you do this then the `<rewrite>` elements 
become ineffective because `<action url="..." />` will replace the entire url.

This Rewrite module also provides a `<delete>` element for removing parts of the request a `<keep>` element to delete
all except certain parts of the request an `<insert>` element and an `<append>` element. These elements allow you to 
perform any modifications to the url that you need. It also has a <normalize> element that allows you to make your
URLs consistent (for example always starting the path with / and never ending the path with /).

An example of a rule that makes changes to the request follows:
```
    <rule name="Flatten forms permenantly">
      <condition scope="OriginalPathElement" index="-1" test="EndsWith" value=".aspx" />
      <condition scope="OriginalPathElement" index="3" test="Equals" value="" negate="true" />
      <rewrite to="Path" from="OriginalPath" operation="LowerCase" />
      <rewrite to="PathElement" toIndex="2" from="PathElement" fromIndex="-1" />
      <keep scope="Path" index="2" />
      <keep scope="Parameter" index="page" />
      <action redirectType="301" />
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
`scope` and `index` attributes will determine what will be passed into your custom condition.

For custom actions pass the name of your action to the `type` attribute of the `<action>` element. Your action will be passed
an interface containing details of the request being processed.

For custom operations pass the name of your operation to the `operation` attribute of the `<rewrite>` element. It will be
passed a string and should return a modified version of that string.

### Example of a custom operation
```
    public class ToMyDomain: IOperation
    {
        public string Execute(string value)
        {
            return "http://mydomain.com/" + value;
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return ToString();
        }

        public override string ToString()
        {
            return "ToMyDomain()";
        }

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            writer.Write(indent);
            writer.WriteLine("adds my domain name to the front of a URL");
        }
    }
```

### Example of a custom condition
```
    public class IsMyDomain : ICondition
    {
        private bool _negate;

        public ICondition Initialize(XElement configuration, IValueGetter valueGetter)
        {
            var negateAttribute = configuration.Attribute("negate");
            if (negateAttribute != null)
                _negate = string.Equals(negateAttribute.Value, "true", StringComparison.InvariantCultureIgnoreCase);

            return this;
        }

        public bool Test(IRequestInfo request, IRuleResult ruleResult)
        {
            var result = string.Equals(request.GetHeader("host"), "mydomain.com", StringComparison.InvariantCultureIgnoreCase);
            return _negate ? !result : result;
        }

        public override string ToString()
        {
            return "host header is" + (_negate ? " not " : " ") + "for my web site";
        }

        public string ToString(IRequestInfo requestInfo)
        {
            return ToString();
        }

        public void Describe(TextWriter writer, string indent, string indentText)
        {
            writer.WriteLine(indent + " If " + ToString());
        }
    }
```

## Optimization techniques
Use this section as a guide to making your redirection rules run as efficiently as possible

### Optimization number 1, no regular expressions
The most important optimization technique is *DO NOT USE REGULAR EXPRESSIONS* not only are they slow
but they are hard to read and repeat the same tests over and over in a heirachical rule set.

Instead of writing this:
```
    <rules>
	  <rule name="rule 1">
	    <condition scope="path" test="matchRegex" value="^/pathA/pathB" />
	  </rule>
	  <rule name="rule 2">
	    <condition scope="path" test="matchRegex" value="^/pathC/pathB" />
	  </rule>
	</rules>
```

Refactor your regular expression into tests for each path element. It might look like more 
lines, but this will execute much faster, and we can factor out common conditions 
(as explained below).

```
    <rules>
	  <rule name="rule 1">
	    <condition scope="pathElement" index="1" test="equals" value="pathA" />
	    <condition scope="pathElement" index="2" test="equals" value="pathB" />
	  </rule>
	  <rule name="rule 2">
	    <condition scope="pathElement" index="1" test="equals" value="pathC" />
	    <condition scope="pathElement" index="2" test="equals" value="pathB" />
	  </rule>
	</rules>
```

Notice that when you write the rules like this you can see that rule 1 and rule 2 
share an identical condition that path element 2 must be `pathB`. We can avoid 
evaluating this condition twice by grouping these rules together into a rule list like this:

```
    <rules>
	  <rule name="rule 3">
	    <condition scope="pathElement" index="2" test="equals" value="pathB" />
		<rules>
	      <rule name="rule 1">
	        <condition scope="pathElement" index="1" test="equals" value="pathA" />
	      </rule>
	      <rule name="rule 2">
	        <condition scope="pathElement" index="1" test="equals" value="pathC" />
	      </rule>
	    </rules>
	  </rule>
    </rules>
```
What happens now is that rule 3 checks if path element 2 is `pathB` and if not skips over
`rule 1` and `rule 2` and we can remove this condition from both of these rules.

### Optimization number 2, modify the URL don't reconstruct it
This Rewrite Module provides a rich set of elements that can make any change to the URL
that you need as efficiently as possible.

With the standard IIS rewrite module to modify the URL you have to capture groups in your
regex then refer to these using curly bracket syntax. A typical example is shown below:
```
    <rule name="InfoID">
      <match url="^newsblast/nb\.asp$" />
      <conditions logicalGrouping="MatchAll" trackAllCaptures="true">
        <add input="{QUERY_STRING}" pattern="infoid=([\d]+)" />
        <add input="{QUERY_STRING}" pattern="email=(.+)" />
      </conditions>
      <action type="Redirect" url="/handlers/legacy.ashx?infoid={C:1}&amp;email={C:2}" appendQueryString="false" />
    </rule>
```
Not only is this rule quite hard to read - it takes more than a few seconds to figure out
exactly what it does, but it is also very slow. You can refector this rule to the following
which does exactly the same thing but is much faster and more readable:
```
    <rule name="InfoID">
      <condition scope="path" test="equals" value="/newsblast/nb.asp" />
      <condition scope="parameter" index="infoid" test="matchRegex" value="[\d]+" />
      <condition scope="parameter" index="email" test="equals" value="" negate="true" />
	  <rewrite scope="path" from="literal" fromIndex="/handlers/legacy.ashx" />
	  <keep scope="parameter" index="infoid,email" />
	  <action type="redirect" />
    </rule>
```
### Optimization number 3, set the stopProcessing flag correctly
Setting this flag correctly will avoid evaluating rules unnecessarily.

By default the `<rule>` element will execute its actions only if all the rule's conditions are 
met. Whether the rule's conditions or met or not, processing continues with the next rule until 
all of the rules in the rule list have been evaluated.

Sometimes you know that when a rule's conditions are met, all the other rules below
it do not need to be evaluated. In this case you should add a `stopProcessing="true"` attribute
to the `<rule>`. This will stop the processing of any further rules only if the conditions
attached to the rule are met.

Note that as a debugging aid you can add a rule with no conditions and the stopProcessing flag
set to true which will always terminate rule processing at that point.

By default if the `<rules>` element finds a matching rule and this rule has the `stopProcessing="true"` 
attribute this will make the rule list skip the rest of the rules in this list, and it will 
pass back a `stopProcessing` flag to its parent rule, so the parent will skip the rest of its rules
etc all the way back up the tree.

To change this behavior set the `stopProcessing="false"` attribute of the `<rules>` element. In short
when the `stopProcessing` attribute of the `<rules>` element is `true` this means "if any stop processing 
rules in this list match the request then we are done processing the whole request". when the `stopProcessing` 
attribute of the `<rules>` element is `false` this means "if any stop processing rules in this 
list match the request then we are done processing rules in this list but continue
evaluating rules at the level above".

### Optimization number 4, look at the execution trace

This Rewrite Module is capable of outputting a complete trace of rewrite requests showing full
details of the rules that were evaluated, the conditions that were tested and the actions
that were executed. The request log includes a timestamp on each event to allow you to identify
any performance bottlenecks.

To enable request tracing on specific URLs you must pass a Lambda expression to the `Initialize()`
method of the Rewrite Module (see section on initialization above).

By default the Rewrite Module will output trace information to the `System.Debug.Trace` output, so you
need to attach a debugger to see this. You can attatch the Visual Studio debugger in which case the
execution trace will be displayed in the "Output" window, or you can attach the DebugView application
from Microsoft.

If you don't want to attach a debugger, you can capture the execution trace by implementing the `ILog`
interface, and passing your implementation to the `Initialize()` method.

This is an example of what the built-in execution trace looks like:
```
    Rewrite:    0.0mS rewriting  /rewritethree.aspx
    Rewrite:    3.6mS   list of 7 rules 'Testing'
    Rewrite:    6.0mS     rule 'Always'
    Rewrite:    7.9mS       Add leading path separator. Remove trailing path separator. 
    Rewrite:    8.5mS     rule 'Always' was executed.   
    Rewrite:    8.6mS     rule 'Icon'
    Rewrite:   14.6mS       request MatchPath MatchWildcard '*.ico' is false
    Rewrite:   14.6mS     rule 'Icon' does not match this request.   
    Rewrite:   14.6mS     rule 'Must be lower case'
    Rewrite:   15.9mS       list of 3 conditions
    Rewrite:   17.7mS         request MatchPath MatchRegex '.*[A-Z].*' (case sensitive) is false
    Rewrite:   17.9mS       list of 3 conditions evaluated to false
    Rewrite:   17.9mS     rule 'Must be lower case' does not match this request.   
    Rewrite:   17.9mS     rule 'Web form'
    Rewrite:   20.7mS       request MatchPath MatchWildcard '*.aspx' is true
    Rewrite:   21.6mS       execute list of 4 actions
    Rewrite:   26.1mS         replace Parameter[ipAddress] with ServerVariable[REMOTE_ADDR]
    Rewrite:   26.3mS         replace Parameter[method] with ServerVariable[REQUEST_METHOD].ToLower()
    Rewrite:   27.2mS         append OriginalQueryString to QueryString
    Rewrite:   27.4mS         Application defined custom action
    Rewrite:   27.6mS       finished executing actions. 
    Rewrite:   27.6mS     rule 'Web form' was executed.  Stop processing. 
    Rewrite:   28.2mS   list of 7 rules 'Testing' 4 rules evaluated.  
    Rewrite:   28.6mS finished  /rewritethree.aspx?ipAddress=::1&method=get
```
On the very first execution you might see the redirection rule processing taking 10's milliseconds. After
the first few requests the times should drop to below 2 milliseconds. If your rules are taking longer than
this to execute you should look through the other optimization tips in this document to resolve the
problem.

# Syntax Reference

## `<rewrite>` element

|   |   |
|---|---|
|Description|Root element of the document|
|Attributes|None|
|Parent|None|
|Children|`<rules>` `<rewriteMaps>`|
|Rules|Can only have one `<rules>` child element. All `<rewriteMaps>` children must come before the `<rules>` child|

## `<rules>` element

|   |   |
|---|---|
|Description|Container for a list of `<rule>` elements|
|`name` attribute|only used in trace output|
|`stopProcessing` attribute|defaults to `true`, set to `false` to stop the propogation of the `stopProcessing` flag from rules within this list to the parent rule|
|Parent|`<rewrite>` or `<rule>`|
|Children|`<rule>` and `<assembly>`|
|Rules|The `<assembly>` children must come before any `<rule>` children that reference the custom extensions in the assemblies. Recommended to put all `<assembly>` children at the top|

## `<rewriteMaps>` element

|   |   |
|---|---|
|Description|Container for a list of `<rewriteMap>` elements|
|Attributes|None|
|Parent|`<rewrite>`|
|Children|`<rewriteMap>`|
|Rules|Children must have unique names|

## `<rewriteMap>` element

|   |   |
|---|---|
|Description|Defines a lookup dictionary that can be used like a function in `{}` expansion|
|`name` attribute|the name used to reference this map in `{}`|
|`defaultValue` attribute|the value to return when there is no matching entry in the dictionary|
|Parent|`<rewriteMaps>`|
|Children|`<add>`|
|Rules|Children must have unique keys|

## `<rewriteMap><add>` element

|   |   |
|---|---|
|Description|Defines an entry in the rewrite map dictionary|
|`key` attribute|the dictionary key|
|`value` attribute|the dictionary value|
|Parent|`<rewriteMap>`|
|Children|none|
|Rules|None|

## `<assembly>` element

|   |   |
|---|---|
|Description|Defines a .Net assembly that contains custom extensions|
|`filename` attribute|the name of the .Net DLL without the .dll file extension|
|Parent|`<rules>`|
|Children|`<class>`|
|Rules|The .Net assembly should be placed in the `bin` folder of your web site|

## `<class>` element

|   |   |
|---|---|
|Description|Defines a .Net class that implements a custom extension|
|`name` attribute|how this extension will be referred to within the rewrite rules|
|`type` attribute|must be `operation`, `action` or `condition`|
|`className` attribute|the fully qualified name of a .Net class. The class must implement `IOperation`, `IAction` or `ICondition`|
|Parent|`<assembly>`|
|Children|None|
|Rules|The combination of `name` and `type` must be unique within the whole rewrite rule file|

## `<rule>` element

|   |   |
|---|---|
|Description|Defines a set of actions to perform only when certain conditions are met|
|`name` attribute|only used in trace output to identify the rule|
|`stopProcessing` attribute|defaults to `false`. Set to `true` to stop processing rules in this list if the conditions for this rule are met. Note that the parent `<rules>` element can also set `stopProcessing="true"` to propogate this up to the next level. Note also that some actions cause processing to stop in which case this flag is redundant|
|`dynamic` attribute|defaults to `false`. Set to `true` to indicate that for the same URL, the conditions on this rule can produce different results at different times, and hence the results of the rule evaluation can not be cached|
|`enabled` attribute|defaults to `true`. Set to `false` to remove this rule from the rewriter|
|Parent|`<rules>`|
|Condition children|`<match>`, `<condition>` and `<conditions>`|
|Action children|`<action>`, `<rewrite>`, `<rules>`, `<delete>`, `<keep>`, `<insert>`, `<append>` and `<normalize>`|
|Rules|All condition children are evaluated before any action children. Actions are only executed if all conditions are met. Actions will be executed in the order they appear and conditions will be tested in the order they appear. If the `<rule>` contains no conditions then its actions are always executed|

## `<conditions>` element

|   |   |
|---|---|
|Description|Defines a set of conditions whose values are combined into a single boolen result|
|`logicalGrouping` attribute|defaults to `matchAll`. Can be set to `matchNone` or `matchAny`|
|`trackAllCaptures` attribute|defaults to `false` which means that the capture groups from each regular expression replace the capture groups from any prior regular expression. Setting this to `true` means that each regular expression will add its capture groups to the list rather than replacing them|
|Parent|`<rule>` or `<conditions>`|
|Children|`<add>`, `<condition>` and `<conditions>`|
|Rules|None|

## `<condition>` element

|   |   |
|---|---|
|Description|Defines a condition that must be met for the actions in the rule to be executed. Note that you can put these inside a `<conditions>` element to define how logic is combined for multiple conditions|
|`scope` attribute|defines which part of the request to test. Can be `originalUrl`, `originalPath`, `originalQueryString`, `originalPathElement`, `originalParameter`, `originalHeader`, `url`, `path`, `matchPath`, `queryString`, `pathElement`, `parameter`, `header`, `originalServerVariable`, `serverVariable`, `literal`, `conditionGroup` or `matchGroup`|
|`index` attribute|expects an integer value when `scope` is `originalPathElement`, `pathElement`, `conditionGroup` or `matchGroup`. Expects a string value when `scope` is `originalParameter`, `originalHeader`, `parameter`, `header`, `originalServerVariable`, `serverVariable` or `literal`. For all other `scope` values the `index` is not applicable|
|`test` attribute|specifies the test to perform on the `scope`. Defaults to `matchRegex`. Can be `startsWith`, `endsWith`, `contains`, `equals`, `matchWildcard`, `matchRegex`, `greater` or `less`. Can also be the `name` of a custom condition defined in an `<assembly>`|
|`value` attribute|the value to test against. Depending on whether this is a number or a string, the values you can specify for `test` are restricted, for example you can't do a `contains` test on a number|
|`negate` attribute|dafaults to `false`. Set to `true` to invert the result. This is especially useful for cases like testing that a paremater is not empty|
|`ignoreCase` attribute|defaults to `true`. Set to `false` to have case-sensitive compare on strings. Not applicable if the `value` attribute contains a number|
|Parent|`<rule>` or `<conditions>`|
|Children|None|
|Rules|None|

## `<conditions><add>` element

|   |   |
|---|---|
|Description|This is for backward compatibnility only. I do not recommend using this in any new rules that you write|
|`input` attribute|specifies how to retrieve the value to test from the request. Uses `{}` syntax which is described elsewhere|
|`matchType` attribute|can be `isFile` or `isDirectory`. This attribute causes the rewrite module to test for the existence of a file or directory in the file system|
|`pattern` attribute|a regular expression used to match the `input` value|
|`negate` attribute|dafaults to `false`. Set to `true` to invert the result. This is especially useful for cases like testing that a paremater is not empty|
|`ignoreCase` attribute|defaults to `true`. Set to `false` to have case-sensitive compare|
|Parent|`<conditions>`|
|Children|None|
|Rules|None|

## `<normalize>` element

|   |   |
|---|---|
|Description|Makes incomming requests all look the same to simplify writing rules. This is often placed inside a `<rule>` with no conditions|
|`pathLeadingSeparator` attribute|defaults to `none`. Can also be set to `add` or `remove`|
|`pathTrailingSeparator` attribute|defaults to `none`. Can also be set to `add` or `remove`|
|Parent|`<rule>`|
|Children|None|
|Rules|If you specify that you want to add a separator and there is one already it will not add another separator. Likewise if you specify to remove the separator and it is not there then no changes will be made|

## `<action>` element

|   |   |
|---|---|
|Description|Most of the functionality of this element is for backwards compatibility only. Do not use the `url` attribute in new rules that you write|
|`url` attribute|specifies the URL to redirect to. Supports the `{}` macro expansion syntax. Adds a `/` to the front of the URL if you pass a relative URL for backwards compatibility.|
|`appendquerystring` attribute|defaults to `true` which copies the whole query string from the original request and appends it to the `url` attribute value. Set it to `false` to disable this behavour|
|`redirectType` attribute|defaults to `307`. Only applicable when `type="redirect"` or the `type` attribute is not specified. Specifies the HTTP response code that will be returned to the browser, Can be `301`, `302`, `303`, `307`, `permanent`, `found`, `seeother` or `temporary`|
|`type` attribute|should only be specified when `redirectType` is not specified. This contains the name of the action type to execute, this can be a custom action defined in a `<class>` element, or one of these built-in action types: `redirect`, `rewrite`, `customresponse`, `abortrequest`, `none`|
|`statusline` attribute|is only applicable when `type="customResponse"`. It defines the first line of the HTTP response to send back to the browser|
|`responseline` attribute|is only applicable when `type="customResponse"`. It defines the second line of the HTTP response to send back to the browser|
|Parent|`<rule>`|
|Children|None|
|Rules|The `appendquerystring` atribute is only applicable when the `url` attribute is provided. I recomend that you do not use the `url` parameter, but instead add editing actions to the rule such at `<rewrite>`, `<append>`... etc to define the changes to make to the URL prior to redirection. Note that all of the built-in action types apart from `none` and `rewrite` stop the processing of further rules, so the `stopProcessing` flag is not necessary on the `<rule>` element. If you write your own custom actions you can decide whether to stop processing or not|

## `<rewrite>` element

|   |   |
|---|---|
|Description|Replaces part of the URL with a new value|
|`to` attribute|specifies the part of the URL to overwrite. Defaults to `path` but can also be `url`, `queryString`, `pathElement`, `parameter`, `header` or `serverVariable`|
|`toIndex` attribute|when `to="pathElement"` this is an integer index into the path with positive values being left to right and negative values being right to left. When `to` is `parameter`, `header` or `serverVariable` then this is the name of the query string parameter, header or server variable to overwrite. For all other values of `to` this attribute is not applicable|
|`from` attribute|specifies where to get the value from that will overwrite part of the URL. Defaults to `path` but can also be `originalUrl`, `originalPath`, `originalQueryString`, `originalPathElement`, `originalParameter`, `originalHeader`, `url`, `queryString`, `pathElement`, `parameter`, `header`, `originalServerVariable`, `serverVariable`, `literal`, `conditionGroup`, or `matchGroup`|
|`fromIndex` attribute|for path elements this is an index into the path. For named parts of the request this is the name of that part (for example the name of the server variable). For other scopes this attribute is not applicable|
|`operation` attribute|is applied to the `from` value before being written to the `to` location. Can be the name of a custom operation defined in a `<class>` element, the name of a `<rewriteMap>` element or one of the built-in operations `toLower`, `toUpper`, 'urlEncode' or `urlDecode`|
|`value` attribute|this is a shorthand way of setting `from="literal"` and the `fromIndex` attribute at the same time|
|Parent|`<rule>`|
|Children|None|
|Rules|When `from="literal"` you can use the `{}` macro expansion syntax in the `fromIndex` attribute. Macro expansion is described in more detail elsewhere in this documentation. This also applies to the `value` attribute|

## `<append>` element

|   |   |
|---|---|
|Description|This element is like the `<rewrite>` element except that it appends to the existing value rather then overwriting it.|
|`to` attribute|Specifies which part of the url should be appended to. Appending to the path assumes that you are adding a new path element, and will add a path separator to the path if necessary. Appending to a path element will add text to the end of the path element without changing the number of elements in the path. Path elements can be referenced from left to right with positive indexes or right to left with negative indexes. Appending to path element 0 is the same as appending to the whole path. Appending to other things like headers, query string parameters and server variables will add text to the end of the header, query string or server variable value.|
|`toIndex` attribute|see `<rewrite>` element documentation|
|`from` attribute|see `<rewrite>` element documentation|
|`fromIndex` attribute|see `<rewrite>` element documentation|
|`operation` attribute|see `<rewrite>` element documentation|
|`value` attribute|see `<rewrite>` element documentation|
|Parent|`<rule>`|
|Children|None|
|Rules|see `<rewrite>` element documentation|

## `<insert>` element

|   |   |
|---|---|
|Description|This element inserts a new path element into the middle of the path. You reference an existing path element the new value will be inserted in that position moving the current value in that position to the right.|
|`to` attribute|Only `pathElement` scope is supported for this action and this is the default, so this attribute can be omitted|
|`toIndex` attribute|The index of the existing path element to replace. Passing 1 will insert the new value into path element 1, pushing all other elements to the right by 1 position. Passing a value of -1 will replace the last element in the path with a new value, pushing the current last path element right by 1. If you reference a non-existent path element no action will be taken, i.e. if you specify a index value of 2 and the path only contains 1 element then the url will not be modified.|
|`from` attribute|see `<rewrite>` element documentation|
|`fromIndex` attribute|see `<rewrite>` element documentation|
|`operation` attribute|see `<rewrite>` element documentation|
|`value` attribute|see `<rewrite>` element documentation|
|Parent|`<rule>`|
|Children|None|
|Rules|see `<rewrite>` element documentation|

## `<delete>` element

|   |   |
|---|---|
|Description|Deletes information from the original request|
|`scope` attribute|specifies which part of the request to delete. Defaults to `pathElement` but can also be `url`, `path`, `queryString`, `header`, `parameter` or `serverVariable`|
|`index` attribute|specifies an index into the path when `scope="pathElement"`. The path index of 0 deletes the entire path, 1 deletes the first element, 2 the second element etc. Negative values of path element index delete from the right hand end of the path. When the `scope` is `header`, `parameter` or `serverVariable` this attribute contains the name of the header, query string parameter or server variable to delete. Not applicable to other scopes|
|Parent|`<rule>`|
|Children|None|
|Rules|None|

## `<keep>` element

|   |   |
|---|---|
|Description|Performs a delete all except type of function. For example if you want to retain only certain parameters in the query string and delete all the others this action can do that|
|`scope` attribute|defines which part of the request will be affected. Defaults to `parameter` but can also be `header` or `pathElement`|
|`index` attribute|a comma separated list of the elements to keep. For `pathElement` these are integer values, for other scopes these are names|
|Parent|`<rule>`|
|Children|None|
|Rules|None|

## `<match>` element

|   |   |
|---|---|
|Description|This element exists only for backwards compatibility. I recomend that you do not use it in any new rules that you write. The element defines a condition that must be met for the rule to be applied|
|`url` attribute|a regular expression or wildcard expression to match against the whole URL including the query string|
|`patternSyntax` attribute|defaults to `ECMAScript` but can also be `Wildcard`|
|`negate` attribute|defaults to `false`. Set to `true` to invert the result|
|`ignoreCase` attribute|defaults to `true`. Set to `false` for case-sensitive matching|
|Parent|`<rule>`|
|Children|None|
|Rules|The URL will have any leading `/` removed before matching. This is for backwards compatibility|

## Scopes

The scope is used to specify what to read or modify in the request. When there are multiple of something in the request
then the scope needs an index as well to specify which one you want to operate on. For example when the scope is `path` 
there is only one path in the request so no indexing is needed, but when the scope is `header` you need to specify which 
header.

Note that not all scopes are applicable in all situations. Please consult the description of the element for a list of applicable scopes.

| Scope | Description |
|---|---|
|`originalUrl`|The full url of the original request regrdless of any rewrite actions that have executed. The way that ASP works means that this will have the `http://domain` part stripped off, and will start with `/` at the beginning of the path. You can only read this scope, the original request can not be modified by design.|
|`originalPath`|Just the path part of the original request excluding the query string. The path starts with the `/` after the domain name and ends with the last character before the `?` if there is one or the rest of the url if there is no `?`. You can only read this scope, the original request can not be modified by design.|
|`originalQueryString`|Just the query string part of the original request. The query string starts with the first `?` and continues to the end of the url. You can only read this scope, the original request can not be modified by design.|
|`originalPathElement`|One element from the path. Use the index associated with the scope to specify which element from the path you want to read from. An index value of 0 refers to the whole path, and is equivalent to `path` scope. A positive integer for the index will refer to elements of the path from left to right, the first element is always 1 regardless if whether the url begins with a `/` or not. A negative integer will refer to elements of the path from right to left, the last element is always -1 regardless whether the path has a trailing `/` or not.|
|`originalParameter`|The value of a parameter from the query string part of the original request. Specify the name of the parameter in the index associated with the scope. The query string part of the url starts with the `?` symbol. Parameters in the query string are separated by `&` symbols. Each parameter is of the form `name=value`. Names and values must be encoded in the url because they can't include characters that have special meaning for urls. The Url Rewrite module will decode these for you so that you can work with the unencoded values in your rules.|
|`originalHeader`|One of the headers from the original request. Pass the name of the header in the index associated with the scope. Headers are passed from the browser to IIS on separate lines below the url and above the body of the request. When using a browser users can not specify the headers directly, they are inserted automatically by the browser, and contain information about the browser. For a list of headers and thier meanings see https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html |
|`originalServerVariable`|These variables do not come from the browser, and are not part of the request. These are values that IIS makes available to your software, and they contain information about the IIS environment. For a complete list see http://www.w3schools.com/asp/coll_servervariables.asp. Specify the name of the server variable in the scope index.|
|`url`|The path and query string that will be passed on to rest of the request processing pipeline. To any handlers that receive this request it will appear as if this was the URL that the user typed into their browser. You can modify the entire url by modifying this scope, or you can modify specific parts of the url using other scopes and these changes will be reflected here. For example if you overwrite a `pathElement` then refer to the `url` the value read back for the url will include the path element modification. Note that you can not set the method and host name by changing this scope, only the path and query string.
|`path`|Just the path part of the `url` scope not including the query string. Any changes you make to `path` will not affect the query string. If you delete the `path` it will be set to `/`|
|`queryString`|Just the query string part of the `url` scope including the leading `?`. Any changes you make to the `queryString` will have no effect on the `path`.
|`pathElement`|One element from the `path` scope. Pass a number for the scope index (see `originalPathElement` above for details). Note that `<insert>` for `pathElement` scope will insert a new element into the `path`, whereas `<append>` will append text to the end of the current path element text.|
|`parameter`|The value of one parameter from the `queryString` scope. Pass a paremater name in the index associated with the scope. You can `<rewrite>`, `<delete>` and `<keep>` query string parameters. If you `<append>` a query string parameter text will be added to the end of the parameter value|
|`header`|The modified version of 'originalHeader' scope. Modifications can be made via `<rewrite>`, `<delete>` and `<keep>` actions. If you `<append>` a header, text will be added to the end of the header value|
|`serverVariable`|Modified version of 'originalServerVariable' scope. Changing these variables only affects the current request.|
|`literal`|Specifying this scope allows you to provide a hard-coded literal value rather than reading a value from the request. The literal value is passed in the scope index. Many actions have a `value` atttribute that is a shorthand way of specifying `literal` scope.|
|`matchGroup`|One of the groups from the last `<match>` element that matched the request. Index 0 is the whole match, index 1 is match group 1 etc. This is equivalent to the `{r:n}` syntax but more readable. To use this feature use regular expressions in your `<match>` and identify match groups with `()`.|
|`conditionGroup`|One of the groups from the last `<condition>` element that matched the request. Only conditions that use regular expression syntax produce match groups. Specify index 0 to match the whole matching string, index 1 is match group 1 etc. This is equivalent to the `{c:n}` syntax but more readable. To use this feature use regulat expressions in your `<condition>` and identify match groups with `()`. By default each matching condition will replace all the match groups. You can change this by setting the `trackAllCaptures` attibute of the parent `<conditions>` element.|
