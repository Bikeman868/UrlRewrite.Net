using System.IO;
using UrlRewrite.Actions;
using UrlRewrite.Conditions;
using UrlRewrite.Interfaces;
using UrlRewrite.Rules;

namespace UrlRewrite.Configuration
{
    internal class TestRuleParser: IRuleParser
    {
        public IRuleList Parse(Stream stream)
        {
            var root = new RuleList("Root", null);

            root.Add(
                new Rule(
                    "Exclude browser link",
                    new StringMatch(Scope.Path, MatchPattern.StartsWith, "/__browserLink"),
                    null,
                    true)
                );

            root.Add(
                new Rule(
                    "Rule 1", 
                    new ConditionList(CombinationLogic.AllTrue)
                        .Add(new StringMatch(Scope.Path, MatchPattern.Contains, "1"))
                        .Add(new StringMatch(Scope.Path, MatchPattern.EndsWith, ".aspx")), 
                    new ActionList(true)
                        .Add(new Replace(Scope.Path, "/rewriteOne.aspx"))
                        .Add(new Rewrite()),
                        true
                    ));

            root.Add(
                new Rule(
                    "Rule 2",
                    new StringMatch(Scope.Path, MatchPattern.Contains, "2"),
                    new ActionList(true)
                        .Add(new Replace(Scope.Path, "/rewriteTwo.aspx"))
                        .Add(new TemporaryRedirect()),
                        true
                    ));

            root.Add(
                new Rule(
                    "Rule 3",
                    new StringMatch(Scope.Path, MatchPattern.Contains, "3"),
                    new ActionList(true)
                        .Add(new Replace(Scope.Path, "rewriteThree.aspx"))
                        .Add(new PermenantRedirect()),
                        true
                    ));

            return root;
        }
    }
}
