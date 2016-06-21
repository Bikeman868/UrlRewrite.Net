namespace UrlRewrite.Interfaces.Conditions
{
    public interface INumberMatch: ICondition
    {
        INumberMatch Initialize(
            IValueGetter valueGetter,
            CompareOperation compareOperation,
            int match,
            bool inverted = false,
            int defaultValue = 0);
    }
}
