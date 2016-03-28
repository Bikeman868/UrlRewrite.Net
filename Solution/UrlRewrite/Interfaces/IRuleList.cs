namespace UrlRewrite.Interfaces
{
    public interface IRuleList : IRuleElement
    {
        string Name { get; }
        IRuleListResult Evaluate(IRequestInfo request);
    }
}
