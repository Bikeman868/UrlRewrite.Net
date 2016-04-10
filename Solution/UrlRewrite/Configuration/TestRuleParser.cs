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
                    new StringMatch()
                        .Initialize(
                            new ValueGetter().Initialize(Scope.OriginalPathElement, 1, null), 
                            CompareOperation.StartsWith, 
                            "__browserLink"),
                    null,
                    true)
                );

            root.Add(
                new Rule(
                    "Rule 1", 
                    new ConditionList().Initialize(CombinationLogic.MatchAll)
                        .Add(new StringMatch()
                            .Initialize(
                                new ValueGetter().Initialize(Scope.OriginalPathElement, -1, null), 
                                CompareOperation.Contains, 
                                "1"))
                        .Add(new StringMatch()
                            .Initialize(
                                new ValueGetter().Initialize(Scope.OriginalPathElement, -1, null), 
                                CompareOperation.EndsWith, 
                                ".aspx")), 
                    new ActionList(true)
                        .Add(new Replace(Scope.Path, null, new ValueGetter().Initialize(Scope.Literal, "/rewriteOne.aspx", null))),
                        true
                    ));

            root.Add(
                new Rule(
                    "Rule 2",
                    new StringMatch()
                        .Initialize(
                            new ValueGetter().Initialize(Scope.OriginalPathElement, -1, null), 
                            CompareOperation.Contains, 
                            "2"),
                    new ActionList(true)
                        .Add(new Replace(Scope.Path, null, new ValueGetter().Initialize(Scope.Literal, "/rewriteTwo.aspx", null)))
                        .Add(new TemporaryRedirect()),
                        true
                    ));

            root.Add(
                new Rule(
                    "Rule 3",
                    new StringMatch()
                        .Initialize(
                            new ValueGetter().Initialize(Scope.OriginalPathElement, -1, null), 
                            CompareOperation.Contains, 
                            "3"),
                    new ActionList(true)
                        .Add(new Replace(Scope.Path, null, new ValueGetter().Initialize(Scope.Literal, "rewriteThree.aspx", null)))
                        .Add(new PermenantRedirect()),
                        true
                    ));

            return root;
        }
    }
}
