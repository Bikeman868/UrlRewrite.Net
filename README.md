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
* Implemented as an IIS managed module. All source code is in .Net so you can set break points and trace code if you want.
* Provides a mechanism similar to the Microsoft Failed Request Trace module to trace rule execution.
* Unless rules are set to 'Dynamic' the results of rule evaluation will be cached for subsequent requests.

## Current status
This project is in the design phase at the moment. If you want to get involved and make a contribution please contact the author. If you are 
looking for this kind of functionallity for your website then you will need to wait a few months untill the tireless men and women of the
open source community have worked their magic.

## Roadmap
Estimated completion date for version 1.0 is July 2016.
