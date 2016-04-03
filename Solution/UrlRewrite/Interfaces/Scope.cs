namespace UrlRewrite.Interfaces
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

        ServerVariable
    }
}
