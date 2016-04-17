namespace UrlRewrite.Interfaces.Conditions
{
    public enum Scope
    {
        OriginalUrl,
        OriginalPath,
        OriginalQueryString,
        OriginalPathElement,
        OriginalParameter,
        OriginalHeader,

        Url,
        Path,
        QueryString,
        PathElement,
        Parameter,
        Header,

        OriginalServerVariable,
        ServerVariable,
        Literal,

        ConditionGroup,
        MatchGroup
    }
}
