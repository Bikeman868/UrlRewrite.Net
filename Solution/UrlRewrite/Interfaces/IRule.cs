namespace UrlRewrite.Interfaces
{
    public interface IRule : IRuleElement
    {
        string Name { get; }
        IRuleResult Evaluate(IRequestInfo request);
    }
}
