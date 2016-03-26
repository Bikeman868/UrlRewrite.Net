using UrlRewrite.Interfaces;

namespace UrlRewrite.Conditions
{
    internal class PathContainsString: ICondition
    {
        private readonly string _match;

        public PathContainsString(string match)
        {
            _match = match.ToLower();
        }
     
        public bool Test(IRequestInfo request)
        {
            return request.Context.Request.RawUrl.ToLower().Contains(_match);
        }
    }
}
