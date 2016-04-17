using System.IO;

namespace UrlRewrite.Interfaces.Rules
{
    public interface IRuleParser
    {
        IRuleList Parse(Stream stream);
    }
}
