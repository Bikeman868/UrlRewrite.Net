namespace UrlRewrite.Interfaces
{
    public interface IOperation : IRuleElement
    {
        string Execute(string value);
    }
}
