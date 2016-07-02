namespace UrlRewrite.Interfaces.Conditions
{
    public interface IStaticFileMatch : ICondition
    {
        IStaticFileMatch Initialize(
            IValueGetter valueGetter,
            bool isDirectory,
            bool inverted = false);
    }
}
