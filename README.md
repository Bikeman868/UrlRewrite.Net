# UrlRewrite.Net
Replacement for the IIS Url Rewriter that fixes all the limitations in the Microsoft implementation

## Why use this?
* Backward compatible with your existing IIS Rewriter module rules.
* Can rewrite of redirect any URL including no file extension etc.
* Fixes all the limitations of the standard Microsoft rewriter module - see features below.
* Order of magnitude faster than the standard Microsoft rewriter module.

## Features
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

## Current status
This project is in the design phase at the moment. If you want to get involved and make a contribution please contact the author. If you are 
looking for this kind of functionallity for your website then you will need to wait a few months untill the tireless men and women of the
open source community have worked their magic.

## Roadmap
Estimated completion date for version 1.0 is July 2016.

## Getting started
If you already use the Microsoft URL Rewriter module, follow these steps to replace it with this modue.

1. Add a reference to the UrlRewrite.dll assembly. You can compile the source, or install the NuGet package "UrlRewrite.Net".
2. Add the module to your web.config file.
3. Implement a couple of interfaces in your application.
4. Initialize the UrlRewrite module in your application startup code.

These steps are described in more detail below.

### Install the rewriter module.
1. In Visual Studio, go to the Tools Menu and choose "NuGet Package Manager" then "Package Manager Console"
2. In the package manager console type "Install-Package UrlRewrite.Net" and press Enter.

### Add the module to your web.config file.
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

### Implement the interfaces that the rewriter depends on
You can define some of the behaviour of the UrlRewriter by implementing a couple of interfaces. These are all optional unless
you want to add custom actions or custom conditions with constructor injection. To get started you don't need to implement
any of these, but implementing `ILog` would be a useful debugging aid.

#### UrlRewrite.Interfaces.ILog
The rewriter module will use this to log exceptions and also for request tracing if you enable it.
The `GetRequestLog()` method of `ILog` can return null if you do not want to use request tracing.

#### UrlRewrite.Interfaces.IFactory
If you implement this interface then the rewriter module will use it to construct your custom actions and conditions
so that these classes can use Dependency Injection via their constructor. If you don't implement this interface
then any custom extensions you write must have a default public constructor.

### Initialize the rewriter
The rewriter must be initialized by your application before it will rewrite any requests. This is usually done in 
the `Application_Start()` method of `Global.asax.cs`.

Note that `Global.asax` runs /after/ the rewriter module, so for the very first request to your web site the 
rewriter will not rewrite the request. Until you initialize the rewriter it does not know where the rules are, or
how to construct your custom types.

To initialize the rewriter call the static `Initialize()` method like this:

```
    void Application_Start(object sender, EventArgs e)
    {
	   UrlRewrite.RewriteModule.Initialize();
    }
   
```

The `Initialize)()` method has parameters you can pass to customize its behvior, these are described in more detail below.
