namespace UrlRewrite.Interfaces.Conditions
{
    public interface IStringMatch: ICondition
    {
        IStringMatch Initialize(
            IValueGetter valueGetter,
            CompareOperation compareOperation,
            string match,
            bool inverted = false,
            bool ignoreCase = true,
            string matchGroupsName = "C");
    }
}
