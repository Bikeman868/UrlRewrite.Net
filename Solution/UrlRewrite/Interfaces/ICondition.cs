namespace UrlRewrite.Interfaces
{
    public interface ICondition : IRuleElement
    {
        /// <summary>
        /// Tests a request to see if it meets this condition
        /// </summary>
        bool Test(IRequestInfo request);
    }
}
