using System.Xml.Linq;
using UrlRewrite.Actions;
using UrlRewrite.Conditions;
using UrlRewrite.Interfaces;
using UrlRewrite.Rules;

namespace UrlRewrite.Configuration
{
    internal class RuleParser
    {
        public IRule Parse(XElement rootNode)
        {
            var root = new RuleList("Root", null);

            root.Add(
                new Simple(
                    "Rule 1", 
                    new PathContainsString("1"), 
                    new ActionList(true)
                        .Add(new Replace(Scope.Path, "/rewriteOne.aspx"))
                        .Add(new Rewrite())
                    ));

            root.Add(
                new Simple(
                    "Rule 2",
                    new PathContainsString("2"),
                    new ActionList(true)
                        .Add(new Replace(Scope.Path, "/rewriteTwo.aspx"))
                        .Add(new TemporaryRedirect())
                    ));

            root.Add(
                new Simple(
                    "Rule 3",
                    new PathContainsString("3"),
                    new ActionList(true)
                        .Add(new Replace(Scope.Path, "rewriteThree.aspx"))
                        .Add(new PermenantRedirect())
                    ));

            return root;
        }
    }
}
