namespace UrlRewrite.Interfaces
{
    internal enum CompareOperation
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
