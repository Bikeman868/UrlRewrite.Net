namespace UrlRewrite.Interfaces.Conditions
{
    public enum CompareOperation
    {
        StartsWith,
        EndsWith,
        Contains,
        Equals,
        MatchWildcard,
        MatchRegex,
        Greater,
        Less
    }
}
