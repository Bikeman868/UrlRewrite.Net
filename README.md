# UrlRewrite.Net
Replacement for the IIS Url Rewriter that fixes all the limitations in the Microsoft implementation

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
* Register .Net classes as conditions or actions allowing you to include complex business logic in your rewriter rules.
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

1. Add a reference to the UrlRewrite.dll assembly. You can compile the source, or install the NuGet package "UrlRewrite.Net".
2. Add the module to your web.config file.
3. Move your rewriting rules into a separate file (you may have it like this already).
4. Initialize the UrlRewrite module in your application startup code.

These steps are described in more detail below.

## Install the rewriter module.
1. In Visual Studio, go to the Tools Menu and choose "NuGet Package Manager" then "Package Manager Console"
2. In the package manager console type "Install-Package UrlRewrite.Net" and press Enter.

## Add the module to your web.config file.
Merge this into your web.config file
```
   <system.webServer>
     <modules runAllManagedModulesForAllRequests="true">
       <remove name="UrlRoutingModule" />
       <add name="UrlRoutingModule" type="UrlRewrite.RewriteModule, UrlRewrite" />
     </modules>
   </system.webServer>
```

Note that your web site needs to be running in Integrated Pipeline mode and not Classic. This is a setting on the AppPool in IIS.

## Move your rewrite rules to a separate file
In .Net applications config files can store sections of the config file in separate files. If you already have your rewriter
rules in a separate file then you can skip this step. If your existing rules are inline within your web.config file then you
need to do this step. Note that if you are including rewriter rules into your application for the first time, you can simply
create a file in the root of your site called RewriteRules.config and skip the rest of this step.

To move the rewriting rules into a separate file, open the web.config file and find the section called 'rewrite'. You need
to copy eveything from inside this element and paste it into a nee file called RewriteRules.config, then you need to replace
the contents of this section with a reference to the file like this:
```
    <rewrite>
      <rules configSource="RewriteRules.config" />
    </rewrite>
```

## Initialize the rewriter
The rewriter must be initialized by your application before it will rewrite any requests. This is usually done in 
the `Application_Start()` method of `Global.asax.cs`.

Note that `Global.asax` runs \after\ the rewriter module, so for the very first request to your web site the 
rewriter will not rewrite the request. Until you initialize the rewriter it does not know where the rules are, or
how to construct your custom types.

To initialize the rewriter call the static `Initialize()` method like this:

```
    void Application_Start(object sender, EventArgs e)
    {
	   UrlRewrite.RewriteModule.Initialize();
    }
   
```

The `Initialize)()` method has optional parameters you can pass to customize its behaviour, these are described in more detail below.

# Integrating your IoC container

You can optionally integrate Rewrite.Net with an IoC container. You might do this if you want to:
* Extend the rewriter with custom application specific logic and those classes use dependency injection.
* Change the standard behaviour of the rewriter by supplying a different implementation of one of its internal classes.

To integrate your IoC container you need to:
* Implement 'UrlRewrite.Interfaces.IFactory' in a way that uses your container to resolve interfaces into concrete classes.
* Pass an instance of your factory class to the `Initialize()` method of the rewriter.
* Register the intefaces that Rewrite.Net uses with your IoC container so that they are resolvable using the factory.

Rewrite.Net resolves the following interfaces via the factory that you supply:
* UrlRewrite.Interfaces.ILog
* UrlRewrite.Interfaces.IFactory
* UrlRewrite.Interfaces.IValuGetter
* UrlRewrite.Interfaces.IStringMatch
* UrlRewrite.Interfaces.INumberMatch
* UrlRewrite.Interfaces.IRequestInfo

# Initializing the rewrite module
You must call the Rewrite modules static `Initialize()` method once only when your application starts up. You can do this
in the Application_Start method of Global.asax.cs if you are using ASP.Net and similar places exist for MVC, OWIN and other
styles of development.

The Initialize method takes a number of parameters, but they are all optional. These are the parameters:

## ILog log
The rewrite module will log exceptions always, and can optionally log a trace of the rewrite rule execution for
specific URLs. If you dont pass this parameter then the log will be output to Trace. You can see this in the Output
window of Visual Studio whilst debugging.

## List<string> requestUrlsToTrace
This is a list of URLs to output trace information for. This is the incomming URL as received by the server from the
browser or other client application. The URLs in this list are case insensitive.

## List<string> rewrittenUrlsToTrace
This is a list of URLs to output trace information for. This is the rewritten URL that is the result of executing
the rewriter rules. The URLs in this list are case insensitive. Note that adding URLs to this list makes the rewriter
execute the rules for these URLs twice.

## IFactory factory
Allows you to integrate your IoC container for resolving interfaces. This is covered in detail above.

If you do not pass a factory here a default factory will be used that calls the default public constructor. This
works perfectly if all of your custom extensions have a public constructor that takes no parameters.

## Stream ruleStream
Pass a stream that can be used to read the rules. This can be a file stream, memory stream, network stream or whatever.
This allows you to store your rewriter rules anywhere you like, including in a database, from an external service etc.

If you do not pass a rele stream then the Rewrite module will attempt to load them from a file called RewriteRules.config
in the root folder of the web site.

IRuleParser ruleParser
This is a pretty advanced use case. It allows you to replace the rule syntax with your own. The Rewrite Module parses
your rule file and constructs a set of objects that can execute those rules as efficiently as possible. You dont have 
to use this XML syntax to define your rules, you can define a different syntax, including loading your rules from
a structured database.

When the Rewrite Module initializes, it passes the rule stream to the parser which produces an IRuleList. Since you can
pass in both the stream and the parser, you can take complete control over the generation of the rule list.
