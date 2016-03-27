using System.Xml.Linq;
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

        public override string ToString()
        {
            return "does the original request path contain '" + _match + "'";
        }

        public void Initialize(XElement configuration)
        {
        }

        public string ToString(IRequestInfo request)
        {
            return ToString();
        }
    }
}
