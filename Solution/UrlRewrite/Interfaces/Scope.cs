namespace UrlRewrite.Interfaces
{
    public enum Scope
    {
        OriginalUrl,
        OriginalPath,
        OriginalQueryString,
        OriginalPathElement,
        OriginalParameter,
        NewUrl,
        NewPath,
        NewQueryString,
        NewPathElement,
        NewParameter
    }
}
